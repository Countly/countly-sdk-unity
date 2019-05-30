using System.Collections.Generic;
using System.Threading.Tasks;
using Plugins.Countly.Helpers;
using UnityEngine;

namespace Plugins.Countly.Services
{
    public interface ICrushReportsCountlyService
    {
        /// <summary>
        /// Called when there is an exception 
        /// </summary>
        /// <param name="message">Exception Class</param>
        /// <param name="stackTrace">Stack Trace</param>
        /// <param name="type">Excpetion type like error, warning, etc</param>
        void LogCallback(string message, string stackTrace, LogType type);

        /// <summary>
        /// Private method that sends crash details to the server. Set param "nonfatal" to true for Custom Logged errors
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stackTrace"></param>
        /// <param name="type"></param>
        /// <param name="segments"></param>
        /// <param name="nonfatal"></param>
        /// <returns></returns>
        Task<CountlyResponse> SendCrashReportAsync(string message, string stackTrace, LogType type,
            IDictionary<string, object> segments = null, bool nonfatal = true);

        /// <summary>
        /// Sends custom logged errors to the server.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stackTrace"></param>
        /// <param name="type"></param>
        /// <param name="segments"></param>
        /// <returns></returns>
        Task<CountlyResponse> SendCrashReportAsync(string message, string stackTrace, LogType type,
            IDictionary<string, object> segments = null);

        /// <summary>
        /// Adds string value to a list which is later sent over as logs whenever a cash is reported by system.
        /// The length of a breadcrumb is limited to 1000 characters. Only first 1000 characters will be accepted in case the length is more 
        /// than 1000 characters.
        /// </summary>
        /// <param name="value"></param>
        void AddBreadcrumbs(string value);
    }
}