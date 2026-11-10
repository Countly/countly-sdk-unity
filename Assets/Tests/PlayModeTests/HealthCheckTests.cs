using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Helpers;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Services;
using UnityEngine;

namespace Assets.Tests.PlayModeTests
{
    public class HealthCheckTests
    {
        private readonly List<Dictionary<string, string>> _sentRequests = new List<Dictionary<string, string>>();
        private CountlyResponse _serverAnswer;

        [SetUp]
        public void SetUp()
        {
            TestUtility.TestCleanup();
            PlayerPrefs.DeleteAll();
            _sentRequests.Clear();
            _serverAnswer = new CountlyResponse { StatusCode = 200, IsSuccess = true, Data = "{\"result\":\"Success\"}" };
            RequestCountlyHelper.ImmediateRequestSender = data => {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                foreach (string pair in data.Split('&')) {
                    string[] parts = pair.Split(new[] { '=' }, 2);
                    parameters[Uri.UnescapeDataString(parts[0])] = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : "";
                }
                _sentRequests.Add(parameters);
                return Task.FromResult(_serverAnswer);
            };
        }

        [TearDown]
        public void TearDown()
        {
            RequestCountlyHelper.ImmediateRequestSender = null;
            TestUtility.TestCleanup();
            PlayerPrefs.DeleteAll();
        }

        private static CountlyConfiguration HealthCheckConfig()
        {
            return new CountlyConfiguration("https://xyz.com/", "772c091355076ead703f987fee94490").SetDeviceId("test_user");
        }

        private static int Usage(JObject hc, string area, string method)
        {
            return hc["fu"][area]?[method]?.Value<int>() ?? 0;
        }

        private static int Code(JObject hc, string code)
        {
            return hc["lc"][code]?.Value<int>() ?? 0;
        }

        /// <summary>
        /// The health check is sent once after init as an immediate request with the base parameters, the app version
        /// as metrics and the counters; the warnings logged during init are in it, and an accepted one is cleared.
        /// </summary>
        [Test]
        public void HealthCheck_SentOnceAfterInit()
        {
            Countly.Instance.Init(HealthCheckConfig().SetEventQueueSizeToSend(0));

            Assert.AreEqual(1, _sentRequests.Count);
            Dictionary<string, string> request = _sentRequests[0];
            Assert.AreEqual("772c091355076ead703f987fee94490", request["app_key"]);
            Assert.AreEqual("test_user", request["device_id"]);
            Assert.IsTrue(request.ContainsKey("timestamp"));
            Assert.AreEqual(Constants.SdkVersion, request["sdk_version"]);
            Assert.AreEqual(Application.version, JObject.Parse(request["metrics"])["_app_version"].Value<string>());

            JObject hc = JObject.Parse(request["hc"]);
            foreach (string key in new[] { "el", "wl", "sc", "em", "bom", "cbom", "fu", "lc" }) {
                Assert.IsTrue(hc.ContainsKey(key), key);
            }
            Assert.GreaterOrEqual(hc["wl"].Value<int>(), 1);
            Assert.AreEqual(1, Code(hc, "w121"));
            Assert.AreEqual(0, Code(Countly.Instance.HealthCheck.Snapshot(), "w121"));
        }

        /// <summary>
        /// Disabled, nothing is sent, but warnings are still counted.
        /// </summary>
        [Test]
        public void HealthCheck_Disabled()
        {
            Countly.Instance.Init(HealthCheckConfig().DisableHealthCheck());

            Assert.AreEqual(0, _sentRequests.Count);
            int before = Countly.Instance.HealthCheck.Snapshot()["wl"].Value<int>();
            Countly.Instance.Events.CancelEvent("never started");
            Assert.AreEqual(before + 1, Countly.Instance.HealthCheck.Snapshot()["wl"].Value<int>());
        }

        /// <summary>
        /// A refused health check leaves the counters for the next start, and they are stored when the app pauses.
        /// </summary>
        [Test]
        public void HealthCheck_RefusedKeepsTheCounters()
        {
            _serverAnswer = new CountlyResponse { StatusCode = 400, IsSuccess = false, Data = "{\"result\":\"App does not exist\"}" };
            Countly.Instance.Init(HealthCheckConfig().SetEventQueueSizeToSend(0));

            Assert.AreEqual(1, _sentRequests.Count);
            Assert.AreEqual(1, Code(Countly.Instance.HealthCheck.Snapshot(), "w121"));

            Countly.Instance.HealthCheck.SaveState();
            JObject stored = JObject.Parse(PlayerPrefs.GetString(HealthCheckCountlyService.StorageKey));
            Assert.AreEqual(1, stored["LC"]["w121"].Value<int>());
            foreach (string key in new[] { "LErr", "LWar", "RStatC", "REMsg", "BReq", "CBReq", "MU", "LC" }) {
                Assert.IsTrue(stored.ContainsKey(key), key);
            }
        }

        /// <summary>
        /// The public event, view and crash methods are counted under the names of the guide.
        /// </summary>
        [Test]
        public void MethodUsage_Counted()
        {
            Countly.Instance.Init(HealthCheckConfig());

            _ = Countly.Instance.Events.RecordEventAsync("click");
            Countly.Instance.Events.StartEvent("timed");
            Countly.Instance.Events.EndEvent("timed");
            Countly.Instance.Events.StartEvent("cancelled");
            Countly.Instance.Events.CancelEvent("cancelled");

            string first = Countly.Instance.Views.StartView("first");
            Countly.Instance.Views.StartAutoStoppedView("auto");
            Countly.Instance.Views.PauseViewWithID(first);
            Countly.Instance.Views.ResumeViewWithID(first);
            Countly.Instance.Views.StopViewWithID(first);
            Countly.Instance.Views.StartView("named");
            Countly.Instance.Views.StopViewWithName("named");
            Countly.Instance.Views.StopAllViews(null);

            Countly.Instance.CrashReports.AddBreadcrumbs("crumb");
            _ = Countly.Instance.CrashReports.SendCrashReportAsync("message", "trace");

            JObject hc = Countly.Instance.HealthCheck.Snapshot();
            Assert.AreEqual(1, Usage(hc, "events", "record"));
            Assert.AreEqual(2, Usage(hc, "events", "start"));
            Assert.AreEqual(1, Usage(hc, "events", "end"));
            Assert.AreEqual(1, Usage(hc, "events", "cancel"));
            Assert.AreEqual(2, Usage(hc, "views", "start"));
            Assert.AreEqual(1, Usage(hc, "views", "startAuto"));
            Assert.AreEqual(1, Usage(hc, "views", "pause"));
            Assert.AreEqual(1, Usage(hc, "views", "resume"));
            Assert.AreEqual(2, Usage(hc, "views", "stop"));
            Assert.AreEqual(1, Usage(hc, "views", "stopAll"));
            Assert.AreEqual(1, Usage(hc, "crashes", "log"));
            Assert.AreEqual(1, Usage(hc, "crashes", "record"));
        }

        /// <summary>
        /// While consent is required, the calls of a feature without consent are not counted, and neither are the
        /// log codes they record.
        /// </summary>
        [Test]
        public void MethodUsage_FollowsConsent()
        {
            Countly.Instance.Init(HealthCheckConfig().SetRequiresConsent(true));

            Countly.Instance.Events.StartEvent("no consent");
            Countly.Instance.Events.EndEvent("not running");
            JObject hc = Countly.Instance.HealthCheck.Snapshot();
            Assert.AreEqual(0, Usage(hc, "events", "start"));
            Assert.AreEqual(0, Code(hc, "w720"));

            Countly.Instance.Consents.GiveConsent(new[] { Consents.Events });
            Countly.Instance.Events.EndEvent("not running");
            Assert.AreEqual(1, Code(Countly.Instance.HealthCheck.Snapshot(), "w720"));
        }

        /// <summary>
        /// Misuse of the event and view methods records the codes of the guide.
        /// </summary>
        [Test]
        public void LogCodes_Misuse()
        {
            Countly.Instance.Init(HealthCheckConfig());

            _ = Countly.Instance.Events.RecordEventAsync("");
            Countly.Instance.Events.StartEvent("twice");
            Countly.Instance.Events.StartEvent("twice");
            Countly.Instance.Events.EndEvent("not running");
            Countly.Instance.Events.CancelEvent("not running");
            Countly.Instance.Views.StartView("");
            Countly.Instance.Views.StopViewWithID("unknown");
            string view = Countly.Instance.Views.StartView("view");
            Countly.Instance.Views.PauseViewWithID(view);
            Countly.Instance.Views.PauseViewWithID(view);
            Countly.Instance.Init(HealthCheckConfig());

            JObject hc = Countly.Instance.HealthCheck.Snapshot();
            Assert.AreEqual(1, Code(hc, "w701"));
            Assert.AreEqual(1, Code(hc, "w722"));
            Assert.AreEqual(1, Code(hc, "w720"));
            Assert.AreEqual(1, Code(hc, "w721"));
            Assert.AreEqual(1, Code(hc, "w710"));
            Assert.AreEqual(1, Code(hc, "w730"));
            Assert.AreEqual(1, Code(hc, "w731"));
            Assert.AreEqual(1, Code(hc, "w110"));
        }

        /// <summary>
        /// An exception Unity caught is counted as a stored number, which the next start reports as "crashes" /
        /// "record:a"; meanwhile the report itself counts no method usage.
        /// </summary>
        [Test]
        public void AutomaticCrash_ReportedNextStart()
        {
            Countly.Instance.Init(HealthCheckConfig());
            Countly.Instance.CrashReports.SendAutomaticCrashReport("automatic", "trace");
            Assert.AreEqual(0, Usage(Countly.Instance.HealthCheck.Snapshot(), "crashes", "record"));
            Assert.AreEqual(1, PlayerPrefs.GetInt(HealthCheckCountlyService.AutomaticCrashesStorageKey));

            HealthCheckCountlyService next = new HealthCheckCountlyService(new CountlyLogHelper(HealthCheckConfig()));
            Assert.AreEqual(1, Usage(next.Snapshot(), "crashes", "record:a"));
            Assert.AreEqual(0, PlayerPrefs.GetInt(HealthCheckCountlyService.AutomaticCrashesStorageKey, 0));
        }

        /// <summary>
        /// A stored state that is not valid JSON is cleared and reported as e503; caps and backoff runs follow the guide.
        /// </summary>
        [Test]
        public void Counter_StoredStateCapsAndBackoff()
        {
            PlayerPrefs.SetString(HealthCheckCountlyService.StorageKey, "{not json");
            HealthCheckCountlyService counter = new HealthCheckCountlyService(new CountlyLogHelper(HealthCheckConfig()));
            Assert.AreEqual(1, Code(counter.Snapshot(), "e503"));
            Assert.AreEqual("", PlayerPrefs.GetString(HealthCheckCountlyService.StorageKey));

            counter.RecordLogCode("w101", 70000);
            counter.LogBackoffRequest();
            counter.LogBackoffRequest();
            counter.LogConsecutiveBackoffRequest();
            counter.LogBackoffRequest();
            counter.LogFailedNetworkRequest(1000, "ignored");
            counter.LogFailedNetworkRequest(503, new string('x', 1200));

            JObject hc = counter.Snapshot();
            Assert.AreEqual(65535, Code(hc, "w101"));
            Assert.AreEqual(3, hc["bom"].Value<int>());
            Assert.AreEqual(2, hc["cbom"].Value<int>());
            Assert.AreEqual(3, Code(hc, "w320"));
            Assert.AreEqual(1, Code(hc, "w321"));
            Assert.AreEqual(503, hc["sc"].Value<int>());
            Assert.AreEqual(1000, hc["em"].Value<string>().Length);
        }
    }
}
