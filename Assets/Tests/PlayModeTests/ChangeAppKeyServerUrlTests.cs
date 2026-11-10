using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Models;
using UnityEngine;
using UnityEngine.TestTools;

namespace Assets.Tests.PlayModeTests
{
    public class ChangeAppKeyServerUrlTests
    {
        private const string AppKey = "772c091355076ead703f987fee94490";
        private const string NewAppKey = "5e7d0a8a6b5c4b3d2e1f0a9b8c7d6e5f4a3b2c1d";
        private const string DeviceId = "change_test_device";
        private const string PushToken = "change_test_push_token";
        private readonly List<MockServer> _servers = new List<MockServer>();

        /// <summary>
        /// Initializes the SDK with the given server URL, the test app key and device ID, and returns it.
        /// </summary>
        private Countly InitSDK(string serverUrl, bool sessions = true, Consents[] consents = null)
        {
            CountlyConfiguration config = new CountlyConfiguration(AppKey, serverUrl).SetDeviceId(DeviceId).DisableHealthCheck();
            if (!sessions) {
                config.DisableAutomaticSessionTracking();
            }
            if (consents != null) {
                config.SetRequiresConsent(true);
                config.GiveConsent(consents);
            }

            Countly.Instance.Init(config);
            return Countly.Instance;
        }

        /// <summary>
        /// Starts a local server that answers like a Countly server. It is stopped after the test.
        /// </summary>
        private MockServer StartServer()
        {
            MockServer server = new MockServer();
            _servers.Add(server);
            return server;
        }

        /// <summary>
        /// Returns the parameters of the queued requests, in queue order.
        /// </summary>
        private List<NameValueCollection> QueuedRequests()
        {
            return Countly.Instance.RequestHelper._requestRepo.Models.Select(r => CountlyUtils.ParseQueryString(r.RequestData)).ToList();
        }

        /// <summary>
        /// Returns the event keys of an "events" request.
        /// </summary>
        private List<string> EventKeys(NameValueCollection request)
        {
            return JArray.Parse(request["events"]).Select(e => (string)e["key"]).ToList();
        }

        /// <summary>
        /// Records an event and packs the event queue into a request.
        /// </summary>
        private IEnumerator QueueEventRequest(string eventKey)
        {
            yield return Countly.Instance.Events.RecordEventAsync(eventKey).AsCoroutine();
            Countly.Instance.Events.AddEventsToRequestQueue();
        }

        /// <summary>
        /// Waits frame by frame until the condition holds, and fails the test after the timeout.
        /// </summary>
        private IEnumerator WaitFor(Func<bool> condition, string description, float timeoutSeconds = 30f)
        {
            float end = Time.realtimeSinceStartup + timeoutSeconds;
            while (!condition() && Time.realtimeSinceStartup < end) {
                yield return null;
            }
            Assert.IsTrue(condition(), "Timed out waiting for: " + description);
        }

        /// <summary>
        /// Changes the server URL to valid values, with and without trailing slashes, and to invalid values that must be ignored.
        /// The app key and the configuration stay as they are, and no request is created.
        /// </summary>
        [Test]
        public void ChangeServerUrl_ValidAndInvalidValues()
        {
            Countly cly = InitSDK("https://server-a.example/", sessions: false);
            TestUtility.ClearSDKQueues(cly);
            Assert.AreEqual("https://server-a.example/i?", cly.Utils.ServerInputUrl);

            cly.ChangeServerUrl("https://server-b.example///");
            Assert.AreEqual("https://server-b.example/i?", cly.Utils.ServerInputUrl);

            string[] invalidUrls = { null, "", " ", "not a url", "ftp://server-c.example", "http://", "/countly", "server-c.example",
                " https://server-c.example", "https://server-c.example/?a=1", "https://server-c.example/#a" };
            foreach (string invalidUrl in invalidUrls) {
                cly.ChangeServerUrl(invalidUrl);
                Assert.AreEqual("https://server-b.example/i?", cly.Utils.ServerInputUrl, $"URL: [{invalidUrl}]");
            }

            cly.ChangeServerUrl("http://server-c.example:8080/countly/");
            Assert.AreEqual("http://server-c.example:8080/countly/i?", cly.Utils.ServerInputUrl);
            Assert.AreEqual(AppKey, cly.Utils.AppKey);
            Assert.AreEqual("https://server-a.example", cly.Configuration.GetServerUrl());
            Assert.AreEqual(AppKey, cly.Configuration.GetAppKey());
            TestUtility.ValidateRQEQSize(cly, 0, 0);
        }

        /// <summary>
        /// Requests queued while server A is down are sent to server B after the URL change, with the same app key.
        /// Later requests go to server B too, and server A gets nothing after the change.
        /// </summary>
        [UnityTest]
        public IEnumerator ChangeServerUrl_QueuedRequestsGoToNewServer()
        {
            MockServer serverA = StartServer();
            MockServer serverB = StartServer();
            serverA.Down = true;

            Countly cly = InitSDK(serverA.Url);
            yield return WaitFor(() => serverA.Count >= 1, "begin_session attempt at server A");
            yield return QueueEventRequest("queued_event");
            yield return new WaitForSeconds(1f);

            List<NameValueCollection> queued = QueuedRequests();
            Assert.AreEqual(2, queued.Count);
            Assert.AreEqual("1", queued[0]["begin_session"]);
            Assert.AreEqual(new List<string> { "queued_event" }, EventKeys(queued[1]));
            int attemptsAtA = serverA.Count;

            cly.ChangeServerUrl(serverB.Url + "/");
            yield return WaitFor(() => cly.RequestHelper._requestRepo.Count == 0, "queue sent to server B");

            Assert.AreEqual(2, serverB.Count);
            Assert.AreEqual("1", serverB.Request(0)["begin_session"]);
            Assert.AreEqual(new List<string> { "queued_event" }, EventKeys(serverB.Request(1)));
            foreach (NameValueCollection request in serverB.Requests()) {
                Assert.AreEqual("/i", request[MockServer.PathKey]);
                Assert.AreEqual(AppKey, request["app_key"]);
                Assert.AreEqual(DeviceId, request["device_id"]);
            }

            yield return cly.Events.RecordEventAsync("after_change").AsCoroutine();
            cly.AttemptToSendStoredRequests();
            yield return WaitFor(() => serverB.Count == 3, "new request at server B");
            Assert.AreEqual(new List<string> { "after_change" }, EventKeys(serverB.Request(2)));
            Assert.AreEqual(attemptsAtA, serverA.Count);
        }

        /// <summary>
        /// The app key change stops the views, packs the events, saves the user profile and ends the session with the old key,
        /// then begins a session with the new key. Timed events and the device ID stay, the stored remote config values are removed,
        /// and the configuration keeps its app key. The requests reach the server in this order.
        /// </summary>
        [UnityTest]
        public IEnumerator ChangeAppKey_ClosesOldAppAndBeginsNewSession()
        {
            MockServer server = StartServer();
            Countly cly = InitSDK(server.Url);
            yield return WaitFor(() => server.Count == 1 && cly.RequestHelper._requestRepo.Count == 0, "begin_session sent");
            yield return cly.RemoteConfigs.Update().AsCoroutine();
            Assert.IsNotNull(cly.RemoteConfigs.Configs);
            Assert.AreEqual(1, cly.StorageHelper.ConfigDao.LoadAll().Count);

            server.Down = true;
            cly.Events.StartEvent("timed_event");
            yield return cly.Events.RecordEventAsync("old_app_event").AsCoroutine();
            cly.Views.StartView("old_view");
            cly.UserProfile.SetProperty("old_property", "old_value");

            cly.ChangeAppKey(NewAppKey);

            // Recording the view end event saves the user profile first, so the events are split around user_details
            List<NameValueCollection> queued = QueuedRequests();
            int last = queued.Count - 1;
            Assert.AreEqual(new List<string> { "old_app_event", "[CLY]_view", "[CLY]_view" },
                queued.Where(r => r["events"] != null).SelectMany(EventKeys).ToList());
            StringAssert.Contains("old_property", queued.Single(r => r["user_details"] != null)["user_details"]);
            Assert.AreEqual("1", queued[last - 1]["end_session"]);
            Assert.AreEqual("1", queued[last]["begin_session"]);
            for (int i = 0; i < last; i++) {
                Assert.AreEqual(AppKey, queued[i]["app_key"], $"request {i}");
            }
            Assert.AreEqual(NewAppKey, queued[last]["app_key"]);

            TestUtility.ValidateRQEQSize(cly, queued.Count, 0);
            Assert.IsTrue(cly.Events._timedEvents.ContainsKey("timed_event"));
            Assert.IsTrue(cly.Session.IsSessionInitiated);
            Assert.AreEqual(DeviceId, cly.Device.DeviceId);
            Assert.IsNull(cly.RemoteConfigs.Configs);
            Assert.AreEqual(0, cly.StorageHelper.ConfigDao.LoadAll().Count);
            Assert.AreEqual(NewAppKey, cly.Utils.AppKey);
            Assert.AreEqual(AppKey, cly.Configuration.GetAppKey());

            yield return new WaitForSeconds(1f);
            server.Down = false;
            int before = server.Count;
            cly.AttemptToSendStoredRequests();
            yield return WaitFor(() => cly.RequestHelper._requestRepo.Count == 0, "queue sent");

            List<NameValueCollection> sent = server.Requests().Skip(before).ToList();
            Assert.AreEqual(queued.Count, sent.Count);
            for (int i = 0; i < sent.Count; i++) {
                Assert.AreEqual(queued[i]["app_key"], sent[i]["app_key"], $"request {i}");
                Assert.AreEqual(queued[i]["events"], sent[i]["events"], $"request {i}");
            }
            Assert.AreEqual("1", sent[last - 1]["end_session"]);
            Assert.AreEqual("1", sent[last]["begin_session"]);
        }

        /// <summary>
        /// Invalid, placeholder and same app keys are ignored: the key stays, the session keeps running and no request is created.
        /// </summary>
        [Test]
        public void ChangeAppKey_InvalidPlaceholderAndSameKeyAreIgnored()
        {
            Countly cly = InitSDK("https://server-a.example");
            TestUtility.ClearSDKQueues(cly);

            foreach (string appKey in new[] { null, "", " ", "YOUR_APP_KEY", AppKey }) {
                cly.ChangeAppKey(appKey);
                Assert.AreEqual(AppKey, cly.Utils.AppKey, $"app key: [{appKey}]");
                Assert.IsTrue(cly.Session.IsSessionInitiated);
                TestUtility.ValidateRQEQSize(cly, 0, 0);
            }
        }

        /// <summary>
        /// Called off the main thread, the app key change ends the session with the old key right away
        /// and begins the new session on the main thread, where Unity allows reading the device metrics.
        /// </summary>
        [UnityTest]
        public IEnumerator ChangeAppKey_OffMainThread()
        {
            Countly cly = InitSDK("https://server-a.example");
            TestUtility.ClearSDKQueues(cly);

            yield return Task.Run(() => cly.ChangeAppKey(NewAppKey)).AsCoroutine();
            for (int i = 0; i < 5; i++) {
                yield return null;
            }

            List<NameValueCollection> queued = QueuedRequests();
            Assert.AreEqual(2, queued.Count);
            Assert.AreEqual("1", queued[0]["end_session"]);
            Assert.AreEqual(AppKey, queued[0]["app_key"]);
            Assert.AreEqual("1", queued[1]["begin_session"]);
            Assert.AreEqual(NewAppKey, queued[1]["app_key"]);
            Assert.IsNotNull(queued[1]["metrics"]);
            Assert.IsTrue(cly.Session.IsSessionInitiated);
        }

        /// <summary>
        /// When the SDK holds a push token, it sends the token again after an app key change and after a server URL change,
        /// with the current app key.
        /// </summary>
        [Test]
        public void ChangeAppKeyAndServerUrl_ResendPushToken()
        {
            Countly cly = InitSDK("https://server-a.example", sessions: false);
            object push =typeof(Countly).GetField("_push", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(cly);
            push.GetType().GetField("_token", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(push, PushToken);
            push.GetType().GetField("_mode", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(push, (TestMode?)TestMode.ProductionToken);
            cly.RequestHelper._requestRepo.Clear();

            cly.ChangeAppKey(NewAppKey);
            cly.ChangeServerUrl("https://server-b.example");

            List<NameValueCollection> tokenRequests = QueuedRequests().Where(r => r["token_session"] == "1").ToList();
            Assert.AreEqual(2, tokenRequests.Count);
            foreach (NameValueCollection request in tokenRequests) {
                Assert.AreEqual(NewAppKey, request["app_key"]);
                Assert.AreEqual("0", request["test_mode"]);
                Assert.IsTrue(request.AllKeys.Any(key => key.EndsWith("_token") && request[key] == PushToken));
            }
        }

        /// <summary>
        /// Without a session, the app key change only packs the events with the old key. While consent is required,
        /// the consent state is sent again after an app key change and after a server URL change, with the new app key.
        /// </summary>
        [UnityTest]
        public IEnumerator ChangeAppKeyAndServerUrl_SendConsentWithoutSession()
        {
            Countly cly = InitSDK("https://server-a.example", sessions: false, consents: new[] { Consents.Events, Consents.Users });
            cly.RequestHelper._requestRepo.Clear();
            yield return cly.Events.RecordEventAsync("old_app_event").AsCoroutine();

            cly.ChangeAppKey(NewAppKey);
            List<NameValueCollection> queued = QueuedRequests();
            Assert.AreEqual(2, queued.Count);
            Assert.AreEqual(new List<string> { "old_app_event" }, EventKeys(queued[0]));
            Assert.AreEqual(AppKey, queued[0]["app_key"]);
            JObject consent = JObject.Parse(queued[1]["consent"]);
            Assert.AreEqual(NewAppKey, queued[1]["app_key"]);
            Assert.IsTrue(consent.Value<bool>("events"));
            Assert.IsFalse(consent.Value<bool>("sessions"));

            cly.ChangeServerUrl("https://server-b.example");
            queued = QueuedRequests();
            Assert.AreEqual(3, queued.Count);
            Assert.IsNotNull(queued[2]["consent"]);
            Assert.AreEqual(NewAppKey, queued[2]["app_key"]);
            Assert.IsNull(queued.FirstOrDefault(r => r["end_session"] != null || r["begin_session"] != null));
        }

        /// <summary>
        /// Replacing the app keys rewrites every queued request, in memory and in storage, and keeps the other parameters.
        /// </summary>
        [UnityTest]
        public IEnumerator ReplaceAllAppKeysInQueueWithCurrentAppKey_RewritesStoredRequests()
        {
            Countly cly = InitSDK("https://server-a.example", sessions: false);
            cly.RequestHelper._requestRepo.Clear();
            yield return QueueEventRequest("old_app_event");
            cly.ChangeAppKey(NewAppKey);
            yield return QueueEventRequest("new_app_event");
            Assert.AreEqual(new[] { AppKey, NewAppKey }, QueuedRequests().Select(r => r["app_key"]).ToArray());

            cly.ReplaceAllAppKeysInQueueWithCurrentAppKey();
            Assert.AreEqual(new[] { NewAppKey, NewAppKey }, QueuedRequests().Select(r => r["app_key"]).ToArray());

            cly.RequestHelper._requestRepo.RefreshMemoryCache();
            List<NameValueCollection> stored = QueuedRequests();
            Assert.AreEqual(new[] { NewAppKey, NewAppKey }, stored.Select(r => r["app_key"]).ToArray());
            Assert.AreEqual(new List<string> { "old_app_event" }, EventKeys(stored[0]));
            Assert.AreEqual(new List<string> { "new_app_event" }, EventKeys(stored[1]));
            Assert.AreEqual(DeviceId, stored[0]["device_id"]);
            Assert.IsNull(stored[0]["rr"]);
        }

        /// <summary>
        /// Removing the requests of other app keys keeps only the current app key's requests, in memory and in storage.
        /// </summary>
        [UnityTest]
        public IEnumerator RemoveDifferentAppKeysFromQueue_KeepsCurrentAppKeyRequests()
        {
            Countly cly = InitSDK("https://server-a.example", sessions: false);
            cly.RequestHelper._requestRepo.Clear();
            yield return QueueEventRequest("old_app_event_1");
            yield return QueueEventRequest("old_app_event_2");
            cly.ChangeAppKey(NewAppKey);
            yield return QueueEventRequest("new_app_event");
            Assert.AreEqual(3, cly.RequestHelper._requestRepo.Count);

            cly.RemoveDifferentAppKeysFromQueue();
            List<NameValueCollection> queued = QueuedRequests();
            Assert.AreEqual(1, queued.Count);
            Assert.AreEqual(NewAppKey, queued[0]["app_key"]);
            Assert.AreEqual(new List<string> { "new_app_event" }, EventKeys(queued[0]));

            cly.RequestHelper._requestRepo.RefreshMemoryCache();
            Assert.AreEqual(1, cly.RequestHelper._requestRepo.Count);
            Assert.AreEqual(NewAppKey, QueuedRequests()[0]["app_key"]);
        }

        /// <summary>
        /// Flushing clears the request and event queues, in memory and in storage. Timed events, running views
        /// and pending user profile changes are kept.
        /// </summary>
        [UnityTest]
        public IEnumerator FlushQueues_KeepsTimedEventsViewsAndUserProfile()
        {
            Countly cly = InitSDK("https://server-a.example");
            cly.Events.StartEvent("timed_event");
            cly.Views.StartView("running_view");
            yield return cly.Events.RecordEventAsync("queued_event").AsCoroutine();
            cly.UserProfile.SetProperty("pending_property", "value");
            TestUtility.ValidateRQEQSize(cly, 1, 2);

            cly.FlushQueues();
            TestUtility.ValidateRQEQSize(cly, 0, 0);
            cly.RequestHelper._requestRepo.RefreshMemoryCache();
            cly.Events._eventRepo.RefreshMemoryCache();
            TestUtility.ValidateRQEQSize(cly, 0, 0);
            Assert.IsTrue(cly.Events._timedEvents.ContainsKey("timed_event"));

            cly.Views.StopAllViews(null);
            Assert.AreEqual(1, cly.Events._eventRepo.Count);
            cly.UserProfile.Save();
            NameValueCollection userDetails = QueuedRequests().Single(r => r["user_details"] != null);
            StringAssert.Contains("pending_property", userDetails["user_details"]);
        }

        /// <summary>
        /// Recorded events are packed and the queue is sent right away, without waiting for the session timer.
        /// </summary>
        [UnityTest]
        public IEnumerator AttemptToSendStoredRequests_SendsEventsRightAway()
        {
            MockServer server = StartServer();
            Countly cly = InitSDK(server.Url);
            yield return WaitFor(() => server.Count == 1 && cly.RequestHelper._requestRepo.Count == 0, "begin_session sent");
            yield return cly.Events.RecordEventAsync("event_1").AsCoroutine();
            yield return cly.Events.RecordEventAsync("event_2").AsCoroutine();
            TestUtility.ValidateRQEQSize(cly, 0, 2);

            cly.AttemptToSendStoredRequests();
            yield return WaitFor(() => server.Count == 2, "events request at the server");
            Assert.AreEqual(new List<string> { "event_1", "event_2" }, EventKeys(server.Request(1)));
            Assert.AreEqual(AppKey, server.Request(1)["app_key"]);
            yield return WaitFor(() => cly.RequestHelper._requestRepo.Count == 0, "queue empty");
            TestUtility.ValidateRQEQSize(cly, 0, 0);
        }

        /// <summary>
        /// Flushing or removing requests while a request is on its way: the request on its way finishes,
        /// the removed requests are never sent and the requests queued afterwards are neither lost nor sent twice.
        /// </summary>
        [UnityTest]
        public IEnumerator QueueChangesWhileRequestIsOnItsWay()
        {
            MockServer server = StartServer();
            Countly cly = InitSDK(server.Url, sessions: false);
            yield return WaitFor(() => cly.RequestHelper._requestRepo.Count == 0, "init requests sent");
            yield return new WaitForSeconds(1f);
            int before = server.Count;

            server.Hold();
            yield return QueueEventRequest("sent_event");
            yield return QueueEventRequest("flushed_event");
            cly.AttemptToSendStoredRequests();
            yield return WaitFor(() => server.Count == before + 1, "first request at the server");
            cly.FlushQueues();
            yield return QueueEventRequest("queued_after_flush");
            server.Release();
            yield return new WaitForSeconds(1f);

            List<NameValueCollection> queued = QueuedRequests();
            Assert.AreEqual(1, queued.Count);
            Assert.AreEqual(new List<string> { "queued_after_flush" }, EventKeys(queued[0]));
            cly.AttemptToSendStoredRequests();
            yield return WaitFor(() => cly.RequestHelper._requestRepo.Count == 0, "queue sent");
            yield return new WaitForSeconds(1f);
            Assert.AreEqual(new[] { "sent_event", "queued_after_flush" },
                server.Requests().Skip(before).Select(r => EventKeys(r).Single()).ToArray());

            before = server.Count;
            server.Hold();
            yield return QueueEventRequest("old_app_event");
            cly.ChangeAppKey(NewAppKey);
            yield return WaitFor(() => server.Count == before + 1, "old app request at the server");
            yield return QueueEventRequest("new_app_event");
            cly.RemoveDifferentAppKeysFromQueue();
            server.Release();
            yield return new WaitForSeconds(1f);

            queued = QueuedRequests();
            Assert.AreEqual(1, queued.Count);
            Assert.AreEqual(new List<string> { "new_app_event" }, EventKeys(queued[0]));
            cly.AttemptToSendStoredRequests();
            yield return WaitFor(() => cly.RequestHelper._requestRepo.Count == 0, "queue sent");
            yield return new WaitForSeconds(1f);

            List<NameValueCollection> sent = server.Requests().Skip(before).ToList();
            Assert.AreEqual(new[] { "old_app_event", "new_app_event" }, sent.Select(r => EventKeys(r).Single()).ToArray());
            Assert.AreEqual(new[] { AppKey, NewAppKey }, sent.Select(r => r["app_key"]).ToArray());
        }

        /// <summary>
        /// The changed server URL and app key are kept in memory only: a new init uses the configuration again.
        /// </summary>
        [UnityTest]
        public IEnumerator NewInit_UsesConfigurationAgain()
        {
            CountlyConfiguration config = InitSDK("https://server-a.example", sessions: false).Configuration;
            Countly.Instance.ChangeServerUrl("https://server-b.example");
            Countly.Instance.ChangeAppKey(NewAppKey);
            Assert.AreEqual("https://server-b.example/i?", Countly.Instance.Utils.ServerInputUrl);
            Assert.AreEqual(NewAppKey, Countly.Instance.Utils.AppKey);

            Countly.Instance.Session.StopSessionTimer();
            Countly.Instance.CloseDBConnection();
            UnityEngine.Object.DestroyImmediate(Countly.Instance);

            Countly.Instance.Init(config);
            Countly cly = Countly.Instance;
            Assert.AreEqual("https://server-a.example/i?", cly.Utils.ServerInputUrl);
            Assert.AreEqual(AppKey, cly.Utils.AppKey);
            cly.RequestHelper._requestRepo.Clear();
            yield return QueueEventRequest("after_restart");
            Assert.AreEqual(AppKey, QueuedRequests()[0]["app_key"]);
        }

        /// <summary>
        /// Before init, the new calls only log a warning: nothing throws and nothing is changed.
        /// </summary>
        [Test]
        public void NotInitialized_CallsAreIgnored()
        {
            Countly cly = Countly.Instance;
            Assert.IsFalse(cly.IsSDKInitialized);

            Assert.DoesNotThrow(() => {
                cly.ChangeServerUrl("https://server-b.example");
                cly.ChangeAppKey(NewAppKey);
                cly.FlushQueues();
                cly.ReplaceAllAppKeysInQueueWithCurrentAppKey();
                cly.RemoveDifferentAppKeysFromQueue();
                cly.AttemptToSendStoredRequests();
            });
            Assert.IsFalse(cly.IsSDKInitialized);
            Assert.IsNull(cly.Utils);

            InitSDK("https://server-a.example", sessions: false);
            Assert.AreEqual("https://server-a.example/i?", Countly.Instance.Utils.ServerInputUrl);
            Assert.AreEqual(AppKey, Countly.Instance.Utils.AppKey);
        }

        /// <summary>
        /// Cleans up the SDK and stops the local servers.
        /// </summary>
        [SetUp]
        [TearDown]
        public void End()
        {
            TestUtility.TestCleanup();
            foreach (MockServer server in _servers) {
                server.Dispose();
            }
            _servers.Clear();
        }

        /// <summary>
        /// Local HTTP server that answers like a Countly server and records the parameters of every request it gets.
        /// </summary>
        private sealed class MockServer : IDisposable
        {
            internal const string PathKey = "__path";
            internal readonly string Url;
            internal volatile bool Down;
            private readonly HttpListener _listener = new HttpListener();
            private readonly List<NameValueCollection> _requests = new List<NameValueCollection>();
            private readonly ManualResetEventSlim _gate = new ManualResetEventSlim(true);

            /// <summary>
            /// Starts listening on a free local port.
            /// </summary>
            internal MockServer()
            {
                TcpListener portFinder = new TcpListener(IPAddress.Loopback, 0);
                portFinder.Start();
                int port = ((IPEndPoint)portFinder.LocalEndpoint).Port;
                portFinder.Stop();

                Url = "http://127.0.0.1:" + port;
                _listener.Prefixes.Add(Url + "/");
                _listener.Start();
                new Thread(Serve) { IsBackground = true }.Start();
            }

            /// <summary>
            /// Number of received requests.
            /// </summary>
            internal int Count { get { lock (_requests) { return _requests.Count; } } }

            /// <summary>
            /// Returns the parameters of the request at the given index.
            /// </summary>
            internal NameValueCollection Request(int index)
            {
                lock (_requests) { return _requests[index]; }
            }

            /// <summary>
            /// Returns the parameters of all received requests.
            /// </summary>
            internal List<NameValueCollection> Requests()
            {
                lock (_requests) { return new List<NameValueCollection>(_requests); }
            }

            /// <summary>
            /// Keeps the next responses back until Release is called.
            /// </summary>
            internal void Hold()
            {
                _gate.Reset();
            }

            /// <summary>
            /// Sends the responses that were kept back.
            /// </summary>
            internal void Release()
            {
                _gate.Set();
            }

            /// <summary>
            /// Records each request and answers with 200 and a result, or with 503 while the server is down.
            /// </summary>
            private void Serve()
            {
                while (_listener.IsListening) {
                    HttpListenerContext context;
                    try {
                        context = _listener.GetContext();
                    } catch (Exception) {
                        return;
                    }

                    string data = context.Request.Url.Query.TrimStart('?');
                    if (context.Request.HasEntityBody) {
                        using (StreamReader reader = new StreamReader(context.Request.InputStream)) {
                            data = reader.ReadToEnd();
                        }
                    }
                    NameValueCollection parameters = CountlyUtils.ParseQueryString(data);
                    parameters.Add(PathKey, context.Request.Url.AbsolutePath);
                    lock (_requests) {
                        _requests.Add(parameters);
                    }

                    _gate.Wait(TimeSpan.FromSeconds(30));
                    try {
                        byte[] body = Encoding.UTF8.GetBytes("{\"result\":\"Success\"}");
                        context.Response.StatusCode = Down ? 503 : 200;
                        context.Response.ContentType = "application/json";
                        context.Response.ContentLength64 = body.Length;
                        context.Response.OutputStream.Write(body, 0, body.Length);
                        context.Response.Close();
                    } catch (Exception) {
                        // the client went away
                    }
                }
            }

            /// <summary>
            /// Stops the server and lets kept back responses go.
            /// </summary>
            public void Dispose()
            {
                _gate.Set();
                try {
                    _listener.Stop();
                    _listener.Close();
                } catch (Exception) {
                    // already stopped
                }
            }
        }
    }
}
