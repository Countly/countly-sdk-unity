using NUnit.Framework;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Enums;

namespace Tests
{
    public class ConfigurationTests
    {
        [Test]
        public void TestSDKInitParams()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = "https://try.count.ly/",
                AppKey = "8e2fe772c091355076ead703f987fee94490fff4",
            };

            Countly.Instance.Init(configuration);

            Assert.AreNotEqual(Countly.Instance, null);
            Assert.AreNotEqual(Countly.Instance.Configuration, null);
          //  Assert.AreEqual(Countly.Instance.IsSDKInitialized, true);
          //  Assert.AreEqual(Countly.Instance.isActiveAndEnabled, true);

            Countly.Instance.ResetDB();
        }

        [Test]
        public void TestDefaultConfigValues()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = "https://try.count.ly/",
                AppKey = "8e2fe772c091355076ead703f987fee94490fff4",
            };

            Countly.Instance.Init(configuration);

            Assert.AreEqual(Countly.Instance.Configuration.SessionDuration, 60);
            Assert.AreEqual(Countly.Instance.Configuration.StoredRequestLimit, 1000);
            Assert.AreEqual(Countly.Instance.Configuration.EventQueueThreshold, 100);
            Assert.AreEqual(Countly.Instance.Configuration.TotalBreadcrumbsAllowed, 100);
            Assert.AreEqual(Countly.Instance.Configuration.NotificationMode, TestMode.None);

            Countly.Instance.ResetDB();
        }

        [Test]
        public void TestServerURLAndAppKey()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = "https://try.count.ly/",
                AppKey = "8e2fe772c091355076ead703f987fee94490fff4",
            };

            Countly.Instance.Init(configuration);

            Assert.AreEqual(Countly.Instance.Configuration.AppKey, "8e2fe772c091355076ead703f987fee94490fff4");
            Assert.AreEqual(Countly.Instance.Configuration.ServerUrl, "https://try.count.ly");

            Countly.Instance.ResetDB();
        }

        [Test]
        public void TestLocationFields()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = "https://try.count.ly/",
                AppKey = "8e2fe772c091355076ead703f987fee94490fff4",
            };

            string countryCode = "us";
            string city = "Houston";
            string latitude = "29.634933";
            string longitude = "-95.220255";
            string ipAddress = "10.2.33.12";

            configuration.SetLocation(countryCode, city, latitude + "," + longitude, ipAddress);
            Countly.Instance.Init(configuration);

            Assert.AreEqual(Countly.Instance.Configuration.City, "Houston");
            Assert.AreEqual(Countly.Instance.Configuration.IsLocationDisabled, false);
            Assert.AreEqual(Countly.Instance.Configuration.Location, "29.634933,-95.220255");

            Countly.Instance.ResetDB();
        }
    }
}
