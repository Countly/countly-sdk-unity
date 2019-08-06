using System.Threading.Tasks;
using Plugins.Countly.Helpers;

namespace Plugins.Countly.Services
{
    public interface IViewCountlyService
    {
        Task<CountlyResponse> RecordOpenViewAsync(string name, bool hasSessionBegunWithView = false);
        Task<CountlyResponse> RecordCloseViewAsync(string name, bool hasSessionBegunWithView = false);
    }
}