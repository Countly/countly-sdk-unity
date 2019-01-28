/* 
    Countly Unity SDK
*/

#region Usings

using Assets.Scripts.Helpers;
using Assets.Scripts.Models;
using CountlyModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using UnityEngine;
using Firebase;
using Assets.Scripts.Enums;
using System.ComponentModel;

#endregion

namespace Assets.Scripts.Main.Development
{
    public class Countly
    {
        #region Fields and Properties

        private static int _extendSessionInterval;
        private static Timer _sessionTimer;
        private static DateTime _lastSessionRequestTime;
        private static string _lastView;
        private static DateTime _lastViewStartTime;

        public static string ServerUrl { get; private set; }
        public static string AppKey { get; private set; }
        public static string DeviceId { get; private set; }
        public static bool IsManualSessionHandlingEnabled { get; private set; }

        public static string CountryCode { get; private set; }
        public static string City { get; private set; }
        public static string Location { get; private set; }
        public static string IPAddress { get; private set; }
        
        public static string Salt { get; private set; }
        public static bool PostRequestEnabled { get; private set; }
        public static bool EnableConsoleErrorLogging { get; private set; }
        public static bool IgnoreSessionCooldown { get; private set; }
        public static TestMode? NotificationMode { get; private set; }

        internal static int EventSendThreshold { get; private set; }
        internal static int StoredRequestLimit { get; private set; }

        public static bool RequestQueuingEnabled;

        public static Queue<string> CrashBreadcrumbs { get; private set; } 
        public static int TotalBreadcrumbsAllowed { get; private set; }

        public static bool IsSessionInitiated { get; private set; }
        public static bool EnableAutomaticCrashReporting { get; private set; }


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

        #region Methods

        #region Initialization

        #region Public methods

        /// <summary>
        /// Initializes countly instance
        /// </summary>
        /// <param name="serverUrl"></param>
        /// <param name="appKey"></param>
        /// <param name="deviceId"></param>
        public static void Begin(string serverUrl, string appKey, string deviceId = null)
        {
            ServerUrl = serverUrl;
            AppKey = appKey;

            //**Priority is**
            //Cached DeviceID (remains even after after app kill)
            //Static DeviceID (only when the app is running either backgroun/foreground)
            //User provided DeviceID
            //Generate Random DeviceID
            var storedDeviceId = PlayerPrefs.GetString("DeviceID");
            DeviceId = !CountlyHelper.IsNullEmptyOrWhitespace(storedDeviceId)
                        ? storedDeviceId
                        : !CountlyHelper.IsNullEmptyOrWhitespace(DeviceId)
                        ? DeviceId
                        : !CountlyHelper.IsNullEmptyOrWhitespace(deviceId)
                        ? deviceId : CountlyHelper.GetUniqueDeviceID();

            if (string.IsNullOrEmpty(ServerUrl))
                throw new ArgumentNullException(serverUrl, "Server URL is required.");
            if (string.IsNullOrEmpty(AppKey))
                throw new ArgumentNullException(appKey, "App Key is required.");

            //Set DeviceID in Cache if it doesn't already exists in Cache
            if (CountlyHelper.IsNullEmptyOrWhitespace(storedDeviceId))
                PlayerPrefs.SetString(Constants.DeviceIDKey, DeviceId);

            //Initialzing collections on app start
            CrashBreadcrumbs = new Queue<string>();
            CountlyRequestModel.InitializeRequestCollection();

            //ConsentGranted = consentGranted;
        }

        /// <summary>
        /// Initializes the Countly SDK with default values
        /// </summary>
        /// <param name="salt"></param>
        /// <param name="enablePost"></param>
        /// <param name="enableConsoleErrorLogging"></param>
        /// <param name="ignoreSessionCooldown"></param>
        /// <returns></returns>
        public static async Task<CountlyResponse> SetDefaults(CountlyConfigModel configModel)
        {
            Salt = configModel.Salt;
            PostRequestEnabled = configModel.EnablePost;
            EnableConsoleErrorLogging = configModel.EnableConsoleErrorLogging;
            IgnoreSessionCooldown = configModel.IgnoreSessionCooldown;
            IsManualSessionHandlingEnabled = configModel.EnableManualSessionHandling;
            NotificationMode = configModel.NotificationMode;
            _extendSessionInterval = configModel.SessionDuration;
            StoredRequestLimit = configModel.StoredRequestLimit;
            EventSendThreshold = configModel.EventSendThreshold;
            TotalBreadcrumbsAllowed = configModel.TotalBreadcrumbsAllowed;
            EnableAutomaticCrashReporting = configModel.EnableAutomaticCrashReporting;

            if (!IsManualSessionHandlingEnabled)
            {
                //Start Session and enable push notification
                await BeginSessionAsync();
            }

            return new CountlyResponse
            {
                IsSuccess = true
            };
        }

        #endregion

        #endregion

        #region Optional Parameters

        #region Public methods

        /// <summary>
        /// Sets Country Code to be used for future requests. Takes ISO Country code as input parameter
        /// </summary>
        /// <param name="country_code"></param>
        public static void SetCountryCode(string country_code)
        {
            CountryCode = country_code;
        }

        /// <summary>
        /// Sets City to be used for future requests.
        /// </summary>
        /// <param name="city"></param>
        public static void SetCity(string city)
        {
            City = city;
        }

        /// <summary>
        /// Sets Location to be used for future requests.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        public static void SetLocation(double latitude, double longitude)
        {
            Location = latitude + "," + longitude;
        }

        /// <summary>
        /// Sets IP address to be used for future requests.
        /// </summary>
        /// <param name="ip_address"></param>
        public static void SetIPAddress(string ip_address)
        {
            IPAddress = ip_address;
        }

        /// <summary>
        /// Disabled the location tracking on the Countly server
        /// </summary>
        public static void DisableLocation()
        {
            Location = string.Empty;
        }

        #endregion

        #endregion

        #region Changing Device Id

        #region Public methods

        /// <summary>
        /// Changes Device Id.
        /// Adds currently recorded but not queued events to request queue.
        /// Clears all started timed-events
        /// Ends cuurent session with old Device Id.
        /// Begins a new session with new Device Id
        /// </summary>
        /// <param name="deviceId"></param>
        public static async Task<CountlyResponse> ChangeDeviceIDAndEndCurrentSessionAsync(string deviceId)
        {
            //Ignore call if new and old device id are same
            if (DeviceId == deviceId)
                return new CountlyResponse { IsSuccess = true };

            //Add currently recorded but not queued events to request queue-----------------------------------
            await CountlyEventModel.ReportAllRecordedEventsAsync(true);

            //Ends current session
            //Do not dispose timer object
            await ExecuteEndSessionAsync(false);

            //Update device id
            UpdateDeviceID(deviceId);

            //Begin new session with new device id
            //Do not initiate timer again, it is already initiated
            await ExecuteBeginSessionAsync();

            return new CountlyResponse { IsSuccess = true };
        }

        /// <summary>
        /// Changes DeviceId. 
        /// Continues with the current session.
        /// Merges data for old and new Device Id. 
        /// </summary>
        /// <param name="deviceId"></param>
        public static async Task<CountlyResponse> ChangeDeviceIDAndMergeSessionDataAsync(string deviceId)
        {
            //Ignore call if new and old device id are same
            if (DeviceId == deviceId)
                return new CountlyResponse { IsSuccess = true };

            //Keep old device id
            var old_device_id = DeviceId;

            //Update device id
            UpdateDeviceID(deviceId);

            //Merge user data for old and new device
            var requestParams =
               new Dictionary<string, object>
               {
                        { "old_device_id", old_device_id }
               };

            await CountlyHelper.GetResponseAsync(requestParams);
            return new CountlyResponse { IsSuccess = true };
        }

        #endregion

        #region System methods

        /// <summary>
        /// Updates Device ID both in app and in cache
        /// </summary>
        /// <param name="newDeviceID"></param>
        private static void UpdateDeviceID(string newDeviceID)
        {
            //Change device id
            DeviceId = newDeviceID;

            //Updating Cache
            PlayerPrefs.SetString(Constants.DeviceIDKey, DeviceId);
        }

        #endregion

        #endregion

        #region Crash Reporting

        #region System methods

        /// <summary>
        /// Called when there is an exception 
        /// </summary>
        /// <param name="message">Exception Class</param>
        /// <param name="stackTrace">Stack Trace</param>
        /// <param name="type">Excpetion type like error, warning, etc</param>
        internal static async void LogCallback(string message, string stackTrace, LogType type)
        {
            if (EnableAutomaticCrashReporting
                && (type == LogType.Error || type == LogType.Exception))
            {
                await SendCrashReportAsync(message, stackTrace, type, null, false);
            }
        }

        /// <summary>
        /// Private method that sends crash details to the server. Set param "nonfatal" to true for Custom Logged errors
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stackTrace"></param>
        /// <param name="type"></param>
        /// <param name="customParams"></param>
        /// <param name="nonfatal"></param>
        /// <returns></returns>
        private static async Task<CountlyResponse> SendCrashReportAsync(string message, string stackTrace, LogType type, 
            IDictionary<string, object> segments = null, bool nonfatal = true)
        {
            //if (ConsentModel.CheckConsent(FeaturesEnum.Crashes.ToString()))
            //{
            var model = CountlyExceptionDetailModel.ExceptionDetailModel;
            model.Error = stackTrace;
            model.Name = message;
            model.Nonfatal = nonfatal;
            model.Custom = segments as Dictionary<string, object>;
            model.Logs = string.Join("\n", CrashBreadcrumbs);
#if UNITY_IOS
            model.Manufacture = UnityEngine.iOS.Device.generation.ToString();
#endif
#if UNITY_ANDROID
            model.Manufacture = SystemInfo.deviceModel;
#endif
            var requestParams = new Dictionary<string, object>
            {
                { "crash", JsonConvert.SerializeObject(model, Formatting.Indented,
                                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }) }
            };

            return await CountlyHelper.GetResponseAsync(requestParams);
            //}
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Sends custom logged errors to the server.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stackTrace"></param>
        /// <param name="type"></param>
        /// <param name="segments"></param>
        /// <returns></returns>
        public static async Task<CountlyResponse> SendCrashReportAsync(string message, string stackTrace, LogType type, 
            IDictionary<string, object> segments = null)
        {
            return await SendCrashReportAsync(message, stackTrace, type, segments, true);
        }

        /// <summary>
        /// Adds string value to a list which is later sent over as logs whenever a cash is reported by system.
        /// The length of a breadcrumb is limited to 1000 characters. Only first 1000 characters will be accepted in case the length is more 
        /// than 1000 characters.
        /// </summary>
        /// <param name="value"></param>
        public static void AddBreadcrumbs(string value)
        {
            string validBreadcrumb = value.Length > 1000 ? value.Substring(0, 1000) : value;

            if (CrashBreadcrumbs.Count == TotalBreadcrumbsAllowed)
                CrashBreadcrumbs.Dequeue();

            CrashBreadcrumbs.Enqueue(value);
        }

        #endregion

        #endregion

        #region Sessions

        #region System methods

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

        private static async Task<CountlyResponse> ExecuteBeginSessionAsync()
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

            requestParams.Add("ip_address", IPAddress);

            var response = await CountlyHelper.GetResponseAsync(requestParams);

            //Extend session only after session has begun
            if (response.IsSuccess)
            {
                //Session initiated
                IsSessionInitiated = true;
                //Start session timer
                if (!IsManualSessionHandlingEnabled)
                {
                    InitSessionTimer();
                    _sessionTimer.Start();
                }
            }

            //}
            return response;
        }

        private static async Task<CountlyResponse> ExecuteEndSessionAsync(bool disposeTimer = true)
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
            var response = await CountlyHelper.GetResponseAsync(requestParams);

            if (!IsManualSessionHandlingEnabled)
            {
                //Do not extend session after session ends
                if (disposeTimer)
                {
                    _sessionTimer.Stop();
                    _sessionTimer.Dispose();
                    _sessionTimer.Close();
                }
            }
            //}
            return response;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Initiates a session by setting begin_session
        /// </summary>
        public static async Task<CountlyResponse> BeginSessionAsync()
        {
            var result = await ExecuteBeginSessionAsync();

            //Enables push notification on start
            if (NotificationMode.HasValue)
            {
                EnablePush(NotificationMode.Value);
            }

            return result;
        }

        /// <summary>
        /// Ends a session by setting end_session
        /// </summary>
        public static async Task<CountlyResponse> EndSessionAsync()
        {
            return await ExecuteEndSessionAsync();
        }

        /// <summary>
        /// Extends a session by another 60 seconds
        /// </summary>
        public static async Task<CountlyResponse> ExtendSessionAsync()
        {
            _lastSessionRequestTime = DateTime.Now;
            //if (ConsentModel.CheckConsent(FeaturesEnum.Sessions.ToString()))
            //{
            var requestParams =
                new Dictionary<string, object>
                {
                        { "session_duration", _extendSessionInterval },
                        { "ignore_cooldown", IgnoreSessionCooldown.ToString().ToLower() }
                };
            requestParams.Add("metrics", JsonConvert.SerializeObject(CountlyMetricModel.Metrics, Formatting.Indented,
                                            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

            return await CountlyHelper.GetResponseAsync(requestParams);
            //}
        }

        #endregion

        #endregion

        #region Events

        #region Public methods

        /// <summary>
        /// Records an event with the specified key in the system.
        /// </summary>
        /// <param name="key"></param>
        public static async Task<CountlyResponse> RecordEventAsync(string key)
        {
            return await CountlyEventModel.RecordEventAsync(key);
        }

        /// <summary>
        /// Records an event with the specified data in the system.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="segmentation"></param>
        /// <param name="count"></param>
        /// <param name="sum"></param>
        /// <returns></returns>
        public static async Task<CountlyResponse> RecordEventAsync(string key, IDictionary<string, object> segmentation,
                                                                int? count = 1, double? sum = 0, double? duration = null)
        {
            return await CountlyEventModel.RecordEventAsync(key, segmentation, count, sum, duration);
        }

        #region Unused Code

        ///// <summary>
        ///// Ends an event with the specified key. Sends events details to the Counlty API 
        ///// </summary>
        ///// <param name="key"></param>
        ///// <returns></returns>
        //public static async Task<CountlyResponse> EndEventAsync(string key)
        //{
        //    return await CountlyEventModel.EndAsync(key);
        //}

        ///// <summary>
        ///// Ends an event with the specified key and data. Sends events details to the Counlty API 
        ///// </summary>
        ///// <param name="key">Key is required</param>
        ///// <param name="segmentation">Custom </param>
        ///// <param name="count"></param>
        ///// <param name="sum"></param>
        ///// <returns></returns>
        //public static async Task<CountlyResponse> EndEventAsync(string key, IDictionary<string, object> segmentation, 
        //                                                        int? count = 1, double? sum = 0)
        //{
        //    return await CountlyEventModel.EndAsync(key, segmentation, count, sum);
        //}

        #endregion

        #endregion

        #endregion

        #region Views

        #region Public methods

        /// <summary>
        /// Reports a view with the specified name and a last visited view if it existed
        /// </summary>
        /// <param name="name"></param>
        /// <param name="hasSessionBegunWithView"></param>
        public static async Task<CountlyResponse> ReportViewAsync(string name, bool hasSessionBegunWithView = false)
        {
            if (string.IsNullOrEmpty(name))
            {
                return new CountlyResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "View name is required."
                };
            }

            var events = new List<CountlyEventModel>();
            var lastView = GetLastView();
            if (lastView != null)
                events.Add(lastView);

            var currentViewSegment =
                new ViewSegment
                {
                    Name = name,
                    Segment = Constants.UnityPlatform,
                    Visit = 1,
                    HasSessionBegunWithView = hasSessionBegunWithView
                };

            var currentView = new CountlyEventModel(CountlyEventModel.ViewEvent, currentViewSegment.ToDictionary());
            events.Add(currentView);

            var res = await CountlyEventModel.ReportMultipleEventsAsync(events);

            _lastView = name;
            _lastViewStartTime = DateTime.Now;

            return res;
        }

        #endregion

        #region System methods

        private static CountlyEventModel GetLastView(bool hasSessionBegunWithView = false)
        {
            if (string.IsNullOrEmpty(_lastView) && string.IsNullOrWhiteSpace(_lastView))
                return null;

            var viewSegment =
                new ViewSegment
                {
                    Name = _lastView,
                    Segment = Constants.UnityPlatform,
                    HasSessionBegunWithView = hasSessionBegunWithView
                };

            var customEvent = new CountlyEventModel(
                                    CountlyEventModel.ViewEvent, viewSegment.ToDictionary(),
                                    null, (DateTime.Now - _lastViewStartTime).TotalMilliseconds);

            return customEvent;
        }

        #endregion

        #endregion

        #region View Action

        #region Public methods

        /// <summary>
        /// Reports a particular action with the specified details
        /// </summary>
        /// <param name="type"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static async Task<CountlyResponse> ReportActionAsync(string type, int x, int y, int width, int height)
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

            return await CountlyEventModel.ReportCustomEventAsync(CountlyEventModel.ViewActionEvent, segment.ToDictionary());
        }

        #endregion

        #endregion

        #region Star Rating

        #region Public methods

        /// <summary>
        /// Sends app rating to the server.
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="app_version"></param>
        /// <param name="rating">Rating should be from 1 to 5</param>
        /// <returns></returns>
        public static async Task<CountlyResponse> ReportStarRatingAsync(string platform, string app_version, int rating)
        {
            if (rating < 1 || rating > 5)
            {
                return new CountlyResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Please provide rating from 1 to 5"
                };
            }

            var segment =
                new StarRatingSegment
                {
                    Platform = platform,
                    AppVersion = app_version,
                    Rating = rating,
                };

            return await CountlyEventModel.ReportCustomEventAsync(
                            CountlyEventModel.StarRatingEvent, segment.ToDictionary(),
                            null, null, null);
        }

        #endregion

        #endregion

        #region User Details

        #region Public methods

        /// <summary>
        /// Modifies all user data. Custom data should be json string.
        /// Deletes an already defined custom property from the Countly server, if it is supplied with a NULL value
        /// </summary>
        /// <param name="userDetails"></param>
        /// <returns></returns>
        public static async Task<CountlyResponse> UserDetailsAsync(CountlyUserDetailsModel userDetails)
        {
            if (userDetails == null)
            {
                return new CountlyResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "No data found."
                };
            }

            return await userDetails.SetUserDetailsAsync();
        }

        /// <summary>
        /// Modifies custom user data only. Custom data should be json string.
        /// Deletes an already defined custom property from the Countly server, if it is supplied with a NULL value
        /// </summary>
        /// <param name="userDetails"></param>
        /// <returns></returns>
        public static async Task<CountlyResponse> UserCustomDetailsAsync(CountlyUserDetailsModel userDetails)
        {
            if (userDetails == null)
            {
                return new CountlyResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "No data found."
                };
            }

            return await userDetails.SetCustomUserDetailsAsync();
        }

        #endregion

        #endregion

        #region Consents

        #region Unused Code

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

        #region Push Notifications

        #region Public methods

        /// <summary>
        /// Enables Push Notification feature for the device
        /// </summary>
        public static void EnablePush(TestMode mode)
        {
            CountlyPushNotificationModel.CountlyPNInstance.EnablePushNotificationAsync(mode);
        }

        #endregion

        #endregion

        #region Timer

        #region System methods

        /// <summary>
        /// Intializes the timer for extending session with sepcified interval
        /// </summary>
        /// <param name="sessionInterval">In milliseconds</param>
        private static void InitSessionTimer()
        {
            if (!IsManualSessionHandlingEnabled)
            {
                _sessionTimer = new Timer();
                _sessionTimer.Interval = _extendSessionInterval * 1000;
                _sessionTimer.Elapsed += SessionTimerOnElapsedAsync;
                _sessionTimer.AutoReset = true;
            }
        }

        /// <summary>
        /// Extends the session after the session duration is elapsed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="elapsedEventArgs"></param>
        private static async void SessionTimerOnElapsedAsync(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (!IsManualSessionHandlingEnabled)
            {
                await ExtendSessionAsync();
            }
            CountlyRequestModel.ProcessQueue();
        }

        #endregion

        #endregion

        #endregion
    }
}
