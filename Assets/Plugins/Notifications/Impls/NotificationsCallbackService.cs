using System.Collections.Generic;
using Plugins.Countly.Enums;
using Plugins.Countly.Models;
using Plugins.Countly.Services;
using UnityEngine;

namespace Notifications
{
    public class NotificationsCallbackServcie : INotificationsCallbackServcie
    {
        CountlyConfigModel _config;
        private Dictionary<int, INotificationListener> _listners;
        internal NotificationsCallbackServcie(CountlyConfigModel config)
        {
            _config = config;
            _listners = new Dictionary<int, INotificationListener>();

            CheckNotificationMode();

        }

        public void AddListener(int instanceId, INotificationListener listener)
        {
            if (_listners.ContainsKey(instanceId)) {
                return;
            }

            CheckNotificationMode();
            _listners.Add(instanceId, listener);
            Debug.Log("[Countly NotificationsCallbackServcie] AddListener: " + instanceId);
        }

        public void RemoveListener(int instanceId)
        {
            CheckNotificationMode();
            _listners.Remove(instanceId);
            Debug.Log("[Countly NotificationsCallbackServcie] RemoveListener: " + instanceId);
        }

        public void SendMessageToListeners(string data)
        {
            foreach (INotificationListener listener in _listners.Values)
            {

                if (listener != null)
                {
                    listener.OnReceive(data);
                }
            }

            Debug.Log("[Countly NotificationsCallbackServcie] SendMessageToListeners: " + data);
        }

        private void CheckNotificationMode()
        {
            if (_config.NotificationMode == TestMode.None)
            {
                Debug.Log("[Countly] NotificationsCallbackServcie: Notifiations are disabled");
            }
        }
    }

    public interface INotificationListener
    {
        void OnReceive(string message);
    }
}