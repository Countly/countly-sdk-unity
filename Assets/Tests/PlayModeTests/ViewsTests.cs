using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Enums;
using System.Threading.Tasks;

namespace Tests
{
    public class ViewsTests
    {
        private readonly string _serverUrl = "https://xyz.com/";
        private readonly string _appKey = "772c091355076ead703f987fee94490";

        private void ValidateViewEvent(CountlyEventModel model, string name, bool isOpenView, int start = 1, bool isAction = false)
        {
            Assert.IsNull(model.Sum);
            Assert.AreEqual(1, model.Count);
            Assert.IsNull(model.Duration);
            Assert.IsNotNull(model.Segmentation);

            if (isAction) {
                Assert.AreEqual(CountlyEventModel.ViewActionEvent, model.Key);
            } else {
                Assert.AreEqual(CountlyEventModel.ViewEvent, model.Key);
                Assert.AreEqual(name, model.Segmentation["name"]);
            }

            if (isOpenView) {
                Assert.AreEqual(1, model.Segmentation["visit"]);
                Assert.AreEqual(start, model.Segmentation["start"]);
            }
        }

        /// <summary>
        /// It validates the event repository initial state.
        /// </summary>
        [Test]
        public void TestViewsRepoInitialState()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

        }

        /// <summary>
        /// It validates the dependency of 'Event Consent'.
        /// </summary>
        [Test]
        public async void TestViews_CheckEventConsentDependency()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                RequiresConsent = true,
            };

            configuration.GiveConsent(new Consents[] { Consents.Views });

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            await Countly.Instance.Views.RecordOpenViewAsync("open_view");
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            CountlyEventModel model = Countly.Instance.Views._eventService._eventRepo.Dequeue();
            ValidateViewEvent(model, "open_view", true);

            await Countly.Instance.Views.RecordOpenViewAsync("close_view");
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            model = Countly.Instance.Views._eventService._eventRepo.Dequeue();
            ValidateViewEvent(model, "close_view", false);
        }

        /// <summary>
        /// It checks the working of views service if no views consent is given.
        /// </summary>
        [Test]
        public async void TestViewsConsent()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                RequiresConsent = true
            };


            Countly.Instance.Init(configuration);

            Countly.Instance.Views._eventService._eventRepo.Clear();
            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            await Countly.Instance.Views.RecordCloseViewAsync("close_view");
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            await Countly.Instance.Views.RecordOpenViewAsync("open_view");
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);
        }

        /// <summary>
        /// It validates the limit of the view's name size.
        /// </summary>
        [Test]
        public async void TestViewNameLimit()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                MaxKeyLength = 5
            };

            Countly.Instance.Init(configuration);

            Countly.Instance.ClearStorage();
            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            await Countly.Instance.Views.RecordOpenViewAsync("open_view", false);
            await Countly.Instance.Views.RecordCloseViewAsync("close_view", false);
            Assert.AreEqual(2, Countly.Instance.Views._eventService._eventRepo.Count);

            CountlyEventModel model = Countly.Instance.Views._eventService._eventRepo.Dequeue();
            ValidateViewEvent(model, "open_", true);

            model = Countly.Instance.Views._eventService._eventRepo.Dequeue();
            ValidateViewEvent(model, "close", false);
        }

        /// <summary>
        /// It validates functionality of method 'RecordCloseViewAsync'.
        /// </summary>
        [Test]
        public async void TestViewMethod_RecordCloseViewAsync()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            await Countly.Instance.Views.RecordCloseViewAsync("close_view");
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            CountlyEventModel model = Countly.Instance.Views._eventService._eventRepo.Dequeue();
            ValidateViewEvent(model, "close_view", false);
        }

        /// <summary>
        /// It validates functionality of method 'RecordOpenViewAsync'.
        /// </summary>
        [Test]
        public async void TestViewsMethod_RecordOpenViewAsync()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            await Countly.Instance.Views.RecordOpenViewAsync("open_view");
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            CountlyEventModel model = Countly.Instance.Views._eventService._eventRepo.Dequeue();
            ValidateViewEvent(model, "open_view", true);


            await Countly.Instance.Views.RecordOpenViewAsync("open_view_2");
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            model = Countly.Instance.Views._eventService._eventRepo.Dequeue();
            ValidateViewEvent(model, "open_view_2", true, 0);
        }

        /// <summary>
        /// It validates functionality of method 'RecordOpenViewAsync' while recording view with segmentations.
        /// </summary>
        [Test]
        public async void TestViewsMethod_RecordOpenViewAsyncWithSegment()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);        

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            Dictionary<string, object> segmentations = new Dictionary<string, object>();
            segmentations.Add("name", "new_open_view"); // override name
            segmentations.Add("key1", "value1"); 
            segmentations.Add("key2", null); // invalid value
            segmentations.Add("", "value2"); // invalid key
            segmentations.Add("visit", null); // override existing key with invalid value

            await Countly.Instance.Views.RecordOpenViewAsync("open_view", segmentations);
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            CountlyEventModel model = Countly.Instance.Views._eventService._eventRepo.Dequeue();

            ValidateViewEvent(model, "open_view", true);
            Assert.AreEqual("value1", model.Segmentation["key1"]);
            Assert.IsFalse(model.Segmentation.ContainsKey("key2"));
            Assert.IsFalse(model.Segmentation.ContainsKey(""));

            await Countly.Instance.Views.RecordOpenViewAsync("open_view_2");
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            model = Countly.Instance.Views._eventService._eventRepo.Dequeue();

            ValidateViewEvent(model, "open_view_2", true, 0);
        }

        /// <summary>
        /// It validates 'EventQueueThreshold' limit.
        /// </summary>
        [Test]
        public async void TestEvent_EventQueueThreshold_Limit()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                EventQueueThreshold = 3
            };

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            await Countly.Instance.Views.RecordOpenViewAsync("open_view");
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            await Countly.Instance.Views.RecordCloseViewAsync("open_view");
            Assert.AreEqual(2, Countly.Instance.Views._eventService._eventRepo.Count);

            await Countly.Instance.Views.ReportActionAsync("action", 10, 10, 100, 100);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);
        }

        /// <summary>
        /// It validates functionality of method 'ReportActionAsync'.
        /// </summary>
        [Test]
        public async void TestViewsMethod_ReportActionAsync()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            await Countly.Instance.Views.ReportActionAsync("action", 10, 20, 100, 100);
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            CountlyEventModel model = Countly.Instance.Views._eventService._eventRepo.Dequeue();
            ValidateViewEvent(model, "", false, 0, true);

            Assert.AreEqual("action", model.Segmentation["type"]);
            Assert.AreEqual(10, model.Segmentation["x"]);
            Assert.AreEqual(20, model.Segmentation["y"]);
            Assert.AreEqual(100, model.Segmentation["width"]);
            Assert.AreEqual(100, model.Segmentation["height"]);
        }

        /// <summary>
        /// It validates the presence of field 'start' in the first view.
        /// </summary>
        [Test]
        public async void TestStartField()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                RequiresConsent = true
            };

            configuration.GiveConsent(new Consents[] { Consents.Crashes, Consents.Events, Consents.Clicks, Consents.StarRating, Consents.Views, Consents.Users, Consents.Push, Consents.RemoteConfig, Consents.Location });
            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);
            Assert.IsTrue(Countly.Instance.Views._isFirstView);

            await Countly.Instance.Views.RecordOpenViewAsync("first_view");
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);
            Assert.IsFalse(Countly.Instance.Views._isFirstView);

            CountlyEventModel model = Countly.Instance.Views._eventService._eventRepo.Dequeue();

            ValidateViewEvent(model, "first_view", true);
        }

        /// <summary>
        /// It validates the presence of field 'start' in the first view after device id change without merge.
        /// </summary>
        [Test]
        public async void TestStartField_AfterDeviceIdChangeWithoutMerge()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                RequiresConsent = true
            };

            configuration.GiveConsent(new Consents[] { Consents.Crashes, Consents.Events, Consents.Clicks, Consents.StarRating, Consents.Views, Consents.Users, Consents.Push, Consents.RemoteConfig, Consents.Location });
            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);
            Assert.IsTrue(Countly.Instance.Views._isFirstView);

            await Countly.Instance.Views.RecordOpenViewAsync("first_view");
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);
            Assert.IsFalse(Countly.Instance.Views._isFirstView);

            await Countly.Instance.Device.ChangeDeviceIdWithoutMerge("new device id");
            Countly.Instance.Views._eventService._eventRepo.Clear();

            Assert.IsTrue(Countly.Instance.Views._isFirstView);
            await Countly.Instance.Views.RecordOpenViewAsync("second_view_open");

            Assert.IsTrue(Countly.Instance.Views._isFirstView);
            Assert.IsFalse(Countly.Instance.Consents.CheckConsent(Consents.Views));
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);
        }

        [TearDown]
        public void End()
        {
            Countly.Instance.ClearStorage();
            Object.DestroyImmediate(Countly.Instance);
        }
    }
}
