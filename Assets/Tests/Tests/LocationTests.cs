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
    public class LocationTests
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
        public void TestNullValue()
        {
            Assert.AreNotEqual(Countly.Instance.Device, null);
        }


        [Test]
        public void TestLocationValuesSetDuringInit()
        {
            Assert.AreNotEqual(Countly.Instance.Location, null);
            Assert.AreEqual(Countly.Instance.Location.City, Countly.Instance.Configuration.City);
            Assert.AreEqual(Countly.Instance.Location.Location, Countly.Instance.Configuration.Location);
            Assert.AreEqual(Countly.Instance.Location.IPAddress, Countly.Instance.Configuration.IPAddress);
            Assert.AreEqual(Countly.Instance.Location.CountryCode, Countly.Instance.Configuration.CountryCode);
            Assert.AreEqual(Countly.Instance.Location.IsLocationDisabled, Countly.Instance.Configuration.IsLocationDisabled);
        }

        [Test]
        public void TestLocationDisable()
        {
            Countly.Instance.Location.DisableLocation();
            Assert.AreEqual(Countly.Instance.Location.Location, null);
            Assert.AreEqual(Countly.Instance.Location.City, null);
            Assert.AreEqual(Countly.Instance.Location.IPAddress, null);
            Assert.AreEqual(Countly.Instance.Location.CountryCode, null);
            Assert.AreEqual(Countly.Instance.Location.IsLocationDisabled, true);
        }

        [TearDown]
        public void End()
        {
            Countly.Instance.ResetDB();
            Object.DestroyImmediate(Countly.Instance);
        }
    }
}
