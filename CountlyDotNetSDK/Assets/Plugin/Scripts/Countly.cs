/* 
    Countly Dot Net SDK
    Under Development
    Since: 06/12/2018
*/

using Assets.Plugin.Enums;
using Assets.Plugin.Models;
using CountlyModels;
using Helpers;
using System;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

namespace Assets.Plugin.Scripts
{
    public class Countly
    {
        #region Fields

        private static string _requestString = string.Empty;
        private const int _extenSessionInterval = 60;
        private static Timer _timer;
        private static DateTime _lastSessionRequestTime;

        public static string ServerUrl { get; private set; }
        public static string AppKey { get; private set; }
        public static string DeviceId { get; private set; }

        public static string CountryCode;
        public static string City;
        public static string Location;
        public static string Salt;
        public static bool PostRequestEnabled;
        public static bool RequestQueuingEnabled;
        public static bool PreventRequestTanmpering;
        public static bool EnableConsoleErrorLogging;
        public static bool IgnoreSessionCooldown;

        //#region Consents
        //public static bool ConsentGranted;

        //private static bool Consent_Sessions;
        //private static bool Consent_Events;
        //private static bool Consent_Location;
        //private static bool Consent_View;
        //private static bool Consent_Scrolls;
        //private static bool Consent_Clicks;
        //private static bool Consent_Forms;
        //private static bool Consent_Crashes;
        //private static bool Consent_Attribution;
        //private static bool Consent_Users;
        //private static bool Consent_Push;
        //private static bool Consent_StarRating;
        //private static bool Consent_AccessoryDevices;

        //#endregion

        #endregion

        #region Public Methods

        public Countly(string serverUrl, string appKey, string deviceId = null)
        {
            ServerUrl = serverUrl;
            AppKey = appKey;
            DeviceId = deviceId ?? CountlyHelper.GenerateUniqueDeviceID();
            //ConsentGranted = consentGranted;
        }

        /// <summary>
        /// Initializes the Countly SDK with default values
        /// </summary>
        /// <param name="countryCode"></param>
        /// <param name="city"></param>
        /// <param name="location"></param>
        /// <param name="salt"></param>
        /// <param name="enablePost"></param>
        /// <param name="enableRequestQueuing"></param>
        /// <param name="isDevelopmentMode"></param>
        /// <param name="sessionUpdateInterval"></param>
        public void Initialize(string salt = null, bool enablePost = false, bool enableConsoleErrorLogging = false,
            bool ignoreSessionCooldown = false)
        {
            Salt = salt;
            PostRequestEnabled = enablePost;
            PreventRequestTanmpering = !string.IsNullOrEmpty(salt);
            EnableConsoleErrorLogging = enableConsoleErrorLogging;
            IgnoreSessionCooldown = ignoreSessionCooldown;

            InitTimer(_extenSessionInterval);
        }

        /// <summary>
        /// Called when there is an exception 
        /// </summary>
        /// <param name="condition">Exception Class</param>
        /// <param name="stackTrace">Stack Trace</param>
        /// <param name="type">Excpetion type like error, warning, etc</param>
        public void LogCallback(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error
                || type == LogType.Exception
                || type == LogType.Log)
            {
                SendCrashReport(condition, stackTrace, type);
            }
        }

        #region Sessions

        /// <summary>
        /// Initiates a session by setting begin_session
        /// </summary>
        public void BeginSession()
        {
            _lastSessionRequestTime = DateTime.Now;
            //if (ConsentModel.CheckConsent(FeaturesEnum.Sessions.ToString()))
            //{
            var requestParams =
                new Dictionary<string, object>
                {
                        { "begin_session", 1 },
                        { "ignore_cooldown", IgnoreSessionCooldown }
                };
            requestParams.Add("metrics", JsonUtility.ToJson(CountlyMetricModel.Metrics));
            _requestString = CountlyHelper.BuildRequest(requestParams);
            CountlyHelper.GetResponse(_requestString);

            //Extend session only after session has begun
            _timer.Start();
            //}
        }

        /// <summary>
        /// Ends a session by setting end_session
        /// </summary>
        public void EndSession()
        {
            //if (ConsentModel.CheckConsent(FeaturesEnum.Sessions.ToString()))
            //{
            var requestParams =
                new Dictionary<string, object>
                {
                        { "end_session", 1 },
                        { "session_duration", (DateTime.Now - _lastSessionRequestTime).Seconds },
                        { "ignore_cooldown", IgnoreSessionCooldown }
                };
            requestParams.Add("metrics", JsonUtility.ToJson(CountlyMetricModel.Metrics));
            _requestString = CountlyHelper.BuildRequest(requestParams);
            CountlyHelper.GetResponse(_requestString);

            //Do not extend session after session ends
            _timer.Stop();
            //}
        }

        /// <summary>
        /// Extends a session by another 60 seconds
        /// </summary>
        public void ExtendSession()
        {
            _lastSessionRequestTime = DateTime.Now;
            //if (ConsentModel.CheckConsent(FeaturesEnum.Sessions.ToString()))
            //{
            var requestParams =
                new Dictionary<string, object>
                {
                        { "session_duration", 60 },
                        { "ignore_cooldown", IgnoreSessionCooldown }
                };
            requestParams.Add("metrics", JsonUtility.ToJson(CountlyMetricModel.Metrics));
            _requestString = CountlyHelper.BuildRequest(requestParams);
            CountlyHelper.GetResponse(_requestString);
            //}
        }

        #endregion

        /// <summary>
        /// Sends details regarding crash
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="stackTrace"></param>
        /// <param name="type"></param>
        public void SendCrashReport(string condition, string stackTrace, LogType type)
        {
            //if (ConsentModel.CheckConsent(FeaturesEnum.Crashes.ToString()))
            //{
            var model = CountlyExceptionDetailModel.ExceptionDetailModel;
            model._error = stackTrace;
            model._name = condition;
#if UNITY_IOS
            model._manufacture = iPhone.generation.ToString(),
#endif
#if UNITY_ANDROID
            model._manufacture = SystemInfo.deviceModel;
#endif
            var requestParams = new Dictionary<string, object> { { "device_id", "testing" }, { "crash", JsonUtility.ToJson(model) } };
            _requestString = CountlyHelper.BuildRequest(requestParams);
            CountlyHelper.GetResponse(_requestString);
            //}
        }

        #region Consents

        /// <summary>
        /// Checks consent for a particular feature
        /// </summary>
        /// <param name="feature"></param>
        /// <returns></returns>
        public bool CheckConsent(string feature)
        {
            return ConsentModel.CheckConsent(feature);
        }

        /// <summary>
        /// Updates a feature to give/deny consent
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="permission"></param>
        public void UpdateConsent(string feature, bool permission)
        {
            ConsentModel.UpdateConsent(feature, permission);
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Intializes the timer for extending session with sepcified interval
        /// </summary>
        /// <param name="sessionInterval">In milliseconds</param>
        private void InitTimer(int sessionInterval)
        {
            _timer = new Timer();
            _timer.Interval = sessionInterval * 1000;
            _timer.Elapsed += TimerOnElapsed;
            _timer.AutoReset = true;
        }

        /// <summary>
        /// Extends the session after the session duration is elapsed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="elapsedEventArgs"></param>
        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            ExtendSession();
        }
        #endregion
    }
}
