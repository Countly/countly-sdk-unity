using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;

namespace Plugins.Countly.Models
{
    internal class TimeMetricModel
    {
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }
        [JsonProperty("hour")]
        public int Hour { get; set; }
        [JsonProperty("dow")]
        public int DayOfWeek { get; set; }
        [JsonProperty("tz")]
        public string Timezone { get; set; }

        //variable to hold last used timestamp
        private DateTimeOffset _lastMilliSecTimeStamp = DateTimeOffset.UtcNow;

        static TimeMetricModel() { }
        private TimeMetricModel() { }

        internal static Dictionary<string, object> GetTimeMetricModel()
        {
            var currentDateTime = DateTime.Now;
            var model = TimeMetricModel.GetTimeZoneInfoForRequest(currentDateTime);

            model.Timestamp = model.GetUniqueMilliSecTimeStamp();
            return new Dictionary<string, object>
            {
                {"timestamp", model.Timestamp },
                {"hour", model.Hour },
                {"dow", model.DayOfWeek },
                {"tz", model.Timezone },
            };
        }

        private long GetUniqueMilliSecTimeStamp(DateTime? requestedDatetime = null)
        {
            //get current timestamp in miliseconds
            var currentMilliSecTimeStamp = DateTimeOffset.UtcNow;

            if (requestedDatetime.HasValue)
            {
                currentMilliSecTimeStamp = requestedDatetime.Value;

                _lastMilliSecTimeStamp = _lastMilliSecTimeStamp >= currentMilliSecTimeStamp
                                        ? _lastMilliSecTimeStamp.AddMilliseconds(1)
                                        : _lastMilliSecTimeStamp = currentMilliSecTimeStamp;
            }
            else
            {
                _lastMilliSecTimeStamp = currentMilliSecTimeStamp;
            }

            return _lastMilliSecTimeStamp.ToUnixTimeMilliseconds();
        }

        internal static TimeMetricModel GetTimeZoneInfoForRequest(DateTime requestedDatetime)
        {
            var currentDateTime = DateTime.Now;
            var model =
                new TimeMetricModel
                {
                    Hour = currentDateTime.TimeOfDay.Hours,
                    DayOfWeek = (int)currentDateTime.DayOfWeek,
                    Timezone = TimeZone.CurrentTimeZone.GetUtcOffset(new DateTime()).TotalMinutes.ToString(CultureInfo.InvariantCulture)
                };
            
            model.Timestamp = model.GetUniqueMilliSecTimeStamp(requestedDatetime);
            return model;
        }
    }
}
