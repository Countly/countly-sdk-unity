using System;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Models;

public class CountlyLogHelper
{
    private const string TAG = "[Countly]";
    private readonly CountlyConfiguration _configuration;
    internal CountlyLogHelper(CountlyConfiguration configuration)
    {
        _configuration = configuration;
    }
    internal void Info(string message)
    {
        if (_configuration.IsLoggingEnabled()) {
            UnityEngine.Debug.Log("[Info]" + TAG + message);
        }
        NotifyListener(message, LogLevel.Info);
    }
    internal void Debug(string message)
    {
        if (_configuration.IsLoggingEnabled()) {
            UnityEngine.Debug.Log("[Debug]" + TAG + message);
        }
        NotifyListener(message, LogLevel.Debug);
    }
    internal void Verbose(string message)
    {
        if (_configuration.IsLoggingEnabled()) {
            UnityEngine.Debug.Log("[Verbose]" + TAG + message);
        }
        NotifyListener(message, LogLevel.Verbose);
    }
    internal void Error(string message)
    {
        if (_configuration.IsLoggingEnabled()) {
            UnityEngine.Debug.LogError("[Error]" + TAG + message);
        }
        NotifyListener(message, LogLevel.Error);
    }
    internal void Warning(string message)
    {
        if (_configuration.IsLoggingEnabled()) {
            UnityEngine.Debug.LogWarning("[Warning]" + TAG + message);
        }
        NotifyListener(message, LogLevel.Warning);
    }

    /// <summary>
    /// Passes a log message to the log listener, if one is set. A listener that throws does not break the SDK.
    /// </summary>
    /// <param name="message">The log message, without its level</param>
    /// <param name="level">The level of the message</param>
    private void NotifyListener(string message, LogLevel level)
    {
        try {
            _configuration.LogListener?.Invoke(TAG + message, level);
        } catch (Exception ex) {
            if (_configuration.IsLoggingEnabled()) {
                UnityEngine.Debug.LogError("[Error]" + TAG + "[CountlyLogHelper] NotifyListener, the log listener threw an exception: " + ex);
            }
        }
    }
}
