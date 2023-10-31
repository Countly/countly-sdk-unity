using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK;
using Newtonsoft.Json;
using System.Web;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using Plugins.CountlySDK.Enums;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine.TestTools;
using System.Linq;
using System;
using Assets.Tests.PlayModeTests;
using Plugins.CountlySDK.Helpers;
using System.Reflection;

namespace Tests
{
    public class SessionTests
    {
        /// <summary>
        /// Assert session request.
        /// </summary>
        /// <param name="collection"> collection of params</param>
        /// <param name="sessionKey"> session predefined key </param>
        /// <param name="deviceId"> device id </param>
        private void AssertSessionRequest(NameValueCollection collection, string sessionKey, double? duration = null, bool hasMetrics = false)
        {
            if (sessionKey != null) {
                Assert.AreEqual("1", collection.Get(sessionKey));
            }

            if (duration != null) {
                Assert.GreaterOrEqual(duration, Convert.ToDouble(collection.Get("session_duration")));

            }

            if (hasMetrics) {
                Assert.IsNotNull(collection.Get("metrics"));
            } else {
                Assert.IsNull(collection.Get("metrics"));
            }
        }

        private void AssertLocation(NameValueCollection collection, string gpsCoord, string city, string ipAddress, string countryCode)
        {
            Assert.AreEqual(ipAddress, collection.Get("ip_address"));
            Assert.AreEqual(countryCode, collection.Get("country_code"));
            Assert.AreEqual(city, collection.Get("city"));
            Assert.AreEqual(gpsCoord, collection.Get("location"));
        }

        /// <summary>
        /// It checks the working of session service if no 'Session' consent is given.
        /// </summary>
        [Test]
        public async void SessionConsent()
        {
            Countly.Instance.Init(TestUtility.createBaseConfigConsent(null));
            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();
            Assert.IsNotNull(Countly.Instance.Session);
            Assert.AreEqual(0, Countly.Instance.Session._requestCountlyHelper._requestRepo.Count);

            await Countly.Instance.Session.BeginSessionAsync();
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            await Countly.Instance.Session.ExtendSessionAsync();
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);

            await Countly.Instance.Session.EndSessionAsync();
            Assert.AreEqual(0, Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Count);
        }

        /// <summary>
        /// It validates the functionality of 'BeginSessionAsync'.
        /// </summary>
        [Test]
        public void SessionBegin_Default()
        {
            Countly.Instance.Init(TestUtility.createBaseConfig());
            Assert.IsNotNull(Countly.Instance.Session);
            Assert.AreEqual(1, Countly.Instance.Session._requestCountlyHelper._requestRepo.Count);

            CountlyRequestModel requestModel = Countly.Instance.Session._requestCountlyHelper._requestRepo.Dequeue();
            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);

            AssertSessionRequest(collection, "begin_session", null, true);
        }

        /// <summary>
        /// It validates the functionality of 'BeginSessionAsync' with location values.
        /// </summary>
        [Test]
        public void SessionBegin_WithLocation()
        {
            CountlyConfiguration configuration = TestUtility.createBaseConfig();
            configuration.SetLocation("PK", "Lahore", "10.0,10.0", "192.168.100.51");
            Countly.Instance.Init(configuration);
            Assert.IsNotNull(Countly.Instance.Session);
            Assert.AreEqual(1, Countly.Instance.Session._requestCountlyHelper._requestRepo.Count);

            CountlyRequestModel requestModel = Countly.Instance.Session._requestCountlyHelper._requestRepo.Dequeue();

            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            AssertLocation(collection, "10.0,10.0", "Lahore", "192.168.100.51", "PK");
            AssertSessionRequest(collection, "begin_session", null, true);
        }

        /// <summary>
        /// It validates the functionality of 'BeginSessionAsync' with location disable.
        /// </summary>
        [Test]
        public void SessionBegin_WithLocationDisable()
        {
            CountlyConfiguration configuration = TestUtility.createBaseConfig();
            configuration.DisableLocation();
            Countly.Instance.Init(configuration);
            Assert.IsNotNull(Countly.Instance.Session);
            Assert.AreEqual(1, Countly.Instance.Session._requestCountlyHelper._requestRepo.Count);

            CountlyRequestModel requestModel = Countly.Instance.Session._requestCountlyHelper._requestRepo.Dequeue();

            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            Assert.AreEqual(string.Empty, collection.Get("location"));
            AssertSessionRequest(collection, "begin_session", null, true);
        }

        /// <summary>
        /// It validates the functionality of 'BeginSessionAsync' when session consent is given after init.
        /// </summary>
        [Test]
        public void SessionBegin_ConsentGivenAfterInit()
        {
            Countly.Instance.Init(TestUtility.createBaseConfigConsent(null));
            Assert.IsNotNull(Countly.Instance.Session);
            Assert.AreEqual(2, Countly.Instance.Session._requestCountlyHelper._requestRepo.Count);
            Countly.Instance.Session._requestCountlyHelper._requestRepo.Clear();

            Countly.Instance.Consents.GiveConsent(new Consents[] { Consents.Sessions });
            //RQ will have consent change request and session begin request
            Assert.AreEqual(2, Countly.Instance.Session._requestCountlyHelper._requestRepo.Count);
            Countly.Instance.Session._requestCountlyHelper._requestRepo.Dequeue(); // Remove consent Request
            CountlyRequestModel requestModel = Countly.Instance.Session._requestCountlyHelper._requestRepo.Dequeue();

            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            AssertSessionRequest(collection, "begin_session", null, true);
        }

        /// <summary>
        /// It validates the working of session service when automatic session tracking is disabled.
        /// </summary>
        [Test]
        public void SessionService_WithDisableTracking()
        {
            CountlyConfiguration configuration = TestUtility.createBaseConfig();
            configuration.DisableAutomaticSessionTracking();
            Countly.Instance.Init(configuration);
            Assert.IsNotNull(Countly.Instance.Session);
            Assert.AreEqual(0, Countly.Instance.Session._requestCountlyHelper._requestRepo.Count);
        }

        /// <summary>
        /// It validates if 'BeginSessionAsync' does call when session consent is given after init and automatic session tracking is disabled.
        /// </summary>
        [Test]
        public void SessionBegin_ConsentGivenAfterInitWithDisableTracking()
        {
            CountlyConfiguration configuration = TestUtility.createBaseConfigConsent(null);
            configuration.DisableAutomaticSessionTracking();
            Countly.Instance.Init(configuration);
            Assert.IsNotNull(Countly.Instance.Session);
            Assert.AreEqual(2, Countly.Instance.Session._requestCountlyHelper._requestRepo.Count);
            Countly.Instance.Session._requestCountlyHelper._requestRepo.Clear();

            Countly.Instance.Consents.GiveConsent(new Consents[] { Consents.Sessions });
            //RQ will have consent change request
            Assert.AreEqual(1, Countly.Instance.Session._requestCountlyHelper._requestRepo.Count);
        }

        /// <summary>
        /// It validates if 'BeginSessionAsync' does call when session consent is given after init and session had begun before.
        /// </summary>
        [Test]
        public void SessionBegin_ConsentGivenAfterSessionBegin()
        {
            Countly.Instance.Init(TestUtility.createBaseConfigConsent(null));
            Assert.IsNotNull(Countly.Instance.Session);
            // RQ will have empty location request and consent reqeust
            Assert.AreEqual(2, Countly.Instance.Session._requestCountlyHelper._requestRepo.Count);
            Countly.Instance.Session._requestCountlyHelper._requestRepo.Clear();

            Countly.Instance.Consents.GiveConsent(new Consents[] { Consents.Sessions });
            // RQ will have Consent change request and Session begin request
            Assert.AreEqual(2, Countly.Instance.Session._requestCountlyHelper._requestRepo.Count);
            Countly.Instance.Session._requestCountlyHelper._requestRepo.Clear();

            Countly.Instance.Consents.RemoveConsent(new Consents[] { Consents.Sessions });
            //RQ will have consent change request and session begin request
            Assert.AreEqual(1, Countly.Instance.Session._requestCountlyHelper._requestRepo.Count);

            Countly.Instance.Session._requestCountlyHelper._requestRepo.Clear();
            // RQ will have Consent change request and Session begin request
            Countly.Instance.Consents.GiveConsent(new Consents[] { Consents.Sessions });
            Assert.AreEqual(2, Countly.Instance.Session._requestCountlyHelper._requestRepo.Count);
        }

        /// <summary>
        /// It validates the request of 'ExtendSessionAsync'.
        /// </summary>
        [UnityTest]
        public IEnumerator SessionExtend()
        {
            System.DateTime sessionStartTime = System.DateTime.Now;

            Countly.Instance.Init(TestUtility.createBaseConfig());
            Assert.IsNotNull(Countly.Instance.Session);
            Assert.AreEqual(1, Countly.Instance.Session._requestCountlyHelper._requestRepo.Count);

            CountlyRequestModel requestModel = Countly.Instance.Session._requestCountlyHelper._requestRepo.Dequeue();

            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            AssertSessionRequest(collection, "begin_session", null, true);

            System.DateTime startTime = System.DateTime.UtcNow;
            do {
                yield return null;
            }
            while ((System.DateTime.UtcNow - startTime).TotalSeconds < 2.0);

            Countly.Instance.Session.ExtendSessionAsync();
            double duration = (System.DateTime.Now - sessionStartTime).TotalSeconds;

            startTime = System.DateTime.UtcNow;
            do {
                yield return null;
            }
            while ((System.DateTime.UtcNow - startTime).TotalSeconds < 0.4);

            Assert.AreEqual(1, Countly.Instance.Session._requestCountlyHelper._requestRepo.Count);

            requestModel = Countly.Instance.Session._requestCountlyHelper._requestRepo.Dequeue();

            collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            AssertSessionRequest(collection, null, duration);
        }

        /// <summary>
        /// It validates the request of 'EndSessionAsync'.
        /// </summary>
        [UnityTest]
        public IEnumerator SessionEnd()
        {
            System.DateTime sessionStartTime = System.DateTime.Now;

            Countly.Instance.Init(TestUtility.createBaseConfig());
            Assert.IsNotNull(Countly.Instance.Session);
            Assert.AreEqual(1, Countly.Instance.Session._requestCountlyHelper._requestRepo.Count);
            Countly.Instance.Session._requestCountlyHelper._requestRepo.Clear();

            System.DateTime startTime = System.DateTime.UtcNow;
            do {
                yield return null;
            }
            while ((System.DateTime.UtcNow - startTime).TotalSeconds < 2.0);

            Countly.Instance.Session.EndSessionAsync();

            double duration = (System.DateTime.Now - sessionStartTime).TotalSeconds;

            startTime = System.DateTime.UtcNow;
            do {
                yield return null;
            }
            while ((System.DateTime.UtcNow - startTime).TotalSeconds < 0.4);

            Assert.AreEqual(1, Countly.Instance.Session._requestCountlyHelper._requestRepo.Count);

            CountlyRequestModel requestModel = Countly.Instance.Session._requestCountlyHelper._requestRepo.Dequeue();

            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            AssertSessionRequest(collection, "end_session", duration);
        }

        // Validates the accuracy of the metrics within a request
        // Parses the metrics within the request and compares with a new 'CountlyMetricModel'
        // Metrics within the parsed object should be equal to the expected values.
        [Test]
        public void SessionMetrics()
        {
            Countly.Instance.Init(TestUtility.createBaseConfig());
            Assert.IsNotNull(Countly.Instance.Session);
            Assert.AreEqual(1, Countly.Instance.Session._requestCountlyHelper._requestRepo.Count);

            CountlyRequestModel requestModel = Countly.Instance.Session._requestCountlyHelper._requestRepo.Dequeue();

            CountlyMetricModel metricModel = new CountlyMetricModel(Countly.Instance.MetricHelper);

            string[] kvp = requestModel.RequestData.Split('&');
            string metricsKeyValue = kvp.FirstOrDefault(kv => kv.StartsWith("metrics="));
            string metricsJsonString = Uri.UnescapeDataString(metricsKeyValue.Substring("metrics=".Length));

            JObject metricsObject = JObject.Parse(metricsJsonString);

            Assert.AreEqual(metricsObject["_os"].ToString(), metricModel.OS);
            Assert.AreEqual(metricsObject["_os_version"].ToString(), metricModel.OSVersion);
            Assert.AreEqual(metricsObject["_device"].ToString(), metricModel.Device);
            Assert.AreEqual(metricsObject["_resolution"].ToString(), metricModel.Resolution);
            Assert.AreEqual(metricsObject["_carrier"], metricModel.Carrier);
            Assert.AreEqual(metricsObject["_app_version"].ToString(), metricModel.AppVersion);
            Assert.AreEqual(metricsObject["_density"].ToString(), metricModel.Density);
            Assert.AreEqual(metricsObject["_store"], metricModel.Store);
            Assert.AreEqual(metricsObject["_browser"], metricModel.Browser);
            Assert.AreEqual(metricsObject["_browser_version"], metricModel.BrowserVersion);
            Assert.AreEqual(metricsObject["_locale"].ToString(), metricModel.Locale);
        }

        [TearDown]
        public void End()
        {
            Countly.Instance.ClearStorage();
            UnityEngine.Object.DestroyImmediate(Countly.Instance);
        }
    }
}
