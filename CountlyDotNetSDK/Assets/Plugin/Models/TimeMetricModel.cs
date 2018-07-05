using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Assets.Plugin.Models
{
    class TimeMetricModel
    {
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }
        [JsonProperty("hour")]
        public int Hour { get; set; }
        [JsonProperty("dow")]
        public int DayOfWeek { get; set; }
        [JsonProperty("tz")]
        public string TZ { get; set; }

        //variable to hold last used timestamp
        private DateTimeOffset _lastMilliSecTimeStamp = DateTimeOffset.UtcNow;

        static TimeMetricModel() { }
        private TimeMetricModel() { }

        internal static Dictionary<string, object> GetTimeMetricModel()
        {
            var currentDateTime = DateTime.Now;
            var model =
                new TimeMetricModel
                {
                    Hour = currentDateTime.TimeOfDay.Hours,
                    DayOfWeek = (int)currentDateTime.DayOfWeek,
                };
            model.Timestamp = model.GetUniqueMilliSecTimeStamp();

            return new Dictionary<string, object>
            {
                {"timestamp", model.Timestamp },
                {"hour", model.Hour },
                {"dow", model.DayOfWeek },
            };
        }

        private string GetUniqueMilliSecTimeStamp()
        {
            //get current timestamp in miliseconds
            var currentMilliSecTimeSatmp = DateTimeOffset.UtcNow;

            _lastMilliSecTimeStamp = _lastMilliSecTimeStamp >= currentMilliSecTimeSatmp
                                    ? _lastMilliSecTimeStamp.AddMilliseconds(1)
                                    : _lastMilliSecTimeStamp = currentMilliSecTimeSatmp;

            return _lastMilliSecTimeStamp.ToUnixTimeMilliseconds().ToString();
        }
    }
}
