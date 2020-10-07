using Newtonsoft.Json.Linq;
using Plugins.Countly.Helpers;
using Plugins.Countly.Models;
using Plugins.Countly.Services;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Notifications.Impls.Android
{
	public class AndroidNotificationsService : INotificationsService
	{
        private const string BridgeName = "[Android] Bridge";
        private const string StorePackageName = "ly.count.unity.push_fcm.MessageStore";
        private const string PackageName = "ly.count.unity.push_fcm.RemoteNotificationsService";

		private readonly AndroidBridge _bridge;
        private readonly IEventCountlyService _eventCountlyService;
        private readonly NotificationsCallbackService _notificationsCallbackServcie;

        public AndroidNotificationsService(IEventCountlyService eventCountlyService, NotificationsCallbackService notificationsCallbackServcie)
		{
            _eventCountlyService = eventCountlyService;
            _notificationsCallbackServcie = notificationsCallbackServcie;

            var gameObject = new GameObject(BridgeName);
			_bridge = gameObject.AddComponent<AndroidBridge>();
		}

        public void GetMessage(Action result)
        {
            _bridge.ListenMessageResult(result);
        }

        public void GetToken(Action<string> result)
		{
			_bridge.ListenTokenResult(result);
			
			using (var jc = new AndroidJavaObject(PackageName))
            {
                jc.Call("getToken");
            }
        }

        public async Task<CountlyResponse> ReportPushActionAsync()
        {
            AndroidJavaClass store = new AndroidJavaClass(StorePackageName);

            bool isInitialized = store.CallStatic<bool>("isInitialized");
            if (!isInitialized)
            {
                AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject applicationContext = activity.Call<AndroidJavaObject>("getApplicationContext");

                store.CallStatic("init", applicationContext);
            }

            string data = store.CallStatic<string>("getMessagesData");
            if (string.IsNullOrEmpty(data))
            {
                return new CountlyResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Key is required."
                };
            }

            JArray jArray = JArray.Parse(data);

            if (jArray != null)
            {
                foreach (JObject item in jArray)
                {
                    string mesageId = item.GetValue("messageId").ToString();
                    string identifier = item.GetValue("action_index").ToString();

                    var segment =
                    new Plugins.Countly.Services.Impls.Actual.PushCountlyService.PushActionSegment
                    {
                        MessageID = mesageId,
                        Identifier = identifier
                    };

                    Debug.Log("[Countly] ReportPushActionAsync key: " + CountlyEventModel.PushActionEvent + ", segments: " + segment);

                    await _eventCountlyService.ReportCustomEventAsync(
                        CountlyEventModel.PushActionEvent, segment.ToDictionary());
                }

                store.CallStatic("clearMessagesData");
                
            }

            _notificationsCallbackServcie.SendMessageToListeners(data);

            return new CountlyResponse
            {
                IsSuccess = true,
            };
        }
    }
}