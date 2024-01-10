using NUnit.Framework;
using UnityEngine;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK;
using System.Web;
using System.Collections.Specialized;
using Plugins.CountlySDK.Enums;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Assets.Tests.PlayModeTests
{
    public class DeviceIdTests
    {
        private readonly string _serverUrl = "https://xyz.com/";
        private readonly string _appKey = "772c091355076ead703f987fee94490";

        /// <summary>
        /// Assert an array of keys against the expected value in consnet reqeust json.
        /// </summary>
        /// <param name="expectedValue"> an expected values of consents</param>
        /// <param name="consents"> an array consents</param>
        private void AssertConsentKeys(JObject consentObj, string[] keys, bool expectedValue)
        {
            foreach (string key in keys) {
                Assert.AreEqual(expectedValue, consentObj.GetValue(key).ToObject<bool>());
            }
        }

        /// <summary>
        /// Assert session request.
        /// </summary>
        /// <param name="collection"> collection of params</param>
        /// <param name="sessionKey"> session predefined key </param>
        /// <param name="deviceId"> device id </param>
        private void AssertSessionRequest(NameValueCollection collection, string sessionKey, string deviceId, bool checkDuration = false)
        {
            Assert.AreEqual("1", collection.Get(sessionKey));
            Assert.AreEqual(deviceId, collection.Get("device_id"));
            if (checkDuration) {
                Assert.IsNotNull(collection["session_duration"]);
            }
        }

        private Countly ConfigureAndInitSDK(string deviceId = null, bool consentRequired = false, Consents[] consents = null, bool isAutomaticSessionTrackingDisabled = false)
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
                RequiresConsent = consentRequired,
                DeviceId = deviceId,
                IsAutomaticSessionTrackingDisabled = isAutomaticSessionTrackingDisabled
            };

            configuration.GiveConsent(consents);

            Countly.Instance.Init(configuration);
            return Countly.Instance;
        }

        private void ValidateDeviceIDAndType(Countly instance, string deviceId, DeviceIdType type, bool compareDeviceId = true)
        {
            Assert.IsNotNull(instance.Device);
            Assert.AreEqual(type, instance.Device.DeviceIdType);

            if (compareDeviceId) {
                Assert.AreEqual(deviceId, instance.Device.DeviceId);
            } else {
                Assert.IsNotEmpty(instance.Device.DeviceId);

            }
        }

        /// <summary>
        /// It validates the working of methods 'ChangeDeviceIdWithMerge' and 'ChangeDeviceIdWithoutMerge' on giving same device id.
        /// </summary>
        [Test]
        public async void TestSameDeviceIdLogic()
        {
            ConfigureAndInitSDK("device_id");

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
            ConfigureAndInitSDK();
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


            Assert.AreEqual("1", collection.Get("t"));
            AssertSessionRequest(collection, "end_session", oldDeviceId, true);

            requestModel = Countly.Instance.Device._requestCountlyHelper._requestRepo.Dequeue();
            collection = HttpUtility.ParseQueryString(requestModel.RequestData);

            Assert.AreEqual("0", collection.Get("t"));
            AssertSessionRequest(collection, "begin_session", "new_device_id");
        }

        /// <summary>
        /// It validates the consent removal after changing the device id without merging.
        /// </summary>
        [Test]
        public async void TestConsentRemoval_ChangeDeviceIdWithoutMerge()
        {
            ConfigureAndInitSDK(null, true, new Consents[] { Consents.Crashes, Consents.Events, Consents.Clicks, Consents.StarRating, Consents.Views, Consents.Users, Consents.Sessions, Consents.Push, Consents.RemoteConfig, Consents.Location, Consents.Feedback });
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

            Assert.AreEqual("1", collection.Get("t"));
            AssertSessionRequest(collection, "end_session", oldDeviceId, true);

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
            ConfigureAndInitSDK();

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

            Assert.AreEqual("0", collection.Get("t"));
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
            ConfigureAndInitSDK(null, true, new Consents[] { Consents.Crashes, Consents.Events, Consents.Clicks, Consents.StarRating, Consents.Views, Consents.Users, Consents.Sessions, Consents.Push, Consents.RemoteConfig, Consents.Location, Consents.Feedback });
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

            Assert.AreEqual("1", collection.Get("t"));
            Assert.AreEqual(oldDeviceId, collection.Get("device_id"));
            AssertSessionRequest(collection, "end_session", oldDeviceId, true);

            Countly.Instance.Consents.GiveConsentAll();

            //RQ will have consent request and begin session request
            Assert.AreEqual(2, Countly.Instance.Device._requestCountlyHelper._requestRepo.Count);

            requestModel = Countly.Instance.Device._requestCountlyHelper._requestRepo.Dequeue();
            collection = HttpUtility.ParseQueryString(requestModel.RequestData);

            JObject consentObj = JObject.Parse(collection.Get("consent"));
            AssertConsentKeys(consentObj, new string[] { "push", "users", "views", "clicks", "events", "crashes", "sessions", "location", "feedback", "star-rating", "remote-config" }, true);

            requestModel = Countly.Instance.Device._requestCountlyHelper._requestRepo.Dequeue();
            collection = HttpUtility.ParseQueryString(requestModel.RequestData);

            AssertSessionRequest(collection, "begin_session", "new_device_id");

            Assert.AreEqual("1", collection.Get("begin_session"));
            Assert.AreEqual("0", collection.Get("t"));
        }

        /// <summary>
        /// It validates the functionality of method 'ChangeDeviceIdWithoutMerge' when automatic session tracking is disabled.
        /// </summary>
        [Test]
        public async void TestMethod_ChangeDeviceIdWithoutMerge_WhenAutomaticSessionTrackingIsDisabled()
        {
            ConfigureAndInitSDK(null, true, new Consents[] { Consents.Crashes, Consents.Events, Consents.Clicks, Consents.StarRating, Consents.Views, Consents.Users, Consents.Sessions, Consents.Push, Consents.RemoteConfig, Consents.Location, Consents.Feedback }, true);

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
            AssertConsentKeys(consentObj, new string[] { "push", "users", "views", "clicks", "events", "crashes", "sessions", "location", "feedback", "star-rating", "remote-config" }, true);

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
        /// Scenario 1: First time init the SDK without custom device ID and init the SDK second time with custom device ID.
        /// SDK Action: During second init, SDK will not override the device ID generated during first init. 
        /// </summary>
        [Test]
        public void TestDeviceIdGeneratedBySDK()
        {
            ConfigureAndInitSDK();
            ValidateDeviceIDAndType(Countly.Instance, null, DeviceIdType.SDKGenerated, false);
        }

        /// <summary>
        /// Scenario 2: First time init the SDK with custom device ID and init the SDK second time without device ID.
        /// SDK Action: During second init, SDK will not override the custom device ID provided in first init. 
        /// </summary>
        [Test]
        public void TestDeviceIdGivenInConfig()
        {
            ConfigureAndInitSDK("device_id");
            ValidateDeviceIDAndType(Countly.Instance, "device_id", DeviceIdType.DeveloperProvided);
        }

        /// <summary>
        /// Scenario 3: First time init the SDK with custom device ID and init the SDK second time without device ID.
        /// SDK Action: During second init, SDK will not override the custom device ID provided in first init. 
        /// </summary>
        [Test]
        public void CustomDeviceIDWasSet_CustomDeviceIDNotProvided()
        {
            ConfigureAndInitSDK("device_id");
            ValidateDeviceIDAndType(Countly.Instance, "device_id", DeviceIdType.DeveloperProvided);

            // Destroy instance before init SDK again.
            CloseDBConnectionAndDestroyInstance();

            ConfigureAndInitSDK();
            ValidateDeviceIDAndType(Countly.Instance, "device_id", DeviceIdType.DeveloperProvided);

        }
        /// <summary>
        /// Scenario 4: First time init the SDK with custom device ID and init the SDK second time with a new custom device ID.
        /// SDK Action: During second init, SDK will not override the custom device ID provided in first init. 
        /// </summary>
        [Test]
        public void CustomDeviceIDWasSet_CustomDeviceIDProvided()
        {
            ConfigureAndInitSDK("device_id");
            ValidateDeviceIDAndType(Countly.Instance, "device_id", DeviceIdType.DeveloperProvided);

            // Destroy instance before init SDK again.
            CloseDBConnectionAndDestroyInstance();

            ConfigureAndInitSDK("device_id_new");
            ValidateDeviceIDAndType(Countly.Instance, "device_id", DeviceIdType.DeveloperProvided);
        }

        /// <summary>
        /// Scenario 5: First time init the SDK without custom device ID and init the SDK second time without custom device ID.
        /// SDK Action: During second init, SDK will not override the device ID generated during first init.
        /// </summary>
        [Test]
        public void GeneratedDeviceID_CustomDeviceIDNotProvided()
        {
            ConfigureAndInitSDK();
            ValidateDeviceIDAndType(Countly.Instance, null, DeviceIdType.SDKGenerated, false);

            string deviceID = Countly.Instance.Device.DeviceId;

            // Destroy instance before init SDK again.
            CloseDBConnectionAndDestroyInstance();

            ConfigureAndInitSDK();
            ValidateDeviceIDAndType(Countly.Instance, deviceID, DeviceIdType.SDKGenerated);
        }

        /// <summary>
        /// Scenario 6: First time init the SDK without custom device ID and init the SDK second time with a custom device ID.
        /// SDK Action: During second init, SDK will not override the device ID generated during first init.
        /// </summary>
        [Test]
        public void GeneratedDeviceID_CustomDeviceIDProvided()
        {
            ConfigureAndInitSDK();
            ValidateDeviceIDAndType(Countly.Instance, null, DeviceIdType.SDKGenerated, false);

            string deviceID = Countly.Instance.Device.DeviceId;

            // Destroy instance before init SDK again.
            CloseDBConnectionAndDestroyInstance();

            ConfigureAndInitSDK("device_id_new");
            ValidateDeviceIDAndType(Countly.Instance, deviceID, DeviceIdType.SDKGenerated);
        }

        private void CloseDBConnectionAndDestroyInstance(bool clearStorage = false)
        {
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
