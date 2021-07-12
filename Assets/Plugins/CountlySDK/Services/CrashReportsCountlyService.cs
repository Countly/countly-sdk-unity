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
        internal bool IsApplicationInBackground { get; set; }
        internal readonly Queue<string> _crashBreadcrumbs = new Queue<string>();

        internal readonly RequestCountlyHelper _requestCountlyHelper;

        internal CrashReportsCountlyService(CountlyConfiguration configuration, CountlyLogHelper logHelper, RequestCountlyHelper requestCountlyHelper, ConsentCountlyService consentService) : base(configuration, logHelper, consentService)
        {
            Log.Debug("[CrashReportsCountlyService] Initializing.");
            _requestCountlyHelper = requestCountlyHelper;
        }


        /// <summary>
        /// Called when there is an exception 
        /// </summary>
        /// <param name="message">Exception Class</param>
        /// <param name="stackTrace">Stack Trace</param>
        /// <param name="type">The type of log message e.g error, warning, Exception etc</param>
        [Obsolete("LogCallback is deprecated, this is going to be removed in the future.")]
        public async void LogCallback(string message, string stackTrace, LogType type)
        {
            lock (LockObj) {
                //In future make this function internal
                if (!_consentService.CheckConsentInternal(Consents.Crashes)) {
                    return;
                }

                if (string.IsNullOrEmpty(message) || string.IsNullOrWhiteSpace(message)) {
                    Log.Warning("[CrashReportsCountlyService] LogCallback : The parameter 'message' can't be null or empty");
                    return;
                }
                CountlyExceptionDetailModel model = ExceptionDetailModel(message, stackTrace, false, null);

                if (_configuration.EnableAutomaticCrashReporting
                    && (type == LogType.Error || type == LogType.Exception)) {
                    _=SendCrashReportInternal(model);
                }
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
            lock (LockObj) {
                Log.Info("[CrashReportsCountlyService] SendCrashReportAsync : message = " + message + ", stackTrace = " + stackTrace);

                if (!_consentService.CheckConsentInternal(Consents.Crashes)) {
                    return;
                }

                if (string.IsNullOrEmpty(message) || string.IsNullOrWhiteSpace(message)) {
                    Log.Warning("[CrashReportsCountlyService] SendCrashReportAsync : The parameter 'message' can't be null or empty");
                    return;
                }


                IDictionary<string, object> segmentation = null;
                if (segments != null) {
                    List<string> toRemove = new List<string>();

                    segmentation = new Dictionary<string, object>();
                    foreach (KeyValuePair<string, object> item in segments) {
                        string k = item.Key;
                        object v = item.Value;

                        if (k.Length > _configuration.MaxKeyLength) {
                            Log.Verbose("[EventCountlyService] ReportCustomEventAsync : Max allowed key length is " + _configuration.MaxKeyLength);
                            k = k.Substring(0, _configuration.MaxKeyLength);
                        }

                        if (v.GetType() == typeof(string) && ((string)v).Length > _configuration.MaxValueSize) {
                            Log.Verbose("[EventCountlyService] ReportCustomEventAsync : Max allowed value length is " + _configuration.MaxValueSize);
                            v = ((string)v).Substring(0, _configuration.MaxValueSize);
                        }

                        segmentation.Add(k, v);
                    }
                }

                    CountlyExceptionDetailModel model = ExceptionDetailModel(message, stackTrace, nonfatal, segmentation);
                _=SendCrashReportInternal(model);
            }

        }

        internal async Task SendCrashReportInternal(CountlyExceptionDetailModel model)
        {
            Log.Debug("[CrashReportsCountlyService] SendCrashReportInternal : model = " + model.ToString());

            Dictionary<string, object> requestParams = new Dictionary<string, object>
            {
                {
                    "crash", JsonConvert.SerializeObject(model, Formatting.Indented,
                        new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore})
                }
            };

            _requestCountlyHelper.AddToRequestQueue(requestParams);
            await _requestCountlyHelper.ProcessQueue();

        }

        /// <summary>
        /// Adds string value to a list which is later sent over as logs whenever a cash is reported by system.
        /// The length of a breadcrumb is limited to 1000 characters. Only first 1000 characters will be accepted in case the length is more 
        /// than 1000 characters.
        /// </summary>
        /// <param name="value">a bread crumb for the crash report</param>
        public void AddBreadcrumbs(string value)
        {
            Log.Info("[CrashReportsCountlyService] AddBreadcrumbs : " + value);

            if (!_consentService.CheckConsentInternal(Consents.Crashes)) {
                return;
            }

            if (_configuration.EnableTestMode) {
                return;
            }

            string validBreadcrumb = value.Length > _configuration.MaxValueSize ? value.Substring(0, _configuration.MaxValueSize) : value;

            if (_crashBreadcrumbs.Count == _configuration.TotalBreadcrumbsAllowed) {
                _crashBreadcrumbs.Dequeue();
            }

            _crashBreadcrumbs.Enqueue(validBreadcrumb);
        }

        /// <summary>
        /// Create an CountlyExceptionDetailModel object from parameters.
        /// </summary>
        /// <param name="message">a string that contain detailed description of the exception.</param>
        /// <param name="stackTrace">a string that describes the contents of the callstack.</param>
        /// <param name="nonfatal">for automatically captured errors, you should set to <code>false</code>, whereas on logged errors it should be <code>true</code></param>
        /// <param name="segments">custom key/values to be reported</param>
        /// <returns>CountlyExceptionDetailModel</returns>
        internal CountlyExceptionDetailModel ExceptionDetailModel(string message, string stackTrace, bool nonfatal, IDictionary<string, object> segments)
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
