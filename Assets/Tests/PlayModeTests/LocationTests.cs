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

            Assert.AreNotEqual(Countly.Instance.Location, null);
            Assert.AreEqual(city, Countly.Instance.OptionalParameters.City);
            Assert.AreEqual(ipAddress, Countly.Instance.OptionalParameters.IPAddress);
            Assert.AreEqual(countryCode, Countly.Instance.OptionalParameters.CountryCode);
            Assert.AreEqual(latitude + "," + longitude, Countly.Instance.OptionalParameters.Location);

            Countly.Instance.OptionalParameters.DisableLocation();
            Assert.AreEqual(null, Countly.Instance.OptionalParameters.City);
            Assert.AreEqual(null, Countly.Instance.OptionalParameters.IPAddress);
            Assert.AreEqual(null, Countly.Instance.OptionalParameters.CountryCode);
        }


        /// <summary>
        /// It matches the user's location values set after init with optional location service
        /// </summary>
        [Test]
        public void TestOptionalLocationValuesSetAfterInit()
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

            Assert.AreNotEqual(Countly.Instance.Location, null);
            Countly.Instance.OptionalParameters.SetLocation(countryCode, city, latitude + "," + longitude, ipAddress);

            Assert.AreEqual(city, Countly.Instance.OptionalParameters.City);
            Assert.AreEqual(ipAddress, Countly.Instance.OptionalParameters.IPAddress);
            Assert.AreEqual(countryCode, Countly.Instance.OptionalParameters.CountryCode);
            Assert.AreEqual(latitude + "," + longitude, Countly.Instance.OptionalParameters.Location);

            Countly.Instance.OptionalParameters.SetCity("Lahore");
            Countly.Instance.OptionalParameters.SetIPAddress("192.168.100.51");
            Countly.Instance.OptionalParameters.SetLocation(10.00, -10.00);
            Countly.Instance.OptionalParameters.SetCountryCode("PK");

            Assert.AreEqual("Lahore", Countly.Instance.OptionalParameters.City);
            Assert.AreEqual("PK", Countly.Instance.OptionalParameters.CountryCode);
            Assert.AreEqual("192.168.100.51", Countly.Instance.OptionalParameters.IPAddress);
            Assert.AreEqual(10.00 + "," + -10.00, Countly.Instance.OptionalParameters.Location);
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

            Assert.AreNotEqual(Countly.Instance.Location, null);
            Countly.Instance.Location.SetLocation(countryCode, city, latitude + "," + longitude, ipAddress);

            Assert.AreEqual(city, Countly.Instance.Location.City);
            Assert.AreEqual(ipAddress, Countly.Instance.Location.IPAddress);
            Assert.AreEqual(countryCode, Countly.Instance.Location.CountryCode);
            Assert.AreEqual(latitude + "," + longitude, Countly.Instance.Location.Location);
            Assert.AreEqual(Countly.Instance.Location.IsLocationDisabled, Countly.Instance.Configuration.IsLocationDisabled);
        }

        [TearDown]
        public void End()
        {
            Countly.Instance.ClearStorage();
            Object.DestroyImmediate(Countly.Instance);
        }
    }
}
