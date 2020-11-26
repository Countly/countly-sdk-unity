using System;
using System.Collections;
using System.Threading.Tasks;
using Plugins.CountlySDK.Helpers;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Services;

namespace Notifications.Impls
{
	public class ProxyNotificationsService : INotificationsService
	{
		private readonly INotificationsService _service;
        private readonly EventCountlyService _eventCountlyService;

        internal ProxyNotificationsService(CountlyConfiguration config, Action<IEnumerator> startCoroutine, EventCountlyService eventCountlyService)
		{

#if UNITY_ANDROID
            _service = new Notifications.Impls.Android.AndroidNotificationsService(config, eventCountlyService);
#elif UNITY_IOS
			_service = new Notifications.Impls.iOs.IOsNotificationsService(config, startCoroutine, eventCountlyService);
#endif
        }


        public void GetToken(Action<string> result)
        {
            if (_service != null)
            {
                _service.GetToken(result);
            }
            
        }

        public void OnNotificationClicked(Action<string, int> result)
        {
            if (_service != null)
            {
                _service.OnNotificationClicked(result);
            }
        }


        public void OnNotificationReceived(Action<string> result)
        {
            if (_service != null)
            {
                _service.OnNotificationReceived(result);
            }
        }

        public async Task<CountlyResponse> ReportPushActionAsync()
        {
            if (_service != null)
            {
                return await _service.ReportPushActionAsync();
            }

            return new CountlyResponse
            {
                IsSuccess = true,
            };
        }
    }
}