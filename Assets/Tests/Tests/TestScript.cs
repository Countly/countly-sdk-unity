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
    public class TestScript
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
                NotificationMode = TestMode.None
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
        public void InitializeTest()
        {
            Assert.AreNotEqual(Countly.Instance, null);
            Assert.AreEqual(Countly.Instance.isActiveAndEnabled, true);
            Assert.AreEqual(Countly.Instance.IsSDKInitialized, true);
        }

        [Test]
        public void ConfigurationTest()
        {
            Assert.AreNotEqual(Countly.Instance.Configuration, null);
            Assert.AreEqual(Countly.Instance.Configuration.AppKey, "YOUR_APP_KEY");
            Assert.AreEqual(Countly.Instance.Configuration.ServerUrl, "https://try.count.ly");
            Assert.AreEqual(Countly.Instance.Configuration.City, "Houston");
            Assert.AreEqual(Countly.Instance.Configuration.Location, "29.634933,-95.220255");
            Assert.AreEqual(Countly.Instance.Configuration.IsLocationDisabled, false);
        }

        [Test]
        public void LocationInitialValuesTest()
        {
            Assert.AreNotEqual(Countly.Instance.Location, null);
            Assert.AreEqual(Countly.Instance.Location.Location, Countly.Instance.Configuration.Location);
            Assert.AreEqual(Countly.Instance.Location.City, Countly.Instance.Configuration.City);
            Assert.AreEqual(Countly.Instance.Location.IPAddress, Countly.Instance.Configuration.IPAddress);
            Assert.AreEqual(Countly.Instance.Location.CountryCode, Countly.Instance.Configuration.CountryCode);
            Assert.AreEqual(Countly.Instance.Location.IsLocationDisabled, Countly.Instance.Configuration.IsLocationDisabled);
        }

        [Test]
        public void LocationDisableTest()
        {
            Countly.Instance.Location.DisableLocation();
            Assert.AreEqual(Countly.Instance.Location.Location, null);
            Assert.AreEqual(Countly.Instance.Location.City, null);
            Assert.AreEqual(Countly.Instance.Location.IPAddress, null);
            Assert.AreEqual(Countly.Instance.Location.CountryCode, null);
            Assert.AreEqual(Countly.Instance.Location.IsLocationDisabled, true);
        }

        [Test]
        public void DeviceIdTest()
        {
            Assert.AreNotEqual(Countly.Instance.Device, null);
            Assert.AreNotEqual(Countly.Instance.Device.DeviceId, null);
        }

        [TearDown]
        public void End()
        {
            Countly.Instance.OnDestroy();
            Object.DestroyImmediate(Countly.Instance);
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator TestScriptWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
