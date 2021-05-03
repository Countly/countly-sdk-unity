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
        internal readonly NonViewEventRepository _eventRepo;
        private readonly RequestCountlyHelper _requestCountlyHelper;

        internal EventCountlyService(CountlyConfiguration configuration, CountlyLogHelper logHelper, RequestCountlyHelper requestCountlyHelper,NonViewEventRepository nonViewEventRepo, ConsentCountlyService consentService) : base(configuration, logHelper, consentService)
        {
            Log.Debug("[EventCountlyService] Initializing.");

            _eventRepo = nonViewEventRepo;
            _requestCountlyHelper = requestCountlyHelper;
        }

        /// <summary>
        ///     Add all recorded events to request queue
        /// </summary>
        internal async Task AddEventsToRequestQueue()
        {

            Log.Debug("[EventCountlyService] AddEventsToRequestQueue");

            if (_eventRepo.Models.Count == 0) {
                return;
            }

           
            //Send all at once
            Dictionary<string, object> requestParams =
                new Dictionary<string, object>
                {
                    {
                        "events", JsonConvert.SerializeObject(_eventRepo.Models, Formatting.Indented,
                            new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore})
                    }
                };

            await _requestCountlyHelper.GetResponseAsync(requestParams);

            _eventRepo.Clear();

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
                await AddEventsToRequestQueue();
            }
        }

        /// <summary>
        /// Report an event to the server.
        /// </summary>
        /// <param name="key">event key</param>
        /// <returns></returns>
        public async Task RecordEventAsync(string key)
        {
            Log.Info("[EventCountlyService] RecordEventAsync : key = " + key);

            if (!_consentService.CheckConsentInternal(Consents.Events)) {
                return;
            }

            await RecordEventAsync(key, null);
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
            Log.Info("[EventCountlyService] RecordEventAsync : key = " + key + ", segmentation = " + segmentation + ", count = " + count + ", sum = " + sum + ", duration = " + duration);

            if (!_consentService.CheckConsentInternal(Consents.Events)) {
                return;
            }

            if (_configuration.EnableTestMode) {
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
                        Log.Warning("[EventCountlyService] RecordEventAsync : In segmentation Data type of item '" + item.Key + "'isn't valid.");
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
        /// <param name="events">a list of events</param>
        /// <returns></returns>
        internal async Task ReportMultipleEventsAsync(List<CountlyEventModel> events)
        {
            if (events == null || events.Count == 0) {
                return;
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
            Log.Info("[EventCountlyService] ReportCustomEventAsync : key = " + key + ", segmentation = " + (segmentation != null) + ", count = " + count + ", sum = " + sum + ", duration = " + duration);

            if (!_consentService.CheckConsentInternal(Consents.Events)) {
                return;
            }

            if (string.IsNullOrEmpty(key) && string.IsNullOrWhiteSpace(key)) {
                return;
            }

            CountlyEventModel evt = new CountlyEventModel(key, segmentation, count, sum, duration);

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