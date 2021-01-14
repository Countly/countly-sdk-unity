using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Helpers;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Persistance.Repositories.Impls;
using UnityEngine;

namespace Plugins.CountlySDK.Services
{
    public class EventCountlyService : AbstractBaseService
    {
        private readonly ViewEventRepository _viewEventRepo;
        private readonly NonViewEventRepository _nonViewEventRepo;
        private readonly CountlyConfiguration _countlyConfiguration;
        private readonly RequestCountlyHelper _requestCountlyHelper;

        internal EventCountlyService(CountlyConfiguration countlyConfiguration, RequestCountlyHelper requestCountlyHelper,
            ViewEventRepository viewEventRepo, NonViewEventRepository nonViewEventRepo)
        {
            _viewEventRepo = viewEventRepo;
            _nonViewEventRepo = nonViewEventRepo;
            _countlyConfiguration = countlyConfiguration;
            _requestCountlyHelper = requestCountlyHelper;
        }

        /// <summary>
        ///     Send all recorded events to request queue
        /// </summary>
        internal async Task AddEventsToRequestQueue()
        {
            if ((_viewEventRepo.Models.Count + _nonViewEventRepo.Models.Count) == 0) {
                return;
            }

            Queue result = new Queue();


            while (_nonViewEventRepo.Count > 0) {
                result.Enqueue(_nonViewEventRepo.Dequeue());
            }
            while (_viewEventRepo.Count > 0) {
                result.Enqueue(_viewEventRepo.Dequeue());
            }

            //Send all at once
            Dictionary<string, object> requestParams =
                new Dictionary<string, object>
                {
                    {
                        "events", JsonConvert.SerializeObject(result, Formatting.Indented,
                            new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore})
                    }
                };

            await _requestCountlyHelper.GetResponseAsync(requestParams);

        }

        internal async Task RecordEventAsync(CountlyEventModel @event)
        {

            if (_countlyConfiguration.EnableConsoleLogging) {
                Debug.Log("[Countly] RecordEventAsync : " + @event.ToString());
            }

            if (_countlyConfiguration.EnableTestMode) {
                return;
            }

            if (_countlyConfiguration.EnableFirstAppLaunchSegment) {
                AddFirstAppSegment(@event);
            }

            if (@event.Key.Equals(CountlyEventModel.ViewEvent)) {
                _viewEventRepo.Enqueue(@event);
            } else {
                _nonViewEventRepo.Enqueue(@event);
            }

            if ((_viewEventRepo.Count + _nonViewEventRepo.Count) >= _countlyConfiguration.EventQueueThreshold) {
                await AddEventsToRequestQueue();
            }
        }

        /// <summary>
        /// Report an event to the server.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="useNumberInSameSession"></param>
        /// <returns></returns>
        public async Task RecordEventAsync(string key)
        {
            if (!Consent.CheckConsent(Features.Events)) {
                return;
            }

            await RecordEventAsync(key, null);
        }

        /// <summary>
        /// Report an event to the server with segmentation.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="segmentation"></param>
        /// <param name="useNumberInSameSession"></param>
        /// <param name="count"></param>
        /// <param name="sum"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public async Task RecordEventAsync(string key, SegmentModel segmentation,
            int? count = 1, double? sum = 0, double? duration = null)
        {
            if (!Consent.CheckConsent(Features.Events)) {
                return;
            }

            if (_countlyConfiguration.EnableConsoleLogging) {
                Debug.Log("[Countly] RecordEventAsync : key = " + key);
            }

            if (_countlyConfiguration.EnableTestMode) {
                return;
            }

            if (string.IsNullOrEmpty(key) && string.IsNullOrWhiteSpace(key)) {
                return;
            }

            if (segmentation != null) {
                List<string> toRemove = new List<string>();

                foreach (KeyValuePair<string, object> item in segmentation) {
                    bool isValidDataType = item.Value.GetType() == typeof(int)
                        || item.Value.GetType() == typeof(bool)
                        || item.Value.GetType() == typeof(float)
                        || item.Value.GetType() == typeof(double)
                        || item.Value.GetType() == typeof(string);

                    if (!isValidDataType) {
                        toRemove.Add(item.Key);
                        Debug.LogWarning("[Countly] RecordEventAsync : In segmentation Data type of item '" + item.Key + "'isn't valid.");
                    }
                }

                foreach (string k in toRemove) {
                    segmentation.Remove(k);
                }
            }

            CountlyEventModel @event = new CountlyEventModel(key, segmentation, count, sum, duration);

            await RecordEventAsync(@event);
        }

        /// <summary>
        ///     Sends multiple events to the countly server. It expects a list of events as input.
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        internal async Task ReportMultipleEventsAsync(List<CountlyEventModel> events)
        {
            if (events == null || events.Count == 0) {
                return;
            }

            if (_countlyConfiguration.EnableFirstAppLaunchSegment) {
                foreach (CountlyEventModel evt in events) {
                    AddFirstAppSegment(evt);
                }
            }

            Dictionary<string, object> requestParams =
                new Dictionary<string, object>
                {
                    {
                        "events", JsonConvert.SerializeObject(events, Formatting.Indented,
                            new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore})
                    }
                };

            await _requestCountlyHelper.GetResponseAsync(requestParams);
        }

        /// <summary>
        ///     Reports a custom event to the Countly server.
        /// </summary>
        /// <returns></returns>
        public async Task ReportCustomEventAsync(string key,
            IDictionary<string, object> segmentation = null,
            int? count = 1, double? sum = null, double? duration = null)
        {
            if (!Consent.CheckConsent(Features.Events)) {
                return;
            }

            if (string.IsNullOrEmpty(key) && string.IsNullOrWhiteSpace(key)) {
                return;
            }

            CountlyEventModel evt = new CountlyEventModel(key, segmentation, count, sum, duration);

            if (_countlyConfiguration.EnableFirstAppLaunchSegment) {
                AddFirstAppSegment(evt);
            }

            Dictionary<string, object> requestParams =
                new Dictionary<string, object>
                {
                    {
                        "events", JsonConvert.SerializeObject(new List<CountlyEventModel> {evt}, Formatting.Indented,
                            new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore})
                    }
                };

            await _requestCountlyHelper.GetResponseAsync(requestParams);
        }

        private void AddFirstAppSegment(CountlyEventModel @event)
        {
            if (@event.Segmentation == null) {
                @event.Segmentation = new SegmentModel();
            }
            @event.Segmentation.Add(Constants.FirstAppLaunchSegment, FirstLaunchAppHelper.IsFirstLaunchApp);
        }

        #region override Methods
        internal override void DeviceIdChanged(string deviceId, bool merged)
        {

        }

        internal override void ConsentChanged(Dictionary<Features, bool> updatedConsents)
        {

        }
        #endregion
    }
}