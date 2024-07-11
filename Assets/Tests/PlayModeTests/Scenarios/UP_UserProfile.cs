using System.Collections.Generic;
using NUnit.Framework;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Enums;

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
                { "picturePath", "" },
                { "gender", "M" },
                { "byear", 1919 },
                { "special_value", "something special" },
                { "not_special_value", "something special cooking" }
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

        private void SendSameData()
        {
            Countly.Instance.UserProfile.SetProperty("a12345", "1");
            Countly.Instance.UserProfile.SetProperty("a12345", "2");
            Countly.Instance.UserProfile.SetProperty("a12345", "3");
            Countly.Instance.UserProfile.SetProperty("a12345", "4");
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

        [Test]
        public void UP_203_CNR_A_Events()
        {
            CountlyConfiguration config = TestUtility.CreateBaseConfig()
                .SetRequiresConsent(false)
                .EnableLogging();
            Countly.Instance.Init(config);
            TestUtility.ValidateRQEQSize(Countly.Instance, 1, 0);

            _ = Countly.Instance.Events.RecordEventAsync("BasicEventA");
            _ = Countly.Instance.Events.RecordEventAsync("BasicEventB");
            SendSameData();
            _ = Countly.Instance.Events.RecordEventAsync("BasicEventC");
            SendSameData();
            _ = Countly.Instance.Events.RecordEventAsync("BasicEventD");
            SendSameData();
            _ = Countly.Instance.Events.RecordEventAsync("BasicEventE");

            TestUtility.ValidateRQEQSize(Countly.Instance, 7, 1);
        }

        [SetUp]
        [TearDown]
        public void End()
        {
            TestUtility.TestCleanup();
        }
    }
}