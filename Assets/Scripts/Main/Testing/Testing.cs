using Assets.Scripts.Enums;
using Assets.Scripts.Helpers;
using Assets.Scripts.Main.Development;
using Assets.Scripts.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

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
            Countly.Begin("Server_URL",
                            "YOUR_APP_KEY",
                            "YOUR_DEVICE_ID");
            var configObj = new CountlyConfigModel(null, false, false, false, false, 60, 100, 1000, 100, TestMode.AndroidTestToken, false);
            await Countly.SetDefaults(configObj);
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

        #endregion

        #region Optional Parameters

        public void SetLocationParameters()
        {
            Countly.SetLocation(34.9285, 138.6007);
            Countly.SetCity("Adelaide");
            Countly.SetCountryCode("AU");
        }

        public void DisableLocation()
        {
            Countly.DisableLocation();
        }

        #endregion

        #region Crash Reporting

        public void AddBreadcrumb(string value)
        {
            Countly.AddBreadcrumbs(value);
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
                await Countly.SendCrashReportAsync(ex.Message, ex.StackTrace, LogType.Exception, segments);
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

        public async void RecordEvents()
        {
            for (int count = 1; count <= 10; count++)
            {
                await Countly.RecordEventAsync($"Event{count}");
                await Task.Delay(count * 1000);
            }
            await Countly.RecordEventAsync("Game_Level_X_Started",
                new Dictionary<string, object>
                {
                    { "Time Spent", "1234455"},
                    { "Retry Attempts", "10"}
                });
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
                                new Dictionary<string, object>
                                {
                                    { "Hair", "Black" },
                                    { "Race", "Asian" },
                                });
            await userDetails.SetUserDetailsAsync();
        }

        public async void SetCustomUserDetails()
        {
            var userDetails = new CountlyUserDetailsModel(
                                new Dictionary<string, object>
                                {
                                    { "Nationality", "Indian" },
                                    { "Height", "5.10" },
                                    { "Mole", "Lower Left Cheek" },
                                });
            await userDetails.SetCustomUserDetailsAsync();
        }

        public void SetCustomUserData()
        {
            //Following are various examples
            //In a request, in case a single property is updated twice or more in a single request, the latest one overrides the preceeding ones.
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
