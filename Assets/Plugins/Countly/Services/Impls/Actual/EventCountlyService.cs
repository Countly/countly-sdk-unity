using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Plugins.Countly.Helpers;
using Plugins.Countly.Models;
using Plugins.Countly.Persistance.Repositories.Impls;

namespace Plugins.Countly.Services.Impls.Actual
{
    public class EventCountlyService : IEventCountlyService
    {
        private readonly CountlyConfigModel _countlyConfigModel;
        private readonly RequestCountlyHelper _requestCountlyHelper;
        private readonly ViewEventRepository _viewEventRepo;
        private readonly NonViewEventRepository _nonViewEventRepo;

        internal EventCountlyService(CountlyConfigModel countlyConfigModel, RequestCountlyHelper requestCountlyHelper, 
            ViewEventRepository viewEventRepo, NonViewEventRepository nonViewEventRepo)
        {
            _countlyConfigModel = countlyConfigModel;
            _requestCountlyHelper = requestCountlyHelper;
            _viewEventRepo = viewEventRepo;
            _nonViewEventRepo = nonViewEventRepo;
        }

        
        public async Task<CountlyResponse> RecordEventAsync(CountlyEventModel @event)
        {
            if (@event.Key.Equals(CountlyEventModel.ViewEvent))
            {
                _viewEventRepo.Enqueue(@event);
            }
            else
            {
                _nonViewEventRepo.Enqueue(@event);
            }

            if (_viewEventRepo.Count >= _countlyConfigModel.EventViewSendThreshold)
                await ReportAllRecordedViewEventsAsync();

            if (_nonViewEventRepo.Count >= _countlyConfigModel.EventNonViewSendThreshold)
                await ReportAllRecordedNonViewEventsAsync();

            return new CountlyResponse
            {
                IsSuccess = true
            };
        }
        
        
        public async Task<CountlyResponse> RecordEventAsync(string key)
        {
            if (string.IsNullOrEmpty(key) && string.IsNullOrWhiteSpace(key))
            {
                return new CountlyResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Key is required."
                };
            
            }

            var @event = new CountlyEventModel(key);
            return await RecordEventAsync(@event);
        }

        public async Task<CountlyResponse> RecordEventAsync(string key, IDictionary<string, object> segmentation,
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
            
            var @event = new CountlyEventModel(key, segmentation, count, sum, duration);
            return await RecordEventAsync(@event);
        }
        
        

        /// <summary>
        ///     Reports all recorded view events to the server
        /// </summary>
        public async Task<CountlyResponse> ReportAllRecordedViewEventsAsync(bool addToRequestQueue = false)
        {
            if (_viewEventRepo.Models.Count == 0)
            {
                return new CountlyResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "No events to send"
                };
            }
            
            //Send all at once
            var requestParams =
                new Dictionary<string, object>
                {
                    {
                        "events", JsonConvert.SerializeObject(_viewEventRepo.Models, Formatting.Indented,
                            new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore})
                    }
                };

            
            //Even if res = false all events should be removed because responses are stored locally.
            _viewEventRepo.Clear();    
            
            var res = await _requestCountlyHelper.GetResponseAsync(requestParams, addToRequestQueue);

            return res;
        }
        
        
        
        /// <summary>
        ///     Reports all recorded events to the server
        /// </summary>
        public async Task<CountlyResponse> ReportAllRecordedNonViewEventsAsync(bool addToRequestQueue = false)
        {
            if (_nonViewEventRepo.Models.Count == 0)
            {
                return new CountlyResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "No events to send"
                };
            }

            //Send all at once
            var requestParams =
                new Dictionary<string, object>
                {
                    {
                        "events", JsonConvert.SerializeObject(_nonViewEventRepo.Models, Formatting.Indented,
                            new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore})
                    }
                };

            
            //Even if res = false all events should be removed because responses are stored locally.
            _nonViewEventRepo.Clear();    
            
            var res = await _requestCountlyHelper.GetResponseAsync(requestParams, addToRequestQueue);

            return res;
        }

        /// <summary>
        ///     Sends multiple events to the countly server. It expects a list of events as input.
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        public async Task<CountlyResponse> ReportMultipleEventsAsync(List<CountlyEventModel> events)
        {
            if (events == null || events.Count == 0)
                return new CountlyResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "No events found."
                };

//            var currentTime = DateTime.UtcNow;
//            foreach (var evt in events)
//            {
//                SetTimeZoneInfo(evt, currentTime);
//            }

            var requestParams =
                new Dictionary<string, object>
                {
                    {
                        "events", JsonConvert.SerializeObject(events, Formatting.Indented,
                            new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore})
                    }
                };
            return await _requestCountlyHelper.GetResponseAsync(requestParams);
        }

        /// <summary>
        ///     Reports a custom event to the Counlty server.
        /// </summary>
        /// <returns></returns>
        public async Task<CountlyResponse> ReportCustomEventAsync(string key,
            IDictionary<string, object> segmentation = null,
            int? count = 1, double? sum = null, double? duration = null)
        {
            if (string.IsNullOrEmpty(key) && string.IsNullOrWhiteSpace(key))
                return new CountlyResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Key is required."
                };

            var evt = new CountlyEventModel(key, segmentation, count, sum, duration);
//            SetTimeZoneInfo(evt, DateTime.UtcNow);

            var requestParams =
                new Dictionary<string, object>
                {
                    {
                        "events", JsonConvert.SerializeObject(new List<CountlyEventModel> {evt}, Formatting.Indented,
                            new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore})
                    }
                };
            return await _requestCountlyHelper.GetResponseAsync(requestParams);
        }

//        private void SetTimeZoneInfo(CountlyEventModel evt, DateTime requestDatetime)
//        {
//            var timezoneInfo = TimeMetricModel.GetTimeZoneInfoForRequest(requestDatetime);
//            evt.Timestamp = timezoneInfo.Timestamp;
//            evt.DayOfWeek = timezoneInfo.DayOfWeek;
//            evt.Hour = timezoneInfo.Hour;
//            evt.Timezone = timezoneInfo.Timezone;
//        }
    }
}