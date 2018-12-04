using Assets.Scripts.Helpers;
using Assets.Scripts.Main.Development;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assets.Scripts.Models
{
    [Serializable]
    struct PushActionSegment
    {
        [JsonProperty("b")]
        internal string Identifier { get; set; }
        [JsonProperty("i")]
        internal string MessageID { get; set; }
    }

    /// <summary>
    /// Custom Segmentation for Views related events.
    /// </summary>
    [Serializable]
    struct ViewSegment
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("segment")]
        public string Segment { get; set; }
        [JsonProperty("visit")]
        public int? Visit { get; set; }
        [JsonIgnore]
        public bool HasSessionBegunWithView { get; set; }
        [JsonProperty("start")]
        internal string Start => HasSessionBegunWithView ? "true" : null;
    }

    /// <summary>
    /// Custom Segmentation for Action related events.
    /// </summary>
    [Serializable]
    struct ActionSegment
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("x")]
        public int PositionX { get; set; }
        [JsonProperty("y")]
        public int PositionY { get; set; }
        [JsonProperty("width")]
        public int Width { get; set; }
        [JsonProperty("height")]
        public int Height { get; set; }
    }

    /// <summary>
    /// Custom Segmentation for Star Rating event.
    /// </summary>
    [Serializable]
    struct StarRatingSegment
    {
        [JsonProperty("platform ")]
        public string Platform { get; set; }
        [JsonProperty("app_version")]
        public string AppVersion { get; set; }
        [JsonProperty("rating")]
        public int Rating { get; set; }
    }

    [Serializable]
    public class CountlyEventModel
    {
        #region Reserved Event Names

        [JsonIgnore]
        internal const string ViewEvent = "[CLY]_view";
        [JsonIgnore]
        internal const string ViewActionEvent = "[CLY]_action";
        [JsonIgnore]
        internal const string StarRatingEvent = "[CLY]_star_rating";
        [JsonIgnore]
        internal const string PushActionEvent = "[CLY]_push_action";

        #endregion

        [JsonProperty("key")]
        internal string Key { get; set; }
        [JsonProperty("count")]
        internal int? Count { get; set; }
        [JsonProperty("sum")]
        internal double? Sum { get; set; }
        [JsonProperty("dur")]
        internal double? Duration { get; set; }
        [JsonProperty("segmentation")]
        internal Dictionary<string, object> Segmentation { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }
        [JsonProperty("hour")]
        public int Hour { get; set; }
        [JsonProperty("dow")]
        public int DayOfWeek { get; set; }
        [JsonProperty("tz")]
        public string Timezone { get; set; }

        [JsonIgnore]
        public DateTime TimeRecorded { get; private set; }

        internal static Dictionary<string, CountlyEventModel> TotalEvents { get; private set; }
                                                        = new Dictionary<string, CountlyEventModel>();

        /// <summary>
        /// Initializes a new instance of event model. 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="segmentation"></param>
        /// <param name="count"></param>
        /// <param name="sum"></param>
        /// <param name="duration"></param>
        public CountlyEventModel(string key, IDictionary<string, object> segmentation = null, int? count = 1, double? sum = null,
                                double? duration = null)
        {
            Key = key;
            Count = count ?? 1;
            if (segmentation != null)
                Segmentation = segmentation as Dictionary<string, object>;
            Duration = duration;
            Sum = sum;

            //Records the time the time the event was recorded
            TimeRecorded = DateTime.Now;
        }

        internal static async Task<CountlyResponse> RecordEventAsync(string key)
        {
            if (string.IsNullOrEmpty(key) && string.IsNullOrWhiteSpace(key))
            {
                return new CountlyResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Key is required."
                };
            }

            var existingEvent = TotalEvents.Select(c => c.Key).FirstOrDefault(x => key.Equals(x, StringComparison.OrdinalIgnoreCase));
            if (existingEvent != null)
            {
                TotalEvents.Remove(existingEvent);
            }

            if (TotalEvents.Count >= Countly.EventSendThreshold)
                await ReportAllRecordedEventsAsync();

            TotalEvents.Add(key, new CountlyEventModel(key));

            return new CountlyResponse
            {
                IsSuccess = true,
            };
        }

        internal static async Task<CountlyResponse> RecordEventAsync(string key, IDictionary<string, object> segmentation,
                                        int? count = 1, double? sum = 0, double? duration = null)
        {
            if (string.IsNullOrEmpty(key) && string.IsNullOrWhiteSpace(key))
            {
                return new CountlyResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Key is required."
                };
            }

            var existingEvent = TotalEvents.Select(c => c.Key).FirstOrDefault(x => key.Equals(x, StringComparison.OrdinalIgnoreCase));
            if (existingEvent != null)
            {
                TotalEvents.Remove(existingEvent);
            }

            if (TotalEvents.Count >= Countly.EventSendThreshold)
                await ReportAllRecordedEventsAsync();

            TotalEvents.Add(key, new CountlyEventModel(key, segmentation, count, sum, duration));

            return new CountlyResponse
            {
                IsSuccess = true,
            };
        }

        /// <summary>
        /// Reports all recorded events to the server
        /// </summary>
        internal static async Task<CountlyResponse> ReportAllRecordedEventsAsync(bool addToRequestQueue = false)
        {
            foreach (var evntObj in TotalEvents)
            {
                var evnt = evntObj.Value;
                evnt.Duration = evnt.Duration ?? (DateTime.Now - evnt.TimeRecorded).TotalMilliseconds;
                SetTimeZoneInfo(evnt, evnt.TimeRecorded);
            }

            //Send all at once
            var requestParams =
                   new Dictionary<string, object>
                   {
                        { "events", JsonConvert.SerializeObject(TotalEvents.Select(x=>x.Value), Formatting.Indented,
                                        new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }) },
                   };

            var res = await CountlyHelper.GetResponseAsync(requestParams, addToRequestQueue);

            //Removing the event
            TotalEvents = new Dictionary<string, CountlyEventModel>();

            return res;
        }

        /// <summary>
        /// Sends multiple events to the countly server. It expects a list of events as input.
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        internal static async Task<CountlyResponse> ReportMultipleEventsAsync(List<CountlyEventModel> events)
        {
            if (events == null || events.Count == 0)
            {
                return new CountlyResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "No events found."
                };
            }

            var currentTime = DateTime.UtcNow;
            foreach (var evnt in events)
            {
                SetTimeZoneInfo(evnt, currentTime);
            }

            var requestParams =
               new Dictionary<string, object>
               {
                    { "events", JsonConvert.SerializeObject(events, Formatting.Indented,
                                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }) },
               };
            return await CountlyHelper.GetResponseAsync(requestParams);
        }

        /// <summary>
        /// Reports a custom event to the Counlty server.
        /// </summary>
        /// <returns></returns>
        internal static async Task<CountlyResponse> ReportCustomEventAsync(string key, IDictionary<string, object> segmentation = null,
                                                    int? count = 1, double? sum = null, double? duration = null)
        {
            if (string.IsNullOrEmpty(key) && string.IsNullOrWhiteSpace(key))
            {
                return new CountlyResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Key is required."
                };
            }

            var evnt = new CountlyEventModel(key, segmentation, count, sum, duration);
            SetTimeZoneInfo(evnt, DateTime.UtcNow);

            var requestParams =
               new Dictionary<string, object>
               {
                    { "events", JsonConvert.SerializeObject(new List<CountlyEventModel>{ evnt }, Formatting.Indented,
                                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }) },
               };
            return await CountlyHelper.GetResponseAsync(requestParams);
        }

        internal static void SetTimeZoneInfo(CountlyEventModel evnt, DateTime requestDatetime)
        {
            var timezoneInfo = TimeMetricModel.GetTimeZoneInfoForRequest(requestDatetime);
            evnt.Timestamp = timezoneInfo.Timestamp;
            evnt.DayOfWeek = timezoneInfo.DayOfWeek;
            evnt.Hour = timezoneInfo.Hour;
            evnt.Timezone = timezoneInfo.Timezone;
        }

        #region Unused Code

        ///// <summary>
        ///// Ends a particular event
        ///// </summary>
        ///// <returns></returns>
        //internal static async Task<CountlyResponse> EndAsync(string key, IDictionary<string, object> segmentation = null, int? count = 1, double? sum = 0,
        //    bool addToRequestQueue = false)
        //{
        //    if (string.IsNullOrEmpty(key))
        //        throw new ArgumentNullException(key, "Key is required.");

        //    var addedEvent = TotalEvents.Where(e => key.Equals(e.Key, StringComparison.OrdinalIgnoreCase))
        //                                .FirstOrDefault();
        //    if (string.IsNullOrEmpty(addedEvent.Key))
        //        throw new Exception($"Event {key} doesn't exists. Please start an event first.");

        //    var evnt = new CountlyEventModel(key, segmentation, count, sum);
        //    evnt.Duration = evnt.Duration ?? (DateTime.Now - addedEvent.Value).TotalMilliseconds;

        //    SetTimeZoneInfo(evnt, addedEvent.Value);

        //    var requestParams =
        //       new Dictionary<string, object>
        //       {
        //            { "events", JsonConvert.SerializeObject(evnt, Formatting.Indented,
        //                            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }) },
        //       };

        //    var res = await CountlyHelper.GetResponseAsync(requestParams, addToRequestQueue);

        //    //Removing the event
        //    if (res.IsSuccess)
        //        TotalEvents.Remove(key);

        //    return res;
        //}

        #endregion
    }
}