using System;

namespace Notifications.Impls
{
	public class ProxyNotificationsService : INotificationsService
	{
		private readonly INotificationsService _service;

		public ProxyNotificationsService()
		{
#if UNITY_EDITOR
			_service = new EditorNotificationsService();
#elif UNITY_ANDROID
			_service = new Notifications.Impls.Android.AndroidNotificationsService();
#endif
		}

		public void GetToken(Action<string> result) => _service.GetToken(result);
	}
}