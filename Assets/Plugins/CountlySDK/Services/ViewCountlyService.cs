using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Helpers;
using Plugins.CountlySDK.Models;
using UnityEngine;

namespace Plugins.CountlySDK.Services
{

    public class ViewCountlyService : AbstractBaseService
    {
        private readonly EventCountlyService _eventService;
        private readonly CountlyConfiguration _configuration;
        private readonly Dictionary<string, DateTime> _viewToLastViewStartTime = new Dictionary<string, DateTime>();

        internal ViewCountlyService(CountlyConfiguration configuration, CountlyLogHelper logHelper, EventCountlyService eventService, ConsentCountlyService consentService) : base(logHelper, consentService)
        {
            Log.Debug("[ViewCountlyService] Initializing.");

            _eventService = eventService;
            _configuration = configuration;
        }
        /// <summary>
        /// Start tracking a view
        /// </summary>
        /// <param name="name"></param>
        /// <param name="hasSessionBegunWithView"></param>
        /// <returns></returns>
        public async Task RecordOpenViewAsync(string name, bool hasSessionBegunWithView = false)
        {
            Log.Info("[ViewCountlyService] RecordOpenViewAsync : name = " + name + ", hasSessionBegunWithView = " + hasSessionBegunWithView);

            if (!_consentService.CheckConsent(Consents.Views)) {
                return;
            }

            if (string.IsNullOrEmpty(name)) {
                return;
            }


            ViewSegment currentViewSegment =
                new ViewSegment {
                    Name = name,
                    Segment = Constants.UnityPlatform,
                    Visit = 1,
                    Exit = 0,
                    Bounce = 0,
                    HasSessionBegunWithView = hasSessionBegunWithView
                };

            if (!_viewToLastViewStartTime.ContainsKey(name)) {
                _viewToLastViewStartTime.Add(name, DateTime.UtcNow);
            }

            CountlyEventModel currentView = new CountlyEventModel(CountlyEventModel.ViewEvent, currentViewSegment.ToDictionary());
            await _eventService.RecordEventAsync(currentView);
        }

        /// <summary>
        /// Stop tracking a view
        /// </summary>
        /// <param name="name"></param>
        /// <param name="hasSessionBegunWithView"></param>
        /// <returns></returns>
        public async Task RecordCloseViewAsync(string name, bool hasSessionBegunWithView = false)
        {
            Log.Info("[ViewCountlyService] RecordCloseViewAsync : name = " + name);

            if (!_consentService.CheckConsent(Consents.Views)) {
                return;
            }

            if (string.IsNullOrEmpty(name)) {
                return;
            }

            ViewSegment currentViewSegment =
                new ViewSegment {
                    Name = name,
                    Segment = Constants.UnityPlatform,
                    Visit = 0,
                    Exit = 1,
                    Bounce = 0,
                    HasSessionBegunWithView = hasSessionBegunWithView
                };

            double? duration = null;
            if (_viewToLastViewStartTime.ContainsKey(name)) {
                DateTime lastViewStartTime = _viewToLastViewStartTime[name];
                duration = (DateTime.UtcNow - lastViewStartTime).TotalSeconds;

                _viewToLastViewStartTime.Remove(name);
            }

            CountlyEventModel currentView = new CountlyEventModel(CountlyEventModel.ViewEvent, currentViewSegment.ToDictionary(), 1, null, duration);
            await _eventService.RecordEventAsync(currentView);
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
        public async Task ReportActionAsync(string type, int x, int y, int width, int height)
        {
            Log.Info("[ViewCountlyService] ReportActionAsync : type = " + type + ", x = " + x + ", y = " + y + ", width = " + width + ", height = " + height);

            if (!_consentService.CheckConsent(Consents.Views)) {
                return;
            }

            ActionSegment segment =
                new ActionSegment {
                    Type = type,
                    PositionX = x,
                    PositionY = y,
                    Width = width,
                    Height = height
                };

            await _eventService.ReportCustomEventAsync(CountlyEventModel.ViewActionEvent, segment.ToDictionary());
        }

        #region override Methods
        internal override void DeviceIdChanged(string deviceId, bool merged)
        {

        }

        internal override void ConsentChanged(List<Consents> updatedConsents, bool newConsentValue)
        {

        }
        #endregion

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
                Dictionary<string, object> dict = new Dictionary<string, object>
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