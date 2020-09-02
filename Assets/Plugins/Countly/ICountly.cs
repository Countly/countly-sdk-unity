using Plugins.Countly.Services;
using Plugins.Countly.Services.Impls.Actual;
using System;

namespace Plugins.Countly
{
    public interface ICountly
    {
        IConsentCountlyService Consents { get; }
        ICrashReportsCountlyService CrashReports { get; }

        [Obsolete("CrushReports is deprecated, please use CrashReports instead.")]
        ICrashReportsCountlyService CrushReports { get; }

        IDeviceIdCountlyService Device { get; }
        IEventCountlyService Events { get; }
        IInitializationCountlyService Initialization { get; }
        IOptionalParametersCountlyService OptionalParameters { get; }
        IRemoteConfigCountlyService RemoteConfigs { get; }
        IStarRatingCountlyService StarRating { get; }
        IUserDetailsCountlyService UserDetails { get; }
        IViewCountlyService Views { get; }

        void ReportAll();
    }
}