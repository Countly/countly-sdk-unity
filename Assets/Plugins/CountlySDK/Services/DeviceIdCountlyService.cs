using System.Collections.Generic;
using System.Threading.Tasks;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Helpers;
using Plugins.CountlySDK.Models;
using UnityEngine;

namespace Plugins.CountlySDK.Services
{
    public class DeviceIdCountlyService : IBaseService
    {
        private List<IBaseService> _listeners;
        private readonly CountlyUtils _countlyUtils;
        private readonly CountlyConfiguration _config;
        private readonly SessionCountlyService _sessionCountlyService;
        private readonly RequestCountlyHelper _requestCountlyHelper;
        private readonly EventCountlyService _eventCountlyService;


        internal DeviceIdCountlyService(CountlyConfiguration config, SessionCountlyService sessionCountlyService,
            RequestCountlyHelper requestCountlyHelper, EventCountlyService eventCountlyService, CountlyUtils countlyUtils)
        {
            _config = config;
            _countlyUtils = countlyUtils;
            _eventCountlyService = eventCountlyService;
            _requestCountlyHelper = requestCountlyHelper;
            _sessionCountlyService = sessionCountlyService;
        }

        public string DeviceId { get; private set; }

        internal void AddListeners(List<IBaseService> listeners)
        {
            _listeners = listeners;
            if (_config.EnableConsoleLogging) {
                Debug.Log("[Countly DeviceIdCountlyService] AddListeners");
            }
        }

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
        /// <param name="deviceId"></param>
        public async Task ChangeDeviceIdAndEndCurrentSessionAsync(string deviceId)
        {
            //Ignore call if new and old device id are same
            if (DeviceId == deviceId) {
                return;
            }

            //Add currently recorded events to request queue-----------------------------------
            await _eventCountlyService.AddEventsToRequestQueue();

            //Ends current session
            //Do not dispose timer object
            await _sessionCountlyService.ExecuteEndSessionAsync(false);

            //Update device id
            UpdateDeviceId(deviceId);

            //Begin new session with new device id
            //Do not initiate timer again, it is already initiated
            await _sessionCountlyService.ExecuteBeginSessionAsync();
            NotifyListeners(false);
        }

        /// <summary>
        /// Changes DeviceId. 
        /// Continues with the current session.
        /// Merges data for old and new Device Id. 
        /// </summary>
        /// <param name="deviceId"></param>
        public async Task ChangeDeviceIdAndMergeSessionDataAsync(string deviceId)
        {
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
               new Dictionary<string, object>
               {
                        { "old_device_id", oldDeviceId }
               };

            await _requestCountlyHelper.GetResponseAsync(requestParams);
            NotifyListeners(true);
        }

        /// <summary>
        /// Updates Device ID both in app and in cache
        /// </summary>
        /// <param name="newDeviceId"></param>
        private void UpdateDeviceId(string newDeviceId)
        {
            //Change device id
            DeviceId = newDeviceId;

            //Updating Cache
            PlayerPrefs.SetString(Constants.DeviceIDKey, DeviceId);
        }

        private void NotifyListeners(bool merged)
        {
            if (_listeners == null) {
                return;
            }

            foreach (IBaseService listener in _listeners) {
                listener.DeviceIdChanged(DeviceId, merged);
            }
        }

        #region override Methods
        public void DeviceIdChanged(string deviceId, bool merged)
        {

        }

        public void ConsentChanged(Dictionary<Features, bool> updatedConsents)
        {

        }
        #endregion
    }
}