using NUnit.Framework;
using UnityEngine;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Enums;

namespace Tests
{
    public class ConsentTests
    {
        private readonly string _serverUrl = "https://xyz.com/";
        private readonly string _appKey = "772c091355076ead703f987fee94490";

        [Test]
        public void TestInitConsents()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                RequiresConsent = true,
                EnableTestMode = true,
            };

            string groupA = "GroupA";
            string groupB = "GroupB";
            string groupC = "GroupC";

            configuration.RequiresConsent = true;
            configuration.GiveConsent(new Consents[] { Consents.Crashes, Consents.Events });

            configuration.CreateConsentGroup(groupA, new Consents[] { Consents.Sessions, Consents.Location });
            configuration.CreateConsentGroup(groupB, new Consents[] { Consents.RemoteConfig, Consents.Users });
            configuration.CreateConsentGroup(groupC, new Consents[] { Consents.StarRating });

            configuration.GiveConsentToGroup(groupA);
            configuration.GiveConsentToGroup(groupC);

            Countly.Instance.Init(configuration);

            Assert.AreNotEqual(Countly.Instance.Consents, null);

            Assert.AreEqual(Countly.Instance.Consents.CheckConsent(Consents.Events), true);
            Assert.AreEqual(Countly.Instance.Consents.CheckConsent(Consents.Crashes), true);
            Assert.AreEqual(Countly.Instance.Consents.CheckConsent(Consents.Sessions), true);
            Assert.AreEqual(Countly.Instance.Consents.CheckConsent(Consents.Location), true);

            Assert.AreEqual(Countly.Instance.Consents.CheckConsent(Consents.Users), false);
            Assert.AreEqual(Countly.Instance.Consents.CheckConsent(Consents.RemoteConfig), false);

            Assert.AreEqual(Countly.Instance.Consents.CheckConsent(Consents.StarRating), true);
        }

        [Test]
        public void TestIndividualConsents()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                RequiresConsent = true,
                EnableTestMode = true,
            };


            Countly.Instance.Init(configuration);

            Assert.AreNotEqual(Countly.Instance.Consents, null);

            Countly.Instance.Consents.GiveConsent(new Consents[] { Consents.Events, Consents.Crashes });

            Assert.AreEqual(Countly.Instance.Consents.CheckConsent(Consents.Events), true);
            Assert.AreEqual(Countly.Instance.Consents.CheckConsent(Consents.Crashes), true);

            Countly.Instance.Consents.RemoveConsent(new Consents[] { Consents.Events });
            Countly.Instance.Consents.GiveConsent(new Consents[] { Consents.StarRating });

            Assert.AreEqual(Countly.Instance.Consents.CheckConsent(Consents.Events), false);
            Assert.AreEqual(Countly.Instance.Consents.CheckConsent(Consents.Crashes), true);
            Assert.AreEqual(Countly.Instance.Consents.CheckConsent(Consents.StarRating), true);
        }

        [Test]
        public void TestGroupConsents()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                RequiresConsent = true,
                EnableTestMode = true,
            };

            string groupA = "GroupA";
            string groupB = "GroupB";

            configuration.RequiresConsent = true;
            configuration.GiveConsent(new Consents[] { Consents.Crashes, Consents.Events });

            configuration.CreateConsentGroup(groupA, new Consents[] { Consents.Sessions, Consents.Location });
            configuration.CreateConsentGroup(groupB, new Consents[] { Consents.RemoteConfig, Consents.Users });

            configuration.GiveConsentToGroup(groupA);

            Countly.Instance.Init(configuration);

            Assert.AreNotEqual(Countly.Instance.Consents, null);

            Assert.AreEqual(Countly.Instance.Consents.CheckConsent(Consents.Events), true);
            Assert.AreEqual(Countly.Instance.Consents.CheckConsent(Consents.Crashes), true);
            Assert.AreEqual(Countly.Instance.Consents.CheckConsent(Consents.Location), true);
            Assert.AreEqual(Countly.Instance.Consents.CheckConsent(Consents.Sessions), true);
            Assert.AreEqual(Countly.Instance.Consents.CheckConsent(Consents.Users), false);
            Assert.AreEqual(Countly.Instance.Consents.CheckConsent(Consents.RemoteConfig), false);

            Countly.Instance.Consents.GiveConsentToGroup(new string[] { groupB });
            Countly.Instance.Consents.RemoveConsentOfGroup(new string[] { groupA });

            Assert.AreEqual(Countly.Instance.Consents.CheckConsent(Consents.Events), true);
            Assert.AreEqual(Countly.Instance.Consents.CheckConsent(Consents.Crashes), true);
            Assert.AreEqual(Countly.Instance.Consents.CheckConsent(Consents.Location), false);
            Assert.AreEqual(Countly.Instance.Consents.CheckConsent(Consents.Sessions), false);
            Assert.AreEqual(Countly.Instance.Consents.CheckConsent(Consents.Users), true);
            Assert.AreEqual(Countly.Instance.Consents.CheckConsent(Consents.RemoteConfig), true);
        }

        [Test]
        public void TestConsentsListeners()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                RequiresConsent = true,
                EnableTestMode = true,
            };

            string city = "Houston";
            string countryCode = "us";
            string latitude = "29.634933";
            string longitude = "-95.220255";
            string ipAddress = "10.2.33.12";

            configuration.SetLocation(countryCode, city, latitude + "," + longitude, ipAddress);

            configuration.GiveConsent(new Consents[] { Consents.Location, Consents.RemoteConfig });

            Countly.Instance.Init(configuration);

            Assert.AreNotEqual(Countly.Instance.Location, null);

            Assert.AreEqual(Countly.Instance.Location.City, "Houston");
            Assert.AreEqual(Countly.Instance.Location.CountryCode, "us");
            Assert.AreEqual(Countly.Instance.Location.IPAddress, "10.2.33.12");
            Assert.AreEqual(Countly.Instance.Location.IsLocationDisabled, false);
            Assert.AreEqual(Countly.Instance.Location.Location, "29.634933,-95.220255");

            Assert.AreNotEqual(Countly.Instance.Consents, null);
            Assert.AreEqual(Countly.Instance.Consents.CheckConsent(Consents.Location), true);

            Countly.Instance.Consents.RemoveConsent(new Consents[] { Consents.Location });

            Assert.AreEqual(Countly.Instance.Consents.CheckConsent(Consents.Location), false);
            Assert.AreEqual(Countly.Instance.Location.City, null);
            Assert.AreEqual(Countly.Instance.Location.Location, null);
            Assert.AreEqual(Countly.Instance.Location.IPAddress, null);
            Assert.AreEqual(Countly.Instance.Location.CountryCode, null);
            Assert.AreEqual(Countly.Instance.Location.IsLocationDisabled, false);

            Countly.Instance.Consents.RemoveConsent(new Consents[] { Consents.RemoteConfig });
            Assert.AreEqual(Countly.Instance.RemoteConfigs.Configs, null);
        }

        [TearDown]
        public void End()
        {
            Countly.Instance.ClearStorage();
            Object.DestroyImmediate(Countly.Instance);
        }
    }
}
