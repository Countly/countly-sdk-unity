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

        private readonly int DEVICE_TYPE_FALLBACK_VALUE = -1;


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
        /// Returns the type Device ID that is currently used by the SDK
        /// </summary>
        public DeviceIdType DeviceIdType { get; private set; }

        /// <summary>
        /// Initialize <code>DeviceId</code> field with device id provided in configuration or with Random generated Id and Cache it.
        /// </summary>
        /// <param name="deviceId">new device id provided in configuration</param>
        internal void InitDeviceId(string deviceId = null)
        {
            //**Priority is**
            //Cached DeviceID (remains even after app kill)
            //Static DeviceID (only when the app is running or in the background)
            //User provided DeviceID
            //Generate Random DeviceID
            string storedDeviceId = PlayerPrefs.GetString(Constants.DeviceIDKey);
            if (!_countlyUtils.IsNullEmptyOrWhitespace(storedDeviceId)) {
                DeviceId = storedDeviceId;
                int storedDIDType = PlayerPrefs.GetInt(Constants.DeviceIDType, DEVICE_TYPE_FALLBACK_VALUE);
                if (storedDIDType == DEVICE_TYPE_FALLBACK_VALUE) {
                    Log.Error("[DeviceIdCountlyService] InitDeviceId: SDK doesn't have device ID type stored. There should have been one.");

                    if (!_countlyUtils.IsNullEmptyOrWhitespace(deviceId)) {
                        DeviceIdType = DeviceIdType.DeveloperProvided;
                    } else {
                        DeviceIdType = DeviceIdType.SDKGenerated;
                    }
                    PlayerPrefs.SetInt(Constants.DeviceIDType, (int)DeviceIdType);
                } else {
                    DeviceIdType = (DeviceIdType)storedDIDType;
                }
            } else {
                if (_countlyUtils.IsNullEmptyOrWhitespace(DeviceId)) {
                    if (!_countlyUtils.IsNullEmptyOrWhitespace(deviceId)) {
                        DeviceId = deviceId;
                        DeviceIdType = DeviceIdType.DeveloperProvided;
                        PlayerPrefs.SetInt(Constants.DeviceIDType, (int)DeviceIdType);
                    } else {
                        DeviceId = _countlyUtils.GetUniqueDeviceId();
                        DeviceIdType = DeviceIdType.SDKGenerated;
                        PlayerPrefs.SetInt(Constants.DeviceIDType, (int)DeviceIdType);
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

                //Ignore call if new and old device id are same
                if (DeviceId == deviceId) {
                    return;
                }

                //Add currently recorded events to request queue
                _eventCountlyService.AddEventsToRequestQueue();

                //Cancel all timed events
                _eventCountlyService.CancelAllTimedEvents();

                //Ends current session
                //Do not dispose timer object
                if (!_configuration.IsAutomaticSessionTrackingDisabled) {
                    _ = _sessionCountlyService.EndSessionAsync();
                }

                //Update device id
                UpdateDeviceId(deviceId);

                if (_consentService.RequiresConsent) {
                    _consentService.SetConsentInternal(_consentService.CountlyConsents.Keys.ToArray(), false, sendRequest: false, ConsentChangedAction.DeviceIDChangedNotMerged);
                }

                //Begin new session with new device id
                //Do not initiate timer again, it is already initiated
                if (!_configuration.IsAutomaticSessionTrackingDisabled) {
                    _ = _sessionCountlyService.BeginSessionAsync();
                }

                NotifyListeners(false);

                _ = _requestCountlyHelper.ProcessQueue();
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

            DeviceIdType = DeviceIdType.DeveloperProvided;
            PlayerPrefs.SetInt(Constants.DeviceIDType, (int)DeviceIdType);

            //Updating Cache
            PlayerPrefs.SetString(Constants.DeviceIDKey, DeviceId);

            Log.Debug("[DeviceIdCountlyService] UpdateDeviceId: " + newDeviceId);
        }

        /// <summary>
        /// Call <code>DeviceIdChanged</code> on all listeners.
        /// </summary>
        /// <param name="merged">If passed "true" if will perform a device ID merge server side of the old and new device ID. This will merge their data</param>
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
        #endregion
    }
}
