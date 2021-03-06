﻿using System;
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
        internal bool _isFirstView = true;
        internal readonly EventCountlyService _eventService;
        private readonly Dictionary<string, DateTime> _viewToLastViewStartTime = new Dictionary<string, DateTime>();

        internal ViewCountlyService(CountlyConfiguration configuration, CountlyLogHelper logHelper, EventCountlyService eventService, ConsentCountlyService consentService) : base(configuration, logHelper, consentService)
        {
            Log.Debug("[ViewCountlyService] Initializing.");

            _eventService = eventService;
        }
        /// <summary>
        /// Start tracking a view
        /// </summary>
        /// <param name="name">name of the view</param>
        /// <param name="hasSessionBegunWithView">set true if the session is beginning with this view</param>
        /// <returns></returns>
        [Obsolete("RecordOpenViewAsync(string name, bool hasSessionBegunWithView) is deprecated, please use RecordOpenViewAsync(string name) instead.")]
        public async Task RecordOpenViewAsync(string name, bool hasSessionBegunWithView)
        {
            Log.Info("[ViewCountlyService] RecordOpenViewAsync : name = " + name + ", hasSessionBegunWithView = " + hasSessionBegunWithView);

            await RecordOpenViewAsync(name);
        }

        /// <summary>
        /// Start tracking a view
        /// </summary>
        /// <param name="name">name of the view</param>
        /// <returns></returns>
        public async Task RecordOpenViewAsync(string name)
        {
            lock (LockObj) {
                Log.Info("[ViewCountlyService] RecordOpenViewAsync : name = " + name);

                if (!_consentService.CheckConsentInternal(Consents.Views)) {
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
                        Start = _isFirstView ? 1 : 0
                    };

                if (!_viewToLastViewStartTime.ContainsKey(name)) {
                    _viewToLastViewStartTime.Add(name, DateTime.UtcNow);
                }

                CountlyEventModel currentView = new CountlyEventModel(CountlyEventModel.ViewEvent, currentViewSegment.OpenViewDictionary());
                _=_eventService.RecordEventAsync(currentView);

                _isFirstView = false;
            }
        }

        /// <summary>
        /// Stop tracking a view
        /// </summary>
        /// <param name="name of the view"></param>
        /// <param name="hasSessionBegunWithView">set true if the session is beginning with this view</param>
        /// <returns></returns>
        [Obsolete("RecordCloseViewAsync(string name, bool hasSessionBegunWithView) is deprecated, please use RecordCloseViewAsync(string name) instead.")]
        public async Task RecordCloseViewAsync(string name, bool hasSessionBegunWithView)
        {
            Log.Info("[ViewCountlyService] RecordCloseViewAsync : name = " + name + ", hasSessionBegunWithView = " + hasSessionBegunWithView);

            await RecordCloseViewAsync(name);

        }

        /// <summary>
        /// Stop tracking a view
        /// </summary>
        /// <param name="name of the view"></param>
        /// <returns></returns>
        public async Task RecordCloseViewAsync(string name)
        {
            lock (LockObj) {
                Log.Info("[ViewCountlyService] RecordCloseViewAsync : name = " + name);

                if (!_consentService.CheckConsentInternal(Consents.Views)) {
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
                        Start = 0
                    };

                double? duration = null;
                if (_viewToLastViewStartTime.ContainsKey(name)) {
                    DateTime lastViewStartTime = _viewToLastViewStartTime[name];
                    duration = (DateTime.UtcNow - lastViewStartTime).TotalSeconds;

                    _viewToLastViewStartTime.Remove(name);
                }

                IDictionary<string, object> segment = currentViewSegment.CloseViewDictionary();

                CountlyEventModel currentView = new CountlyEventModel(CountlyEventModel.ViewEvent, segment, 1, null, duration);
                _=_eventService.RecordEventAsync(currentView);
            }
        }

        /// <summary>
        /// Reports a particular action with the specified details
        /// </summary>
        /// <param name="type"> type of action</param>
        /// <param name="x">x-coordinate</param>
        /// <param name="y">y-coordinate</param>
        /// <param name="width">width of screen</param>
        /// <param name="height">height of screen</param>
        /// <returns></returns>
        public async Task ReportActionAsync(string type, int x, int y, int width, int height)
        {
            lock (LockObj) {
                Log.Info("[ViewCountlyService] ReportActionAsync : type = " + type + ", x = " + x + ", y = " + y + ", width = " + width + ", height = " + height);

                if (!_consentService.CheckConsentInternal(Consents.Views)) {
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

                CountlyEventModel currentView = new CountlyEventModel(CountlyEventModel.ViewActionEvent, segment.ToDictionary());
                _=_eventService.RecordEventAsync(currentView);
            }
        }

        #region override Methods
        internal override void DeviceIdChanged(string deviceId, bool merged)
        {
            if (!merged) {
                _isFirstView = true;
            }
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
            public int Start { get; set; }

            public IDictionary<string, object> OpenViewDictionary()
            {
                Dictionary<string, object> dict = new Dictionary<string, object>
                {
                    {"name", Name},
                    {"segment", Segment},
                    {"visit", Visit},
                    {"start", Start}
                };
                return dict;
            }

            public IDictionary<string, object> CloseViewDictionary()
            {
                Dictionary<string, object> dict = new Dictionary<string, object>
                {
                    {"name", Name},
                    {"segment", Segment},
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
