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

namespace Tests
{
    public class CrashTests
    {
        private readonly string _serverUrl = "https://xyz.com/";
        private readonly string _appKey = "772c091355076ead703f987fee94490";

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


        /// <summary>
        /// It checks the working of crash service if no 'Crash' consent is given.
        /// </summary>
        [Test]
        public async void TestCrashConsent()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                RequiresConsent = true
            };

            Countly.Instance.Init(configuration);
            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();

            Assert.IsNotNull(Countly.Instance.CrashReports);
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            Dictionary<string, object> seg = new Dictionary<string, object>{
                { "Time Spent", "1234455"},
                { "Retry Attempts", "10"}
            };

            await Countly.Instance.CrashReports.SendCrashReportAsync("message", "StackTrace", LogType.Exception, seg);
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);


        }

        /// <summary>
        /// It validates the functionality of 'AddBreadcrumbs'.
        /// </summary>
        [Test]
        public void TestCrashBreadCrumbs()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);
            Assert.IsNotNull(Countly.Instance.CrashReports);
            Assert.AreEqual(0, Countly.Instance.CrashReports._crashBreadcrumbs.Count);

            Countly.Instance.CrashReports.AddBreadcrumbs("bread_crumbs");

            Assert.AreEqual(1, Countly.Instance.CrashReports._crashBreadcrumbs.Count);
            Assert.AreEqual("bread_crumbs", Countly.Instance.CrashReports._crashBreadcrumbs.Dequeue());

        }

        /// <summary>
        /// It validates the crash limits.
        /// </summary>
        [Test]
        public async void TestCrashLimits()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                MaxValueSize = 5,
                MaxKeyLength = 5,
                MaxSegmentationValues = 2,
                MaxStackTraceLineLength = 5,
                MaxStackTraceLinesPerThread = 2
            };

            Countly.Instance.Init(configuration);
            Countly.Instance.ClearStorage();

            Assert.IsNotNull(Countly.Instance.CrashReports);
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            await Countly.Instance.CrashReports.SendCrashReportAsync("", "StackTrace", LogType.Exception, null);
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            await Countly.Instance.CrashReports.SendCrashReportAsync(null, "StackTrace", LogType.Exception, null);
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            await Countly.Instance.CrashReports.SendCrashReportAsync(" ", "StackTrace", LogType.Exception, null);
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);


            Dictionary<string, object> seg = new Dictionary<string, object>{
                { "Time", "1234455"},
                { "Retry Attempts", "10"},
                { "Temp", "100"}
            };

            await Countly.Instance.CrashReports.SendCrashReportAsync("message", "StackTrace_1\nStackTrace_2\nStackTrace_3", LogType.Exception, seg);
            Assert.AreEqual(1, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            CountlyRequestModel requestModel = Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Dequeue();
            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);

            Dictionary<string, object> segmentation = new Dictionary<string, object>{
                { "Time", "12344"},
                { "Retry", "10"},
            };

            AssertCrashRequest(collection, "message", "Stack\nStack", true, segmentation);

        }

        /// <summary>
        /// It validates the functionality of 'SendCrashReportAsync'.
        /// </summary>
        [Test]
        public async void TestMethod_SendCrashReportAsync()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);
            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();

            Assert.IsNotNull(Countly.Instance.CrashReports);
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            await Countly.Instance.CrashReports.SendCrashReportAsync("", "StackTrace", LogType.Exception, null);
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            await Countly.Instance.CrashReports.SendCrashReportAsync(null, "StackTrace", LogType.Exception, null);
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            await Countly.Instance.CrashReports.SendCrashReportAsync(" ", "StackTrace", LogType.Exception, null);
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);


            Dictionary<string, object> seg = new Dictionary<string, object>{
                { "Time Spent", "1234455"},
                { "Retry Attempts", "10"}
            };

            await Countly.Instance.CrashReports.SendCrashReportAsync("message", "StackTrace", LogType.Exception, seg);
            Assert.AreEqual(1, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            CountlyRequestModel requestModel = Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Dequeue();
            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            AssertCrashRequest(collection, "message", "StackTrace", true, seg);


        }

        /// <summary>
        /// It validates the maximum size of a bread crumb.
        /// </summary>
        [Test]
        public void TestCrashBreadCrumbsLength()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);

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

        /// <summary>
        /// It validates the limit of total allowed bread crumbs.
        /// </summary>
        [Test]
        public void TestLimitOfAllowedBreadCrumbs()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                TotalBreadcrumbsAllowed = 5
            };

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.CrashReports);

            Assert.AreEqual(0, Countly.Instance.CrashReports._crashBreadcrumbs.Count);

            Countly.Instance.CrashReports.AddBreadcrumbs("bread_crumbs_1");
            Countly.Instance.CrashReports.AddBreadcrumbs("bread_crumbs_2");
            Countly.Instance.CrashReports.AddBreadcrumbs("bread_crumbs_3");
            Assert.AreEqual(3, Countly.Instance.CrashReports._crashBreadcrumbs.Count);

            Countly.Instance.CrashReports.AddBreadcrumbs("bread_crumbs_4");
            Countly.Instance.CrashReports.AddBreadcrumbs("bread_crumbs_5");
            Countly.Instance.CrashReports.AddBreadcrumbs("bread_crumbs_6");
            Assert.AreEqual(5, Countly.Instance.CrashReports._crashBreadcrumbs.Count);

            Countly.Instance.CrashReports.AddBreadcrumbs("bread_crumbs_7");
            Countly.Instance.CrashReports.AddBreadcrumbs("bread_crumbs_8");
            Countly.Instance.CrashReports.AddBreadcrumbs("bread_crumbs_9");
            Assert.AreEqual(5, Countly.Instance.CrashReports._crashBreadcrumbs.Count);

            Assert.AreEqual("bread_crumbs_5", Countly.Instance.CrashReports._crashBreadcrumbs.Dequeue());
            Assert.AreEqual("bread_crumbs_6", Countly.Instance.CrashReports._crashBreadcrumbs.Dequeue());
            Assert.AreEqual("bread_crumbs_7", Countly.Instance.CrashReports._crashBreadcrumbs.Dequeue());
            Assert.AreEqual("bread_crumbs_8", Countly.Instance.CrashReports._crashBreadcrumbs.Dequeue());
            Assert.AreEqual("bread_crumbs_9", Countly.Instance.CrashReports._crashBreadcrumbs.Dequeue());
        }

        [TearDown]
        public void End()
        {
            Countly.Instance.ClearStorage();
            Object.DestroyImmediate(Countly.Instance);
        }
    }
}
