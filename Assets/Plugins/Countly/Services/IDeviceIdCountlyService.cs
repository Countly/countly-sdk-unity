using System.Threading.Tasks;
using Plugins.Countly.Helpers;

namespace Plugins.Countly.Services
{
    public interface IDeviceIdCountlyService
    {
        string DeviceId { get; }
        void InitDeviceId(string deviceId = null);

        /// <summary>
        /// Changes Device Id.
        /// Adds currently recorded but not queued events to request queue.
        /// Clears all started timed-events
        /// Ends cuurent session with old Device Id.
        /// Begins a new session with new Device Id
        /// </summary>
        /// <param name="deviceId"></param>
        Task<CountlyResponse> ChangeDeviceIdAndEndCurrentSessionAsync(string deviceId);

        /// <summary>
        /// Changes DeviceId. 
        /// Continues with the current session.
        /// Merges data for old and new Device Id. 
        /// </summary>
        /// <param name="deviceId"></param>
        Task<CountlyResponse> ChangeDeviceIdAndMergeSessionDataAsync(string deviceId);

        /// <summary>
        /// Updates Device ID both in app and in cache
        /// </summary>
        /// <param name="newDeviceId"></param>
        void UpdateDeviceId(string newDeviceId);
    }
}