using Assets.Scripts.Enums;

namespace Assets.Scripts.Models
{
    public class CountlyConfigModel
    {
        public string Salt { get; private set; }
        public bool EnablePost { get; private set; }
        public bool EnableConsoleErrorLogging { get; private set; }
        public bool IgnoreSessionCooldown { get; private set; }
        public TestMode? NotificationMode { get; private set; }
        public bool EnableManualSessionHandling { get; private set; }
        public int SessionDuration { get; private set; }
        public int EventSendThreshold { get; private set; }
        public int StoredRequestLimit { get; private set; }
        public int TotalBreadcrumbsAllowed { get; private set; }

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
                                    int sessionDuration = 60, int eventThreshold = 100,
                                    int storedRequestLimit = 1000, int totalBreadcrumbsAllowed = 100,
                                    TestMode? notificationMode = null)
        {
            Salt = salt;
            EnablePost = enablePost;
            EnableConsoleErrorLogging = enableConsoleErrorLogging;
            IgnoreSessionCooldown = ignoreSessionCooldown;
            NotificationMode = notificationMode;
            SessionDuration = sessionDuration;
            EnableManualSessionHandling = enableManualSessionHandling;
            EventSendThreshold = eventThreshold;
            StoredRequestLimit = storedRequestLimit;
            TotalBreadcrumbsAllowed = totalBreadcrumbsAllowed;
        }
    }
}
