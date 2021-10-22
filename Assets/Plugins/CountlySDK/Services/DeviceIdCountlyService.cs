using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Helpers;
using Plugins.CountlySDK.Models;
using UnityEngine;

namespace Plugins.CountlySDK.Services
{
    public class DeviceIdCountlyService : AbstractBaseService
    {
        private readonly CountlyUtils _countlyUtils;
        private readonly EventCountlyService _eventCountlyService;
        internal readonly RequestCountlyHelper _requestCountlyHelper;
        private readonly SessionCountlyService _sessionCountlyService;

        internal DeviceIdCountlyService(CountlyConfiguration configuration, CountlyLogHelper logHelper, SessionCountlyService sessionCountlyService,
            RequestCountlyHelper requestCountlyHelper, EventCountlyService eventCountlyService, CountlyUtils countlyUtils, ConsentCountlyService consentService) : base(configuration, logHelper, consentService)
        {
            Log.Debug("[DeviceIdCountlyService] Initializing.");

            _countlyUtils = countlyUtils;
            _eventCountlyService = eventCountlyService;
            _requestCountlyHelper = requestCountlyHelper;
            _sessionCountlyService = sessionCountlyService;
        }
        /// <summary>
        /// Returns the Device ID that is currently used by the SDK
        /// </summary>
        public string DeviceId { get; private set; }

        /// <summary>
        /// Initialize <code>DeviceId</code> field with device id provided in configuration or with Randome generated Id and Cache it.
        /// </summary>
        /// <param name="deviceId">new device id provided in configuration</param>
        internal void InitDeviceId(string deviceId = null)
        {
            //**Priority is**
            //Cached DeviceID (remains even after after app kill)
            //Static DeviceID (only when the app is running or in the background)
            //User provided DeviceID
            //Generate Random DeviceID
            string storedDeviceId = PlayerPrefs.GetString("DeviceID");
            if (!_countlyUtils.IsNullEmptyOrWhitespace(storedDeviceId)) {
                DeviceId = storedDeviceId;
            } else {
                if (_countlyUtils.IsNullEmptyOrWhitespace(DeviceId)) {
                    if (!_countlyUtils.IsNullEmptyOrWhitespace(deviceId)) {
                        DeviceId = deviceId;
                    } else {
                        DeviceId = _countlyUtils.GetUniqueDeviceId();
                    }
                }
            }

            //Set DeviceID in Cache if it doesn't already exists in Cache
            if (_countlyUtils.IsNullEmptyOrWhitespace(storedDeviceId)) {
                PlayerPrefs.SetString(Constants.DeviceIDKey, DeviceId);
            }
        }

        /// <summary>
        /// Changes Device Id.
        /// Adds currently recorded but not queued events to request queue.
        /// Clears all started timed-events
        /// Ends current session with old Device Id.
        /// Begins a new session with new Device Id
        /// </summary>
        /// <param name="deviceId">new device id</param>
        [Obsolete("ChangeDeviceIdAndEndCurrentSessionAsync is deprecated, please use ChangeDeviceIdWithoutMerge method instead.")]
        public async Task ChangeDeviceIdAndEndCurrentSessionAsync(string deviceId)
        {
            Log.Info("[DeviceIdCountlyService] ChangeDeviceIdAndEndCurrentSessionAsync: deviceId = " + deviceId);

            if (!_consentService.AnyConsentGiven()) {
                Log.Debug("[DeviceIdCountlyService] ChangeDeviceIdAndEndCurrentSessionAsync: Please set at least a single consent before calling this!");
                return;
            }

            await ChangeDeviceIdWithoutMerge(deviceId);
        }

        /// <summary>
        /// Changes Device Id.
        /// Adds currently recorded but not queued events to request queue.
        /// Clears all started timed-events
        /// Ends current session with old Device Id.
        /// Begins a new session with new Device Id
        /// </summary>
        /// <param name="deviceId">new device id</param>
        public async Task ChangeDeviceIdWithoutMerge(string deviceId)
        {
            lock (LockObj) {
                Log.Info("[DeviceIdCountlyService] ChangeDeviceIdWithoutMerge: deviceId = " + deviceId);

                if (!_consentService.AnyConsentGiven()) {
                    Log.Debug("[DeviceIdCountlyService] ChangeDeviceIdWithoutMerge: Please set at least a single consent before calling this!");
                    return;
                }

                //Ignore call if new and old device id are same
                if (DeviceId == deviceId) {
                    return;
                }

                //Add currently recorded events to request queue-----------------------------------
                _eventCountlyService.AddEventsToRequestQueue();

                //Ends current session
                //Do not dispose timer object
                if (!_configuration.IsAutomaticSessionTrackingDisabled) {
                    _ = _sessionCountlyService.EndSessionAsync();
                }

                //Update device id
                UpdateDeviceId(deviceId);

                //Begin new session with new device id
                //Do not initiate timer again, it is already initiated
                if (!_configuration.IsAutomaticSessionTrackingDisabled) {
                    _ = _sessionCountlyService.BeginSessionAsync();
                }

                NotifyListeners(false);

                _ = _requestCountlyHelper.ProcessQueue();

                if (_consentService.RequiresConsent) {
                    _consentService.SetConsentInternal(_consentService.CountlyConsents.Keys.ToArray(), false);
                }
            }
        }

        /// <summary>
        /// Changes DeviceId.
        /// Continues with the current session.
        /// Merges data for old and new Device Id.
        /// </summary>
        /// <param name="deviceId">new device id</param>
        [Obsolete("ChangeDeviceIdAndMergeSessionDataAsync is deprecated, please use ChangeDeviceIdWithMerge method instead.")]
        public async Task ChangeDeviceIdAndMergeSessionDataAsync(string deviceId)
        {
            Log.Info("[DeviceIdCountlyService] ChangeDeviceIdAndMergeSessionDataAsync: deviceId = " + deviceId);

            if (!_consentService.AnyConsentGiven()) {
                Log.Debug("[DeviceIdCountlyService] ChangeDeviceIdAndMergeSessionDataAsync: Please set at least a single consent before calling this!");
                return;
            }

            await ChangeDeviceIdWithMerge(deviceId);
        }

        /// <summary>
        /// Changes DeviceId.
        /// Continues with the current session.
        /// Merges data for old and new Device Id.
        /// </summary>
        /// <param name="deviceId">new device id</param>
        public async Task ChangeDeviceIdWithMerge(string deviceId)
        {
            lock (LockObj) {
                Log.Info("[DeviceIdCountlyService] ChangeDeviceIdWithMerge: deviceId = " + deviceId);

                if (!_consentService.AnyConsentGiven()) {
                    Log.Debug("[DeviceIdCountlyService] ChangeDeviceIdWithMerge: Please set at least a single consent before calling this!");
                    return;
                }

                //Ignore call if new and old device id are same
                if (DeviceId == deviceId) {
                    return;
                }

                //Keep old device id
                string oldDeviceId = DeviceId;

                //Update device id
                UpdateDeviceId(deviceId);

                //Merge user data for old and new device
                Dictionary<string, object> requestParams =
                    new Dictionary<string, object> { { "old_device_id", oldDeviceId } };

                _requestCountlyHelper.AddToRequestQueue(requestParams);
                _ = _requestCountlyHelper.ProcessQueue();
                NotifyListeners(true);
            }
        }

        /// <summary>
        /// Updates Device ID both in app and in cache
        /// </summary>
        /// <param name="newDeviceId">new device id</param>
        private void UpdateDeviceId(string newDeviceId)
        {
            //Change device id
            DeviceId = newDeviceId;

            //Updating Cache
            PlayerPrefs.SetString(Constants.DeviceIDKey, DeviceId);

            Log.Debug("[DeviceIdCountlyService] UpdateDeviceId: " + newDeviceId);

        }

        /// <summary>
        /// Call <code>DeviceIdChanged</code> on all listeners.
        /// </summary>
        /// <param name="merged">If passed "true" if will perform a device ID merge serverside of the old and new device ID. This will merge their data</param>
        private void NotifyListeners(bool merged)
        {
            if (Listeners == null) {
                return;
            }

            foreach (AbstractBaseService listener in Listeners) {
                listener.DeviceIdChanged(DeviceId, merged);
            }
        }

        #region override Methods
        internal override void DeviceIdChanged(string deviceId, bool merged)
        {

        }

        internal override void ConsentChanged(List<Consents> updatedConsents, bool newConsentValue)
        {

        }
        #endregion
    }
}
