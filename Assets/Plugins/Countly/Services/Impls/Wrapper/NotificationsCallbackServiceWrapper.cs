
using UnityEngine;

namespace Plugins.Countly.Services.Impls.Wrapper
{
    public class NotificationsCallbackServiceWrapper
    {
     
        internal NotificationsCallbackServiceWrapper()
        {

        }

        public void AddListener(int instanceId, INotificationListener listener)
        {
            Debug.Log("[Countly NotificationsCallbackServiceWrapper] AddListener: " + instanceId);
        }

        public void AddListener(int instanceId, Notifications.INotificationListener listener)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveListener(int instanceId)
        {         
            Debug.Log("[Countly NotificationsCallbackServiceWrapper] RemoveListener: " + instanceId);
        }

        public void SendMessageToListeners(string data)
        {
           
            Debug.Log("[Countly NotificationsCallbackServiceWrapper] SendMessageToListeners: " + data);
        }
    }

    public interface INotificationListener
    {
        void OnReceive(string message);
    }
}