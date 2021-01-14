using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Notifications;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Helpers;
namespace Plugins.CountlySDK.Services
{
    public class PushCountlyService : AbstractBaseService
    {
        private string _token;
        private TestMode? _mode;
        private readonly EventCountlyService _eventCountlyService;
        private readonly RequestCountlyHelper _requestCountlyHelper;
        private readonly INotificationsService _notificationsService;
        private readonly NotificationsCallbackService _notificationsCallbackService;

        internal PushCountlyService(EventCountlyService eventCountlyService, RequestCountlyHelper requestCountlyHelper, INotificationsService notificationsService, NotificationsCallbackService notificationsCallbackService)
        {
            _eventCountlyService = eventCountlyService;
            _requestCountlyHelper = requestCountlyHelper;
            _notificationsService = notificationsService;
            _notificationsCallbackService = notificationsCallbackService;
        }

        /// <summary>
        /// Registers device for receiving Push Notifications
        /// </summary>
        /// <param name="mode">Application mode</param>
        internal void EnablePushNotificationAsync(TestMode mode)
        {
            _mode = mode;
            _notificationsService.GetToken(async result => {
                _token = result;
                /*
                 * When the push notification service gets enabled successfully for the device, 
                 * we send a request to the Countly server that the user is ready to receive push notifications.
               */
                await PostToCountlyAsync(_mode, _token);
                await ReportPushActionAsync();
            });

            _notificationsService.OnNotificationClicked(async (data, index) => {
                _notificationsCallbackService.NotifyOnNotificationClicked(data, index);
                await ReportPushActionAsync();
            });

            _notificationsService.OnNotificationReceived(data => {
                _notificationsCallbackService.NotifyOnNotificationReceived(data);
            });

        }

        /// <summary>
        /// Notifies Countly that the device is capable of receiving Push Notifications
        /// </summary>
        /// <returns></returns>
        private async Task PostToCountlyAsync(TestMode? mode, string token)
        {
            if (!_mode.HasValue) {
                return;
            }

            Dictionary<string, object> requestParams =
                new Dictionary<string, object>
                {
                    { "token_session", 1 },
                    { "test_mode", (int)mode.Value },
                    { $"{Constants.UnityPlatform}_token", token },
                };

            await _requestCountlyHelper.GetResponseAsync(requestParams);
        }

        /// <summary>
        /// Report Push Actions stored in local cache to Countly server.,
        /// </summary>
        private async Task<CountlyResponse> ReportPushActionAsync()
        {
            return await _notificationsService.ReportPushActionAsync();
        }

        [Serializable]
        public struct PushActionSegment
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