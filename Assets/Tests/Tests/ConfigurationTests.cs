using NUnit.Framework;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Enums;
using UnityEngine;
using System.Threading.Tasks;

namespace Tests
{
    public class ConfigurationTests
    {
        private readonly string _serverUrl = "https://xyz.com/";
        private readonly string _appKey = "772c091355076ead703f987fee94490";

        [OneTimeSetUp]
        public void DbNumberSetup()
        {
            Countly.DbNumber = 999;
        }

        [Test]
        public void TestSDKInitParams()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            string city = "Houston";
            string countryCode = "us";
            string latitude = "29.634933";
            string longitude = "-95.220255";
            string ipAddress = "10.2.33.12";

            configuration.SetLocation(countryCode, city, latitude + "," + longitude, ipAddress);
            Countly.Instance.Init(configuration);

            Assert.AreNotEqual(Countly.Instance.Configuration, null);

            Assert.AreEqual(Countly.Instance.Configuration.AppKey, _appKey);
            Assert.AreEqual(Countly.Instance.Configuration.ServerUrl, "https://xyz.com");

            Assert.AreEqual(Countly.Instance.Configuration.City, "Houston");
            Assert.AreEqual(Countly.Instance.Configuration.IsLocationDisabled, false);
            Assert.AreEqual(Countly.Instance.Configuration.Location, "29.634933,-95.220255");
        }

        [Test]
        public void TestDefaultConfigValues()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);

            Assert.AreEqual(Countly.Instance.Configuration.SessionDuration, 60);
            Assert.AreEqual(Countly.Instance.Configuration.StoredRequestLimit, 1000);
            Assert.AreEqual(Countly.Instance.Configuration.EventQueueThreshold, 100);
            Assert.AreEqual(Countly.Instance.Configuration.TotalBreadcrumbsAllowed, 100);
            Assert.AreEqual(Countly.Instance.Configuration.NotificationMode, TestMode.None);
        }

        [TearDown]
        public void End()
        {
            Countly.Instance.ResetDB();
            Object.DestroyImmediate(Countly.Instance);
        }
    }
}
