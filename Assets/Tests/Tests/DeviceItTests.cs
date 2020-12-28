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
    public class DeviceItTests
    {
        // A Test behaves as an ordinary method
        [SetUp]
        public void InitSDK()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = "https://try.count.ly/",
                AppKey = "YOUR_APP_KEY",
                EnableConsoleLogging = true,
                EnableTestMode = true
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
        public void TestNullDeviceId()
        {
            Assert.AreNotEqual(Countly.Instance.Device.DeviceId, null);
        }

        [TearDown]
        public void End()
        {
            Countly.Instance.ResetDB();
            Object.DestroyImmediate(Countly.Instance);
        }
    }
}
