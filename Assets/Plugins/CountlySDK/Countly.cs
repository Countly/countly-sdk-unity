using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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

[assembly: InternalsVisibleTo("Tests")]
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

        private static Countly _instance = null;

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
        ///     Exposes functinality to report start rating.
        /// </summary>
        /// <returns>StarRatingCountlyService</returns>
        public StarRatingCountlyService StarRating { get; private set; }

        /// <summary>
        ///     Exposes functionality to set and change custom user properties and interract with custom property modiffiers.
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
            if (Auth != null && Config != null) {
                Init(new CountlyConfiguration(Auth, Config));
            }

        }


        public void OnDestroy()
        {
            _db.Close();
        }

        public async void Init(CountlyConfiguration configuration)
        {
            if (IsSDKInitialized) {
                return;
            }

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

            _db = CountlyBoxDbHelper.BuildDatabase(DbNumber);

            DB.AutoBox auto = _db.Open();
            Dao<ConfigEntity> configDao = new Dao<ConfigEntity>(auto, EntityType.Configs.ToString(), Configuration);
            Dao<RequestEntity> requestDao = new Dao<RequestEntity>(auto, EntityType.Requests.ToString(), Configuration);
            Dao<EventEntity> viewEventDao = new Dao<EventEntity>(auto, EntityType.ViewEvents.ToString(), Configuration);
            SegmentDao viewSegmentDao = new SegmentDao(auto, EntityType.ViewEventSegments.ToString(), Configuration);
            Dao<EventEntity> nonViewEventDao = new Dao<EventEntity>(auto, EntityType.NonViewEvents.ToString(), Configuration);
            SegmentDao nonViewSegmentDao = new SegmentDao(auto, EntityType.NonViewEventSegments.ToString(), Configuration);

            RequestRepository requestRepo = new RequestRepository(requestDao, Configuration);
            ViewEventRepository eventViewRepo = new ViewEventRepository(viewEventDao, viewSegmentDao, Configuration);
            NonViewEventRepository eventNonViewRepo = new NonViewEventRepository(nonViewEventDao, nonViewSegmentDao, Configuration);
            EventNumberInSameSessionDao eventNrInSameSessionDao = new EventNumberInSameSessionDao(auto, EntityType.EventNumberInSameSessions.ToString(), Configuration);

            requestRepo.Initialize();
            eventViewRepo.Initialize();
            eventNonViewRepo.Initialize();
            
            EventNumberInSameSessionHelper eventNumberInSameSessionHelper = new EventNumberInSameSessionHelper(eventNrInSameSessionDao);

            Init(requestRepo, eventViewRepo, eventNonViewRepo, configDao, eventNumberInSameSessionHelper);


            Device.InitDeviceId(configuration.DeviceId);
            await Initialization.OnInitializationComplete();

            IsSDKInitialized = true;
        }

        private void Init(RequestRepository requestRepo, ViewEventRepository viewEventRepo,
            NonViewEventRepository nonViewEventRepo, Dao<ConfigEntity> configDao, EventNumberInSameSessionHelper eventNumberInSameSessionHelper)
        {
            CountlyUtils countlyUtils = new CountlyUtils(this);
            RequestCountlyHelper requests = new RequestCountlyHelper(Configuration, countlyUtils, requestRepo);

            Consents = new ConsentCountlyService();
            Events = new EventCountlyService(Configuration, requests, viewEventRepo, nonViewEventRepo, eventNumberInSameSessionHelper);

            Location = new Services.LocationService(Configuration, requests);
            OptionalParameters = new OptionalParametersCountlyService(Location, Configuration);
            Notifications = new NotificationsCallbackService(Configuration);
            ProxyNotificationsService notificationsService = new ProxyNotificationsService(transform, Configuration, InternalStartCoroutine, Events);
            _push = new PushCountlyService(Events, requests, notificationsService, Notifications);
            Session = new SessionCountlyService(Configuration, Events, _push, requests, Location, Consents, eventNumberInSameSessionHelper);

            CrashReports = new CrashReportsCountlyService(Configuration, requests);

            Device = new DeviceIdCountlyService(Session, requests, Events, countlyUtils);
            Initialization = new InitializationCountlyService(Configuration, Location, Consents, Session);

            RemoteConfigs = new RemoteConfigCountlyService(Configuration, requests, countlyUtils, configDao);

            StarRating = new StarRatingCountlyService(Events);
            UserDetails = new UserDetailsCountlyService(requests, countlyUtils);
            Views = new ViewCountlyService(Configuration, Events);
            _inputObserver = InputObserverResolver.Resolve();
        }


        /// <summary>
        ///     End session on application close/quit
        /// </summary>
        private void OnApplicationQuit()
        {
            if (Configuration.EnableConsoleLogging) {
                Debug.Log("[Countly] OnApplicationQuit");
            }

            _db.Close();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (Configuration.EnableConsoleLogging) {
                Debug.Log("[Countly] OnApplicationFocus: " + hasFocus);
            }

            if (hasFocus) {
                SubscribeAppLog();
            } else {
                HandleAppPauseOrFocus();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (Configuration.EnableConsoleLogging) {
                Debug.Log("[Countly] OnApplicationPause: " + pauseStatus);
            }

            if (pauseStatus) {
                HandleAppPauseOrFocus();
            } else {
                SubscribeAppLog();
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

        private void Update()
        {
            CheckInputEvent();
        }

        private void CheckInputEvent()
        {
            if (!_inputObserver.HasInput) {
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