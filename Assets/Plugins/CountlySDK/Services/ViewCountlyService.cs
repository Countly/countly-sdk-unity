using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugins.CountlySDK.Helpers;
using Plugins.CountlySDK.Models;
using UnityEngine;

namespace Plugins.CountlySDK.Services
{

    public class ViewCountlyService
    {
        private readonly CountlyConfigModel _config;
        private readonly Dictionary<string, DateTime> _viewToLastViewStartTime = new Dictionary<string, DateTime>();

        private readonly EventCountlyService _eventService;

        internal ViewCountlyService(CountlyConfigModel config, EventCountlyService eventService)
        {
            _config = config;
            _eventService = eventService;
        }
        /// <summary>
        /// Start tracking a view
        /// </summary>
        /// <param name="name"></param>
        /// <param name="hasSessionBegunWithView"></param>
        /// <returns></returns>
        public async Task<CountlyResponse> RecordOpenViewAsync(string name, bool hasSessionBegunWithView = false)
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
                    Exit = 0,
                    Bounce = 0,
                    HasSessionBegunWithView = hasSessionBegunWithView
                };

            if (!_viewToLastViewStartTime.ContainsKey(name))
            {
                _viewToLastViewStartTime.Add(name, DateTime.UtcNow);   
            }

            if (_config.EnableConsoleLogging)
            {
                Debug.Log("[ViewCountlyService] RecordOpenViewAsync: " + name);
            }
            
            var currentView = new CountlyEventModel(CountlyEventModel.ViewEvent, currentViewSegment.ToDictionary());
            return await _eventService.RecordEventAsync(currentView);
        }

        /// <summary>
        /// Stop tracking a view
        /// </summary>
        /// <param name="name"></param>
        /// <param name="hasSessionBegunWithView"></param>
        /// <returns></returns>
        public async Task<CountlyResponse> RecordCloseViewAsync(string name, bool hasSessionBegunWithView = false)
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
                    Bounce = 0,
                    HasSessionBegunWithView = hasSessionBegunWithView
                };

            double? duration = null;
            if (_viewToLastViewStartTime.ContainsKey(name))
            {
                var lastViewStartTime = _viewToLastViewStartTime[name];
                duration = (DateTime.UtcNow - lastViewStartTime).TotalSeconds;

                _viewToLastViewStartTime.Remove(name);
            }

            if (_config.EnableConsoleLogging)
            {
                Debug.Log("[ViewCountlyService] RecordCloseViewAsync: " + name + ", duration: " + duration);
            }

            var currentView = new CountlyEventModel(CountlyEventModel.ViewEvent, currentViewSegment.ToDictionary(), 1, null, duration);
            return await _eventService.RecordEventAsync(currentView);
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