using Plugins.CountlySDK;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Persistance.Entities;

namespace Assets.Tests.PlayModeTests
{
    /// Utility class for creating Countly configurations, clearing SDK queues, and managing log helpers.
    public class TestUtility
    {
        private readonly static string SERVER_URL = "https://xyz.com/";
        private readonly static string APP_KEY = "772c091355076ead703f987fee94490";
        private readonly static string DEVICE_ID = "test_user";

        /// <summary>
        /// Creates a basic Countly configuration with predefined server URL, app key, and device ID.
        /// </summary>
        /// <returns>CountlyConfiguration object representing the basic configuration.</returns>
        public static CountlyConfiguration createBaseConfig()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = SERVER_URL,
                AppKey = APP_KEY,
                DeviceId = DEVICE_ID,
            };

            return configuration;
        }

        /// Clears the queues of the Countly SDK.
        public static void ClearSDKQueues(Countly CountlyInstance)
        {
            CountlyInstance.Views._eventService._eventRepo.Clear();
            CountlyInstance.CrashReports._requestCountlyHelper._requestRepo.Clear();
        }

        /// <summary>
        /// Creates a CountlyLogHelper instance for testing purposes, based on the provided configuration and enables or disables logging.
        /// </summary>
        /// <returns>CountlyLogHelper instance with the specified logging configuration.</returns>
        public static CountlyLogHelper LogHelper(bool enableLogging)
        {
            CountlyConfiguration config = createBaseConfig();
            config.EnableConsoleLogging = enableLogging;
            CountlyLogHelper logHelper = new CountlyLogHelper(config);

            return logHelper;
        }
        public static EventEntity CreateEventEntity(int id, string json)
        {
            EventEntity entity = new EventEntity {
                Id = id,
                Json = json
            };
            return entity;
        }

    }
}
