using System;
using UnityEngine;

namespace Notifications.Impls.Android
{
	public class AndroidBridge : MonoBehaviour
	{
		private Action<string> _onTokenResult;

		public void ListenTokenResult(Action<string> result) => _onTokenResult = result;

		public void OnTokenResult(string token)
		{
			_onTokenResult?.Invoke(token);
			UnityEngine.Debug.Log("[AndroidBridge] Firebase token: " + token);
		}
	}
}