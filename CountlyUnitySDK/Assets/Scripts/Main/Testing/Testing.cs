using Assets.Scripts.Enums;
using Assets.Scripts.Helpers;
using Assets.Scripts.Main.Development;
using Assets.Scripts.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Main.Testing
{
    /// <summary>
    /// This class contains samples to invoke SDK Features and involves sample data only.
    /// </summary>
    public class Testing : MonoBehaviour
    {
        #region Initialization
        public async void InitializeCountlySDk()
        {
            Countly.Begin("https://us-try.count.ly/",
                            "YOUR_APP_KEY",
                            "YOUR_DEVICE_ID");
            await Countly.SetDefaults(null, false, false, false, TestMode.TestToken);
        }

        #endregion

        #region Manual Session Handling

        public async void StartSession()
        {
            await Countly.BeginSessionAsync();
        }

        public async void ExtendSession()
        {
            await Countly.ExtendSessionAsync();
        }

        public async void EndSession()
        {
            await Countly.EndSessionAsync();
        }

        public void SetSessionDuration()
        {
            Countly.SetSessionDuration(30);
        }

        #endregion

        #region Optional Parameters

        public void SetLocationParameters()
        {
            Countly.SetLocation(34.9285, 138.6007);
            Countly.SetCity("Adelaide");
            Countly.SetCountryCode("AU");
        }

        #endregion

        #region Crash Reporting

        public void AddBreadcrumb()
        {
            Countly.AddBreadcrumbs("SomeValueToBeSentWithCrashReport");
        }

        public void SendAutoCrashReport()
        {
            Countly.AddBreadcrumbs("Value1");
            Countly.AddBreadcrumbs("Value2");
            int x = 0, y = 0, z;
            z = x / y;
        }

        public async void SendManualCrashReport()
        {
            var segments = new Dictionary<string, object>
            {
                { "Key1", "Value1" },
                { "Key2", "Value2" }
            };
            Countly.AddBreadcrumbs("Value1");
            Countly.AddBreadcrumbs("Value2");
            try
            {
                int x = 0, y = 0, z;
                z = x / y;
            }
            catch (Exception ex)
            {
                await Countly.SendCrashReportAsync(ex.Message, ex.StackTrace, LogType.Exception,
                JsonConvert.SerializeObject(segments));
            }
        }

        // Whenever app is enabled
        void OnEnable()
        {
            Application.logMessageReceived += LogCallback;
        }

        //Whenever app is disabled
        void OnDisable()
        {
            Application.logMessageReceived -= LogCallback;
        }

        public void LogCallback(string condition, string stackTrace, LogType type)
        {
            Countly.LogCallback(condition, stackTrace, type);
        }

        #endregion

        #region Events

        public async void ReportCustomEvent()
        {
            var segment = new Dictionary<string, object>
            {
                { "Key1", "Value1" },
                { "Key2", "Value2" }
            };
            await Countly.ReportCustomEventAsync("Click");
        }

        public void SetEventSendThreshold(int threshold)
        {
            Countly.SetEventSendThreshold(threshold);
        }

        public async void StartEvent()
        {
            for (int count = 1; count <= 10; count++)
            {
                Countly.StartEvent($"Event{count}");
                await Task.Delay(count * 1000);
            }
        }

        public async void EndEvent()
        {
            for (int count = 1; count <= 10; count++)
            {
                await Countly.EndEventAsync($"Event{count}",
                    JsonConvert.SerializeObject(
                            new Dictionary<string, object>
                            { { $"Key{count}", $"Value{count}"} }), 1, 10);
            }
        }

        public async void EndEvent(string key, string segment, int count, double sum)
        {
            await Countly.EndEventAsync(key, segment, count, sum);
        }

        public async void ReportMultipleEvents()
        {
            var events = new List<CountlyEventModel>
            {
                new CountlyEventModel("Click",
                        JsonConvert.SerializeObject(
                            new Dictionary<string, object>
                            { { "Key1", "Value1"} } )),
                new CountlyEventModel("Hover",
                        JsonConvert.SerializeObject(
                            new Dictionary<string, object>
                            { { "Key2", "Value2"} } )),
            };
            await Countly.ReportMultipleEventsAsync(events);
        }
        #endregion

        #region Views

        public async void ReportView()
        {
            await Countly.ReportViewAsync("LoginScreen", true);
            await Countly.ReportViewAsync("MainDashboard");
        }

        #endregion

        #region View Actions

        public async void ReportViewAction()
        {
            await Countly.ReportActionAsync("Touch", 0, 0, 50, 50);
        }

        #endregion

        #region Star Rating

        public async void ReportStarRating()
        {
            await Countly.ReportStarRatingAsync("android", "0.1", 3);
        }

        #endregion

        #region User Details

        public async void SetUserDetails()
        {
            var userDetails = new CountlyUserDetailsModel(
                                "Full Name", "username", "test@email.com", "Organization",
                                "222-222-222", "https://www.teacherspocketbooks.co.uk/wp-content/uploads/sites/2/2015/04/tp-computer-icon.png", "M", "1986",
                                JsonConvert.SerializeObject(new Dictionary<string, object>
                                {
                                    { "Hair", "Black" },
                                    { "Race", "Asian" },
                                }));
            await userDetails.SetUserDetailsAsync();
        }

        public async void SetCustomUserDetails()
        {
            var userDetails = new CountlyUserDetailsModel(
                                null, null, null, null,
                                null, null, null, null,
                                JsonConvert.SerializeObject(new Dictionary<string, object>
                                {
                                    { "Nationality", "Indian" },
                                    { "Height", "5.10" },
                                    { "Mole", "Lower Left Cheek" },
                                }));
            await userDetails.SetCustomUserDetailsAsync();
        }

        public void SetCustomUserData()
        {
            //Following are various examples
            //In a request, there cannot be more than one update request for a single key.
            //Ex: SetOnce and Increment cannot be clubbed together in one request because both are updating property "weight".
            //However, SetOnce and IncrementBy can be clubbed together.
            CountlyUserDetailsModel.SetOnce("Weight", "80");
            CountlyUserDetailsModel.Increment("Weight");
            CountlyUserDetailsModel.IncrementBy("Height", 1);
            CountlyUserDetailsModel.Multiply("Weight", 2);
            CountlyUserDetailsModel.Max("Weight", 190);
            CountlyUserDetailsModel.Min("Height", 5.5);
            CountlyUserDetailsModel.Push("Mole", new string[] { "Left Cheek", "Back", "Toe", "Back of the Neck", "Back" });
            CountlyUserDetailsModel.PushUnique("Mole", new string[] { "Right & Leg", "Right Leg", "Right Leg" });
            CountlyUserDetailsModel.Pull("Mole", new string[] { "Right & Leg", "Back" });
        }

        public async void SaveCustomUserData()
        {
            await CountlyUserDetailsModel.SaveAsync();
        }

        #endregion

        #region Push Notifications

        public async void SendLocalNotification()
        {
            await NotificationHelper.SendNotificationAsync(1, new TimeSpan(0, 0, 5), "Hey!", "How are you doing?",
                    true, true,
                    "https://images.pexels.com/photos/257840/pexels-photo-257840.jpeg?auto=compress&cs=tinysrgb&h=350",
                    new NotificationHelper.Action[] { });
        }

        #endregion

        #region Device ID

        public async void ChangeDeviceIDAndEndCurrentSessionAsync()
        {
            await Countly.ChangeDeviceIDAndEndCurrentSessionAsync("YOUR_NEW_DEVICE_ID");
        }

        public async void ChangeDeviceIDAndMergeSessionDataAsync()
        {
            await Countly.ChangeDeviceIDAndMergeSessionDataAsync("YOUR_NEW_DEVICE_ID");
        }

        #endregion
    }
}
