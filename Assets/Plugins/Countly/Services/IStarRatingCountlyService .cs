using System.Threading.Tasks;
using Plugins.Countly.Helpers;

namespace Plugins.Countly.Services
{
    public interface IStarRatingCountlyService
    {
        /// <summary>
        /// Sends app rating to the server.
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="appVersion"></param>
        /// <param name="rating">Rating should be from 1 to 5</param>
        /// <returns></returns>
        Task<CountlyResponse> ReportStarRatingAsync(string platform, string appVersion, int rating);
    }
}