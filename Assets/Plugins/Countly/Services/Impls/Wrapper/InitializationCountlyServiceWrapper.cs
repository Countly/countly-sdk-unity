using System.Threading.Tasks;
using Plugins.Countly.Helpers;
using Plugins.Countly.Models;
using UnityEngine;

namespace Plugins.Countly.Services.Impls.Wrapper
{
    public class InitializationCountlyServiceWrapper : IInitializationCountlyService
    {
        public string ServerUrl { get; private set; }
        public string AppKey { get; private set; }
        public void Begin(string serverUrl, string appKey)
        {
            Debug.Log("[InitializationCountlyServiceWrapper] Begin, serverUrl: " + serverUrl + ", appKey: " + appKey);
            ServerUrl = serverUrl;
            AppKey = appKey;
        }

        public Task<CountlyResponse> SetDefaults(CountlyConfigModel configModel)
        {
            Debug.Log("[InitializationCountlyServiceWrapper] SetDefaults, model: \n" + configModel);
            return Task.FromResult(new CountlyResponse());
        }

        public string GetBaseUrl()
        {
            Debug.Log("[InitializationCountlyServiceWrapper] GetBaseUrl");
            return string.Empty;
        }
    }
}