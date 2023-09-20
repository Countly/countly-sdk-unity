using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using Assets.Tests.PlayModeTests;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Models;
using UnityEngine;

namespace Tests
{
    public class UserCustomDetailsTests
    {
        private readonly string _serverUrl = "https://xyz.com/";
        private readonly string _appKey = "772c091355076ead703f987fee94490";

        void AssertlUserDetailRequest(NameValueCollection collection, CountlyUserDetailsModel userInfo, IDictionary<string, object> customInfo)
        {
            JObject userDetailJson = JObject.Parse(collection["user_details"]);

            if (userInfo != null) {
                Assert.AreEqual(userInfo.Name, userDetailJson["name"].ToString());
                Assert.AreEqual(userInfo.Username, userDetailJson["username"].ToString());
                Assert.AreEqual(userInfo.Email, userDetailJson["email"].ToString());
                Assert.AreEqual(userInfo.Organization, userDetailJson["organization"].ToString());
                Assert.AreEqual(userInfo.Phone, userDetailJson["phone"].ToString());
                Assert.AreEqual(userInfo.PictureUrl, userDetailJson["picture"].ToString());
                Assert.AreEqual(userInfo.Gender, userDetailJson["gender"].ToString());
                Assert.AreEqual(userInfo.BirthYear, userDetailJson["byear"].ToString());
            }

            if (customInfo != null) {
                foreach (KeyValuePair<string, object> entry in customInfo) {
                    Assert.AreEqual(entry.Value, userDetailJson["custom"][entry.Key].ToString());
                }
            }
        }

        /// <summary>
        /// It check the working of method 'SetUserDetailsAsync'.
        /// </summary>
        [Test]
        public async void SetUserDetailsAsync()
        {
            Countly.Instance.Init(TestUtility.createBaseConfig());

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
            AssertlUserDetailRequest(collection, userDetails, userDetails.Custom);
        }

        /// <summary>
        /// 'SetUserDetailAsync' method in UserDetailsCountlyService
        /// we pass an invalid URL to the user profiles property
        /// This value should then be used in the end request. It should not be rejected
        /// </summary>
        [Test]
        public async void SetPicturePath_invalidUrl()
        {
            SetPicturePath_base("Invalid URL", "Invalid URL");
        }


        /// <summary>
        /// 'SetUserDetailAsync' method in UserDetailsCountlyService
        /// we pass an 'null' URL to the user profiles property
        /// This value should then be used in the end request. It should not be rejected
        /// </summary>
        [Test]
        public async void SetPicturePath_nullUrl()
        {
            SetPicturePath_base(null, null);
        }

        /// <summary>
        /// 'SetUserDetailAsync' method in UserDetailsCountlyService
        /// we pass an empty string URL to the user profiles property
        /// This value should then be used in the end request. It should not be rejected.
        /// </summary>
        [Test]
        public async void SetPicturePath_emptyUrl()
        {
            SetPicturePath_base("", "");
        }

        public async void SetPicturePath_base(string setPictureUrl, string expectedValue)
        {
            Countly.Instance.Init(TestUtility.createBaseConfig());
            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();

            // Ensure that the UserDetails instance is not null and request repository is empty
            Assert.IsNotNull(Countly.Instance.UserDetails);
            Assert.AreEqual(0, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);

            // Attempt to set user details with a null user model
            CountlyUserDetailsModel userDetails = null;
            await Countly.Instance.UserDetails.SetUserDetailsAsync(userDetails);
            Assert.AreEqual(0, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);

            // Initialize userDetails and assign an invalid picture URL to the user model
            userDetails = new CountlyUserDetailsModel("FirstName", "LastName", "name@email.com", "Company",
                    "666-777-888",
                    setPictureUrl,
                    "F", "1999",
                    new Dictionary<string, object>{
                        { "Song", "Billie Jean" },
                        { "Team", "Arsenal" },
                    });
            await Countly.Instance.UserDetails.SetUserDetailsAsync(userDetails);
            // Ensure that a request is added, retrieve and parse it for verification
            Assert.AreEqual(1, Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Count);
            CountlyRequestModel requestModel = Countly.Instance.UserDetails._requestCountlyHelper._requestRepo.Dequeue();
            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);

            // Parse the "user_details" JSON from the request data
            JObject userDetailJson = JObject.Parse(collection["user_details"]);
            // Verify that the picture URL in the JSON matches the modified user model's picture URL
            if (expectedValue != null) {
                Assert.AreEqual(expectedValue, userDetailJson["picture"].ToString());
            } else {
                Assert.IsNull(userDetailJson["picture"]);
            }
        }

        /// <summary>
        /// It validate user profile fields limits.
        /// </summary>
        [Test]
        public async void TestUserProfileFieldsLimits()
        {
            CountlyConfiguration configuration = TestUtility.createBaseConfig();
            configuration.MaxValueSize = 3;

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

            userDetails = new CountlyUserDetailsModel("Ful", "use", "use", "Org",
                   "222",
                   userDetails.PictureUrl.Substring(0, 4096),
                   "M", "198",
                   new Dictionary<string, object>{
                        { "Hair", "Bla" },
                        { "Height", "5.9" },
                   });

            AssertlUserDetailRequest(collection, userDetails, userDetails.Custom);
        }

        /// <summary>
        /// It check the working of method 'SetUserDetailsAsync'.
        /// </summary>
        [Test]
        public void SetCustomUserDetailsAsync()
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

            AssertlUserDetailRequest(collection, null, userCustomDetail);
        }


        /// <summary>
        /// It validate user detail segments limits.
        /// </summary>
        [Test]
        public void UserDetailSegmentLimits()
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

            userCustomDetail = new Dictionary<string, object> {
                        { "Hair", "Black_" },
                        { "Heigh", "5.9" },
            };

            AssertlUserDetailRequest(collection, null, userCustomDetail);
        }

        /// <summary>
        /// It validate custom user detail segments key and value limits.
        /// </summary>
        [Test]
        public void CustomUserDetailSegmentLimits()
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
        public async void CustomUserDetailInvalidKeys()
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
        public void UserDetailMethod_UserCustomDetails()
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
            AssertlUserDetailRequest(collection, null, userCustomDetail);
        }


        /// <summary>
        /// It validates the user's custom properties set via 'SetOnce' and 'Set' methods.
        /// </summary>
        [Test]
        public void UserCustomProperty_SetOnceAndSet()
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
        public void UserCustomProperty_IncrementBy()
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
        public void UserCustomProperty_Pull()
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
        public void UserCustomProperty_PushUniqueAndPush()
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
        public void UserCustomProperty_MinAndMax()
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
        public async void UserDetailService_SaveAsync()
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
