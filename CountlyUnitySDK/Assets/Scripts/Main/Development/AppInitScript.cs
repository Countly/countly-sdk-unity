using Assets.Scripts.Enums;
using Assets.Scripts.Models;
using UnityEngine;

namespace Assets.Scripts.Main.Development
{
    public class AppInitScript : MonoBehaviour
    {
        #region App Event Methods

        /// <summary>
        /// Initialize SDK at the start of your app
        /// </summary>
        async void Start()
        {
            //Countly.Begin("Server_URL",
            //                "YOUR_APP_KEY",
            //                "YOUR_DEVICE_ID");
            //var config = new CountlyConfigModel(....parameters);
            //await Countly.SetDefaults(config);
        }

        async void Update()
        {
            /*
             * When the push notification service gets enabled successfully for the device, 
             * we send a request to the Countly server that the user is ready to receive push notifications.
             * Update method is called multiple times during a particular scene,
             * therefore we send this request to the Countly server only once
             */
            if (CountlyPushNotificationModel.IsPushServiceReady)
            {
#if UNITY_IOS
                byte[] apnToken = NotificationServices.deviceToken;
                if (apnToken != null)
                {
                    CountlyPushNotificationModel.Token = System.BitConverter.ToString(apnToken).Replace("-", "");
                }
#endif
                CountlyPushNotificationModel.IsPushServiceReady = false;

                //Enabling the User to receive Push Notifications
                await CountlyPushNotificationModel.CountlyPNInstance.PostToCountlyAsync((int)CountlyPushNotificationModel.Mode);
            }
        }

        /// <summary>
        /// End session on application close/quit
        /// </summary>
        async void OnApplicationQuit()
        {
            if (Countly.IsSessionInitiated && !Countly.IsManualSessionHandlingEnabled)
            {
                await Countly.EndSessionAsync();
            }
        }

        // Whenever app is enabled
        void OnEnable()
        {
            if (Countly.IsSessionInitiated)
                Application.logMessageReceived += LogCallback;
        }

        // Whenever app is disabled
        void OnDisable()
        {
            if (Countly.IsSessionInitiated)
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