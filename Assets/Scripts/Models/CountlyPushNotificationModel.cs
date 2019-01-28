#region Usings

using Assets.Scripts.Enums;
using Assets.Scripts.Helpers;
using Assets.Scripts.Main.Development;
using Firebase;
using Firebase.Messaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#if UNITY_IOS
using NotificationServices = UnityEngine.iOS.NotificationServices;
using NotificationType = UnityEngine.iOS.NotificationType;
#endif

#endregion

namespace Assets.Scripts.Models
{
    [Serializable]
    internal class CountlyPushNotificationModel
    {
        internal static CountlyPushNotificationModel CountlyPNInstance;
        internal static string Token = null;
        internal static TestMode Mode;
        internal static bool IsPushServiceReady = false;
        internal static bool IsFirebaseReady { get; set; }

        static CountlyPushNotificationModel()
        {
            CountlyPNInstance = new CountlyPushNotificationModel();
        }

        private CountlyPushNotificationModel() { }

        /// <summary>
        /// Initializes Firebase Cloud Messaging
        /// </summary>
        /// <returns></returns>
        private async Task<DependencyStatus> InitializeFirebaseAsync()
        {
            DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
            await FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    // Create and hold a reference to your FirebaseApp, i.e.
                    //  app = Firebase.FirebaseApp.DefaultInstance;
                    // where app is a Firebase.FirebaseApp property of your application class.
                    //FirebaseAppInstance = FirebaseApp.DefaultInstance;

                    // Set a flag here indicating that Firebase is ready to use by your application.
                    IsFirebaseReady = true;
                    return dependencyStatus;
                }
                else
                {
                    // Firebase Unity SDK is not safe to use here.
                    return dependencyStatus;
                }
            });
            return dependencyStatus;
        }

        /// <summary>
        /// Registers Firebase messaging events
        /// </summary>
        /// <param name="mode"></param>
        internal void RegisterEvents()
        {
            //Attaching events
            FirebaseMessaging.TokenReceived += OnTokenReceived;
            FirebaseMessaging.MessageReceived += OnMessageReceived;
        }

        /// <summary>
        /// Registers device for receiving Push Notifications
        /// </summary>
        /// <param name="mode">Application mode</param>
        internal async void EnablePushNotificationAsync(TestMode mode)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (IsFirebaseReady)
                return;

            //Initialize Firebase
            var dependencyStatus = await CountlyPNInstance.InitializeFirebaseAsync();
            if (IsFirebaseReady)
            {
                Mode = mode;

                //Register events
                CountlyPNInstance.RegisterEvents();
            }
            else
            {
                throw new Exception($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
#endif
#if UNITY_IOS && !UNITY_EDITOR
            NotificationServices.RegisterForNotifications(
                NotificationType.Alert
                | NotificationType.Badge
                | NotificationType.Sound);

            Mode = mode;
            IsPushServiceReady = true;
#endif

        }

        /// <summary>
        /// Fired when the app receives token from Firebase during registration
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="token"></param>
        public void OnTokenReceived(object sender, TokenReceivedEventArgs token)
        {
            //Save token for further use
#if UNITY_ANDROID
            Token = token.Token;
            IsPushServiceReady = true;
#endif
        }

        /// <summary>
        /// Fired when app receives a message (Push Notification) from Countly.
        /// Returns notification id
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message != null && e.Message.Data != null && e.Message.Data.Count > 0)
            {
                int messageId;
                bool isSound;
                var data = e.Message.Data;
                var model = new
                {
                    MessageID = int.TryParse(GetValue(data, Constants.MessageIDKey), out messageId)
                                ? messageId : new Random().Next(),
                    ImageUrl = GetValue(data, Constants.ImageUrlKey),
                    Title = GetValue(data, Constants.TitleDataKey),
                    Message = GetValue(data, Constants.MessageDataKey),
                    Sound = bool.TryParse(GetValue(data, Constants.SoundDataKey), out isSound)
                                ? isSound : false,
                    //Get Notification actions *********************************************
                    //NotificationActions = 
                };

                //Push local notification
                await NotificationHelper.SendNotificationAsync(model.MessageID, new TimeSpan(0, 0, 5), model.Title, model.Message,
                    true, true, model.ImageUrl,
                    new NotificationHelper.Action[]
                    {
                        //Bind Notification actions here
                        //Not supported in release version 1
                    });
            }
        }

        //public void OnAction(string handlerMethod)
        //{
        //    var data = JsonConvert.DeserializeObject<PayLoad>(payload);
        //    CountlyHelper.InvokeMethod($"{data.ID}_Click", new object[] { data.Data });
        //    
        //    //Report action to Countly server              
        //    await ReportPushAction("Btn_Ok", "1");
        //}

        /// <summary>
        /// Notifies Countly that the device is capable of receiving Push Notifications
        /// </summary>
        /// <returns></returns>
        internal async Task<CountlyResponse> PostToCountlyAsync(int mode)
        {
            var requestParams =
               new Dictionary<string, object>
               {
                    { "token_session", 1 },
                    { "test_mode", mode },
                    { $"{Constants.UnityPlatform}_token", Token },
               };
            return await CountlyHelper.GetResponseAsync(requestParams);
        }

        private string GetValue(IDictionary<string, string> source, string key)
        {
            string value;
            return source.TryGetValue(key, out value) ? value : null;
        }

        #region Unused Code

        /// <summary>
        /// Reports user action on Push Notification to the Countly Server
        /// </summary>
        /// <param name="messageID"></param>
        /// <param name="index">Button Index</param>
        /// <returns></returns>
        //private async Task<CountlyResponse> ReportPushAction(string messageID, string index)
        //{
        //    var segment =
        //        new PushActionSegment
        //        {
        //            Identifier = index,
        //            MessageID = messageID
        //        };

        //    var action = new CountlyEventModel(CountlyEventModel.PushActionEvent,
        //                    JsonConvert.SerializeObject(segment, Formatting.Indented), null);

        //    return await action.ReportCustomEvent();
        //}

        //public async Task<int> SendLocalNotification()
        //{
        //    return await NotificationHelper.SendNotificationAsync(1, new TimeSpan(0, 0, 5), "Hey!", "How are you doing?",
        //            true, true,
        //            "https://images.pexels.com/photos/257840/pexels-photo-257840.jpeg?auto=compress&cs=tinysrgb&h=350",
        //            new NotificationHelper.Action[] { });
        //}

        #endregion
    }
}