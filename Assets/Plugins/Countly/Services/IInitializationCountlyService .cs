using System.Threading.Tasks;
using Plugins.Countly.Helpers;
using Plugins.Countly.Models;

namespace Plugins.Countly.Services
{
    public interface IInitializationCountlyService
    {
        string ServerUrl { get; }
        string AppKey { get; }

        /// <summary>
        ///     Initializes countly instance
        /// </summary>
        /// <param name="serverUrl"></param>
        /// <param name="appKey"></param>
        /// <param name="deviceId"></param>
        void Begin(string serverUrl, string appKey);

        /// <summary>
        ///     Initializes the Countly SDK with default values
        /// </summary>
        /// <param name="salt"></param>
        /// <param name="enablePost"></param>
        /// <param name="enableConsoleErrorLogging"></param>
        /// <param name="ignoreSessionCooldown"></param>
        /// <returns></returns>
        Task<CountlyResponse> SetDefaults(CountlyConfigModel configModel);

        /// <summary>
        ///     Gets the base url to make requests to the Countly server.
        /// </summary>
        /// <returns></returns>
        string GetBaseUrl();
    }
}