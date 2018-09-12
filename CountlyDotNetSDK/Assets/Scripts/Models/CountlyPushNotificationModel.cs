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

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Countly.Message = string.Join(",", e.Message.Data.Select(x => $"{x.Key} {x.Value}"));
            //Post to Countly
            Debug.Log("Received a new message from: " + e.Message.Data);
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
        private Task<CountlyResponse> PostToCountly(int mode)
        {
            var requestParams =
               new Dictionary<string, object>
               {
                    { "token_session", 1 },
                    { "test_mode", mode },
                    { $"{Application.platform.ToString().ToLower()}_token", Token },
               };
            return CountlyHelper.GetResponseAsync(requestParams);
        }
    }
}
