using Assets.Scripts.Enums;
using Assets.Scripts.Helpers;
using Assets.Scripts.Main.Development;
using Firebase;
using Firebase.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

namespace Assets.Scripts.Models
{
    [Serializable]
    internal class CountlyPushNotificationModel : MonoBehaviour
    {
        internal static CountlyPushNotificationModel CountlyPNInstance;
        internal static string Token = null;
        private static TestMode _mode;

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
                    Countly.FirebaseAppInstance = FirebaseApp.DefaultInstance;

                    // Set a flag here indicating that Firebase is ready to use by your application.
                    Countly.IsFirebaseReady = true;
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
        internal void RegisterEvents(TestMode mode)
        {
            _mode = mode;
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
            if (Countly.IsFirebaseReady)
                return;

            //Initialize Firebase
            var dependencyStatus = await CountlyPNInstance.InitializeFirebaseAsync();
            if (Countly.IsFirebaseReady)
            {
                //Register events
                CountlyPNInstance.RegisterEvents(mode);
            }
            else
            {
                throw new Exception($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        }

        /// <summary>
        /// Fired when the app receives token from Firebase during registration
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="token"></param>
        public async void OnTokenReceived(object sender, TokenReceivedEventArgs token)
        {
            //Save token for further use
            Token = token.Token;
            //Post to Countly
            await PostToCountlyAsync((int)_mode);
        }

        /// <summary>
        /// Fired when app receives a message (Push Notification) from Countly
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
                                ? messageId : new System.Random().Next(),
                    ImageUrl = GetValue(data, Constants.ImageUrlKey),
                    Title = GetValue(data, Constants.TitleDataKey),
                    Message = GetValue(data, Constants.MessageDataKey),
                    Sound = bool.TryParse(GetValue(data, Constants.SoundDataKey), out isSound)
                                ? isSound : false,
                    //Get Notification actions *********************************************
                    //NotificationActions = 
                };

                //Push local notification
                await NotificationHelper.SendNotificationAsync(model.MessageID, new TimeSpan(0, 0, 5), model.Title, model.Message, model.Sound,
                                    model.Sound, true, null, null, null, model.ImageUrl,
                                    new NotificationHelper.Action[]
                                    {
                                        //Bind Notification actions here
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
        private async Task<CountlyResponse> PostToCountlyAsync(int mode)
        {
            var requestParams =
               new Dictionary<string, object>
               {
                    { "token_session", 1 },
                    { "test_mode", mode },
                    { $"{Application.platform.ToString().ToLower()}_token", Token },
               };
            return await CountlyHelper.GetResponseAsync(requestParams);
        }

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

        private string GetValue(IDictionary<string, string> source, string key)
        {
            string value;
            return source.TryGetValue(key, out value) ? value : null;
        }
    }
}
