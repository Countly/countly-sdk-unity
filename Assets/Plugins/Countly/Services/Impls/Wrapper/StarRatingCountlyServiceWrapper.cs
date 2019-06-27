using System.Threading.Tasks;
using Plugins.Countly.Helpers;
using UnityEngine;

namespace Plugins.Countly.Services.Impls.Wrapper
{
    public class StarRatingCountlyServiceWrapper : IStarRatingCountlyService
    {
        public Task<CountlyResponse> ReportStarRatingAsync(string platform, string appVersion, int rating)
        {
            Debug.Log("[StarRatingCountlyServiceWrapper] ReportStarRatingAsync, platform: " + platform + ", appVersion: " + appVersion + ", rating: " + rating);
            return Task.FromResult(new CountlyResponse());
        }
    }
}