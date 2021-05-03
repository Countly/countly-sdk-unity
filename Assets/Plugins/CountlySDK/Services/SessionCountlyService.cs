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

        /// <summary>
        /// Check if session has been initiated.
        /// </summary>
        /// <returns>bool</returns>
        internal bool IsSessionInitiated { get; private set; }

        private readonly LocationService _locationService;
        private readonly EventCountlyService _eventService;
        private readonly RequestCountlyHelper _requestCountlyHelper;

        internal SessionCountlyService(CountlyConfiguration configuration, CountlyLogHelper logHelper, EventCountlyService eventService,
            RequestCountlyHelper requestCountlyHelper, LocationService locationService, ConsentCountlyService consentService) : base(configuration, logHelper, consentService)
        {
            Log.Debug("[SessionCountlyService] Initializing.");

            _eventService = eventService;
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
        /// <param name="sender">reference of caller</param>
        /// <param name="elapsedEventArgs"> Provides data for <code>Timer.Elapsed</code>event.</param>
        private async void SessionTimerOnElapsedAsync(object sender, ElapsedEventArgs elapsedEventArgs)
        {

            Log.Debug("[SessionCountlyService] SessionTimerOnElapsedAsync");

            if (!IsSessionInitiated) {
                return;
            }

            await _eventService.AddEventsToRequestQueue();

            await _requestCountlyHelper.ProcessQueue();

            if (!_configuration.EnableManualSessionHandling) {
                await ExtendSessionAsync();
            }
        }

        /// <summary>
        /// Initiates a session
        /// </summary>
        internal async Task BeginSessionAsync()
        {
            Log.Debug("[SessionCountlyService] BeginSessionAsync");

            if (!_consentService.CheckConsentInternal(Consents.Sessions)) {
                return;
            }

            if (IsSessionInitiated) {
                return;
            }

            _lastSessionRequestTime = DateTime.Now;
            //Session initiated
            IsSessionInitiated = true;

            Dictionary<string, object> requestParams =
                new Dictionary<string, object>();


            requestParams.Add("begin_session", 1);

            /* If location is disabled or no location consent is given,
            the SDK adds an empty location entry to every "begin_session" request. */
            if (_locationService.IsLocationDisabled || !_consentService.CheckConsentInternal(Consents.Location)) {
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

        /// <summary>
        /// Ends a session
        /// </summary>
        internal async Task EndSessionAsync()
        {
            Log.Debug("[SessionCountlyService] EndSessionAsync");

            if (!_consentService.CheckConsentInternal(Consents.Sessions)) {
                return;
            }

            if (!IsSessionInitiated) {
                return;
            }

            IsSessionInitiated = false;

            await _eventService.AddEventsToRequestQueue();

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
                _sessionTimer.Stop();
                _sessionTimer.Dispose();
                _sessionTimer.Close();
                _sessionTimer = null;
            }
        }


        /// <summary>
        /// Extends a session by another session duration provided in configuration. By default session duration is 60 seconds.
        /// </summary>
        internal async Task ExtendSessionAsync()
        {
            Log.Debug("[SessionCountlyService] ExtendSessionAsync");

            if (!_consentService.CheckConsentInternal(Consents.Sessions)) {
                return;
            }

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