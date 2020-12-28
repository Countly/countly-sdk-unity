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

            Countly.Instance.Init(configuration);

        }

        [Test]
        public void TestNullValue()
        {
            Assert.AreNotEqual(Countly.Instance.UserDetails, null);
        }

        [Test]
        public void TestUserCustomeDetailModel()
        {
            Countly.Instance.UserDetails.SetOnce("Distance", "10KM");
            Assert.AreEqual(Countly.Instance.UserDetails.CustomeDataProperties.ContainsKey("Distance"), true);
            Dictionary<string, object>  dic = Countly.Instance.UserDetails.CustomeDataProperties["Distance"] as Dictionary<string, object>;
            Assert.AreEqual(dic["$setOnce"], "10KM");   
        }

        [TearDown]
        public void End()
        {
            Countly.Instance.ResetDB();
            Object.DestroyImmediate(Countly.Instance);
        }

   

        private void IncreamentValue()
        {
            Countly.Instance.UserDetails.Increment("Weight");
        }
    }
}
