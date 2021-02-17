using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Helpers;
using Plugins.CountlySDK.Models;
using UnityEngine;

namespace Plugins.CountlySDK.Services
{
    public class CrashReportsCountlyService : AbstractBaseService
    {
        public bool IsApplicationInBackground { get; internal set; }
        private readonly Queue<string> _crashBreadcrumbs = new Queue<string>();

        private readonly CountlyConfiguration _configuration;
        private readonly RequestCountlyHelper _requestCountlyHelper;

        internal CrashReportsCountlyService(CountlyConfiguration configuration, CountlyLogHelper logHelper, RequestCountlyHelper requestCountlyHelper, ConsentCountlyService consentService) : base(logHelper, consentService)
        {
            Log.Debug("[CrashReportsCountlyService] Initializing.");

            _configuration = configuration;
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
            if (!_consentService.CheckConsent(Consents.Crashes)) {
                return;
            }

            Log.Info("[CrashReportsCountlyService] LogCallback : message = " + message + ", stackTrace = " + stackTrace);


            if (_configuration.EnableAutomaticCrashReporting
                && (type == LogType.Error || type == LogType.Exception)) {
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
        public async Task SendCrashReportAsync(string message, string stackTrace, LogType type,
            IDictionary<string, object> segments = null, bool nonfatal = true)
        {
            if (!_consentService.CheckConsent(Consents.Crashes)) {
                return;
            }

            Log.Info("[CrashReportsCountlyService] LogCallback : message = " + message + ", stackTrace = " + stackTrace);

            CountlyExceptionDetailModel model = ExceptionDetailModel(message, stackTrace, nonfatal, segments);

            Dictionary<string, object> requestParams = new Dictionary<string, object>
            {
                {
                    "crash", JsonConvert.SerializeObject(model, Formatting.Indented,
                        new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore})
                }
            };

            await _requestCountlyHelper.GetResponseAsync(requestParams);

        }

        /// <summary>
        /// Adds string value to a list which is later sent over as logs whenever a cash is reported by system.
        /// The length of a breadcrumb is limited to 1000 characters. Only first 1000 characters will be accepted in case the length is more 
        /// than 1000 characters.
        /// </summary>
        /// <param name="value"></param>
        public void AddBreadcrumbs(string value)
        {
            if (!_consentService.CheckConsent(Consents.Crashes)) {
                return;
            }

            Log.Info("[CrashReportsCountlyService] AddBreadcrumbs : " + value);

            if (_configuration.EnableTestMode) {
                return;
            }

            string validBreadcrumb = value.Length > 1000 ? value.Substring(0, 1000) : value;

            if (_crashBreadcrumbs.Count == _configuration.TotalBreadcrumbsAllowed) {
                _crashBreadcrumbs.Dequeue();
            }

            _crashBreadcrumbs.Enqueue(value);
        }

        private CountlyExceptionDetailModel ExceptionDetailModel(string message, string stackTrace, bool nonfatal, IDictionary<string, object> segments)
        {
            return new CountlyExceptionDetailModel {
                OS = Constants.UnityPlatform,
                OSVersion = SystemInfo.operatingSystem,
                Device = SystemInfo.deviceName,
                Resolution = Screen.currentResolution.ToString(),
                AppVersion = Application.version,
                Cpu = SystemInfo.processorType,
                Opengl = SystemInfo.graphicsDeviceVersion,
                RamTotal = SystemInfo.systemMemorySize.ToString(),
                Battery = SystemInfo.batteryLevel.ToString(),
                Orientation = Screen.orientation.ToString(),
                Online = (Application.internetReachability > 0).ToString(),

                Name = message,
                Error = stackTrace,
                Nonfatal = nonfatal,
                RamCurrent = null,
                DiskCurrent = null,
                DiskTotal = null,
                Muted = null,
                Background = IsApplicationInBackground.ToString(),
                Root = null,
                Logs = string.Join("\n", _crashBreadcrumbs),
                Custom = segments as Dictionary<string, object>,
                Run = Time.realtimeSinceStartup.ToString(),
#if UNITY_IOS
                Manufacture = UnityEngine.iOS.Device.generation.ToString()
#endif
#if UNITY_ANDROID
                Manufacture = SystemInfo.deviceModel
#endif
            };
        }
        #region override Methods
        internal override void DeviceIdChanged(string deviceId, bool merged)
        {

        }
        #endregion
    }
}