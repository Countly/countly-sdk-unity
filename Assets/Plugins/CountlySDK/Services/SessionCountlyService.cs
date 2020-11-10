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
		
		private readonly CountlyConfigModel _configModel;
		private readonly EventCountlyService _eventService;
		private readonly PushCountlyService _pushCountlyService;
        private readonly RequestCountlyHelper _requestCountlyHelper;
        private readonly OptionalParametersCountlyService _optionalParametersCountlyService;
        private readonly EventNumberInSameSessionHelper _eventNumberInSameSessionHelper;

        internal SessionCountlyService(CountlyConfigModel configModel, EventCountlyService eventService, PushCountlyService pushCountlyService, 
            RequestCountlyHelper requestCountlyHelper, OptionalParametersCountlyService optionalParametersCountlyService,
            EventNumberInSameSessionHelper eventNumberInSameSessionHelper)
        {
            _configModel = configModel;
			_eventService = eventService;
			_pushCountlyService = pushCountlyService;
            _requestCountlyHelper = requestCountlyHelper;
            _optionalParametersCountlyService = optionalParametersCountlyService;
            _eventNumberInSameSessionHelper = eventNumberInSameSessionHelper;
        }

		/// <summary>
		/// Intializes the timer for extending session with specified interval
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

			await _eventService.ReportAllRecordedViewEventsAsync();
			await _eventService.ReportAllRecordedNonViewEventsAsync();

			_requestCountlyHelper.ProcessQueue();
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

		public async Task<CountlyResponse> ExecuteBeginSessionAsync()
		{
			FirstLaunchAppHelper.Process();
			_lastSessionRequestTime = DateTime.Now;
			//Session initiated
			IsSessionInitiated = true;
			_eventNumberInSameSessionHelper.RemoveAllEvents();

			//if (ConsentModel.CheckConsent(FeaturesEnum.Sessions.ToString()))
			//{
			var requestParams =
				new Dictionary<string, object>
				{
					{"begin_session", 1},
					{"ignore_cooldown", _configModel.IgnoreSessionCooldown}
				};
			requestParams.Add("metrics", JsonConvert.SerializeObject(CountlyMetricModel.Metrics, Formatting.Indented,
				new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore}));

			requestParams.Add("ip_address", _optionalParametersCountlyService.IPAddress);

			var response = await _requestCountlyHelper.GetResponseAsync(requestParams);

			//Extend session only after session has begun
//            if (response.IsSuccess)
//            {
			
			//Start session timer
			if (!_configModel.EnableManualSessionHandling)
			{
				InitSessionTimer();
				_sessionTimer.Start();
			}
//            }

			//}
			return response;
		}

		public async Task<CountlyResponse> ExecuteEndSessionAsync(bool disposeTimer = true)
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

			var response = await _requestCountlyHelper.GetResponseAsync(requestParams);

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

			//}
			return response;
		}


		/// <summary>
		/// Initiates a session by setting begin_session
		/// </summary>
		public async Task<CountlyResponse> BeginSessionAsync()
		{
			var result = await ExecuteBeginSessionAsync();

			if (_configModel.EnableTestMode)
			{
				return result;
			}

			//Enables push notification on start
			if (_configModel.NotificationMode != TestMode.None)
			{
				_pushCountlyService.EnablePushNotificationAsync(_configModel.NotificationMode);
			}

			return result;
		}

		/// <summary>
		/// Ends a session by setting end_session
		/// </summary>
		public async Task<CountlyResponse> EndSessionAsync()
		{
			return await ExecuteEndSessionAsync();
		}

		/// <summary>
		/// Extends a session by another 60 seconds
		/// </summary>
		public async Task<CountlyResponse> ExtendSessionAsync()
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

			return await _requestCountlyHelper.GetResponseAsync(requestParams);
			//}
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