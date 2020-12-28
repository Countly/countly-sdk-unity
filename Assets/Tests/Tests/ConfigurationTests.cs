using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Enums;
using System.Threading.Tasks;

namespace Tests
{
    public class ConfigurationTests
    {
        // A Test behaves as an ordinary method
        [SetUp]
        public void InitSDK()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = "https://try.count.ly/",
                AppKey = "YOUR_APP_KEY",
                EnableConsoleLogging = true,
                EnableTestMode = true,
            };

            string countryCode = "us";
            string city = "Houston";
            string latitude = "29.634933";
            string longitude = "-95.220255";
            string ipAddress = "10.2.33.12";

            configuration.SetLocation(countryCode, city, latitude + "," + longitude, ipAddress);
            Countly.Instance.Init(configuration);

        }

        [Test]
        public void TestNullValue()
        {
            Assert.AreNotEqual(Countly.Instance.Configuration, null);
        }

        [Test]
        public void TestDefaultConfigValues()
        {
            Assert.AreEqual(Countly.Instance.Configuration.SessionDuration, 60);
            Assert.AreEqual(Countly.Instance.Configuration.StoredRequestLimit, 1000);
            Assert.AreEqual(Countly.Instance.Configuration.EventQueueThreshold, 100);
            Assert.AreEqual(Countly.Instance.Configuration.TotalBreadcrumbsAllowed, 100);
            Assert.AreEqual(Countly.Instance.Configuration.NotificationMode, TestMode.None);
        }

        [Test]
        public void TestServerURLAndAppKey()
        {
            Assert.AreEqual(Countly.Instance.Configuration.AppKey, "YOUR_APP_KEY");
            Assert.AreEqual(Countly.Instance.Configuration.ServerUrl, "https://try.count.ly");
        }

        [Test]
        public void TestLocationFields()
        {
            Assert.AreEqual(Countly.Instance.Configuration.City, "Houston");
            Assert.AreEqual(Countly.Instance.Configuration.IsLocationDisabled, false);
            Assert.AreEqual(Countly.Instance.Configuration.Location, "29.634933,-95.220255");
        }

        [TearDown]
        public void End()
        {
            Countly.Instance.ResetDB();
            Object.DestroyImmediate(Countly.Instance);
        }
    }
}
