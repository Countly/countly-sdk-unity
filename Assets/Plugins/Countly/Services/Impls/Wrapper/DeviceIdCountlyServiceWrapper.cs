using System.Threading.Tasks;
using Plugins.Countly.Helpers;
using UnityEngine;

namespace Plugins.Countly.Services.Impls.Wrapper
{
    public class DeviceIdCountlyServiceWrapper : IDeviceIdCountlyService
    {
        public string DeviceId { get; }
        public void InitDeviceId(string deviceId = null)
        {
            Debug.Log("[DeviceIdCountlyServiceWrapper] init device id: " + deviceId);
        }

        public Task<CountlyResponse> ChangeDeviceIdAndEndCurrentSessionAsync(string deviceId)
        {
            Debug.Log("[DeviceIdCountlyServiceWrapper] ChangeDeviceIdAndEndCurrentSessionAsync, device id: " + deviceId);
            return Task.FromResult(new CountlyResponse());
        }

        public Task<CountlyResponse> ChangeDeviceIdAndMergeSessionDataAsync(string deviceId)
        {
            Debug.Log("[DeviceIdCountlyServiceWrapper] ChangeDeviceIdAndMergeSessionDataAsync, device id: " + deviceId);
            return Task.FromResult(new CountlyResponse());
        }

        public void UpdateDeviceId(string newDeviceId)
        {
            Debug.Log("[DeviceIdCountlyServiceWrapper] UpdateDeviceId, device id: " + newDeviceId);
        }
    }
}