﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Models;

namespace Plugins.CountlySDK.Services
{
    public class StarRatingCountlyService : AbstractBaseService
    {
        private readonly EventCountlyService _eventCountlyService;

        internal StarRatingCountlyService(CountlyConfiguration configuration, CountlyLogHelper logHelper, ConsentCountlyService consentService, EventCountlyService eventCountlyService) : base(configuration, logHelper, consentService)
        {
            Log.Debug("[StarRatingCountlyService] Initializing.");

            _eventCountlyService = eventCountlyService;
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
        /// Sends app rating to the server.
        /// </summary>
        /// <param name="platform">name of platform</param>
        /// <param name="appVersion">the current version of the app</param>
        /// <param name="rating">value from 0 to 5 that will be set as the rating value</param>
        /// <returns></returns>
        public async Task ReportStarRatingAsync(string platform, string appVersion, int rating)
        {
            Log.Info("[StarRatingCountlyService] ReportStarRatingAsync");

            if (!_consentService.CheckConsentInternal(Consents.Star_Rating)) {
                return;
            }

            if (rating < 1 || rating > 5) {
                return;
            }

            StarRatingSegment segment =
                new StarRatingSegment {
                    Platform = platform,
                    AppVersion = appVersion,
                    Rating = rating,
                };

            await _eventCountlyService.ReportCustomEventAsync(
                CountlyEventModel.StarRatingEvent, segment.ToDictionary(),
                null, null, null);
        }


        /// <summary>
        /// Custom Segmentation for Star Rating event.
        /// </summary>
        [Serializable]
        struct StarRatingSegment
        {
            public string Platform { get; set; }
            public string AppVersion { get; set; }
            public int Rating { get; set; }

            public IDictionary<string, object> ToDictionary()
            {
                return new Dictionary<string, object>()
                {
                    {"platform", Platform},
                    {"app_version", AppVersion},
                    {"rating", Rating},
                };
            }

        }

    }
}