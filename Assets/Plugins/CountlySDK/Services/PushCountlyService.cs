using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Notifications;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Helpers;
using Plugins.CountlySDK.Models;

namespace Plugins.CountlySDK.Services
{
    public class PushCountlyService : AbstractBaseService
    {
        private string _token;
        private TestMode? _mode;
        private bool _isDeviceRegistered;
        private readonly CountlyConfiguration _configuration;
        private readonly EventCountlyService _eventCountlyService;
        private readonly RequestCountlyHelper _requestCountlyHelper;
        private readonly INotificationsService _notificationsService;
        private readonly NotificationsCallbackService _notificationsCallbackService;

        internal PushCountlyService(CountlyConfiguration configuration, EventCountlyService eventCountlyService, RequestCountlyHelper requestCountlyHelper, INotificationsService notificationsService, NotificationsCallbackService notificationsCallbackService, ConsentCountlyService consentService) : base(consentService)
        {
            _configuration = configuration;
            _eventCountlyService = eventCountlyService;
            _requestCountlyHelper = requestCountlyHelper;
            _notificationsService = notificationsService;
            _notificationsCallbackService = notificationsCallbackService;
        }

        private void EnableNotification()
        {
            //Enables push notification on start
            if (_configuration.EnableTestMode || !_consentService.CheckConsent(Consents.Push) || _configuration.NotificationMode == TestMode.None) {
                return;
            }

            EnablePushNotificationAsync(_configuration.NotificationMode);
        }

        /// <summary>
        /// Registers device for receiving Push Notifications
        /// </summary>
        /// <param name="mode">Application mode</param>
        private void EnablePushNotificationAsync(TestMode mode)
        {
            _mode = mode;
            _isDeviceRegistered = true;
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
            if (!_consentService.CheckConsent(Consents.Push)) {
                return new CountlyResponse { IsSuccess = false};
            }

            return await _notificationsService.ReportPushActionAsync();
        }

        #region override Methods
        internal override void OnInitializationComplete()
        {
            EnableNotification();
        }

        internal override void DeviceIdChanged(string deviceId, bool merged)
        {

        }

        internal override void ConsentChanged(List<Consents> updatedConsents, bool newConsentValue)
        {
            if (updatedConsents.Contains(Consents.Push) && newConsentValue && !_isDeviceRegistered) {
                EnableNotification();
            }
        }
        #endregion

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