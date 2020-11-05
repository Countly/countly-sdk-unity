using System.Collections.Generic;
using Plugins.CountlySDK.Models;
using UnityEngine;

namespace Notifications
{
    public class NotificationsCallbackService 
    {
        CountlyConfigModel _config;
        private List<INotificationListener> _listeners;
        internal NotificationsCallbackService(CountlyConfigModel config)
        {
            _config = config;
            _listeners = new List<INotificationListener>();
        }

        /// <summary>
        /// Add Notification listener into list.
        /// </summary>
        /// <param name="listener"></param>
        public void AddListener(INotificationListener listener)
        {
            if (_listeners.Contains(listener)) {
                return;
            }

            _listeners.Add(listener);

            if (_config.EnableConsoleLogging)
            {
                Debug.Log("[Countly NotificationsCallbackService] AddListener: " + listener);
            }
        }
        /// <summary>
        /// Remove Notification listener from list.
        /// </summary>
        /// <param name="listener"></param>
        public void RemoveListener(INotificationListener listener)
        {
            _listeners.Remove(listener);

            if (_config.EnableConsoleLogging)
            {
                Debug.Log("[Countly NotificationsCallbackService] RemoveListener: " + listener);
            }
        }

        /// <summary>
        /// Triger listener's Notification Received event with payload of push notification.
        /// </summary>
        /// <param name="data"></param>
        public void NotifyOnNotificationReceived(string data)
        {
            foreach (INotificationListener listener in _listeners)
            {
                if (listener != null)
                {
                    listener.OnNotificationReceived(data);
                }
            }

            if (_config.EnableConsoleLogging)
            {
                Debug.Log("[Countly NotificationsCallbackService] SendMessageToListeners: " + data);
            }
        }

        /// <summary>
        /// Triger listener's Notification Clicked event with payload of push notification and action index.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="index"></param>
        public void NotifyOnNotificationClicked(string data, int index)
        {
            foreach (INotificationListener listener in _listeners)
            {
                if (listener != null)
                {
                    listener.OnNotificationClicked(data, index);
                }
            }

            if (_config.EnableConsoleLogging)
            {
                Debug.Log("[Countly NotificationsCallbackService] SendMessageToListeners: " + data);
            }
        }
    }

    public interface INotificationListener
    {
        void OnNotificationReceived(string message);
        void OnNotificationClicked(string message, int index);
    }
}