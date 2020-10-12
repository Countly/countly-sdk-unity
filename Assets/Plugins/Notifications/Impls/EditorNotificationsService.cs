using System;
using UnityEngine;
using System.Threading.Tasks;
using Plugins.Countly.Helpers;

namespace Notifications.Impls
{
	public class EditorNotificationsService : INotificationsService
	{
      
        public void GetToken(Action<string> result) => result.Invoke("FakeToken");

        public void OnNoticicationClicked(Action<string, int> result)
        {
            Debug.Log("[EditorNotificationsService] OnNoticicationClicked");
            result.Invoke("Fake OnNoticicationClicked", 0);
        }

        public void OnNotificationReceived(Action<string> result)
        {
            Debug.Log("[EditorNotificationsService] OnNotificationReceived");
            result.Invoke("Fake OnNotificationReceived");
        }

        public Task<CountlyResponse> ReportPushActionAsync()
        {
            Debug.Log("[EditorNotificationsService] ReportPushActionAsync");
            return Task.FromResult(new CountlyResponse());           
        }
    }
}