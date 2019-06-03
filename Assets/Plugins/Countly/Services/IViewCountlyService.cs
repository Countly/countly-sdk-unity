using System.Threading.Tasks;
using Plugins.Countly.Helpers;

namespace Plugins.Countly.Services.Impls.Actual
{
    public interface IViewCountlyService
    {
        Task<CountlyResponse> ReportOpenViewAsync(string name, bool hasSessionBegunWithView = false);
        Task<CountlyResponse> ReportCloseViewAsync(string name, bool hasSessionBegunWithView = false);
    }
}