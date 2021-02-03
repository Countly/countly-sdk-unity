using Plugins.CountlySDK.Models;
using UnityEngine;

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
            Debug.Log(message);
        }
        
    }

    internal void Error(string message)
    {
        if (_configuration.EnableConsoleLogging) {
            Debug.LogError(message);
        }
    }

    internal void Warning(string message)
    {
        if (_configuration.EnableConsoleLogging) {
            Debug.LogWarning(message);
        }
    }

}
