using System.Collections.Generic;
using UnityEngine;

namespace Notifications
{
    public class NotificationsCallbackService
    {
        private readonly CountlyLogHelper _logHelper;
        private readonly List<INotificationListener> _listeners;
        internal NotificationsCallbackService(CountlyLogHelper logHelper)
        {
            _logHelper = logHelper;
            _listeners = new List<INotificationListener>();
        }

        /// <summary>
        /// Add Notification listener.
        /// </summary>
        /// <param name="listener"></param>
        public void AddListener(INotificationListener listener)
        {
            if (_listeners.Contains(listener)) {
                return;
            }

            _listeners.Add(listener);

            _logHelper.Info("[Countly NotificationsCallbackService] AddListener: " + listener);
        }
        /// <summary>
        /// Remove Notification listener.
        /// </summary>
        /// <param name="listener"></param>
        public void RemoveListener(INotificationListener listener)
        {
            _listeners.Remove(listener);
            _logHelper.Info("[Countly NotificationsCallbackService] RemoveListener: " + listener);

        }

        /// <summary>
        /// Trigger listener's Notification Received event with payload of push notification.
        /// </summary>
        /// <param name="data"></param>
        internal void NotifyOnNotificationReceived(string data)
        {
            foreach (INotificationListener listener in _listeners) {
                if (listener != null) {
                    listener.OnNotificationReceived(data);
                }
            }

            _logHelper.Info("[Countly NotificationsCallbackService] SendMessageToListeners: " + data);

        }

        /// <summary>
        /// Trigger listener's Notification Clicked event with payload of push notification and action index.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="index"></param>
        internal void NotifyOnNotificationClicked(string data, int index)
        {
            foreach (INotificationListener listener in _listeners) {
                if (listener != null) {
                    listener.OnNotificationClicked(data, index);
                }
            }

            _logHelper.Info("[Countly NotificationsCallbackService] SendMessageToListeners: " + data);
        }
    }

    public interface INotificationListener
    {
        void OnNotificationReceived(string message);
        void OnNotificationClicked(string message, int index);
    }
}