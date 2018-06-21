using Assets.Plugin.Scripts;
using UnityEngine;
using Helpers;

public class AppInitScript : MonoBehaviour
{
    #region Fields
    #endregion

    #region Properties

    private static Countly _instance { get; set; }
    public static Countly Instance => _instance ??
        (_instance = new Countly(
                            "https://us-try.count.ly/",
                            "73a6570ef97d4cf9174a6aeb97a38e1c3f88d6d9",
                            "b019e1b8-584b-413c-81f6-5b801519c9f1"));

    #endregion

    #region Event Methods

    // Use Start for initialization
    void Start()
    {
        Instance.Initialize(null, false, true);
        Instance.BeginSession();
    }

    void OnApplicationQuit()
    {
        Instance.EndSession();
    }

    void OnApplicationPause(bool pause)
    {
        //Note that iOS applications are usually suspended and do not quit. 
        //You should tick "Exit on Suspend" in Player settings for iOS builds to cause the game to quit and not suspend
        if (pause)
        {
            Instance.EndSession();
        }
    }

    // Whenever app is enabled
    void OnEnable()
    {
        Application.logMessageReceived += LogCallback;
    }

    // Whenever app is disabled
    void OnDisable()
    {
        Application.logMessageReceived -= LogCallback;
    }

    #endregion

    #region Methods

    public void LogCallback(string condition, string stackTrace, LogType type)
    {
        Instance.LogCallback(condition, stackTrace, type);
    }

    #endregion
}