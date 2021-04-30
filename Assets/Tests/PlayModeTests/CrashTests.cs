using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK;
using Newtonsoft.Json;
using System.Web;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;

namespace Tests
{
    public class CrashTests
    {
        private readonly string _serverUrl = "https://xyz.com/";
        private readonly string _appKey = "772c091355076ead703f987fee94490";


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
            Countly.Instance.ClearStorage();
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
                { "timeSpent", "1234455"},
                { "rtetryAttempts", "10"}
            };

            await Countly.Instance.CrashReports.SendCrashReportAsync("message", "StackTrace", LogType.Exception, seg);
            Assert.AreEqual(1, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            CountlyRequestModel requestModel = Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Dequeue();
            string myUri = requestModel.RequestUrl; ;
            NameValueCollection value = HttpUtility.ParseQueryString(myUri);
            string crash = HttpUtility.ParseQueryString(myUri).Get("crash");
            JObject json = JObject.Parse(crash);
            Assert.AreEqual("message", json.GetValue("_name").ToString());
            Assert.AreEqual("True", json.GetValue("_nonfatal").ToString());
            Assert.AreEqual("StackTrace", json.GetValue("_error").ToString());

            JObject custom = json["_custom"].ToObject<JObject>();

            Assert.AreEqual("1234455", custom.GetValue("timeSpent").ToString());
            Assert.AreEqual("10", custom.GetValue("rtetryAttempts").ToString());

        }

        /// <summary>
        /// It validates the functionality of 'SendCrashReportInternal'.
        /// </summary>
        [Test]
        public async void TestMethod_SendCrashReportInternal()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);
            Countly.Instance.ClearStorage();

            Assert.IsNotNull(Countly.Instance.CrashReports);

            Dictionary<string, object> seg = new Dictionary<string, object>{
                { "Time Spent", "1234455"},
                { "Retry Attempts", "10"}
            };


            CountlyExceptionDetailModel model = Countly.Instance.CrashReports.ExceptionDetailModel("message", "StackTrace", true, seg);
            await Countly.Instance.CrashReports.SendCrashReportInternal(model);

            Assert.AreEqual(1, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            CountlyRequestModel requestModel = Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Dequeue();

            Dictionary<string, object> requestParams = new Dictionary<string, object>
            {
                {
                    "crash", JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented,
                        new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore})
                }
            };

            string url = 
                Countly.Instance.CrashReports._requestCountlyHelper.BuildGetRequest(requestParams);
            int index = url.IndexOf("crash");
            Assert.AreEqual(url.Substring(index), requestModel.RequestUrl.Substring(index));

            // Test Case for Post request
            CountlyExceptionDetailModel model1 = Countly.Instance.CrashReports.ExceptionDetailModel("A very long message to test post request scenario.",
                "StackTrace StackTrace StackTrace StackTrace StackTraceStackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTraceStackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTraceStackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTraceStackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTraceStackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTraceStackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTraceStackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTraceStackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTraceStackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTraceStackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTraceStackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTraceStackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTrace StackTraceStackTrace", true, seg);
            await Countly.Instance.CrashReports.SendCrashReportInternal(model1);

            Assert.AreEqual(1, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            CountlyRequestModel requestModel1 = Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Dequeue();

            Dictionary<string, object> requestParams1 = new Dictionary<string, object>
            {
                {
                    "crash", JsonConvert.SerializeObject(model1, Newtonsoft.Json.Formatting.Indented,
                        new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore})
                }
            };


            string url1 = Countly.Instance.CrashReports._requestCountlyHelper.BuildPostRequest(requestParams1);


            int index1 = url.IndexOf("crash");
            Assert.AreEqual(url1.Substring(index1), requestModel1.RequestData.Substring(index1));

        }

        /// <summary>
        /// It validates the limit on bread crumbs lenght (limit = 1000).
        /// </summary>
        [Test]
        public void TestCrashBreadCrumbsLenght()
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
            Assert.AreEqual(1000, qBreadCrumbs.Length);

            string validBreadcrumb = breadCrumbs.Length > 1000 ? breadCrumbs.Substring(0, 1000) : breadCrumbs;
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
