using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Models;
using UnityEngine.TestTools;

namespace Assets.Tests.PlayModeTests
{
    public class LogListenerTests
    {
        /// <summary>
        /// Every level reaches the listener while console logging is disabled, as the tag and the message without the level.
        /// </summary>
        [Test]
        public void ListenerReceivesEveryLevelWhileLoggingIsDisabled()
        {
            List<KeyValuePair<string, LogLevel>> received = new List<KeyValuePair<string, LogLevel>>();
            CountlyConfiguration configuration = TestUtility.CreateBaseConfig()
                .SetLogListener((message, level) => received.Add(new KeyValuePair<string, LogLevel>(message, level)));
            Assert.IsFalse(configuration.IsLoggingEnabled());

            CountlyLogHelper logHelper = new CountlyLogHelper(configuration);
            logHelper.Error("[LogListenerTests] error line");
            logHelper.Warning("[LogListenerTests] warning line");
            logHelper.Info("[LogListenerTests] info line");
            logHelper.Debug("[LogListenerTests] debug line");
            logHelper.Verbose("[LogListenerTests] verbose line");

            Assert.AreEqual(5, received.Count);
            Assert.AreEqual(new KeyValuePair<string, LogLevel>("[Countly][LogListenerTests] error line", LogLevel.Error), received[0]);
            Assert.AreEqual(new KeyValuePair<string, LogLevel>("[Countly][LogListenerTests] warning line", LogLevel.Warning), received[1]);
            Assert.AreEqual(new KeyValuePair<string, LogLevel>("[Countly][LogListenerTests] info line", LogLevel.Info), received[2]);
            Assert.AreEqual(new KeyValuePair<string, LogLevel>("[Countly][LogListenerTests] debug line", LogLevel.Debug), received[3]);
            Assert.AreEqual(new KeyValuePair<string, LogLevel>("[Countly][LogListenerTests] verbose line", LogLevel.Verbose), received[4]);
        }

        /// <summary>
        /// The SDK's own log lines reach the listener from init on, and a log helper without a listener keeps working.
        /// </summary>
        [Test]
        public void ListenerReceivesSdkLogLines()
        {
            List<KeyValuePair<string, LogLevel>> received = new List<KeyValuePair<string, LogLevel>>();
            CountlyConfiguration configuration = TestUtility.CreateBaseConfig()
                .SetLogListener((message, level) => received.Add(new KeyValuePair<string, LogLevel>(message, level)));
            Countly.Instance.Init(configuration);

            Assert.IsTrue(Countly.Instance.IsSDKInitialized);
            Assert.IsTrue(received.Exists(line => line.Value == LogLevel.Info && line.Key.StartsWith("[Countly][Countly] InitInternal, Initializing Countly")));

            CountlyLogHelper withoutListener = new CountlyLogHelper(TestUtility.CreateBaseConfig());
            withoutListener.Info("[LogListenerTests] line without a listener");
        }

        /// <summary>
        /// A listener that throws does not stop the SDK from initializing and recording events.
        /// </summary>
        [UnityTest]
        public IEnumerator ThrowingListenerDoesNotBreakTheSdk()
        {
            int calls = 0;
            CountlyConfiguration configuration = TestUtility.CreateBaseConfig()
                .SetLogListener((message, level) => {
                    calls++;
                    throw new InvalidOperationException("log listener failure");
                });
            Countly.Instance.Init(configuration);
            Assert.IsTrue(Countly.Instance.IsSDKInitialized);

            int eventsBefore = Countly.Instance.Events._eventRepo.Count;
            yield return Countly.Instance.Events.RecordEventAsync("after_listener_threw").AsCoroutine();
            Assert.AreEqual(eventsBefore + 1, Countly.Instance.Events._eventRepo.Count);
            Assert.Greater(calls, 1);
        }

        /// <summary>
        /// Clears the SDK's storage and instance before and after every test.
        /// </summary>
        [SetUp]
        [TearDown]
        public void End()
        {
            TestUtility.TestCleanup();
        }
    }
}
