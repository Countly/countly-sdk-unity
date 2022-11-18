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
using System.Collections.Specialized;
using System.Web;
using System.Linq;

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

            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            JObject userDetailJson = JObject.Parse(collection["user_details"]);

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
        /// It validate user profile fields limits.
        /// </summary>
        [Test]
        public async void TestUserProfileFieldsLimits()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                MaxValueSize = 3,
                EnablePost = true
            };

            Countly.Instance.Init(configuration);
            Countly.Instance.ClearStorage();

            Assert.IsNotNull(Countly.Instance.UserDetails);
            Assert.AreEqual(0, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);

            CountlyUserDetailsModel userDetails = null;

            await Countly.Instance.UserDetails.SetUserDetailsAsync(userDetails);
            Assert.AreEqual(0, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);

            userDetails = new CountlyUserDetailsModel("Full Name", "username", "useremail@email.com", "Organization",
                    "222-222-222",
                    "12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
                    "12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
                    "12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
                    "12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
                    "12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
                    "12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890.png",
                    "M", "1986",
                    new Dictionary<string, object>{
                        { "Hair", "Black" },
                        { "Height", "5.9" },
                    });
            await Countly.Instance.UserDetails.SetUserDetailsAsync(userDetails);
            Assert.AreEqual(1, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);

            CountlyRequestModel requestModel = Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Dequeue();


            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            JObject userDetailJson = JObject.Parse(collection["user_details"]);

            Assert.AreEqual("Ful", userDetailJson["name"].ToString());
            Assert.AreEqual("use", userDetailJson["username"].ToString());
            Assert.AreEqual("use", userDetailJson["email"].ToString());
            Assert.AreEqual("Org", userDetailJson["organization"].ToString());
            Assert.AreEqual("222", userDetailJson["phone"].ToString());
            Assert.AreEqual(4096, userDetailJson["picture"].ToString().Length);
            Assert.AreEqual("M", userDetailJson["gender"].ToString());
            Assert.AreEqual("198", userDetailJson["byear"].ToString());

            Assert.AreEqual("Bla", userDetailJson["custom"]["Hair"].ToString());
            Assert.AreEqual("5.9", userDetailJson["custom"]["Height"].ToString());
        }

        /// <summary>
        /// It check the working of method 'SetUserDetailsAsync'.
        /// </summary>
        [Test]
        public void TestUserDetailMethod_SetCustomUserDetailsAsync()
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

            Dictionary<string, object> userCustomDetail = null;

            Countly.Instance.UserDetails.SetCustomUserDetails(userCustomDetail);
            Assert.AreEqual(0, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);

            userCustomDetail = new Dictionary<string, object> {
                        { "Hair", "Black" },
                        { "Height", "5.9" },
            };
            Countly.Instance.UserDetails.SetCustomUserDetails(userCustomDetail);
            Assert.AreEqual(1, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);

            CountlyRequestModel requestModel = Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Dequeue();
            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            JObject custom = JObject.Parse(collection["user_details"]);


            Assert.AreEqual("Black", custom["custom"]["Hair"].ToString());
            Assert.AreEqual("5.9", custom["custom"]["Height"].ToString());
        }


        /// <summary>
        /// It validate user detail segments limits.
        /// </summary>
        [Test]
        public void TestUserDetailSegmentLimits()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                MaxKeyLength = 5,
                MaxValueSize = 6,
                EnablePost = true
            };

            Countly.Instance.Init(configuration);
            Countly.Instance.ClearStorage();

            Assert.IsNotNull(Countly.Instance.UserDetails);
            Assert.AreEqual(0, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);

            Dictionary<string, object> userCustomDetail = null;
            Countly.Instance.UserDetails.SetCustomUserDetails(userCustomDetail);
            Assert.AreEqual(0, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);

            userCustomDetail = new Dictionary<string, object> {
                        { "Hair", "Black_1" },
                        { "Height", "5.9" },
            };

            Countly.Instance.UserDetails.SetCustomUserDetails(userCustomDetail);
            Assert.AreEqual(1, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);

            CountlyRequestModel requestModel = Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Dequeue();
            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            JObject custom = JObject.Parse(collection["user_details"]);

            Assert.AreEqual("Black_", custom["custom"]["Hair"].ToString());
            Assert.AreEqual("5.9", custom["custom"]["Heigh"].ToString());
        }

        /// <summary>
        /// It validate custom user detail segments key and value limits.
        /// </summary>
        [Test]
        public void TestCustomUserDetailSegmentLimits()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                MaxKeyLength = 5,
                MaxValueSize = 4,
                EnablePost = true
            };

            Countly.Instance.Init(configuration);
            Countly.Instance.ClearStorage();

            Assert.IsNotNull(Countly.Instance.UserDetails);
            Assert.AreEqual(0, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);


            Countly.Instance.UserDetails.IncrementBy("IncrementBy", 5);
            Assert.IsTrue(Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Incre"));
            Dictionary<string, object> dic = Countly.Instance.UserDetails.CustomDataProperties["Incre"] as Dictionary<string, object>;
            Assert.AreEqual(5, dic["$inc"]);

            Countly.Instance.UserDetails.Increment("Increment");
            Assert.IsTrue(Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Incre"));
            dic = Countly.Instance.UserDetails.CustomDataProperties["Incre"] as Dictionary<string, object>;
            Assert.AreEqual(1, dic["$inc"]);

            Countly.Instance.UserDetails.SetOnce("SetOnce", "100KM");
            Assert.IsTrue(Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("SetOn"));
            dic = Countly.Instance.UserDetails.CustomDataProperties["SetOn"] as Dictionary<string, object>;
            Assert.AreEqual("100K", dic["$setOnce"]);

            Countly.Instance.UserDetails.Set("Set", "6000.0");
            Assert.IsTrue(Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Set"));
            string height = (string)Countly.Instance.UserDetails.CustomDataProperties["Set"];
            Assert.AreEqual("6000", height);

            Countly.Instance.UserDetails.Pull("Pull", new string[] { "50KM", "100KM" });
            Assert.IsTrue(Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Pull"));
            dic = Countly.Instance.UserDetails.CustomDataProperties["Pull"] as Dictionary<string, object>;
            Assert.AreEqual("50KM", ((string[])dic["$pull"])[0]);
            Assert.AreEqual("100K", ((string[])dic["$pull"])[1]);

            Countly.Instance.UserDetails.PushUnique("PushUnique", new string[] { "2900.0" });
            Assert.IsTrue(Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("PushU"));
            dic = Countly.Instance.UserDetails.CustomDataProperties["PushU"] as Dictionary<string, object>;
            Assert.AreEqual(new string[] { "2900" }, dic["$addToSet"]);
            Countly.Instance.UserDetails.CustomDataProperties.Clear();

            Countly.Instance.UserDetails.Push("Push", new string[] { "6000.0" });
            Assert.IsTrue(Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Push"));
            dic = Countly.Instance.UserDetails.CustomDataProperties["Push"] as Dictionary<string, object>;
            Assert.AreEqual("6000", ((string[])dic["$push"])[0]);
            Countly.Instance.UserDetails.CustomDataProperties.Clear();

            Countly.Instance.UserDetails.Min("Min", 10.0);
            Assert.IsTrue(Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Min"));
            dic = Countly.Instance.UserDetails.CustomDataProperties["Min"] as Dictionary<string, object>;
            Assert.AreEqual(10.0, dic["$min"]);
            Countly.Instance.UserDetails.CustomDataProperties.Clear();

            Countly.Instance.UserDetails.Max("Max", 10000.0);
            Assert.IsTrue(Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Max"));
            dic = Countly.Instance.UserDetails.CustomDataProperties["Max"] as Dictionary<string, object>;
            Assert.AreEqual(10000.0, dic["$max"]);

            Countly.Instance.UserDetails.Multiply("Multiply", 10.0);
            Assert.IsTrue(Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Multi"));
            dic = Countly.Instance.UserDetails.CustomDataProperties["Multi"] as Dictionary<string, object>;
            Assert.AreEqual(10.0, dic["$mul"]);
            Countly.Instance.UserDetails.CustomDataProperties.Clear();

        }

        /// <summary>
        /// It validate custom user detail invalid keys.
        /// </summary>
        [Test]
        public async void TestCustomUserDetailInvalidKeys()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                MaxKeyLength = 5,
                MaxValueSize = 4,
                EnablePost = true
            };

            Countly.Instance.Init(configuration);
            Countly.Instance.ClearStorage();
            Countly.Instance.UserDetails.CustomDataProperties.Clear();
            Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Clear();

            Assert.IsNotNull(Countly.Instance.UserDetails);
            Assert.AreEqual(0, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);


            Countly.Instance.UserDetails.IncrementBy("", 5);
            Countly.Instance.UserDetails.Increment("");
            Countly.Instance.UserDetails.SetOnce("", "100KM");
            Countly.Instance.UserDetails.Set("", "6000.0");
            Countly.Instance.UserDetails.Pull("", new string[] { "50KM", "100KM" });
            Countly.Instance.UserDetails.PushUnique("", new string[] { "2900.0" });
            Countly.Instance.UserDetails.Push("", new string[] { "6000.0" });
            Countly.Instance.UserDetails.Min("", 10.0);
            Countly.Instance.UserDetails.Max("", 10000.0);
            Countly.Instance.UserDetails.Multiply("", 10.0);

            Countly.Instance.UserDetails.IncrementBy(null, 5);
            Countly.Instance.UserDetails.Increment(null);
            Countly.Instance.UserDetails.SetOnce(null, "100KM");
            Countly.Instance.UserDetails.Set(null, "6000.0");
            Countly.Instance.UserDetails.Pull(null, new string[] { "50KM", "100KM" });
            Countly.Instance.UserDetails.PushUnique(null, new string[] { "2900.0" });
            Countly.Instance.UserDetails.Push(null, new string[] { "6000.0" });
            Countly.Instance.UserDetails.Min(null, 10.0);
            Countly.Instance.UserDetails.Max(null, 10000.0);
            Countly.Instance.UserDetails.Multiply(null, 10.0);

            Assert.AreEqual(0, Countly.Instance.UserDetails.CustomDataProperties.Count);

            await Countly.Instance.UserDetails.SaveAsync();
            Assert.AreEqual(0, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);


        }

        /// <summary>
        /// It validates the user's custom properties set with method 'UserCustomDetails'.
        /// Case 1: If invalid custom detail is provided, no request will add to the request queue.
        /// Case 2: If valid custom detail is provided, a request that have custom detail, should be added in the request queue.
        /// </summary>
        [Test]
        public void TestUserDetailMethod_UserCustomDetails()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                EnablePost = true,
            };

            Countly.Instance.Init(configuration);
            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();

            Assert.IsNotNull(Countly.Instance.UserDetails);
            Assert.AreEqual(0, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);

            Dictionary<string, object> InvalidUserDetails = null;
            Countly.Instance.UserDetails.SetCustomUserDetails(InvalidUserDetails);
            Assert.AreEqual(0, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);

            Dictionary<string, object> userCustomDetail = new Dictionary<string, object> {
                        { "Hair", "Black" },
                        { "Height", "5.9" },
            };

            Countly.Instance.UserDetails.SetCustomUserDetails(userCustomDetail);
            Assert.AreEqual(1, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);

            CountlyRequestModel requestModel = Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Dequeue();
            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            JObject userdetail = JObject.Parse(collection["user_details"]);

            Assert.AreEqual("Black", userdetail["custom"]["Hair"].ToString());
            Assert.AreEqual("5.9", userdetail["custom"]["Height"].ToString());
        }

        /// <summary>
        /// It validates the user's custom properties set with method 'UserCustomDetailsAsync'.
        /// </summary>
        [Test]
        public async void TestUserDetailMethod_UserCustomDetailsAsync()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                EnablePost = true,
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

            Countly.Instance.UserDetails.SetCustomUserDetailsAsync(userDetails);
            Assert.AreEqual(1, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);

            CountlyRequestModel requestModel = Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Dequeue();
            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            JObject userdetail = JObject.Parse(collection["user_details"]);

            Assert.AreEqual("Black", userdetail["custom"]["Hair"].ToString());
            Assert.AreEqual("5.9", userdetail["custom"]["Height"].ToString());
        }
        /// <summary>
        /// It validates the user's custom properties set via 'SetOnce' and 'Set' methods.
        /// </summary>
        [Test]
        public void TestUserCustomProperty_SetOnceAndSet()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey
            };

            Countly.Instance.Init(configuration);
            Countly.Instance.ClearStorage();

            Assert.IsNotNull(Countly.Instance.UserDetails);
            Assert.AreEqual(0, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);


            Countly.Instance.UserDetails.SetOnce("Distance", "100KM");
            Assert.IsTrue(Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Distance"));
            Dictionary<string, object> dic = Countly.Instance.UserDetails.CustomDataProperties["Distance"] as Dictionary<string, object>;
            Assert.AreEqual("100KM", dic["$setOnce"]);

            Countly.Instance.UserDetails.Set("Height", "5.9125");
            Assert.IsTrue(Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Height"));
            string height = (string)Countly.Instance.UserDetails.CustomDataProperties["Height"];
            Assert.AreEqual("5.9125", height);
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
            Assert.IsTrue(Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Distance"));
            Dictionary<string, object> dic = Countly.Instance.UserDetails.CustomDataProperties["Distance"] as Dictionary<string, object>;
            Assert.AreEqual(5, dic["$inc"]);

            Countly.Instance.UserDetails.Increment("Height");
            Assert.IsTrue(Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Height"));
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

            Countly.Instance.UserDetails.Pull("Distance", new string[] { "5" });
            Assert.IsTrue(Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Distance"));
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
            Assert.IsTrue(Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Age"));
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
            Assert.IsTrue(Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Distance"));
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
            Assert.IsTrue(Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Distance"));
            Dictionary<string, object> dic = Countly.Instance.UserDetails.CustomDataProperties["Distance"] as Dictionary<string, object>;
            Assert.AreEqual(2, dic["$mul"]);

            Countly.Instance.UserDetails.Push("Age", new string[] { "29" });
            Assert.IsTrue(Countly.Instance.UserDetails.CustomDataProperties.ContainsKey("Age"));
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
