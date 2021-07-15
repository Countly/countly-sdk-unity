using System;
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
        private bool isQueueBeingProcessed = false;
        internal readonly NonViewEventRepository _eventRepo;
        private readonly RequestCountlyHelper _requestCountlyHelper;

        internal EventCountlyService(CountlyConfiguration configuration, CountlyLogHelper logHelper, RequestCountlyHelper requestCountlyHelper, NonViewEventRepository nonViewEventRepo, ConsentCountlyService consentService) : base(configuration, logHelper, consentService)
        {
            Log.Debug("[EventCountlyService] Initializing.");

            _eventRepo = nonViewEventRepo;
            _requestCountlyHelper = requestCountlyHelper;
        }

        /// <summary>
        ///     Add all recorded events to request queue
        /// </summary>
        internal void AddEventsToRequestQueue()
        {

            Log.Debug("[EventCountlyService] AddEventsToRequestQueue: Start");

            if (_eventRepo.Models.Count == 0) {
                Log.Debug("[EventCountlyService] AddEventsToRequestQueue: Event queue is empty!");
                return;
            }

            if (isQueueBeingProcessed) {
                Log.Verbose("[EventCountlyService] AddEventsToRequestQueue: Event queue being processed!");
                return;
            }
            isQueueBeingProcessed = true;

            int count = _eventRepo.Models.Count;
            //Send all at once
            Dictionary<string, object> requestParams =
                new Dictionary<string, object>
                {
                    {
                        "events", JsonConvert.SerializeObject(_eventRepo.Models, Formatting.Indented,
                            new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore})
                    }
                };

            _requestCountlyHelper.AddToRequestQueue(requestParams);

            Log.Debug("[EventCountlyService] AddEventsToRequestQueue: Remove events from event queue, count: " + count);
            for (int i = 0; i < count; ++i) {
                _eventRepo.Dequeue();
            }

            isQueueBeingProcessed = false;
            Log.Debug("[EventCountlyService] AddEventsToRequestQueue: End");
        }

        /// <summary>
        /// An internal function to add an event to event queue.
        /// </summary>
        /// <param name="event">an event</param>
        /// <returns></returns>
        internal async Task RecordEventAsync(CountlyEventModel @event)
        {
            Log.Debug("[EventCountlyService] RecordEventAsync : " + @event.ToString());

            if (_configuration.EnableTestMode) {
                return;
            }

            _eventRepo.Enqueue(@event);

            if (_eventRepo.Count >= _configuration.EventQueueThreshold) {
                AddEventsToRequestQueue();
                await _requestCountlyHelper.ProcessQueue();
            }
        }

        /// <summary>
        /// Report an event to the server.
        /// </summary>
        /// <param name="key">event key</param>
        /// <returns></returns>
        public async Task RecordEventAsync(string key)
        {
            lock (LockObj) {
                Log.Info("[EventCountlyService] RecordEventAsync : key = " + key);

                if (!_consentService.CheckConsentInternal(Consents.Events)) {
                    return;
                }

                _ = RecordEventAsync(key, null);
            }

        }

        /// <summary>
        /// Report an event to the server with segmentation.
        /// </summary>
        /// <param name="key">event key</param>
        /// <param name="segmentation">custom segmentation you want to set, leave null if you don't want to add anything</param>
        /// <param name="count">how many of these events have occurred, default value is "1"</param>
        /// <param name="sum">set sum if needed, default value is "0"</param>
        /// <param name="duration">set sum if needed, default value is "0"</param>
        /// <returns></returns>
        public async Task RecordEventAsync(string key, IDictionary<string, object> segmentation = null,
            int? count = 1, double? sum = 0, double? duration = null)
        {
            lock (LockObj) {
                Log.Info("[EventCountlyService] RecordEventAsync : key = " + key + ", segmentation = " + segmentation + ", count = " + count + ", sum = " + sum + ", duration = " + duration);

                if (!_consentService.CheckConsentInternal(Consents.Events)) {
                    return;
                }

                if (_configuration.EnableTestMode) {
                    return;
                }

                if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key)) {
                    Log.Warning("[EventCountlyService] RecordEventAsync : The event key '" + key + "'isn't valid.");

                    return;
                }

                if (key.Length > _configuration.MaxKeyLength) {
                    Log.Verbose("[EventCountlyService] RecordEventAsync : Max allowed key length is " + _configuration.MaxKeyLength);
                    key = key.Substring(0, _configuration.MaxKeyLength);
                }
                IDictionary<string, object> segments = MainpulateSegments(segmentation);
                CountlyEventModel @event = new CountlyEventModel(key, segments, count, sum, duration);

                _ = RecordEventAsync(@event);
            }

        }

        /// <summary>
        ///     Reports a custom event to the Countly server.
        /// </summary>
        /// <param name="key">event key</param>
        /// <param name="segmentation">custom segmentation you want to set, leave null if you don't want to add anything</param>
        /// <param name="count">how many of these events have occurred, default value is "1"</param>
        /// <param name="sum">set sum if needed, default value is "0"</param>
        /// <param name="duration">set sum if needed, default value is "0"</param>
        /// <returns></returns>
        [Obsolete("ReportCustomEventAsync is deprecated, please use RecordEventAsync method instead.")]
        public async Task ReportCustomEventAsync(string key,
                    IDictionary<string, object> segmentation = null,
                    int? count = 1, double? sum = null, double? duration = null)
        {
            lock (LockObj) {
                Log.Info("[EventCountlyService] ReportCustomEventAsync : key = " + key + ", segmentation = " + (segmentation != null) + ", count = " + count + ", sum = " + sum + ", duration = " + duration);

                if (!_consentService.CheckConsentInternal(Consents.Events)) {
                    return;
                }

                if (string.IsNullOrEmpty(key) && string.IsNullOrWhiteSpace(key)) {
                    return;
                }

                if (key.Length > _configuration.MaxKeyLength) {
                    Log.Verbose("[EventCountlyService] ReportCustomEventAsync : Max allowed key length is " + _configuration.MaxKeyLength);
                    key = key.Substring(0, _configuration.MaxKeyLength);
                }


                IDictionary<string, object> segments = MainpulateSegments(segmentation);
                CountlyEventModel @event = new CountlyEventModel(key, segments, count, sum, duration);

                _ = RecordEventAsync(@event);
            }
        }

        #region override Methods
        internal override void DeviceIdChanged(string deviceId, bool merged)
        {

        }

        internal override void ConsentChanged(List<Consents> updatedConsents, bool newConsentValue)
        {

        }
        #endregion
    }
}
