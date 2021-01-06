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
    public class SessionCountlyService : IBaseService
    {
        private Timer _sessionTimer;
        private DateTime _lastSessionRequestTime;
        public bool IsSessionInitiated { get; private set; }

        private readonly LocationService _locationService;
        private readonly CountlyConfiguration _configModel;
        private readonly EventCountlyService _eventService;
        private readonly ConsentCountlyService _consentService;
        private readonly PushCountlyService _pushCountlyService;
        private readonly RequestCountlyHelper _requestCountlyHelper;

        internal SessionCountlyService(CountlyConfiguration configModel, EventCountlyService eventService, PushCountlyService pushCountlyService,
            RequestCountlyHelper requestCountlyHelper, LocationService locationService, ConsentCountlyService consentService)
        {
            _configModel = configModel;
            _eventService = eventService;
            _consentService = consentService;
            _locationService = locationService;
            _pushCountlyService = pushCountlyService;
            _requestCountlyHelper = requestCountlyHelper;
        }

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
            if (!IsSessionInitiated) {
                return;
            }

            await _eventService.AddEventsToRequestQueue();

            await _requestCountlyHelper.ProcessQueue();

            if (!_configModel.EnableManualSessionHandling) {
                await ExtendSessionAsync();
            }
        }

        public async Task ExecuteBeginSessionAsync()
        {
            if (IsSessionInitiated) {
                return;
            }

            if (_configModel.EnableConsoleLogging) {
                Debug.Log("[Countly] SessionCountlyService: ExecuteBeginSessionAsync");
            }

            FirstLaunchAppHelper.Process();
            _lastSessionRequestTime = DateTime.Now;
            //Session initiated
            IsSessionInitiated = true;

            Dictionary<string, object> requestParams =
                new Dictionary<string, object>();

            if (_consentService.CheckConsent(Features.Sessions)) {
                requestParams.Add("begin_session", 1);

                /* If location is disabled or no location consent is given,
				the SDK adds an empty location entry to every "begin_session" request. */
                if (_locationService.IsLocationDisabled || !_consentService.CheckConsent(Features.Location)) {
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

            //Start session timer
            if (!_configModel.EnableManualSessionHandling) {
                InitSessionTimer();
                _sessionTimer.Start();
            }
        }

        public async Task ExecuteEndSessionAsync(bool disposeTimer = true)
        {
            IsSessionInitiated = false;

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

            if (!_configModel.EnableManualSessionHandling) {
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
            await ExecuteBeginSessionAsync();

            if (_configModel.EnableTestMode) {
                return;
            }

            //Enables push notification on start
            if (_configModel.NotificationMode != TestMode.None) {
                _pushCountlyService.EnablePushNotificationAsync(_configModel.NotificationMode);
            }
        }

        /// <summary>
        /// Ends a session by setting end_session
        /// </summary>
        public async Task EndSessionAsync()
        {
            if (_configModel.EnableConsoleLogging) {
                Debug.Log("[Countly] SessionCountlyService: ExtendSessionAsync");
            }

            await ExecuteEndSessionAsync();
        }

        /// <summary>
        /// Extends a session by another 60 seconds
        /// </summary>
        public async Task ExtendSessionAsync()
        {
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

        public void DeviceIdChanged(string deviceId, bool merged)
        {

        }


        #region Unused Code

        ///// <summary>
        ///// The method must be used only when Manual Session Handling is disabled.
        ///// Sets the session duration. Session will be extended each time this interval elapses. The interval value must be in seconds.
        ///// </summary>
        ///// <param name="interval"></param>
        //private static void SetSessionDuration(int interval)
        //{
        //    if (!IsManualSessionHandlingEnabled)
        //    {
        //        _extendSessionInterval = interval;
        //        _sessionTimer.Interval = _extendSessionInterval * 1000;
        //    }
        //}

        #endregion
    }
}