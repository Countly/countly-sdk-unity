using UnityEngine;

namespace Assets.Scripts.Main.Development
{
    public class AppInitScript : MonoBehaviour
    {
        #region Fields
        #endregion

        #region Properties

        private static Countly _instance { get; set; }
        public static Countly Instance => _instance ??
            (_instance = new Countly(
                                "https://us-try.count.ly/",
                                "[APP_KEY]",
                                "[DEVICE_ID]"));

        #endregion

        #region Event Methods

        // Use Start for initialization
        async void Start()
        {
            Instance.Initialize("[SALT]", false, true);
            await Instance.BeginSession();
        }

        async void OnApplicationQuit()
        {
            await Instance.EndSession();
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
}