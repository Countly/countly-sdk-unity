using System;
using System.Collections.Generic;

namespace Assets.Plugin.Models
{
    public class TimeMetricModel
    {
        public string timestamp;
        public int hour;
        public int dow;
        public string tz;

        //variable to hold last used timestamp
        private DateTime _lastMilliSecTimeStamp;

        static TimeMetricModel() { }
        private TimeMetricModel() { }

        public static Dictionary<string, object> GetTimeMetricModel()
        {
            var currentDateTime = DateTime.Now;
            var model = 
                new TimeMetricModel
                {
                    hour = currentDateTime.TimeOfDay.Hours,
                    dow = (int)currentDateTime.DayOfWeek,
                };
            model.timestamp = model.GetUniqueMilliSecTimeStamp();

            return new Dictionary<string, object>
            {
                {"timestamp", model.timestamp },
                {"hour", model.hour },
                {"dow", model.dow },
            };
        }

        private string GetUniqueMilliSecTimeStamp()
        {
            //get current timestamp in miliseconds
            var currentMilliSecTimeSatmp = DateTime.Now;

            _lastMilliSecTimeStamp = _lastMilliSecTimeStamp >= currentMilliSecTimeSatmp
                                    ? _lastMilliSecTimeStamp.AddMilliseconds(1)
                                    : _lastMilliSecTimeStamp = currentMilliSecTimeSatmp;

            return _lastMilliSecTimeStamp.ToString("HHmmssffff");
        }
    }
}
