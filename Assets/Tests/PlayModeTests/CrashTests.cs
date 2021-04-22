using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Enums;
using System.Threading.Tasks;
using System.Linq;

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
            Assert.AreNotEqual(null, Countly.Instance.CrashReports);
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
            Assert.AreNotEqual(null, Countly.Instance.CrashReports);
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

            Assert.AreNotEqual(null, Countly.Instance.CrashReports);
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

            Assert.AreNotEqual(null, Countly.Instance.CrashReports);

            Assert.AreEqual(0, Countly.Instance.CrashReports._crashBreadcrumbs.Count);

            string breadCrumbs = "12345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345123451234512345";
            Countly.Instance.CrashReports.AddBreadcrumbs(breadCrumbs);

            Assert.AreEqual(1, Countly.Instance.CrashReports._crashBreadcrumbs.Count);
           Assert.AreEqual(1000, Countly.Instance.CrashReports._crashBreadcrumbs.Dequeue().Length);

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

            Assert.AreNotEqual(null, Countly.Instance.CrashReports);

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

        }

        [TearDown]
        public void End()
        {
            Countly.Instance.ClearStorage();
            Object.DestroyImmediate(Countly.Instance);
        }
    }
}
