/* 
    Countly Dot Net SDK
    Under Development
    Since: 06/12/2018
*/

#region Usings

using Assets.Plugin.Models;
using CountlyModels;
using Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

#endregion

namespace Assets.Plugin.Scripts.Development
{
    public class Countly
    {
        #region Fields

        private const int _extenSessionInterval = 30;
        private static Timer _timer;
        private static DateTime _lastSessionRequestTime;
        private static string _lastView;
        private static DateTime _lastViewStartTime;

        public static string ServerUrl { get; private set; }
        public static string AppKey { get; private set; }
        public static string DeviceId { get; private set; }

        public static string CountryCode;
        public static string City;
        public static string Location;
        public static string IPAddress;

        public static string Salt;
        public static bool PostRequestEnabled;
        public static bool RequestQueuingEnabled;
        public static bool PreventRequestTanmpering;
        public static bool EnableConsoleErrorLogging;
        public static bool IgnoreSessionCooldown;
        public static Dictionary<string, DateTime> TotalEvents = new Dictionary<string, DateTime>();
        private static CountlyEventModel _countlyEventModel;

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

        #region Initialization
        /// <summary>
        /// Initializes the countly 
        /// </summary>
        /// <param name="serverUrl"></param>
        /// <param name="appKey"></param>
        /// <param name="deviceId"></param>
        public Countly(string serverUrl, string appKey, string deviceId = null)
        {
            ServerUrl = serverUrl;
            AppKey = appKey;
            DeviceId = DeviceId ?? deviceId ?? CountlyHelper.GenerateUniqueDeviceID();

            if (string.IsNullOrEmpty(ServerUrl))
                throw new ArgumentNullException(serverUrl, "ServerURL is required.");
            if (string.IsNullOrEmpty(AppKey))
                throw new ArgumentNullException(appKey, "AppKey is required.");
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

        #endregion

        #region Optional Parameters

        /// <summary>
        /// Sets Country Code to be used for future requests.
        /// </summary>
        /// <param name="country_code"></param>
        public void SetCountryCode(string country_code)
        {
            CountryCode = country_code;
        }

        /// <summary>
        /// Sets City to be used for future requests.
        /// </summary>
        /// <param name="city"></param>
        public void SetCity(string city)
        {
            City = city;
        }

        /// <summary>
        /// Sets Location to be used for future requests.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        public void SetLocation(double latitude, double longitude)
        {
            Location = latitude + "," + longitude;
        }

        /// <summary>
        /// Sets IP address to be used for future requests.
        /// </summary>
        /// <param name="ip_address"></param>
        public void SetIPAddress(string ip_address)
        {
            IPAddress = ip_address;
        }

        #endregion

        #region Changing Device Id

        /// <summary>
        /// Changes Device Id.
        /// Adds currently recorded but not queued events to request queue.
        /// Clears all started timed-events
        /// Ends cuurent session with old Device Id.
        /// Begins a new session with new Device Id
        /// </summary>
        /// <param name="deviceId"></param>
        public void ChangeDeviceAndEndCurrentSession(string deviceId)
        {
            //Ignore call if new and old device id are same
            if (DeviceId == deviceId)
                return;

            //Add currently recorded but not queued events to request queue-----------------------------------
            //------------------------------------------------------------------------------------------------

            //Ends current session
            EndSession();

            //Clear all started timed-events------------------------------------------------------------------
            //------------------------------------------------------------------------------------------------

            //Update the device id
            DeviceId = deviceId;

            //Begin new session with new device id
            BeginSession();
        }

        /// <summary>
        /// Changes DeviceId. 
        /// Continues with the current session.
        /// Merges data for old and new Device Id. 
        /// </summary>
        /// <param name="deviceId"></param>
        public bool ChangeDevicAndMergeSessionData(string deviceId)
        {
            //Ignore call if new and old device id are same
            if (DeviceId == deviceId)
                return true;

            //Keep old device id
            var old_device_id = DeviceId;

            //Change device id
            DeviceId = deviceId;

            //Merge user data for old and new device
            var requestParams =
               new Dictionary<string, object>
               {
                        { "old_device_id", old_device_id }
               };

            CountlyHelper.GetResponse(requestParams);
            return true;
        }

        #endregion

        #region Crash Reporting

        /// <summary>
        /// Called when there is an exception 
        /// </summary>
        /// <param name="message">Exception Class</param>
        /// <param name="stackTrace">Stack Trace</param>
        /// <param name="type">Excpetion type like error, warning, etc</param>
        internal void LogCallback(string message, string stackTrace, LogType type)
        {
            if (type == LogType.Error
                || type == LogType.Exception)
            {
                SendCrashReport(message, stackTrace, type);
            }
        }

        /// <summary>
        /// Sends crash details to the server. Set param "nonfatal" to true for Custom Logged errors
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stackTrace"></param>
        /// <param name="type"></param>
        /// <param name="customParams"></param>
        /// <param name="nonfatal"></param>
        /// <returns></returns>
        public string SendCrashReport(string message, string stackTrace, LogType type, string segments = null, bool nonfatal = true)
        {
            //if (ConsentModel.CheckConsent(FeaturesEnum.Crashes.ToString()))
            //{
            var model = CountlyExceptionDetailModel.ExceptionDetailModel;
            model.Error = stackTrace;
            model.Name = message;
            model.Nonfatal = nonfatal;
            model.Custom = segments;
#if UNITY_IOS
            model._manufacture = iPhone.generation.ToString(),
#endif
#if UNITY_ANDROID
            model.Manufacture = SystemInfo.deviceModel;
#endif
            var requestParams = new Dictionary<string, object>
            {
                { "crash", JsonConvert.SerializeObject(model, Formatting.Indented,
                                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }) }
            };
            return CountlyHelper.GetResponse(requestParams);
            //}
        }

        #endregion

        #region Sessions

        /// <summary>
        /// Initiates a session by setting begin_session
        /// </summary>
        public string BeginSession()
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
            requestParams.Add("metrics", JsonConvert.SerializeObject(CountlyMetricModel.Metrics, Formatting.Indented,
                                            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

            if (!string.IsNullOrEmpty(IPAddress))
                requestParams.Add("ip_address", IPAddress);

            var response = CountlyHelper.GetResponse(requestParams);

            //Extend session only after session has begun
            _timer.Start();
            //}
            return response;
        }

        /// <summary>
        /// Ends a session by setting end_session
        /// </summary>
        public string EndSession()
        {
            //if (ConsentModel.CheckConsent(FeaturesEnum.Sessions.ToString()))
            //{
            var requestParams =
                new Dictionary<string, object>
                {
                        { "end_session", 1 },
                        { "session_duration", (DateTime.Now - _lastSessionRequestTime).Seconds },
                        { "ignore_cooldown", IgnoreSessionCooldown.ToString().ToLower() }
                };
            requestParams.Add("metrics", JsonConvert.SerializeObject(CountlyMetricModel.Metrics, Formatting.Indented,
                                            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
            var response = CountlyHelper.GetResponse(requestParams);

            //Do not extend session after session ends
            _timer.Stop();
            _timer.Dispose();
            _timer.Close();
            //}
            return response;
        }

        /// <summary>
        /// Extends a session by another 60 seconds
        /// </summary>
        public string ExtendSession()
        {
            _lastSessionRequestTime = DateTime.Now;
            //if (ConsentModel.CheckConsent(FeaturesEnum.Sessions.ToString()))
            //{
            var requestParams =
                new Dictionary<string, object>
                {
                        { "session_duration", 60 },
                        { "ignore_cooldown", IgnoreSessionCooldown.ToString().ToLower() }
                };
            requestParams.Add("metrics", JsonConvert.SerializeObject(CountlyMetricModel.Metrics, Formatting.Indented,
                                            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
            return CountlyHelper.GetResponse(requestParams);
            //}
        }

        #endregion

        #region Events

        /// <summary>
        /// Adds an event with the specified key. Doesn't send the request to the Countly API
        /// </summary>
        /// <param name="key"></param>
        public void StartEvent(string key)
        {
            _countlyEventModel = new CountlyEventModel(key);
        }

        /// <summary>
        /// Ends an event with the specified key. Sends events details to the Counlty API 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool EndEvent(string key)
        {
            if (_countlyEventModel == null)
                throw new NullReferenceException("Please start an event first.");

            _countlyEventModel.Key = key;
            return _countlyEventModel.End();
        }

        /// <summary>
        /// Ends and event with the specified key and data. Sends events details to the Counlty API 
        /// </summary>
        /// <param name="key">Key is required</param>
        /// <param name="segmentation">Custom </param>
        /// <param name="count"></param>
        /// <param name="sum"></param>
        /// <returns></returns>
        public bool EndEvent(string key, string segmentation, int? count = 1, double? sum = 0)
        {
            if (_countlyEventModel == null)
                throw new NullReferenceException("Please start an event first.");

            _countlyEventModel.Key = key;
            _countlyEventModel.Segmentation = new JRaw(segmentation);
            _countlyEventModel.Count = count;
            _countlyEventModel.Sum = sum;
            return _countlyEventModel.End();
        }

        #endregion

        #region Views

        /// <summary>
        /// Reports a view with the specified name and a last visited view if it existed
        /// </summary>
        /// <param name="name"></param>
        /// <param name="hasSessionBegunWithView"></param>
        public void ReportView(string name, bool hasSessionBegunWithView = false)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("Parameter name is required.");

            var events = new List<CountlyEventModel>();
            var lastView = ReportViewDuration();
            if (lastView != null)
                events.Add(lastView);

            var currentViewSegment =
                new ViewSegment
                {
                    Name = name,
                    Segment = CountlyHelper.OperationSystem,
                    Visit = 1,
                    HasSessionBegunWithView = hasSessionBegunWithView
                };

            var currentView = new CountlyEventModel(CountlyEventModel.ViewEvent,
                                                        (JsonConvert.SerializeObject(currentViewSegment, Formatting.Indented,
                                                            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, })),
                                                        null
                                                    );
            events.Add(currentView);

            CountlyEventModel.StartMultipleEvents(events);

            _lastView = name;
            _lastViewStartTime = DateTime.Now;
        }

        private CountlyEventModel ReportViewDuration(bool hasSessionBegunWithView = false)
        {
            if (string.IsNullOrEmpty(_lastView) && string.IsNullOrWhiteSpace(_lastView))
                return null;

            var viewSegment =
                new ViewSegment
                {
                    Name = _lastView,
                    Segment = CountlyHelper.OperationSystem,
                    HasSessionBegunWithView = hasSessionBegunWithView
                };

            var customEvent = new CountlyEventModel(CountlyEventModel.ViewEvent,
                                                        (JsonConvert.SerializeObject(viewSegment, Formatting.Indented,
                                                            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore,  })),
                                                        (DateTime.Now - _lastViewStartTime).TotalMilliseconds
                                                    );

            return customEvent;
        }

        #endregion

        #region View Action

        /// <summary>
        /// Reports a particular action with the specified details
        /// </summary>
        /// <param name="type"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public bool ReportAction(string type, int x, int y, int width, int height)
        {
            var segment =
                new ActionSegment
                {
                    Type = type,
                    PositionX = x,
                    PositionY = y,
                    Width = width,
                    Height = height
                };

            var action = new CountlyEventModel(CountlyEventModel.ViewActionEvent,
                                                        (JsonConvert.SerializeObject(segment, Formatting.Indented,
                                                            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, })),
                                                        null
                                                    );
            action.ReportCustomEvent();
            return true;
        }

        #endregion

        #region Star Rating

        /// <summary>
        /// Sends app rating to the server.
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="app_version"></param>
        /// <param name="rating">Rating should be from 1 to 5</param>
        /// <returns></returns>
        public bool ReportStarRating(string platform, string app_version, int rating)
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentException("Please provide rating from 1 to 5");

            var segment =
                new StarRatingSegment
                {
                    Platform = platform,
                    AppVersion = app_version,
                    Rating = rating,
                };

            var action = new CountlyEventModel(CountlyEventModel.StarRatingEvent,
                                                        (JsonConvert.SerializeObject(segment, Formatting.Indented,
                                                            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, })),
                                                        null
                                                    );
            action.ReportCustomEvent();
            return true;
        }

        #endregion

        #region User Details

        /// <summary>
        /// Modifies all user data. Custom data should be json string.
        /// Deletes a property if it is not supplied
        /// </summary>
        /// <param name="userDetails"></param>
        /// <returns></returns>
        public bool UserDetails(CountlyUserDetailsModel userDetails)
        {
            if (userDetails == null)
                throw new ArgumentNullException("Please provide user details.");

            return userDetails.SetUserDetails();
        }

        /// <summary>
        /// Modifies custom user data only. Custom data should be json string.
        /// Deletes a property if it is not supplied
        /// </summary>
        /// <param name="userDetails"></param>
        /// <returns></returns>
        public bool UserCustomDetails(CountlyUserDetailsModel userDetails)
        {
            if (userDetails == null)
                throw new ArgumentNullException("Please provide user details.");

            return userDetails.SetCustomUserDetails();
        }

        #endregion

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

        #region Timer

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
