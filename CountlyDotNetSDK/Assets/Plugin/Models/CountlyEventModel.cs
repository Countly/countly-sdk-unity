using Assets.Plugin.Scripts;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Plugin.Models
{
    [Serializable]
    class CountlyEventModel
    {
        public string key;
        public int? count;
        public string segmentation;
        public double? dur;
        public double? sum;

        public CountlyEventModel(string _key, string _segmentation = null, double? _sum = null, int? _count = 1, double? _dur = null)
        {
            key = _key;
            count = _count;
            segmentation = _segmentation;
            dur = _dur;
            sum = _sum;

            Start();
        }

        private void Start()
        {
            if (string.IsNullOrEmpty(key) && string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(key, "Key is required.");

            if (Countly.TotalEvents.Any(evnt => key.Equals(evnt.Key, StringComparison.OrdinalIgnoreCase)))
                throw new Exception($"Event {key} already started. Please end this event first.");

            Countly.TotalEvents.Add(key, DateTime.Now);
        }

        public bool End()
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(key, "Key is required.");

            var addedEvent = Countly.TotalEvents.Where(evnt => key.Equals(evnt.Key, StringComparison.OrdinalIgnoreCase))
                                        .FirstOrDefault();
            if (string.IsNullOrEmpty(addedEvent.Key))
                throw new Exception($"Event {key} doesn't exists. Please add an event first.");

            dur = (DateTime.Now - addedEvent.Value).TotalMilliseconds;

            var requestParams =
               new Dictionary<string, object>
               {
                    { "events", JsonUtility.ToJson(this) },
               };
            var requestString = CountlyHelper.BuildRequest(requestParams);
            CountlyHelper.GetResponse(requestString);

            //Removing the event
            Countly.TotalEvents.Remove(key);

            return true;
        }
    }
}
