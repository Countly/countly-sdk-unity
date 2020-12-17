using System.Collections.Generic;
using System.Threading.Tasks;
using Plugins.CountlySDK.Helpers;
using UnityEngine;

namespace Plugins.CountlySDK.Services
{
    public class DeviceIdCountlyService
    {

        private readonly SessionCountlyService _sessionCountlyService;
        private readonly RequestCountlyHelper _requestCountlyHelper;
        private readonly EventCountlyService _eventCountlyService;
        private readonly CountlyUtils _countlyUtils;

        internal DeviceIdCountlyService(SessionCountlyService sessionCountlyService, 
            RequestCountlyHelper requestCountlyHelper, EventCountlyService eventCountlyService, CountlyUtils countlyUtils)
        {
            _sessionCountlyService = sessionCountlyService;
            _requestCountlyHelper = requestCountlyHelper;
            _eventCountlyService = eventCountlyService;
            _countlyUtils = countlyUtils;
        }

        public string DeviceId { get; private set; }

        internal void InitDeviceId(string deviceId = null)
        {
            //**Priority is**
            //Cached DeviceID (remains even after after app kill)
            //Static DeviceID (only when the app is running or in the background)
            //User provided DeviceID
            //Generate Random DeviceID
            string storedDeviceId = PlayerPrefs.GetString("DeviceID");
            DeviceId = !_countlyUtils.IsNullEmptyOrWhitespace(storedDeviceId)
                ? storedDeviceId
                : !_countlyUtils.IsNullEmptyOrWhitespace(DeviceId)
                    ? DeviceId
                    : !_countlyUtils.IsNullEmptyOrWhitespace(deviceId)
                        ? deviceId : _countlyUtils.GetUniqueDeviceId();
            
            //Set DeviceID in Cache if it doesn't already exists in Cache
            if (_countlyUtils.IsNullEmptyOrWhitespace(storedDeviceId))
            {
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
        public async Task<CountlyResponse> ChangeDeviceIdAndEndCurrentSessionAsync(string deviceId)
        {
            //Ignore call if new and old device id are same
            if (DeviceId == deviceId) {
                return new CountlyResponse { IsSuccess = true };
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

            return new CountlyResponse { IsSuccess = true };
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
            if (DeviceId == deviceId)
            {
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

    }
}