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
    public class UserCustomDetailsTests
    {
        private readonly string _serverUrl = "https://xyz.com/";
        private readonly string _appKey = "772c091355076ead703f987fee94490";


        [Test]
        public void TestUserCustomeDetailModel()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);

            Assert.AreNotEqual(Countly.Instance.UserDetails, null);

            Countly.Instance.UserDetails.SetOnce("Distance", "10KM");
            Assert.AreEqual(Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Distance"), true);
            Dictionary<string, object> dic = Countly.Instance.UserDetails.CustomDataProperties["Distance"] as Dictionary<string, object>;
            Assert.AreEqual(dic["$setOnce"], "10KM");
        }

        [TearDown]
        public void End()
        {
            Countly.Instance.ClearStorage();
            Object.DestroyImmediate(Countly.Instance);
        }

    }
}
