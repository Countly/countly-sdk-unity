using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugins.CountlySDK.Models;

namespace Plugins.CountlySDK.Services
{
    public class StarRatingCountlyService : IBaseService
    {

        private readonly EventCountlyService _eventCountlyService;

        internal StarRatingCountlyService(EventCountlyService eventCountlyService)
        {
            _eventCountlyService = eventCountlyService;
        }

        public void DeviceIdChanged(string deviceId, bool merged)
        {
            
        }


        /// <summary>
        /// Sends app rating to the server.
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="appVersion"></param>
        /// <param name="rating">Rating should be from 1 to 5</param>
        /// <returns></returns>
        public async Task ReportStarRatingAsync(string platform, string appVersion, int rating)
        {
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