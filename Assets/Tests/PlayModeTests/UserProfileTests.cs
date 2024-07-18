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
                { "special_value", "something special" },
                { "not_special_value", "something special cooking" }
            };

            return expectedUserDetails;
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

        [SetUp]
        [TearDown]
        public void End()
        {
            TestUtility.TestCleanup();
        }
    }
}