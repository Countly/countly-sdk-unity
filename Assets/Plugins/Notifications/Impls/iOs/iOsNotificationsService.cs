using Plugins.CountlySDK.Helpers;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Services;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Notifications.Impls.iOs
{
    public class IOsNotificationsService : INotificationsService
    {
        private readonly Transform _countlyGameObject;
        private readonly CountlyConfiguration _config;
        private readonly Action<IEnumerator> _startCoroutine;
        private readonly EventCountlyService _eventCountlyService;

        private readonly IOSBridage _bridge;
        private const string BridgeName = "[iOS] Bridge";

        internal IOsNotificationsService(Transform countlyGameObject, CountlyConfiguration config, Action<IEnumerator> startCoroutine, EventCountlyService eventCountlyService)
        {
            _config = config;
            _startCoroutine = startCoroutine;
            _countlyGameObject = countlyGameObject;
            _eventCountlyService = eventCountlyService;

            GameObject gameObject = new GameObject(BridgeName);
            gameObject.transform.parent = _countlyGameObject;

            _bridge = gameObject.AddComponent<IOSBridage>();
            _bridge.Config = _config;

        }

        public void GetToken(Action<string> result)
        {
            // _startCoroutine.Invoke(RequestAuthorization(result));
            _bridge.ListenTokenResult(result);
            _bridge.GetToken();
        }

        public async Task<CountlyResponse> ReportPushActionAsync()
        {
            string mesageId = _bridge.MessageId;
            string identifier = _bridge.ButtonIndex;

            if (_bridge.MessageId != null) {
                PushCountlyService.PushActionSegment segment =
                    new Plugins.CountlySDK.Services.PushCountlyService.PushActionSegment {
                        MessageID = mesageId,
                        Identifier = identifier
                    };

                if (_config.EnableConsoleLogging) {
                    Debug.Log("[Countly] ReportPushActionAsync key: " + CountlyEventModel.PushActionEvent + ", segments: " + segment);
                }

                await _eventCountlyService.ReportCustomEventAsync(
                    CountlyEventModel.PushActionEvent, segment.ToDictionary());
            }

            _bridge.MessageId = null;
            _bridge.ButtonIndex = null;

            return new CountlyResponse {
                IsSuccess = true,
            };
        }

        public void OnNotificationClicked(Action<string, int> result)
        {
            if (_config.EnableConsoleLogging) {
                Debug.Log("[Countly] OnNotificationClicked register");
            }

            _bridge.ListenClickResult(result);

        }

        public void OnNotificationReceived(Action<string> result)
        {
            if (_config.EnableConsoleLogging) {
                Debug.Log("[Countly] OnNotificationReceived register");
            }

            _bridge.ListenReceiveResult(result);

        }


    }
}