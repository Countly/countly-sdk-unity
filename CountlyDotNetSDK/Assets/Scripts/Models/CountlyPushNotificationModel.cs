using Assets.Scripts.Helpers;
using Assets.Scripts.Main.Development;
using Firebase;
using Firebase.Messaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

namespace Assets.Scripts.Models
{
    internal class CountlyPushNotificationModel
    {
        internal static CountlyPushNotificationModel CountlyPNInstance;
        internal static string Token = null;
        static CountlyPushNotificationModel()
        {
            CountlyPNInstance = new CountlyPushNotificationModel();
        }

        private CountlyPushNotificationModel() { }

        private async Task<DependencyStatus> InitializeFirebase()
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

        internal void EnablePushNotification()
        {
            //Attaching events
            FirebaseMessaging.TokenReceived += OnTokenReceived;
            FirebaseMessaging.MessageReceived += OnMessageReceived;
        }

        private async void OnTokenReceived(object sender, TokenReceivedEventArgs token)
        {
            //Save token for further use
            Token = token.Token;
            Countly.Message = Token;
            //Post to Countly
            await PostToCountly(2);
        }

        private async void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Countly.Message = e.Message.RawData;
                //string.Join(",", e.Message.Data.Select(x => $"{x.Key} {x.Value}"));
            if (e.Message != null && e.Message.Data != null && e.Message.Data.Count > 0)
            {
                int messageId;
                bool isSound;
                var data = e.Message.Data;
                int.TryParse(data.First(x => x.Key == Constants.MessageIDKey).Value, out messageId);
                var model = new
                {
                    MessageID = messageId > 0 ? messageId : new System.Random().Next(),
                    ImageUrl = data.First(x => x.Key == Constants.ImageUrlKey).Value,
                    Title = data.First(x => x.Key == Constants.TitleDataKey).Value,
                    Message = data.First(x => x.Key == Constants.MessageDataKey).Value,
                    Sound = bool.TryParse(data.First(x => x.Key == Constants.SoundDataKey).Value, out isSound)
                                ? isSound : false
                };

                //Push local notification
                //await Task.Run(() => NotificationHelper.SendNotification(model.MessageID, 0, model.Title, model.Message, model.Sound,
                //                    model.Sound, true, null, null, null, model.ImageUrl, new NotificationHelper.Action[] { }));
                //Debugging
                Countly.Message = "Notification Sent";

                //Notify to the Countly server
            }
        }

        internal async void EnablePushNotifications()
        {
            if (Countly.IsFirebaseReady)
                return;

            //Initialize Firebase
            var dependencyStatus = await CountlyPNInstance.InitializeFirebase();
            if (Countly.IsFirebaseReady)
            {
                //Register events
                CountlyPNInstance.EnablePushNotification();
            }
            else
            {
                throw new Exception($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        }

        /// <summary>
        /// Notifies Countly that the device is capable of receiving Push Notifications
        /// </summary>
        /// <returns></returns>
        private async Task<CountlyResponse> PostToCountly(int mode)
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
    }
}
