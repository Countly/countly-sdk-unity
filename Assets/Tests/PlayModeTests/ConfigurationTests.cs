using NUnit.Framework;
using UnityEngine;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Enums;
using UnityEditor;

namespace Assets.Tests.PlayModeTests
{
    public class ConfigurationTests
    {
        private readonly string _serverUrl = "https://xyz.com/";
        private readonly string _appKey = "772c091355076ead703f987fee94490";

        [Test]
        public void ConfigConstructor_ConstructorValidation()
        {
            CountlyConfiguration config = new CountlyConfiguration(_appKey, _serverUrl);
            Assert.AreEqual(_appKey, config.GetAppKey());
            Assert.AreEqual(_serverUrl, config.GetServerUrl());
            Countly.Instance.Init(config);
            Assert.AreEqual(Countly.Instance.Configuration.GetAppKey(), _appKey);
            Assert.AreEqual(Countly.Instance.Configuration.GetServerUrl(), "https://xyz.com");
        }

        [Test]
        public void ConfigConstructor_DefaultValues()
        {
            CountlyConfiguration config = new CountlyConfiguration(_appKey, _serverUrl);
            Countly.Instance.Init(config);

            Assert.IsTrue(Countly.Instance.IsSDKInitialized);

            Assert.AreEqual(60, Countly.Instance.Configuration.GetUpdateSessionTimerDelay());
            Assert.AreEqual(1000, Countly.Instance.Configuration.GetMaxRequestQueueSize());
            Assert.AreEqual(100, Countly.Instance.Configuration.GetEventQueueSizeToSend());
            Assert.AreEqual(100, Countly.Instance.Configuration.GetMaxBreadcrumbCount());
            Assert.AreEqual(TestMode.None, Countly.Instance.Configuration.GetNotificationMode());

            Assert.AreEqual(256, Countly.Instance.Configuration.GetMaxValueSize());
            Assert.AreEqual(128, Countly.Instance.Configuration.GetMaxKeyLength());
            Assert.AreEqual(100, Countly.Instance.Configuration.GetMaxSegmentationValues());
            Assert.AreEqual(200, Countly.Instance.Configuration.GetMaxStackTraceLineLength());
            Assert.AreEqual(30, Countly.Instance.Configuration.GetMaxStackTraceLinesPerThread());

            Assert.IsNull(Countly.Instance.Configuration.GetParameterTamperingProtectionSalt());
            Assert.IsNull(Countly.Instance.Configuration.GetDeviceId());
            Assert.IsFalse(Countly.Instance.Configuration.IsForcedHttpPostEnabled());
            Assert.IsFalse(Countly.Instance.Configuration.IsLoggingEnabled());
            Assert.IsTrue(Countly.Instance.Configuration.IsAutomaticCrashReportingEnabled());
            Assert.IsFalse(Countly.Instance.Configuration.IsAutomaticSessionTrackingDisabled);

            Assert.IsNull(Countly.Instance.Configuration.City);
            Assert.IsNull(Countly.Instance.Configuration.Location);
            Assert.IsNull(Countly.Instance.Configuration.IPAddress);
            Assert.IsNull(Countly.Instance.Configuration.CountryCode);
            Assert.IsFalse(Countly.Instance.Configuration.IsLocationDisabled);
            Assert.IsFalse(Countly.Instance.Configuration.IsConsentRequired());
        }

        [Test]
        public void ConfigSetters_ValidValues()
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
                .DisableAutomaticCrashReporting()
                .EnableLogging()
                .SetRequiresConsent(true)
                .EnableForcedHttpPost();

            Assert.AreEqual(_appKey, config.GetAppKey());
            Assert.AreEqual(_serverUrl, config.GetServerUrl());
            Assert.AreEqual("device id", config.GetDeviceId());
            Assert.AreEqual("+90", config.CountryCode);
            Assert.AreEqual("İzmir", config.City);
            Assert.AreEqual("38.4237° N", config.Location);
            Assert.AreEqual("XXX.XXX.XX.XX", config.IPAddress);
            Assert.AreEqual("Salt", config.GetParameterTamperingProtectionSalt());
            Assert.AreEqual(10, config.GetMaxBreadcrumbCount());
            Assert.AreEqual(5, config.GetMaxRequestQueueSize());
            Assert.AreEqual(50, config.GetUpdateSessionTimerDelay());
            Assert.AreEqual(129, config.GetMaxKeyLength());
            Assert.AreEqual(40, config.GetMaxSegmentationValues());
            Assert.AreEqual(44, config.GetMaxStackTraceLineLength());
            Assert.AreEqual(2, config.GetMaxStackTraceLinesPerThread());
            Assert.AreEqual(1222, config.GetEventQueueSizeToSend());
            Assert.AreEqual(TestMode.None, config.GetNotificationMode());
            Assert.IsFalse(config.IsAutomaticCrashReportingEnabled());
            Assert.IsTrue(config.IsLoggingEnabled());
            Assert.IsTrue(config.IsForcedHttpPostEnabled());
            Assert.IsTrue(config.IsConsentRequired());
        }

        [Test]
        public void ConfigSetters_NegativeNullValues()
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
                .SetNotificationMode(TestMode.ProductionToken);

            Assert.AreEqual(_appKey, config.GetAppKey());
            Assert.AreEqual(_serverUrl, config.GetServerUrl());
            Assert.AreEqual(null, config.GetDeviceId());
            Assert.AreEqual(null, config.CountryCode);
            Assert.AreEqual(null, config.City);
            Assert.AreEqual(null, config.Location);
            Assert.AreEqual(null, config.IPAddress);
            Assert.AreEqual(null, config.GetParameterTamperingProtectionSalt());
            Assert.AreEqual(-10, config.GetMaxBreadcrumbCount());
            Assert.AreEqual(-5, config.GetMaxRequestQueueSize());
            Assert.AreEqual(-50, config.GetUpdateSessionTimerDelay());
            Assert.AreEqual(-129, config.GetMaxKeyLength());
            Assert.AreEqual(-40, config.GetMaxSegmentationValues());
            Assert.AreEqual(-44, config.GetMaxStackTraceLineLength());
            Assert.AreEqual(-2, config.GetMaxStackTraceLinesPerThread());
            Assert.AreEqual(-1222, config.GetEventQueueSizeToSend());
            Assert.AreEqual(TestMode.ProductionToken, config.GetNotificationMode());
        }

        [SetUp]
        [TearDown]
        public void End()
        {
            TestUtility.TestCleanup();
        }
    }
}
