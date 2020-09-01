using System.Collections.Generic;
using System.Threading.Tasks;
using Plugins.Countly.Helpers;
using UnityEngine;

namespace Plugins.Countly.Services.Impls.Wrapper
{
    public class CrashReportsCountlyServiceWrapper : ICrashReportsCountlyService
    {
        public void LogCallback(string message, string stackTrace, LogType type)
        {
            
        }

        public Task<CountlyResponse> SendCrashReportAsync(string message, string stackTrace, LogType type, IDictionary<string, object> segments = null,
            bool nonfatal = true)
        {
            Debug.Log("[CrashReportsCountlyServiceWrapper] Send crash report async, message: \n" + message + "\n" +
                      stackTrace + "\n " + type);
            return Task.FromResult(new CountlyResponse());
        }

        public Task<CountlyResponse> SendCrashReportAsync(string message, string stackTrace, LogType type, IDictionary<string, object> segments = null)
        {
            Debug.Log("[CrashReportsCountlyServiceWrapper] Send crash report async, message: \n" + message + "\n" +
                      stackTrace + "\n " + type);
            return Task.FromResult(new CountlyResponse());
        }

        public void AddBreadcrumbs(string value)
        {
            Debug.Log("[CrashReportsCountlyServiceWrapper] Add breadcrumbs, value: " + value);
        }
    }
}