using NUnit.Framework;
using UnityEngine;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Enums;

namespace Tests
{
    public class ConfigurationTests
    {
        private readonly string _serverUrl = "https://xyz.com/";
        private readonly string _appKey = "772c091355076ead703f987fee94490";

        [Test]
        public void TestSDKInitParams()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,

                Salt = "091355076ead",
                DeviceId = "device-xyz",

                EnablePost = true,
                EnableTestMode = true,
                EnableConsoleLogging = true,
                EnableFirstAppLaunchSegment = true,
                EnableAutomaticCrashReporting = false,

                SessionDuration = 10,
                StoredRequestLimit = 100,
                TotalBreadcrumbsAllowed = 200,

                NotificationMode = TestMode.AndroidTestToken
            };

            string city = "Houston";
            string countryCode = "us";
            string latitude = "29.634933";
            string longitude = "-95.220255";
            string ipAddress = "10.2.33.12";

            configuration.SetLocation(countryCode, city, latitude + "," + longitude, ipAddress);
            Countly.Instance.Init(configuration);

            Assert.AreNotEqual(Countly.Instance, null);
            Assert.AreEqual(Countly.Instance.IsSDKInitialized, true);
            Assert.AreEqual(Countly.Instance.isActiveAndEnabled, true);

            Assert.AreNotEqual(Countly.Instance.Events, null);

            Assert.AreNotEqual(Countly.Instance.Device, null);
            Assert.AreNotEqual(Countly.Instance.Device.DeviceId, null);

            Assert.AreNotEqual(Countly.Instance.Configuration, null);

            Assert.AreEqual(Countly.Instance.Configuration.AppKey, _appKey);
            Assert.AreEqual(Countly.Instance.Configuration.ServerUrl, "https://xyz.com");

            Assert.AreEqual(Countly.Instance.Configuration.City, "Houston");
            Assert.AreEqual(Countly.Instance.Configuration.CountryCode, "us");
            Assert.AreEqual(Countly.Instance.Configuration.IsLocationDisabled, false);
            Assert.AreEqual(Countly.Instance.Configuration.Location, "29.634933,-95.220255");

            Assert.AreEqual(Countly.Instance.Configuration.SessionDuration, 10);
            Assert.AreEqual(Countly.Instance.Configuration.StoredRequestLimit, 100);
            Assert.AreEqual(Countly.Instance.Configuration.EventQueueThreshold, 100);
            Assert.AreEqual(Countly.Instance.Configuration.TotalBreadcrumbsAllowed, 200);
            Assert.AreEqual(Countly.Instance.Configuration.NotificationMode, TestMode.AndroidTestToken);

            Assert.AreEqual(Countly.Instance.Configuration.Salt, "091355076ead");
            Assert.AreEqual(Countly.Instance.Configuration.DeviceId, "device-xyz");

            Assert.AreEqual(Countly.Instance.Configuration.EnablePost, true);
            Assert.AreEqual(Countly.Instance.Configuration.EnableTestMode, true);
            Assert.AreEqual(Countly.Instance.Configuration.EnableConsoleLogging, true);
            Assert.AreEqual(Countly.Instance.Configuration.EnableFirstAppLaunchSegment, true);
            Assert.AreEqual(Countly.Instance.Configuration.EnableAutomaticCrashReporting, false);
        }

        [Test]
        public void TestDefaultConfigValues()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl
            };

            Countly.Instance.Init(configuration);

            Assert.AreEqual(Countly.Instance.Configuration.SessionDuration, 60);
            Assert.AreEqual(Countly.Instance.Configuration.StoredRequestLimit, 1000);
            Assert.AreEqual(Countly.Instance.Configuration.EventQueueThreshold, 100);
            Assert.AreEqual(Countly.Instance.Configuration.TotalBreadcrumbsAllowed, 100);
            Assert.AreEqual(Countly.Instance.Configuration.NotificationMode, TestMode.None);

            Assert.AreEqual(Countly.Instance.Configuration.Salt, null);
            Assert.AreEqual(Countly.Instance.Configuration.DeviceId, null);
            Assert.AreEqual(Countly.Instance.Configuration.EnablePost, false);
            Assert.AreEqual(Countly.Instance.Configuration.EnableTestMode, false);
            Assert.AreEqual(Countly.Instance.Configuration.EnableConsoleLogging, false);
            Assert.AreEqual(Countly.Instance.Configuration.EnableFirstAppLaunchSegment, false);
            Assert.AreEqual(Countly.Instance.Configuration.EnableAutomaticCrashReporting, true);

            Assert.AreEqual(Countly.Instance.Configuration.City, null);
            Assert.AreEqual(Countly.Instance.Configuration.Location, null);
            Assert.AreEqual(Countly.Instance.Configuration.IPAddress, null);
            Assert.AreEqual(Countly.Instance.Configuration.CountryCode, null);
            Assert.AreEqual(Countly.Instance.Configuration.IsLocationDisabled, false);
        }

       

        [TearDown]
        public void End()
        {
            Countly.Instance.ClearStorage();
            Object.DestroyImmediate(Countly.Instance);
        }

    }
}
