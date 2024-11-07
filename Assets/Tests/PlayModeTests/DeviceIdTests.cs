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

        private Countly ConfigureAndInitSDK(string deviceId = null, bool consentRequired = false, Consents[] consents = null, bool isAutomaticSessionTrackingDisabled = false)
        {
            CountlyConfiguration configuration = new CountlyConfiguration(_appKey, _serverUrl)
                .SetRequiresConsent(consentRequired);
            
            if(isAutomaticSessionTrackingDisabled) {
                configuration.DisableAutomaticSessionTracking();
            }
            if(deviceId != null) {
                configuration.SetDeviceId(deviceId);
            }

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

        // 'ChangeDeviceIdWithMerge' method in 'DeviceIdCountlyService'
        // We provide the same device id to validate the functionality
        // Should not generate requests and all should work correctly
        [Test]
        public async void ChangeDeviceIdWithMerge_SameId()
        {
            CountlyConfiguration configuration = new CountlyConfiguration(_appKey, _serverUrl)
                .SetDeviceId("device_id");

            Countly.Instance.Init(configuration);
            Assert.IsNotNull(Countly.Instance.Device);
            Assert.AreEqual("device_id", Countly.Instance.Device.DeviceId);
            Assert.AreEqual(DeviceIdType.DeveloperProvided, Countly.Instance.Device.DeviceIdType);

            // make sure request repository is clean before merge
            Countly.Instance.Device._requestCountlyHelper._requestRepo.Clear();
            // should not generate any requests
            await Countly.Instance.Device.ChangeDeviceIdWithMerge("device_id");
            Assert.AreEqual(0, Countly.Instance.Device._requestCountlyHelper._requestRepo.Count);
            Assert.AreEqual(DeviceIdType.DeveloperProvided, Countly.Instance.Device.DeviceIdType);
            
            // validate that device id remains same
            Assert.AreEqual("device_id", Countly.Instance.Device.DeviceId);
        }

        // 'ChangeDeviceIdWithoutMerge' method in 'DeviceIdCountlyService'
        // We provide the same device id to validate the functionality
        // Should not generate requests and all should work correctly
        [Test]
        public async void ChangeDeviceIdWithoutMerge_SameId()
        {
            CountlyConfiguration configuration = new CountlyConfiguration(_appKey, _serverUrl)
                .SetDeviceId("device_id");

            Countly.Instance.Init(configuration);
            Assert.IsNotNull(Countly.Instance.Device);
            Assert.AreEqual("device_id", Countly.Instance.Device.DeviceId);
            Assert.AreEqual(DeviceIdType.DeveloperProvided, Countly.Instance.Device.DeviceIdType);

            // make sure request repository is clean before merge
            Countly.Instance.Device._requestCountlyHelper._requestRepo.Clear();
            // should not generate any requests
            await Countly.Instance.Device.ChangeDeviceIdWithoutMerge("device_id");
            Assert.AreEqual(0, Countly.Instance.Device._requestCountlyHelper._requestRepo.Count);
            Assert.AreEqual(DeviceIdType.DeveloperProvided, Countly.Instance.Device.DeviceIdType);

            // validate that device id remains same
            Assert.AreEqual("device_id", Countly.Instance.Device.DeviceId);
        }

        // 'ChangeDeviceIdWithoutMerge' method in 'DeviceIdCountlyService'
        // We provide the new device id over SDK generated device id to validate the functionality
        // Should generate requests, change device id and type, all should work correctly
        [Test]
        public async void ChangeDeviceIdWithoutMerge_SDKGeneratedId()
        {
            CountlyConfiguration configuration = new CountlyConfiguration(_appKey, _serverUrl);
            Countly.Instance.Init(configuration);
            Assert.AreEqual(DeviceIdType.SDKGenerated, Countly.Instance.Device.DeviceIdType);
            string oldDeviceId = Countly.Instance.Device.DeviceId;

            // make sure the request repo is clean
            Countly.Instance.RequestHelper._requestRepo.Clear();
            // should generate 2 requests. 1 for end session and 1 for begin session
            await Countly.Instance.Device.ChangeDeviceIdWithoutMerge("new_device_id");
            Assert.AreEqual(2, Countly.Instance.Device._requestCountlyHelper._requestRepo.Count);
            Assert.AreEqual(DeviceIdType.DeveloperProvided, Countly.Instance.Device.DeviceIdType);
            CountlyRequestModel requestModel = Countly.Instance.Device._requestCountlyHelper._requestRepo.Dequeue();
        }

        // 'ChangeDeviceIdWithoutMerge' method in 'DeviceIdCountlyService'
        // We validate whether consent is remaining or not after changing the device id without merging.
        // Device Id should change and no consent should be given after id change
        [Test]
        public async void ChangeDeviceIdWithoutMerge_Consent()
        {
            CountlyConfiguration configuration = new CountlyConfiguration(_appKey, _serverUrl)
                .SetRequiresConsent(true);

            configuration.GiveConsent(new Consents[] { Consents.Crashes, Consents.Events, Consents.Clicks, Consents.StarRating, Consents.Views, Consents.Users, Consents.Sessions, Consents.Push, Consents.RemoteConfig, Consents.Location, Consents.Feedback });
            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Consents);
            Assert.AreEqual(DeviceIdType.SDKGenerated, Countly.Instance.Device.DeviceIdType);
            // validate that request repo is clean before changing id
            Countly.Instance.RequestHelper._requestRepo.Clear();
            string oldDeviceId = Countly.Instance.Device.DeviceId;
            await Countly.Instance.Device.ChangeDeviceIdWithoutMerge("new_device_id_1");

            //RQ will have end session request
            Assert.AreEqual(1, Countly.Instance.Device._requestCountlyHelper._requestRepo.Count);
            Assert.AreEqual(DeviceIdType.DeveloperProvided, Countly.Instance.Device.DeviceIdType);
            Assert.AreEqual(Countly.Instance.Device.DeviceId, "new_device_id_1");

            Assert.IsTrue(Countly.Instance.Consents.RequiresConsent);
            Consents[] consents = System.Enum.GetValues(typeof(Consents)).Cast<Consents>().ToArray();
            foreach (Consents consent in consents) {
                Assert.IsFalse(Countly.Instance.Consents.CheckConsentInternal(consent));
            }
        }

        // 'ChangeDeviceIdWithMerge' method in 'DeviceIdCountlyService'
        // We validate whether consent is remaining or not after changing the device id without merging.
        // Device Id should change and consent should be there
        [Test]
        public async void ChangeDeviceIdWithMerge_Consent()
        {
            CountlyConfiguration configuration = new CountlyConfiguration(_appKey, _serverUrl)
                .SetRequiresConsent(true);
            
            configuration.GiveConsent(new Consents[] { Consents.Crashes, Consents.Events, Consents.Clicks, Consents.StarRating, Consents.Views, Consents.Users, Consents.Sessions, Consents.Push, Consents.RemoteConfig, Consents.Location, Consents.Feedback });
            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Consents);
            Assert.AreEqual(DeviceIdType.SDKGenerated, Countly.Instance.Device.DeviceIdType);

            string oldDeviceId = Countly.Instance.Device.DeviceId;
            // validate that request repo is clean before changing id
            Countly.Instance.RequestHelper._requestRepo.Clear();
            await Countly.Instance.Device.ChangeDeviceIdWithMerge("new_device_id");
            //RQ will have begin session and end session requests
            Assert.AreEqual(1, Countly.Instance.Device._requestCountlyHelper._requestRepo.Count);
            Assert.AreEqual(DeviceIdType.DeveloperProvided, Countly.Instance.Device.DeviceIdType);
            Assert.AreEqual(Countly.Instance.Device.DeviceId, "new_device_id");

            Assert.IsTrue(Countly.Instance.Consents.RequiresConsent);
            Consents[] consents = System.Enum.GetValues(typeof(Consents)).Cast<Consents>().ToArray();
            foreach (Consents consent in consents) {
                Assert.IsTrue(Countly.Instance.Consents.CheckConsentInternal(consent));
            }
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

        // 'SetId' method in DeviceIdCountlyService
        // We provide empty, null, whitespace and valid ids to SetId method over DeveloperProvided id
        // Id should only change in case of a valid id
        [TestCase(DeviceIdType.DeveloperProvided, "", DeviceIdType.DeveloperProvided, false, false)]
        [TestCase(DeviceIdType.DeveloperProvided, null, DeviceIdType.DeveloperProvided, false, false)]
        [TestCase(DeviceIdType.DeveloperProvided, " ", DeviceIdType.DeveloperProvided, false, false)]
        [TestCase(DeviceIdType.DeveloperProvided, "new_user", DeviceIdType.DeveloperProvided, true, false)]
        [TestCase(DeviceIdType.SDKGenerated, "", DeviceIdType.SDKGenerated, false, false)]
        [TestCase(DeviceIdType.SDKGenerated, null, DeviceIdType.SDKGenerated, false, false)]
        [TestCase(DeviceIdType.SDKGenerated, " ", DeviceIdType.SDKGenerated, false, false)]
        [TestCase(DeviceIdType.SDKGenerated, "new_user", DeviceIdType.DeveloperProvided, true, true)]
        public void SetId(DeviceIdType startingType, string id, DeviceIdType expectedIdType, bool isExpectingChange, bool isExpectingMerge)
        {
            CountlyConfiguration config = new CountlyConfiguration(_appKey, _serverUrl);
            if (startingType == DeviceIdType.DeveloperProvided) {
                config = TestUtility.CreateBaseConfig();
            }
            Countly.Instance.Init(config);
            string deviceID = Countly.Instance.Device.DeviceId; // either becomes "test_user" or a id generated by sdk
            ValidateDeviceIDAndType(Countly.Instance, deviceID, startingType, true);
            Countly.Instance.Device.SetId(id);
            if (isExpectingChange) {
                deviceID = id;
            }
            ValidateDeviceIDAndType(Countly.Instance, deviceID, expectedIdType, true);
            CountlyRequestModel[] requests = Countly.Instance.Device._requestCountlyHelper._requestRepo.Models.ToArray();
            Assert.AreEqual(isExpectingMerge, ValidateMergeOccured(requests));
        }

        // 'SetId' method in DeviceIdCountlyService
        // We provide the same device id that SDK generated 
        // Id should remain same and type shouldn't change
        [TestCase(DeviceIdType.SDKGenerated, false)]
        [TestCase(DeviceIdType.DeveloperProvided, false)]
        public void SetId_SameId(DeviceIdType startType, bool isExpectingMerge)
        {
            CountlyConfiguration config = new CountlyConfiguration(_appKey, _serverUrl);
            if (startType == DeviceIdType.DeveloperProvided) {
                config = TestUtility.CreateBaseConfig();
            }
            Countly.Instance.Init(config);
            DeviceIdType currentType = startType;
            string deviceId = Countly.Instance.Device.DeviceId;
            ValidateDeviceIDAndType(Countly.Instance, deviceId, currentType, true);
            Countly.Instance.Device.SetId(deviceId);
            ValidateDeviceIDAndType(Countly.Instance, deviceId, currentType, true);
            CountlyRequestModel[] requests = Countly.Instance.Device._requestCountlyHelper._requestRepo.Models.ToArray();
            Assert.AreEqual(isExpectingMerge, ValidateMergeOccured(requests));
        }

        // 'SetId' method in DeviceIdCountlyService
        // We call SetId method twice over the SDK generated id
        // Id should change correctly both times and type should change into DeveloperProvided
        [Test]
        public void SetId_DoubleCall()
        {
            Countly.Instance.Init(new CountlyConfiguration(_appKey, _serverUrl));
            string deviceId = Countly.Instance.Device.DeviceId;
            DeviceIdType currentType = DeviceIdType.SDKGenerated;
            ValidateDeviceIDAndType(Countly.Instance, deviceId, currentType, true);

            string newId1 = "newId1";
            string newId2 = "newId2";

            Countly.Instance.Device.SetId(newId1);
            currentType = DeviceIdType.DeveloperProvided;
            ValidateDeviceIDAndType(Countly.Instance, newId1, currentType, true);
            Countly.Instance.Device.SetId(newId2);
            ValidateDeviceIDAndType(Countly.Instance, newId2, currentType, true);

            CountlyRequestModel[] requests = Countly.Instance.Device._requestCountlyHelper._requestRepo.Models.ToArray();
            Assert.AreEqual(true, ValidateMergeOccured(requests));
        }

        private bool ValidateMergeOccured(CountlyRequestModel[] requestModels)
        {
            foreach (CountlyRequestModel item in requestModels) {
                if (item.RequestData.Contains("old_device_id")) {
                    return true;
                }
            }
            return false;
        }

        private void CloseDBConnectionAndDestroyInstance(bool clearStorage = false)
        {
            if (clearStorage) {
                PlayerPrefs.DeleteAll();
            }

            Countly.Instance.CloseDBConnection();
            Object.DestroyImmediate(Countly.Instance);
        }

        [SetUp]
        [TearDown]
        public void End()
        {
            TestUtility.TestCleanup();
        }
    }
}
