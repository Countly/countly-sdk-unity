using NUnit.Framework;
using UnityEngine;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK;

namespace Tests
{
    public class LocationTests
    {
        private readonly string _serverUrl = "https://xyz.com/";
        private readonly string _appKey = "772c091355076ead703f987fee94490";

        private void AssertLocation(string gpsCoord, string city, string ipAddress, string countryCode, bool IsLocationDisabled) {
            Assert.AreNotEqual(Countly.Instance.Location, null);

            Assert.AreEqual(city, Countly.Instance.Location.City);
            Assert.AreEqual(ipAddress, Countly.Instance.Location.IPAddress);
            Assert.AreEqual(countryCode, Countly.Instance.Location.CountryCode);
            Assert.AreEqual(gpsCoord, Countly.Instance.Location.Location);

            Assert.AreEqual(IsLocationDisabled, Countly.Instance.Location.IsLocationDisabled);
        }

        /// <summary>
        /// It matches the user's location values set during configuration with location service values and, it also validates location default values after the location gets disable.
        /// </summary>
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

            

            CountlyConfiguration cc = Countly.Instance.Configuration;
            AssertLocation(cc.Location, cc.City, cc.IPAddress, cc.CountryCode, cc.IsLocationDisabled);

            Countly.Instance.Location.DisableLocation();
            AssertLocation(null, null, null, null, true);
           
        }

        /// <summary>
        /// It matches the user's location values set during configuration with optional location service values and, it also validates location default values after the location gets disable.
        /// </summary>
        [Test]
        public void TestLocationValuesSetDuringInitOnOptionalLocationService()
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
        }

        /// <summary>
        /// It matches the user's location values set after init.
        /// </summary>
        [Test]
        public void TestLocationValuesSetAfterInit()
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

            Countly.Instance.Init(configuration);

            Countly.Instance.Location.SetLocation(countryCode, city, latitude + "," + longitude, ipAddress);
            AssertLocation(latitude + "," + longitude, city, ipAddress, countryCode, Countly.Instance.Configuration.IsLocationDisabled);
        }

        [TearDown]
        public void End()
        {
            Countly.Instance.ClearStorage();
            Object.DestroyImmediate(Countly.Instance);
        }
    }
}
