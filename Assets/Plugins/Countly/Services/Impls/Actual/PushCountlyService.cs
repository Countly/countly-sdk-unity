using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Notifications;
using Plugins.Countly.Enums;
using Plugins.Countly.Helpers;
using Plugins.Countly.Models;
using UnityEngine;

namespace Plugins.Countly.Services.Impls.Actual
{
    public class PushCountlyService
    {
        private string _token;
        private TestMode? _mode;
        private readonly IEventCountlyService _eventCountlyService;
        private readonly RequestCountlyHelper _requestCountlyHelper;
        private readonly INotificationsService _notificationsService;

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
            _notificationsService.GetToken(async result =>
            {
                _token = result;
                /*
                 * When the push notification service gets enabled successfully for the device, 
                 * we send a request to the Countly server that the user is ready to receive push notifications.
               */
                await PostToCountlyAsync(_mode, _token);
                await ReportPushActionAsync();
            });

            _notificationsService.GetMessage(async () =>
            {
                await ReportPushActionAsync();
            });

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

            return await _requestCountlyHelper.GetResponseAsync(requestParams, true);
        }

        /// <summary>
        /// Report Push Actions stored in local cache to Countly server.,
        /// </summary>
        public async Task<CountlyResponse> ReportPushActionAsync()
        {
            const string StorePackageName = "ly.count.unity.push_fcm.MessageStore";
            AndroidJavaClass store = new AndroidJavaClass(StorePackageName);

            bool isInitialized = store.CallStatic<bool>("isInitialized");
            if (!isInitialized)
            {
                AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject applicationContext = activity.Call<AndroidJavaObject>("getApplicationContext");

                store.CallStatic("init", applicationContext);
            }

            string data = store.CallStatic<string>("getMessagesData");
            if (string.IsNullOrEmpty(data))
            {
                return new CountlyResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Key is required."
                };
            }

            JArray jArray = JArray.Parse(data);

            if (jArray != null)
            {
                foreach (JObject item in jArray)
                {
                    string mesageId = item.GetValue("messageId").ToString();
                    string identifier = item.GetValue("action_index").ToString();

                    var segment =
                    new PushActionSegment
                    {
                        MessageID = mesageId,
                        Identifier = identifier
                    };

                    await _eventCountlyService.ReportCustomEventAsync(
                        CountlyEventModel.PushActionEvent, segment.ToDictionary());
                }

                store.CallStatic("clearMessagesData");
            }

            return new CountlyResponse
            {
                IsSuccess = true,
            };
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