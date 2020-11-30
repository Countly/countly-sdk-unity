using System;
using System.Threading.Tasks;
using Plugins.CountlySDK.Helpers;
using Plugins.CountlySDK.Models;

namespace Plugins.CountlySDK.Services
{
    public class InitializationCountlyService
    {
        private readonly SessionCountlyService _sessionCountlyService;

        internal InitializationCountlyService(SessionCountlyService sessionCountlyService)
        {
            _sessionCountlyService = sessionCountlyService;
        }

        public string ServerUrl { get; private set; }
        public string AppKey { get; private set; }

        /// <summary>
        ///     Initializes countly instance
        /// </summary>
        /// <param name="serverUrl"></param>
        /// <param name="appKey"></param>
        /// <param name="deviceId"></param>
        internal void Begin(string serverUrl, string appKey)
        {
            ServerUrl = serverUrl;
            AppKey = appKey;

            if (string.IsNullOrEmpty(ServerUrl))
                throw new ArgumentNullException(serverUrl, "Server URL is required.");
            if (string.IsNullOrEmpty(AppKey))
                throw new ArgumentNullException(appKey, "App Key is required.");


            //ConsentGranted = consentGranted;
        }

        /// <summary>
        ///     Initializes the Countly SDK with default values
        /// </summary>
        /// <param name="salt"></param>
        /// <param name="enablePost"></param>
        /// <param name="enableConsoleErrorLogging"></param>
        /// <param name="ignoreSessionCooldown"></param>
        /// <returns></returns>
        internal async Task SetDefaults(CountlyConfiguration configModel)
        {
            if (!configModel.EnableManualSessionHandling)
            {
                //Start Session and enable push notification
                await _sessionCountlyService.BeginSessionAsync();
            }
        }


        /// <summary>
        ///     Gets the base url to make requests to the Countly server.
        /// </summary>
        /// <returns></returns>
        internal string GetBaseUrl()
        {
            return string.Format(ServerUrl[ServerUrl.Length - 1] == '/' ? "{0}i?" : "{0}/i?", ServerUrl);
        }
    }
}