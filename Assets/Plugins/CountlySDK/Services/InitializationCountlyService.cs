using System.Collections.Generic;
using System.Threading.Tasks;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Models;

namespace Plugins.CountlySDK.Services
{
    public class InitializationCountlyService : AbstractBaseService
    {
        private readonly PushCountlyService _pushService;
        private readonly LocationService _locationService;
        private readonly CountlyConfiguration _configModel;
        private readonly SessionCountlyService _sessionService;

        internal InitializationCountlyService(CountlyConfiguration configModel, PushCountlyService pushService, LocationService locationService, SessionCountlyService sessionCountlyService)
        {
            _pushService = pushService;
            _configModel = configModel;
            _locationService = locationService;
            _sessionService = sessionCountlyService;
        }

        public string ServerUrl { get; private set; }
        public string AppKey { get; private set; }

        #region override Methods
        internal override void DeviceIdChanged(string deviceId, bool merged)
        {

        }

        internal override void ConsentChanged(Dictionary<Features, bool> updatedConsents)
        {

        }
        #endregion

        /// <summary>
        ///     Initializes countly instance
        /// </summary>
        /// <param name="serverUrl"></param>
        /// <param name="appKey"></param>
        /// <param name="deviceId"></param>
        internal async Task OnInitializationComplete()
        {
            AppKey = _configModel.AppKey;
            ServerUrl = _configModel.ServerUrl;

            if (!Consent.CheckConsent(Features.Sessions)) {
                /* If location is disabled in init
                and no session consent is given. Send empty location as separate request.*/
                if (_locationService.IsLocationDisabled) {
                    await _locationService.SendRequestWithEmptyLocation();
                } else {
                    /*
                 * If there is no session consent, 
                 * location values set in init should be sent as a separate location request.
                 */
                    await _locationService.SendIndependantLocationRequest();
                }
            } else if (!_configModel.EnableManualSessionHandling) {
                //Start Session
                await _sessionService.BeginSessionAsync();
            }

            if (_configModel.EnableTestMode || !Consent.CheckConsent(Features.Push) || _configModel.NotificationMode == TestMode.None) {
                return;
            }

            //Enables push notification on start
            _pushService.EnablePushNotificationAsync(_configModel.NotificationMode);
        }
    }
}