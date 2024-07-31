using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Assets.Tests.PlayModeTests.Scenarios;
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
            Countly.Instance.UserProfile.SaveMax("e12345", 100);
            Countly.Instance.UserProfile.SaveMin("f12345", 50);
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
            cly.RequestHelper._requestRepo.Clear();

            SendUserData();
            cly.UserProfile.Save();

            TestUtility.ValidateRQEQSize(cly, 1, 0);
            Dictionary<string, object> up1 = TestUtility.ExtractAndDeserializeUserDetails(cly.RequestHelper._requestRepo.Models);
            foreach (KeyValuePair<string, object> entry in up1) {
                Debug.Log($"Key: {entry.Key}, Value: {entry.Value}");
            }
            TestUtility.ValidateUserDetails(up1, ExpectedUserData());
        }

        // Recording an Event while User Profile Data is set
        // We initialize the sdk, set user profile data and record an event
        // When we record the event it should create an User Profile request
        [Test]
        public void EventWithRecordedValue_A_CNR()
        {
            Countly cly = Countly.Instance;
            cly.Init(TestUtility.CreateBaseConfig());

            cly.UserProfile.SetProperties(ExpectedUserProperty());
            _ = cly.Events.RecordEventAsync("EventA");

            TestUtility.ValidateRQEQSize(cly, 2, 1);
            Dictionary<string, object> up1 = TestUtility.ExtractAndDeserializeUserDetails(cly.RequestHelper._requestRepo.Models);
            TestUtility.ValidateUserDetails(up1, ExpectedUserProperty());
            cly.RequestHelper._requestRepo.Clear();
        }

        // Recording an Event while User Profile Data is set
        // We initialize the sdk, with consent requirement, however not provide it. then set user profile data and record an event
        // Since consent is not provided event or user profile request should not be created
        [Test]
        public void EventWithRecordedValue_A_CR_CNG()
        {
            CountlyConfiguration config = TestUtility.CreateBaseConfig()
                .SetRequiresConsent(true);

            Countly cly = Countly.Instance;
            cly.Init(config);
            // 1 request for session and 1 for consents
            TestUtility.ValidateRQEQSize(cly, 2, 0);
            cly.RequestHelper._requestRepo.Clear();

            cly.UserProfile.SetProperties(ExpectedUserProperty());
            _ = cly.Events.RecordEventAsync("EventA");

            TestUtility.ValidateRQEQSize(cly, 0, 0);
        }

        // Starting a View while User Profile Data is set
        // We initialize the sdk, set user profile data and start a view
        // Starting a view should record a view event and create an user profile request
        [Test]
        public void ViewWithRecordedValue_A_CNR()
        {
            Countly cly = Countly.Instance;
            cly.Init(TestUtility.CreateBaseConfig());

            cly.UserProfile.SetProperties(ExpectedUserProperty());
            cly.Views.StartView("viewA");

            TestUtility.ValidateRQEQSize(cly, 2, 1);
            Dictionary<string, object> up1 = TestUtility.ExtractAndDeserializeUserDetails(cly.RequestHelper._requestRepo.Models);
            TestUtility.ValidateUserDetails(up1, ExpectedUserProperty());
            cly.RequestHelper._requestRepo.Clear();
        }

        // Starting a View while User Profile Data is set
        // We initialize the sdk, with consent requirement, however not provide it. then set user profile data and start a view
        // Since consent is not provided starting a view should not record a view event and create an user profile request
        [Test]
        public void ViewWithRecordedValue_A_CR_CNG()
        {
            CountlyConfiguration config = TestUtility.CreateBaseConfig()
                .SetRequiresConsent(true);

            Countly cly = Countly.Instance;
            cly.Init(config);
            // 1 request for session and 1 for consents
            TestUtility.ValidateRQEQSize(cly, 2, 0);
            cly.RequestHelper._requestRepo.Clear();

            cly.UserProfile.SetProperties(ExpectedUserProperty());
            cly.Views.StartView("viewA");
            TestUtility.ValidateRQEQSize(cly, 0, 0);
        }

        // Changing Device ID with merge while User Profile Data is set
        // We initialize the sdk, record user profile data and change the device id
        // Changing device id should create a request for device id change and user profile data
        [Test]
        public void ChangeDeviceIdWithMergeWithRecordedValue_A_CNR()
        {
            Countly cly = Countly.Instance;
            cly.Init(TestUtility.CreateBaseConfig());

            cly.UserProfile.SetProperties(ExpectedUserProperty());
            _ = cly.Device.ChangeDeviceIdWithMerge("new_deviceid");

            TestUtility.ValidateRQEQSize(cly, 3, 0);
            Dictionary<string, object> up1 = TestUtility.ExtractAndDeserializeUserDetails(cly.RequestHelper._requestRepo.Models);
            TestUtility.ValidateUserDetails(up1, ExpectedUserProperty());
            cly.RequestHelper._requestRepo.Clear();
        }

        // Changing Device ID with merge while User Profile Data is set
        // We initialize the sdk, record user profile data and change the device id
        // Changing device id should create a request for device id change and user profile data
        [Test]
        public void ChangeDeviceIdWithMergeWithRecordedValue_A_CR_CNG()
        {
            CountlyConfiguration config = TestUtility.CreateBaseConfig()
                .SetRequiresConsent(true);

            Countly cly = Countly.Instance;
            cly.Init(config);
            // 1 request for session and 1 for consents
            TestUtility.ValidateRQEQSize(cly, 2, 0);
            cly.RequestHelper._requestRepo.Clear();

            cly.UserProfile.SetProperties(ExpectedUserProperty());
            _ = cly.Device.ChangeDeviceIdWithMerge("new_deviceid");
            TestUtility.ValidateRQEQSize(cly, 1, 0);
            Assert.AreEqual(0, TestUtility.ExtractAndDeserializeUserDetails(cly.RequestHelper._requestRepo.Models).Count);
            cly.RequestHelper._requestRepo.Clear();
        }

        // Changing Device ID without merge while User Profile Data is set
        // We initialize the sdk, record user profile data and change the device id
        // Changing device id itself should not create a request for device id but it should create one for user profile data
        [Test]
        public void ChangeDeviceIdWithoutMergeWithRecordedValue_A_CNR()
        {
            Countly cly = Countly.Instance;
            cly.Init(TestUtility.CreateBaseConfig().EnableLogging());
            TestUtility.ValidateRQEQSize(cly, 1, 0);
            cly.RequestHelper._requestRepo.Clear();

            cly.UserProfile.SetProperties(ExpectedUserProperty());
            _ = cly.Device.ChangeDeviceIdWithoutMerge("new_deviceid");

            // 1 for end session, 1 for start session, 1 for user profile, changing without merge itself should not create a request
            TestUtility.ValidateRQEQSize(cly, 3, 0);
            Dictionary<string, object> up1 = TestUtility.ExtractAndDeserializeUserDetails(cly.RequestHelper._requestRepo.Models);
            TestUtility.ValidateUserDetails(up1, ExpectedUserProperty());
            cly.RequestHelper._requestRepo.Clear();
        }

        // Manual Session Timer with User Profile data is set
        // We initialize the sdk with manual session with a timer delay of 3 seconds, begin session, record user profile data and wait 4 seconds
        // When session timer is elapsed, it should automaticly create an user profile request
        [Test]
        public void SessionTimerElapseWithRecordedValue_M_CNR()
        {
            CountlyConfiguration config = TestUtility.CreateBaseConfig()
                .SetUpdateSessionTimerDelay(3);
            config.DisableAutomaticSessionTracking();
            Countly cly = Countly.Instance;
            cly.Init(config);
            _ = cly.Session.BeginSessionAsync();
            TestUtility.ValidateRQEQSize(cly, 1, 0);
            cly.RequestHelper._requestRepo.Clear();
            cly.UserProfile.SetProperties(ExpectedUserProperty());

            Thread.Sleep(4000);

            TestUtility.ValidateRQEQSize(cly, 1, 0);
            Dictionary<string, object> up1 = TestUtility.ExtractAndDeserializeUserDetails(cly.RequestHelper._requestRepo.Models);
            TestUtility.ValidateUserDetails(up1, ExpectedUserProperty());
            cly.RequestHelper._requestRepo.Clear();
        }

        // Manual Session Timer with User Profile data is set
        // We initialize the sdk with consent requirement, however not provide it.
        // We start manual session with a timer delay of 3 seconds, try to record user profile data and wait 4 seconds
        // After 4 seconds it should create no request since there is no consent provided
        [Test]
        public void SessionTimerElapseWithRecordedValue_M_CR_CNG()
        {
            CountlyConfiguration config = TestUtility.CreateBaseConfig()
                .SetRequiresConsent(true)
                .SetUpdateSessionTimerDelay(3);

            config.DisableAutomaticSessionTracking();
            Countly cly = Countly.Instance;
            cly.Init(config);
            _ = cly.Session.BeginSessionAsync();

            TestUtility.ValidateRQEQSize(cly, 2, 0);
            cly.RequestHelper._requestRepo.Clear();

            cly.UserProfile.SetProperties(ExpectedUserProperty());

            Thread.Sleep(4000);

            TestUtility.ValidateRQEQSize(cly, 0, 0);
            Assert.AreEqual(0, TestUtility.ExtractAndDeserializeUserDetails(cly.RequestHelper._requestRepo.Models).Count);
        }

        // Manual Session Timer with User Profile data is set
        // We initialize the sdk with consent requirement, and provide consent.
        // We start manual session with a timer delay of 3 seconds, record user profile data and wait 4 seconds
        // When session timer is elapsed, it should automaticly create an user profile request
        [Test]
        public void SessionTimerElapseWithRecordedValue_M_CR_CG()
        {
            Consents[] consent = new Consents[] { Consents.Crashes, Consents.Events, Consents.Clicks, Consents.StarRating, Consents.Views, Consents.Users, Consents.Push, Consents.RemoteConfig, Consents.Location, Consents.Feedback, Consents.Sessions };
            CountlyConfiguration config = TestUtility.CreateBaseConfigConsent(consent)
                .SetUpdateSessionTimerDelay(3);
            config.DisableAutomaticSessionTracking();
            Countly cly = Countly.Instance;
            cly.Init(config);
            _ = cly.Session.BeginSessionAsync();

            TestUtility.ValidateRQEQSize(cly, 2, 0);
            cly.RequestHelper._requestRepo.Clear();

            cly.UserProfile.SetProperties(ExpectedUserProperty());

            Thread.Sleep(4000);

            TestUtility.ValidateRQEQSize(cly, 1, 0);
            Dictionary<string, object> up1 = TestUtility.ExtractAndDeserializeUserDetails(cly.RequestHelper._requestRepo.Models);
            TestUtility.ValidateUserDetails(up1, ExpectedUserProperty());
            cly.RequestHelper._requestRepo.Clear();
        }

        // Initializes Countly and returns the Countly instance with clear repository
        private Countly BaseInit()
        {
            Countly cly = Countly.Instance;
            CountlyConfiguration config = TestUtility.CreateBaseConfig();
            cly.Init(config);

            // session request
            TestUtility.ValidateRQEQSize(cly, 1, 0);
            cly.RequestHelper._requestRepo.Clear();

            return cly;
        }

        // 'Increment' in Countly.Instance.UserProfile
        // We initialize the sdk with no consent requirement, try to record user profile data with 'Increment' and call save
        // Since we call 'Increment' with bad keys, it should not record any user profile data
        [Test]
        public void Increment_BadKey_CNR()
        {
            Countly cly = BaseInit();

            cly.UserProfile.Increment(null);
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly.UserProfile.Increment(" ");
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly.UserProfile.Increment("");
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);
        }

        // 'IncrementBy' in Countly.Instance.UserProfile
        // We initialize the sdk with no consent requirement, try to record user profile data with 'IncrementBy' and call save
        // Since we call 'IncrementBy' with bad keys, it should not record any user profile data
        [Test]
        public void IncrementBy_BadKey_CNR()
        {
            Countly cly = BaseInit();

            cly.UserProfile.IncrementBy(null, 10);
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly.UserProfile.IncrementBy(" ", 10);
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly.UserProfile.IncrementBy("", 10);
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);
        }

        // 'Max' in Countly.Instance.UserProfile
        // We initialize the sdk with no consent requirement, try to record user profile data with 'Max' and call save
        // Since we call 'Max' with bad keys, it should not record any user profile data
        [Test]
        public void Max_BadKey_CNR()
        {
            Countly cly = BaseInit();

            cly.UserProfile.SaveMax(null, 10);
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly.UserProfile.SaveMax(" ", 10);
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly.UserProfile.SaveMax("", 10);
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);
        }

        // 'Min' in Countly.Instance.UserProfile
        // We initialize the sdk with no consent requirement, try to record user profile data with 'Min' and call save
        // Since we call 'Min' with bad keys, it should not record any user profile data
        [Test]
        public void Min_BadKey_CNR()
        {
            Countly cly = BaseInit();

            cly.UserProfile.SaveMin(null, 10);
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly.UserProfile.SaveMin(" ", 10);
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly.UserProfile.SaveMin("", 10);
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);
        }

        // 'Multiply' in Countly.Instance.UserProfile
        // We initialize the sdk with no consent requirement, try to record user profile data with 'Multiply' and call save
        // Since we call 'Multiply' with bad keys, it should not record any user profile data
        [Test]
        public void Multiply_BadKey_CNR()
        {
            Countly cly = BaseInit();

            cly.UserProfile.Multiply(null, 10);
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly.UserProfile.Multiply(" ", 10);
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly.UserProfile.Multiply("", 10);
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);
        }

        // 'Pull' in Countly.Instance.UserProfile
        // We initialize the sdk with no consent requirement, try to record user profile data with 'Pull' and call save
        // Since we call 'Pull' with bad keys and values, it should not record any user profile data
        [Test]
        public void Pull_BadKeyAndValue_CNR()
        {
            Countly cly = BaseInit();

            cly.UserProfile.Pull(null, "value1");
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly.UserProfile.Pull(" ", "value2");
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly.UserProfile.Pull("", "value2");
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly.UserProfile.Pull("Key1", null);
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly.UserProfile.Pull("Key2", " ");
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 1, 0);
        }

        // 'Push' in Countly.Instance.UserProfile
        // We initialize the sdk with no consent requirement, try to record user profile data with 'Push' and call save
        // Since we call 'Push' with bad keys and values, it should not record any user profile data
        [Test]
        public void Push_BadKeyAndValue_CNR()
        {
            Countly cly = BaseInit();

            cly.UserProfile.Push(null, "value1");
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly.UserProfile.Push(" ", "value2");
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly.UserProfile.Push("", "value2");
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly.UserProfile.Push("Key1", null);
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly.UserProfile.Push("Key2", " ");
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 1, 0);
        }

        // 'PushUnique' in Countly.Instance.UserProfile
        // We initialize the sdk with no consent requirement, try to record user profile data with 'PushUnique' and call save
        // Since we call 'PushUnique' with bad keys and values, it should not record any user profile data
        [Test]
        public void PushUnique_BadKeyAndValue_CNR()
        {
            Countly cly = BaseInit();

            cly.UserProfile.PushUnique(null, "value1");
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly.UserProfile.PushUnique(" ", "value2");
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly.UserProfile.PushUnique("", "value2");
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly.UserProfile.PushUnique("Key1", null);
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly.UserProfile.PushUnique("Key2", " ");
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 1, 0);
        }

        // 'SetOnce' in Countly.Instance.UserProfile
        // We initialize the sdk with no consent requirement, try to record user profile data with 'SetOnce' and call save
        // Since we call 'SetOnce' with bad keys and values, it should not record any user profile data
        [Test]
        public void SetOnce_BadKeyAndValue_CNR()
        {
            Countly cly = BaseInit();

            cly.UserProfile.SetOnce(null, "value1");
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly.UserProfile.SetOnce(" ", "value2");
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly.UserProfile.SetOnce("", "value2");
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly.UserProfile.SetOnce("Key1", null);
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly.UserProfile.SetOnce("Key2", " ");
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 1, 0);
        }

        // 'SetProperty' in Countly.Instance.UserProfile
        // We initialize the sdk with no consent requirement, try to record user profile data with 'SetProperty' and call save
        // Since we call 'SetProperty' with bad keys and values, it should not record any user profile data
        [Test]
        public void SetProperty_BadKeyAndValue_CNR()
        {
            Countly cly = BaseInit();

            cly.UserProfile.SetProperty(null, "value1");
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly.UserProfile.SetProperty("", "value2");
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly.UserProfile.SetProperty("Key1", null);
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);
        }

        // 'SetProperties' in Countly.Instance.UserProfile
        // We initialize the sdk with no consent requirement, try to record user profile data with 'SetProperties' and call save
        // Since we call 'SetProperties' with bad values, it should not record any user profile data
        [Test]
        public void SetProperties_BadKeyAndValue_CNR()
        {
            Countly cly = BaseInit();

            cly.UserProfile.SetProperties(null);
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly.UserProfile.SetProperties(new Dictionary<string, object>());
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);
        }

        // 'SetData' in Countly.Instance.UserProfile
        // We initialize the sdk with no consent requirement, try to record user profile data with 'SetData' and call save
        // Since we call 'SetData' with bad values, it should not record any user profile data
        [Test]
        public void SetData_BadKeyAndValue_CNR()
        {
            Countly cly = BaseInit();

            Dictionary<string, object> badData = new Dictionary<string, object>
            {
                { "name", null },
                { "username", null },
                { "email", null },
                { "organization", null },
                { "phone", null },
                { "picture", null },
                { "gender", null },
                { "byear", 0 }
            };

            cly.UserProfile.SetProperties(null);
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly.UserProfile.SetProperties(new Dictionary<string, object>());
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);

            cly.UserProfile.SetProperties(badData);
            cly.UserProfile.Save();
            TestUtility.ValidateRQEQSize(cly, 0, 0);
        }

        [SetUp]
        [TearDown]
        public void End()
        {
            TestUtility.TestCleanup();
        }
    }
}