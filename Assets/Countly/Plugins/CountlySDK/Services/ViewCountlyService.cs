using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Helpers;
using Plugins.CountlySDK.Models;
using UnityEngine;

namespace Plugins.CountlySDK.Services
{
    public class ViewCountlyService : AbstractBaseService, IViewModule, IViewIDProvider
    {
        private class ViewData
        {
            public string ViewID;
            public long ViewStartTimeSeconds; // If this is 0, the view is not started yet or was paused
            public string ViewName;
            public bool IsAutoStoppedView; // Views started with "startAutoStoppedView" would have this as "true".
            public bool IsAutoPaused; // This marks that this view automatically paused when going to the background
            public Dictionary<string, object> ViewSegmentation;
        }

        private string currentViewID;
        private string previousViewID;
        private readonly string viewEventKey = "[CLY]_view";

        readonly Dictionary<string, ViewData> viewDataMap = new Dictionary<string, ViewData>();

        internal bool _isFirstView = true;
        internal readonly EventCountlyService _eventService;
        internal readonly Countly _cly;
        internal readonly CountlyUtils _utils;
        internal ISafeIDGenerator safeViewIDGenerator;

        private readonly Dictionary<string, DateTime> _viewToLastViewStartTime = new Dictionary<string, DateTime>();

        internal ViewCountlyService(Countly countly, CountlyUtils utils, CountlyConfiguration configuration, CountlyLogHelper logHelper, EventCountlyService eventService, ConsentCountlyService consentService) : base(configuration, logHelper, consentService)
        {
            Log.Debug("[ViewCountlyService] Initializing.");
            _cly = countly;
            _utils = utils;
            eventService.viewIDProvider = this;
            _eventService = eventService;
            safeViewIDGenerator = configuration.SafeViewIDGenerator;
        }

        #region PublicAPI
        /// <summary>
        /// Returns the current ViewID
        /// </summary>
        public string GetCurrentViewId()
        {
            return currentViewID == null ? "" : currentViewID;
        }

        /// <summary>
        /// Returns the previous ViewID
        /// </summary>
        public string GetPreviousViewId()
        {
            return previousViewID == null ? "" : previousViewID;
        }
        #endregion

        #region Deprecated Methods
        /// <summary>
        /// Start tracking a view
        /// </summary>
        /// <param name="name">name of the view</param>
        /// <returns></returns>
        [Obsolete("RecordOpenViewAsync(string name, IDictionary<string, object> segmentation = null) is deprecated, this is going to be removed in the future.")]
        public async Task RecordOpenViewAsync(string name, IDictionary<string, object> segmentation = null)
        {
            lock (LockObj) {
                Log.Info("[ViewCountlyService] RecordOpenViewAsync : name = " + name);

                if (!_consentService.CheckConsentInternal(Consents.Views)) {
                    Log.Debug("[ViewCountlyService] RecordOpenViewAsync, consent is not given, ignoring the request.");
                    return;
                }

                if (string.IsNullOrEmpty(name)) {
                    return;
                }

                if (name.Length > _configuration.MaxKeyLength) {
                    Log.Verbose("[ViewCountlyService] RecordOpenViewAsync : Max allowed key length is " + _configuration.MaxKeyLength);
                    name = name.Substring(0, _configuration.MaxKeyLength);
                }

                IDictionary<string, object> openViewSegment = new Dictionary<string, object>
                {
                    {"name", name},
                    {"segment", _configuration.metricHelper.OS},
                    {"visit", 1},
                    {"start", _isFirstView ? 1 : 0}
                };

                if (segmentation != null) {
                    segmentation = RemoveSegmentInvalidDataTypes(segmentation);
                    segmentation = FixSegmentKeysAndValues(segmentation);

                    foreach (KeyValuePair<string, object> item in openViewSegment) {
                        segmentation[item.Key] = item.Value;
                    }
                } else {
                    segmentation = openViewSegment;
                }

                if (!_viewToLastViewStartTime.ContainsKey(name)) {
                    _viewToLastViewStartTime.Add(name, DateTime.UtcNow);
                }

                ViewData currentViewData = new ViewData();
                currentViewData.ViewID = safeViewIDGenerator.GenerateValue();
                currentViewData.ViewName = name;
                currentViewData.ViewStartTimeSeconds = _utils.CurrentTimestampSeconds();
                currentViewData.IsAutoStoppedView = false;

                viewDataMap.Add(currentViewData.ViewID, currentViewData);
                previousViewID = currentViewID;
                currentViewID = currentViewData.ViewID;

                CountlyEventModel currentView = new CountlyEventModel(viewEventKey, segmentation, 1, null, null, currentViewData.ViewID, previousViewID, currentViewID);
                _ = _eventService.RecordEventAsync(currentView);

                _isFirstView = false;
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Stop tracking a view
        /// </summary>
        /// <param name="name of the view"></param>
        /// <returns></returns>
        [Obsolete("RecordCloseViewAsync(string name) is deprecated, this is going to be removed in the future.")]
        public async Task RecordCloseViewAsync(string name)
        {
            lock (LockObj) {
                Log.Info("[ViewCountlyService] RecordCloseViewAsync : name = " + name);

                if (!_consentService.CheckConsentInternal(Consents.Views)) {
                    Log.Debug("[ViewCountlyService] RecordCloseViewAsync, consent is not given, ignoring the request.");
                    return;
                }

                if (string.IsNullOrEmpty(name)) {
                    return;
                }

                if (name.Length > _configuration.MaxKeyLength) {
                    Log.Verbose("[ViewCountlyService] RecordCloseViewAsync : Max allowed key length is " + _configuration.MaxKeyLength);
                    name = name.Substring(0, _configuration.MaxKeyLength);
                }

                double? duration = null;
                if (_viewToLastViewStartTime.ContainsKey(name)) {
                    DateTime lastViewStartTime = _viewToLastViewStartTime[name];
                    duration = (DateTime.UtcNow - lastViewStartTime).TotalSeconds;

                    _viewToLastViewStartTime.Remove(name);
                }

                IDictionary<string, object> segment = new Dictionary<string, object>
                {
                    {"name", name},
                    {"segment", _configuration.metricHelper.OS},
                };

                CountlyEventModel currentView = new CountlyEventModel(CountlyEventModel.ViewEvent, segment, 1, null, duration);
                _ = _eventService.RecordEventAsync(currentView);
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Reports a particular action with the specified details
        /// </summary>
        /// <param name="type"> type of action</param>
        /// <param name="x">x-coordinate</param>
        /// <param name="y">y-coordinate</param>
        /// <param name="width">width of screen</param>
        /// <param name="height">height of screen</param>
        /// <returns></returns>
        [Obsolete("ReportActionAsync(string type, int x, int y, int width, int height) is deprecated, this is going to be removed in the future.")]
        public async Task ReportActionAsync(string type, int x, int y, int width, int height)
        {
            lock (LockObj) {
                Log.Info("[ViewCountlyService] ReportActionAsync : type = " + type + ", x = " + x + ", y = " + y + ", width = " + width + ", height = " + height);

                if (!_consentService.CheckConsentInternal(Consents.Views)) {
                    Log.Debug("[ViewCountlyService] ReportActionAsync, consent is not given, ignoring the request.");
                    return;
                }

                IDictionary<string, object> segmentation = new Dictionary<string, object>()
                {
                    {"type", type},
                    {"x", x},
                    {"y", y},
                    {"width", width},
                    {"height", height},
                };
                CountlyEventModel currentView = new CountlyEventModel(CountlyEventModel.ViewActionEvent, segmentation);
                _ = _eventService.RecordEventAsync(currentView);
            }
            await Task.CompletedTask;
        }

        #region override Methods
        internal override void DeviceIdChanged(string deviceId, bool merged)
        {
            if (!merged) {
                _isFirstView = true;
            }
        }
        #endregion

        #endregion
    }
}
