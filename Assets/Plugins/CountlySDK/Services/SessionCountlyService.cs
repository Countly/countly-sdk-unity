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
	public class SessionCountlyService
	{
		private Timer _sessionTimer;
		private DateTime _lastSessionRequestTime;
		public bool IsSessionInitiated { get; private set; }

		private DateTime _lastInputTime;

		private readonly LocationService _locationService;
		private readonly CountlyConfiguration _configModel;
		private readonly EventCountlyService _eventService;
		private readonly ConsentCountlyService _consentService;
		private readonly PushCountlyService _pushCountlyService;
        private readonly RequestCountlyHelper _requestCountlyHelper;
        private readonly EventNumberInSameSessionHelper _eventNumberInSameSessionHelper;

        internal SessionCountlyService(CountlyConfiguration configModel, EventCountlyService eventService, PushCountlyService pushCountlyService, 
            RequestCountlyHelper requestCountlyHelper, LocationService recordLocationService, ConsentCountlyService consentCountlyService,
			EventNumberInSameSessionHelper eventNumberInSameSessionHelper)
        {
            _configModel = configModel;
			_eventService = eventService;
			_pushCountlyService = pushCountlyService;
            _requestCountlyHelper = requestCountlyHelper;
            _locationService = recordLocationService;
			_consentService = consentCountlyService;
			_eventNumberInSameSessionHelper = eventNumberInSameSessionHelper;
        }

		/// <summary>
		/// Initializes the timer for extending session with specified interval
		/// </summary>
		/// <param name="sessionInterval">In milliseconds</param>
		private void InitSessionTimer()
		{
			if (_configModel.EnableManualSessionHandling) return;
			_sessionTimer = new Timer {Interval = _configModel.SessionDuration * 1000};
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
            if (!IsSessionInitiated)
            {
                return;
            }

			await _eventService.AddEventsToRequestQueue();

			await _requestCountlyHelper.ProcessQueue();
            var sessionOver = (DateTime.Now - _lastInputTime).TotalSeconds >= _configModel.SessionDuration;

            if (sessionOver)
            {
                await ExecuteEndSessionAsync();
            }
            else if (!_configModel.EnableManualSessionHandling)
            {
                await ExtendSessionAsync();
            }
        }

		public async void UpdateInputTime()
		{
			_lastInputTime = DateTime.Now;

			if (!IsSessionInitiated && _sessionTimer == null) //session was over
			{
				await ExecuteBeginSessionAsync();
			}
		}

		public async Task ExecuteBeginSessionAsync()
		{
			FirstLaunchAppHelper.Process();
			_lastSessionRequestTime = DateTime.Now;
			//Session initiated
			IsSessionInitiated = true;
			_eventNumberInSameSessionHelper.RemoveAllEvents();

			var requestParams =
				new Dictionary<string, object>();

			if (_consentService.CheckConsent(FeaturesEnum.Sessions))
			{
				requestParams.Add("begin_session", 1);

				/* If location is disabled or no location consent is given,
				the SDK adds an empty location entry to every "begin_session" request. */
				if (_locationService.IsLocationDisabled || !_consentService.CheckConsent(FeaturesEnum.Location))
				{
					requestParams.Add("location", string.Empty);
				}
				else
				{
					if (!string.IsNullOrEmpty(_locationService.IPAddress))
					{
						requestParams.Add("ip_address", _locationService.IPAddress);
					}

					if (!string.IsNullOrEmpty(_locationService.CountryCode))
					{
						requestParams.Add("country_code", _locationService.CountryCode);
					}

					if (!string.IsNullOrEmpty(_locationService.City))
					{
						requestParams.Add("city", _locationService.City);
					}

					if (!string.IsNullOrEmpty(_locationService.Location))
					{
						requestParams.Add("location", _locationService.Location);
					}
				}

				requestParams.Add("metrics", JsonConvert.SerializeObject(CountlyMetricModel.Metrics, Formatting.Indented,
				new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

				await _requestCountlyHelper.GetResponseAsync(requestParams);
			}

			//Start session timer
			if (!_configModel.EnableManualSessionHandling)
			{
				InitSessionTimer();
				_sessionTimer.Start();
			}
		}

		public async Task ExecuteEndSessionAsync(bool disposeTimer = true)
		{
			//if (ConsentModel.CheckConsent(FeaturesEnum.Sessions.ToString()))
			//{
			IsSessionInitiated = false;
			_eventNumberInSameSessionHelper.RemoveAllEvents();
			
			var requestParams =
				new Dictionary<string, object>
				{
					{"end_session", 1},
					{"session_duration", (DateTime.Now - _lastSessionRequestTime).TotalSeconds},
					{"ignore_cooldown", _configModel.IgnoreSessionCooldown.ToString().ToLower()}
				};
			requestParams.Add("metrics", JsonConvert.SerializeObject(CountlyMetricModel.Metrics, Formatting.Indented,
				new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore}));

			await _requestCountlyHelper.GetResponseAsync(requestParams);

			if (!_configModel.EnableManualSessionHandling)
			{
				//Do not extend session after session ends
				if (disposeTimer)
				{
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

			if (_configModel.EnableTestMode)
			{
				return;
			}

			//Enables push notification on start
			if (_configModel.NotificationMode != TestMode.None)
			{
				_pushCountlyService.EnablePushNotificationAsync(_configModel.NotificationMode);
			}
		}

		/// <summary>
		/// Ends a session by setting end_session
		/// </summary>
		public async Task EndSessionAsync()
		{
			await ExecuteEndSessionAsync();
		}

		/// <summary>
		/// Extends a session by another 60 seconds
		/// </summary>
		public async Task ExtendSessionAsync()
		{
			_lastSessionRequestTime = DateTime.Now;
			//if (ConsentModel.CheckConsent(FeaturesEnum.Sessions.ToString()))
			//{
			var requestParams =
				new Dictionary<string, object>
				{
					{
						"session_duration", _configModel.SessionDuration
					},
					{"ignore_cooldown", _configModel.IgnoreSessionCooldown.ToString().ToLower()}
				};
			requestParams.Add("metrics", JsonConvert.SerializeObject(CountlyMetricModel.Metrics, Formatting.Indented,
				new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore}));

			await _requestCountlyHelper.GetResponseAsync(requestParams);
			
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