using System.Threading.Tasks;
using Plugins.Countly.Helpers;
using Plugins.Countly.Services.Impls.Actual;
using UnityEngine;

namespace Plugins.Countly.Services.Impls.Wrapper
{
    public class ViewCountlyServiceWrapper : IViewCountlyService
    {
        public Task<CountlyResponse> RecordOpenViewAsync(string name, bool hasSessionBegunWithView = false)
        {
            Debug.Log("[ViewCountlyServiceWrapper] ReportOpenViewAsync, name: " + name + ", hasSessionBegunWithView: " + hasSessionBegunWithView);
            return Task.FromResult(new CountlyResponse());
        }

        public Task<CountlyResponse> RecordCloseViewAsync(string name, bool hasSessionBegunWithView = false)
        {
            Debug.Log("[ViewCountlyServiceWrapper] ReportCloseViewAsync, name: " + name + ", hasSessionBegunWithView: " + hasSessionBegunWithView);
            return Task.FromResult(new CountlyResponse());
        }
    }
}