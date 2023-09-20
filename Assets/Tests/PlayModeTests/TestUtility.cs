using Plugins.CountlySDK;
using Plugins.CountlySDK.Models;

namespace Assets.Tests.PlayModeTests
{
    public class TestUtility
    {
        readonly static string SERVER_URL = "https://xyz.com/";
        readonly static string APP_KEY = "772c091355076ead703f987fee94490";
        readonly static string DEVICE_ID = "test_user";
        public static CountlyConfiguration createBaseConfig()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = SERVER_URL,
                AppKey = APP_KEY,
                DeviceId = DEVICE_ID,
            };

            return configuration;

        }
        public static void ClearSDKQueues(Countly CountlyInstance)
        {
            CountlyInstance.Views._eventService._eventRepo.Clear();
            CountlyInstance.CrashReports._requestCountlyHelper._requestRepo.Clear();
        }
    }
}
