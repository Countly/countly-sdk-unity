using System.Collections.Generic;
using System.Linq;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Services;
using UnityEngine;

namespace Notifications
{
    public class NotificationsCallbackService : AbstractBaseService
    {
        private readonly List<INotificationListener> _listeners;
        internal NotificationsCallbackService(CountlyConfiguration configuration, CountlyLogHelper logHelper, ConsentCountlyService consentService) : base(configuration, logHelper, consentService)
        {
            Log.Debug("[NotificationsCallbackService] Initializing.");
            _listeners = configuration.NotificationEventListeners.Distinct().ToList();
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

            Log.Debug("[NotificationsCallbackService] AddListener: " + listener);
        }
        /// <summary>
        /// Remove Notification listener.
        /// </summary>
        /// <param name="listener"></param>
        public void RemoveListener(INotificationListener listener)
        {
            if (!_listeners.Contains(listener)) {
                return;
            }

            _listeners.Remove(listener);
            Log.Debug("[NotificationsCallbackService] RemoveListener: " + listener);

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

            Log.Debug("[NotificationsCallbackService] SendMessageToListeners: " + data);

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

            Log.Debug("[NotificationsCallbackService] SendMessageToListeners: " + data);
        }
    }

    public interface INotificationListener
    {
        void OnNotificationReceived(string message);
        void OnNotificationClicked(string message, int index);
    }
}