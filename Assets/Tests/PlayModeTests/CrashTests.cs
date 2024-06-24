using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK;
using System.Web;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;

namespace Assets.Tests.PlayModeTests
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
                    string key = entry.Key;
                    object expectedValue = entry.Value;

                    // Check if key exists in custom
                    Assert.IsTrue(custom.ContainsKey(key), $"Key '{key}' not found in custom");

                    // Get actual value from custom
                    JToken actualValue = custom[key];

                    // Compare expected and actual values
                    if (expectedValue is Array || expectedValue is IList) {
                        // Convert expected value to JArray for comparison
                        JArray expectedArray = JArray.FromObject(expectedValue);
                        JArray actualArray = (JArray)actualValue;

                        for(int i = 0; i < actualArray.Count; i++)
                        {
                            Assert.AreEqual(expectedArray[i].ToString(), actualArray[i].ToString());
                        }
                    } else {
                        // Compare single values as strings
                        Assert.AreEqual(expectedValue.ToString(), actualValue.ToString(), $"Mismatch for key '{key}'");
                    }
                }
            }
        }

        // 'SendCrashReportAsync' method in CrashReportsCountlyService
        // Checks if crash service is working if no 'Crash' consent is given.
        // If consent is not given, no report should be sent.
        [Test]
        public async void CrashConsent()
        {
            CountlyConfiguration configuration = TestUtility.CreateBaseConfigConsent(new Plugins.CountlySDK.Enums.Consents[] { });
            Countly.Instance.Init(configuration);

            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();
            Assert.IsNotNull(Countly.Instance.CrashReports);
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            Dictionary<string, object> seg = new Dictionary<string, object>
            {
                { "Time Spent", "1234455" },
                { "Retry Attempts", "10" }
            };

            await Countly.Instance.CrashReports.SendCrashReportAsync("message", "StackTrace", seg);
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);
        }

        // 'AddBreadcrumbs' method in the CrashReportsCountlyService 
        // We check the working of adding Breadcrumbs in Crash Reports.
        // Breadcrumbs should be added and stored.
        [Test]
        public void CrashBreadCrumbs()
        {
            Countly.Instance.Init(TestUtility.CreateBaseConfig());

            Assert.IsNotNull(Countly.Instance.CrashReports);
            Assert.AreEqual(0, Countly.Instance.CrashReports._crashBreadcrumbs.Count);

            Countly.Instance.CrashReports.AddBreadcrumbs("bread_crumbs");

            Assert.AreEqual(1, Countly.Instance.CrashReports._crashBreadcrumbs.Count);
            Assert.AreEqual("bread_crumbs", Countly.Instance.CrashReports._crashBreadcrumbs.Dequeue());
        }

        // 'SendCrashReportAsync' method in CrashReportsCountlyService.
        // Validate SDK limits on crash parameters
        // The provided values should be limited by the limits
        [Test]
        public async void CrashLimits()
        {
            CountlyConfiguration configuration = TestUtility.CreateBaseConfig()
                .SetMaxValueSize(5)
                .SetMaxKeyLength(5)
                .SetMaxSegmentationValues(2)
                .SetMaxStackTraceLineLength(5)
                .SetMaxStackTraceLinesPerThread(2);

            Countly.Instance.Init(configuration);
            Assert.AreEqual(1, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);
            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();

            Assert.IsNotNull(Countly.Instance.CrashReports);

            Dictionary<string, object> seg = new Dictionary<string, object>
            {
                { "Time", "1234455" },
                { "Retry Attempts", "10" },
                { "Temp", "100" }
            };

            await Countly.Instance.CrashReports.SendCrashReportAsync("message", "StackTrace_1\nStackTrace_2\nStackTrace_3", seg);

            // TODO: Instead use ValidateRQEQSize from TestUtility
            Assert.AreEqual(1, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            // Dequeue the sent request and parse its data for further verification.
            CountlyRequestModel requestModel = Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Dequeue();
            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);

            Dictionary<string, object> segmentation = new Dictionary<string, object>
            {
                { "Time", "12344" },
                { "Retry", "10" },
            };

            AssertCrashRequest(collection, "message", "Stack\nStack", true, segmentation);
        }

        // 'SendCrashReportAsync' deprecated method in CrashReportsCountlyService.
        // We verify if a CrashReport is sent with empty, null or whitespace messages
        // Deprecated function with LogType variable should still have the functionality and valid message should be sent. 
        [Test]
        public async void SendCrashReportAsyncDeprecated()
        {
            Countly.Instance.Init(TestUtility.CreateBaseConfig());
            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();

            Assert.IsNotNull(Countly.Instance.CrashReports);
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            await Countly.Instance.CrashReports.SendCrashReportAsync(null, "stackTrace", LogType.Log, null);
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            await Countly.Instance.CrashReports.SendCrashReportAsync(" ", "stackTrace", LogType.Log, null);
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            await Countly.Instance.CrashReports.SendCrashReportAsync("", "stackTrace", LogType.Log, null);
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            Dictionary<string, object> seg = new Dictionary<string, object>{
                { "ExampleSegmentation", "Segment1"},
            };

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
            Countly.Instance.Init(TestUtility.CreateBaseConfig());
            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();

            Assert.IsNotNull(Countly.Instance.CrashReports);
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            //crashes with bad params are not recorded
            await Countly.Instance.CrashReports.SendCrashReportAsync("", "StackTrace", null);
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            await Countly.Instance.CrashReports.SendCrashReportAsync(null, "StackTrace", null);
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            await Countly.Instance.CrashReports.SendCrashReportAsync(" ", "StackTrace", null);
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);


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
            Countly.Instance.Init(TestUtility.CreateBaseConfig());

            Assert.IsNotNull(Countly.Instance.CrashReports);
            Assert.AreEqual(0, Countly.Instance.CrashReports._crashBreadcrumbs.Count);

            string breadCrumbs = "12345123451234512345123451234512345112345123451234512345123451234512345112345123451234512345123451234512345112345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345";
            Countly.Instance.CrashReports.AddBreadcrumbs(breadCrumbs);

            Assert.AreEqual(1, Countly.Instance.CrashReports._crashBreadcrumbs.Count);
            string qBreadCrumbs = Countly.Instance.CrashReports._crashBreadcrumbs.Dequeue();
            Assert.AreEqual(256, qBreadCrumbs.Length);

            string validBreadcrumb = breadCrumbs.Length > 256 ? breadCrumbs.Substring(0, 256) : breadCrumbs;
            Assert.AreEqual(validBreadcrumb, qBreadCrumbs);
        }

        // 'AddBreadcrumbs' method in the CrashReportsCountlyService
        // It stores only the allowed number of breadcrumbs, dequeuing older ones when the limit is exceeded.
        // Older breadcrumbs should be dequeued, and the stored breadcrumb count remains within the limit.
        [Test]
        public void LimitOfAllowedBreadCrumbs()
        {
            CountlyConfiguration configuration = TestUtility.CreateBaseConfig();
            configuration.TotalBreadcrumbsAllowed = 5;
            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.CrashReports);
            Assert.AreEqual(0, Countly.Instance.CrashReports._crashBreadcrumbs.Count);

            Countly.Instance.CrashReports.AddBreadcrumbs("bread_crumbs_1");
            Countly.Instance.CrashReports.AddBreadcrumbs("bread_crumbs_2");
            Countly.Instance.CrashReports.AddBreadcrumbs("bread_crumbs_3");
            Assert.AreEqual(3, Countly.Instance.CrashReports._crashBreadcrumbs.Count);

            // Add more and recognize that they are already limited
            Countly.Instance.CrashReports.AddBreadcrumbs("bread_crumbs_4");
            Countly.Instance.CrashReports.AddBreadcrumbs("bread_crumbs_5");
            Countly.Instance.CrashReports.AddBreadcrumbs("bread_crumbs_6");
            Assert.AreEqual(5, Countly.Instance.CrashReports._crashBreadcrumbs.Count);

            // Add more breadcrumbs beyond the allowed limit.
            Countly.Instance.CrashReports.AddBreadcrumbs("bread_crumbs_7");
            Countly.Instance.CrashReports.AddBreadcrumbs("bread_crumbs_8");
            Countly.Instance.CrashReports.AddBreadcrumbs("bread_crumbs_9");
            Assert.AreEqual(5, Countly.Instance.CrashReports._crashBreadcrumbs.Count);

            // Verify that the oldest breadcrumbs have been dropped and only the latest ones remain
            Assert.AreEqual("bread_crumbs_5", Countly.Instance.CrashReports._crashBreadcrumbs.Dequeue());
            Assert.AreEqual("bread_crumbs_6", Countly.Instance.CrashReports._crashBreadcrumbs.Dequeue());
            Assert.AreEqual("bread_crumbs_7", Countly.Instance.CrashReports._crashBreadcrumbs.Dequeue());
            Assert.AreEqual("bread_crumbs_8", Countly.Instance.CrashReports._crashBreadcrumbs.Dequeue());
            Assert.AreEqual("bread_crumbs_9", Countly.Instance.CrashReports._crashBreadcrumbs.Dequeue());
        }

        [Test]
        public async void CrashSegmentationDataTypes()
        {
            Countly.Instance.Init(TestUtility.CreateBaseConfig());
            Countly cly = Countly.Instance;

            Dictionary<string, object> seg = new Dictionary<string, object>
            {
                { "Time", 1234455 },
                { "Retry Attempts", 10 },
                { "Temp", 100.0f },
                { "IsSuccess", true },
                { "Message", "Test message" },
                { "Average", 75.5 },
                { "LargeNumber", 12345678901234L },
                { "IntArray", new int[] { 1, 2, 3 } },
                { "BoolArray", new bool[] { true, false, true } },
                { "FloatArray", new float[] { 1.1f, 2.2f, 3.3f } },
                { "DoubleArray", new double[] { 1.1, 2.2, 3.3 } },
                { "StringArray", new string[] { "a", "b", "c" } },
                { "LongArray", new long[] { 10000000000L, 20000000000L, 30000000000L } },
                { "IntList", new List<int> { 1, 2, 3 } },
                { "BoolList", new List<bool> { true, false, true } },
                { "FloatList", new List<float> { 1.1f, 2.2f, 3.3f } },
                { "DoubleList", new List<double> { 1.1, 2.2, 3.3 } },
                { "StringList", new List<string> { "a", "b", "c" } },
                { "LongList", new List<long> { 10000000000L, 20000000000L, 30000000000L } }
            };

            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();
            Assert.IsNotNull(Countly.Instance.CrashReports);
            await Countly.Instance.CrashReports.SendCrashReportAsync("message", "StackTrace_1\nStackTrace_2\nStackTrace_3", seg);
            TestUtility.ValidateRQEQSize(cly, 1, 0);
            CountlyRequestModel requestModel = Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Dequeue();
            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            AssertCrashRequest(collection, "message", "StackTrace_1\nStackTrace_2\nStackTrace_3", true, seg);
        }

        [SetUp]
        [TearDown]
        public void End()
        {
            TestUtility.TestCleanup();
        }
    }
}
