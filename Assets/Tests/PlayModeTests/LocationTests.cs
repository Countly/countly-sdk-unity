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
        private readonly string _serverUrl = "https://xyz.com/";
        private readonly string _appKey = "772c091355076ead703f987fee94490";

        [Test]
        public void TestLocationValuesSetDuringInit()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            string countryCode = "us";
            string city = "Houston";
            string latitude = "29.634933";
            string longitude = "-95.220255";
            string ipAddress = "10.2.33.12";

            configuration.SetLocation(countryCode, city, latitude + "," + longitude, ipAddress);
            Countly.Instance.Init(configuration);

            Assert.AreNotEqual(Countly.Instance.Location, null);
            Assert.AreEqual(Countly.Instance.Location.City, Countly.Instance.Configuration.City);
            Assert.AreEqual(Countly.Instance.Location.Location, Countly.Instance.Configuration.Location);
            Assert.AreEqual(Countly.Instance.Location.IPAddress, Countly.Instance.Configuration.IPAddress);
            Assert.AreEqual(Countly.Instance.Location.CountryCode, Countly.Instance.Configuration.CountryCode);
            Assert.AreEqual(Countly.Instance.Location.IsLocationDisabled, Countly.Instance.Configuration.IsLocationDisabled);

            Countly.Instance.Location.DisableLocation();
            Assert.AreEqual(Countly.Instance.Location.City, null);
            Assert.AreEqual(Countly.Instance.Location.IPAddress, null);
            Assert.AreEqual(Countly.Instance.Location.CountryCode, null);
            Assert.AreEqual(Countly.Instance.Location.IsLocationDisabled, true);
        }

        [TearDown]
        public void End()
        {
            Countly.Instance.ClearStorage();
            Object.DestroyImmediate(Countly.Instance);
        }
    }
}
