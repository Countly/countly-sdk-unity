using System.Collections.Generic;
using Plugins.Countly.Models;
using UnityEngine;

namespace Notifications
{
    public class NotificationsCallbackService 
    {
        CountlyConfigModel _config;
        private List<INotificationListener> _listners;
        internal NotificationsCallbackService(CountlyConfigModel config)
        {
            _config = config;
            _listners = new List<INotificationListener>();
        }

        public void AddListener(INotificationListener listener)
        {
            if (_listners.Contains(listener)) {
                return;
            }

            _listners.Add(listener);

            if (_config.EnableConsoleErrorLogging)
            {
                Debug.Log("[Countly NotificationsCallbackServcie] AddListener: " + listener);
            }
        }

        public void RemoveListener(INotificationListener listener)
        {
            _listners.Remove(listener);

            if (_config.EnableConsoleErrorLogging)
            {
                Debug.Log("[Countly NotificationsCallbackServcie] RemoveListener: " + listener);
            }
        }

        public void SendMessageToListeners(string data)
        {
            foreach (INotificationListener listener in _listners)
            {
                if (listener != null)
                {
                    listener.OnReceive(data);
                }
            }

            if (_config.EnableConsoleErrorLogging)
            {
                Debug.Log("[Countly NotificationsCallbackServcie] SendMessageToListeners: " + data);
            }
        }
    }

    public interface INotificationListener
    {
        void OnReceive(string message);
    }
}