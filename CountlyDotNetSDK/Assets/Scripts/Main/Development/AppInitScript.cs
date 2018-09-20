using UnityEngine;
using Assets.Scripts.Helpers;

namespace Assets.Scripts.Main.Development
{
    public class AppInitScript : MonoBehaviour
    {
        #region Fields
        #endregion

        #region Properties

        private static Countly _instance { get; set; }
        public static Countly Instance => _instance ??
            (_instance = 
            new Countly("https://us-try.count.ly/",
                            Constants.AppKey,
                            Constants.DeviceID)
            );

        #endregion

        #region Event Methods

        // Use Start for initialization
        async void Start()
        {
            Instance.Initialize("1234567890", false, true);
            await Instance.BeginSessionAsync();

            //Testing Purpose
            Instance.EnablePush(Enums.TestMode.TestToken);
        }

        async void OnApplicationQuit()
        {
            await Instance.EndSessionAsync();
        }

        void OnApplicationPause(bool pause)
        {
            //Note that iOS applications are usually suspended and do not quit. 
            //You should tick "Exit on Suspend" in Player settings for iOS builds to cause the game to quit and not suspend
#if UNITY_IPHONE
        if (pause)
        {
            Instance.EndSession();
        }
#endif
        }

        // Whenever app is enabled
        void OnEnable()
        {
            //Application.logMessageReceived += LogCallback;
        }

        // Whenever app is disabled
        void OnDisable()
        {
            //Application.logMessageReceived -= LogCallback;
        }

        #endregion

        #region Methods

        public void LogCallback(string condition, string stackTrace, LogType type)
        {
            Instance.LogCallback(condition, stackTrace, type);
        }

        #endregion
    }
}