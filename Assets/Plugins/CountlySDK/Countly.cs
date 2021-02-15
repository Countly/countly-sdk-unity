using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using iBoxDB.LocalServer;
using Notifications;
using Notifications.Impls;
using Plugins.CountlySDK.Helpers;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Persistance;
using Plugins.CountlySDK.Persistance.Dao;
using Plugins.CountlySDK.Persistance.Entities;
using Plugins.CountlySDK.Persistance.Repositories;
using Plugins.CountlySDK.Persistance.Repositories.Impls;
using Plugins.CountlySDK.Services;
using Plugins.iBoxDB;
using UnityEngine;

[assembly: InternalsVisibleTo("PlayModeTests")]
namespace Plugins.CountlySDK
{
    public class Countly : MonoBehaviour
    {

        public CountlyAuthModel Auth;
        public CountlyConfigModel Config;
        internal CountlyConfiguration Configuration;

        /// <summary>
        /// Check if SDK has been initialized.
        /// </summary>
        /// <returns>bool</returns>
        public bool IsSDKInitialized { get; private set; }

        private CountlyLogHelper _logHelper;
        private static Countly _instance = null;
        private List<AbstractBaseService> _listeners = new List<AbstractBaseService>();

        /// <summary>
        /// Return countly shared instance.
        /// </summary>
        /// <returns>Countly</returns>
        public static Countly Instance
        {
            get {
                if (_instance == null) {

                    GameObject gameObject = new GameObject("_countly");
                    _instance = gameObject.AddComponent<Countly>();
                }

                return _instance;

            }
            internal set {
                _instance = value;
            }
        }

        /// <summary>
        /// Check/Update consent for a particular feature.
        /// </summary>
        ///<returns>ConsentCountlyService</returns>
        public ConsentCountlyService Consents { get; private set; }

        [Obsolete("CrushReports is deprecated, please use CrashReports instead.")]
        public CrashReportsCountlyService CrushReports { get { return CrashReports; } }

        /// <summary>
        /// Exposes functionality to record crashes/errors and record breadcrumbs.
        /// </summary>
        /// <returns>CrashReportsCountlyService</returns>
        public CrashReportsCountlyService CrashReports { get; private set; }

        /// <summary>
        /// Exposes functionality to get the current device ID and change id.
        /// </summary>
        /// <returns>DeviceIdCountlyService</returns>
        public DeviceIdCountlyService Device { get; private set; }

        /// <summary>
        /// Exposes functionality to record custom events.
        /// </summary>
        /// <returns>EventCountlyService</returns>
        public EventCountlyService Events { get; private set; }

        internal InitializationCountlyService Initialization { get; private set; }

        [Obsolete("OptionalParameters is deprecated, please use Location instead.")]
        public OptionalParametersCountlyService OptionalParameters { get; private set; }

        /// <summary>
        ///     Exposes functionality to set location parameters.
        /// </summary>
        /// <returns>LocationService</returns>
        public Services.LocationService Location { get; private set; }

        /// <summary>
        ///     Exposes functionality to update the remote config values. It also provides a way to access the currently downloaded ones.
        /// </summary>
        /// <returns>RemoteConfigCountlyService</returns>
        public RemoteConfigCountlyService RemoteConfigs { get; private set; }

        /// <summary>
        ///     Exposes functionality to report start rating.
        /// </summary>
        /// <returns>StarRatingCountlyService</returns>
        public StarRatingCountlyService StarRating { get; private set; }

        /// <summary>
        ///     Exposes functionality to set and change custom user properties and interract with custom property modifiers.
        /// </summary>
        /// <returns>UserDetailsCountlyService</returns>
        public UserDetailsCountlyService UserDetails { get; private set; }

        /// <summary>
        ///     Exposes functionality to start and stop recording views and report positions for heatmap.
        /// </summary>
        /// <returns>ViewCountlyService</returns>
        public ViewCountlyService Views { get; private set; }

        private SessionCountlyService Session { get; set; }

        /// <summary>
        ///     Add callbacks to listen to push notification events for when a notification is received and when it is clicked.
        /// </summary>
        /// <returns>NotificationsCallbackService</returns>
        public NotificationsCallbackService Notifications { get; set; }

        private DB _db;
        private bool _logSubscribed;
        internal const long DbNumber = 3;
        private PushCountlyService _push;

        private Dao<ConfigEntity> _configDao;
        private RequestRepository _requestRepo;
        private ViewEventRepository _viewEventRepo;
        private NonViewEventRepository _nonViewEventRepo;


        /// <summary>
        ///     Initialize SDK at the start of your app
        /// </summary>
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;

            //Auth and Config will not be null in case initializing through countly prefab
            if (Auth != null && Config != null) {
                Init(new CountlyConfiguration(Auth, Config));
            }

        }

        public void Init(CountlyConfiguration configuration)
        {
            if (IsSDKInitialized) {
                return;
            }

            _logHelper.Info("[Init] Initializing Countly [SdkName: " + Constants.SdkName + " SdkVersion: " + Constants.SdkVersion + "]");


            if (configuration.Parent != null) {
                transform.parent = configuration.Parent.transform;
            }

            if (string.IsNullOrEmpty(configuration.ServerUrl)) {
                throw new ArgumentNullException(configuration.ServerUrl, "Server URL is required.");
            }

            if (string.IsNullOrEmpty(configuration.AppKey)) {
                throw new ArgumentNullException(configuration.AppKey, "App Key is required.");
            }

            if (configuration.ServerUrl[configuration.ServerUrl.Length - 1] == '/') {
                configuration.ServerUrl = configuration.ServerUrl.Remove(configuration.ServerUrl.Length - 1);
            }

            Configuration = configuration;
            _logHelper = new CountlyLogHelper(Configuration);

            _db = CountlyBoxDbHelper.BuildDatabase(DbNumber);

            DB.AutoBox auto = _db.Open();
            _configDao = new Dao<ConfigEntity>(auto, EntityType.Configs.ToString(), _logHelper);
            Dao<RequestEntity> requestDao = new Dao<RequestEntity>(auto, EntityType.Requests.ToString(), _logHelper);
            Dao<EventEntity> viewEventDao = new Dao<EventEntity>(auto, EntityType.ViewEvents.ToString(), _logHelper);
            SegmentDao viewSegmentDao = new SegmentDao(auto, EntityType.ViewEventSegments.ToString(), _logHelper);
            Dao<EventEntity> nonViewEventDao = new Dao<EventEntity>(auto, EntityType.NonViewEvents.ToString(), _logHelper);
            SegmentDao nonViewSegmentDao = new SegmentDao(auto, EntityType.NonViewEventSegments.ToString(), _logHelper);

            _requestRepo = new RequestRepository(requestDao, _logHelper);
            _viewEventRepo = new ViewEventRepository(viewEventDao, viewSegmentDao, _logHelper);
            _nonViewEventRepo = new NonViewEventRepository(nonViewEventDao, nonViewSegmentDao, _logHelper);

            Dao<EventNumberInSameSessionEntity> eventNrInSameSessionDao = new Dao<EventNumberInSameSessionEntity>(auto, EntityType.EventNumberInSameSessions.ToString(), _logHelper);
            eventNrInSameSessionDao.RemoveAll(); /* Clear EventNumberInSameSessions Entity data */

            _requestRepo.Initialize();
            _viewEventRepo.Initialize();
            _nonViewEventRepo.Initialize();

            Init(_requestRepo, _viewEventRepo, _nonViewEventRepo, _configDao);

            Device.InitDeviceId(configuration.DeviceId);
            OnInitialisationComplete();

            _logHelper.Debug("[Countly] Finished Initializing SDK.");

        }

        private void Init(RequestRepository requestRepo, ViewEventRepository viewEventRepo,
            NonViewEventRepository nonViewEventRepo, Dao<ConfigEntity> configDao)
        {
            CountlyUtils countlyUtils = new CountlyUtils(this);
            RequestCountlyHelper requests = new RequestCountlyHelper(Configuration, _logHelper, countlyUtils, requestRepo);

            Consents = new ConsentCountlyService(Configuration, _logHelper, Consents);
            Events = new EventCountlyService(Configuration, _logHelper, requests, viewEventRepo, nonViewEventRepo, Consents);

            Location = new Services.LocationService(Configuration, _logHelper, requests, Consents);
            OptionalParameters = new OptionalParametersCountlyService(Location, Configuration);
            Notifications = new NotificationsCallbackService(_logHelper);
            ProxyNotificationsService notificationsService = new ProxyNotificationsService(transform, Configuration, _logHelper, InternalStartCoroutine, Events);
            _push = new PushCountlyService(Configuration, _logHelper, requests, notificationsService, Notifications, Consents);
            Session = new SessionCountlyService(Configuration, _logHelper, Events, requests, Location, Consents);

            CrashReports = new CrashReportsCountlyService(Configuration, _logHelper, requests, Consents);
            Initialization = new InitializationCountlyService(Configuration, _logHelper, Location, Session, Consents);
            RemoteConfigs = new RemoteConfigCountlyService(Configuration, _logHelper, requests, countlyUtils, configDao, Consents);

            StarRating = new StarRatingCountlyService(Configuration, _logHelper, Consents, Events);
            UserDetails = new UserDetailsCountlyService(Configuration, _logHelper, requests, countlyUtils, Consents);
            Views = new ViewCountlyService(Configuration, _logHelper, Events, Consents);
            Device = new DeviceIdCountlyService(Configuration, _logHelper, Session, requests, Events, countlyUtils, Consents);

            CreateListOfIBaseService();
            RegisterListenersToServices();
        }

        private async void OnInitialisationComplete()
        {
            IsSDKInitialized = true;
            await Initialization.OnInitialisationComplete();
            foreach (AbstractBaseService listener in _listeners) {
                listener.OnInitializationCompleted();
            }
        }

        private void CreateListOfIBaseService()
        {
            _listeners.Clear();

            _listeners.Add(_push);
            _listeners.Add(Views);
            _listeners.Add(Events);
            _listeners.Add(Device);
            _listeners.Add(Session);
            _listeners.Add(Location);
            _listeners.Add(Consents);
            _listeners.Add(StarRating);
            _listeners.Add(UserDetails);
            _listeners.Add(CrashReports);
            _listeners.Add(RemoteConfigs);
            _listeners.Add(Initialization);
        }

        private void RegisterListenersToServices()
        {
            Device.Listeners = _listeners;
            Consents.Listeners = _listeners;
        }

        /// <summary>
        ///     End session on application close/quit
        /// </summary>
        private void OnApplicationQuit()
        {
            if (!IsSDKInitialized) {
                return;
            }

            _logHelper.Debug("[Countly] OnApplicationQuit");

            _db.Close();
        }

        internal void ClearStorage()
        {
            if (!IsSDKInitialized) {
                return;
            }
            _logHelper.Debug("[Countly] ClearStorage");

            _requestRepo.Clear();
            _viewEventRepo.Clear();
            _configDao.RemoveAll();
            _nonViewEventRepo.Clear();

            PlayerPrefs.DeleteAll();

            _db.Close();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            _logHelper.Debug("[Countly] OnApplicationFocus: " + hasFocus);

            if (hasFocus) {
                SubscribeAppLog();
            } else {
                HandleAppPauseOrFocus();
            }
        }

        private async void OnApplicationPause(bool pauseStatus)
        {
            _logHelper.Debug("[Countly] OnApplicationPause: " + pauseStatus);

            if (CrashReports != null) {
                CrashReports.IsApplicationInBackground = pauseStatus;
            }

            if (pauseStatus) {
                HandleAppPauseOrFocus();
                await Session?.EndSessionAsync();
            } else {
                SubscribeAppLog();
                await Session?.ExecuteBeginSessionAsync();

            }
        }

        private void HandleAppPauseOrFocus()
        {
            UnsubscribeAppLog();
        }

        // Whenever app is enabled
        private void OnEnable()
        {
            SubscribeAppLog();
        }

        // Whenever app is disabled
        private void OnDisable()
        {
            UnsubscribeAppLog();
        }

        private void LogCallback(string condition, string stackTrace, LogType type)
        {
            CrashReports?.LogCallback(condition, stackTrace, type);
        }

        private void SubscribeAppLog()
        {
            if (_logSubscribed) {
                return;
            }

            Application.logMessageReceived += LogCallback;
            _logSubscribed = true;
        }

        private void UnsubscribeAppLog()
        {
            if (!_logSubscribed) {
                return;
            }

            Application.logMessageReceived -= LogCallback;
            _logSubscribed = false;
        }

        private void InternalStartCoroutine(IEnumerator enumerator)
        {
            StartCoroutine(enumerator);
        }
    }
}