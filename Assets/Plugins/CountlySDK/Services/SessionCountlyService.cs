using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Helpers;
using Plugins.CountlySDK.Models;
using UnityEngine;

namespace Plugins.CountlySDK.Services
{
    public class SessionCountlyService : AbstractBaseService
    {
        private Timer _sessionTimer;
        private bool _isSessionInitiated;
        private DateTime _lastSessionRequestTime;

        private readonly LocationService _locationService;
        private readonly CountlyConfiguration _configModel;
        private readonly EventCountlyService _eventService;
        private readonly RequestCountlyHelper _requestCountlyHelper;

        internal SessionCountlyService(CountlyConfiguration configModel, EventCountlyService eventService,
            RequestCountlyHelper requestCountlyHelper, LocationService locationService, ConsentCountlyService consentService) : base(consentService)
        {
            _configModel = configModel;
            _eventService = eventService;
            _locationService = locationService;
            _requestCountlyHelper = requestCountlyHelper;
        }

        #region private Methods
        /// <summary>
        /// Initializes the timer for extending session with specified interval
        /// </summary>
        /// <param name="sessionInterval">In milliseconds</param>
        private void InitSessionTimer()
        {
            if (_configModel.EnableManualSessionHandling) {
                return;
            }

            _sessionTimer = new Timer { Interval = _configModel.SessionDuration * 1000 };
            _sessionTimer.Elapsed += SessionTimerOnElapsedAsync;
            _sessionTimer.AutoReset = true;
        }

        /// <summary>
        /// Extends the session after the session duration is elapsed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="elapsedEventArgs"></param>
        private async void SessionTimerOnElapsedAsync(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (!_isSessionInitiated) {
                return;
            }

            await _eventService.AddEventsToRequestQueue();

            await _requestCountlyHelper.ProcessQueue();

            if (!_configModel.EnableManualSessionHandling) {
                await ExecuteExtendSessionAsync();
            }
        }

        #endregion

        #region internal Methods
        internal async Task ExecuteBeginSessionAsync()
        {
            if (!_consentService.CheckConsent(Consents.Sessions)) {
                return;
            }

            if (_isSessionInitiated) {
                return;
            }

            if (_configModel.EnableConsoleLogging) {
                Debug.Log("[Countly] SessionCountlyService: ExecuteBeginSessionAsync");
            }

            FirstLaunchAppHelper.Process();
            _lastSessionRequestTime = DateTime.Now;
            //Session initiated
            _isSessionInitiated = true;

            Dictionary<string, object> requestParams =
                new Dictionary<string, object>();


            requestParams.Add("begin_session", 1);

            /* If location is disabled or no location consent is given,
            the SDK adds an empty location entry to every "begin_session" request. */
            if (_locationService.IsLocationDisabled || !_consentService.CheckConsent(Consents.Location)) {
                requestParams.Add("location", string.Empty);
            } else {
                if (!string.IsNullOrEmpty(_locationService.IPAddress)) {
                    requestParams.Add("ip_address", _locationService.IPAddress);
                }

                if (!string.IsNullOrEmpty(_locationService.CountryCode)) {
                    requestParams.Add("country_code", _locationService.CountryCode);
                }

                if (!string.IsNullOrEmpty(_locationService.City)) {
                    requestParams.Add("city", _locationService.City);
                }

                if (!string.IsNullOrEmpty(_locationService.Location)) {
                    requestParams.Add("location", _locationService.Location);
                }
            }

            requestParams.Add("metrics", JsonConvert.SerializeObject(CountlyMetricModel.Metrics, Formatting.Indented,
            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

            await _requestCountlyHelper.GetResponseAsync(requestParams);

            //Start session timer
            if (!_configModel.EnableManualSessionHandling) {
                InitSessionTimer();
                _sessionTimer.Start();
            }
        }

        internal async Task ExecuteExtendSessionAsync()
        {
            if (!_consentService.CheckConsent(Consents.Sessions)) {
                return;
            }

            if (!_isSessionInitiated) {
                return;
            }

            _lastSessionRequestTime = DateTime.Now;
            Dictionary<string, object> requestParams =
                new Dictionary<string, object>
                {
                    {
                        "session_duration", _configModel.SessionDuration
                    },
                    {"ignore_cooldown", _configModel.IgnoreSessionCooldown.ToString().ToLower()}
                };
            requestParams.Add("metrics", JsonConvert.SerializeObject(CountlyMetricModel.Metrics, Formatting.Indented,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

            await _requestCountlyHelper.GetResponseAsync(requestParams);

        }

        internal async Task ExecuteEndSessionAsync()
        {
            if (!_consentService.CheckConsent(Consents.Sessions)) {
                return;
            }

            if (!_isSessionInitiated) {
                return;
            }

            if (_configModel.EnableConsoleLogging) {
                Debug.Log("[Countly] SessionCountlyService: ExecuteEndSessionAsync");
            }

            _isSessionInitiated = false;

            Dictionary<string, object> requestParams =
                new Dictionary<string, object>
                {
                    {"end_session", 1},
                    {"session_duration", (DateTime.Now - _lastSessionRequestTime).TotalSeconds},
                    {"ignore_cooldown", _configModel.IgnoreSessionCooldown.ToString().ToLower()}
                };
            requestParams.Add("metrics", JsonConvert.SerializeObject(CountlyMetricModel.Metrics, Formatting.Indented,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

            await _requestCountlyHelper.GetResponseAsync(requestParams);

            _sessionTimer.Stop();
            _sessionTimer.Dispose();
            _sessionTimer.Close();
            _sessionTimer = null;
        }

        #endregion

        #region public Methods

        /// <summary>
        /// Initiates a session
        /// </summary>
        public async Task BeginSession()
        {
            await ExecuteBeginSessionAsync();
        }

        /// <summary>
        /// Extends a session
        /// </summary>
        public async Task ExtendSession()
        {
            await ExecuteExtendSessionAsync();
        }

        /// <summary>
        /// Ends a session
        /// </summary>
        ///
        public async Task EndSession()
        {
            await ExecuteEndSessionAsync();
        }

        #endregion

        #region override Methods
        internal override void DeviceIdChanged(string deviceId, bool merged)
        {

        }

        internal override void ConsentChanged(List<Consents> updatedConsents, bool newConsentValue)
        {

        }
        #endregion
    }
}