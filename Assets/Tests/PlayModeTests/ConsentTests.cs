using NUnit.Framework;
using UnityEngine;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Services;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Tests
{
    public class ConsentTests
    {
        private readonly string _serverUrl = "https://xyz.com/";
        private readonly string _appKey = "772c091355076ead703f987fee94490";


        /// <summary>
        /// Assert an array of consent against the expected value.
        /// </summary>
        /// <param name="expectedValue"> an expected values of consents</param>
        /// <param name="consents"> an array consents</param>
        public void AssertConsentArray(Consents[] consents, bool expectedValue)
        {
            foreach (Consents consent in consents) {
                Assert.AreEqual(expectedValue, Countly.Instance.Consents.CheckConsentInternal(consent));
            }
        }

        /// <summary>
        /// Assert all consents against the expected value.
        /// </summary>
        /// <param name="expectedValue">an expected values of consents</param>
        public void AssertConsentAll(bool expectedValue)
        {
            Consents[] consents = System.Enum.GetValues(typeof(Consents)).Cast<Consents>().ToArray();
            AssertConsentArray(consents, expectedValue);
        }

        /// <summary>
        /// Case: if 'RequiresConsent' isn't set in the configuration during initialization.
        /// Result: All features should work.
        /// </summary>
        [Test]
        public void TestDefaultStateOfConsents()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
            };

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Consents);
            AssertConsentAll(expectedValue: true);
        }

        /// <summary>
        /// Case: if 'RequiresConsent' isn't set in the configuration during initialization.
        /// Result: Consent request should not send.
        /// </summary>
        [Test]
        public void TestConsentsRequest_RequiresConsent_IsFalse()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
            };

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Consents);
            Assert.AreEqual(1, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            CountlyRequestModel requestModel = Countly.Instance.Consents._requestCountlyHelper._requestRepo.Dequeue();
            string myUri = requestModel.RequestUrl;
            string consents = HttpUtility.ParseQueryString(myUri).Get("consent");
            Assert.IsNull(consents);

            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();
            Countly.Instance.Consents.GiveConsent(new Consents[] { Consents.Sessions });
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();
            Countly.Instance.Consents.RemoveConsent(new Consents[] { Consents.Sessions });
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

        }

        /// <summary>
        /// It validates the initial consent request that generates after SDK initialization
        /// </summary>
        [Test]
        public void TestConsentRequest()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                RequiresConsent = true,
            };

            configuration.GiveConsent(new Consents[] { Consents.Crashes, Consents.Events, Consents.Clicks, Consents.StarRating, Consents.Views, Consents.Users, Consents.Push, Consents.RemoteConfig, Consents.Location });
            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Consents);
            Assert.AreEqual(1, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            CountlyRequestModel requestModel = Countly.Instance.Consents._requestCountlyHelper._requestRepo.Dequeue();
            string myUri = requestModel.RequestUrl;
            string consents = HttpUtility.ParseQueryString(myUri).Get("consent");
            JObject json = JObject.Parse(consents);
            Assert.AreEqual(9, json.Count);
            Assert.IsTrue(json.GetValue("push").ToObject<bool>());
            Assert.IsTrue(json.GetValue("users").ToObject<bool>());
            Assert.IsTrue(json.GetValue("views").ToObject<bool>());
            Assert.IsTrue(json.GetValue("clicks").ToObject<bool>());
            Assert.IsTrue(json.GetValue("events").ToObject<bool>());
            Assert.IsTrue(json.GetValue("crashes").ToObject<bool>());
            Assert.IsTrue(json.GetValue("location").ToObject<bool>());
            Assert.IsTrue(json.GetValue("star-rating").ToObject<bool>());
            Assert.IsTrue(json.GetValue("remote-config").ToObject<bool>());

            Countly.Instance.Consents.GiveConsent(new Consents[] { Consents.Crashes, Consents.Events });
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            // RQ will have Consent change request and Session begin request
            Countly.Instance.Consents.GiveConsent(new Consents[] { Consents.Sessions });
            Assert.AreEqual(2, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            requestModel = Countly.Instance.Consents._requestCountlyHelper._requestRepo.Dequeue();
            myUri = requestModel.RequestUrl;
            consents = HttpUtility.ParseQueryString(myUri).Get("consent");
            json = JObject.Parse(consents);
            Assert.AreEqual(1, json.Count);
            Assert.IsTrue(json.GetValue("sessions").ToObject<bool>());

            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();
            Countly.Instance.Consents.RemoveConsent(new Consents[] { Consents.Crashes, Consents.Views });
            requestModel = Countly.Instance.Consents._requestCountlyHelper._requestRepo.Dequeue();
            myUri = requestModel.RequestUrl;
            consents = HttpUtility.ParseQueryString(myUri).Get("consent");
            json = JObject.Parse(consents);
            Assert.AreEqual(2, json.Count);
            Assert.IsFalse(json.GetValue("crashes").ToObject<bool>());
            Assert.IsFalse(json.GetValue("views").ToObject<bool>());

            Countly.Instance.Consents.RemoveConsent(new Consents[] { Consents.Crashes });
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

        }

        /// <summary>
        /// It validates the consent request when consent of a specific feature is given/removed multiple times.
        /// </summary>
        [Test]
        public void TestConsentRequest_WithConsentIsGivenorRemovedMultipleTimes()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                RequiresConsent = true,
            };

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Consents);

            Countly.Instance.ClearStorage();
            Countly.Instance.Consents.GiveConsent(new Consents[] { Consents.Crashes, Consents.Events });
            Assert.AreEqual(1, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            CountlyRequestModel requestModel = Countly.Instance.Consents._requestCountlyHelper._requestRepo.Dequeue();
            string myUri = requestModel.RequestUrl;
            string consents = HttpUtility.ParseQueryString(myUri).Get("consent");
            JObject json = JObject.Parse(consents);
            Assert.AreEqual(2, json.Count);
            Assert.IsTrue(json.GetValue("crashes").ToObject<bool>());
            Assert.IsTrue(json.GetValue("events").ToObject<bool>());

            Countly.Instance.Consents.GiveConsent(new Consents[] { Consents.Crashes, Consents.Views });
            Assert.AreEqual(1, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);
            requestModel = Countly.Instance.Consents._requestCountlyHelper._requestRepo.Dequeue();
            myUri = requestModel.RequestUrl;
            consents = HttpUtility.ParseQueryString(myUri).Get("consent");
            json = JObject.Parse(consents);
            Assert.AreEqual(1, json.Count);
            Assert.IsTrue(json.GetValue("views").ToObject<bool>());

            Countly.Instance.Consents.GiveConsent(new Consents[] { Consents.Views });
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            Countly.Instance.Consents.RemoveConsent(new Consents[] { Consents.Crashes, Consents.Views });
            requestModel = Countly.Instance.Consents._requestCountlyHelper._requestRepo.Dequeue();
            myUri = requestModel.RequestUrl;
            consents = HttpUtility.ParseQueryString(myUri).Get("consent");
            json = JObject.Parse(consents);
            Assert.AreEqual(2, json.Count);
            Assert.IsFalse(json.GetValue("crashes").ToObject<bool>());
            Assert.IsFalse(json.GetValue("views").ToObject<bool>());

            Countly.Instance.Consents.RemoveConsent(new Consents[] { Consents.Events, Consents.Views });
            requestModel = Countly.Instance.Consents._requestCountlyHelper._requestRepo.Dequeue();
            myUri = requestModel.RequestUrl;
            consents = HttpUtility.ParseQueryString(myUri).Get("consent");
            json = JObject.Parse(consents);
            Assert.AreEqual(1, json.Count);
            Assert.IsFalse(json.GetValue("events").ToObject<bool>());

            Countly.Instance.Consents.RemoveConsent(new Consents[] { Consents.Crashes });
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

        }

        /// <summary>
        /// Case: if 'RequiresConsent' is set in the configuration and no consent is given during initialization.
        /// Result: All features shouldn't work.
        /// </summary>
        [Test]
        public void TestConsentDefaultValuesWithRequiresConsentTrue()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                RequiresConsent = true
            };

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Consents);

            AssertConsentAll(expectedValue: false);
        }

        /// <summary>
        /// Case: If 'RequiresConsent' isn't set in the configuration and consents change before and after initialization.
        /// Result: All features should work.
        /// </summary>
        [Test]
        public void TestConsentDefaultValuesWithRequiresConsentFalse()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
            };

            string groupA = "GroupA";
            configuration.GiveConsent(new Consents[] { Consents.Crashes, Consents.Events });

            configuration.CreateConsentGroup(groupA, new Consents[] { Consents.Sessions, Consents.Location });

            configuration.GiveConsentToGroup(new string[] { groupA });

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Consents);

            AssertConsentAll(expectedValue: true);

            Countly.Instance.Consents.RemoveConsent(new Consents[] { Consents.Crashes, Consents.Location });

            AssertConsentAll(expectedValue: true);

            Countly.Instance.Consents.RemoveConsentOfGroup(new string[] { groupA });

            AssertConsentAll(expectedValue: true);

            Countly.Instance.Consents.RemoveAllConsent();

            AssertConsentAll(expectedValue: true);

        }

        /// <summary>
        /// Case: If 'RequiresConsent' is set in the configuration and consents are given to a consent group named 'GroupA" during initialization.
        /// Result: Only Consents of group 'GroupA' should work.
        /// </summary>
        [Test]
        public void TestConsentsGivenDuringInit()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                RequiresConsent = true
            };

            string groupA = "GroupA";
            configuration.GiveConsent(new Consents[] { Consents.Crashes, Consents.Events });

            configuration.CreateConsentGroup(groupA, new Consents[] { Consents.Sessions, Consents.Location });

            configuration.GiveConsentToGroup(new string[] { groupA });

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Consents);

            AssertConsentArray(new Consents[] { Consents.Events, Consents.Crashes, Consents.Sessions, Consents.Location }, true);
            AssertConsentArray(new Consents[] { Consents.Views, Consents.Users, Consents.Clicks, Consents.StarRating, Consents.RemoteConfig }, false);

        }

        /// <summary>
        /// Case: If 'RequiresConsent' is set in the configuration. Consents are given or removed using 'GiveConsentAll' and 'RemoveAllConsent' after initialization.
        /// </summary>
        [Test]
        public void TestGiveAndRemoveAllConsent()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                RequiresConsent = true
            };


            Countly.Instance.Init(configuration);

            /// All consent shouldn't work.
            Assert.IsNotNull(Countly.Instance.Consents);
            AssertConsentAll(expectedValue: false);

            /// All consent should work.
            Countly.Instance.Consents.GiveConsentAll();
            AssertConsentAll(expectedValue: true);

            /// All consents shouldn't work
            Countly.Instance.Consents.RemoveAllConsent();
            AssertConsentAll(expectedValue: false);


        }

        /// <summary>
        /// Case: If 'RequiresConsent' is set in the configuration and consents are given using multiple groups during initialization.
        /// </summary>
        [Test]
        public void TestConfigGiveConsents()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                RequiresConsent = true,
            };

            string groupA = "GroupA";
            string groupB = "GroupB";
            string groupC = "GroupC";

            configuration.RequiresConsent = true;
            configuration.GiveConsent(new Consents[] { Consents.Crashes, Consents.Events });

            configuration.CreateConsentGroup(groupA, new Consents[] { Consents.Sessions, Consents.Location });
            configuration.CreateConsentGroup(groupB, new Consents[] { Consents.RemoteConfig, Consents.Users, Consents.Location });
            configuration.CreateConsentGroup(groupC, new Consents[] { Consents.StarRating });

            configuration.GiveConsentToGroup(new string[] { groupA, groupC });

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Consents);

            AssertConsentArray(new Consents[] { Consents.Events, Consents.Crashes, Consents.Sessions, Consents.Location, Consents.StarRating }, true);
            AssertConsentArray(new Consents[] { Consents.Views, Consents.Users, Consents.Clicks, Consents.RemoteConfig }, false);

        }

        /// <summary>
        /// Case: If 'RequiresConsent' is set in the configuration and individual consents are given to multiple features after initialization.
        /// </summary>
        [Test]
        public void TestGiveIndividualConsents()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                RequiresConsent = true,
            };


            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Consents);

            // Only Events and Crashes features should work
            Countly.Instance.Consents.GiveConsent(new Consents[] { Consents.Events, Consents.Crashes });
            AssertConsentArray(new Consents[] { Consents.Events, Consents.Crashes }, true);
            AssertConsentArray(new Consents[] { Consents.Views, Consents.Users, Consents.Clicks, Consents.RemoteConfig, Consents.Sessions, Consents.Location, Consents.StarRating }, false);

            // Only Events, Crashes, and StarRating features should work
            Countly.Instance.Consents.GiveConsent(new Consents[] { Consents.StarRating });
            AssertConsentArray(new Consents[] { Consents.Events, Consents.Crashes, Consents.StarRating }, true);
            AssertConsentArray(new Consents[] { Consents.Views, Consents.Users, Consents.Clicks, Consents.RemoteConfig, Consents.Sessions, Consents.Location }, false);

        }

        /// <summary>
        /// Case: If 'RequiresConsent' is set in the configuration and individual consents are given to multiple features during initialization and removed a few consents after initialization.
        /// </summary>
        [Test]
        public void TestRemovalIndividualConsents()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                RequiresConsent = true,
            };

            configuration.GiveConsent(new Consents[] { Consents.Crashes, Consents.Views, Consents.StarRating, Consents.Events, Consents.Users });
            Countly.Instance.Init(configuration);

            AssertConsentArray(new Consents[] { Consents.Events, Consents.Crashes, Consents.StarRating, Consents.Views, Consents.Users }, true);
            AssertConsentArray(new Consents[] { Consents.Clicks, Consents.RemoteConfig, Consents.Sessions, Consents.Location }, false);

            Countly.Instance.Consents.RemoveConsent(new Consents[] { Consents.Views, Consents.Users });
            AssertConsentArray(new Consents[] { Consents.Events, Consents.Crashes, Consents.StarRating }, true);
            AssertConsentArray(new Consents[] { Consents.Clicks, Consents.RemoteConfig, Consents.Sessions, Consents.Location, Consents.Views, Consents.Users }, false);
        }

        /// <summary>
        /// Case: If 'RequiresConsent' is set in the configuration and consents are given to multiple groups during and after initialization.
        /// </summary>
        [Test]
        public void TestGiveConsentToGroup()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                RequiresConsent = true,
            };

            string groupA = "GroupA";
            string groupB = "GroupB";

            configuration.CreateConsentGroup(groupA, new Consents[] { Consents.Sessions, Consents.Location });
            configuration.CreateConsentGroup(groupB, new Consents[] { Consents.RemoteConfig, Consents.Users });

            configuration.GiveConsentToGroup(new string[] { groupA });

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Consents);
            AssertConsentArray(new Consents[] { Consents.Sessions, Consents.Location }, true);
            AssertConsentArray(new Consents[] { Consents.Views, Consents.Users, Consents.Clicks, Consents.RemoteConfig, Consents.Events, Consents.Crashes, Consents.StarRating }, false);


            Countly.Instance.Consents.GiveConsentToGroup(new string[] { groupB });
            AssertConsentArray(new Consents[] { Consents.Sessions, Consents.Location, Consents.Users, Consents.RemoteConfig, }, true);
            AssertConsentArray(new Consents[] { Consents.Views, Consents.Clicks, Consents.Events, Consents.Crashes, Consents.StarRating }, false);
        }

        /// <summary>
        /// Case: If 'RequiresConsent' is set in the configuration and consents are removed of multiple groups after initialization.
        /// </summary>
        [Test]
        public void TestRemoveConsentOfGroup()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                RequiresConsent = true,
            };

            string groupA = "GroupA";
            string groupB = "GroupB";

            configuration.CreateConsentGroup(groupA, new Consents[] { Consents.Clicks, Consents.Views });
            configuration.CreateConsentGroup(groupB, new Consents[] { Consents.RemoteConfig, Consents.Users });


            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Consents);

            Countly.Instance.Consents.GiveConsentAll();
            AssertConsentAll(expectedValue: true);

            Countly.Instance.Consents.RemoveConsentOfGroup(new string[] { groupA });
            AssertConsentArray(new Consents[] { Consents.Sessions, Consents.Location, Consents.Users, Consents.RemoteConfig, Consents.Events, Consents.Crashes, Consents.StarRating }, true);
            AssertConsentArray(new Consents[] { Consents.Views, Consents.Clicks }, false);


            Countly.Instance.Consents.RemoveConsentOfGroup(new string[] { groupB });
            AssertConsentArray(new Consents[] { Consents.Sessions, Consents.Location, Consents.Events, Consents.Crashes, Consents.StarRating }, true);
            AssertConsentArray(new Consents[] { Consents.Views, Consents.Clicks, Consents.Users, Consents.RemoteConfig }, false);
        }

        /// <summary>
        /// Case: If 'RequiresConsent' is set in the configuration, the user's location is given and the location consent is given during initialization, and location consent gets removed after initialization.
        /// Result: User's location should reset to the default value.
        /// </summary>
        [Test]
        public void TestLocationConsentChangedListener()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                RequiresConsent = true,
            };

            string city = "Houston";
            string countryCode = "us";
            string latitude = "29.634933";
            string longitude = "-95.220255";
            string ipAddress = "10.2.33.12";

            configuration.SetLocation(countryCode, city, latitude + "," + longitude, ipAddress);

            configuration.GiveConsent(new Consents[] { Consents.Location, Consents.RemoteConfig });

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Location);

            Assert.AreEqual(Countly.Instance.Location.City, "Houston");
            Assert.AreEqual(Countly.Instance.Location.CountryCode, "us");
            Assert.IsFalse(Countly.Instance.Location.IsLocationDisabled);
            Assert.AreEqual(Countly.Instance.Location.IPAddress, "10.2.33.12");
            Assert.AreEqual(Countly.Instance.Location.Location, "29.634933,-95.220255");

            Assert.IsNotNull(Countly.Instance.Consents);
            Assert.IsTrue(Countly.Instance.Consents.CheckConsentInternal(Consents.Location));

            Countly.Instance.Consents.RemoveConsent(new Consents[] { Consents.Location });

            Assert.IsFalse(Countly.Instance.Consents.CheckConsentInternal(Consents.Location));
            Assert.IsNull(Countly.Instance.Location.City);
            Assert.IsNull(Countly.Instance.Location.Location);
            Assert.IsNull(Countly.Instance.Location.IPAddress);
            Assert.IsNull(Countly.Instance.Location.CountryCode);
            Assert.IsFalse(Countly.Instance.Location.IsLocationDisabled);
        }

        /// <summary>
        /// Case: If 'RequiresConsent' is set in the configuration and consent of a specific feature is given multiple times after initialization.
        /// Result: 'ConsentChanged' should call with distinct modified consents list on listeners. There shouldn't any duplicates entries.
        /// </summary>
        [Test]
        public void TestListenerOnMultipleConsentOfSameFeature()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                RequiresConsent = true,
            };


            ConsentTestHelperClass listener = new ConsentTestHelperClass();
            CountlyLogHelper logHelper = new CountlyLogHelper(configuration);
            ConsentCountlyService consentCountlyService = new ConsentCountlyService(configuration, logHelper, null, null);
            consentCountlyService.LockObj = new object();
            consentCountlyService.Listeners = new List<AbstractBaseService> { listener };

            consentCountlyService.GiveConsent(new Consents[] { Consents.Location, Consents.RemoteConfig, Consents.RemoteConfig, Consents.Events });
            Assert.IsTrue(listener.Validate(0, new Consents[] { Consents.Location, Consents.RemoteConfig, Consents.Events }, true));

            consentCountlyService.RemoveConsent(new Consents[] { Consents.Location, Consents.Location, Consents.StarRating, Consents.Events });
            Assert.AreEqual(2, listener.DeltaConsentsList[1].updatedConsents.Count);
            Assert.IsTrue(listener.Validate(1, new Consents[] { Consents.Location, Consents.Events }, false));
        }

        /// <summary>
        /// Case: If 'RequiresConsent' is set in the configuration and individual consents are given and removed to multiple features after initialization. 
        /// Result: 'ConsentChanged' should call with a modified consents list on listeners.
        /// </summary>
        [Test]
        public void TestListenerOnConsentChanged()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                RequiresConsent = true,
            };


            ConsentTestHelperClass listener = new ConsentTestHelperClass();
            CountlyLogHelper logHelper = new CountlyLogHelper(configuration);
            ConsentCountlyService consentCountlyService = new ConsentCountlyService(configuration, logHelper, null, null);
            consentCountlyService.LockObj = new object();
            consentCountlyService.Listeners = new List<AbstractBaseService> { listener };

            consentCountlyService.GiveConsent(new Consents[] { Consents.Location, Consents.RemoteConfig, Consents.Events });
            Assert.IsTrue(listener.Validate(0, new Consents[] { Consents.Location, Consents.RemoteConfig, Consents.Events }, true));

            consentCountlyService.GiveConsent(new Consents[] { Consents.Location, Consents.StarRating });
            Assert.IsTrue(listener.Validate(1, new Consents[] { Consents.StarRating }, true));

            consentCountlyService.RemoveConsent(new Consents[] { Consents.Location, Consents.StarRating });
            Assert.IsTrue(listener.Validate(2, new Consents[] { Consents.Location, Consents.StarRating }, false));

            consentCountlyService.RemoveConsent(new Consents[] { Consents.Events, Consents.StarRating });
            Assert.IsTrue(listener.Validate(3, new Consents[] { Consents.Events }, false));
        }

        /// <summary>
        /// Case: If 'RequiresConsent' is set in the configuration and consents are given and removed to multiple groups after initialization. 
        /// Result: 'ConsentChanged' should call with a modified consents list on listeners.
        /// </summary>
        [Test]
        public void TestListenerOnConsentGroups()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                RequiresConsent = true,
            };

            string groupA = "GroupA";
            string groupB = "GroupB";

            configuration.CreateConsentGroup(groupA, new Consents[] { Consents.Clicks, Consents.Views });

            ConsentTestHelperClass listener = new ConsentTestHelperClass();
            CountlyLogHelper logHelper = new CountlyLogHelper(configuration);
            ConsentCountlyService consentCountlyService = new ConsentCountlyService(configuration, logHelper, null, null);
            consentCountlyService.LockObj = new object();
            consentCountlyService.Listeners = new List<AbstractBaseService> { listener };

            consentCountlyService.GiveConsentToGroup(new string[] { groupA });
            Assert.IsTrue(listener.Validate(0, new Consents[] { Consents.Clicks, Consents.Views }, true));

            consentCountlyService.GiveConsentToGroup(new string[] { groupB });
            Assert.AreEqual(1, listener.DeltaConsentsList.Count);

            consentCountlyService.RemoveConsentOfGroup(new string[] { groupA });
            Assert.IsTrue(listener.Validate(1, new Consents[] { Consents.Clicks, Consents.Views }, false));


            consentCountlyService.GiveConsent(new Consents[] { Consents.Push, Consents.Views });
            Assert.IsTrue(listener.Validate(2, new Consents[] { Consents.Push, Consents.Views }, true));

            consentCountlyService.GiveConsentToGroup(new string[] { groupA });
            Assert.IsTrue(listener.Validate(3, new Consents[] { Consents.Clicks }, true));
        }

        /// <summary>
        /// Case:
        /// step 1: If 'RequiresConsent' is set in the configuration, consents are given to an individual group 'GroupA' after initialization. 
        /// Result: 'ConsentChanged' should call with a modified consents list containing consents of 'GroupA' on listeners.
        /// step 2: Remove consents of all features. 
        /// Result: 'ConsentChanged' should call with a modified consents list containing consents of 'GroupA' on listeners.
        /// step 2: Give consents to all features. 
        /// Result: 'ConsentChanged' should call with a modified consents list containing all consents on listeners.
        /// </summary>
        [Test]
        public void TestListenerOnAllConsentRemovalAndGiven()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                RequiresConsent = true,
            };

            string groupA = "GroupA";

            configuration.CreateConsentGroup(groupA, new Consents[] { Consents.Clicks, Consents.Views });

            ConsentTestHelperClass listener = new ConsentTestHelperClass();
            CountlyLogHelper logHelper = new CountlyLogHelper(configuration);
            ConsentCountlyService consentCountlyService = new ConsentCountlyService(configuration, logHelper, null, null);
            consentCountlyService.LockObj = new object();

            consentCountlyService.Listeners = new List<AbstractBaseService> { listener };

            consentCountlyService.GiveConsentToGroup(new string[] { groupA });
            Assert.IsTrue(listener.Validate(0, new Consents[] { Consents.Clicks, Consents.Views }, true));


            consentCountlyService.RemoveAllConsent();
            Assert.IsTrue(listener.Validate(1, new Consents[] { Consents.Clicks, Consents.Views }, false));

            consentCountlyService.GiveConsentAll();
            Consents[] consents = System.Enum.GetValues(typeof(Consents)).Cast<Consents>().ToArray();
            Assert.IsTrue(listener.Validate(2, consents, true));
        }
        [TearDown]
        public void End()
        {
            Countly.Instance.ClearStorage();
            Object.DestroyImmediate(Countly.Instance);

        }

        private class ConsentTestHelperClass : AbstractBaseService
        {
            internal List<DeltaConsents> DeltaConsentsList { get; private set; }
            internal ConsentTestHelperClass() : base(null, null, null)
            {
                DeltaConsentsList = new List<DeltaConsents>();
            }

            internal override void ConsentChanged(List<Consents> updatedConsents, bool newConsentValue)
            {
                DeltaConsents deltaConsents;
                deltaConsents.value = newConsentValue;
                deltaConsents.updatedConsents = updatedConsents;

                DeltaConsentsList.Add(deltaConsents);

            }

            internal bool Validate(int callIndex, Consents[] calledConsents, bool targetValue)
            {
                if (callIndex < 0 || callIndex > DeltaConsentsList.Count - 1) {
                    return false;
                }

                DeltaConsents deltaConsents = DeltaConsentsList[callIndex];

                if (deltaConsents.value != targetValue || deltaConsents.updatedConsents.Count != calledConsents.Length) {
                    return false;
                }

                foreach (Consents consent in calledConsents) {
                    if (!deltaConsents.updatedConsents.Contains(consent)) {
                        return false;
                    }
                }

                return true;
            }

            internal struct DeltaConsents
            {
                internal bool value;
                internal List<Consents> updatedConsents;
            }
        }
    }
}
