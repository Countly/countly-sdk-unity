using System;
using System.Collections;

namespace Notifications.Impls
{
	public class ProxyNotificationsService : INotificationsService
	{
		private readonly INotificationsService _service;

		public ProxyNotificationsService(Action<IEnumerator> startCoroutine)
		{
#if UNITY_EDITOR
			_service = new EditorNotificationsService();
#elif UNITY_ANDROID
			_service = new Notifications.Impls.Android.AndroidNotificationsService();
#elif UNITY_IOS
			_service = new Notifications.Impls.iOs.IOsNotificationsService(startCoroutine);
#endif
		}

		public void GetToken(Action<string> result) => _service.GetToken(result);
	}
}