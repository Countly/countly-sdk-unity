using NUnit.Framework;
using UnityEngine;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK;
using System.Threading.Tasks;
using System.Web;
using System.Collections.Specialized;
using Plugins.CountlySDK.Enums;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;

namespace Tests
{
    public class DeviceIdTests
    {
        private readonly string _serverUrl = "https://xyz.com/";
        private readonly string _appKey = "772c091355076ead703f987fee94490";

        /// <summary>
        /// It validates the working of methods 'ChangeDeviceIdWithMerge' and 'ChangeDeviceIdWithoutMerge' on giving same device id.
        /// </summary>
        [Test]
        public async void TestSameDeviceIdLogic()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                DeviceId = "device_id"
            };

            Countly.Instance.Init(configuration);
            Assert.IsNotNull(Countly.Instance.Device);
            Assert.AreEqual("device_id", Countly.Instance.Device.DeviceId);
            Assert.AreEqual(DeviceIdType.DeveloperProvided, Countly.Instance.Device.DeviceIdType);

            Countly.Instance.Device._requestCountlyHelper._requestRepo.Clear();
            await Countly.Instance.Device.ChangeDeviceIdWithMerge("device_id");
            Assert.AreEqual(0, Countly.Instance.Device._requestCountlyHelper._requestRepo.Count);
            Assert.AreEqual(DeviceIdType.DeveloperProvided, Countly.Instance.Device.DeviceIdType);

            await Countly.Instance.Device.ChangeDeviceIdWithoutMerge("device_id");
            Assert.AreEqual(0, Countly.Instance.Device._requestCountlyHelper._requestRepo.Count);
            Assert.AreEqual(DeviceIdType.DeveloperProvided, Countly.Instance.Device.DeviceIdType);

            Assert.AreEqual("device_id", Countly.Instance.Device.DeviceId);

        }


        /// <summary>
        /// It validates the functionality of method 'ChangeDeviceIdWithoutMerge'.
        /// </summary>
        [Test]
        public async void TestDeviceServiceMethod_ChangeDeviceIdWithoutMerge()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
            };

            Countly.Instance.Init(configuration);
            Assert.IsNotNull(Countly.Instance.Consents);
            Assert.AreEqual(DeviceIdType.SDKGenerated, Countly.Instance.Device.DeviceIdType);

            string oldDeviceId = Countly.Instance.Device.DeviceId;
            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();
            await Countly.Instance.Device.ChangeDeviceIdWithoutMerge("new_device_id");
            //RQ will have begin session and end session requests
            Assert.AreEqual(2, Countly.Instance.Device._requestCountlyHelper._requestRepo.Count);
            Assert.AreEqual(DeviceIdType.DeveloperProvided, Countly.Instance.Device.DeviceIdType);


            CountlyRequestModel requestModel = Countly.Instance.Device._requestCountlyHelper._requestRepo.Dequeue();
            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);

            Assert.AreEqual("1", collection.Get("end_session"));
            Assert.AreEqual(oldDeviceId, collection.Get("device_id"));
            Assert.IsNotNull(collection["session_duration"]);

            requestModel = Countly.Instance.Device._requestCountlyHelper._requestRepo.Dequeue();
            collection = HttpUtility.ParseQueryString(requestModel.RequestData);

            Assert.AreEqual("1", collection.Get("begin_session"));
            Assert.AreEqual("new_device_id", collection.Get("device_id"));
            Assert.AreEqual("new_device_id", Countly.Instance.Device.DeviceId);
        }

        /// <summary>
        /// It validates the consent removal after changing the device id without merging.
        /// </summary>
        [Test]
        public async void TestConsentRemoval_ChangeDeviceIdWithoutMerge()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                RequiresConsent = true,
            };

            configuration.GiveConsent(new Consents[] { Consents.Crashes, Consents.Events, Consents.Clicks, Consents.StarRating, Consents.Views, Consents.Users, Consents.Sessions, Consents.Push, Consents.RemoteConfig, Consents.Location, Consents.Feedback });

            Countly.Instance.Init(configuration);
            Assert.IsNotNull(Countly.Instance.Consents);
            Assert.AreEqual(DeviceIdType.SDKGenerated, Countly.Instance.Device.DeviceIdType);

            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();
            string oldDeviceId = Countly.Instance.Device.DeviceId;
            await Countly.Instance.Device.ChangeDeviceIdWithoutMerge("new_device_id_1");
            //RQ will have end session request
            Assert.AreEqual(1, Countly.Instance.Device._requestCountlyHelper._requestRepo.Count);
            Assert.AreEqual(DeviceIdType.DeveloperProvided, Countly.Instance.Device.DeviceIdType);

            CountlyRequestModel requestModel = Countly.Instance.Device._requestCountlyHelper._requestRepo.Dequeue();
            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);

            Assert.AreEqual("1", collection.Get("end_session"));
            Assert.AreEqual(oldDeviceId, collection.Get("device_id"));
            Assert.IsNotNull(collection["session_duration"]);

            Assert.IsTrue(Countly.Instance.Consents.RequiresConsent);

            Consents[] consents = System.Enum.GetValues(typeof(Consents)).Cast<Consents>().ToArray();
            foreach (Consents consent in consents) {
                Assert.IsFalse(Countly.Instance.Consents.CheckConsentInternal(consent));
            }
        }

        /// <summary>
        /// It validates functionality of method 'ChangeDeviceIdWithMerge'.
        /// </summary>
        [Test]
        public async void TestConset_ChangeDeviceIdWithMerge()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
            };

            Countly.Instance.Init(configuration);
            Assert.IsNotNull(Countly.Instance.Consents);
            Assert.AreEqual(DeviceIdType.SDKGenerated, Countly.Instance.Device.DeviceIdType);

            string oldDeviceId = Countly.Instance.Device.DeviceId;
            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();
            await Countly.Instance.Device.ChangeDeviceIdWithMerge("new_device_id");
            //RQ will have begin session and end session requests
            Assert.AreEqual(1, Countly.Instance.Device._requestCountlyHelper._requestRepo.Count);
            Assert.AreEqual(DeviceIdType.DeveloperProvided, Countly.Instance.Device.DeviceIdType);

            CountlyRequestModel requestModel = Countly.Instance.Device._requestCountlyHelper._requestRepo.Dequeue();
            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);

            Assert.AreEqual(oldDeviceId, collection.Get("old_device_id"));
            Assert.AreEqual("new_device_id", collection.Get("device_id"));
            Assert.AreEqual("new_device_id", Countly.Instance.Device.DeviceId);
        }

        /// <summary>
        /// It validates the functionality of method 'ChangeDeviceIdWithoutMerge' when automatic session tracking is enabled.
        /// </summary>
        [Test]
        public async void TestMethod_ChangeDeviceIdWithoutMerge_WhenAutomaticSessionTrackingEnabled()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                RequiresConsent = true,
                IsAutomaticSessionTrackingDisabled = false,
            };

            configuration.GiveConsent(new Consents[] { Consents.Crashes, Consents.Events, Consents.Clicks, Consents.StarRating, Consents.Views, Consents.Users, Consents.Sessions, Consents.Push, Consents.RemoteConfig, Consents.Location, Consents.Feedback });

            Countly.Instance.Init(configuration);
            Assert.IsNotNull(Countly.Instance.Consents);
            Assert.AreEqual(DeviceIdType.SDKGenerated, Countly.Instance.Device.DeviceIdType);

            string oldDeviceId = Countly.Instance.Device.DeviceId;
            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();
            await Countly.Instance.Device.ChangeDeviceIdWithoutMerge("new_device_id");
            //RQ will have end session request
            Assert.AreEqual(1, Countly.Instance.Device._requestCountlyHelper._requestRepo.Count);
            Assert.AreEqual(DeviceIdType.DeveloperProvided, Countly.Instance.Device.DeviceIdType);

            CountlyRequestModel requestModel = Countly.Instance.Device._requestCountlyHelper._requestRepo.Dequeue();
            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);

            Assert.AreEqual("1", collection.Get("end_session"));
            Assert.AreEqual(oldDeviceId, collection.Get("device_id"));
            Assert.IsNotNull(collection["session_duration"]);

            Countly.Instance.Consents.GiveConsentAll();

            //RQ will have consent request and begin session request
            Assert.AreEqual(2, Countly.Instance.Device._requestCountlyHelper._requestRepo.Count);

            requestModel = Countly.Instance.Device._requestCountlyHelper._requestRepo.Dequeue();
            collection = HttpUtility.ParseQueryString(requestModel.RequestData);

            JObject consentObj = JObject.Parse(collection.Get("consent"));

            Assert.AreEqual(11, consentObj.Count);
            Assert.IsTrue(consentObj.GetValue("push").ToObject<bool>());
            Assert.IsTrue(consentObj.GetValue("users").ToObject<bool>());
            Assert.IsTrue(consentObj.GetValue("views").ToObject<bool>());
            Assert.IsTrue(consentObj.GetValue("clicks").ToObject<bool>());
            Assert.IsTrue(consentObj.GetValue("events").ToObject<bool>());
            Assert.IsTrue(consentObj.GetValue("crashes").ToObject<bool>());
            Assert.IsTrue(consentObj.GetValue("sessions").ToObject<bool>());
            Assert.IsTrue(consentObj.GetValue("location").ToObject<bool>());
            Assert.IsTrue(consentObj.GetValue("feedback").ToObject<bool>());
            Assert.IsTrue(consentObj.GetValue("star-rating").ToObject<bool>());
            Assert.IsTrue(consentObj.GetValue("remote-config").ToObject<bool>());

            requestModel = Countly.Instance.Device._requestCountlyHelper._requestRepo.Dequeue();
            collection = HttpUtility.ParseQueryString(requestModel.RequestData);

            Assert.AreEqual("1", collection.Get("begin_session"));
            Assert.AreEqual("new_device_id", collection.Get("device_id"));
            Assert.AreEqual("new_device_id", Countly.Instance.Device.DeviceId);
        }

        /// <summary>
        /// It validates the functionality of method 'ChangeDeviceIdWithoutMerge' when automatic session tracking is disabled.
        /// </summary>
        [Test]
        public async void TestMethod_ChangeDeviceIdWithoutMerge_WhenAutomaticSessionTrackingIsDisabled()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                RequiresConsent = true,
                IsAutomaticSessionTrackingDisabled = true,
            };

            configuration.GiveConsent(new Consents[] { Consents.Crashes, Consents.Events, Consents.Clicks, Consents.StarRating, Consents.Views, Consents.Users, Consents.Sessions, Consents.Push, Consents.RemoteConfig, Consents.Location, Consents.Feedback });

            Countly.Instance.Init(configuration);
            Assert.IsNotNull(Countly.Instance.Consents);
            Assert.AreEqual(DeviceIdType.SDKGenerated, Countly.Instance.Device.DeviceIdType);

            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();
            await Countly.Instance.Device.ChangeDeviceIdWithoutMerge("new_device_id");
            //Since automatic session tracking is disabled, RQ will be empty
            Assert.AreEqual(0, Countly.Instance.Device._requestCountlyHelper._requestRepo.Count);
            Assert.AreEqual(DeviceIdType.DeveloperProvided, Countly.Instance.Device.DeviceIdType);


            Countly.Instance.Consents.GiveConsentAll();

            //RQ will have only consent request
            Assert.AreEqual(1, Countly.Instance.Device._requestCountlyHelper._requestRepo.Count);
            CountlyRequestModel requestModel = Countly.Instance.Device._requestCountlyHelper._requestRepo.Dequeue();
            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            JObject consentObj = JObject.Parse(collection.Get("consent"));

            Assert.AreEqual(11, consentObj.Count);
            Assert.IsTrue(consentObj.GetValue("push").ToObject<bool>());
            Assert.IsTrue(consentObj.GetValue("users").ToObject<bool>());
            Assert.IsTrue(consentObj.GetValue("views").ToObject<bool>());
            Assert.IsTrue(consentObj.GetValue("clicks").ToObject<bool>());
            Assert.IsTrue(consentObj.GetValue("events").ToObject<bool>());
            Assert.IsTrue(consentObj.GetValue("crashes").ToObject<bool>());
            Assert.IsTrue(consentObj.GetValue("sessions").ToObject<bool>());
            Assert.IsTrue(consentObj.GetValue("location").ToObject<bool>());
            Assert.IsTrue(consentObj.GetValue("feedback").ToObject<bool>());
            Assert.IsTrue(consentObj.GetValue("star-rating").ToObject<bool>());
            Assert.IsTrue(consentObj.GetValue("remote-config").ToObject<bool>());

            Assert.IsTrue(Countly.Instance.Configuration.IsAutomaticSessionTrackingDisabled);
        }

        /**
         * +--------------------------------------------------+------------------------------------+----------------------+
         * | SDK state at the end of the previous app session | Provided configuration during init | Action taken by SDK  |
         * +--------------------------------------------------+------------------------------------+----------------------+
         * |           Custom      |   SDK used a             |              Custom                |    Flag   |   flag   |
         * |         device ID     |   generated              |            device ID               |    not    |          |
         * |         was set       |       ID                 |             provided               |    set    |   set    |
         * +--------------------------------------------------+------------------------------------+----------------------+
         * |                     First init                   |                   -                |    1      |    -     |
         * +--------------------------------------------------+------------------------------------+----------------------+
         * |                     First init                   |                   x                |    2      |    -     |
         * +--------------------------------------------------+------------------------------------+----------------------+
         * |            x          |             -            |                   -                |    3      |    -     |
         * +--------------------------------------------------+------------------------------------+----------------------+
         * |            x          |             -            |                   x                |    4      |    -     |
         * +--------------------------------------------------+------------------------------------+----------------------+
         * |            -          |             x            |                   -                |    5      |    -     |
         * +--------------------------------------------------+------------------------------------+----------------------+
         * |            -          |             x            |                   x                |    6      |    -     |
         * +--------------------------------------------------+------------------------------------+----------------------+
         */

        /// <summary>
        /// Scenario 1: First time init the SDK with custom device ID and init the SDK second time without device ID.
        /// SDK Action: During second init, SDK will not override the custom device ID provided in first init. 
        /// </summary>
        [Test]
        public void TestDeviceIdGivenInConfig()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                DeviceId = "device_id"
            };

            Countly.Instance.Init(configuration);
            Assert.IsNotNull(Countly.Instance.Device);
            Assert.AreEqual("device_id", Countly.Instance.Device.DeviceId);
            Assert.AreEqual(DeviceIdType.DeveloperProvided, Countly.Instance.Device.DeviceIdType);

            // Destroy instance before init SDK again.
            CloseDBConnectionAndDestroyInstance();

            //Initialize SDK again without custom key
            configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);
            Assert.IsNotNull(Countly.Instance.Device);
            Assert.AreEqual("device_id", Countly.Instance.Device.DeviceId);
            Assert.AreEqual(DeviceIdType.DeveloperProvided, Countly.Instance.Device.DeviceIdType);
        }

        /// <summary>
        /// Scenario 2: First time init the SDK without custom device ID and init the SDK second time with custom device ID.
        /// SDK Action: During second init, SDK will not override the device ID generated during first init. 
        /// </summary>
        [Test]
        public void TestDeviceIdGeneratedBySDK()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);
            Assert.IsNotNull(Countly.Instance.Device);
            Assert.IsNotNull(Countly.Instance.Device.DeviceId);
            Assert.IsNotEmpty(Countly.Instance.Device.DeviceId);
            Assert.AreEqual(DeviceIdType.SDKGenerated, Countly.Instance.Device.DeviceIdType);

            // Destroy instance before init SDK again.
            CloseDBConnectionAndDestroyInstance();

            //Initialize SDK again with custom key
            configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                DeviceId = "device_id"
            };

            Countly.Instance.Init(configuration);
            Assert.IsNotNull(Countly.Instance.Device);
            Assert.IsNotNull(Countly.Instance.Device.DeviceId);
            Assert.IsNotEmpty(Countly.Instance.Device.DeviceId);
            Assert.AreEqual(DeviceIdType.SDKGenerated, Countly.Instance.Device.DeviceIdType);
        }

        /// <summary>
        /// Scenario 3: First time init the SDK with custom device ID and init the SDK second time without device ID.
        /// SDK Action: During second init, SDK will not override the custom device ID provided in first init. 
        /// </summary>
        [Test]
        public void CustomDeviceIDWasSet_CustomDeviceIDNotProvided()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                DeviceId = "device_id"
            };

            Countly.Instance.Init(configuration);
            Assert.IsNotNull(Countly.Instance.Device);
            Assert.AreEqual("device_id", Countly.Instance.Device.DeviceId);
            Assert.AreEqual(DeviceIdType.DeveloperProvided, Countly.Instance.Device.DeviceIdType);

            // Destroy instance before init SDK again.
            CloseDBConnectionAndDestroyInstance();

            configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);
            Assert.IsNotNull(Countly.Instance.Device);
            Assert.AreEqual("device_id", Countly.Instance.Device.DeviceId);
            Assert.AreEqual(DeviceIdType.DeveloperProvided, Countly.Instance.Device.DeviceIdType);
        }
        /// <summary>
        /// Scenario 4: First time init the SDK with custom device ID and init the SDK second time with a new custom device ID.
        /// SDK Action: During second init, SDK will not override the custom device ID provided in first init. 
        /// </summary>
        [Test]
        public void CustomDeviceIDWasSet_CustomDeviceIDProvided()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                DeviceId = "device_id"
            };

            Countly.Instance.Init(configuration);
            Assert.IsNotNull(Countly.Instance.Device);
            Assert.AreEqual("device_id", Countly.Instance.Device.DeviceId);
            Assert.AreEqual(DeviceIdType.DeveloperProvided, Countly.Instance.Device.DeviceIdType);

            // Destroy instance before init SDK again.
            CloseDBConnectionAndDestroyInstance();

            configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                DeviceId = "device_id_new"
            };

            Countly.Instance.Init(configuration);
            Assert.IsNotNull(Countly.Instance.Device);
            Assert.AreEqual("device_id", Countly.Instance.Device.DeviceId);
            Assert.AreEqual(DeviceIdType.DeveloperProvided, Countly.Instance.Device.DeviceIdType);
        }

        /// <summary>
        /// Scenario 5: First time init the SDK without custom device ID and init the SDK second time without custom device ID.
        /// SDK Action: During second init, SDK will not override the device ID generated during first init.
        /// </summary>
        [Test]
        public void GeneratedDeviceID_CustomDeviceIDNotProvided()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);
            Assert.IsNotNull(Countly.Instance.Device);
            Assert.IsNotEmpty(Countly.Instance.Device.DeviceId);
            Assert.AreEqual(DeviceIdType.SDKGenerated, Countly.Instance.Device.DeviceIdType);

            string deviceID = Countly.Instance.Device.DeviceId;

            // Destroy instance before init SDK again.
            CloseDBConnectionAndDestroyInstance();

            configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);
            Assert.IsNotNull(Countly.Instance.Device);
            Assert.AreEqual(deviceID, Countly.Instance.Device.DeviceId);
            Assert.AreEqual(DeviceIdType.SDKGenerated, Countly.Instance.Device.DeviceIdType);
        }

        /// <summary>
        /// <summary>
        /// Scenario 6: First time init the SDK without custom device ID and init the SDK second time with a custom device ID.
        /// SDK Action: During second init, SDK will not override the device ID generated during first init.
        /// </summary>
        /// </summary>
        [Test]
        public void GeneratedDeviceID_CustomDeviceIDProvided()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);
            Assert.IsNotNull(Countly.Instance.Device);
            Assert.IsNotEmpty(Countly.Instance.Device.DeviceId);
            Assert.AreEqual(DeviceIdType.SDKGenerated, Countly.Instance.Device.DeviceIdType);

            string deviceID = Countly.Instance.Device.DeviceId;

            // Destroy instance before init SDK again.
            CloseDBConnectionAndDestroyInstance();

            configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                DeviceId = "device_id_new"
            };

            Countly.Instance.Init(configuration);
            Assert.IsNotNull(Countly.Instance.Device);
            Assert.AreEqual(deviceID, Countly.Instance.Device.DeviceId);
            Assert.AreEqual(DeviceIdType.SDKGenerated, Countly.Instance.Device.DeviceIdType);
        }

        private void CloseDBConnectionAndDestroyInstance(bool clearStorage = false) {
            if (clearStorage) {
                PlayerPrefs.DeleteAll();
            }

            Countly.Instance.CloseDBConnection();
            Object.DestroyImmediate(Countly.Instance);
        }
        

            [TearDown]
        public void End()
        {
            Countly.Instance.ClearStorage();
            Object.DestroyImmediate(Countly.Instance);
        }
    }
}
