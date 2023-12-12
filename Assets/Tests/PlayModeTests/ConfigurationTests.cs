using NUnit.Framework;
using UnityEngine;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Enums;
using Assets.Tests.PlayModeTests;
using UnityEditor;

namespace Tests
{
    public class ConfigurationTests
    {
        private readonly string _serverUrl = "https://xyz.com/";
        private readonly string _appKey = "772c091355076ead703f987fee94490";

        [SetUp]
        public void SetUp()
        {
            TestUtility.TestCleanup();
        }

        /// <summary>
        /// It validates configuration values provided during init and URL sanitation.
        /// </summary>
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
                EnableAutomaticCrashReporting = false,

                SessionDuration = 10,
                StoredRequestLimit = 100,
                EventQueueThreshold = 150,
                TotalBreadcrumbsAllowed = 200,

                MaxValueSize = 4,
                MaxKeyLength = 5,
                MaxSegmentationValues = 6,
                MaxStackTraceLineLength = 7,
                MaxStackTraceLinesPerThread = 8,

                NotificationMode = TestMode.AndroidTestToken
            };

            string city = "Houston";
            string countryCode = "us";
            string latitude = "29.634933";
            string longitude = "-95.220255";
            string ipAddress = "10.2.33.12";

            configuration.DisableAutomaticSessionTracking();
            configuration.SetLocation(countryCode, city, latitude + "," + longitude, ipAddress);
            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance);
            Assert.IsTrue(Countly.Instance.IsSDKInitialized);
            Assert.IsTrue(Countly.Instance.isActiveAndEnabled);

            Assert.IsNotNull(Countly.Instance.Device);
            Assert.IsNotNull(Countly.Instance.Device.DeviceId);

            Assert.IsNotNull(Countly.Instance.Configuration);

            Assert.AreEqual(_appKey, Countly.Instance.Configuration.AppKey);
            Assert.AreEqual("https://xyz.com", Countly.Instance.Configuration.ServerUrl);

            Assert.AreEqual("Houston", Countly.Instance.Configuration.City);
            Assert.AreEqual("us", Countly.Instance.Configuration.CountryCode);
            Assert.IsFalse(Countly.Instance.Configuration.IsLocationDisabled);
            Assert.AreEqual("29.634933,-95.220255", Countly.Instance.Configuration.Location);

            Assert.AreEqual(10, Countly.Instance.Configuration.SessionDuration);
            Assert.AreEqual(100, Countly.Instance.Configuration.StoredRequestLimit);
            Assert.AreEqual(150, Countly.Instance.Configuration.EventQueueThreshold);
            Assert.AreEqual(200, Countly.Instance.Configuration.TotalBreadcrumbsAllowed);
            Assert.AreEqual(TestMode.AndroidTestToken, Countly.Instance.Configuration.NotificationMode);

            Assert.AreEqual(4, Countly.Instance.Configuration.MaxValueSize);
            Assert.AreEqual(5, Countly.Instance.Configuration.MaxKeyLength);
            Assert.AreEqual(6, Countly.Instance.Configuration.MaxSegmentationValues);
            Assert.AreEqual(7, Countly.Instance.Configuration.MaxStackTraceLineLength);
            Assert.AreEqual(8, Countly.Instance.Configuration.MaxStackTraceLinesPerThread);

            Assert.AreEqual("091355076ead", Countly.Instance.Configuration.Salt);
            Assert.AreEqual("device-xyz", Countly.Instance.Configuration.DeviceId);

            Assert.IsTrue(Countly.Instance.Configuration.EnablePost);
            Assert.IsTrue(Countly.Instance.Configuration.EnableTestMode);
            Assert.IsTrue(Countly.Instance.Configuration.EnableConsoleLogging);
            Assert.IsFalse(Countly.Instance.Configuration.EnableAutomaticCrashReporting);
            Assert.IsTrue(Countly.Instance.Configuration.IsAutomaticSessionTrackingDisabled);

        }

        /// <summary>
        /// It validates the configuration's default values.
        /// </summary>
        [Test]
        public void TestDefaultConfigValues()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl
            };

            Countly.Instance.Init(configuration);

            Assert.AreEqual(60, Countly.Instance.Configuration.SessionDuration);
            Assert.AreEqual(1000, Countly.Instance.Configuration.StoredRequestLimit);
            Assert.AreEqual(100, Countly.Instance.Configuration.EventQueueThreshold);
            Assert.AreEqual(100, Countly.Instance.Configuration.TotalBreadcrumbsAllowed);
            Assert.AreEqual(TestMode.None, Countly.Instance.Configuration.NotificationMode);

            Assert.AreEqual(256, Countly.Instance.Configuration.MaxValueSize);
            Assert.AreEqual(128, Countly.Instance.Configuration.MaxKeyLength);
            Assert.AreEqual(100, Countly.Instance.Configuration.MaxSegmentationValues);
            Assert.AreEqual(200, Countly.Instance.Configuration.MaxStackTraceLineLength);
            Assert.AreEqual(30, Countly.Instance.Configuration.MaxStackTraceLinesPerThread);

            Assert.IsNull(Countly.Instance.Configuration.Salt);
            Assert.IsNull(Countly.Instance.Configuration.DeviceId);
            Assert.IsFalse(Countly.Instance.Configuration.EnablePost);
            Assert.IsFalse(Countly.Instance.Configuration.EnableTestMode);
            Assert.IsFalse(Countly.Instance.Configuration.EnableConsoleLogging);
            Assert.IsTrue(Countly.Instance.Configuration.EnableAutomaticCrashReporting);
            Assert.IsFalse(Countly.Instance.Configuration.IsAutomaticSessionTrackingDisabled);

            Assert.IsNull(Countly.Instance.Configuration.City);
            Assert.IsNull(Countly.Instance.Configuration.Location);
            Assert.IsNull(Countly.Instance.Configuration.IPAddress);
            Assert.IsNull(Countly.Instance.Configuration.CountryCode);
            Assert.IsFalse(Countly.Instance.Configuration.IsLocationDisabled);
        }

        /// <summary>
        /// It initialize SDK with countly prefab and validates the new configuration against old configuration.
        /// </summary>
        [Test]
        public void TestNewConfigurationAndOldConfiguration()
        {
            Object countlyPrefab = AssetDatabase.LoadAssetAtPath("Assets/Plugins/CountlySDK/Prefabs/Countly.prefab", typeof(GameObject));

            Object.Instantiate(countlyPrefab);

            Assert.IsNotNull(Countly.Instance);
            Assert.IsTrue(Countly.Instance.IsSDKInitialized);
            Assert.IsTrue(Countly.Instance.isActiveAndEnabled);

            Assert.AreEqual(Countly.Instance.Config.SessionDuration, Countly.Instance.Configuration.SessionDuration);
            Assert.AreEqual(Countly.Instance.Config.StoredRequestLimit, Countly.Instance.Configuration.StoredRequestLimit);
            Assert.AreEqual(Countly.Instance.Config.EventQueueThreshold, Countly.Instance.Configuration.EventQueueThreshold);
            Assert.AreEqual(Countly.Instance.Config.TotalBreadcrumbsAllowed, Countly.Instance.Configuration.TotalBreadcrumbsAllowed);
            Assert.AreEqual(Countly.Instance.Config.NotificationMode, Countly.Instance.Configuration.NotificationMode);

            Assert.AreEqual(Countly.Instance.Config.Salt, Countly.Instance.Configuration.Salt);
            Assert.AreEqual(Countly.Instance.Config.EnablePost, Countly.Instance.Configuration.EnablePost);
            Assert.AreEqual(Countly.Instance.Config.EnableConsoleLogging, Countly.Instance.Configuration.EnableConsoleLogging);
            Assert.AreEqual(Countly.Instance.Config.EnableAutomaticCrashReporting, Countly.Instance.Configuration.EnableAutomaticCrashReporting);

            Assert.AreEqual(256, Countly.Instance.Configuration.MaxValueSize);
            Assert.AreEqual(128, Countly.Instance.Configuration.MaxKeyLength);
            Assert.AreEqual(100, Countly.Instance.Configuration.MaxSegmentationValues);
            Assert.AreEqual(200, Countly.Instance.Configuration.MaxStackTraceLineLength);
            Assert.AreEqual(30, Countly.Instance.Configuration.MaxStackTraceLinesPerThread);

            Assert.IsNull(Countly.Instance.Configuration.City);
            Assert.IsNull(Countly.Instance.Configuration.Location);
            Assert.IsNull(Countly.Instance.Configuration.IPAddress);
            Assert.IsNull(Countly.Instance.Configuration.CountryCode);
            Assert.IsFalse(Countly.Instance.Configuration.IsLocationDisabled);
        }

        [Test]
        public void NewConfigConstructor_ConstructorValidation()
        {
            CountlyConfiguration config = new CountlyConfiguration(_appKey, _serverUrl);

            Assert.AreEqual(_appKey, config.AppKey);
            Assert.AreEqual(_serverUrl, config.ServerUrl);

            Countly.Instance.Init(config);

            Assert.AreEqual(Countly.Instance.Configuration.AppKey, _appKey);
            Assert.AreEqual(Countly.Instance.Configuration.ServerUrl, "https://xyz.com");
        }

        [Test]
        public void NewConfigConstructor_DefaultValues()
        {
            CountlyConfiguration config = new CountlyConfiguration(_appKey, _serverUrl);
            Countly.Instance.Init(config);

            Assert.IsTrue(Countly.Instance.IsSDKInitialized);

            Assert.AreEqual(60, Countly.Instance.Configuration.SessionDuration);
            Assert.AreEqual(1000, Countly.Instance.Configuration.StoredRequestLimit);
            Assert.AreEqual(100, Countly.Instance.Configuration.EventQueueThreshold);
            Assert.AreEqual(100, Countly.Instance.Configuration.TotalBreadcrumbsAllowed);
            Assert.AreEqual(TestMode.None, Countly.Instance.Configuration.NotificationMode);

            Assert.AreEqual(256, Countly.Instance.Configuration.MaxValueSize);
            Assert.AreEqual(128, Countly.Instance.Configuration.MaxKeyLength);
            Assert.AreEqual(100, Countly.Instance.Configuration.MaxSegmentationValues);
            Assert.AreEqual(200, Countly.Instance.Configuration.MaxStackTraceLineLength);
            Assert.AreEqual(30, Countly.Instance.Configuration.MaxStackTraceLinesPerThread);

            Assert.IsNull(Countly.Instance.Configuration.Salt);
            Assert.IsNull(Countly.Instance.Configuration.DeviceId);
            Assert.IsFalse(Countly.Instance.Configuration.EnablePost);
            Assert.IsFalse(Countly.Instance.Configuration.EnableTestMode);
            Assert.IsFalse(Countly.Instance.Configuration.EnableConsoleLogging);
            Assert.IsTrue(Countly.Instance.Configuration.EnableAutomaticCrashReporting);
            Assert.IsFalse(Countly.Instance.Configuration.IsAutomaticSessionTrackingDisabled);

            Assert.IsNull(Countly.Instance.Configuration.City);
            Assert.IsNull(Countly.Instance.Configuration.Location);
            Assert.IsNull(Countly.Instance.Configuration.IPAddress);
            Assert.IsNull(Countly.Instance.Configuration.CountryCode);
            Assert.IsFalse(Countly.Instance.Configuration.IsLocationDisabled);
            Assert.IsFalse(Countly.Instance.Configuration.RequiresConsent);
        }

        [Test]
        public void NewConfigSetters_ValidValues()
        {
            CountlyConfiguration config = new CountlyConfiguration(_appKey, _serverUrl)
                .SetDeviceId("device id")
                .SetLocation("+90", "İzmir", "38.4237° N", "XXX.XXX.XX.XX")
                .SetParameterTamperingProtectionSalt("Salt")
                .SetMaxBreadcrumbCount(10)
                .SetMaxRequestQueueSize(5)
                .SetUpdateSessionTimerDelay(50)
                .SetMaxKeyLength(129)
                .SetMaxSegmentationValues(40)
                .SetMaxStackTraceLineLength(44)
                .SetMaxStackTraceLinesPerThread(2)
                .SetEventQueueSizeToSend(1222)
                .SetNotificationMode(TestMode.None)
                .EnableAutomaticCrashReport(true)
                .EnableLogging(true)
                .SetRequiresConsent(true)
                .EnableHttpPostForced(true);

            Assert.AreEqual(_appKey, config.AppKey);
            Assert.AreEqual(_serverUrl, config.ServerUrl);
            Assert.AreEqual("device id", config.DeviceId);
            Assert.AreEqual("+90", config.CountryCode);
            Assert.AreEqual("İzmir", config.City);
            Assert.AreEqual("38.4237° N", config.Location);
            Assert.AreEqual("XXX.XXX.XX.XX", config.IPAddress);
            Assert.AreEqual("Salt", config.Salt);
            Assert.AreEqual(10, config.TotalBreadcrumbsAllowed);
            Assert.AreEqual(5, config.StoredRequestLimit);
            Assert.AreEqual(50, config.SessionDuration);
            Assert.AreEqual(129, config.MaxKeyLength);
            Assert.AreEqual(40, config.MaxSegmentationValues);
            Assert.AreEqual(44, config.MaxStackTraceLineLength);
            Assert.AreEqual(2, config.MaxStackTraceLinesPerThread);
            Assert.AreEqual(1222, config.EventQueueThreshold);
            Assert.AreEqual(TestMode.None, config.NotificationMode);
            Assert.IsTrue(config.EnableAutomaticCrashReporting);
            Assert.IsTrue(config.EnableConsoleLogging);
            Assert.IsTrue(config.EnablePost);
            Assert.IsTrue(config.RequiresConsent);
        }

        [Test]
        public void NewConfigSetters_NegativeNullValues()
        {
            CountlyConfiguration config = new CountlyConfiguration(_appKey, _serverUrl)
                .SetDeviceId(null)
                .SetLocation(null, null, null, null)
                .SetParameterTamperingProtectionSalt(null)
                .SetMaxBreadcrumbCount(-10)
                .SetMaxRequestQueueSize(-5)
                .SetUpdateSessionTimerDelay(-50)
                .SetMaxKeyLength(-129)
                .SetMaxSegmentationValues(-40)
                .SetMaxStackTraceLineLength(-44)
                .SetMaxStackTraceLinesPerThread(-2)
                .SetEventQueueSizeToSend(-1222)
                .SetNotificationMode(TestMode.ProductionToken)
                .EnableAutomaticCrashReport(false)
                .EnableLogging(false)
                .SetRequiresConsent(false)
                .EnableHttpPostForced(false);

            Assert.AreEqual(_appKey, config.AppKey);
            Assert.AreEqual(_serverUrl, config.ServerUrl);
            Assert.AreEqual(null, config.DeviceId);
            Assert.AreEqual(null, config.CountryCode);
            Assert.AreEqual(null, config.City);
            Assert.AreEqual(null, config.Location);
            Assert.AreEqual(null, config.IPAddress);
            Assert.AreEqual(null, config.Salt);
            Assert.AreEqual(-10, config.TotalBreadcrumbsAllowed);
            Assert.AreEqual(-5, config.StoredRequestLimit);
            Assert.AreEqual(-50, config.SessionDuration);
            Assert.AreEqual(-129, config.MaxKeyLength);
            Assert.AreEqual(-40, config.MaxSegmentationValues);
            Assert.AreEqual(-44, config.MaxStackTraceLineLength);
            Assert.AreEqual(-2, config.MaxStackTraceLinesPerThread);
            Assert.AreEqual(-1222, config.EventQueueThreshold);
            Assert.AreEqual(TestMode.ProductionToken, config.NotificationMode);
            Assert.IsFalse(config.EnableAutomaticCrashReporting);
            Assert.IsFalse(config.EnableConsoleLogging);
            Assert.IsFalse(config.EnablePost);
            Assert.IsFalse(config.RequiresConsent);
        }

        [TearDown]
        public void End()
        {
            TestUtility.TestCleanup();
        }

    }
}
