using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Notifications;
using Plugins.Countly.Enums;
using Plugins.Countly.Helpers;
using Plugins.Countly.Models;
using UnityEngine;

namespace Plugins.Countly.Services.Impls.Actual
{
    public class PushCountlyService
    {
        private readonly IEventCountlyService _eventCountlyService;
        private readonly RequestCountlyHelper _requestCountlyHelper;
        private readonly INotificationsService _notificationsService;

        private string _token;
        private TestMode? _mode;
        private static bool _tokenSent;

        public PushCountlyService(IEventCountlyService eventCountlyService, RequestCountlyHelper requestCountlyHelper, INotificationsService notificationsService)
        {
            _eventCountlyService = eventCountlyService;
            _requestCountlyHelper = requestCountlyHelper;
            _notificationsService = notificationsService;
        }

        /// <summary>
        /// Registers device for receiving Push Notifications
        /// </summary>
        /// <param name="mode">Application mode</param>
        internal void EnablePushNotificationAsync(TestMode mode)
        {
            _mode = mode;
            _notificationsService.GetToken(result =>
            {
                _token = result;
                Debug.Log("[PushCountlyService], token: " + _token);
            });
            
        }
        
        internal async void Update()
        {
            /*
             * When the push notification service gets enabled successfully for the device, 
             * we send a request to the Countly server that the user is ready to receive push notifications.
             * Update method is called multiple times during a particular scene,
             * therefore we send this request to the Countly server only once
             */
            if(string.IsNullOrEmpty(_token)) return;

            if(_tokenSent) return;
            
            //Enabling the User to receive Push Notifications
            _tokenSent = true;
            await PostToCountlyAsync(_mode, _token);
        }
        
        /// <summary>
        /// Notifies Countly that the device is capable of receiving Push Notifications
        /// </summary>
        /// <returns></returns>
        private async Task<CountlyResponse> PostToCountlyAsync(TestMode? mode, string token)
        {
            if (!_mode.HasValue)
            {
                return new CountlyResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Pushes are disabled."
                };

            }

            var requestParams =
                new Dictionary<string, object>
                {
                    { "token_session", 1 },
                    { "test_mode", (int)mode.Value },
                    { $"{Constants.UnityPlatform}_token", token },
                };
            return await _requestCountlyHelper.GetResponseAsync(requestParams);
        }

        public async Task<CountlyResponse> ReportPushActionAsync(string mesageId, string identifier = "0")
        {
            Debug.Log("[Countly] ReportPushActionAsync, mesageId: " + mesageId);
            var segment =
                new PushActionSegment
                {
                    MessageID = mesageId,
                    Identifier = identifier
                };

            return await _eventCountlyService.ReportCustomEventAsync(
                CountlyEventModel.PushActionEvent, segment.ToDictionary());
        }

        [Serializable]
        struct PushActionSegment
        {
            public string Identifier { get; set; }
            public string MessageID { get; set; }

            public IDictionary<string, object> ToDictionary()
            {
                return new Dictionary<string, object>()
                {
                    {"b", Identifier},
                    {"i", MessageID}
                };
            }
        }

    }
}