using System;
using System.Collections;
using Unity.Notifications.iOS;
using UnityEngine;

namespace Notifications.Impls.iOs
{
    public class IOsNotificationsService : INotificationsService
    {
        private readonly Action<IEnumerator> _startCoroutine;

        public IOsNotificationsService(Action<IEnumerator> startCoroutine)
        {
            _startCoroutine = startCoroutine;
        }

        public void GetToken(Action<string> result)
        {
            _startCoroutine.Invoke(RequestAuthorization(result));
        }

        private IEnumerator RequestAuthorization(Action<string> result)
        {
            Debug.Log("[IOsNotificationsService] RequestAuthorization");
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
        }
    }
}