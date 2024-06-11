using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Models;

namespace Assets.Tests.PlayModeTests
{
    public class UserCustomDetailsTests
    {
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

        // 'SetUserDetailsAsync' method in Countly.Instance.UserDetails
        // We call the method first with a null value then a valid user details value
        // Null value shouldn't record anything, valid values should be recorded correctly
        [Test]
        public async void SetUserDetailsAsync()
        {
            Countly.Instance.Init(TestUtility.CreateBaseConfig());
            Assert.IsNotNull(Countly.Instance.UserDetails);
            TestUtility.ValidateRQEQSize(Countly.Instance, 1, 0);
            Countly.Instance.RequestHelper._requestRepo.Clear();
            TestUtility.ValidateRQEQSize(Countly.Instance, 0, 0);

            CountlyUserDetailsModel userDetails = null;

            await Countly.Instance.UserDetails.SetUserDetailsAsync(userDetails);
            TestUtility.ValidateRQEQSize(Countly.Instance, 0, 0);

            userDetails = new CountlyUserDetailsModel("Full Name", "username", "useremail@email.com", "Organization",
                    "222-222-222",
                    "http://webresizer.com/images2/bird1_after.jpg",
                    "M", "1986",
                    new Dictionary<string, object>{
                        { "Hair", "Black" },
                        { "Height", "5.9" },
                    });
            await Countly.Instance.UserDetails.SetUserDetailsAsync(userDetails);

            TestUtility.ValidateRQEQSize(Countly.Instance, 1, 0);
            CountlyRequestModel requestModel = Countly.Instance.RequestHelper._requestRepo.Dequeue();

            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            AssertlUserDetailRequest(collection, userDetails, userDetails.Custom);
        }

        // 'SetUserDetailAsync' method in Countly.Instance.UserDetails
        // We pass invalid URLs to the user profiles property as null, empty and non url string
        // Nothing should break and picture url should be recorded
        [TestCase("Invalid URL", "Invalid URL")]
        [TestCase(null, null)]
        [TestCase("", "")]
        public async void SetPicturePath(string setPictureUrl, string expectedValue)
        {
            Countly.Instance.Init(TestUtility.CreateBaseConfig());
            Countly.Instance.RequestHelper._requestRepo.Clear();

            // Ensure that the UserDetails instance is not null and request repository is empty
            Assert.IsNotNull(Countly.Instance.UserDetails);
            TestUtility.ValidateRQEQSize(Countly.Instance, 0, 0);

            // Attempt to set user details with a null user model
            CountlyUserDetailsModel userDetails = null;
            await Countly.Instance.UserDetails.SetUserDetailsAsync(userDetails);
            TestUtility.ValidateRQEQSize(Countly.Instance, 0, 0);

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
            TestUtility.ValidateRQEQSize(Countly.Instance, 1, 0);
            CountlyRequestModel requestModel = Countly.Instance.RequestHelper._requestRepo.Dequeue();
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

        // 'SetUserDetailAsync' method in Countly.Instance.UserDetails
        // We check configuration limits for user profile fields
        // Nothing should break and values should be recorded within limits
        [Test]
        public async void TestUserProfileFieldsLimits()
        {
            CountlyConfiguration configuration = TestUtility.CreateBaseConfig()
                .SetMaxValueSize(3);

            // Initialize, ensure that the UserDetails instance is not null and request repository is empty
            Countly.Instance.Init(configuration);
            Countly.Instance.RequestHelper._requestRepo.Clear();
            Assert.IsNotNull(Countly.Instance.UserDetails);
            TestUtility.ValidateRQEQSize(Countly.Instance, 0, 0);

            CountlyUserDetailsModel userDetails = null;

            await Countly.Instance.UserDetails.SetUserDetailsAsync(userDetails);
            TestUtility.ValidateRQEQSize(Countly.Instance, 0, 0);
            
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
            TestUtility.ValidateRQEQSize(Countly.Instance, 1, 0);
            
            CountlyRequestModel requestModel = Countly.Instance.RequestHelper._requestRepo.Dequeue();
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

        // 'SetCustomUserDetails' method in Countly.Instance.UserDetails
        // We record valid custom detail values
        // Nothing should break, values should be recorded correctly
        [Test]
        public void SetCustomUserDetails()
        {
            // Initialize, ensure that the UserDetails instance is not null and request repository is empty
            Countly.Instance.Init(TestUtility.CreateBaseConfig());
            Countly.Instance.RequestHelper._requestRepo.Clear();
            Assert.IsNotNull(Countly.Instance.UserDetails);
            TestUtility.ValidateRQEQSize(Countly.Instance, 0, 0);

            Dictionary<string, object> userCustomDetail = null;
            Countly.Instance.UserDetails.SetCustomUserDetails(userCustomDetail);
            TestUtility.ValidateRQEQSize(Countly.Instance, 0, 0);

            userCustomDetail = new Dictionary<string, object> {
                        { "Hair", "Black" },
                        { "Height", "5.9" },
            };
            Countly.Instance.UserDetails.SetCustomUserDetails(userCustomDetail);
            TestUtility.ValidateRQEQSize(Countly.Instance, 1, 0);

            CountlyRequestModel requestModel = Countly.Instance.RequestHelper._requestRepo.Dequeue();
            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            AssertlUserDetailRequest(collection, null, userCustomDetail);
        }

        // 'SetCustomUserDetails' method in Countly.Instance.UserDetails
        // We check configuration internal limits for custom detail fields
        // Nothing should break, values should be recorded correctly within limits
        [Test]
        public void UserDetailSegmentLimits()
        {
            CountlyConfiguration configuration = TestUtility.CreateBaseConfig()
                .SetMaxKeyLength(5)
                .SetMaxValueSize(6);

            // Initialize, ensure that the UserDetails instance is not null and request repository is empty
            Countly.Instance.Init(configuration);
            Countly.Instance.RequestHelper._requestRepo.Clear();
            Assert.IsNotNull(Countly.Instance.UserDetails);
            TestUtility.ValidateRQEQSize(Countly.Instance, 0, 0);

            Dictionary<string, object> userCustomDetail = null;
            Countly.Instance.UserDetails.SetCustomUserDetails(userCustomDetail);
            TestUtility.ValidateRQEQSize(Countly.Instance, 0, 0);

            userCustomDetail = new Dictionary<string, object> {
                        { "Hair", "Black_1" },
                        { "Height", "5.9" },
            };

            Countly.Instance.UserDetails.SetCustomUserDetails(userCustomDetail);
            TestUtility.ValidateRQEQSize(Countly.Instance, 1, 0);

            CountlyRequestModel requestModel = Countly.Instance.RequestHelper._requestRepo.Dequeue();
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

        // User detail methods in Countly.Instance.UserDetails
        // We check configuration internal limits for custom detail segments key and value limits.
        // Nothing should break, values should be recorded correctly within limits
        [Test]
        public void CustomUserDetailSegmentLimits()
        {
            CountlyConfiguration configuration = TestUtility.CreateBaseConfig()
                .SetMaxKeyLength(5)
                .SetMaxValueSize(4)
                .EnableForcedHttpPost();

            // Initialize, ensure that the UserDetails instance is not null and request repository is empty
            Countly.Instance.Init(configuration);
            Countly.Instance.RequestHelper._requestRepo.Clear();
            Assert.IsNotNull(Countly.Instance.UserDetails);
            TestUtility.ValidateRQEQSize(Countly.Instance, 0, 0);

            Countly.Instance.UserDetails.IncrementBy("IncrementBy", 5);
            Assert.IsTrue(Countly.Instance.UserDetails.ContainsCustomDataKey("Incre"));
            Dictionary<string, object> dic = Countly.Instance.UserDetails.RetrieveCustomDataValue("Incre") as Dictionary<string, object>;
            Assert.AreEqual(5, dic["$inc"]);

            Countly.Instance.UserDetails.Increment("Increment");
            Assert.IsTrue(Countly.Instance.UserDetails.ContainsCustomDataKey("Incre"));
            dic = Countly.Instance.UserDetails.RetrieveCustomDataValue("Incre") as Dictionary<string, object>;
            Assert.AreEqual(1, dic["$inc"]);

            Countly.Instance.UserDetails.SetOnce("SetOnce", "100KM");
            Assert.IsTrue(Countly.Instance.UserDetails.ContainsCustomDataKey("SetOn"));
            dic = Countly.Instance.UserDetails.RetrieveCustomDataValue("SetOn") as Dictionary<string, object>;
            Assert.AreEqual("100K", dic[key: "$setOnce"]);

            Countly.Instance.UserDetails.Set("Set", "6000.0");
            Assert.IsTrue(Countly.Instance.UserDetails.ContainsCustomDataKey("Set"));
            string height = (string)Countly.Instance.UserDetails.RetrieveCustomDataValue("Set");
            Assert.AreEqual("6000", height);

            Countly.Instance.UserDetails.Pull("Pull", new string[] { "50KM", "100KM" });
            Assert.IsTrue(Countly.Instance.UserDetails.ContainsCustomDataKey("Pull"));
            dic = Countly.Instance.UserDetails.RetrieveCustomDataValue("Pull") as Dictionary<string, object>;
            Assert.AreEqual("50KM", ((string[])dic["$pull"])[0]);
            Assert.AreEqual("100K", ((string[])dic["$pull"])[1]);

            Countly.Instance.UserDetails.PushUnique("PushUnique", new string[] { "2900.0" });
            Assert.IsTrue(Countly.Instance.UserDetails.ContainsCustomDataKey("PushU"));
            dic = Countly.Instance.UserDetails.RetrieveCustomDataValue("PushU") as Dictionary<string, object>;
            Assert.AreEqual(new string[] { "2900" }, dic["$addToSet"]);

            Countly.Instance.UserDetails.Push("Push", new string[] { "6000.0" });
            Assert.IsTrue(Countly.Instance.UserDetails.ContainsCustomDataKey("Push"));
            dic = Countly.Instance.UserDetails.RetrieveCustomDataValue("Push") as Dictionary<string, object>;
            Assert.AreEqual("6000", ((string[])dic["$push"])[0]);

            Countly.Instance.UserDetails.Min("Min", 10.0);
            Assert.IsTrue(Countly.Instance.UserDetails.ContainsCustomDataKey("Min"));
            dic = Countly.Instance.UserDetails.RetrieveCustomDataValue("Min") as Dictionary<string, object>;
            Assert.AreEqual(10.0, dic["$min"]);

            Countly.Instance.UserDetails.Max("Max", 10000.0);
            Assert.IsTrue(Countly.Instance.UserDetails.ContainsCustomDataKey("Max"));
            dic = Countly.Instance.UserDetails.RetrieveCustomDataValue("Max") as Dictionary<string, object>;
            Assert.AreEqual(10000.0, dic["$max"]);

            Countly.Instance.UserDetails.Multiply("Multiply", 10.0);
            Assert.IsTrue(Countly.Instance.UserDetails.ContainsCustomDataKey("Multi"));
            dic = Countly.Instance.UserDetails.RetrieveCustomDataValue("Multi") as Dictionary<string, object>;
            Assert.AreEqual(10.0, dic["$mul"]);
        }

        // User detail methods in Countly.Instance.UserDetails
        // We validate custom user detail methods with invalid keys
        // Nothing should be recorded and nothing should break
        [Test]
        public async void CustomUserDetailInvalidKeys()
        {
            CountlyConfiguration configuration = TestUtility.CreateBaseConfig()
                .SetMaxKeyLength(5)
                .SetMaxValueSize(4)
                .EnableForcedHttpPost();

            // Initialize, ensure that the UserDetails instance is not null and request repository is empty
            Countly.Instance.Init(configuration);
            Countly.Instance.RequestHelper._requestRepo.Clear();
            Assert.IsNotNull(Countly.Instance.UserDetails);
            TestUtility.ValidateRQEQSize(Countly.Instance, 0, 0);

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

            await Countly.Instance.UserDetails.SaveAsync();
            TestUtility.ValidateRQEQSize(Countly.Instance, 0, 0);
        }

        // 'SetOnce' and 'Set' methods in Countly.Instance.UserDetails
        // We validate the user's custom properties set via these methods
        // Nothing should break, values should be recorded correctly
        [Test]
        public void UserCustomProperty_SetOnceAndSet()
        {
            // Initialize, ensure that the UserDetails instance is not null and request repository is empty
            Countly.Instance.Init(TestUtility.CreateBaseConfig());
            Countly.Instance.RequestHelper._requestRepo.Clear();
            Assert.IsNotNull(Countly.Instance.UserDetails);
            TestUtility.ValidateRQEQSize(Countly.Instance, 0, 0);

            Countly.Instance.UserDetails.SetOnce("Distance", "100KM");
            Assert.IsTrue(Countly.Instance.UserDetails.ContainsCustomDataKey("Distance"));
            Dictionary<string, object> dic = Countly.Instance.UserDetails.RetrieveCustomDataValue("Distance") as Dictionary<string, object>;
            Assert.AreEqual("100KM", dic["$setOnce"]);

            Countly.Instance.UserDetails.Set("Height", "5.9125");
            Assert.IsTrue(Countly.Instance.UserDetails.ContainsCustomDataKey("Height"));
            string height = (string)Countly.Instance.UserDetails.RetrieveCustomDataValue("Height");
            Assert.AreEqual("5.9125", height);
        }

        // 'IncrementBy' and 'Increment' methods in Countly.Instance.UserDetails
        // We validate the user's custom properties set via these methods
        // Nothing should break, values should be recorded correctly
        [Test]
        public void UserCustomProperty_IncrementBy()
        {
            // Initialize, ensure that the UserDetails instance is not null and request repository is empty
            Countly.Instance.Init(TestUtility.CreateBaseConfig());
            Countly.Instance.RequestHelper._requestRepo.Clear();
            Assert.IsNotNull(Countly.Instance.UserDetails);
            TestUtility.ValidateRQEQSize(Countly.Instance, 0, 0);

            Countly.Instance.UserDetails.IncrementBy("Distance", 5);
            Assert.IsTrue(Countly.Instance.UserDetails.ContainsCustomDataKey("Distance"));
            Dictionary<string, object> dic = Countly.Instance.UserDetails.RetrieveCustomDataValue("Distance") as Dictionary<string, object>;
            Assert.AreEqual(5, dic["$inc"]);

            Countly.Instance.UserDetails.Increment("Height");
            Assert.IsTrue(Countly.Instance.UserDetails.ContainsCustomDataKey("Height"));
            Dictionary<string, object> dic1 = Countly.Instance.UserDetails.RetrieveCustomDataValue("Height") as Dictionary<string, object>;
            Assert.AreEqual(1, dic1["$inc"]);
        }

        // 'Pull' method in Countly.Instance.UserDetails
        // We validate the user's custom property set via 'Pull'
        // Nothing should break, values should be recorded correctly
        [Test]
        public void UserCustomProperty_Pull()
        {
            // Initialize, ensure that the UserDetails instance is not null and request repository is empty
            Countly.Instance.Init(TestUtility.CreateBaseConfig());
            Countly.Instance.RequestHelper._requestRepo.Clear();
            Assert.IsNotNull(Countly.Instance.UserDetails);
            TestUtility.ValidateRQEQSize(Countly.Instance, 0, 0);

            Countly.Instance.UserDetails.Pull("Distance", new string[] { "5" });
            Assert.IsTrue(Countly.Instance.UserDetails.ContainsCustomDataKey("Distance"));
            Dictionary<string, object> dic = Countly.Instance.UserDetails.RetrieveCustomDataValue("Distance") as Dictionary<string, object>;
            Assert.AreEqual(new string[] { "5" }, dic["$pull"]);
        }

        // 'PushUnique' and 'Push' methods in Countly.Instance.UserDetails
        // We validate the user's custom property set via these methods
        // Nothing should break, values should be recorded correctly
        [Test]
        public void UserCustomProperty_PushUniqueAndPush()
        {
            // Initialize, ensure that the UserDetails instance is not null and request repository is empty
            Countly.Instance.Init(TestUtility.CreateBaseConfig());
            Countly.Instance.RequestHelper._requestRepo.Clear();
            Assert.IsNotNull(Countly.Instance.UserDetails);
            TestUtility.ValidateRQEQSize(Countly.Instance, 0, 0);

            Countly.Instance.UserDetails.PushUnique("Age", new string[] { "29" });
            Assert.IsTrue(Countly.Instance.UserDetails.ContainsCustomDataKey("Age"));
            Dictionary<string, object> dic = Countly.Instance.UserDetails.RetrieveCustomDataValue("Age") as Dictionary<string, object>;
            Assert.AreEqual(new string[] { "29" }, dic["$addToSet"]);

            Countly.Instance.UserDetails.Push("Height", new string[] { "6" });
            Assert.IsTrue(Countly.Instance.UserDetails.ContainsCustomDataKey("Height"));
            Dictionary<string, object> dic2 = Countly.Instance.UserDetails.RetrieveCustomDataValue("Height") as Dictionary<string, object>;
            Assert.AreEqual(new string[] { "6" }, dic2["$push"]);
        }

        // 'Min' and 'Max' methods in Countly.Instance.UserDetails
        // We validate the user's custom property set via these methods
        // Nothing should break, values should be recorded correctly
        [Test]
        public void UserCustomProperty_MinAndMax()
        {
            // Initialize, ensure that the UserDetails instance is not null and request repository is empty
            Countly.Instance.Init(TestUtility.CreateBaseConfig());
            Countly.Instance.RequestHelper._requestRepo.Clear();
            Assert.IsNotNull(Countly.Instance.UserDetails);
            TestUtility.ValidateRQEQSize(Countly.Instance, 0, 0);

            Countly.Instance.UserDetails.Min("Distance", 10.0);
            Assert.IsTrue(Countly.Instance.UserDetails.ContainsCustomDataKey("Distance"));
            Dictionary<string, object> dic = Countly.Instance.UserDetails.RetrieveCustomDataValue("Distance") as Dictionary<string, object>;
            Assert.AreEqual(10.0, dic["$min"]);

            Countly.Instance.UserDetails.Max("Distance", 100.0);
            Assert.IsTrue(Countly.Instance.UserDetails.ContainsCustomDataKey("Distance"));
            Dictionary<string, object> dic1 = Countly.Instance.UserDetails.RetrieveCustomDataValue("Distance") as Dictionary<string, object>;
            Assert.AreEqual(100.0, dic1["$max"]);
        }
        
        // 'SaveAsync' method in Countly.Instance.UserDetails
        // We validate the user's custom properties before and after calling 'SaveAsync'.
        // Nothing should break, values should be recorded correctly
        [Test]
        public async void UserDetailService_SaveAsync()
        {
            // Initialize, ensure that the UserDetails instance is not null and request repository is empty
            Countly.Instance.Init(TestUtility.CreateBaseConfig());
            Countly.Instance.RequestHelper._requestRepo.Clear();
            Assert.IsNotNull(Countly.Instance.UserDetails);
            TestUtility.ValidateRQEQSize(Countly.Instance, 0, 0);

            Countly.Instance.UserDetails.Multiply("Distance", 2);
            Assert.IsTrue(Countly.Instance.UserDetails.ContainsCustomDataKey("Distance"));
            Dictionary<string, object> dic = Countly.Instance.UserDetails.RetrieveCustomDataValue("Distance") as Dictionary<string, object>;
            Assert.AreEqual(2, dic["$mul"]);

            Countly.Instance.UserDetails.Push("Age", new string[] { "29" });
            Assert.IsTrue(Countly.Instance.UserDetails.ContainsCustomDataKey("Age"));
            Dictionary<string, object> dic2 = Countly.Instance.UserDetails.RetrieveCustomDataValue("Age") as Dictionary<string, object>;
            Assert.AreEqual(new string[] { "29" }, dic2["$push"]);

            await Countly.Instance.UserDetails.SaveAsync();

            Assert.AreEqual(false, Countly.Instance.UserDetails.ContainsCustomDataKey("Age"));
            Assert.AreEqual(false, Countly.Instance.UserDetails.ContainsCustomDataKey("Distance"));
        }

        [SetUp]
        [TearDown]
        public void End()
        {
            TestUtility.TestCleanup();
        }
    }
}
