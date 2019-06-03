using System.Collections.Generic;
using System.Threading.Tasks;
using Plugins.Countly.Helpers;
using Plugins.Countly.Models;
using UnityEngine;

namespace Plugins.Countly.Services.Impls.Wrapper
{
    public class EventCountlyServiceWrapper : IEventCountlyService
    {
        public Task<CountlyResponse> RecordEventAsync(CountlyEventModel @event)
        {
            Debug.Log("[EventCountlyServiceWrapper] RecordEventAsync: " + @event);
            return Task.FromResult(new CountlyResponse());
        }

        public Task<CountlyResponse> RecordEventAsync(string key)
        {
            Debug.Log("[EventCountlyServiceWrapper] RecordEventAsync, key: " + key);
            return Task.FromResult(new CountlyResponse());
        }

        public Task<CountlyResponse> RecordEventAsync(string key, IDictionary<string, object> segmentation, int? count, double? sum, double? duration = null)
        {
            Debug.Log("[EventCountlyServiceWrapper] RecordEventAsync, key: " + key + ", segments: " + segmentation + ", int: " + count + ", sum: " + sum + ", dur: " + duration);
            return Task.FromResult(new CountlyResponse());
        }

        public Task<CountlyResponse> ReportAllRecordedViewEventsAsync(bool addToRequestQueue = false)
        {
            Debug.Log("[EventCountlyServiceWrapper] ReportAllRecordedViewEventsAsync, addToRequestQueue: " + addToRequestQueue);
            return Task.FromResult(new CountlyResponse());
        }

        public Task<CountlyResponse> ReportAllRecordedNonViewEventsAsync(bool addToRequestQueue = false)
        {
            Debug.Log("[EventCountlyServiceWrapper] ReportAllRecordedNonViewEventsAsync, addToRequestQueue: " + addToRequestQueue);
            return Task.FromResult(new CountlyResponse());
        }

        public Task<CountlyResponse> ReportMultipleEventsAsync(List<CountlyEventModel> events)
        {
            Debug.Log("[EventCountlyServiceWrapper] ReportMultipleEventsAsync, events: " + events.Count);
            return Task.FromResult(new CountlyResponse());
        }

        public Task<CountlyResponse> ReportCustomEventAsync(string key, IDictionary<string, object> segmentation, int? count, double? sum = null,
            double? duration = null)
        {
            Debug.Log("[EventCountlyServiceWrapper] ReportCustomEventAsync, key: " + key + ", segments: " + segmentation + ", int: " + count + ", sum: " + sum + ", dur: " + duration);
            return Task.FromResult(new CountlyResponse());
        }
    }
}