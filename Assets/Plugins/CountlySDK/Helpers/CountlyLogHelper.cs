using Plugins.CountlySDK.Models;

public class CountlyLogHelper
{
    private readonly CountlyConfiguration _configuration;
    internal CountlyLogHelper(CountlyConfiguration configuration)
    {
        _configuration = configuration;
    }

    internal void Info(string message)
    {
        if (_configuration.EnableConsoleLogging) {
            UnityEngine.Debug.Log("[Info][Countly]" + message);
        }
        
    }

    internal void Debug(string message)
    {
        if (_configuration.EnableConsoleLogging) {
            UnityEngine.Debug.Log("[Debug][Countly]" + message);
        }

    }

    internal void Verbose(string message)
    {
        if (_configuration.EnableConsoleLogging) {
            UnityEngine.Debug.Log("[Verbose][Countly]" + message);
        }

    }

    internal void Error(string message)
    {
        if (_configuration.EnableConsoleLogging) {
            UnityEngine.Debug.LogError("[Error][Countly]" + message);
        }
    }

    internal void Warning(string message)
    {
        if (_configuration.EnableConsoleLogging) {
            UnityEngine.Debug.LogWarning("[Warning][Countly]" + message);
        }
    }

}
