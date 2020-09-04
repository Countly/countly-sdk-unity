using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plugins.Countly.Helpers;
using Plugins.Countly.Models;
using UnityEngine;

namespace Plugins.Countly.Services.Impls.Wrapper
{
    public class EventCountlyServiceWrapper : IEventCountlyService
    {
        public Task<CountlyResponse> RecordEventAsync(CountlyEventModel @event, bool useNumberInSameSession = false)
        {
            Debug.Log("[EventCountlyServiceWrapper] RecordEventAsync: " + @event);
            return Task.FromResult(new CountlyResponse());
        }

        public Task<CountlyResponse> RecordEventAsync(string key, SegmentModel segmentModel = null, bool useNumberInSameSession = false, int? count = 1, double? sum = 0, double? duration = null)
        {
            
            Debug.Log("[EventCountlyServiceWrapper] RecordEventAsync, key: " + key + ", segments: " + segmentModel + ", count: " + count + ", sum: " + sum + ", dur: " + duration);
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
            var segmentStr = string.Join(";", segmentation.Select(x => x.Key + "=" + x.Value).ToArray());
            Debug.Log("[EventCountlyServiceWrapper] ReportCustomEventAsync, key: " + key + ", segments: \n" + segmentStr + "\n, int: " + count + ", sum: " + sum + ", dur: " + duration);
            return Task.FromResult(new CountlyResponse());
        }
    }
}