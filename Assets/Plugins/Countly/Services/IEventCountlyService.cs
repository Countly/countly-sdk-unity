using System.Collections.Generic;
using System.Threading.Tasks;
using Plugins.Countly.Helpers;
using Plugins.Countly.Models;

namespace Plugins.Countly.Services
{
    public interface IEventCountlyService
    {
        Task<CountlyResponse> RecordEventAsync(CountlyEventModel @event, bool useNumberInSameSession = false);

        Task<CountlyResponse> RecordEventAsync(string key, SegmentModel segmentation = null, bool useNumberInSameSession = false,
            int? count = 1, double? sum = 0, double? duration = null);

        /// <summary>
        ///     Reports all recorded view events to the server
        /// </summary>
        Task<CountlyResponse> ReportAllRecordedViewEventsAsync(bool addToRequestQueue = false);

        /// <summary>
        ///     Reports all recorded events to the server
        /// </summary>
        Task<CountlyResponse> ReportAllRecordedNonViewEventsAsync(bool addToRequestQueue = false);

        /// <summary>
        ///     Sends multiple events to the countly server. It expects a list of events as input.
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        Task<CountlyResponse> ReportMultipleEventsAsync(List<CountlyEventModel> events);

        /// <summary>
        ///     Reports a custom event to the Counlty server.
        /// </summary>
        /// <returns></returns>
        Task<CountlyResponse> ReportCustomEventAsync(string key,
            IDictionary<string, object> segmentation = null,
            int? count = 1, double? sum = null, double? duration = null);
    }
}