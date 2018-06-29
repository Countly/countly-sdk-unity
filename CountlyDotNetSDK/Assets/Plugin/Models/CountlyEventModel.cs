using Assets.Plugin.Scripts;
using Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Plugin.Models
{
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
        public string Start => HasSessionBegunWithView ? "true" : null;
    }

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

    [Serializable]
    class CountlyEventModel
    {
        #region Reserved Event Names

        [JsonIgnore]
        public const string ViewEvent = "[CLY]_view";
        [JsonIgnore]
        public const string ViewActionEvent = "[CLY]_action";
        [JsonIgnore]
        public const string StarRatingEvent = "[CLY]_star_rating";

        #endregion

        [JsonProperty("key")]
        public string Key { get; set; }
        [JsonProperty("count")]
        public int? Count { get; set; }
        [JsonProperty("sum")]
        public double? Sum { get; set; }
        [JsonProperty("dur")]
        public double? Duration { get; set; }
        [JsonProperty("segmentation")]
        public JRaw Segmentation { get; set; }

        /// <summary>
        /// Initializes an instance of event model. It doesn't mark the event as started instead it reports them.
        /// It is used for internal custom events like reporting views, actions, etc.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="segmentation"></param>
        /// <param name="duration"></param>
        internal CountlyEventModel(string key, string segmentation, double? duration)
        {
            Key = key;
            Count = 1;
            Segmentation = new JRaw(segmentation);
            Duration = duration;
        }

        /// <summary>
        /// Initializes an instance of event model. It also marks the event as started. 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="segmentation"></param>
        /// <param name="sum"></param>
        /// <param name="count"></param>
        /// <param name="dur"></param>
        public CountlyEventModel(string key, string segmentation = null, double? sum = null, int? count = 1, double? dur = null)
        {
            Key = key;
            Count = count;
            Segmentation = new JRaw(segmentation);
            Duration = dur;
            Sum = sum;

            Start();
        }


        private void Start()
        {
            if (string.IsNullOrEmpty(Key) && string.IsNullOrWhiteSpace(Key))
                throw new ArgumentNullException(Key, "Key is required.");

            if (Countly.TotalEvents.Any(evnt => Key.Equals(evnt.Key, StringComparison.OrdinalIgnoreCase)))
                throw new Exception($"Event {Key} already started. Please end this event first.");

            Countly.TotalEvents.Add(Key, DateTime.Now);
        }

        /// <summary>
        /// Ends a particular event
        /// </summary>
        /// <returns></returns>
        public bool End()
        {
            if (string.IsNullOrEmpty(Key))
                throw new ArgumentNullException(Key, "Key is required.");

            var addedEvent = Countly.TotalEvents.Where(evnt => Key.Equals(evnt.Key, StringComparison.OrdinalIgnoreCase))
                                        .FirstOrDefault();
            if (string.IsNullOrEmpty(addedEvent.Key))
                throw new Exception($"Event {Key} doesn't exists. Please add an event first.");

            Duration = Duration ?? (DateTime.Now - addedEvent.Value).TotalMilliseconds;

            var requestParams =
               new Dictionary<string, object>
               {
                    { "events", JsonConvert.SerializeObject(this, Formatting.Indented,
                                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }) },
               };
            var requestString = CountlyHelper.BuildRequest(requestParams);
            CountlyHelper.GetResponse(requestString);

            //Removing the event
            Countly.TotalEvents.Remove(Key);

            return true;
        }

        internal static bool StartMultipleEvents(string events)
        {
            var requestParams =
               new Dictionary<string, object>
               {
                    { "events", events },
               };
            var requestString = CountlyHelper.BuildRequest(requestParams);
            CountlyHelper.GetResponse(requestString);

            return true;
        }

        internal static bool StartMultipleEvents(List<CountlyEventModel> events)
        {
            var requestParams =
               new Dictionary<string, object>
               {
                    { "events", JsonConvert.SerializeObject(events, Formatting.Indented,
                                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }) },
               };
            var requestString = CountlyHelper.BuildRequest(requestParams);
            CountlyHelper.GetResponse(requestString);

            return true;
        }

        internal bool ReportCustomEvent()
        {
            if (string.IsNullOrEmpty(Key))
                throw new ArgumentNullException(Key, "Key is required.");

            var requestParams =
               new Dictionary<string, object>
               {
                    { "events", JsonConvert.SerializeObject(this, Formatting.Indented,
                                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }) },
               };
            var requestString = CountlyHelper.BuildRequest(requestParams);
            CountlyHelper.GetResponse(requestString);

            return true;
        }
    }
}