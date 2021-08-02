using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Enums;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

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
                EnablePost = true
            };

            Countly.Instance.Init(configuration);

            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();

            Assert.IsNotNull(Countly.Instance.UserDetails);
            Assert.AreEqual(0, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);

            CountlyUserDetailsModel userDetails = null;

            await Countly.Instance.UserDetails.SetUserDetailsAsync(userDetails);
            Assert.AreEqual(0, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);

            userDetails = new CountlyUserDetailsModel("Full Name", "username", "useremail@email.com", "Organization",
                    "222-222-222",
                    "http://webresizer.com/images2/bird1_after.jpg",
                    "M", "1986",
                    new Dictionary<string, object>{
                        { "Hair", "Black" },
                        { "Height", "5.9" },
                    });
            await Countly.Instance.UserDetails.SetUserDetailsAsync(userDetails);
            Assert.AreEqual(1, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);

            CountlyRequestModel requestModel = Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Dequeue();

            string userDetailData = requestModel.RequestData;
            JObject json = JObject.Parse(userDetailData);
            JObject userDetailJson = JObject.Parse(json["user_details"].ToString());

            Assert.AreEqual("Full Name", userDetailJson["name"].ToString());
            Assert.AreEqual("username", userDetailJson["username"].ToString());
            Assert.AreEqual("useremail@email.com", userDetailJson["email"].ToString());
            Assert.AreEqual("Organization", userDetailJson["organization"].ToString());
            Assert.AreEqual("222-222-222", userDetailJson["phone"].ToString());
            Assert.AreEqual("http://webresizer.com/images2/bird1_after.jpg", userDetailJson["picture"].ToString());
            Assert.AreEqual("M", userDetailJson["gender"].ToString());
            Assert.AreEqual("1986", userDetailJson["byear"].ToString());

            Assert.AreEqual("Black", userDetailJson["custom"]["Hair"].ToString());
            Assert.AreEqual("5.9", userDetailJson["custom"]["Height"].ToString());
        }

        /// <summary>
        /// It check the working of method 'SetUserDetailsAsync'.
        /// </summary>
        [Test]
        public async void TestUserDetailMethod_SetCustomUserDetailsAsync()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                EnablePost = true
            };

            Countly.Instance.Init(configuration);
            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();

            Assert.IsNotNull(Countly.Instance.UserDetails);
            Assert.AreEqual(0, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);

            CountlyUserDetailsModel userDetails = null;

            await Countly.Instance.UserDetails.SetCustomUserDetailsAsync(userDetails);
            Assert.AreEqual(0, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);

            userDetails = new CountlyUserDetailsModel(
                    new Dictionary<string, object>{
                        { "Hair", "Black" },
                        { "Height", "5.9" },
                    });
            await Countly.Instance.UserDetails.SetCustomUserDetailsAsync(userDetails);
            Assert.AreEqual(1, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);

            CountlyRequestModel requestModel = Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Dequeue();

          
            string userDetailData = requestModel.RequestData;
            JObject json = JObject.Parse(userDetailData);
            string userDetail = json["user_details"].ToString();
            JObject custom = JObject.Parse(userDetail);

            Assert.AreEqual("Black", custom["custom"]["Hair"].ToString());
            Assert.AreEqual("5.9", custom["custom"]["Height"].ToString());

        }

        /// <summary>
        /// It check the working of method 'UserCustomDetailsAsync'.
        /// </summary>
        [Test]
        public async void TestUserDetailMethod_UserCustomDetailsAsync()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);
            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();

            Assert.IsNotNull(Countly.Instance.UserDetails);
            Assert.AreEqual(0, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);

            CountlyUserDetailsModel InvalidUserDetails = null;
            await Countly.Instance.UserDetails.UserCustomDetailsAsync(InvalidUserDetails);
            Assert.AreEqual(0, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);

            InvalidUserDetails = new CountlyUserDetailsModel(null);
            await Countly.Instance.UserDetails.UserCustomDetailsAsync(InvalidUserDetails);
            Assert.AreEqual(0, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);

            Dictionary<string, object> customDetail = new Dictionary<string, object>{
                { "Height", "5.8" },
                { "Mole", "Lower Left Cheek" }
            };
            CountlyUserDetailsModel userDetails = new CountlyUserDetailsModel(customDetail);
            await Countly.Instance.UserDetails.UserCustomDetailsAsync(userDetails);
            Assert.AreEqual(1, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);
        }
        /// <summary>
        /// It validates the user's custom properties set via 'SetOnce' and 'Set' methods.
        /// </summary>
        [Test]
        public void TestUserCustomProperty_SetOnceAndSet()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.UserDetails);

            Countly.Instance.UserDetails.SetOnce("Distance", "10KM");
            Assert.IsTrue(Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Distance"));
            Dictionary<string, object> dic = Countly.Instance.UserDetails.CustomDataProperties["Distance"] as Dictionary<string, object>;
            Assert.AreEqual("10KM", dic["$setOnce"]);

            Countly.Instance.UserDetails.Set("Height", "6");
            Assert.IsTrue(Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Height"));
            string height = (string)Countly.Instance.UserDetails.CustomDataProperties["Height"];
            Assert.AreEqual("6", height);
        }

        /// <summary>
        /// It validates the user's custom properties set via 'IncrementBy' and 'Increment' methods.
        /// </summary>
        [Test]
        public void TestUserCustomProperty_IncrementBy()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.UserDetails);

            Countly.Instance.UserDetails.IncrementBy("Distance", 5);
            Assert.IsTrue( Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Distance"));
            Dictionary<string, object> dic = Countly.Instance.UserDetails.CustomDataProperties["Distance"] as Dictionary<string, object>;
            Assert.AreEqual(5, dic["$inc"]);

            Countly.Instance.UserDetails.Increment("Height");
            Assert.IsTrue( Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Height"));
            Dictionary<string, object> dic1 = Countly.Instance.UserDetails.CustomDataProperties["Height"] as Dictionary<string, object>;
            Assert.AreEqual(1, dic1["$inc"]);
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

            Assert.IsNotNull(Countly.Instance.UserDetails);

            Countly.Instance.UserDetails.Pull("Distance", new string[] { "5"});
            Assert.IsTrue( Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Distance"));
            Dictionary<string, object> dic = Countly.Instance.UserDetails.CustomDataProperties["Distance"] as Dictionary<string, object>;
            Assert.AreEqual(new string[] { "5" }, dic["$pull"]);
        }

        /// <summary>
        /// It validates the user's custom properties set via 'PushUnique' and 'Push' methods.
        /// </summary>
        [Test]
        public void TestUserCustomProperty_PushUniqueAndPush()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.UserDetails);

            Countly.Instance.UserDetails.PushUnique("Age", new string[] { "29" });
            Assert.IsTrue( Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Age"));
            Dictionary<string, object> dic = Countly.Instance.UserDetails.CustomDataProperties["Age"] as Dictionary<string, object>;
            Assert.AreEqual(new string[] { "29" }, dic["$addToSet"]);

            Countly.Instance.UserDetails.Push("Height", new string[] { "6" });
            Assert.IsTrue(Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Height"));
            Dictionary<string, object> dic2 = Countly.Instance.UserDetails.CustomDataProperties["Height"] as Dictionary<string, object>;
            Assert.AreEqual(new string[] { "6" }, dic2["$push"]);
        }

        /// <summary>
        /// It validates the user's custom properties set via 'Min' and 'Max' methods.
        /// </summary>
        [Test]
        public void TestUserCustomProperty_MinAndMax()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.UserDetails);

            Countly.Instance.UserDetails.Min("Distance", 10.0);
            Assert.IsTrue( Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Distance"));
            Dictionary<string, object> dic = Countly.Instance.UserDetails.CustomDataProperties["Distance"] as Dictionary<string, object>;
            Assert.AreEqual(10.0, dic["$min"]);

            Countly.Instance.UserDetails.Max("Distance", 100.0);
            Assert.IsTrue(Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Distance"));
            Dictionary<string, object> dic1 = Countly.Instance.UserDetails.CustomDataProperties["Distance"] as Dictionary<string, object>;
            Assert.AreEqual(100.0, dic1["$max"]);
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

            Assert.IsNotNull(Countly.Instance.UserDetails);

            Countly.Instance.UserDetails.Multiply("Distance", 2);
            Assert.IsTrue( Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Distance"));
            Dictionary<string, object> dic = Countly.Instance.UserDetails.CustomDataProperties["Distance"] as Dictionary<string, object>;
            Assert.AreEqual(2, dic["$mul"]);

            Countly.Instance.UserDetails.Push("Age", new string[] { "29" });
            Assert.IsTrue( Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Age"));
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
