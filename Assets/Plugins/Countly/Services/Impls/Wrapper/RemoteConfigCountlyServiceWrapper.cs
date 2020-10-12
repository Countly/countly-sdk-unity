using System.Collections.Generic;
using System.Threading.Tasks;
using Plugins.Countly.Helpers;
using UnityEngine;

namespace Plugins.Countly.Services.Impls.Wrapper
{
    public class RemoteConfigCountlyServiceWrapper : IRemoteConfigCountlyService
    {
        public Dictionary<string, object> Configs { get; }
        
        public Task<CountlyResponse> InitConfig()
        {
            Debug.Log("[RemoteConfigCountlyServiceWrapper] InitConfig");
            return Task.FromResult(new CountlyResponse());
        }

        public Task<CountlyResponse> Update()
        {
            Debug.Log("[RemoteConfigCountlyServiceWrapper] Update");
            return Task.FromResult(new CountlyResponse());
        }
    }
}