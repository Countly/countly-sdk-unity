using System;
using Plugins.Countly.Enums;

namespace Plugins.Countly.Models
{
    [Serializable]
    public class CountlyConfigModel
    {
        public string Salt;
        public bool EnablePost;
        public bool EnableConsoleErrorLogging;
        public bool IgnoreSessionCooldown;
        public TestMode NotificationMode;
        public bool EnableManualSessionHandling;
        public int SessionDuration;
        public int EventViewSendThreshold;
        public int EventNonViewSendThreshold;
        public int TotalBreadcrumbsAllowed;
        public bool EnableAutomaticCrashReporting;

        /// <summary>
        /// Initializes the SDK configurations
        /// </summary>
        /// <param name="salt"></param>
        /// <param name="enablePost"></param>
        /// <param name="enableConsoleErrorLogging"></param>
        /// <param name="ignoreSessionCooldown"></param>
        /// <param name="enableManualSessionHandling"></param>
        /// <param name="sessionDuration">Session is updated after each interval passed.
        /// This interval is also used to process request queue. The interval must be in seconds</param>
        /// <param name="notificationMode"></param>
        public CountlyConfigModel(string salt, bool enablePost = false, bool enableConsoleErrorLogging = false,
                                    bool ignoreSessionCooldown = false, bool enableManualSessionHandling = false,
                                    int sessionDuration = 60, int eventViewThreshold = 100, int totalBreadcrumbsAllowed = 100,
                                    TestMode notificationMode = TestMode.None, bool enableAutomaticCrashReporting = true)

        {
            this.Salt = salt;
            EnablePost = enablePost;
            EnableConsoleErrorLogging = enableConsoleErrorLogging;
            IgnoreSessionCooldown = ignoreSessionCooldown;
            NotificationMode = notificationMode;
            SessionDuration = sessionDuration;
            EnableManualSessionHandling = enableManualSessionHandling;
            EventViewSendThreshold = eventViewThreshold;
            TotalBreadcrumbsAllowed = totalBreadcrumbsAllowed;
            EnableAutomaticCrashReporting = enableAutomaticCrashReporting;
        }

        public override string ToString()
        {
            return $"{nameof(Salt)}: {Salt}, {nameof(EnablePost)}: {EnablePost}, {nameof(EnableConsoleErrorLogging)}: {EnableConsoleErrorLogging}, {nameof(IgnoreSessionCooldown)}: {IgnoreSessionCooldown}, {nameof(NotificationMode)}: {NotificationMode}, {nameof(EnableManualSessionHandling)}: {EnableManualSessionHandling}, {nameof(SessionDuration)}: {SessionDuration}, {nameof(EventViewSendThreshold)}: {EventViewSendThreshold}, {nameof(EventNonViewSendThreshold)}: {EventNonViewSendThreshold}, {nameof(TotalBreadcrumbsAllowed)}: {TotalBreadcrumbsAllowed}, {nameof(EnableAutomaticCrashReporting)}: {EnableAutomaticCrashReporting}";
        }
    }
}
