using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Models;
using UnityEngine;

namespace Assets.Tests.PlayModeTests
{
    /**************************************************************
    A: Automatic Session
    M: Manual Session
    CR: Consent Required
    CNR: Consent Not Required
    CG: Consent Given
    CNG: Consent Not Given
    ***************************************************************/

    public class UserProfileTests
    {
        private Dictionary<string, object> ExpectedUserProperty()
        {
            var expectedUserDetails = new Dictionary<string, object>
            {
                { "name", "Nicola Tesla" },
                { "username", "nicola" },
                { "email", "info@nicola.tesla" },
                { "organization", "Trust Electric Ltd" },
                { "phone", "90 822 140 2546" },
                { "picture", "http://images2.fanpop.com/images/photos/3300000/Nikola-Tesla-nikola-tesla-3365940-600-738.jpg" },
                { "gender", "M" },
                { "byear", 1919 },
                { "Boolean", true },
                { "Integer", 26 },
                { "Float", 3.1f },
                { "String", "something special cooking" },
                { "IntArray", new [] { 1, 2, 3 } },
                { "BoolArray", new [] { true, false, true } },
                { "FloatArray", new [] { 1.1f, 2.2f, 3.3f } },
                { "StringArray", new [] { "a", "b", "c" } },
                { "IntList", new List<int> { 1, 2, 3 } },
                { "BoolList", new List<bool> { true, false, true } },
                { "FloatList", new List<float> { 1.1f, 2.2f, 3.3f } },
                { "StringList", new List<string> { "a", "b", "c" } }
            };

            return expectedUserDetails;
        }

        private Dictionary<string, object> ExpectedUserData()
        {
            var expectedUserDetails = new Dictionary<string, object>
            {
                { "a12345", "My Property" },
                { "b12345", new Dictionary<string, object> { { "$inc", 1.0 } } },
                { "c12345", new Dictionary<string, object> { { "$inc", 10.0 } } },
                { "d12345", new Dictionary<string, object> { { "$mul", 20.0 } } },
                { "e12345", new Dictionary<string, object> { { "$max", 100.0 } } },
                { "f12345", new Dictionary<string, object> { { "$min", 50.0 } } },
                { "g12345", new Dictionary<string, object> { { "$setOnce", "200" } } },
                { "h12345", new Dictionary<string, object> { { "$addToSet", new List<string> { "morning" } } } },
                { "i12345", new Dictionary<string, object> { { "$push", new List<string> { "morning" } } } },
                { "k12345", new Dictionary<string, object> { { "$pull", new List<string> { "morning" } } } }
            };

            return expectedUserDetails;
        }

        private void SendUserData()
        {
            Countly.Instance.UserProfile.SetProperty("a12345", "My Property");
            Countly.Instance.UserProfile.Increment("b12345");
            Countly.Instance.UserProfile.IncrementBy("c12345", 10);
            Countly.Instance.UserProfile.Multiply("d12345", 20);
            Countly.Instance.UserProfile.Max("e12345", 100);
            Countly.Instance.UserProfile.Min("f12345", 50);
            Countly.Instance.UserProfile.SetOnce("g12345", "200");
            Countly.Instance.UserProfile.PushUnique("h12345", "morning");
            Countly.Instance.UserProfile.Push("i12345", "morning");
            Countly.Instance.UserProfile.Pull("k12345", "morning");
        }

        // 'SetUserProperties' in CountlyConfiguration
        // We set user profile data during configuration and initialize the sdk with automatic sessions enabled
        // When automatic session starts in initialzation, it should create an User Profile request
        [Test]
        public void ConfigTimeUserProfile_A_CNR()
        {
            CountlyConfiguration config = TestUtility.CreateBaseConfig()
                .SetUserProperties(ExpectedUserProperty());

            Countly cly = Countly.Instance;
            cly.Init(config);

            TestUtility.ValidateRQEQSize(cly, 2, 0);
            Dictionary<string, object> up1 = TestUtility.ExtractAndDeserializeUserDetails(cly.RequestHelper._requestRepo.Models);
            TestUtility.ValidateUserDetails(up1, ExpectedUserProperty());
            cly.RequestHelper._requestRepo.Clear();
        }

        // 'SetUserProperties' in CountlyConfiguration
        // We set user profile data during configuration and initialize the sdk with automatic sessions disabled
        // When initialization is completed, it should create an User Profile request
        [Test]
        public void ConfigTimeUserProfile_M_CNR()
        {
            CountlyConfiguration config = TestUtility.CreateBaseConfig()
                .SetUserProperties(ExpectedUserProperty());

            config.DisableAutomaticSessionTracking();
            Countly cly = Countly.Instance;
            cly.Init(config);

            TestUtility.ValidateRQEQSize(cly, 1, 0);
            Dictionary<string, object> up1 = TestUtility.ExtractAndDeserializeUserDetails(cly.RequestHelper._requestRepo.Models);
            TestUtility.ValidateUserDetails(up1, ExpectedUserProperty());
            cly.RequestHelper._requestRepo.Clear();
        }

        // 'SetUserProperties' in CountlyConfiguration
        // We set user profile data during configuration and initialize the sdk with consent requirement, however not provide it
        // When initialization is completed, it should not create an User Profile request
        [Test]
        public void ConfigTimeUserProfile_A_CR_CNG()
        {
            CountlyConfiguration config = TestUtility.CreateBaseConfig()
                .SetRequiresConsent(true)
                .SetUserProperties(ExpectedUserProperty());

            Countly cly = Countly.Instance;
            cly.Init(config);
            // 1 request for session and 1 for consents
            TestUtility.ValidateRQEQSize(cly, 2, 0);
            // which should return an empty dictionary since there are no user profile requests
            Dictionary<string, object> up1 = TestUtility.ExtractAndDeserializeUserDetails(cly.RequestHelper._requestRepo.Models);
            Assert.IsTrue(up1.Count == 0);
        }

        // 'SetUserProperties' in CountlyConfiguration
        // We set user profile data during configuration and initialize the sdk with consent requirement, however not provide it
        // When initialization is completed, it should not create an User Profile request
        [Test]
        public void ConfigTimeUserProfile_A_CR_CG()
        {
            Consents[] consent = new Consents[] { Consents.Crashes, Consents.Events, Consents.Clicks, Consents.StarRating, Consents.Views, Consents.Users, Consents.Push, Consents.RemoteConfig, Consents.Location, Consents.Feedback, Consents.Sessions };
            CountlyConfiguration config = TestUtility.CreateBaseConfigConsent(consent)
                .SetUserProperties(ExpectedUserProperty());

            Countly cly = Countly.Instance;
            cly.Init(config);

            TestUtility.ValidateRQEQSize(cly, 3, 0);
            Dictionary<string, object> up1 = TestUtility.ExtractAndDeserializeUserDetails(cly.RequestHelper._requestRepo.Models);
            TestUtility.ValidateUserDetails(up1, ExpectedUserProperty());
            cly.RequestHelper._requestRepo.Clear();
        }

        // 'Save' in Countly.Instance.UserProfile
        // We initialize the sdk and call save afterwards without recording any user profile data
        // Calling 'Save' should not record any request
        [Test]
        public void SaveWithNoRecordedValue_A_CNR()
        {
            Countly cly = Countly.Instance;
            cly.Init(TestUtility.CreateBaseConfig());

            TestUtility.ValidateRQEQSize(cly, 1, 0);
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 1, 0);
        }

        // 'Save' in Countly.Instance.UserProfile
        // We initialize the sdk and record user profile values afterwards. Then we call save.
        // Calling 'Save' should record an User Profile request
        [Test]
        public void SaveWithRecordedValue_A_CNR()
        {
            Countly cly = Countly.Instance;
            cly.Init(TestUtility.CreateBaseConfig());

            cly.UserProfile.SetProperties(ExpectedUserProperty());
            cly.UserProfile.Save();

            TestUtility.ValidateRQEQSize(cly, 2, 0);
            Dictionary<string, object> up1 = TestUtility.ExtractAndDeserializeUserDetails(cly.RequestHelper._requestRepo.Models);
            TestUtility.ValidateUserDetails(up1, ExpectedUserProperty());
            cly.RequestHelper._requestRepo.Clear();
        }
        
        // 'Save' in Countly.Instance.UserProfile
        // We initialize the sdk with consent requirement however not give consent. We try to record user profile data and call save
        // Since consent is not provided calling 'Save' should not record any request
        [Test]
        public void SaveWithRecordedValue_CR_CNG()
        { 
            CountlyConfiguration config = TestUtility.CreateBaseConfig()
                .SetRequiresConsent(true);

            Countly cly = Countly.Instance;
            cly.Init(config);

            SendUserData();
            cly.UserProfile.Save();

            // 1 request for session and 1 for consents
            TestUtility.ValidateRQEQSize(cly, 2, 0);
            // which should return an empty dictionary since there are no user profile requests
            Dictionary<string, object> up1 = TestUtility.ExtractAndDeserializeUserDetails(cly.RequestHelper._requestRepo.Models);
            Assert.IsTrue(up1.Count == 0);
        }
        
        // 'Save' in Countly.Instance.UserProfile
        // We initialize the sdk with consent requirement and give consent. We try to record user profile data and call save
        // Since consent is provided calling 'Save' should record an User Profile request
        [Test]
        public void SaveWithRecordedValue_CR_CG()
        { 
            Consents[] consent = new Consents[] { Consents.Crashes, Consents.Events, Consents.Clicks, Consents.StarRating, Consents.Views, Consents.Users, Consents.Push, Consents.RemoteConfig, Consents.Location, Consents.Feedback, Consents.Sessions };
            CountlyConfiguration config = TestUtility.CreateBaseConfigConsent(consent);

            Countly cly = Countly.Instance;
            cly.Init(config);

            SendUserData();
            cly.UserProfile.Save();

            TestUtility.ValidateRQEQSize(cly, 3, 0);
            Dictionary<string, object> up1 = TestUtility.ExtractAndDeserializeUserDetails(cly.RequestHelper._requestRepo.Models);
            TestUtility.ValidateUserDetails(up1, ExpectedUserData());
        }

        [SetUp]
        [TearDown]
        public void End()
        {
            TestUtility.TestCleanup();
        }
    }
}