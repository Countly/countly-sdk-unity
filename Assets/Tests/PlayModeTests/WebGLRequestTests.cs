using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Models;
using UnityEngine.TestTools;

namespace Assets.Tests.PlayModeTests
{
    /// <summary>
    /// The request path WebGL builds use: every request goes as a POST through UnityWebRequest. It is called directly
    /// here, as the editor and the other players send through HttpWebRequest instead.
    /// </summary>
    public class WebGLRequestTests
    {
        private const string AppKey = "772c091355076ead703f987fee94490";
        private const string DeviceId = "webgl_test_device";
        private const string Salt = "webgl-test-salt";
        private readonly List<RecordingServer> _servers = new List<RecordingServer>();

        /// <summary>
        /// A request is a form POST with its parameters and checksum in the body and none in the URL, the same request
        /// the other platforms send, so the checksum covers what the server reads.
        /// </summary>
        [UnityTest]
        public IEnumerator Request_SendsTheParametersAndChecksumInTheBodyOnly()
        {
            RecordingServer server = StartServer();
            Countly.Instance.Init(new CountlyConfiguration(AppKey, server.Url)
                .SetDeviceId(DeviceId)
                .DisableHealthCheck()
                .DisableAutomaticSessionTracking()
                .SetParameterTamperingProtectionSalt(Salt));
            string data = "app_key=" + AppKey + "&device_id=" + DeviceId + "&marker=webgl_probe&events=%5B%5D";

            CountlyResponse response = null;
            yield return Countly.Instance.RequestHelper.ProcessRequestCoroutine(server.Url + "/i?", data, (r) => response = r);

            RecordedRequest request = server.Requests().Single(r => r.Body.Contains("marker=webgl_probe") || r.Query.Contains("marker=webgl_probe"));
            Assert.AreEqual("POST", request.Method);
            Assert.AreEqual("/i", request.Path);
            Assert.AreEqual("", request.Query, "no parameter travels in the URL");
            StringAssert.StartsWith("application/x-www-form-urlencoded", request.ContentType);
            Assert.AreEqual(data + "&checksum256=" + Sha256Hex(data + Salt), request.Body);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess);
        }

        /// <summary>
        /// Without a salt, the body is the request data as it is.
        /// </summary>
        [UnityTest]
        public IEnumerator Request_WithoutSalt_SendsTheParametersInTheBody()
        {
            RecordingServer server = StartServer();
            Countly.Instance.Init(new CountlyConfiguration(AppKey, server.Url)
                .SetDeviceId(DeviceId)
                .DisableHealthCheck()
                .DisableAutomaticSessionTracking());
            string data = "app_key=" + AppKey + "&device_id=" + DeviceId + "&marker=webgl_plain";

            yield return Countly.Instance.RequestHelper.ProcessRequestCoroutine(server.Url + "/i?", data, null);

            RecordedRequest request = server.Requests().Single(r => r.Body.Contains("marker=webgl_plain") || r.Query.Contains("marker=webgl_plain"));
            Assert.AreEqual("", request.Query, "no parameter travels in the URL");
            Assert.AreEqual(data, request.Body);
        }

        /// <summary>
        /// Starts a local server that records every request. It is stopped after the test.
        /// </summary>
        private RecordingServer StartServer()
        {
            RecordingServer server = new RecordingServer();
            _servers.Add(server);
            return server;
        }

        /// <summary>
        /// The SHA-256 of the text as lowercase hex, as the SDK writes the checksum.
        /// </summary>
        private static string Sha256Hex(string text)
        {
            using (SHA256 sha256 = SHA256.Create()) {
                return string.Concat(sha256.ComputeHash(Encoding.UTF8.GetBytes(text)).Select(b => b.ToString("x2")));
            }
        }

        /// <summary>
        /// Cleans up the SDK and stops the local servers.
        /// </summary>
        [SetUp]
        [TearDown]
        public void End()
        {
            TestUtility.TestCleanup();
            foreach (RecordingServer server in _servers) {
                server.Dispose();
            }
            _servers.Clear();
        }

        /// <summary>
        /// One request as the server received it.
        /// </summary>
        private sealed class RecordedRequest
        {
            internal string Method;
            internal string Path;
            internal string Query;
            internal string ContentType;
            internal string Body;
        }

        /// <summary>
        /// Local HTTP server that records the method, path, query, content type and body of every request and answers
        /// it like a Countly server.
        /// </summary>
        private sealed class RecordingServer : IDisposable
        {
            internal readonly string Url;
            private readonly HttpListener _listener = new HttpListener();
            private readonly List<RecordedRequest> _requests = new List<RecordedRequest>();

            /// <summary>
            /// Starts listening on a free local port.
            /// </summary>
            internal RecordingServer()
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
            /// Returns every received request.
            /// </summary>
            internal List<RecordedRequest> Requests()
            {
                lock (_requests) { return new List<RecordedRequest>(_requests); }
            }

            /// <summary>
            /// Records each request and answers it with 200 and a result.
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

                    string body = "";
                    if (context.Request.HasEntityBody) {
                        using (StreamReader reader = new StreamReader(context.Request.InputStream)) {
                            body = reader.ReadToEnd();
                        }
                    }
                    lock (_requests) {
                        _requests.Add(new RecordedRequest {
                            Method = context.Request.HttpMethod,
                            Path = context.Request.Url.AbsolutePath,
                            Query = context.Request.Url.Query.TrimStart('?'),
                            ContentType = context.Request.ContentType ?? "",
                            Body = body
                        });
                    }

                    byte[] answer = Encoding.UTF8.GetBytes("{\"result\":\"Success\"}");
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/json";
                    context.Response.ContentLength64 = answer.Length;
                    context.Response.OutputStream.Write(answer, 0, answer.Length);
                    context.Response.Close();
                }
            }

            /// <summary>
            /// Stops listening.
            /// </summary>
            public void Dispose()
            {
                _listener.Stop();
                _listener.Close();
            }
        }
    }
}
