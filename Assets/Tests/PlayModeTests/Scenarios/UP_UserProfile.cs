using System.Collections.Generic;
using NUnit.Framework;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Enums;
using System.Threading;
using System.Diagnostics;
using UnityEngine;
using NUnit.Framework.Internal;

namespace Assets.Tests.PlayModeTests.Scenarios
{
    public class UP_UserProfile
    {
        private void SendUserProperty()
        {
            Dictionary<string, object> userProperties = new Dictionary<string, object>
            {
                { "name", "Nicola Tesla" },
                { "username", "nicola" },
                { "email", "info@nicola.tesla" },
                { "organization", "Trust Electric Ltd" },
                { "phone", "+90 822 140 2546" },
                { "picture", "http://images2.fanpop.com/images/photos/3300000/Nikola-Tesla-nikola-tesla-3365940-600-738.jpg" },
                { "gender", "M" },
                { "byear", 1919 },
                { "Boolean", true },
                { "Integer", 26 },
                { "Float", 3.1f },
                { "String", "something special cooking" },
                { "IntArray", new int[] { 1, 2, 3 } },
                { "BoolArray", new bool[] { true, false, true } },
                { "FloatArray", new float[] { 1.1f, 2.2f, 3.3f } },
                { "StringArray", new string[] { "a", "b", "c" } },
                { "IntList", new List<int> { 1, 2, 3 } },
                { "BoolList", new List<bool> { true, false, true } },
                { "FloatList", new List<float> { 1.1f, 2.2f, 3.3f } },
                { "StringList", new List<string> { "a", "b", "c" } }
            };

            Countly.Instance.UserProfile.SetProperties(userProperties);
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

        private void SendSameData()
        {
            Countly.Instance.UserProfile.SetProperty("a12345", "1");
            Countly.Instance.UserProfile.SetProperty("a12345", "2");
            Countly.Instance.UserProfile.SetProperty("a12345", "3");
            Countly.Instance.UserProfile.SetProperty("a12345", "4");
        }

        private Dictionary<string, object> ExpectedSameData()
        {
            // Example expected user details for validation
            var expectedUserDetails = new Dictionary<string, object>
            {
                { "a12345", "4" }
            };

            return expectedUserDetails;
        }

        // UserProfile calls in Countly.Instance.UserProfile
        // We initialize SDK without consent requirement, check the RQ and EQ sizes, then call public methods
        // Methods we call should record nothing in the RQ and EQ
        [Test]
        public void UP_200_CNR_A()
        {
            CountlyConfiguration config = TestUtility.CreateBaseConfig()
                .SetRequiresConsent(false);
            Countly.Instance.Init(config);
            TestUtility.ValidateRQEQSize(Countly.Instance, 1, 0);

            SendUserData();
            SendUserProperty();
            TestUtility.ValidateRQEQSize(Countly.Instance, 1, 0);
        }

        // UserProfile calls in Countly.Instance.UserProfile
        // We initialize SDK with consent requirement and give consents, check the RQ and EQ sizes, then call public methods
        // Methods we call should record nothing in the RQ and EQ
        [Test]
        public void UP_201_CR_CG_A()
        {
            Consents[] consent = new Consents[] { Consents.Crashes, Consents.Events, Consents.Clicks, Consents.StarRating, Consents.Views, Consents.Users, Consents.Push, Consents.RemoteConfig, Consents.Location, Consents.Feedback, Consents.Sessions };
            CountlyConfiguration config = TestUtility.CreateBaseConfigConsent(consent);
            Countly.Instance.Init(config);
            TestUtility.ValidateRQEQSize(Countly.Instance, 2, 0);

            SendUserData();
            SendUserProperty();
            TestUtility.ValidateRQEQSize(Countly.Instance, 2, 0);
        }

        // UserProfile calls in Countly.Instance.UserProfile
        // We initialize SDK with consent requirement and give no consent, check the RQ and EQ sizes, then call public methods
        // Methods we call should record nothing in the RQ and EQ
        [Test]
        public void UP_202_CR_CNG_A()
        {
            Consents[] consent = new Consents[] { };
            CountlyConfiguration config = TestUtility.CreateBaseConfigConsent(consent);
            Countly.Instance.Init(config);
            TestUtility.ValidateRQEQSize(Countly.Instance, 2, 0);

            SendUserData();
            SendUserProperty();
            TestUtility.ValidateRQEQSize(Countly.Instance, 2, 0);
        }

        // RecordEventAsync with UserProfile changes
        // We record events without consent requirement, between events we record user profile data
        // With each event, if user data information is recorded, it should flush EQ and send user profile data
        [Test]
        public void UP_203_CNR_A_Events()
        {
            CountlyConfiguration config = TestUtility.CreateBaseConfig()
                .SetRequiresConsent(false);
            Countly.Instance.Init(config);
            Countly cly = Countly.Instance;
            TestUtility.ValidateRQEQSize(cly, 1, 0);

            _ = Countly.Instance.Events.RecordEventAsync("BasicEventA");
            _ = Countly.Instance.Events.RecordEventAsync("BasicEventB");
            SendSameData();
            _ = Countly.Instance.Events.RecordEventAsync("BasicEventC");
            
            // Extract and validate user_details requests
            TestUtility.ValidateRQEQSize(cly, 3, 1);
            Dictionary<string, object> up1 = TestUtility.ExtractAndDeserializeUserDetails(cly.RequestHelper._requestRepo.Models);
            TestUtility.ValidateUserDetails(up1, ExpectedSameData());
            cly.RequestHelper._requestRepo.Clear();

            SendSameData();
            _ = Countly.Instance.Events.RecordEventAsync("BasicEventD");
            
            // Extract and validate user_details requests
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            Dictionary<string, object> up2 = TestUtility.ExtractAndDeserializeUserDetails(cly.RequestHelper._requestRepo.Models);
            TestUtility.ValidateUserDetails(up2, ExpectedSameData());
            cly.RequestHelper._requestRepo.Clear();

            SendSameData();
            _ = Countly.Instance.Events.RecordEventAsync("BasicEventE");

            // Extract and validate user_details requests
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            Dictionary<string, object> up3 = TestUtility.ExtractAndDeserializeUserDetails(cly.RequestHelper._requestRepo.Models);
            TestUtility.ValidateUserDetails(up3, ExpectedSameData());
            cly.RequestHelper._requestRepo.Clear();
        }

        // RecordEventAsync with UserProfile changes
        // We record events with consent requirement, between events we record user profile data
        // With each event, if user data information is recorded, it should flush EQ and send user profile data
        [Test]
        public void UP_205_CR_CG_A()
        {
            Consents[] consent = new Consents[] { Consents.Crashes, Consents.Events, Consents.Clicks, Consents.StarRating, Consents.Views, Consents.Users, Consents.Push, Consents.RemoteConfig, Consents.Location, Consents.Feedback, Consents.Sessions };
            CountlyConfiguration config = TestUtility.CreateBaseConfigConsent(consent);
            Countly.Instance.Init(config);
            Countly cly = Countly.Instance;
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            _ = Countly.Instance.Events.RecordEventAsync("BasicEventA");
            _ = Countly.Instance.Events.RecordEventAsync("BasicEventB");
            SendSameData();
            _ = Countly.Instance.Events.RecordEventAsync("BasicEventC");

            // Extract and validate user_details requests
            TestUtility.ValidateRQEQSize(cly, 4, 1);
            Dictionary<string, object> up1 = TestUtility.ExtractAndDeserializeUserDetails(cly.RequestHelper._requestRepo.Models);
            TestUtility.ValidateUserDetails(up1, ExpectedSameData());
            cly.RequestHelper._requestRepo.Clear();

            SendSameData();
            _ = Countly.Instance.Events.RecordEventAsync("BasicEventD");

            // Extract and validate user_details requests
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            Dictionary<string, object> up2 = TestUtility.ExtractAndDeserializeUserDetails(cly.RequestHelper._requestRepo.Models);
            TestUtility.ValidateUserDetails(up2, ExpectedSameData());
            cly.RequestHelper._requestRepo.Clear();

            SendSameData();
            _ = Countly.Instance.Events.RecordEventAsync("BasicEventE");

            // Extract and validate user_details requests
            TestUtility.ValidateRQEQSize(cly, 2, 1);
            Dictionary<string, object> up3 = TestUtility.ExtractAndDeserializeUserDetails(cly.RequestHelper._requestRepo.Models);
            TestUtility.ValidateUserDetails(up3, ExpectedSameData());
            cly.RequestHelper._requestRepo.Clear();
        }

        // RecordEventAsync with UserProfile changes
        // We record events with consent requirement, however provide no consent. Between events we record user profile data
        // Since no consent is provided, events and user profile calls should not record anything
        [Test]
        public void UP_206_CR_CNG_A()
        {
            Consents[] consent = new Consents[] { };
            CountlyConfiguration config = TestUtility.CreateBaseConfigConsent(consent);
            Countly.Instance.Init(config);
            TestUtility.ValidateRQEQSize(Countly.Instance, 2, 0);

            _ = Countly.Instance.Events.RecordEventAsync("BasicEventA");
            _ = Countly.Instance.Events.RecordEventAsync("BasicEventB");
            SendSameData();
            _ = Countly.Instance.Events.RecordEventAsync("BasicEventC");
            SendSameData();
            _ = Countly.Instance.Events.RecordEventAsync("BasicEventD");
            SendSameData();
            _ = Countly.Instance.Events.RecordEventAsync("BasicEventE");

            TestUtility.ValidateRQEQSize(Countly.Instance, 2, 0);
        }

        // DeviceID changes with UserProfile changes
        // We record events and User Profile data without consent requirement. We also change device id with and without merge
        // Since consent is not required, events, device id change with merge and events should be recorded correctly
        [Test]
        public void UP_207_CNR_M()
        {
            CountlyConfiguration config = TestUtility.CreateBaseConfig();
            config.DisableAutomaticSessionTracking();
            Countly.Instance.Init(config);
            Countly cly = Countly.Instance;
            _ = cly.Session.BeginSessionAsync();
            TestUtility.ValidateRQEQSize(cly, 1, 0);

            _ = Countly.Instance.Events.RecordEventAsync("BasicEventA");
            _ = Countly.Instance.Events.RecordEventAsync("BasicEventB");
            SendSameData();
            _ = cly.Session.EndSessionAsync();

            // Extract and validate user_details requests
            TestUtility.ValidateRQEQSize(cly, 4, 0);
            Dictionary<string, object> up1 = TestUtility.ExtractAndDeserializeUserDetails(cly.RequestHelper._requestRepo.Models);
            TestUtility.ValidateUserDetails(up1, ExpectedSameData());
            cly.RequestHelper._requestRepo.Clear();

            _ = Countly.Instance.Events.RecordEventAsync("BasicEventC");
            SendUserData();
            _ = cly.Session.EndSessionAsync();
            _ = cly.Device.ChangeDeviceIdWithMerge("merge_id");
            // Extract and validate user_details requests
            TestUtility.ValidateRQEQSize(cly, 3, 0);
            Dictionary<string, object> up2 = TestUtility.ExtractAndDeserializeUserDetails(cly.RequestHelper._requestRepo.Models);
            TestUtility.ValidateUserDetails(up2, ExpectedUserData());
            cly.RequestHelper._requestRepo.Clear();

            SendSameData();
            _ = cly.Device.ChangeDeviceIdWithoutMerge("non_merge_id");
            // Extract and validate user_details requests
            TestUtility.ValidateRQEQSize(cly, 1, 0);
            Dictionary<string, object> up3 = TestUtility.ExtractAndDeserializeUserDetails(cly.RequestHelper._requestRepo.Models);
            TestUtility.ValidateUserDetails(up3, ExpectedSameData());
            cly.RequestHelper._requestRepo.Clear();

            SendSameData();
            _ = Countly.Instance.Events.RecordEventAsync("BasicEventD");
            // Extract and validate user_details requests
            TestUtility.ValidateRQEQSize(cly, 1, 1);
            Dictionary<string, object> up4 = TestUtility.ExtractAndDeserializeUserDetails(cly.RequestHelper._requestRepo.Models);
            TestUtility.ValidateUserDetails(up4, ExpectedSameData());
            cly.RequestHelper._requestRepo.Clear();
        }

        // DeviceID changes with UserProfile changes
        // We record events and User Profile data with consent requirement. We also change device id with and without merge
        // Since consent is required, events, device id change with merge and events should be recorded correctly and post id change events shouldn't be recorded
        [Test]
        public void UP_208_CR_CG_M()
        {
            Consents[] consent = new Consents[] { Consents.Crashes, Consents.Events, Consents.Clicks, Consents.StarRating, Consents.Views, Consents.Users, Consents.Push, Consents.RemoteConfig, Consents.Location, Consents.Feedback, Consents.Sessions };
            CountlyConfiguration config = TestUtility.CreateBaseConfigConsent(consent);
            config.DisableAutomaticSessionTracking();
            Countly.Instance.Init(config);
            Countly cly = Countly.Instance;
            _ = cly.Session.BeginSessionAsync();
            TestUtility.ValidateRQEQSize(cly, 2, 0);
            cly.RequestHelper._requestRepo.Clear();

            _ = Countly.Instance.Events.RecordEventAsync("BasicEventA");
            _ = Countly.Instance.Events.RecordEventAsync("BasicEventB");
            SendSameData();
            _ = cly.Session.EndSessionAsync();
            // Extract and validate user_details requests
            TestUtility.ValidateRQEQSize(cly, 3, 0);
            Dictionary<string, object> up1 = TestUtility.ExtractAndDeserializeUserDetails(cly.RequestHelper._requestRepo.Models);
            TestUtility.ValidateUserDetails(up1, ExpectedSameData());
            cly.RequestHelper._requestRepo.Clear();

            _ = Countly.Instance.Events.RecordEventAsync("BasicEventC");
            SendUserData();
            _ = cly.Session.EndSessionAsync();
            _ = cly.Device.ChangeDeviceIdWithMerge("merge_id");
            // Extract and validate user_details requests
            TestUtility.ValidateRQEQSize(cly, 3, 0);
            Dictionary<string, object> up2 = TestUtility.ExtractAndDeserializeUserDetails(cly.RequestHelper._requestRepo.Models);
            TestUtility.ValidateUserDetails(up2, ExpectedUserData());
            cly.RequestHelper._requestRepo.Clear();

            SendSameData();
            _ = cly.Device.ChangeDeviceIdWithoutMerge("non_merge_id");
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            SendSameData();
            _ = Countly.Instance.Events.RecordEventAsync("BasicEventD");
            TestUtility.ValidateRQEQSize(cly, 0, 0);
        }

        // DeviceID changes with UserProfile changes
        // We record events and User Profile data with consent requirement. However we don't provide consent. We also change device id with and without merge
        // Since consent is required and not given, nothing except the, session and device id change with merge, should be recorded
        [Test]
        public void UP_209_CR_CNG_M()
        {
            Consents[] consent = new Consents[] { };
            CountlyConfiguration config = TestUtility.CreateBaseConfigConsent(consent);
            config.DisableAutomaticSessionTracking();
            Countly.Instance.Init(config);
            Countly cly = Countly.Instance;
            _ = cly.Session.BeginSessionAsync();
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            _ = Countly.Instance.Events.RecordEventAsync("BasicEventA");
            _ = Countly.Instance.Events.RecordEventAsync("BasicEventB");
            SendSameData();
            _ = cly.Session.EndSessionAsync();
            TestUtility.ValidateRQEQSize(cly, 2, 0);

            _ = Countly.Instance.Events.RecordEventAsync("BasicEventC");
            SendUserData();
            _ = cly.Session.EndSessionAsync();
            _ = cly.Device.ChangeDeviceIdWithMerge("merge_id");
            TestUtility.ValidateRQEQSize(cly, 3, 0);

            SendSameData();
            _ = cly.Device.ChangeDeviceIdWithoutMerge("non_merge_id");
            TestUtility.ValidateRQEQSize(cly, 3, 0);

            SendSameData();
            _ = Countly.Instance.Events.RecordEventAsync("BasicEventD");
            TestUtility.ValidateRQEQSize(cly, 3, 0);
        }

        // Manual session elapse with UserProfile changes
        // We start a manual session with an update timer of 5 seconds, and record User Profile data, wait 6 seconds after that.
        // User Data request should automaticly be created after 6 seconds
        [Test]
        public void UP_210_CNR_M_duration()
        {
            Countly cly = Countly.Instance;
            CountlyConfiguration config = TestUtility.CreateBaseConfig()
                .SetUpdateSessionTimerDelay(5);
            config.DisableAutomaticSessionTracking();
            cly.Init(config);
            _ = cly.Session.BeginSessionAsync();
            TestUtility.ValidateRQEQSize(cly, 1, 0);
            SendUserData();
            Thread.Sleep(6000);
            // Extract and validate user_details requests
            TestUtility.ValidateRQEQSize(cly, 2, 0);
            Dictionary<string, object> up1 = TestUtility.ExtractAndDeserializeUserDetails(cly.RequestHelper._requestRepo.Models);
            TestUtility.ValidateUserDetails(up1, ExpectedUserData());
            cly.RequestHelper._requestRepo.Clear();
        }

        [SetUp]
        [TearDown]
        public void End()
        {
            TestUtility.TestCleanup();
        }
    }
}