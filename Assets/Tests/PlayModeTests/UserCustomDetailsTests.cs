﻿using System.Collections;
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

        /// <summary>
        /// It check the working of method 'SetUserDetailsAsync'.
        /// </summary>
        [Test]
        public async void TestUserDetailMethod_SetUserDetailsAsync()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);

            Assert.AreNotEqual(null, Countly.Instance.UserDetails);
            Assert.AreNotEqual(0, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);

            CountlyUserDetailsModel userDetails = null;

            await Countly.Instance.UserDetails.SetUserDetailsAsync(userDetails);
            Assert.AreNotEqual(0, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);

            userDetails = new CountlyUserDetailsModel("Full Name", "username", "useremail@email.com", "Organization",
                    "222-222-222",
                    "http://webresizer.com/images2/bird1_after.jpg",
                    "M", "1986",
                    new Dictionary<string, object>{
                        { "Hair", "Black" },
                        { "Race", "Asian" },
                    });
            await Countly.Instance.UserDetails.SetUserDetailsAsync(userDetails);
            Assert.AreNotEqual(1, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);
        }

        /// <summary>
        /// It check the working of method 'UserCustomDetailsAsync'.
        /// </summary>
        [Test]
        public async void TestUserDetailMethod_UserCustomDetailsAsyncc()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);

            Assert.AreNotEqual(null, Countly.Instance.UserDetails);
            Assert.AreNotEqual(0, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);

            CountlyUserDetailsModel InvalidUserDetails = null;
            await Countly.Instance.UserDetails.UserCustomDetailsAsync(InvalidUserDetails);
            Assert.AreNotEqual(0, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);

            InvalidUserDetails = new CountlyUserDetailsModel(null);
            await Countly.Instance.UserDetails.UserCustomDetailsAsync(InvalidUserDetails);
            Assert.AreNotEqual(0, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);

            Dictionary<string, object> customDetail = new Dictionary<string, object>{
                { "Height", "5.8" },
                { "Mole", "Lower Left Cheek" }
            };
            CountlyUserDetailsModel userDetails = new CountlyUserDetailsModel(customDetail);
            await Countly.Instance.UserDetails.UserCustomDetailsAsync(userDetails);
            Assert.AreNotEqual(1, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);
        }
        /// <summary>
        /// It validates the user's custom property set via 'SetOnce'.
        /// </summary>
        [Test]
        public void TestUserCustomProperty_SetOnce()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);

            Assert.AreNotEqual(null, Countly.Instance.UserDetails);

            Countly.Instance.UserDetails.SetOnce("Distance", "10KM");
            Assert.AreEqual(true, Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Distance"));
            Dictionary<string, object> dic = Countly.Instance.UserDetails.CustomDataProperties["Distance"] as Dictionary<string, object>;
            Assert.AreEqual("10KM", dic["$setOnce"]);
        }

        /// <summary>
        /// It validates the user's custom property set via 'IncrementBy'.
        /// </summary>
        [Test]
        public void TestUserCustomProperty_IncrementBy()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);

            Assert.AreNotEqual(null, Countly.Instance.UserDetails);

            Countly.Instance.UserDetails.IncrementBy("Distance", 5);
            Assert.AreEqual(true, Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Distance"));
            Dictionary<string, object> dic = Countly.Instance.UserDetails.CustomDataProperties["Distance"] as Dictionary<string, object>;
            Assert.AreEqual(5, dic["$inc"]);
        }

        /// <summary>
        /// It validates the user's custom property set via 'Pull'.
        /// </summary>
        [Test]
        public void TestUserCustomProperty_Pull()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);

            Assert.AreNotEqual(null, Countly.Instance.UserDetails);

            Countly.Instance.UserDetails.Pull("Distance", new string[] { "5"});
            Assert.AreEqual(true, Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Distance"));
            Dictionary<string, object> dic = Countly.Instance.UserDetails.CustomDataProperties["Distance"] as Dictionary<string, object>;
            Assert.AreEqual(new string[] { "5" }, dic["$pull"]);
        }

        /// <summary>
        /// It validates the user's custom property set via 'PushUnique'.
        /// </summary>
        [Test]
        public void TestUserCustomProperty_PushUnique()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);

            Assert.AreNotEqual(null, Countly.Instance.UserDetails);

            Countly.Instance.UserDetails.PushUnique("Age", new string[] { "29" });
            Assert.AreEqual(true, Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Age"));
            Dictionary<string, object> dic = Countly.Instance.UserDetails.CustomDataProperties["Age"] as Dictionary<string, object>;
            Assert.AreEqual(new string[] { "29" }, dic["$addToSet"]);
        }

        /// <summary>
        /// It validates the user's custom property set via 'Min'.
        /// </summary>
        [Test]
        public void TestUserCustomProperty_Min()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);

            Assert.AreNotEqual(null, Countly.Instance.UserDetails);

            Countly.Instance.UserDetails.Min("Distance", 10.0);
            Assert.AreEqual(true, Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Distance"));
            Dictionary<string, object> dic = Countly.Instance.UserDetails.CustomDataProperties["Distance"] as Dictionary<string, object>;
            Assert.AreEqual(10.0, dic["$min"]);
        }

        /// <summary>
        /// It validates the user's custom properties befor and after calling SaveAsync'.
        /// </summary>
        [Test]
        public async void TestUserDetailService_SaveAsync()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);

            Assert.AreNotEqual(null, Countly.Instance.UserDetails);

            Countly.Instance.UserDetails.Multiply("Distance", 2);
            Assert.AreEqual(true, Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Distance"));
            Dictionary<string, object> dic = Countly.Instance.UserDetails.CustomDataProperties["Distance"] as Dictionary<string, object>;
            Assert.AreEqual(2, dic["$mul"]);

            Countly.Instance.UserDetails.Push("Age", new string[] { "29" });
            Assert.AreEqual(true, Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Age"));
            Dictionary<string, object> dic2 = Countly.Instance.UserDetails.CustomDataProperties["Age"] as Dictionary<string, object>;
            Assert.AreEqual(new string[] { "29" }, dic2["$push"]);

            await Countly.Instance.UserDetails.SaveAsync();

            Assert.AreEqual(false, Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Age"));
            Assert.AreEqual(false, Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Distance"));
            Assert.AreEqual(0, Countly.Instance.UserDetails.CustomDataProperties.Count);


        }

        [TearDown]
        public void End()
        {
            Countly.Instance.ClearStorage();
            Object.DestroyImmediate(Countly.Instance);
        }

    }
}
