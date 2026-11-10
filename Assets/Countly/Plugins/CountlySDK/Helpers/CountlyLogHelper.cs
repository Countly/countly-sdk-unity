using System;
using System.Collections.Generic;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Services;

public class CountlyLogHelper
{
    private const string TAG = "[Countly]";
    private readonly CountlyConfiguration _configuration;
    private readonly object _healthCheckLock = new object();
    private HealthCheckCountlyService _healthCheck;
    private long _warningsBeforeHealthCheck;
    private long _errorsBeforeHealthCheck;
    private readonly Dictionary<string, int> _logCodesBeforeHealthCheck = new Dictionary<string, int>();

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
        CountError();
        if (_configuration.IsLoggingEnabled()) {
            UnityEngine.Debug.LogError("[Error]" + TAG + message);
        }
        NotifyListener(message, LogLevel.Error);
    }
    internal void Warning(string message)
    {
        CountWarning();
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

    /// <summary>
    /// Hands the health check counters to the logger, with the warnings, errors and log codes counted before they existed.
    /// </summary>
    internal void SetHealthCheck(HealthCheckCountlyService healthCheck)
    {
        long warnings;
        long errors;
        Dictionary<string, int> codes;
        lock (_healthCheckLock) {
            _healthCheck = healthCheck;
            if (healthCheck == null) {
                return;
            }
            warnings = _warningsBeforeHealthCheck;
            errors = _errorsBeforeHealthCheck;
            codes = new Dictionary<string, int>(_logCodesBeforeHealthCheck);
            _warningsBeforeHealthCheck = 0;
            _errorsBeforeHealthCheck = 0;
            _logCodesBeforeHealthCheck.Clear();
        }

        for (long a = 0; a < warnings; a++) {
            healthCheck.LogWarning();
        }
        for (long a = 0; a < errors; a++) {
            healthCheck.LogError();
        }
        foreach (KeyValuePair<string, int> code in codes) {
            healthCheck.RecordLogCode(code.Key, code.Value);
        }
    }

    /// <summary>
    /// The health check counters, null before they exist.
    /// </summary>
    internal HealthCheckCountlyService HealthCheck
    {
        get {
            lock (_healthCheckLock) {
                return _healthCheck;
            }
        }
    }

    /// <summary>
    /// Records a health check log code, such as "w740", next to the log line of its condition. A code recorded before
    /// the counters exist is kept until they do.
    /// </summary>
    internal void RecordLogCode(string code, int count = 1)
    {
        HealthCheckCountlyService healthCheck;
        lock (_healthCheckLock) {
            healthCheck = _healthCheck;
            if (healthCheck == null) {
                _logCodesBeforeHealthCheck.TryGetValue(code, out int current);
                _logCodesBeforeHealthCheck[code] = current + count;
                return;
            }
        }
        healthCheck.RecordLogCode(code, count);
    }

    /// <summary>
    /// Records a health check log code that a public method of a feature area records, ignored while that feature has
    /// no consent.
    /// </summary>
    internal void RecordLogCode(string code, string area)
    {
        HealthCheck?.RecordLogCode(code, area);
    }

    /// <summary>
    /// Counts one call of a method for the health check method usage.
    /// </summary>
    internal void RecordUsage(string area, string method)
    {
        HealthCheck?.RecordUsage(area, method);
    }

    private void CountWarning()
    {
        HealthCheckCountlyService healthCheck;
        lock (_healthCheckLock) {
            healthCheck = _healthCheck;
            if (healthCheck == null) {
                _warningsBeforeHealthCheck++;
                return;
            }
        }
        healthCheck.LogWarning();
    }

    private void CountError()
    {
        HealthCheckCountlyService healthCheck;
        lock (_healthCheckLock) {
            healthCheck = _healthCheck;
            if (healthCheck == null) {
                _errorsBeforeHealthCheck++;
                return;
            }
        }
        healthCheck.LogError();
    }
}
