using UnityEngine;

namespace Assets.Scripts.Main.Development
{
    public class AppInitScript : MonoBehaviour
    {
        #region App Event Methods

        async void Start()
        {
            Countly.Begin("https://us-try.count.ly/",
                            "YOUR_APP_KEY",
                            "YOUR_DEVICE_ID");
            await Countly.SetDefaults("SALT_HERE", false, false, true, null);
        }

        async void OnApplicationQuit()
        {
#if UNITY_EDITOR
            if (Countly.IsInitialized)
            {
                await Countly.EndSessionAsync();
            }
#endif
        }

        async void OnApplicationPause(bool pause)
        {
            if (pause && Countly.IsInitialized)
            {
                await Countly.EndSessionAsync();
            }
        }

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