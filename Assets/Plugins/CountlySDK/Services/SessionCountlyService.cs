﻿using System;
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
        internal Timer _sessionTimer;
        internal DateTime _lastSessionRequestTime;

        /// <summary>
        /// Check if session has been initiated.
        /// </summary>
        /// <returns>bool</returns>
        internal bool IsSessionInitiated { get; private set; }

        private readonly LocationService _locationService;
        private readonly EventCountlyService _eventService;
        internal readonly RequestCountlyHelper _requestCountlyHelper;

        internal SessionCountlyService(CountlyConfiguration configuration, CountlyLogHelper logHelper, EventCountlyService eventService,
            RequestCountlyHelper requestCountlyHelper, LocationService locationService, ConsentCountlyService consentService) : base(configuration, logHelper, consentService)
        {
            Log.Debug("[SessionCountlyService] Initializing.");

            _eventService = eventService;
            _locationService = locationService;
            _requestCountlyHelper = requestCountlyHelper;

            if (_configuration.IsAutomaticSessionTrackingDisabled) {
                Log.Verbose("[Countly][CountlyConfiguration] Automatic session tracking disabled!");
            }
        }

        /// <summary>
        /// Run session startup logic and start timer with the specified interval
        /// </summary>
        internal async Task StartSessionService()
        {
            if (!_consentService.CheckConsentInternal(Consents.Sessions)) {
                /* If location is disabled in init
                and no session consent is given. Send empty location as separate request.*/
                if (_locationService.IsLocationDisabled || !_consentService.CheckConsentInternal(Consents.Location)) {
                    await _locationService.SendRequestWithEmptyLocation();
                } else {
                    /*
                 * If there is no session consent, 
                 * location values set in init should be sent as a separate location request.
                 */
                    await _locationService.SendIndependantLocationRequest();
                }
            } else {
                if (!_configuration.IsAutomaticSessionTrackingDisabled) {
                    //Start Session
                    await BeginSessionAsync();
                } 
            }

            InitSessionTimer();
        }

        /// <summary>
        /// Initializes the timer for extending session with specified interval
        /// </summary>
        private void InitSessionTimer()
        {
            _sessionTimer = new Timer { Interval = _configuration.SessionDuration * 1000 };
            _sessionTimer.Elapsed += SessionTimerOnElapsedAsync;
            _sessionTimer.AutoReset = true;
            _sessionTimer.Start();
        }

        /// <summary>
        /// Extends the session after the session duration is elapsed
        /// </summary>
        /// <param name="sender">reference of caller</param>
        /// <param name="elapsedEventArgs"> Provides data for <code>Timer.Elapsed</code>event.</param>
        private async void SessionTimerOnElapsedAsync(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            Log.Debug("[SessionCountlyService] SessionTimerOnElapsedAsync");

            await _eventService.AddEventsToRequestQueue();
            await _requestCountlyHelper.ProcessQueue();

            if (!_configuration.IsAutomaticSessionTrackingDisabled) {
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
                Log.Warning("[SessionCountlyService] BeginSessionAsync: The session has already started!");
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
                Log.Warning("[SessionCountlyService] EndSessionAsync: The session isn't started yet!");
                return;
            }

            IsSessionInitiated = false;

            await _eventService.AddEventsToRequestQueue();

            Dictionary<string, object> requestParams =
                new Dictionary<string, object>
                {
                    {"end_session", 1},
                    {"session_duration", (DateTime.Now - _lastSessionRequestTime).TotalSeconds}
                };
           

            await _requestCountlyHelper.GetResponseAsync(requestParams);
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

            if (!IsSessionInitiated) {
                Log.Warning("[SessionCountlyService] ExtendSessionAsync: The session isn't started yet!");
                return;
            }

            _lastSessionRequestTime = DateTime.Now;
            Dictionary<string, object> requestParams =
                new Dictionary<string, object>
                {
                    {
                        "session_duration", _configuration.SessionDuration
                    }
                };

            await _requestCountlyHelper.GetResponseAsync(requestParams);

        }

        #region override Methods
        internal override void DeviceIdChanged(string deviceId, bool merged)
        {

        }

        internal override async void ConsentChanged(List<Consents> updatedConsents, bool newConsentValue)
        {
            if (updatedConsents.Contains(Consents.Sessions) && newConsentValue) {
                if (!_configuration.IsAutomaticSessionTrackingDisabled) {
                    IsSessionInitiated = false;
                    await BeginSessionAsync();
                }
            }

        }
        #endregion
    }
}