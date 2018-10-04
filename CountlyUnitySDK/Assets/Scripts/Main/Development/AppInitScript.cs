using Assets.Scripts.Enums;
using UnityEngine;

namespace Assets.Scripts.Main.Development
{
    public class AppInitScript : MonoBehaviour
    {
        #region App Event Methods

        /// <summary>
        /// Initialize SDK at the start of you app
        /// </summary>
        //async void Start()
        //{
        //    Countly.Begin("https://us-try.count.ly/",
        //                    "YOUR_APP_KEY",
        //                    "YOUR_DEVICE_ID");
        //    await Countly.SetDefaults(null, false, false, false, TestMode.TestToken);
        //}

        /// <summary>
        /// End session on application close/quit
        /// </summary>
        //async void OnApplicationQuit()
        //{
        //    if (Countly.IsInitialized)
        //    {
        //        await Countly.EndSessionAsync();
        //    }
        //}

        // Whenever app is enabled
        void OnEnable()
        {
            if (Countly.IsInitialized)
                Application.logMessageReceived += LogCallback;
        }

        // Whenever app is disabled
        void OnDisable()
        {
            if (Countly.IsInitialized)
                Application.logMessageReceived -= LogCallback;
        }

        #endregion

        #region Methods

        public void LogCallback(string condition, string stackTrace, LogType type)
        {
            Countly.LogCallback(condition, stackTrace, type);
        }

        #endregion
    }
}