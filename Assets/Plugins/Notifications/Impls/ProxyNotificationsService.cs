using System;
using System.Collections;
using System.Threading.Tasks;
using Plugins.Countly.Helpers;
using Plugins.Countly.Services;

namespace Notifications.Impls
{
	public class ProxyNotificationsService : INotificationsService
	{
		private readonly INotificationsService _service;
        private readonly IEventCountlyService _eventCountlyService;

        public ProxyNotificationsService(Action<IEnumerator> startCoroutine, IEventCountlyService eventCountlyService)
		{

#if UNITY_EDITOR
            _service = new EditorNotificationsService();
#elif UNITY_ANDROID
            _service = new Notifications.Impls.Android.AndroidNotificationsService(eventCountlyService);
#elif UNITY_IOS
			_service = new Notifications.Impls.iOs.IOsNotificationsService(startCoroutine, eventCountlyService);

#endif
        }

        public void GetMessage(Action result) => _service.GetMessage(result);

        public void GetToken(Action<string> result) => _service.GetToken(result);

        public async Task<CountlyResponse> ReportPushActionAsync()
        {
            return await _service.ReportPushActionAsync();
        }
    }
}