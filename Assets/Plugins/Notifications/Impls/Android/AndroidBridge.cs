using Newtonsoft.Json.Linq;
using Plugins.Countly.Models;
using System;
using UnityEngine;

namespace Notifications.Impls.Android
{
	public class AndroidBridge : MonoBehaviour
	{
        private Action<string> _onTokenResult;
        private Action<string> _OnNotificationReceiveResult;
        private Action<string, int> _OnNotificationClickResult;

        public CountlyConfigModel Config { get; set; }

        public void ListenTokenResult(Action<string> result) => _onTokenResult = result;
        public void ListenReceiveResult(Action<string> result) => _OnNotificationReceiveResult = result;
        public void ListenClickResult(Action<string, int> result) => _OnNotificationClickResult = result;
        

		public void OnTokenResult(string token)
		{
			_onTokenResult?.Invoke(token);
            if(Config.EnableConsoleErrorLogging)
            {
                Debug.Log("[Countly] AndroidBridge Firebase token: " + token);
            }
			
		}

        public void OnNotificationReceived(string data) {
            _OnNotificationReceiveResult?.Invoke(data);
            if (Config.EnableConsoleErrorLogging)
            {
                Debug.Log("[CountlyAndroidBridge] onMessageReceived");
            }
        }

        public void OnNotificationClicked(string data)
        {
            int index = 0;

            JObject jObject = JObject.Parse(data);

            if (jObject != null)
            {
                index = (int)jObject.GetValue("click_index");
            }
                _OnNotificationClickResult?.Invoke(data, index);
            if (Config.EnableConsoleErrorLogging)
            {
                Debug.Log("[CountlyAndroidBridge] OnNotificationClicked");
            }
        }
    }
}