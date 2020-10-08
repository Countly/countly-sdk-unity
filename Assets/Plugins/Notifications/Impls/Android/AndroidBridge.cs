using Plugins.Countly.Models;
using System;
using UnityEngine;

namespace Notifications.Impls.Android
{
	public class AndroidBridge : MonoBehaviour
	{
        private Action _onMessageResult;
        private Action<string> _onTokenResult;
        public CountlyConfigModel Config { get; set; }


        public void ListenMessageResult(Action result) => _onMessageResult = result;
        public void ListenTokenResult(Action<string> result) => _onTokenResult = result;

		public void OnTokenResult(string token)
		{
			_onTokenResult?.Invoke(token);
            if(Config.EnableConsoleErrorLogging)
            {
                Debug.Log("[Countly] AndroidBridge Firebase token: " + token);
            }
			
		}

        public void onMessageReceived(string messageId) {
            _onMessageResult?.Invoke();
            if (Config.EnableConsoleErrorLogging)
            {
                Debug.Log("[CountlyAndroidBridge] onMessageReceived");
            }
        }
	}
}