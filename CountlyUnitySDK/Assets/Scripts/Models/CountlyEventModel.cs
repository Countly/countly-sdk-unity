using Assets.Scripts.Helpers;
using Assets.Scripts.Main.Development;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        internal string Name { get; set; }
        [JsonProperty("segment")]
        internal string Segment { get; set; }
        [JsonProperty("visit")]
        internal int? Visit { get; set; }
        [JsonIgnore]
        internal bool HasSessionBegunWithView { get; set; }
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
        internal string Type { get; set; }
        [JsonProperty("x")]
        internal int PositionX { get; set; }
        [JsonProperty("y")]
        internal int PositionY { get; set; }
        [JsonProperty("width")]
        internal int Width { get; set; }
        [JsonProperty("height")]
        internal int Height { get; set; }
    }

    /// <summary>
    /// Custom Segmentation for Star Rating event.
    /// </summary>
    [Serializable]
    struct StarRatingSegment
    {
        [JsonProperty("platform ")]
        internal string Platform { get; set; }
        [JsonProperty("app_version")]
        internal string AppVersion { get; set; }
        [JsonProperty("rating")]
        internal int Rating { get; set; }
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
        internal JRaw Segmentation { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }
        [JsonProperty("hour")]
        public int Hour { get; set; }
        [JsonProperty("dow")]
        public int DayOfWeek { get; set; }
        [JsonProperty("tz")]
        public string Timezone { get; set; }

        /// <summary>
        /// Initializes a new instance of event model. 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="segmentation"></param>
        /// <param name="count"></param>
        /// <param name="sum"></param>
        /// <param name="duration"></param>
        public CountlyEventModel(string key, string segmentation = null, int? count = 1, double? sum = null, 
                                double? duration = null)
        {
            Key = key;
            Count = count ?? 1;
            if (segmentation != null)
                Segmentation = new JRaw(segmentation);
            Duration = duration;
            Sum = sum;
        }

        internal static void StartEvent(string key)
        {
            if (string.IsNullOrEmpty(key) && string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(key, "Key is required.");

            if (Countly.TotalEvents.Any(evnt => key.Equals(evnt.Key, StringComparison.OrdinalIgnoreCase)))
                throw new Exception($"Event {key} already started. Please end this event first.");

            if (Countly.TotalEvents.Count >= Countly.EventSendThreshold)
                throw new Exception($"Event count reached threshold value of {Countly.EventSendThreshold}");

            Countly.TotalEvents.Add(key, DateTime.Now);
        }

        /// <summary>
        /// Ends a particular event
        /// </summary>
        /// <returns></returns>
        internal static async Task<CountlyResponse> EndAsync(string key, string segmentation = null, int? count = 1, double? sum = 0,
            bool addToRequestQueue = false)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(key, "Key is required.");

            var addedEvent = Countly.TotalEvents.Where(e => key.Equals(e.Key, StringComparison.OrdinalIgnoreCase))
                                        .FirstOrDefault();
            if (string.IsNullOrEmpty(addedEvent.Key))
                throw new Exception($"Event {key} doesn't exists. Please start an event first.");

            var evnt = new CountlyEventModel(key, segmentation, count, sum);
            evnt.Duration = evnt.Duration ?? (DateTime.Now - addedEvent.Value).TotalMilliseconds;

            SetTimeZoneInfo(evnt, addedEvent.Value);

            var requestParams =
               new Dictionary<string, object>
               {
                    { "events", JsonConvert.SerializeObject(evnt, Formatting.Indented,
                                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }) },
               };

            var res = await CountlyHelper.GetResponseAsync(requestParams, addToRequestQueue);

            //Removing the event
            if (res.IsSuccess)
                Countly.TotalEvents.Remove(key);

            return res;
        }

        internal static void SetTimeZoneInfo(CountlyEventModel evnt, DateTime requestDatetime)
        {
            var timezoneInfo = TimeMetricModel.GetTimeZoneInfoForRequest(requestDatetime);
            evnt.Timestamp = timezoneInfo.Timestamp;
            evnt.DayOfWeek = timezoneInfo.DayOfWeek;
            evnt.Hour = timezoneInfo.Hour;
            evnt.Timezone = timezoneInfo.Timezone;
        }
    }
}