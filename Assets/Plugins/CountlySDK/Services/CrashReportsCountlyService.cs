﻿using System.Collections.Generic;
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
        internal bool IsApplicationInBackground { get; set; }
        private readonly Queue<string> _crashBreadcrumbs = new Queue<string>();
        private readonly CountlyConfiguration _configModel;
        private readonly RequestCountlyHelper _requestCountlyHelper;

        internal CrashReportsCountlyService(CountlyConfiguration configModel, RequestCountlyHelper requestCountlyHelper, ConsentCountlyService consentService) : base(consentService)
        {
            _configModel = configModel;
            _requestCountlyHelper = requestCountlyHelper;
        }


        /// <summary>
        /// Called when there is an exception 
        /// </summary>
        /// <param name="message">Exception Class</param>
        /// <param name="stackTrace">Stack Trace</param>
        /// <param name="type">The type of log message e.g error, warning, Exception etc</param>
        public async void LogCallback(string message, string stackTrace, LogType type)
        {
            if (!_consentService.CheckConsent(Consents.Crashes)) {
                return;
            }

            if (_configModel.EnableAutomaticCrashReporting
                && (type == LogType.Error || type == LogType.Exception)) {
                await SendCrashReportAsync(message, stackTrace, type, null, false);
            }
        }

        /// <summary>
        /// Public method that sends crash details to the server. Set param "nonfatal" to true for Custom Logged errors
        /// </summary>
        /// <param name="message">a string that contain detailed description of the exception.</param>
        /// <param name="stackTrace">a string that describes the contents of the callstack.</param>
        /// <param name="type">the type of the log message</param>
        /// <param name="segments">custom key/values to be reported</param>
        /// <param name="nonfatal">Fof automatically captured errors, you should set to <code>false</code>, whereas on logged errors it should be <code>true</code></param>
        /// <returns></returns>
        public async Task SendCrashReportAsync(string message, string stackTrace, LogType type,
            IDictionary<string, object> segments = null, bool nonfatal = true)
        {
            if (!_consentService.CheckConsent(Consents.Crashes)) {
                return;
            }

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
        /// <param name="value">a bread crumb for the crash report</param>
        public void AddBreadcrumbs(string value)
        {
            if (!_consentService.CheckConsent(Consents.Crashes)) {
                return;
            }

            if (_configModel.EnableConsoleLogging) {
                Debug.Log("[Countly] AddBreadcrumbs : " + value);
            }

            if (_configModel.EnableTestMode) {
                return;
            }

            string validBreadcrumb = value.Length > 1000 ? value.Substring(0, 1000) : value;

            if (_crashBreadcrumbs.Count == _configModel.TotalBreadcrumbsAllowed) {
                _crashBreadcrumbs.Dequeue();
            }

            _crashBreadcrumbs.Enqueue(value);
        }

        /// <summary>
        /// Create an CountlyExceptionDetailModel object from parameters.
        /// </summary>
        /// <param name="message">a string that contain detailed description of the exception.</param>
        /// <param name="stackTrace">a string that describes the contents of the callstack.</param>
        /// <param name="nonfatal">for automatically captured errors, you should set to <code>false</code>, whereas on logged errors it should be <code>true</code></param>
        /// <param name="segments">custom key/values to be reported</param>
        /// <returns>CountlyExceptionDetailModel</returns>
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