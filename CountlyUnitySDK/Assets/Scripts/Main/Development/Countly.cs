/* 
    Countly Unity SDK
    Release Version: v1
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

#endregion

namespace Assets.Scripts.Main.Development
{
    public class Countly
    {
        #region Fields and Properties

        private static int _extendSessionInterval = 60;
        private static Timer _sessionTimer;
        private static DateTime _lastSessionRequestTime;
        private static string _lastView;
        private static DateTime _lastViewStartTime;

        public static string ServerUrl { get; private set; }
        public static string AppKey { get; private set; }
        public static string DeviceId { get; private set; }

        internal static string CountryCode;
        internal static string City;
        internal static string Location;
        internal static string IPAddress;
        internal static int EventSendThreshold = 0;
        internal static int StoredRequestLimit = 1000;

        public static string Salt;
        public static bool PostRequestEnabled;
        public static bool RequestQueuingEnabled;
        public static bool PreventRequestTanmpering;
        public static bool EnableConsoleErrorLogging;
        public static bool IgnoreSessionCooldown;
        internal static Dictionary<string, DateTime> TotalEvents = new Dictionary<string, DateTime>();
        internal static Queue<CountlyRequestModel> TotalRequests = new Queue<CountlyRequestModel>();

        internal static bool IsFirebaseReady { get; set; }

        //Not used anywhere for now
        internal static FirebaseApp FirebaseAppInstance { get; set; }

        internal static List<string> CrashBreadcrumbs { get; set; } 

        internal static bool IsInitialized { get; set; }

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
                        ? deviceId : CountlyHelper.GenerateUniqueDeviceID();

            if (string.IsNullOrEmpty(ServerUrl))
                throw new ArgumentNullException(serverUrl, "ServerURL is required.");
            if (string.IsNullOrEmpty(AppKey))
                throw new ArgumentNullException(appKey, "AppKey is required.");

            //Set DeviceID in Cache if it doesn't already exists in Cache
            if (CountlyHelper.IsNullEmptyOrWhitespace(storedDeviceId))
                PlayerPrefs.SetString(Constants.DeviceIDKey, DeviceId);

            //Initialzing log breadcrumbs on app start
            CrashBreadcrumbs = new List<string>();

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
        public static async Task<CountlyResponse> SetDefaults(string salt = null, bool enablePost = false, 
            bool enableConsoleErrorLogging = false, bool ignoreSessionCooldown = false,
            TestMode? notificationMode = null)
        {
            Salt = salt;
            PostRequestEnabled = enablePost;
            PreventRequestTanmpering = !string.IsNullOrEmpty(salt);
            EnableConsoleErrorLogging = enableConsoleErrorLogging;
            IgnoreSessionCooldown = ignoreSessionCooldown;

            //Start Session
            await BeginSessionAsync();
            
            SetStoredRequestLimit(1000);

            //Enables push notification on start
            if (notificationMode.HasValue)
            {
                EnablePush(notificationMode.Value);
            }

            //SDK has been initialized now
            IsInitialized = true;

            return new CountlyResponse
            {
                IsSuccess = true
            };
        }

        #endregion

        #region Optional Parameters

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

        #endregion

        #region Internal Configuration

        /// <summary>
        /// Sets a threshold value that limits the number of events that can be stored internally.
        /// Default is 1000 number of events.
        /// </summary>
        /// <param name="eventThreshold"></param>
        public static void SetEventSendThreshold(int eventThreshold)
        {
            EventSendThreshold = eventThreshold;
        }

        /// <summary>
        /// Sets a threshold value that limits the number of requests that can be stored internally 
        /// </summary>
        /// <param name="limit"></param>
        public static void SetStoredRequestLimit(int limit)
        {
            StoredRequestLimit = limit;
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
        public static async Task<CountlyResponse> ChangeDeviceIDAndEndCurrentSessionAsync(string deviceId)
        {
            //Ignore call if new and old device id are same
            if (DeviceId == deviceId)
                return new CountlyResponse { IsSuccess = true };

            //Add currently recorded but not queued events to request queue-----------------------------------
            EndAllRecordedEventsAsync();

            //Ends current session
            //Do not dispose timer object
            await ExecuteEndSessionAsync(false);

            //Clear all started timed-events------------------------------------------------------------------
            //------------------------------------------------------------------------------------------------

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

        #region Crash Reporting

        /// <summary>
        /// Called when there is an exception 
        /// </summary>
        /// <param name="message">Exception Class</param>
        /// <param name="stackTrace">Stack Trace</param>
        /// <param name="type">Excpetion type like error, warning, etc</param>
        internal static async void LogCallback(string message, string stackTrace, LogType type)
        {
            if (type == LogType.Error
                || type == LogType.Exception)
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
        private static async Task<CountlyResponse> SendCrashReportAsync(string message, string stackTrace, LogType type, string segments = null, bool nonfatal = true)
        {
            //if (ConsentModel.CheckConsent(FeaturesEnum.Crashes.ToString()))
            //{
            var model = CountlyExceptionDetailModel.ExceptionDetailModel;
            model.Error = stackTrace;
            model.Name = message;
            model.Nonfatal = nonfatal;
            model.Custom = segments;
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

        /// <summary>
        /// Sends custom logged errors to the server.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stackTrace"></param>
        /// <param name="type"></param>
        /// <param name="segments"></param>
        /// <returns></returns>
        public static async Task<CountlyResponse> SendCrashReportAsync(string message, string stackTrace, LogType type, string segments = null)
        {
            return await SendCrashReportAsync(message, stackTrace, type, segments, true);
        }

        /// <summary>
        /// Adds string value to a list which is later sent over as logs whenever a cash is reported by system
        /// </summary>
        /// <param name="value"></param>
        public static void AddBreadcrumbs(string value)
        {
            CrashBreadcrumbs.Add(value);
        }

        #endregion

        #region Sessions

        /// <summary>
        /// Initiates a session by setting begin_session
        /// </summary>
        public static async Task<CountlyResponse> BeginSessionAsync()
        {
            return await ExecuteBeginSessionAsync();
        }

        /// <summary>
        /// Ends a session by setting end_session
        /// </summary>
        public static async Task<CountlyResponse> EndSessionAsync()
        {
            return await ExecuteEndSessionAsync();
        }

        /// <summary>
        /// Sets the session duration. Session will be extended each time this interval elapses. The interval value must be in seconds.
        /// </summary>
        /// <param name="interval"></param>
        public static void SetSessionDuration(int interval)
        {
            _extendSessionInterval = interval;
            _sessionTimer.Interval = _extendSessionInterval * 1000;
        }

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
                //Start session timer
                InitSessionTimer();
                _sessionTimer.Start();
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

            //Do not extend session after session ends
            if (disposeTimer)
            {
                _sessionTimer.Stop();
                _sessionTimer.Dispose();
                _sessionTimer.Close();
            }
            //}
            return response;
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

        #region Events

        /// <summary>
        /// Reports a custom event to the Counlty server.
        /// </summary>
        /// <returns></returns>
        public static async Task<CountlyResponse> ReportCustomEventAsync(string key, string segmentation = null, int? count = 1,
                                                    double? sum = null, double? duration = null)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(key, "Key is required.");

            var evnt = new CountlyEventModel(key, segmentation, count, sum, duration);

            var requestParams =
               new Dictionary<string, object>
               {
                    { "events", JsonConvert.SerializeObject(new List<CountlyEventModel>{ evnt }, Formatting.Indented,
                                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }) },
               };
            return await CountlyHelper.GetResponseAsync(requestParams);
        }

        /// <summary>
        /// Adds an event with the specified key. Doesn't send the request to the Countly API
        /// </summary>
        /// <param name="key"></param>
        public static void StartEvent(string key)
        {
            CountlyEventModel.StartEvent(key);
        }

        /// <summary>
        /// Ends an event with the specified key. Sends events details to the Counlty API 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static async Task<CountlyResponse> EndEventAsync(string key)
        {
            return await CountlyEventModel.EndAsync(key);
        }

        /// <summary>
        /// Ends an event with the specified key and data. Sends events details to the Counlty API 
        /// </summary>
        /// <param name="key">Key is required</param>
        /// <param name="segmentation">Custom </param>
        /// <param name="count"></param>
        /// <param name="sum"></param>
        /// <returns></returns>
        public static async Task<CountlyResponse> EndEventAsync(string key, string segmentation, int? count = 1, double? sum = 0)
        {
            return await CountlyEventModel.EndAsync(key, segmentation, count, sum);
        }

        /// <summary>
        /// Adds all recorded but not queued events to Request Queue
        /// </summary>
        private static async void EndAllRecordedEventsAsync()
        {
            var events = TotalEvents.Select(x => x.Key).ToList();
            foreach (var evnt in events)
            {
                await CountlyEventModel.EndAsync(evnt, null, null, null, true);
            }
        }

        /// <summary>
        /// Sends multiple events to the countly server. It expects a list of events as input.
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        public static async Task<CountlyResponse> ReportMultipleEventsAsync(List<CountlyEventModel> events)
        {
            if (events == null || events.Count == 0)
                throw new ArgumentException("No events found to record.");

            var currentTime = DateTime.UtcNow;
            foreach (var evnt in events)
            {
                CountlyEventModel.SetTimeZoneInfo(evnt, currentTime);
            }

            var requestParams =
               new Dictionary<string, object>
               {
                    { "events", JsonConvert.SerializeObject(events, Formatting.Indented,
                                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }) },
               };
            return await CountlyHelper.GetResponseAsync(requestParams);
        }

        #endregion

        #region Views

        /// <summary>
        /// Reports a view with the specified name and a last visited view if it existed
        /// </summary>
        /// <param name="name"></param>
        /// <param name="hasSessionBegunWithView"></param>
        public static async Task<CountlyResponse> ReportViewAsync(string name, bool hasSessionBegunWithView = false)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("Parameter name is required.");

            var events = new List<CountlyEventModel>();
            var lastView = GetLastView();
            if (lastView != null)
                events.Add(lastView);

            var currentViewSegment =
                new ViewSegment
                {
                    Name = name,
                    Segment = Application.platform.ToString(),
                    Visit = 1,
                    HasSessionBegunWithView = hasSessionBegunWithView
                };

            var currentView = new CountlyEventModel(
                                    CountlyEventModel.ViewEvent,
                                    JsonConvert.SerializeObject(currentViewSegment, Formatting.Indented,
                                        new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, }));
            events.Add(currentView);

            var res = await ReportMultipleEventsAsync(events);

            _lastView = name;
            _lastViewStartTime = DateTime.Now;

            return res;
        }

        private static CountlyEventModel GetLastView(bool hasSessionBegunWithView = false)
        {
            if (string.IsNullOrEmpty(_lastView) && string.IsNullOrWhiteSpace(_lastView))
                return null;

            var viewSegment =
                new ViewSegment
                {
                    Name = _lastView,
                    Segment = Application.platform.ToString(),
                    HasSessionBegunWithView = hasSessionBegunWithView
                };

            var customEvent = new CountlyEventModel(
                                    CountlyEventModel.ViewEvent,
                                    JsonConvert.SerializeObject(viewSegment, Formatting.Indented,
                                        new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, }),
                                    null, (DateTime.Now - _lastViewStartTime).TotalMilliseconds);

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

            return await ReportCustomEventAsync(
                           CountlyEventModel.ViewActionEvent,
                           JsonConvert.SerializeObject(segment, Formatting.Indented,
                               new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, }),
                           null, null, null);
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
        public static async Task<CountlyResponse> ReportStarRatingAsync(string platform, string app_version, int rating)
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

            return await ReportCustomEventAsync(
                            CountlyEventModel.StarRatingEvent,
                            JsonConvert.SerializeObject(segment, Formatting.Indented,
                                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, }),
                            null, null, null);
        }

        #endregion

        #region User Details

        /// <summary>
        /// Modifies all user data. Custom data should be json string.
        /// Deletes an already defined custom property from the Countly server, if it is supplied with a NULL value
        /// </summary>
        /// <param name="userDetails"></param>
        /// <returns></returns>
        public static async Task<CountlyResponse> UserDetailsAsync(CountlyUserDetailsModel userDetails)
        {
            if (userDetails == null)
                throw new ArgumentNullException("Please provide user details.");

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
                throw new ArgumentNullException("Please provide user details.");

            return await userDetails.SetCustomUserDetailsAsync();
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

        #region Push Notifications

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

        /// <summary>
        /// Intializes the timer for extending session with sepcified interval
        /// </summary>
        /// <param name="sessionInterval">In milliseconds</param>
        private static void InitSessionTimer()
        {
            _sessionTimer = new Timer();
            _sessionTimer.Interval = _extendSessionInterval * 1000;
            _sessionTimer.Elapsed += SessionTimerOnElapsedAsync;
            _sessionTimer.AutoReset = true;
        }

        /// <summary>
        /// Extends the session after the session duration is elapsed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="elapsedEventArgs"></param>
        private static async void SessionTimerOnElapsedAsync(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            await ExtendSessionAsync();
            CountlyRequestModel.ProcessQueue();
        }

        #endregion
    }
}
