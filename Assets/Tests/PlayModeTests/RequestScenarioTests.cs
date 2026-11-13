using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Models;
using UnityEngine;
using UnityEngine.TestTools;

namespace Assets.Tests.PlayModeTests
{
    /// <summary>
    /// In each request scenario the SDK queues the requests api/requests/[scenario].json records, with the values that
    /// change from run to run masked. The scenarios make the calls the other Countly SDKs make in their own request
    /// tests. Queued requests go to /i, which is the path each record carries. A record that differs is also written
    /// under artifacts/request-records/, which the test results artifact of a CI run holds. A deliberate change: run these
    /// tests with the environment variable COUNTLY_UPDATE_REQUESTS=1, or take the files from that artifact, check the
    /// difference and add a line to CHANGELOG.md.
    /// </summary>
    public class RequestScenarioTests
    {
        private const string MASK = "<masked>";
        private const int MAX_INT = 2147483647;
        private static readonly string[] MaskedParams = { "timestamp", "hour", "dow", "tz", "sdk_version", "av", "rr", "checksum", "checksum256", "session_duration" };
        private static readonly string[] JsonParams = { "events", "metrics", "user_details", "crash", "consent", "hc" };
        private static readonly string[] MaskedEventFields = { "timestamp", "hour", "dow", "id", "pvid", "cvid", "peid" };
        private static readonly string[] MaskedCrashFields = { "_error", "_run", "_os", "_os_version", "_resolution", "_density", "_locale", "_device", "_device_type", "_manufacture", "_manufacturer", "_cpu", "_opengl", "_ram_current", "_ram_total", "_disk_current", "_disk_total", "_bat", "_orientation", "_root", "_online", "_muted", "_background" };
        // a view's "segment" is the platform the SDK runs on
        private static readonly string[] MaskedViewSegments = { "segment" };

        /// <summary>
        /// Starts and ends every scenario with an SDK that has no stored data.
        /// </summary>
        [SetUp]
        [TearDown]
        public void Cleanup()
        {
            TestUtility.TestCleanup();
        }

        /// <summary>
        /// A session with an event of every segmentation value type, the user details and a session update.
        /// </summary>
        [UnityTest]
        public IEnumerator Parity()
        {
            Countly.Instance.Init(Config());
            yield return Countly.Instance.Session.BeginSessionAsync().AsCoroutine();
            yield return Countly.Instance.Events.RecordEventAsync("purchase", ParitySegmentation(), 2, 9.99, 5).AsCoroutine();
            Countly.Instance.UserProfile.SetProperties(new Dictionary<string, object> {
                { "name", "John Doe" },
                { "username", "john_d" },
                { "email", "john@example.com" },
                { "organization", "Countly" },
                { "phone", "1234567890" },
                { "picture", "https://example.com/picture.jpg" },
                { "gender", "M" },
                { "byear", 1980 },
                { "tier", "gold" },
                { "age", 30 }
            });
            Countly.Instance.UserProfile.Save();
            // a session update reports elapsed seconds, so at least one has to pass for it to be sent
            yield return new WaitForSecondsRealtime(1.1f);
            yield return Countly.Instance.Session.ExtendSessionAsync().AsCoroutine();
            yield return Countly.Instance.Session.EndSessionAsync().AsCoroutine();
            AssertRequests("parity");
        }

        /// <summary>
        /// Two views in a session, the first with segmentation.
        /// </summary>
        [UnityTest]
        public IEnumerator Views()
        {
            Countly.Instance.Init(Config());
            yield return Countly.Instance.Session.BeginSessionAsync().AsCoroutine();
            Countly.Instance.Views.StartView("Home", new Dictionary<string, object> { { "source", "scenario" } });
            Countly.Instance.Views.StopViewWithName("Home");
            Countly.Instance.Views.StartView("Settings");
            yield return Countly.Instance.Session.EndSessionAsync().AsCoroutine();
            AssertRequests("views");
        }

        /// <summary>
        /// A handled exception with a breadcrumb and segmentation.
        /// </summary>
        [UnityTest]
        public IEnumerator Crash()
        {
            Countly.Instance.Init(Config());
            Countly.Instance.CrashReports.AddBreadcrumbs("breadcrumb one");
            yield return Countly.Instance.CrashReports.SendCrashReportAsync("parity crash", "parity crash\n  at the scenario", new Dictionary<string, object> { { "tier", "gold" } }, true).AsCoroutine();
            AssertRequests("crash");
        }

        /// <summary>
        /// Consent given for sessions and events, then taken back for events.
        /// </summary>
        [UnityTest]
        public IEnumerator Consent()
        {
            Countly.Instance.Init(Config().SetRequiresConsent(true));
            Countly.Instance.Consents.GiveConsent(new[] { Consents.Sessions, Consents.Events });
            yield return Countly.Instance.Session.BeginSessionAsync().AsCoroutine();
            yield return Countly.Instance.Events.RecordEventAsync("consented").AsCoroutine();
            Countly.Instance.Events.AddEventsToRequestQueue();
            Countly.Instance.Consents.RemoveConsent(new[] { Consents.Events });
            yield return Countly.Instance.Events.RecordEventAsync("not_consented").AsCoroutine();
            yield return Countly.Instance.Session.EndSessionAsync().AsCoroutine();
            AssertRequests("consent");
        }

        /// <summary>
        /// A device ID change with merge, then one without.
        /// </summary>
        [UnityTest]
        public IEnumerator DeviceId()
        {
            Countly.Instance.Init(Config());
            yield return Countly.Instance.Session.BeginSessionAsync().AsCoroutine();
            yield return Countly.Instance.Device.ChangeDeviceIdWithMerge("parity-device-002").AsCoroutine();
            yield return Countly.Instance.Events.RecordEventAsync("after_merge").AsCoroutine();
            Countly.Instance.Events.AddEventsToRequestQueue();
            yield return Countly.Instance.Device.ChangeDeviceIdWithoutMerge("parity-device-003").AsCoroutine();
            yield return Countly.Instance.Events.RecordEventAsync("after_new_id").AsCoroutine();
            yield return Countly.Instance.Session.EndSessionAsync().AsCoroutine();
            AssertRequests("device-id");
        }

        /// <summary>
        /// The init config every scenario starts from: the parity app key and device ID, a server that cannot be reached so
        /// that every request stays queued, and manual sessions.
        /// </summary>
        /// <returns>the config</returns>
        private static CountlyConfiguration Config()
        {
            CountlyConfiguration config = new CountlyConfiguration("parity-app-key", "https://parity.example")
                .SetDeviceId("parity-device-001")
                .DisableHealthCheck();
            config.DisableAutomaticSessionTracking();
            return config;
        }

        /// <summary>
        /// The event segmentation of the parity scenario, with one value of every type.
        /// </summary>
        /// <returns>the segmentation</returns>
        private static Dictionary<string, object> ParitySegmentation()
        {
            return new Dictionary<string, object> {
                { "nar", "uto" },
                { "sasuke", MAX_INT },
                { "sakura", false },
                { "itachi", double.MaxValue },
                { "kakaski", double.Epsilon },
                { "hashirama", -MAX_INT },
                { "tobi", new Dictionary<string, object> { { "obito", "uchiha" }, { "zetsu", "white" } } },
                { "stringList", new List<string> { "value1", "value2", "value3" } },
                { "intList", new List<int> { 1, 2, 3 } },
                { "doubleList", new List<double> { 1.1, 2.2, 3.3 } },
                { "boolList", new List<bool> { true, false, true } },
                { "mixedList", new List<object> { "value1", 2, 3.3, true } },
                { "mapList", new List<Dictionary<string, object>> {
                    new Dictionary<string, object> { { "key1", "value1" }, { "key2", 2 } },
                    new Dictionary<string, object> { { "key1", "value2" }, { "key2", 3 } },
                    new Dictionary<string, object> { { "key1", "value3" }, { "key2", 4 } }
                } }
            };
        }

        /// <summary>
        /// Moves the recorded events into the request queue and compares the queue with the record of the scenario, or
        /// writes the record when COUNTLY_UPDATE_REQUESTS is set.
        /// </summary>
        /// <param name="name">scenario name, the record file name without .json</param>
        private static void AssertRequests(string name)
        {
            Countly.Instance.Events.AddEventsToRequestQueue();
            string current = Report(Countly.Instance.RequestHelper._requestRepo.Models.ToArray());
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string file = Path.Combine(projectRoot, "api", "requests", name + ".json");
            if (Environment.GetEnvironmentVariable("COUNTLY_UPDATE_REQUESTS") != null) {
                Directory.CreateDirectory(Path.GetDirectoryName(file));
                File.WriteAllText(file, current, new UTF8Encoding(false));
                return;
            }
            string committed = File.Exists(file) ? File.ReadAllText(file).Replace("\r\n", "\n") : "(no api/requests/" + name + ".json)\n";
            if (committed != current) {
                string kept = Path.Combine(projectRoot, "artifacts", "request-records", "api", "requests");
                Directory.CreateDirectory(kept);
                File.WriteAllText(Path.Combine(kept, name + ".json"), current, new UTF8Encoding(false));
            }
            Assert.AreEqual(committed, current, "the requests of the " + name + " scenario differ from api/requests/" + name + ".json. If that is deliberate, take the file from the test results artifact or run these tests with COUNTLY_UPDATE_REQUESTS=1, check the difference and add a line to CHANGELOG.md.");
        }

        /// <summary>
        /// The record of the queued requests: each with its path and its parameters, the values that change from run to run
        /// masked, the JSON parameters parsed and the keys of every object sorted.
        /// </summary>
        /// <param name="requests">the request queue, oldest first</param>
        /// <returns>file content</returns>
        private static string Report(CountlyRequestModel[] requests)
        {
            JArray report = new JArray();
            foreach (CountlyRequestModel request in requests) {
                NameValueCollection sent = CountlyUtils.ParseQueryString(request.RequestData);
                JObject parameters = new JObject();
                foreach (string name in sent.AllKeys.Where(key => key != null).OrderBy(key => key, StringComparer.Ordinal)) {
                    string value = sent[name];
                    if (MaskedParams.Contains(name)) {
                        parameters[name] = MASK;
                    } else if (JsonParams.Contains(name) && !string.IsNullOrEmpty(value)) {
                        parameters[name] = MaskedJson(name, JToken.Parse(value));
                    } else {
                        parameters[name] = value;
                    }
                }
                report.Add(new JObject { ["path"] = "/i", ["params"] = parameters });
            }
            return report.ToString(Formatting.Indented).Replace("\r\n", "\n") + "\n";
        }

        /// <summary>
        /// The value of a JSON parameter with the values that change from run to run masked and the keys of every object
        /// sorted.
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <param name="value">parsed parameter value</param>
        /// <returns>the masked value</returns>
        private static JToken MaskedJson(string name, JToken value)
        {
            if ((name == "metrics" || name == "hc") && value is JObject fields) {
                return Masked(fields, fields.Properties().Select(property => property.Name).ToArray());
            }
            if (name == "events" && value is JArray events) {
                return new JArray(events.Select(MaskedEvent));
            }
            if (name == "crash" && value is JObject crash) {
                return Masked(crash, MaskedCrashFields);
            }
            return Sorted(value);
        }

        /// <summary>
        /// An event with the values that change from run to run masked, for a view also in its segmentation.
        /// </summary>
        /// <param name="value">the event</param>
        /// <returns>the masked event</returns>
        private static JToken MaskedEvent(JToken value)
        {
            if (!(value is JObject sentEvent)) {
                return Sorted(value);
            }
            if ((string)sentEvent["key"] != "[CLY]_view") {
                return Masked(sentEvent, MaskedEventFields);
            }
            JObject view = Masked(sentEvent, MaskedEventFields.Concat(new[] { "dur" }).ToArray());
            if (view["segmentation"] is JObject segmentation) {
                view["segmentation"] = Masked(segmentation, MaskedViewSegments);
            }
            return view;
        }

        /// <summary>
        /// A copy of an object with the values of the given keys masked and the keys of every object sorted.
        /// </summary>
        /// <param name="value">object</param>
        /// <param name="keys">keys to mask</param>
        /// <returns>the masked copy</returns>
        private static JObject Masked(JObject value, string[] keys)
        {
            JObject copy = new JObject();
            foreach (JProperty property in value.Properties().OrderBy(property => property.Name, StringComparer.Ordinal)) {
                copy[property.Name] = keys.Contains(property.Name) ? new JValue(MASK) : Sorted(property.Value);
            }
            return copy;
        }

        /// <summary>
        /// A copy of a JSON value with the keys of every object sorted.
        /// </summary>
        /// <param name="value">JSON value</param>
        /// <returns>the sorted copy</returns>
        private static JToken Sorted(JToken value)
        {
            if (value is JObject obj) {
                return Masked(obj, new string[0]);
            }
            if (value is JArray array) {
                return new JArray(array.Select(Sorted));
            }
            return value.DeepClone();
        }
    }
}
