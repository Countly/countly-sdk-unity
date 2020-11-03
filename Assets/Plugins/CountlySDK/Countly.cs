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
        public static Countly Instance { get; internal set; }

        public ConsentCountlyService Consents { get; private set; }

        [Obsolete("CrushReports is deprecated, please use CrashReports instead.")]
        public CrashReportsCountlyService CrushReports { get { return CrashReports; } }

        public CrashReportsCountlyService CrashReports { get; private set; }

        public DeviceIdCountlyService Device { get; private set; }

        public EventCountlyService Events { get; private set; }

        public InitializationCountlyService Initialization { get; private set; }

        public OptionalParametersCountlyService OptionalParameters { get; private set; }

        public RemoteConfigCountlyService RemoteConfigs { get; private set; }

        public StarRatingCountlyService StarRating { get; private set; }

        public UserDetailsCountlyService UserDetails { get; private set; }

        public ViewCountlyService Views { get; private set; }

        private SessionCountlyService Session { get; set; }

        public NotificationsCallbackService Notifications { get; set; }
        

        public async void ReportAll()
        {
            await Events.ReportAllRecordedViewEventsAsync();
            await Events.ReportAllRecordedNonViewEventsAsync();
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
        private async void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;

            _db = CountlyBoxDbHelper.BuildDatabase(DbNumber);

            var auto = _db.Open();
            var configDao = new Dao<ConfigEntity>(auto, EntityType.Configs.ToString());
            var requestDao = new Dao<RequestEntity>(auto, EntityType.Requests.ToString());
            var viewEventDao = new Dao<EventEntity>(auto, EntityType.ViewEvents.ToString());
            var viewSegmentDao = new SegmentDao(auto, EntityType.ViewEventSegments.ToString());
            var nonViewEventDao = new Dao<EventEntity>(auto, EntityType.NonViewEvents.ToString());  
            var nonViewSegmentDao = new SegmentDao(auto, EntityType.NonViewEventSegments.ToString());

            var requestRepo = new RequestRepository(requestDao, Config);
            var eventViewRepo = new ViewEventRepository(viewEventDao, viewSegmentDao, Config);
            var eventNonViewRepo = new NonViewEventRepository(nonViewEventDao, nonViewSegmentDao, Config);
            var eventNrInSameSessionDao = new EventNumberInSameSessionDao(auto, EntityType.EventNumberInSameSessions.ToString());

            requestRepo.Initialize();
            eventViewRepo.Initialize();
            eventNonViewRepo.Initialize();

            var eventNumberInSameSessionHelper = new EventNumberInSameSessionHelper(eventNrInSameSessionDao);

            Init(requestRepo, eventViewRepo, eventNonViewRepo, configDao, eventNumberInSameSessionHelper);

            
            Initialization.Begin(Auth.ServerUrl, Auth.AppKey);
            Device.InitDeviceId(Auth.DeviceId);

            await Initialization.SetDefaults(Config);
        }

        private void Init(RequestRepository requestRepo, ViewEventRepository viewEventRepo,
            NonViewEventRepository nonViewEventRepo, Dao<ConfigEntity> configDao, EventNumberInSameSessionHelper eventNumberInSameSessionHelper)
        {
            var countlyUtils = new CountlyUtils(this);
            var requests = new RequestCountlyHelper(Config, countlyUtils, requestRepo);

            Events = new EventCountlyService(Config, requests, viewEventRepo, nonViewEventRepo, eventNumberInSameSessionHelper);
            OptionalParameters = new OptionalParametersCountlyService();
            Notifications = new NotificationsCallbackService(Config);
            var notificationsService = new ProxyNotificationsService(Config, InternalStartCoroutine, Events);
            _push = new PushCountlyService(Events, requests, notificationsService, Notifications);
            Session = new SessionCountlyService(Config, _push, requests, OptionalParameters, eventNumberInSameSessionHelper);
            
            Consents = new ConsentCountlyService();
            CrashReports = new CrashReportsCountlyService(Config, requests);

            Device = new DeviceIdCountlyService(Session, requests, Events, countlyUtils);
            Initialization = new InitializationCountlyService(Session);

            RemoteConfigs = new RemoteConfigCountlyService(Config, requests, countlyUtils, configDao);

            StarRating = new StarRatingCountlyService(Events);
            UserDetails = new UserDetailsCountlyService(requests, countlyUtils);
            Views = new ViewCountlyService(Config, Events);
            _inputObserver = InputObserverResolver.Resolve();
        }


        /// <summary>
        ///     End session on application close/quit
        /// </summary>
        private async void OnApplicationQuit()
        {
            if (Config.EnableConsoleLogging)
            {
                Debug.Log("[Countly] OnApplicationQuit");
            }

            if (Session != null && Session.IsSessionInitiated && !Config.EnableManualSessionHandling)
            {
                ReportAll();
                await Session.EndSessionAsync();
            }
            _db.Close();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (Config.EnableConsoleLogging)
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
            if (Config.EnableConsoleLogging)
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