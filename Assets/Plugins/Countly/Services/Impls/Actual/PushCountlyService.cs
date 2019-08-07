using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Notifications;
using Plugins.Countly.Enums;
using Plugins.Countly.Helpers;
using UnityEngine;

namespace Plugins.Countly.Services.Impls.Actual
{
    public class PushCountlyService
    {
        private readonly RequestCountlyHelper _requestCountlyHelper;
        private readonly INotificationsService _notificationsService;
        private TestMode? _mode;
        private string _token;

        private static bool _tokenSent;

        public PushCountlyService(RequestCountlyHelper requestCountlyHelper, INotificationsService notificationsService)
        {
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

        [Serializable]
        struct PushActionSegment
        {
            [JsonProperty("b")]
            internal string Identifier { get; set; }
            [JsonProperty("i")]
            internal string MessageID { get; set; }
        }

    }
}