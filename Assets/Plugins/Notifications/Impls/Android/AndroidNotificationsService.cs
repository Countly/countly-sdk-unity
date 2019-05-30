using System;
using UnityEngine;

namespace Notifications.Impls.Android
{
	public class AndroidNotificationsService : INotificationsService
	{
		private const string PackageName = "com.playdarium.services.notifications.RemoteNotificationsService";
		private const string BridgeName = "[Android] Bridge";

		private readonly AndroidBridge _bridge;

		public AndroidNotificationsService()
		{
			var gameObject = new GameObject(BridgeName);
			_bridge = gameObject.AddComponent<AndroidBridge>();
		}

		public void GetToken(Action<string> result)
		{
			_bridge.ListenTokenResult(result);

			using (var jc = new AndroidJavaObject(PackageName))
				jc.Call("getToken");
		}
	}
}