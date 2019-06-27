using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugins.Countly.Helpers;
using Plugins.Countly.Models;
using UnityEngine;

namespace Plugins.Countly.Services.Impls.Actual
{

    public class ViewCountlyService : IViewCountlyService
    {        
        private string _lastView;
        private DateTime? _lastViewStartTime;

        private readonly IEventCountlyService _eventService;

        public ViewCountlyService(IEventCountlyService eventService)
        {
            _eventService = eventService;
        }

        public async Task<CountlyResponse> ReportOpenViewAsync(string name, bool hasSessionBegunWithView = false)
        {
            if (string.IsNullOrEmpty(name))
            {
                return new CountlyResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "View name is required."
                };
            }
            
            var currentViewSegment =
                new ViewSegment
                {
                    Name = name,
                    Segment = Constants.UnityPlatform,
                    Visit = 1,
                    HasSessionBegunWithView = hasSessionBegunWithView
                };

            
            _lastViewStartTime = DateTime.UtcNow;

            var currentView = new CountlyEventModel(CountlyEventModel.ViewEvent, currentViewSegment.ToDictionary());
            return await _eventService.RecordEventAsync(currentView);
        }
        
        public async Task<CountlyResponse> ReportCloseViewAsync(string name, bool hasSessionBegunWithView = false)
        {
            if (string.IsNullOrEmpty(name))
            {
                return new CountlyResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "View name is required."
                };
            }
            
            var currentViewSegment =
                new ViewSegment
                {
                    Name = name,
                    Segment = Constants.UnityPlatform,
                    Visit = 0,
                    Exit =  1,
                    HasSessionBegunWithView = hasSessionBegunWithView
                };


            double? duration;
            duration = _lastViewStartTime != null ? (DateTime.UtcNow - _lastViewStartTime.Value).TotalSeconds : (double?) null;
            if (duration.HasValue)
            {
                duration = Math.Round(duration.Value);
            }

            var currentView = new CountlyEventModel(CountlyEventModel.ViewEvent, currentViewSegment.ToDictionary(), 1, null, duration);
            return await _eventService.RecordEventAsync(currentView);
        }
        
        
        
        
        /// <summary>
        /// Reports a view with the specified name and a last visited view if it existed
        /// </summary>
        /// <param name="name"></param>
        /// <param name="hasSessionBegunWithView"></param>
        public async Task<CountlyResponse> ReportViewAsync(string name, bool hasSessionBegunWithView = false)
        {
            if (string.IsNullOrEmpty(name))
            {
                return new CountlyResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "View name is required."
                };
            }

            var events = new List<CountlyEventModel>();
            var lastView = GetLastView();
            Debug.Log("[ReportViewAsync] get last view: " + lastView);
            if (lastView != null)
                events.Add(lastView);

            var currentViewSegment =
                new ViewSegment
                {
                    Name = name,
                    Segment = Constants.UnityPlatform,
                    Visit = 1,
                    HasSessionBegunWithView = hasSessionBegunWithView
                };

            var currentView = new CountlyEventModel(CountlyEventModel.ViewEvent, currentViewSegment.ToDictionary());
            events.Add(currentView);
            
            _lastView = name;
            _lastViewStartTime = DateTime.Now;
            
            return await _eventService.ReportMultipleEventsAsync(events);
        }

        private CountlyEventModel GetLastView(bool hasSessionBegunWithView = false)
        {
            if (string.IsNullOrEmpty(_lastView) && string.IsNullOrWhiteSpace(_lastView))
                return null;

            var viewSegment =
                new ViewSegment
                {
                    Name = _lastView,
                    Segment = Constants.UnityPlatform,
                    HasSessionBegunWithView = hasSessionBegunWithView
                };

            var customEvent = new CountlyEventModel(
                                    CountlyEventModel.ViewEvent, viewSegment.ToDictionary(),
                                    null, null, (DateTime.Now - _lastViewStartTime.Value).TotalSeconds);

            return customEvent;
        }
        

        /// <summary>
        /// Reports a particular action with the specified details
        /// </summary>
        /// <param name="type"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public async Task<CountlyResponse> ReportActionAsync(string type, int x, int y, int width, int height)
        {
            var segment =
                new ActionSegment
                {
                    Type = type,
                    PositionX = x,
                    PositionY = y,
                    Width = width,
                    Height = height
                };

            return await _eventService.ReportCustomEventAsync(CountlyEventModel.ViewActionEvent, segment.ToDictionary());
        }
        
        /// <summary>
        /// Custom Segmentation for Views related events.
        /// </summary>
        [Serializable]
        class ViewSegment
        {
            public string Name { get; set; }
            public string Segment { get; set; }
            public int Visit { get; set; }
            public int Exit { get; set; }
            public int Bounce { get; set; }
            public bool HasSessionBegunWithView { get; set; }
            private int Start => HasSessionBegunWithView ? 1 : 0;
            
            public IDictionary<string, object> ToDictionary()
            {
                var dict = new Dictionary<string, object>
                {
                    {"name", Name}, 
                    {"segment", Segment}, 
                    {"exit", Exit}, 
                    {"visit", Visit}, 
                    {"start", Start},
                    {"bounce", Bounce}
                };
                return dict;
            }
        }

        
        /// <summary>
        /// Custom Segmentation for Action related events.
        /// </summary>
        [Serializable]
        class ActionSegment
        {
            public string Type { get; set; }
            public int PositionX { get; set; }
            public int PositionY { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            
            public IDictionary<string, object> ToDictionary()
            {
                return new Dictionary<string, object>()
                {
                    {"type", Type},
                    {"x", PositionX},
                    {"y", PositionY},
                    {"width", Width},
                    {"height", Height},
                };
            }
        }


    }
}