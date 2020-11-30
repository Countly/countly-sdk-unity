using System;
using Plugins.CountlySDK.Enums;
using UnityEngine;

namespace Plugins.CountlySDK.Models
{
    [Serializable]
    public class CountlyConfiguration
    {
        public string ServerUrl = null;
        public string AppKey = null;
        public string DeviceId = null;

        public string Salt = null;
        public bool EnableFirstAppLaunchSegment = false;
        public bool EnablePost = false;
        public bool EnableTestMode = false;
        public bool EnableConsoleLogging = false;
        public bool IgnoreSessionCooldown = false;
        public TestMode NotificationMode = TestMode.None;
        public readonly bool EnableManualSessionHandling = false;
        public int SessionDuration = 60;
        public int EventQueueThreshold = 100;
        public int StoredRequestLimit = 1000;
        public int TotalBreadcrumbsAllowed = 100;
        public bool EnableAutomaticCrashReporting = true;

        /// <summary>
        ///     Parent must be undestroyable
        /// </summary>
        public GameObject Parent = null;

        public CountlyConfiguration() { }

        internal CountlyConfiguration(CountlyAuthModel authModel, CountlyConfigModel config)
        {
            ServerUrl = authModel.ServerUrl;
            AppKey = authModel.AppKey;
            DeviceId = authModel.DeviceId;

            Salt = config.Salt;
            EnablePost = config.EnablePost;
            EnableManualSessionHandling = config.EnableManualSessionHandling;
            EnableFirstAppLaunchSegment = config.EnableFirstAppLaunchSegment;
            EnableTestMode = config.EnableTestMode;
            EnableConsoleLogging = config.EnableConsoleLogging;
            IgnoreSessionCooldown = config.IgnoreSessionCooldown;
            NotificationMode = config.NotificationMode;
            SessionDuration = config.SessionDuration;
            EventQueueThreshold = config.EventQueueThreshold;
            StoredRequestLimit = config.StoredRequestLimit;
            TotalBreadcrumbsAllowed = config.TotalBreadcrumbsAllowed;
            EnableAutomaticCrashReporting = config.EnableAutomaticCrashReporting;

        }

        public override string ToString()
        {
            return $"{nameof(Salt)}: {Salt}, {nameof(EnablePost)}: {EnablePost}, {nameof(EnableConsoleLogging)}: {EnableConsoleLogging}, {nameof(IgnoreSessionCooldown)}: {IgnoreSessionCooldown}, {nameof(NotificationMode)}: {NotificationMode}, {nameof(EnableManualSessionHandling)}: {EnableManualSessionHandling}, {nameof(SessionDuration)}: {SessionDuration}, {nameof(EventQueueThreshold)}: {EventQueueThreshold}, {nameof(StoredRequestLimit)}: {StoredRequestLimit}, {nameof(TotalBreadcrumbsAllowed)}: {TotalBreadcrumbsAllowed}, {nameof(EnableAutomaticCrashReporting)}: {EnableAutomaticCrashReporting}";
        }
    }
}