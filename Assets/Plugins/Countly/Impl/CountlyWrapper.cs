using System;
using Countly.Input;
using Plugins.Countly.Services;
using Plugins.Countly.Services.Impls.Actual;
using Plugins.Countly.Services.Impls.Wrapper;
using UnityEngine;

namespace Plugins.Countly.Impl
{
    public class CountlyWrapper : MonoBehaviour, ICountly 
    {
        public IConsentCountlyService Consents { get; private set; }

        public ICrashReportsCountlyService CrashReports { get; private set; }

        public ICrashReportsCountlyService CrushReports { get; private set; }

        public IDeviceIdCountlyService Device { get; private set; }

        public IEventCountlyService Events { get; private set; }

        public IInitializationCountlyService Initialization { get; private set; }

        public IOptionalParametersCountlyService OptionalParameters { get; private set; }

        public IRemoteConfigCountlyService RemoteConfigs { get; private set; }
        
        public IStarRatingCountlyService StarRating { get; private set; }

        public IUserDetailsCountlyService UserDetails { get; private set; }

        public IViewCountlyService Views { get; private set; }
        
        public async void ReportAll()
        {
            await Events.ReportAllRecordedViewEventsAsync();
            await Events.ReportAllRecordedNonViewEventsAsync();
            await UserDetails.SaveAsync();    
        }

        private IInputObserver _inputObserver;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            Consents = new ConsentCountlyServiceWrapper();
            CrashReports = new CrashReportsCountlyServiceWrapper();
            CrushReports = CrashReports;
            Device = new DeviceIdCountlyServiceWrapper();
            Events = new EventCountlyServiceWrapper();
            Initialization = new InitializationCountlyServiceWrapper();
            OptionalParameters = new OptionalParametersCountlyServiceWrapper();
            RemoteConfigs = new RemoteConfigCountlyServiceWrapper();
            StarRating = new StarRatingCountlyServiceWrapper();
            UserDetails = new UserDetailsCountlyServiceWrapper();
            Views = new ViewCountlyServiceWrapper();
            _inputObserver = InputObserverResolver.Resolve();
        }

        private void Update()
        {
            CheckLastInput();
        }

        private void CheckLastInput()
        {
            if(!_inputObserver.HasInput)
                return;
            Debug.Log($"[CountlyWrapper] Last touch date: " + DateTime.Now);
        }
    }
}