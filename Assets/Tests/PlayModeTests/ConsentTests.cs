using NUnit.Framework;
using UnityEngine;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Services;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    public class ConsentTests
    {
        private readonly string _serverUrl = "https://xyz.com/";
        private readonly string _appKey = "772c091355076ead703f987fee94490";


        public void AssertConsentArray(Consents[] consents, bool expectedValue)
        {
            foreach (Consents consent in consents) {
                Assert.AreEqual(Countly.Instance.Consents.CheckConsent(consent), expectedValue);
            }
        }

        public void AssertConsentAll(bool expectedValue)
        {
            Consents[] consents = System.Enum.GetValues(typeof(Consents)).Cast<Consents>().ToArray();
            AssertConsentArray(consents, expectedValue);
        }

        [Test]
        public void TestDefaultStateOfConsents()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                EnableTestMode = true,
            };

            Countly.Instance.Init(configuration);

            Assert.AreNotEqual(Countly.Instance.Consents, null);
            AssertConsentAll(expectedValue: true);
        }

        [Test]
        public void TestConsentDefaultValuesWithRequiresConsentTrue()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                EnableTestMode = true,
                RequiresConsent = true
            };

            Countly.Instance.Init(configuration);

            Assert.AreNotEqual(Countly.Instance.Consents, null);

            AssertConsentAll(expectedValue: false);
        }

        [Test]
        public void TestConsentDefaultValuesWithRequiresConsentFalse()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                EnableTestMode = true,
            };

            string groupA = "GroupA";
            configuration.GiveConsent(new Consents[] { Consents.Crashes, Consents.Events });

            configuration.CreateConsentGroup(groupA, new Consents[] { Consents.Sessions, Consents.Location });

            configuration.GiveConsentToGroup(groupA);

            Countly.Instance.Init(configuration);

            Assert.AreNotEqual(Countly.Instance.Consents, null);

            AssertConsentAll(expectedValue: true);

            Countly.Instance.Consents.RemoveConsent(new Consents[] { Consents.Crashes, Consents.Location });

            AssertConsentAll(expectedValue: true);

            Countly.Instance.Consents.RemoveConsentOfGroup(new string[] { groupA });

            AssertConsentAll(expectedValue: true);

            Countly.Instance.Consents.RemoveAllConsent();

            AssertConsentAll(expectedValue: true);

        }

        [Test]
        public void TestConsentsGivenDuringInit()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                EnableTestMode = true,
                RequiresConsent = true
            };

            string groupA = "GroupA";
            configuration.GiveConsent(new Consents[] { Consents.Crashes, Consents.Events });

            configuration.CreateConsentGroup(groupA, new Consents[] { Consents.Sessions, Consents.Location });

            configuration.GiveConsentToGroup(groupA);

            Countly.Instance.Init(configuration);

            Assert.AreNotEqual(Countly.Instance.Consents, null);

            AssertConsentArray(new Consents[] { Consents.Events, Consents.Crashes, Consents.Sessions, Consents.Location }, true);
            AssertConsentArray(new Consents[] { Consents.Views, Consents.Users, Consents.Clicks, Consents.StarRating, Consents.RemoteConfig}, false);

        }

        [Test]
        public void TestGiveAndRemoveAllConsent()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                EnableTestMode = true,
                RequiresConsent = true
            };


            Countly.Instance.Init(configuration);

            Assert.AreNotEqual(Countly.Instance.Consents, null);
            AssertConsentAll(expectedValue: false);


            Countly.Instance.Consents.GiveConsentAll();
            AssertConsentAll(expectedValue: true);


            Countly.Instance.Consents.RemoveAllConsent();
            AssertConsentAll(expectedValue: false);


        }

        [Test]
        public void TestConfigGiveConsents()
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
            configuration.CreateConsentGroup(groupB, new Consents[] { Consents.RemoteConfig, Consents.Users, Consents.Location });
            configuration.CreateConsentGroup(groupC, new Consents[] { Consents.StarRating });

            configuration.GiveConsentToGroup(groupA);
            configuration.GiveConsentToGroup(groupC);

            Countly.Instance.Init(configuration);

            Assert.AreNotEqual(Countly.Instance.Consents, null);

            AssertConsentArray(new Consents[] { Consents.Events, Consents.Crashes, Consents.Sessions, Consents.Location, Consents.StarRating }, true);
            AssertConsentArray(new Consents[] { Consents.Views, Consents.Users, Consents.Clicks, Consents.RemoteConfig }, false);

        }

        [Test]
        public void TestGiveIndividualConsents()
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
            AssertConsentArray(new Consents[] { Consents.Events, Consents.Crashes}, true);
            AssertConsentArray(new Consents[] { Consents.Views, Consents.Users, Consents.Clicks, Consents.RemoteConfig, Consents.Sessions, Consents.Location, Consents.StarRating }, false);

            Countly.Instance.Consents.GiveConsent(new Consents[] { Consents.StarRating });
            AssertConsentArray(new Consents[] { Consents.Events, Consents.Crashes, Consents.StarRating }, true);
            AssertConsentArray(new Consents[] { Consents.Views, Consents.Users, Consents.Clicks, Consents.RemoteConfig, Consents.Sessions, Consents.Location }, false);

        }

        [Test]
        public void TestRemovalIndividualConsents()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                RequiresConsent = true,
                EnableTestMode = true,
            };

            configuration.GiveConsent(new Consents[] { Consents.Crashes, Consents.Views, Consents.StarRating, Consents.Events, Consents.Users });
            Countly.Instance.Init(configuration);

            AssertConsentArray(new Consents[] { Consents.Events, Consents.Crashes, Consents.StarRating, Consents.Views, Consents.Users }, true);
            AssertConsentArray(new Consents[] { Consents.Clicks, Consents.RemoteConfig, Consents.Sessions, Consents.Location }, false);

            Countly.Instance.Consents.RemoveConsent(new Consents[] { Consents.Views, Consents.Users });
            AssertConsentArray(new Consents[] { Consents.Events, Consents.Crashes, Consents.StarRating }, true);
            AssertConsentArray(new Consents[] { Consents.Clicks, Consents.RemoteConfig, Consents.Sessions, Consents.Location, Consents.Views, Consents.Users }, false);
        }

        [Test]
        public void TestGiveConsentToGroup()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                RequiresConsent = true,
                EnableTestMode = true,
            };

            string groupA = "GroupA";
            string groupB = "GroupB";

            configuration.CreateConsentGroup(groupA, new Consents[] { Consents.Sessions, Consents.Location });
            configuration.CreateConsentGroup(groupB, new Consents[] { Consents.RemoteConfig, Consents.Users });

            configuration.GiveConsentToGroup(groupA);

            Countly.Instance.Init(configuration);

            Assert.AreNotEqual(Countly.Instance.Consents, null);
            AssertConsentArray(new Consents[] { Consents.Sessions, Consents.Location }, true);
            AssertConsentArray(new Consents[] { Consents.Views, Consents.Users, Consents.Clicks, Consents.RemoteConfig, Consents.Events, Consents.Crashes, Consents.StarRating }, false);


            Countly.Instance.Consents.GiveConsentToGroup(new string[] { groupB });
            AssertConsentArray(new Consents[] { Consents.Sessions, Consents.Location, Consents.Users, Consents.RemoteConfig, }, true);
            AssertConsentArray(new Consents[] { Consents.Views, Consents.Clicks, Consents.Events, Consents.Crashes, Consents.StarRating }, false);
        }

        [Test]
        public void TestRemoveConsentOfGroup()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                RequiresConsent = true,
                EnableTestMode = true,
            };

            string groupA = "GroupA";
            string groupB = "GroupB";

            configuration.CreateConsentGroup(groupA, new Consents[] { Consents.Clicks, Consents.Views });
            configuration.CreateConsentGroup(groupB, new Consents[] { Consents.RemoteConfig, Consents.Users });


            Countly.Instance.Init(configuration);

            Assert.AreNotEqual(Countly.Instance.Consents, null);

            Countly.Instance.Consents.GiveConsentAll();
            AssertConsentAll(expectedValue: true);

            Countly.Instance.Consents.RemoveConsentOfGroup(new string[] { groupA });
            AssertConsentArray(new Consents[] { Consents.Sessions, Consents.Location, Consents.Users, Consents.RemoteConfig, Consents.Events, Consents.Crashes, Consents.StarRating }, true);
            AssertConsentArray(new Consents[] { Consents.Views, Consents.Clicks }, false);


            Countly.Instance.Consents.RemoveConsentOfGroup(new string[] { groupB });
            AssertConsentArray(new Consents[] { Consents.Sessions, Consents.Location, Consents.Events, Consents.Crashes, Consents.StarRating }, true);
            AssertConsentArray(new Consents[] { Consents.Views, Consents.Clicks, Consents.Users, Consents.RemoteConfig }, false);
        }

        [Test]
        public void TestLocationConsentChangedListener()
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
        }

        [Test]
        public void TestListenerOnMultipleConsentOfSameFeature()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                RequiresConsent = true,
                EnableTestMode = true,
            };


            ConsentTestHelperClass eventListener = new ConsentTestHelperClass(Consents.Events);
            ConsentTestHelperClass locatoinListener = new ConsentTestHelperClass(Consents.Location);
            ConsentTestHelperClass starRatingListener = new ConsentTestHelperClass(Consents.StarRating);
            ConsentTestHelperClass remoteConfigListener = new ConsentTestHelperClass(Consents.RemoteConfig);

            List<AbstractBaseService> listeners = new List<AbstractBaseService> { eventListener, locatoinListener, remoteConfigListener, starRatingListener };

            ConsentCountlyService consentCountlyService = new ConsentCountlyService(configuration, null);
            consentCountlyService.Listeners = listeners;

            consentCountlyService.GiveConsent(new Consents[] { Consents.Location, Consents.RemoteConfig, Consents.RemoteConfig, Consents.Events });

            Assert.AreEqual(eventListener.Count, 1);
            Assert.AreEqual(locatoinListener.Count, 1);
            Assert.AreEqual(starRatingListener.Count, 0);
            Assert.AreEqual(remoteConfigListener.Count, 1);

            consentCountlyService.RemoveConsent(new Consents[] { Consents.Location, Consents.Location, Consents.StarRating, Consents.Events });

            Assert.AreEqual(eventListener.Count, 1);
            Assert.AreEqual(locatoinListener.Count, 1);
            Assert.AreEqual(starRatingListener.Count, 1);
            Assert.AreEqual(remoteConfigListener.Count, 0);
        }

        [Test]
        public void TestListenerOnConsentChanged()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                RequiresConsent = true,
                EnableTestMode = true,
            };

            ConsentTestHelperClass eventListener = new ConsentTestHelperClass(Consents.Events);
            ConsentTestHelperClass locatoinListener = new ConsentTestHelperClass(Consents.Location);
            ConsentTestHelperClass starRatingListener = new ConsentTestHelperClass(Consents.StarRating);
            ConsentTestHelperClass remoteConfigListener = new ConsentTestHelperClass(Consents.RemoteConfig);

            List<AbstractBaseService> listeners = new List<AbstractBaseService> { eventListener, locatoinListener, remoteConfigListener, starRatingListener };

            ConsentCountlyService consentCountlyService = new ConsentCountlyService(configuration, null);
            consentCountlyService.Listeners = listeners;

            consentCountlyService.GiveConsent(new Consents[] { Consents.Location, Consents.RemoteConfig, Consents.Events });
            Assert.AreEqual(eventListener.Count, 1);
            Assert.AreEqual(locatoinListener.Count, 1);
            Assert.AreEqual(starRatingListener.Count, 0);
            Assert.AreEqual(remoteConfigListener.Count, 1);

            consentCountlyService.GiveConsent(new Consents[] { Consents.Location, Consents.StarRating });
            Assert.AreEqual(eventListener.Count, 0);
            Assert.AreEqual(locatoinListener.Count, 0);
            Assert.AreEqual(starRatingListener.Count, 1);
            Assert.AreEqual(remoteConfigListener.Count, 0);

            consentCountlyService.RemoveConsent(new Consents[] { Consents.Location, Consents.StarRating });
            Assert.AreEqual(eventListener.Count, 0);
            Assert.AreEqual(locatoinListener.Count, 1);
            Assert.AreEqual(starRatingListener.Count, 1);
            Assert.AreEqual(remoteConfigListener.Count, 0);

            consentCountlyService.RemoveConsent(new Consents[] { Consents.Events, Consents.StarRating });
            Assert.AreEqual(eventListener.Count, 1);
            Assert.AreEqual(locatoinListener.Count, 0);
            Assert.AreEqual(starRatingListener.Count, 0);
            Assert.AreEqual(remoteConfigListener.Count, 0);
        }

        [Test]
        public void TestListenerOnConsentGroups()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                RequiresConsent = true,
                EnableTestMode = true,
            };

            string groupA = "GroupA";
            string groupB = "GroupB";

            configuration.CreateConsentGroup(groupA, new Consents[] { Consents.Clicks, Consents.Views });

            ConsentTestHelperClass pushListener = new ConsentTestHelperClass(Consents.Push);
            ConsentTestHelperClass UsersListener = new ConsentTestHelperClass(Consents.Users);
            ConsentTestHelperClass viewsListener = new ConsentTestHelperClass(Consents.Views);
            ConsentTestHelperClass ClicksListener = new ConsentTestHelperClass(Consents.Clicks);


            List<AbstractBaseService> listeners = new List<AbstractBaseService> { viewsListener, ClicksListener, UsersListener, pushListener };

            ConsentCountlyService consentCountlyService = new ConsentCountlyService(configuration, null);
            consentCountlyService.Listeners = listeners;

            consentCountlyService.GiveConsentToGroup(new string[] { groupA });
            Assert.AreEqual(pushListener.Count, 0);
            Assert.AreEqual(UsersListener.Count, 0);
            Assert.AreEqual(viewsListener.Count, 1);
            Assert.AreEqual(ClicksListener.Count, 1);

            consentCountlyService.GiveConsentToGroup(new string[] { groupB });
            Assert.AreEqual(pushListener.Count, 0);
            Assert.AreEqual(UsersListener.Count, 0);
            Assert.AreEqual(viewsListener.Count, 1);
            Assert.AreEqual(ClicksListener.Count, 1);

            consentCountlyService.RemoveConsentOfGroup(new string[] { groupA });
            Assert.AreEqual(pushListener.Count, 0);
            Assert.AreEqual(UsersListener.Count, 0);
            Assert.AreEqual(viewsListener.Count, 1);
            Assert.AreEqual(ClicksListener.Count, 1);

            consentCountlyService.RemoveConsentOfGroup(new string[] { groupB });
            Assert.AreEqual(pushListener.Count, 0);
            Assert.AreEqual(UsersListener.Count, 0);
            Assert.AreEqual(viewsListener.Count, 1);
            Assert.AreEqual(ClicksListener.Count, 1);

            consentCountlyService.GiveConsent(new Consents[] { Consents.Push, Consents.Views });
            Assert.AreEqual(pushListener.Count, 1);
            Assert.AreEqual(UsersListener.Count, 0);
            Assert.AreEqual(viewsListener.Count, 1);
            Assert.AreEqual(ClicksListener.Count, 0);

            consentCountlyService.GiveConsentToGroup(new string[] { groupA });
            Assert.AreEqual(pushListener.Count, 0);
            Assert.AreEqual(UsersListener.Count, 0);
            Assert.AreEqual(viewsListener.Count, 0);
            Assert.AreEqual(ClicksListener.Count, 1);

        }

        [Test]
        public void TestListenerOnAllConsentRemovalAndGiven()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                RequiresConsent = true,
                EnableTestMode = true,
            };

            string groupA = "GroupA";

            configuration.CreateConsentGroup(groupA, new Consents[] { Consents.Clicks, Consents.Views });

            ConsentTestHelperClass pushListener = new ConsentTestHelperClass(Consents.Push);
            ConsentTestHelperClass UsersListener = new ConsentTestHelperClass(Consents.Users);
            ConsentTestHelperClass viewsListener = new ConsentTestHelperClass(Consents.Views);
            ConsentTestHelperClass ClicksListener = new ConsentTestHelperClass(Consents.Clicks);


            List<AbstractBaseService> listeners = new List<AbstractBaseService> { viewsListener, ClicksListener, UsersListener, pushListener };

            ConsentCountlyService consentCountlyService = new ConsentCountlyService(configuration, null);
            consentCountlyService.Listeners = listeners;

            consentCountlyService.GiveConsentToGroup(new string[] { groupA });
            Assert.AreEqual(pushListener.Count, 0);
            Assert.AreEqual(UsersListener.Count, 0);
            Assert.AreEqual(viewsListener.Count, 1);
            Assert.AreEqual(ClicksListener.Count, 1);

            consentCountlyService.RemoveAllConsent();
            Assert.AreEqual(pushListener.Count, 0);
            Assert.AreEqual(UsersListener.Count, 0);
            Assert.AreEqual(viewsListener.Count, 1);
            Assert.AreEqual(ClicksListener.Count, 1);

            consentCountlyService.GiveConsentAll();
            Assert.AreEqual(pushListener.Count, 1);
            Assert.AreEqual(UsersListener.Count, 1);
            Assert.AreEqual(viewsListener.Count, 1);
            Assert.AreEqual(ClicksListener.Count, 1);

        }
        [TearDown]
        public void End()
        {
            Countly.Instance.ClearStorage();
            Object.DestroyImmediate(Countly.Instance);

        }

        private class ConsentTestHelperClass : AbstractBaseService
        {
            internal int Count { set; get; }
            internal Consents _consent;
            internal ConsentTestHelperClass(Consents consent) : base(null)
            {
                _consent = consent;
            }

            internal override void ConsentChanged(List<Consents> updatedConsents, bool newConsentValue)
            {
                Count = 0;
                foreach (Consents consent in updatedConsents) {
                    if (consent == _consent) {
                        ++Count;
                    }
                }
            }
        }
    }
}
