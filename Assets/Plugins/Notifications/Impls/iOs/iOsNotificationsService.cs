using Newtonsoft.Json.Linq;
using Plugins.Countly.Helpers;
using Plugins.Countly.Services;
using System;
using System.Collections;
using System.Threading.Tasks;
#if UNITY_IOS
using Unity.Notifications.iOS;
#endif
using UnityEngine;

namespace Notifications.Impls.iOs
{
    public class IOsNotificationsService : INotificationsService
    {
        private readonly Action<IEnumerator> _startCoroutine;
        private readonly IEventCountlyService _eventCountlyService;

        public IOsNotificationsService(Action<IEnumerator> startCoroutine, IEventCountlyService eventCountlyService)
        {
            _startCoroutine = startCoroutine;
            _eventCountlyService = eventCountlyService;
        }

        public void GetMessage(Action result)
        {
            result.Invoke();    
        }

        public void GetToken(Action<string> result)
        {
            _startCoroutine.Invoke(RequestAuthorization(result));
        }

        private IEnumerator RequestAuthorization(Action<string> result)
        {
            Debug.Log("[IOsNotificationsService] RequestAuthorization");
#if UNITY_IOS
            using (var req = new AuthorizationRequest(AuthorizationOption.Alert | AuthorizationOption.Badge, true))
            {
                while (!req.IsFinished)
                {
                    yield return null;
                }
                
                var res = "\n RequestAuthorization: \n";
                res += "\n finished: " + req.IsFinished;
                res += "\n granted :  " + req.Granted;
                res += "\n error:  " + req.Error;
                res += "\n deviceToken:  " + req.DeviceToken;
                Debug.Log(res);
                
                result.Invoke(req.DeviceToken);
            }
#else
            Debug.Log("[Countly] IOsNotificationsService, RequestAuthorization, execution will be skipped, Unity.Notification.iOS exists only on IOS platform");
            yield return null;
#endif
        }

        public async Task<CountlyResponse> ReportPushActionAsync()
        {
            return await Task.FromResult(new CountlyResponse());
        }
    }
}