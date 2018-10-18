using Assets.Scripts.Enums;

namespace Assets.Scripts.Models
{
    public class CountlyConfigModel
    {
        public string Salt { get; set; }
        public bool EnablePost { get; set; }
        public bool EnableConsoleErrorLogging { get; set; }
        public bool IgnoreSessionCooldown { get; set; }
        public TestMode? NotificationMode { get; set; }

        public CountlyConfigModel(string salt, bool enablePost = false, bool enableConsoleErrorLogging = false,
                                    bool ignoreSessionCooldown = false, TestMode? notificationMode = null)
        {
            Salt = salt;
            EnablePost = enablePost;
            EnableConsoleErrorLogging = enableConsoleErrorLogging;
            IgnoreSessionCooldown = ignoreSessionCooldown;
            NotificationMode = notificationMode;
        }
    }
}
