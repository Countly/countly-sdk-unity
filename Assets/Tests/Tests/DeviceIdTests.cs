using NUnit.Framework;
using UnityEngine;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK;
using System.Threading.Tasks;

namespace Tests
{
    public class DeviceIdTests
    {
        private readonly string _serverUrl = "https://xyz.com/";
        private readonly string _appKey = "772c091355076ead703f987fee94490";

        [OneTimeSetUp]
        public void DbNumberSetup()
        {
            Countly.DbNumber = 999;
        }

        [Test]
        public void TestNullValues()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);
            Assert.AreNotEqual(Countly.Instance.Device, null);
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
