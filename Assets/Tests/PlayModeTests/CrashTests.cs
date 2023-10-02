using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK;
using Newtonsoft.Json;
using System.Web;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using System.Linq;
using Assets.Tests.PlayModeTests;

namespace Tests
{
    public class CrashTests
    {
        // Verifies that the collected crash report matches the with the expected 
        private void AssertCrashRequest(NameValueCollection collection, string msg, string stackTrace, bool isNonFatal, IDictionary<string, object> segmentation)
        {
            JObject crashObj = JObject.Parse(collection["crash"]);

            Assert.AreEqual(msg, crashObj.GetValue("_name").ToString());
            Assert.AreEqual(isNonFatal, crashObj.GetValue("_nonfatal").ToObject<bool>());
            Assert.AreEqual(stackTrace, crashObj.GetValue("_error").ToString());

            JObject custom = crashObj["_custom"].ToObject<JObject>();
            if (segmentation != null) {
                Assert.AreEqual(segmentation.Count, custom.Count);
                foreach (KeyValuePair<string, object> entry in segmentation) {
                    Assert.AreEqual(entry.Value, custom.GetValue(entry.Key).ToString());
                }
            }
        }

        // 'SendCrashReportAsync' method in CrashReportsCountlyService
        // Checks if crash service is working if no 'Crash' consent is given.
        // If consent is not given, no report should be sent.
        [Test]
        public async void CrashConsent()
        {
            CountlyConfiguration configuration = TestUtility.createBaseConfig();
            configuration.RequiresConsent = true;
            Countly.Instance.Init(configuration);

            // Clear the request repository to ensure no requests are queued.
            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();

            // Assert that the CrashReports instance is not null.
            Assert.IsNotNull(Countly.Instance.CrashReports);

            // Assert that the initial count of requests in the repository is 0.
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            // Define custom segmentation data for the crash report.
            Dictionary<string, object> seg = new Dictionary<string, object>
            {
                { "Time Spent", "1234455" },
                { "Retry Attempts", "10" }
            };

            // Send a crash report asynchronously.
            await Countly.Instance.CrashReports.SendCrashReportAsync("message", "StackTrace", seg);

            // Assert that no requests have been added to the request repository.
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);
        }

        // 'AddBreadcrumbs' method in the CrashReportsCountlyService 
        // We check the working of adding Breadcrumbs in Crash Reports.
        // Breadcrumbs should be added and stored.
        [Test]
        public void CrashBreadCrumbs()
        {
            // Initialize Countly
            CountlyConfiguration configuration = TestUtility.createBaseConfig();
            Countly.Instance.Init(configuration);

            // Assert that the CrashReports instance is not null.
            Assert.IsNotNull(Countly.Instance.CrashReports);

            // Assert that the initial count of crash breadcrumbs is 0.
            Assert.AreEqual(0, Countly.Instance.CrashReports._crashBreadcrumbs.Count);

            // Add a breadcrumb to the crash reporting system.
            Countly.Instance.CrashReports.AddBreadcrumbs("bread_crumbs");

            // Assert that there is now 1 breadcrumb in the system.
            Assert.AreEqual(1, Countly.Instance.CrashReports._crashBreadcrumbs.Count);

            // Verify that the added breadcrumb can be dequeued and matches the original value.
            Assert.AreEqual("bread_crumbs", Countly.Instance.CrashReports._crashBreadcrumbs.Dequeue());
        }

        // 'SendCrashReportAsync' method in CrashReportsCountlyService.
        // We verify the limitations of crash reporting parameters based on the configured maximum values.
        // CrashReport that is sent should be within the configured maximum values.
        [Test]
        public async void CrashLimits()
        {
            // Initialize Countly with specific configuration values for maximum parameter lengths and counts.
            CountlyConfiguration configuration = TestUtility.createBaseConfig();
            configuration.MaxValueSize = 5;
            configuration.MaxKeyLength = 5;
            configuration.MaxSegmentationValues = 2;
            configuration.MaxStackTraceLineLength = 5;
            configuration.MaxStackTraceLinesPerThread = 2;
            Countly.Instance.Init(configuration);
            Countly.Instance.ClearStorage();

            // Assert that the CrashReports instance is not null.
            Assert.IsNotNull(Countly.Instance.CrashReports);

            // Assert that the initial count of requests in the repository is 0.
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            // Attempt to send empty or null crash reports and verify that they are not added to the repository.
            await Countly.Instance.CrashReports.SendCrashReportAsync("", "StackTrace", null);
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            await Countly.Instance.CrashReports.SendCrashReportAsync(null, "StackTrace", null);
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            await Countly.Instance.CrashReports.SendCrashReportAsync(" ", "StackTrace", null);
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            // Define custom segmentation data for a valid crash report.
            Dictionary<string, object> seg = new Dictionary<string, object>
            {
                { "Time", "1234455" },
                { "Retry Attempts", "10" },
                { "Temp", "100" }
            };

            // Send a valid crash report and verify that it is added to the repository.
            await Countly.Instance.CrashReports.SendCrashReportAsync("message", "StackTrace_1\nStackTrace_2\nStackTrace_3", seg);
            Assert.AreEqual(1, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            // Dequeue the sent request and parse its data for further verification.
            CountlyRequestModel requestModel = Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Dequeue();
            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);

            // Define segmentation data for the expected crash report.
            Dictionary<string, object> segmentation = new Dictionary<string, object>
            {
                { "Time", "12344" },
                { "Retry", "10" },
            };

            // Verify that the sent crash report matches the expected properties.
            AssertCrashRequest(collection, "message", "Stack\nStack", true, segmentation);
        }

        // 'SendCrashReportAsync' deprecated method in CrashReportsCountlyService.
        // We verify if a CrashReport is sent with empty, null or whitespace messages
        // Deprecated function with LogType variable should still have the functionality and valid message should be sent. 
        [Test]
        public async void SendCrashReportAsyncDeprecated()
        {
            // Initialize Countly and clear the repository
            CountlyConfiguration configuration = TestUtility.createBaseConfig();
            Countly.Instance.Init(configuration);
            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();

            // Validate that there are no CrashReport at the repository
            Assert.IsNotNull(Countly.Instance.CrashReports);
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            // Send CrashReport with null message, LogType and null segments
            await Countly.Instance.CrashReports.SendCrashReportAsync(null, "stackTrace", LogType.Log, null);
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            // Send CrashReport with white-space message, LogType and null segments
            await Countly.Instance.CrashReports.SendCrashReportAsync(" ", "stackTrace", LogType.Log, null);
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            // Send CrashReport with empty message, LogType and null segments
            await Countly.Instance.CrashReports.SendCrashReportAsync("", "stackTrace", LogType.Log, null);
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            // Create segmentation to send a CrashReport with segmentation
            Dictionary<string, object> seg = new Dictionary<string, object>{
                { "ExampleSegmentation", "Segment1"},
            };

            // Send CrashReport with message, LogType and segmentation
            await Countly.Instance.CrashReports.SendCrashReportAsync("Crash message", "stackTrace", LogType.Log, seg);
            Assert.AreEqual(1, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            CountlyRequestModel requestModel = Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Dequeue();
            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            AssertCrashRequest(collection, "Crash message", "stackTrace", true, seg);
        }

        // 'SendCrashReportAsync' method in CrashReportsCountlyService.
        // We verify if a CrashReport is sent with empty, null or whitespace messages
        // When provided a valid message, a crash report should be sent.
        [Test]
        public async void SendCrashReportAsync()
        {
            // Initialize Countly and clear the repository
            CountlyConfiguration configuration = TestUtility.createBaseConfig();
            Countly.Instance.Init(configuration);
            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();

            // Validate that there are no CrashReport at the repository
            Assert.IsNotNull(Countly.Instance.CrashReports);
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            // Send CrashReport with empty message and null segments
            await Countly.Instance.CrashReports.SendCrashReportAsync("", "StackTrace", null);
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            // Send CrashReport with null message and null segments
            await Countly.Instance.CrashReports.SendCrashReportAsync(null, "StackTrace", null);
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            // Send CrashReport with white-space message and null segments
            await Countly.Instance.CrashReports.SendCrashReportAsync(" ", "StackTrace", null);
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            // Create segmentation to send a CrashReport with segmentation
            Dictionary<string, object> seg = new Dictionary<string, object>{
                { "Time Spent", "1234455"},
                { "Retry Attempts", "10"}
            };

            // Send CrashReport with valid message and segmentation
            await Countly.Instance.CrashReports.SendCrashReportAsync("message", "StackTrace", seg);
            Assert.AreEqual(1, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            CountlyRequestModel requestModel = Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Dequeue();
            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            AssertCrashRequest(collection, "message", "StackTrace", true, seg);
        }

        // 'AddBreadcrumbs' method in the CrashReportsCountlyService 
        // We add a long breadcrumb string, and method truncates long breadcrumbs to a maximum length of 256 characters
        // It should add Breadcrumbs to system and limit the maximum length
        [Test]
        public void CrashBreadCrumbsLength()
        {
            // Initialize Countly
            CountlyConfiguration configuration = TestUtility.createBaseConfig();
            Countly.Instance.Init(configuration);

            // CrashReports instance is not null and initial count of crash breadcrumbs is 0
            Assert.IsNotNull(Countly.Instance.CrashReports);
            Assert.AreEqual(0, Countly.Instance.CrashReports._crashBreadcrumbs.Count);

            // Define a long breadcrumb string and add the breadcrumb
            string breadCrumbs = "12345123451234512345123451234512345112345123451234512345123451234512345112345123451234512345123451234512345112345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345";
            Countly.Instance.CrashReports.AddBreadcrumbs(breadCrumbs);

            // There is now 1 breadcrumb in the queue
            Assert.AreEqual(1, Countly.Instance.CrashReports._crashBreadcrumbs.Count);

            // Dequeue the breadcrumb from the queue
            string qBreadCrumbs = Countly.Instance.CrashReports._crashBreadcrumbs.Dequeue();

            // The dequeued breadcrumb has a length of 256 characters
            Assert.AreEqual(256, qBreadCrumbs.Length);

            // Create a valid breadcrumb string of maximum length 256 characters
            string validBreadcrumb = breadCrumbs.Length > 256 ? breadCrumbs.Substring(0, 256) : breadCrumbs;

            // The dequeued breadcrumb matches the valid breadcrumb
            Assert.AreEqual(validBreadcrumb, qBreadCrumbs);
        }

        // 'AddBreadcrumbs' method in the CrashReportsCountlyService
        // It stores only the allowed number of breadcrumbs, dequeuing older ones when the limit is exceeded.
        // Older breadcrumbs should be dequeued, and the stored breadcrumb count remains within the limit.
        [Test]
        public void LimitOfAllowedBreadCrumbs()
        {
            // Initialize Countly with a configuration allowing a maximum of 5 breadcrumbs.
            CountlyConfiguration configuration = TestUtility.createBaseConfig();
            configuration.TotalBreadcrumbsAllowed = 5;
            Countly.Instance.Init(configuration);

            // Assert that the CrashReports instance is not null.
            Assert.IsNotNull(Countly.Instance.CrashReports);

            // Assert that the initial count of crash breadcrumbs is 0.
            Assert.AreEqual(0, Countly.Instance.CrashReports._crashBreadcrumbs.Count);

            // Add breadcrumbs to the system.
            Countly.Instance.CrashReports.AddBreadcrumbs("bread_crumbs_1");
            Countly.Instance.CrashReports.AddBreadcrumbs("bread_crumbs_2");
            Countly.Instance.CrashReports.AddBreadcrumbs("bread_crumbs_3");

            // Verify that there are 3 breadcrumbs in the system.
            Assert.AreEqual(3, Countly.Instance.CrashReports._crashBreadcrumbs.Count);

            // Add more breadcrumbs to reach the maximum allowed count.
            Countly.Instance.CrashReports.AddBreadcrumbs("bread_crumbs_4");
            Countly.Instance.CrashReports.AddBreadcrumbs("bread_crumbs_5");
            Countly.Instance.CrashReports.AddBreadcrumbs("bread_crumbs_6");

            // Verify that there are now 5 breadcrumbs in the system.
            Assert.AreEqual(5, Countly.Instance.CrashReports._crashBreadcrumbs.Count);

            // Add more breadcrumbs beyond the allowed limit.
            Countly.Instance.CrashReports.AddBreadcrumbs("bread_crumbs_7");
            Countly.Instance.CrashReports.AddBreadcrumbs("bread_crumbs_8");
            Countly.Instance.CrashReports.AddBreadcrumbs("bread_crumbs_9");

            // Verify that the system still contains 5 breadcrumbs, with older ones dequeued.
            Assert.AreEqual(5, Countly.Instance.CrashReports._crashBreadcrumbs.Count);

            // Verify that the oldest breadcrumbs have been dequeued.
            Assert.AreEqual("bread_crumbs_5", Countly.Instance.CrashReports._crashBreadcrumbs.Dequeue());
            Assert.AreEqual("bread_crumbs_6", Countly.Instance.CrashReports._crashBreadcrumbs.Dequeue());
            Assert.AreEqual("bread_crumbs_7", Countly.Instance.CrashReports._crashBreadcrumbs.Dequeue());
            Assert.AreEqual("bread_crumbs_8", Countly.Instance.CrashReports._crashBreadcrumbs.Dequeue());
            Assert.AreEqual("bread_crumbs_9", Countly.Instance.CrashReports._crashBreadcrumbs.Dequeue());
        }

        // Performs cleanup after each test.
        // Clears Countly storage and destroys the Countly instance to ensure a clean state for subsequent tests.
        [TearDown]
        public void End()
        {
            // Clear Countly storage to remove any stored data.
            Countly.Instance.ClearStorage();

            // Destroy the Countly instance immediately to ensure a clean state.
            Object.DestroyImmediate(Countly.Instance);
        }
    }
}
