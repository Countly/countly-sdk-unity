using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Plugins.CountlySDK.Persistance;

namespace Plugins.CountlySDK.Models
{
    [Serializable]
    public class CountlyEventModel : IModel
    {
        /// <summary>
        ///     Initializes a new instance of event model.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="segmentation"></param>
        /// <param name="count"></param>
        /// <param name="sum"></param>
        /// <param name="duration"></param>
        public CountlyEventModel(string key, IDictionary<string, object> segmentation = null, int? count = 1,
            double? sum = null,
            double? duration = null)
        {
            Key = key;
            Count = count ?? 1;
            if (segmentation != null) {
                Segmentation = new SegmentModel(segmentation);
            }
            Duration = duration;
            Sum = sum;

            TimeMetricModel timeModel = TimeMetricModel.GetTimeZoneInfoForRequest();

            Hour = timeModel.Hour;
            DayOfWeek = timeModel.DayOfWeek;
            Timestamp = timeModel.Timestamp;
        }

        public CountlyEventModel()
        {
        }

        [JsonIgnore]
        public long Id { get; set; }

        [JsonProperty("key")] public string Key { get; set; }

        [JsonProperty("count")] public int? Count { get; set; }

        [JsonProperty("sum")] public double? Sum { get; set; }

        [JsonProperty("dur")] public double? Duration { get; set; }

        [JsonProperty("segmentation")] public SegmentModel Segmentation { get; set; }

        [JsonProperty("timestamp")] public long Timestamp { get; set; }

        [JsonProperty("hour")] public int Hour { get; set; }

        [JsonProperty("dow")] public int DayOfWeek { get; set; }

        [JsonIgnore]
        [Obsolete("Timezone is deprecated, it will get removed in the future.")]
        private double Timezone { get; set; }

        //        [JsonIgnore] public DateTime TimeRecorded { get; set; }

        #region Reserved Event Names

        [JsonIgnore] public const string NPSEvent = "[CLY]_nps";

        [JsonIgnore] public const string ViewEvent = "[CLY]_view";

        [JsonIgnore] public const string SurveyEvent = "[CLY]_survey";

        [JsonIgnore] public const string ViewActionEvent = "[CLY]_action";

        [JsonIgnore] public const string StarRatingEvent = "[CLY]_star_rating";

        [JsonIgnore] public const string PushActionEvent = "[CLY]_push_action";

        [JsonIgnore] public const string OrientationEvent = "[CLY]_orientation";

        

        #endregion

        protected bool Equals(CountlyEventModel other)
        {
            return Id == other.Id && string.Equals(Key, other.Key) && Count == other.Count && Sum.Equals(other.Sum) && Duration.Equals(other.Duration) && Equals(Segmentation, other.Segmentation) && Timestamp == other.Timestamp && Hour == other.Hour && DayOfWeek == other.DayOfWeek && Timezone.Equals(other.Timezone);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((CountlyEventModel)obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                int hashCode = Id.GetHashCode();
                hashCode = (hashCode * 397) ^ (Key != null ? Key.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Count.GetHashCode();
                hashCode = (hashCode * 397) ^ Sum.GetHashCode();
                hashCode = (hashCode * 397) ^ Duration.GetHashCode();
                hashCode = (hashCode * 397) ^ (Segmentation != null ? Segmentation.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Timestamp.GetHashCode();
                hashCode = (hashCode * 397) ^ Hour;
                hashCode = (hashCode * 397) ^ DayOfWeek;
                hashCode = (hashCode * 397) ^ Timezone.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Key)}: {Key}, {nameof(Count)}: {Count}, {nameof(Sum)}: {Sum}, {nameof(Duration)}: {Duration}, {nameof(Segmentation)}: {Segmentation}, {nameof(Timestamp)}: {Timestamp}, {nameof(Hour)}: {Hour}, {nameof(DayOfWeek)}: {DayOfWeek}, {nameof(Timezone)}: {Timezone}";
        }
    }
}
