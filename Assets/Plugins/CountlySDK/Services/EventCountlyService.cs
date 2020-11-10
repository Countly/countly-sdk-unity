using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Plugins.CountlySDK.Helpers;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Persistance.Repositories.Impls;
using UnityEngine;

namespace Plugins.CountlySDK.Services
{
    public class EventCountlyService
    {
        private readonly CountlyConfigModel _countlyConfigModel;
        private readonly RequestCountlyHelper _requestCountlyHelper;
        private readonly ViewEventRepository _viewEventRepo;
        private readonly NonViewEventRepository _nonViewEventRepo;
        private readonly EventNumberInSameSessionHelper _eventNumberInSameSessionHelper;

        internal EventCountlyService(CountlyConfigModel countlyConfigModel, RequestCountlyHelper requestCountlyHelper, 
            ViewEventRepository viewEventRepo, NonViewEventRepository nonViewEventRepo, EventNumberInSameSessionHelper eventNumberInSameSessionHelper)
        {
            _countlyConfigModel = countlyConfigModel;
            _requestCountlyHelper = requestCountlyHelper;
            _viewEventRepo = viewEventRepo;
            _nonViewEventRepo = nonViewEventRepo;
            _eventNumberInSameSessionHelper = eventNumberInSameSessionHelper;
        }

        internal async Task<CountlyResponse> RecordEventAsync(CountlyEventModel @event, bool useNumberInSameSession = false)
        {

            if (_countlyConfigModel.EnableConsoleLogging)
            {
                Debug.Log("[Countly] RecordEventAsync : " + @event.ToString());
            }

            if (_countlyConfigModel.EnableTestMode)
            {
                return new CountlyResponse
                {
                    IsSuccess = true
                };
            }

            if (_countlyConfigModel.EnableFirstAppLaunchSegment)
            {
                AddFirstAppSegment(@event);   
            }

            if (@event.Key.Equals(CountlyEventModel.ViewEvent))
            {
                _viewEventRepo.Enqueue(@event);
            }
            else
            {
                _nonViewEventRepo.Enqueue(@event);
            }

            if (useNumberInSameSession)
            {
                _eventNumberInSameSessionHelper.IncreaseNumberInSameSession(@event);
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

        /// <summary>
        /// Report an event to the server.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="useNumberInSameSession"></param>
        /// <returns></returns>
        public async Task<CountlyResponse> RecordEventAsync(string key, bool useNumberInSameSession = false)
        {
            return await RecordEventAsync(key, null, useNumberInSameSession);
        }

        /// <summary>
        /// Report an event to the server with segmentation.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="segmentation"></param>
        /// <param name="useNumberInSameSession"></param>
        /// <param name="count"></param>
        /// <param name="sum"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public async Task<CountlyResponse> RecordEventAsync(string key, SegmentModel segmentation, bool useNumberInSameSession = false,
            int? count = 1, double? sum = 0, double? duration = null)
        {
            if (_countlyConfigModel.EnableConsoleLogging)
            {
                Debug.Log("[Countly] RecordEventAsync : key = " + key);
            }

            if (_countlyConfigModel.EnableTestMode)
            {
                return new CountlyResponse
                {
                    IsSuccess = true
                };
            }

            if (string.IsNullOrEmpty(key) && string.IsNullOrWhiteSpace(key))
            {
                return new CountlyResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Key is required."
                };            
            }
            
            var @event = new CountlyEventModel(key, segmentation, count, sum, duration);
            
            if (useNumberInSameSession)
            {
                _eventNumberInSameSessionHelper.IncreaseNumberInSameSession(@event);
            }
            
            return await RecordEventAsync(@event);
        }

        /// <summary>
        ///     Reports all recorded view events to the server
        /// </summary>
        /// <returns></returns>
        internal async Task<CountlyResponse> ReportAllRecordedViewEventsAsync()
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
            
            var res = await _requestCountlyHelper.GetResponseAsync(requestParams);

            return res;
        }
        
        
        
        /// <summary>
        ///     Reports all recorded events to the server
        /// </summary>
        internal async Task<CountlyResponse> ReportAllRecordedNonViewEventsAsync()
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
            
            var res = await _requestCountlyHelper.GetResponseAsync(requestParams);

            return res;
        }

        /// <summary>
        ///     Sends multiple events to the countly server. It expects a list of events as input.
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        internal async Task<CountlyResponse> ReportMultipleEventsAsync(List<CountlyEventModel> events)
        {
            if (events == null || events.Count == 0)
                return new CountlyResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "No events found."
                };

            if (_countlyConfigModel.EnableFirstAppLaunchSegment)
            {
                foreach (var evt in events)
                {
                    AddFirstAppSegment(evt);
                }       
            }

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

            if (_countlyConfigModel.EnableFirstAppLaunchSegment)
            {
                AddFirstAppSegment(evt);   
            }
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

        private void AddFirstAppSegment(CountlyEventModel @event)
        {
            if (@event.Segmentation == null)
            {
                @event.Segmentation = new SegmentModel();
            }
            @event.Segmentation.Add(Constants.FirstAppLaunchSegment, FirstLaunchAppHelper.IsFirstLaunchApp);
        }
    }
}