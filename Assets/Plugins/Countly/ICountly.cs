using Notifications;
using Plugins.Countly.Services.Impls.Actual;
using System;

namespace Plugins.Countly
{
    public interface ICountly
    {
        ConsentCountlyService Consents { get; }
        CrashReportsCountlyService CrashReports { get; }

        [Obsolete("CrushReports is deprecated, please use CrashReports instead.")]
        CrashReportsCountlyService CrushReports { get; }

        DeviceIdCountlyService Device { get; }
        EventCountlyService Events { get; }
        InitializationCountlyService Initialization { get; }
        OptionalParametersCountlyService OptionalParameters { get; }
        RemoteConfigCountlyService RemoteConfigs { get; }
        StarRatingCountlyService StarRating { get; }
        UserDetailsCountlyService UserDetails { get; }
        ViewCountlyService Views { get; }
        NotificationsCallbackService Notifications { get;}

        void ReportAll();
    }
}