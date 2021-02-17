using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Helpers;
using Plugins.CountlySDK.Models;

namespace Plugins.CountlySDK.Services
{
    public class SessionCountlyService : AbstractBaseService
    {
        private Timer _sessionTimer;
        private DateTime _lastSessionRequestTime;
        public bool IsSessionInitiated { get; private set; }

        private readonly LocationService _locationService;
        private readonly EventCountlyService _eventService;
        private readonly CountlyConfiguration _configuration;
        private readonly RequestCountlyHelper _requestCountlyHelper;

        internal SessionCountlyService(CountlyConfiguration configuration, CountlyLogHelper logHelper, EventCountlyService eventService,
            RequestCountlyHelper requestCountlyHelper, LocationService locationService, ConsentCountlyService consentService) : base(logHelper, consentService)
        {
            Log.Debug("[SessionCountlyService] Initializing.");

            _eventService = eventService;
            _configuration = configuration;
            _locationService = locationService;
            _requestCountlyHelper = requestCountlyHelper;
        }

        /// <summary>
        /// Initializes the timer for extending session with specified interval
        /// </summary>
        /// <param name="sessionInterval">In milliseconds</param>
        private void InitSessionTimer()
        {
            if (_configuration.EnableManualSessionHandling) {
                return;
            }

            _sessionTimer = new Timer { Interval = _configuration.SessionDuration * 1000 };
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
            if (!IsSessionInitiated) {
                return;
            }

            Log.Debug("[SessionCountlyService] SessionTimerOnElapsedAsync");

            await _eventService.AddEventsToRequestQueue();

            await _requestCountlyHelper.ProcessQueue();

            if (!_configuration.EnableManualSessionHandling) {
                await ExtendSessionAsync();
            }
        }

        public async Task ExecuteBeginSessionAsync()
        {
            if (!_consentService.CheckConsent(Consents.Sessions)) {
                return;
            }

            if (IsSessionInitiated) {
                return;
            }

            Log.Info("[SessionCountlyService] ExecuteBeginSessionAsync");

            FirstLaunchAppHelper.Process();
            _lastSessionRequestTime = DateTime.Now;
            //Session initiated
            IsSessionInitiated = true;

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
            if (!_configuration.EnableManualSessionHandling) {
                InitSessionTimer();
                _sessionTimer.Start();
            }
        }

        public async Task ExecuteEndSessionAsync(bool disposeTimer = true)
        {
            if (!_consentService.CheckConsent(Consents.Sessions)) {
                return;
            }

            Log.Info("[SessionCountlyService] ExecuteEndSessionAsync");


            IsSessionInitiated = false;

            Dictionary<string, object> requestParams =
                new Dictionary<string, object>
                {
                    {"end_session", 1},
                    {"session_duration", (DateTime.Now - _lastSessionRequestTime).TotalSeconds},
                    {"ignore_cooldown", _configuration.IgnoreSessionCooldown.ToString().ToLower()}
                };
            requestParams.Add("metrics", JsonConvert.SerializeObject(CountlyMetricModel.Metrics, Formatting.Indented,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

            await _requestCountlyHelper.GetResponseAsync(requestParams);

            if (!_configuration.EnableManualSessionHandling) {
                //Do not extend session after session ends
                if (disposeTimer) {
                    _sessionTimer.Stop();
                    _sessionTimer.Dispose();
                    _sessionTimer.Close();
                    _sessionTimer = null;
                }
            }
        }


        /// <summary>
        /// Initiates a session by setting begin_session
        /// </summary>
        public async Task BeginSessionAsync()
        {
            Log.Info("[SessionCountlyService] BeginSessionAsync");

            await ExecuteBeginSessionAsync();
        }

        /// <summary>
        /// Ends a session by setting end_session
        /// </summary>
        public async Task EndSessionAsync()
        {
            if (!_consentService.CheckConsent(Consents.Sessions)) {
                return;
            }

            Log.Info("[SessionCountlyService] ExtendSessionAsync");

            await ExecuteEndSessionAsync();
        }

        /// <summary>
        /// Extends a session by another 60 seconds
        /// </summary>
        public async Task ExtendSessionAsync()
        {
            if (!_consentService.CheckConsent(Consents.Sessions)) {
                return;
            }

            Log.Info("[SessionCountlyService] ExtendSessionAsync");


            _lastSessionRequestTime = DateTime.Now;
            Dictionary<string, object> requestParams =
                new Dictionary<string, object>
                {
                    {
                        "session_duration", _configuration.SessionDuration
                    },
                    {"ignore_cooldown", _configuration.IgnoreSessionCooldown.ToString().ToLower()}
                };
            requestParams.Add("metrics", JsonConvert.SerializeObject(CountlyMetricModel.Metrics, Formatting.Indented,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

            await _requestCountlyHelper.GetResponseAsync(requestParams);

        }

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