using System;
using System.Collections;
using CountlySDK.Input;
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

namespace Plugins.CountlySDK
{
    public class Countly : MonoBehaviour
    {

        public CountlyAuthModel Auth;
        public CountlyConfigModel Config;
        private CountlyConfiguration Configuration;

        /// <summary>
        /// Is sdk has been initialized?
        /// </summary>
        /// <returns>bool</returns>
        public bool IsSDKInitialized { get; private set; }

        private static Countly _instance = null;

        /// <summary>
        /// Return countly shared instance.
        /// </summary>
        /// <returns>Countly</returns>
        public static Countly Instance {
            get
            {
                if (_instance == null)
                {

                    var gameObject = new GameObject("_countly");
                    _instance = gameObject.AddComponent<Countly>();
                }

                return _instance;

            }
            internal set
            {
                _instance = value;
            }
        }

        /// <summary>
        /// Check/Update for a particular feature.
        /// </summary>
        ///<returns>ConsentCountlyService</returns>
        public ConsentCountlyService Consents { get; private set; }

        [Obsolete("CrushReports is deprecated, please use CrashReports instead.")]
        public CrashReportsCountlyService CrushReports { get { return CrashReports; } }

        /// <summary>
        /// Collect crash reports.
        /// </summary>
        /// <returns>CrashReportsCountlyService</returns>
        public CrashReportsCountlyService CrashReports { get; private set; }

        /// <summary>
        /// Changing Device ID.
        /// </summary>
        /// <returns>DeviceIdCountlyService</returns>
        public DeviceIdCountlyService Device { get; private set; }

        /// <summary>
        /// Record events.
        /// </summary>
        /// <returns>EventCountlyService</returns>
        public EventCountlyService Events { get; private set; }


        /// <summary>
        ///     Initializes countly instance.
        /// </summary>
        /// <returns>InitializationCountlyService</returns>
        internal InitializationCountlyService Initialization { get; private set; }

        /// <summary>
        ///     Track user location.
        /// </summary>
        /// <returns>OptionalParametersCountlyService</returns>
        public OptionalParametersCountlyService OptionalParameters { get; private set; }

        /// <summary>
        ///     Access/Update remote config.
        /// </summary>
        /// <returns>RemoteConfigCountlyService</returns>
        public RemoteConfigCountlyService RemoteConfigs { get; private set; }

        /// <summary>
        ///     Collect user feedback and comments.
        /// </summary>
        /// <returns>StarRatingCountlyService</returns>
        public StarRatingCountlyService StarRating { get; private set; }

        /// <summary>
        ///     Upload specific data related to a user.
        /// </summary>
        /// <returns>UserDetailsCountlyService</returns>
        public UserDetailsCountlyService UserDetails { get; private set; }

        /// <summary>
        ///     View tracking.
        /// </summary>
        /// <returns>ViewCountlyService</returns>
        public ViewCountlyService Views { get; private set; }

        private SessionCountlyService Session { get; set; }

        /// <summary>
        ///     Listen push notification.
        /// </summary>
        /// <returns>NotificationsCallbackService</returns>
        public NotificationsCallbackService Notifications { get; set; }
        

        public async void ReportAll()
        {
            await Events.AddEventsToRequestQueue();
            await UserDetails.SaveAsync();
        }

        private DB _db;
        private bool _logSubscribed;
        private const long DbNumber = 3;
        private PushCountlyService _push;
        private IInputObserver _inputObserver;

        /// <summary>
        ///     Initialize SDK at the start of your app
        /// </summary>
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;

            //Auth and Config will not be null in case initializing through countly prefab
            if (Auth != null && Config != null)
            {
                Init(new CountlyConfiguration(Auth, Config));
            }
            
        }

        public async void Init(CountlyConfiguration configuration)
        {
            if (IsSDKInitialized)
            {
                return;
            }

            if (configuration.Parent != null)
            {
                transform.parent = configuration.Parent.transform;
            }
            
            Configuration = configuration;

            _db = CountlyBoxDbHelper.BuildDatabase(DbNumber);

            var auto = _db.Open();
            var configDao = new Dao<ConfigEntity>(auto, EntityType.Configs.ToString());
            var requestDao = new Dao<RequestEntity>(auto, EntityType.Requests.ToString());
            var viewEventDao = new Dao<EventEntity>(auto, EntityType.ViewEvents.ToString());
            var viewSegmentDao = new SegmentDao(auto, EntityType.ViewEventSegments.ToString());
            var nonViewEventDao = new Dao<EventEntity>(auto, EntityType.NonViewEvents.ToString());  
            var nonViewSegmentDao = new SegmentDao(auto, EntityType.NonViewEventSegments.ToString());

            var requestRepo = new RequestRepository(requestDao, Configuration);
            var eventViewRepo = new ViewEventRepository(viewEventDao, viewSegmentDao, Configuration);
            var eventNonViewRepo = new NonViewEventRepository(nonViewEventDao, nonViewSegmentDao, Configuration);
            var eventNrInSameSessionDao = new EventNumberInSameSessionDao(auto, EntityType.EventNumberInSameSessions.ToString());

            requestRepo.Initialize();
            eventViewRepo.Initialize();
            eventNonViewRepo.Initialize();

            var eventNumberInSameSessionHelper = new EventNumberInSameSessionHelper(eventNrInSameSessionDao);

            Init(requestRepo, eventViewRepo, eventNonViewRepo, configDao, eventNumberInSameSessionHelper);

            
            Initialization.Begin(configuration.ServerUrl, configuration.AppKey);
            Device.InitDeviceId(configuration.DeviceId);

            await Initialization.SetDefaults(Configuration);

            IsSDKInitialized = true;
        }

        private void Init(RequestRepository requestRepo, ViewEventRepository viewEventRepo,
            NonViewEventRepository nonViewEventRepo, Dao<ConfigEntity> configDao, EventNumberInSameSessionHelper eventNumberInSameSessionHelper)
        {
            var countlyUtils = new CountlyUtils(this);
            var requests = new RequestCountlyHelper(Configuration, countlyUtils, requestRepo);

            Events = new EventCountlyService(Configuration, requests, viewEventRepo, nonViewEventRepo, eventNumberInSameSessionHelper);
            OptionalParameters = new OptionalParametersCountlyService();
            Notifications = new NotificationsCallbackService(Configuration);
            var notificationsService = new ProxyNotificationsService(transform, Configuration, InternalStartCoroutine, Events);
            _push = new PushCountlyService(Events, requests, notificationsService, Notifications);
            Session = new SessionCountlyService(Configuration, Events, _push, requests, OptionalParameters, eventNumberInSameSessionHelper);
            
            Consents = new ConsentCountlyService();
            CrashReports = new CrashReportsCountlyService(Configuration, requests);

            Device = new DeviceIdCountlyService(Session, requests, Events, countlyUtils);
            Initialization = new InitializationCountlyService(Session);

            RemoteConfigs = new RemoteConfigCountlyService(Configuration, requests, countlyUtils, configDao);

            StarRating = new StarRatingCountlyService(Events);
            UserDetails = new UserDetailsCountlyService(requests, countlyUtils);
            Views = new ViewCountlyService(Configuration, Events);
            _inputObserver = InputObserverResolver.Resolve();
        }


        /// <summary>
        ///     End session on application close/quit
        /// </summary>
        private async void OnApplicationQuit()
        {
            if (Configuration.EnableConsoleLogging)
            {
                Debug.Log("[Countly] OnApplicationQuit");
            }

            if (Session != null && Session.IsSessionInitiated && !Configuration.EnableManualSessionHandling)
            {
                ReportAll();
                await Session.EndSessionAsync();
            }
            _db.Close();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (Configuration.EnableConsoleLogging)
            {
                Debug.Log("[Countly] OnApplicationFocus: " + hasFocus);
            }

            if (hasFocus)
            {
                SubscribeAppLog();
            }
            else
            {
                HandleAppPauseOrFocus();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (Configuration.EnableConsoleLogging)
            {
                Debug.Log("[Countly] OnApplicationPause: " + pauseStatus);
            }

            if (pauseStatus)
            {
                HandleAppPauseOrFocus();
            }
            else
            {
                SubscribeAppLog();
            }
        }

        private void HandleAppPauseOrFocus()
        {
            UnsubscribeAppLog();
            if (Session != null && Session.IsSessionInitiated)
            {
                ReportAll();
            }
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
            if (_logSubscribed)
            {
                return;
            }

            Application.logMessageReceived += LogCallback;
            _logSubscribed = true;
        }

        private void UnsubscribeAppLog()
        {
            if (!_logSubscribed)
            {
                return;
            }

            Application.logMessageReceived -= LogCallback;
            _logSubscribed = false;
        }

        private void Update()
        {
            CheckInputEvent();
        }

        private void CheckInputEvent()
        {
            if (!_inputObserver.HasInput)
            {
                return;
            }

            Session?.UpdateInputTime();
        }

        private void InternalStartCoroutine(IEnumerator enumerator)
        {
            StartCoroutine(enumerator);
        }
    }
}