
namespace Plugins.Countly.Helpers
{
    internal class Constants
    {
        public const string CountlyServerUrl = "https://us-try.count.ly/";
        public const string DeviceIDKey = "DeviceID";
        
        public const string FirstAppLaunch = "Countly.FirstAppLaunch";
        public const string FirstAppLaunchSegment = "firstAppLaunch";
        
        #region Notification Keys

        public const string MessageIDKey = "c.i";
        public const string TitleDataKey = "title";
        public const string MessageDataKey = "message";
        public const string ImageUrlKey = "c.m";
        public const string ActionButtonKey = "c.b";
        public const string SoundDataKey = "sound";

        #endregion

        #region Unity System

        public static string UnityPlatform =>
            UnityEngine.Application.platform.ToString().ToLower() == "iphoneplayer"
            ? "ios"
            : UnityEngine.Application.platform.ToString().ToLower();

        #endregion
    }
}
