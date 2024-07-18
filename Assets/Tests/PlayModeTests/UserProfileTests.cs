using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Models;
using UnityEngine;

namespace Assets.Tests.PlayModeTests
{
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

        [Test]
        public void ConfigTimeUserProfile()
        {
            CountlyConfiguration config = TestUtility.CreateBaseConfig()
                .SetUserProperties(ExpectedUserProperty());

            Countly cly = Countly.Instance;
            cly.Init(config);

            // Extract and validate user_details requests
            TestUtility.ValidateRQEQSize(cly, 2, 0);
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