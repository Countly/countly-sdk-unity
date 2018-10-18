using Assets.Scripts.Enums;
using Assets.Scripts.Models;
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
            //Countly.Begin("https://us-try.count.ly/",
            //                "YOUR_APP_KEY",
            //                "YOUR_DEVICE_ID");
            //var config = new CountlyConfigModel(null, false, false, false, TestMode.TestToken);
            //await Countly.SetDefaults(config);
        //}

        async void Update()
        {
            if (CountlyPushNotificationModel.IsPushServiceReady || CountlyPushNotificationModel.IsFirebaseReady)
            {
#if UNITY_IOS
                byte[] apnToken = NotificationServices.deviceToken;
                if (apnToken != null)
                {
                    CountlyPushNotificationModel.Token = System.BitConverter.ToString(apnToken).Replace("-", "");
                }
#endif
                CountlyPushNotificationModel.IsPushServiceReady = false;

                //Post to Countly
                await CountlyPushNotificationModel.CountlyPNInstance.PostToCountlyAsync((int)CountlyPushNotificationModel.Mode);
            }
        }

        /// <summary>
        /// End session on application close/quit
        /// </summary>
        async void OnApplicationQuit()
        {
            if (Countly.IsInitialized)
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