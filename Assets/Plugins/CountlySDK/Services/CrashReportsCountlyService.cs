using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Plugins.CountlySDK.Helpers;
using Plugins.CountlySDK.Models;
using UnityEngine;

namespace Plugins.CountlySDK.Services
{
    public class CrashReportsCountlyService
    {
        private readonly Queue<string> _crashBreadcrumbs = new Queue<string>();
        private readonly CountlyConfigModel _configModel;
        private readonly RequestCountlyHelper _requestCountlyHelper;

        internal CrashReportsCountlyService(CountlyConfigModel configModel, RequestCountlyHelper requestCountlyHelper)
        {
            _configModel = configModel;
            _requestCountlyHelper = requestCountlyHelper;
        }


        /// <summary>
        /// Called when there is an exception 
        /// </summary>
        /// <param name="message">Exception Class</param>
        /// <param name="stackTrace">Stack Trace</param>
        /// <param name="type">Excpetion type like error, warning, etc</param>
        public async void LogCallback(string message, string stackTrace, LogType type)
        {
            if (_configModel.EnableAutomaticCrashReporting
                && (type == LogType.Error || type == LogType.Exception))
            {
                await SendCrashReportAsync(message, stackTrace, type, null, false);
            }
        }

        /// <summary>
        /// Public method that sends crash details to the server. Set param "nonfatal" to true for Custom Logged errors
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stackTrace"></param>
        /// <param name="type"></param>
        /// <param name="segments"></param>
        /// <param name="nonfatal"></param>
        /// <returns></returns>
        public async Task<CountlyResponse> SendCrashReportAsync(string message, string stackTrace, LogType type,
            IDictionary<string, object> segments = null, bool nonfatal = true)
        {
            //if (ConsentModel.CheckConsent(FeaturesEnum.Crashes.ToString()))
            //{
            var model = CountlyExceptionDetailModel.ExceptionDetailModel;
            model.Error = stackTrace;
            model.Name = message;
            model.Nonfatal = nonfatal;
            model.Custom = segments as Dictionary<string, object>;
            model.Logs = string.Join("\n", _crashBreadcrumbs);
#if UNITY_IOS
            model.Manufacture = UnityEngine.iOS.Device.generation.ToString();
#endif
#if UNITY_ANDROID
            model.Manufacture = SystemInfo.deviceModel;
#endif
            var requestParams = new Dictionary<string, object>
            {
                {
                    "crash", JsonConvert.SerializeObject(model, Formatting.Indented,
                        new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore})
                }
            };

            return await _requestCountlyHelper.GetResponseAsync(requestParams);
            //}
        }

        /// <summary>
        /// Adds string value to a list which is later sent over as logs whenever a cash is reported by system.
        /// The length of a breadcrumb is limited to 1000 characters. Only first 1000 characters will be accepted in case the length is more 
        /// than 1000 characters.
        /// </summary>
        /// <param name="value"></param>
        public void AddBreadcrumbs(string value)
        {
            if (_configModel.EnableConsoleLogging)
            {
                Debug.Log("[Countly] AddBreadcrumbs : " + value);
            }

            if (_configModel.EnableTestMode)
            {
                return;
            }

            var validBreadcrumb = value.Length > 1000 ? value.Substring(0, 1000) : value;

            if (_crashBreadcrumbs.Count == _configModel.TotalBreadcrumbsAllowed)
                _crashBreadcrumbs.Dequeue();

            _crashBreadcrumbs.Enqueue(value);
        }
    }
}