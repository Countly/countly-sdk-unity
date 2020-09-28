using System;
using UnityEngine;
using System.Threading.Tasks;
using Plugins.Countly.Helpers;

namespace Notifications.Impls
{
	public class EditorNotificationsService : INotificationsService
	{
        public void GetMessage(Action result)
        {
            Debug.Log("[EditorNotificationsService] GetMessage");
            result.Invoke();
        }

        public void GetToken(Action<string> result) => result.Invoke("FakeToken");

        public Task<CountlyResponse> ReportPushActionAsync()
        {
            Debug.Log("[EditorNotificationsService] ReportPushActionAsync");
            return Task.FromResult(new CountlyResponse());           
        }
    }
}