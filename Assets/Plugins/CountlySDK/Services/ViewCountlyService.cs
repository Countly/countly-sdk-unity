using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Helpers;
using Plugins.CountlySDK.Models;
using UnityEngine;

namespace Plugins.CountlySDK.Services
{
    // interface for SDK users
    public interface IViewCountlyService
    {
        string GetCurrentViewId();
        string GetPreviousViewId();
        string? StartView(string? viewName);
        string? StartView(string? viewName, Dictionary<string, object>? viewSegmentation);
        void StopViewWithName(string? viewName);
        void StopViewWithName(string? viewName, Dictionary<string, object>? viewSegmentation);
        void StopViewWithID(string? viewID);
        void StopViewWithID(string? viewID, Dictionary<string, object>? viewSegmentation);
        void PauseViewWithID(string? viewID);
        void ResumeViewWithID(string? viewID);
        void StopAllViews(Dictionary<string, object> viewSegmentation);
        void SetGlobalViewSegmentation(Dictionary<string, object> viewSegmentation);
        void AddSegmentationToViewWithID(string? viewID, Dictionary<string, object>? viewSegmentation);
        void AddSegmentationToViewWithName(string? viewName, Dictionary<string, object>? viewSegmentation);
    }

    public class ViewCountlyService : AbstractBaseService, IViewCountlyService
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
        readonly Dictionary<string, object> automaticViewSegmentation = new Dictionary<string, object>();

        internal bool _isFirstView = true;
        internal readonly EventCountlyService _eventService;
        internal readonly Countly _cly;
        internal readonly CountlyUtils _utils;

        readonly string[] reservedSegmentationKeysViews = { "name", "visit", "start", "segment" };
        private readonly Dictionary<string, DateTime> _viewToLastViewStartTime = new Dictionary<string, DateTime>();

        internal ViewCountlyService(Countly countly, CountlyUtils utils, CountlyConfiguration configuration, CountlyLogHelper logHelper, EventCountlyService eventService, ConsentCountlyService consentService) : base(configuration, logHelper, consentService)
        {
            Log.Debug("[ViewCountlyService] Initializing.");
            _cly = countly;
            _utils = utils;
            _eventService = eventService;
        }

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

        /// <summary>
        /// Starts a view which would not close automatically
        /// </summary>
        /// <param name="viewName"></param>
        /// <returns></returns>
        public string? StartView(string? viewName)
        {
            lock (LockObj) {
                Log.Info("[ViewCountlyService] Calling StartView vn[" + viewName + "]");

                return StartViewInternal(viewName, null, false);
            }
        }

        /// <summary>
        /// Starts a view which would not close automatically
        /// </summary>
        /// <param name="viewName">name of the view</param>
        /// <param name="viewSegmentation">segmentation that will be added to the view, set 'null' if none should be added</param>
        /// <returns></returns>
        public string? StartView(string? viewName, Dictionary<string, object>? viewSegmentation)
        {
            lock (LockObj) {
                Log.Info("[ViewCountlyService] Calling StartView vn[" + viewName + "] sg[" + (viewSegmentation == null ? "null" : viewSegmentation.Count.ToString()) + "]");

                return StartViewInternal(viewName, viewSegmentation, false);
            }
        }

        /// <summary>
        /// Stops a view with the given name if it was open
        /// </summary>
        /// <param name="viewName">name of the view</param>
        public void StopViewWithName(string? viewName)
        {
            lock (LockObj) {
                Log.Info("[ViewCountlyService] Calling StopViewWithName vn[" + viewName + "]");

                StopViewWithNameInternal(viewName, null);
            }
        }

        /// <summary>
        /// Stops a view with the given name if it was open
        /// </summary>
        /// <param name="viewName">name of the view</param>
        /// <param name="viewSegmentation">view segmentation</param>
        public void StopViewWithName(string? viewName, Dictionary<string, object>? viewSegmentation)
        {
            lock(LockObj) {
                Log.Info("[ViewCountlyService] Calling StopViewWithName vn[" + viewName + "] sg[" + (viewSegmentation == null ? "null" : viewSegmentation.Count.ToString()) + "]");

                StopViewWithNameInternal(viewName, viewSegmentation);
            }
        }

        /// <summary>
        /// Stops a view with the given ID if it was open
        /// </summary>
        /// <param name="viewID">ID of the view</param>
        public void StopViewWithID(string? viewID)
        {
            lock(LockObj) {
                Log.Info("[ViewCountlyService] Calling StopViewWithID vi[" + viewID + "]");

                StopViewWithIDInternal(viewID, null);
            }
        }

        /// <summary>
        /// Stops a view with the given ID if it was open
        /// </summary>
        /// <param name="viewID">ID of the view</param>
        /// <param name="viewSegmentation">view segmentation</param>
        public void StopViewWithID(string? viewID, Dictionary<string, object>? viewSegmentation)
        {
            lock (LockObj) {
                Log.Info("[ViewCountlyService] Calling StopViewWithID vi[" + viewID + "] sg[" + (viewSegmentation == null ? "null" : viewSegmentation.Count.ToString()) + "]");

                StopViewWithIDInternal(viewID, viewSegmentation);
            }
        }

        /// <summary>
        /// Pauses a view with the given ID
        /// </summary>
        /// <param name="viewID">ID of the view</param>
        public void PauseViewWithID(string? viewID)
        {
            lock(LockObj) {
                Log.Info("[ViewCountlyService] Calling PauseViewWithID vi[" + viewID + "]");

                PauseViewWithIDInternal(viewID, false);
            }
        }

        /// <summary>
        /// Resumes a view with the given ID
        /// </summary>
        /// <param name="viewID">ID of the view</param>
        public void ResumeViewWithID(string? viewID)
        {
            lock(LockObj) {
                Log.Info("[ViewCountlyService] Calling ResumeViewWithID vi[" + viewID + "]");

                ResumeViewWithIDInternal(viewID);
            }
        }

        /// <summary>
        /// Stops all views and records a segmentation if set
        /// </summary>
        /// <param name="viewSegmentation">view segmentation</param>
        public void StopAllViews(Dictionary<string, object> viewSegmentation)
        {
            lock(LockObj) {
                Log.Info("[ViewCountlyService] Calling StopAllViews sg[" + (viewSegmentation == null ? "null" : viewSegmentation.Count.ToString()) + "]");

                StopAllViewsInternal(viewSegmentation);
            }
        }

        /// <summary>
        /// Set a segmentation to be recorded with all views
        /// </summary>
        /// <param name="viewSegmentation">global view segmentation</param>
        public void SetGlobalViewSegmentation(Dictionary<string, object> viewSegmentation)
        {
            lock(LockObj) {
                Log.Info("[ViewCountlyService] Calling SetGlobalViewSegmentation sg[" + (viewSegmentation == null ? "null" : viewSegmentation.Count.ToString()) + "]");

                SetGlobalViewSegmentationInternal(viewSegmentation);
            }
        }

        /// <summary>
        /// Updates the segmentation of a view with view id
        /// </summary>
        /// <param name="viewID">ID of the view</param>
        /// <param name="viewSegmentation">view segmentation</param>
        public void AddSegmentationToViewWithID(string? viewID, Dictionary<string, object>? viewSegmentation)
        {
            lock(LockObj) {
                Log.Info("[ViewCountlyService] Calling AddSegmentationToViewWithID for view ID: [" + viewID + "]");

                AddSegmentationToViewWithIDInternal(viewID, viewSegmentation);
            }
        }

        /// <summary>
        /// Updates the segmentation of a view with view name
        /// </summary>
        /// <param name="viewName"></param>
        /// <param name="viewSegmentation"></param>
        public void AddSegmentationToViewWithName(string? viewName, Dictionary<string, object>? viewSegmentation)
        {
            lock(LockObj) {
                Log.Info("[ViewCountlyService] Calling AddSegmentationToViewWithName for Name: [" + viewName + "]");

                AddSegmentationToViewWithNameInternal(viewName, viewSegmentation);
            }
        }

        /// <summary>
        /// Starts a view which would not close automatically.
        /// </summary>
        /// <param name="viewName">name of the view</param>
        /// <param name="customViewSegmentation">segmentation that will be added to the view, set 'null' if none should be added</param>
        /// <param name="viewShouldBeAutomaticallyStopped"></param>
        /// <returns></returns>
        private string? StartViewInternal(string? viewName,  Dictionary<string, object>? customViewSegmentation, bool viewShouldBeAutomaticallyStopped)
        {
            if (!_cly.IsSDKInitialized) {
                Log.Warning("Countly.Instance.Init() must be called before startViewInternal");
                return null;
            }

            if (_utils.IsNullEmptyOrWhitespace(viewName)) {
                Log.Warning("[ViewCountlyService] StartViewInternal, Trying to record view with null or empty view name, ignoring request");
                return null;
            }

            _utils.TruncateSegmentationValues(customViewSegmentation, _configuration.MaxSegmentationValues, "[ViewCountlyService] StartViewInternal, ", Log);
            _utils.RemoveReservedKeysFromSegmentation(customViewSegmentation, reservedSegmentationKeysViews, "[ViewCountlyService] AutoCloseRequiredViews, ", Log);

            int segmCount = 0;
            if (customViewSegmentation != null) {
                segmCount = customViewSegmentation.Count;
            }

            Log.Debug("[ViewCountlyService] Recording view with name: [" + viewName + "], previous view ID:[" + currentViewID + "] custom view segment count:[" + segmCount + "], first:[" + _isFirstView + "], autoStop:[" + viewShouldBeAutomaticallyStopped + "]");

            AutoCloseRequiredViews(false, null);

            ViewData currentViewData = new ViewData();
            currentViewData.ViewID = CountlyUtils.SafeRandomVal();
            currentViewData.ViewName = viewName;
            currentViewData.ViewStartTimeSeconds = _utils.CurrentTimestampSeconds();
            currentViewData.IsAutoStoppedView = viewShouldBeAutomaticallyStopped;

            viewDataMap.Add(currentViewData.ViewID, currentViewData);
            previousViewID = currentViewID;
            currentViewID = currentViewData.ViewID;

            Dictionary<string, object> accumulatedEventSegm = new Dictionary<string, object>(automaticViewSegmentation);

            if(customViewSegmentation != null) {
                foreach(KeyValuePair<string, object> kvp in customViewSegmentation) {
                    accumulatedEventSegm.Add(kvp.Key, kvp.Value);
                }
            }

            Dictionary<string, object> viewSegmentation = CreateViewEventSegmentation(currentViewData, _isFirstView, true, accumulatedEventSegm);

            if (_isFirstView) {
                Log.Debug("[ViewCountlyService] Recording view as the first one in the session. [" + viewName + "]");
                _isFirstView = false;
            }
            _ = _eventService.RecordEventAsync(viewEventKey, viewSegmentation, 1, 0, null);    

            return currentViewData.ViewID;
        }

        /// <summary>
        /// Stops a view with the given name if it was open.
        /// </summary>
        /// <param name="viewName">name of the view</param>
        /// <param name="customViewSegmentation"></param>
        private void StopViewWithNameInternal(string? viewName, Dictionary<string, object>? customViewSegmentation)
        {
            if (_utils.IsNullEmptyOrWhitespace(viewName)) {
                Log.Warning("[ViewCountlyService] StopViewWithNameInternal, Trying to record view with null or empty view name, ignoring request");
                return;
            }

            string viewID = null;

            foreach (KeyValuePair<string, ViewData> entry in viewDataMap) {
                string key = entry.Key;
                ViewData vd = entry.Value;

                if (vd != null && viewName.Equals(vd.ViewName)) {
                    viewID = key;
                }
            }

            if (viewID == null) {
                Log.Warning("[ViewCountlyService] StopViewWithNameInternal, No view entry found with the provided name :[" + viewName + "]");
                return;
            }

            StopViewWithIDInternal(viewID, customViewSegmentation);
        }

        /// <summary>
        /// Closes given views or all views.
        /// </summary>
        /// <param name="closeAllViews"></param>
        /// <param name="customViewSegmentation"></param>
        private void AutoCloseRequiredViews(bool closeAllViews, Dictionary<string, object>? customViewSegmentation)
        {
            Log.Debug("[ViewCountlyService] AutoCloseRequiredViews");
            List<string> viewsToRemove = new List<string>(1);

            foreach (var entry in viewDataMap) {
                ViewData vd = entry.Value;
                if (closeAllViews || vd.IsAutoStoppedView) {
                    viewsToRemove.Add(vd.ViewID);
                }
            }

            if (viewsToRemove.Count > 0) {
                Log.Debug("[ViewCountlyService] AutoCloseRequiredViews, about to close [" + viewsToRemove.Count + "] views");
            }

            _utils.RemoveReservedKeysFromSegmentation(customViewSegmentation, reservedSegmentationKeysViews, "[ViewCountlyService] AutoCloseRequiredViews, ", Log);

            for (int i = 0; i < viewsToRemove.Count; i++) {
                StopViewWithIDInternal(viewsToRemove[i], customViewSegmentation);
            }
        }

        /// <summary>
        /// Stops a view with the given ID if it was open.
        /// </summary>
        /// <param name="viewID">ID of the view</param>
        /// <param name="viewSegmentation">view segmentation</param>
        private void StopViewWithIDInternal(string viewID, Dictionary<string, object>? customViewSegmentation)
        {
            if (_utils.IsNullEmptyOrWhitespace(viewID)) {
                Log.Warning("[ViewCountlyService] StopViewWithIDInternal, Trying to record view with null or empty view ID, ignoring request");
                return;
            }
            
            if (!viewDataMap.ContainsKey(viewID)) {
                Log.Warning("[ViewCountlyService] StopViewWithIDInternal, there is no view with the provided view id to close");
                return;
            }

            ViewData vd = viewDataMap[viewID];
            if (vd == null) {
                Log.Warning("[ViewCountlyService] StopViewWithIDInternal, view id:[" + viewID + "] has a 'null' value. This should not be happening");
                return;
            }

            Log.Debug("[ViewCountlyService] View [" + vd.ViewName + "], id:[" + vd.ViewID + "] is getting closed, reporting duration: [" + (_utils.CurrentTimestampSeconds() - vd.ViewStartTimeSeconds) + "] s, current timestamp: [" + _utils.CurrentTimestampSeconds() + "]");

            if (!_consentService.CheckConsentInternal(Consents.Views)) {
                return;
            }

            _utils.TruncateSegmentationValues(customViewSegmentation, _configuration.MaxSegmentationValues, "[ViewCountlyService] StopViewWithIDInternal", Log);

            RecordViewEndEvent(vd, customViewSegmentation, "StopViewWithIDInternal");

            viewDataMap.Remove(vd.ViewID);
        }

        /// <summary>
        /// Pauses a view with the given ID.
        /// </summary>
        /// <param name="viewID"></param>
        /// <param name="pausedAutomatically"></param>
        private void PauseViewWithIDInternal(string viewID, bool pausedAutomatically)
        {
            if (_utils.IsNullEmptyOrWhitespace(viewID)) {
                Log.Warning("[ViewCountlyService] PauseViewWithIDInternal, Trying to record view with null or empty view ID, ignoring request");
                return;
            }

            if (!viewDataMap.ContainsKey(viewID)) {
                Log.Warning("[ViewCountlyService] PauseViewWithIDInternal, there is no view with the provided view id to close");
                return;
            }

            ViewData vd = viewDataMap[viewID];
            if (vd == null) {
                Log.Warning("[ViewCountlyService] PauseViewWithIDInternal, view id:[" + viewID + "] has a 'null' value. This should not be happening, auto paused:[" + pausedAutomatically + "]");
                return;
            }

            if (!_consentService.CheckConsentInternal(Consents.Views)) {
                return;
            }

            Log.Debug("[ViewCountlyService] PauseViewWithIDInternal, pausing view for ID:[" + viewID + "], name:[" + vd.ViewName + "]");

            if (vd.ViewStartTimeSeconds == 0) {
                Log.Warning("[ViewCountlyService] PauseViewWithIDInternal, pausing a view that is already paused. ID:[" + viewID + "], name:[" + vd.ViewName + "]");
                return;
            }

            vd.IsAutoPaused = pausedAutomatically;

            RecordViewEndEvent(vd, null, "PauseViewWithIDInternal");

            vd.ViewStartTimeSeconds = 0;
        }

        /// <summary>
        /// Records event with given ViewData and filtered segmentation.
        /// </summary>
        /// <param name="vd"></param>
        /// <param name="filteredCustomViewSegmentation"></param>
        /// <param name="viewRecordingSource"></param>
        private void RecordViewEndEvent(ViewData vd, Dictionary<string, object> filteredCustomViewSegmentation, string viewRecordingSource)
        {
            long lastElapsedDurationSeconds = 0;

            if (vd.ViewStartTimeSeconds < 0) {
                Log.Warning("[ViewCountlyService] " + viewRecordingSource + ", view start time value is not normal: [" + vd.ViewStartTimeSeconds + "], ignoring that duration");
            } else if (vd.ViewStartTimeSeconds == 0) {
                Log.Info("[ViewCountlyService] " + viewRecordingSource + ", view is either paused or didn't run, ignoring start timestamp");
            } else {
                lastElapsedDurationSeconds = _utils.CurrentTimestampSeconds() - vd.ViewStartTimeSeconds;
            }

            if (vd.ViewName == null) {
                Log.Warning("[ViewCountlyService] StopViewWithIDInternal, view has no internal name, ignoring it");
                return;
            }

            Dictionary<string, object> accumulatedEventSegm = new Dictionary<string, object>(automaticViewSegmentation);
            if (filteredCustomViewSegmentation != null) {
                foreach(KeyValuePair<string, object> kvp in filteredCustomViewSegmentation) {
                    accumulatedEventSegm.Add(kvp.Key, kvp.Value);
                }
            }

            if(vd.ViewSegmentation != null) {
                foreach(KeyValuePair<string, object> kvp in vd.ViewSegmentation) {
                    accumulatedEventSegm.Add(kvp.Key, kvp.Value);
                }
            }

            long viewDurationSeconds = lastElapsedDurationSeconds;
            Dictionary <string, object> segments = CreateViewEventSegmentation(vd, false, false, accumulatedEventSegm);
            CountlyEventModel currentView = new CountlyEventModel(CountlyEventModel.ViewEvent, segments, duration: viewDurationSeconds);
            _ = _eventService.RecordEventAsync(currentView);
        }

        /// <summary>
        /// Creates view event segmentation
        /// </summary>
        /// <param name="vd"></param>
        /// <param name="firstView"></param>
        /// <param name="visit"></param>
        /// <param name="customViewSegmentation"></param>
        /// <returns></returns>
        private Dictionary<string, object> CreateViewEventSegmentation(ViewData vd, bool firstView, bool visit, Dictionary<string, object> customViewSegmentation)
        {
            Dictionary<string, object> viewSegmentation = new Dictionary<string, object>();
            if (customViewSegmentation != null) {
                foreach (KeyValuePair<string, object> kvp in customViewSegmentation) {
                    viewSegmentation.Add(kvp.Key, kvp.Value);
                }
            }

            viewSegmentation.Add("name", vd.ViewName);
            if (visit) {
                viewSegmentation.Add("visit", 1);
            }
            if (firstView) {
                viewSegmentation.Add("start", 1);
            }
            viewSegmentation.Add("segment", _configuration.metricHelper.OS);

            return viewSegmentation;
        }

        /// <summary>
        /// Resumes a paused view with the given ID. 
        /// </summary>
        /// <param name="viewID"></param>
        private void ResumeViewWithIDInternal(string viewID)
        {
            if (_utils.IsNullEmptyOrWhitespace(viewID)) {
                Log.Warning("[ViewCountlyService] ResumeViewWithIDInternal, Trying to record view with null or empty view ID, ignoring request");
                return;
            }

            if (!viewDataMap.ContainsKey(viewID)) {
                Log.Warning("[ViewCountlyService] ResumeViewWithIDInternal, there is no view with the provided view id to close");
                return;
            }

            ViewData vd = viewDataMap[viewID];
            if (vd == null) {
                Log.Warning("[ViewCountlyService] ResumeViewWithIDInternal, view id:[" + viewID + "] has a 'null' value. This should not be happening");
                return;
            }

            if (!_consentService.CheckConsentInternal(Consents.Views)) {
                return;
            }

            Log.Debug("[ViewCountlyService] ResumeViewWithIDInternal, resuming view for ID:[" + viewID + "], name:[" + vd.ViewName + "]");

            if (vd.ViewStartTimeSeconds > 0) {
                Log.Warning("[ViewCountlyService] ResumeViewWithIDInternal, resuming a view that is already running. ID:[" + viewID + "], name:[" + vd.ViewName + "]");
                return;
            }

            vd.ViewStartTimeSeconds = _utils.CurrentTimestampSeconds();
            vd.IsAutoPaused = false;
        }

        /// <summary>
        /// Stops all open views and records a segmentation if set.
        /// </summary>
        /// <param name="viewSegmentation"></param>
        private void StopAllViewsInternal(Dictionary<string, object> viewSegmentation)
        {
            Log.Debug("[ViewCountlyService] StopAllViewsInternal");

            AutoCloseRequiredViews(true, viewSegmentation);
        }

        /// <summary>
        /// Set a segmentation to be recorded with all views
        /// </summary>
        /// <param name="viewSegmentation"></param>
        private void SetGlobalViewSegmentationInternal(Dictionary<string, object> viewSegmentation)
        {
            Log.Debug("[ViewCountlyService] Calling SetGlobalViewSegmentationInternal with[" + (viewSegmentation == null ? "null" : viewSegmentation.Count.ToString()) + "] entries");

            automaticViewSegmentation.Clear();

            if (viewSegmentation != null) {

                _utils.RemoveReservedKeysFromSegmentation(viewSegmentation, reservedSegmentationKeysViews, "[ViewCountlyService] SetGlobalViewSegmentationInternal, ", Log);

                if (_utils.RemoveUnsupportedDataTypes(viewSegmentation, Log)) {  
                    Log.Warning("[ViewCountlyService] SetGlobalViewSegmentationInternal, You have provided an unsupported data type in your View Segmentation. Removing the unsupported values.");
                }

                foreach(KeyValuePair<string, object> kvp in viewSegmentation) {
                    automaticViewSegmentation.Add(kvp.Key, kvp.Value);
                }
            }
        }

        /// <summary>
        /// Updates the segmentation of a view.
        /// </summary>
        /// <param name="viewID"></param>
        /// <param name="viewSegmentation"></param>
        private void AddSegmentationToViewWithIDInternal(string? viewID, Dictionary<string, object>? viewSegmentation)
        {
            if (_utils.IsNullEmptyOrWhitespace(viewID) || viewSegmentation == null) {
                Log.Warning("[ViewsCountlyService] AddSegmentationToViewWithID, null or empty parameters provided");
                return;
            }

            if (!viewDataMap.ContainsKey(viewID)) {
                Log.Warning("[ViewsCountlyService] AddSegmentationToViewWithID, there is no view with the provided view id");
                return;
            }

            ViewData vd = viewDataMap[viewID];
            if (vd == null) {
                Log.Warning("[ViewsCountlyService] AddSegmentationToViewWithID, view id:[" + viewID + "] has a 'null' view data. This should not be happening");
                return;
            }

            _utils.TruncateSegmentationValues(viewSegmentation, _cly.Configuration.MaxSegmentationValues, "[ViewsCountlyService] AddSegmentationToViewWithID", Log);
            _utils.RemoveReservedKeysFromSegmentation(viewSegmentation, reservedSegmentationKeysViews, "[ViewsCountlyService] AddSegmentationToViewWithID, ", Log);

            if (vd.ViewSegmentation == null) {
                vd.ViewSegmentation = new Dictionary<string, object>(viewSegmentation);
            } else {
                foreach (KeyValuePair<string, object> kvp in viewSegmentation)
                    {
                    vd.ViewSegmentation.Add(kvp.Key, kvp.Value);
                }
            }
        }

        /// <summary>
        /// Updates the segmentation of a view.
        /// </summary>
        /// <param name="viewName"></param>
        /// <param name="viewSegmentation"></param>
        private void AddSegmentationToViewWithNameInternal(string? viewName, Dictionary<string, object>? viewSegmentation)
        {
            string viewID = null;

            foreach (var entry in viewDataMap) {
                string key = entry.Key;
                ViewData vd = entry.Value;

                if (vd != null && viewName != null && viewName.Equals(vd.ViewName)) {
                    viewID = key;
                }
            }

            if (viewID == null) {
                Log.Warning("[ViewsCountlyService] AddSegmentationToViewWithName, No view entry found with the provided name :[" + viewName + "]");
                return;
            }

            Log.Info("[ViewsCountlyService] Will add segmentation for view: [" + viewName + "] with ID:[" + viewID + "]");

            AddSegmentationToViewWithIDInternal(viewID, viewSegmentation);
        }

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

                CountlyEventModel currentView = new CountlyEventModel(viewEventKey, segmentation, 1);
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
