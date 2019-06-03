using Plugins.Countly.Services;
using Plugins.Countly.Services.Impls.Actual;

namespace Plugins.Countly
{
    public interface ICountly
    {
        IConsentCountlyService Consents { get; }
        ICrushReportsCountlyService CrushReports { get; }
        IDeviceIdCountlyService Device { get; }
        IEventCountlyService Events { get; }
        IInitializationCountlyService Initialization { get; }
        IOptionalParametersCountlyService OptionalParameters { get; }
        IRemoteConfigCountlyService RemoteConfigs { get; }
        IStarRatingCountlyService StarRating { get; }
        IUserDetailsCountlyService UserDetails { get; }
        IViewCountlyService Views { get; }
    }
}