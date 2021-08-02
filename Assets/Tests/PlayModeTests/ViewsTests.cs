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

            configuration.GiveConsent(new Consents[] { Consents.Views});

            Countly.Instance.Init(configuration);

            Countly.Instance.ClearStorage();
            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            await Countly.Instance.Views.RecordOpenViewAsync("open_view");
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            CountlyEventModel model = Countly.Instance.Views._eventService._eventRepo.Dequeue();

            Assert.AreEqual(CountlyEventModel.ViewEvent, model.Key);
            Assert.IsNull(model.Sum);
            Assert.AreEqual(1, model.Count);
            Assert.IsNull(model.Duration);
            Assert.IsNotNull(model.Segmentation);
            Assert.AreEqual("open_view", model.Segmentation["name"]);

            await Countly.Instance.Views.RecordOpenViewAsync("close_view");
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            model = Countly.Instance.Views._eventService._eventRepo.Dequeue();

            Assert.AreEqual(CountlyEventModel.ViewEvent, model.Key);
            Assert.IsNull(model.Sum);
            Assert.AreEqual(1, model.Count);
            Assert.IsNull(model.Duration);
            Assert.IsNotNull(model.Segmentation);
            Assert.AreEqual("close_view", model.Segmentation["name"]);
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

            Countly.Instance.ClearStorage();
            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            await Countly.Instance.Views.RecordCloseViewAsync("close_view");
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            await Countly.Instance.Views.RecordOpenViewAsync("open_view");
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);


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

            Countly.Instance.ClearStorage();
            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            await Countly.Instance.Views.RecordCloseViewAsync("close_view");
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            CountlyEventModel model = Countly.Instance.Views._eventService._eventRepo.Dequeue();

            Assert.AreEqual(CountlyEventModel.ViewEvent, model.Key);
            Assert.IsNull(model.Sum);
            Assert.AreEqual(1, model.Count);
            Assert.IsNull( model.Duration);
            Assert.IsNotNull(model.Segmentation);
            Assert.AreEqual("close_view", model.Segmentation["name"]);
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

            Countly.Instance.ClearStorage();
            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            await Countly.Instance.Views.RecordOpenViewAsync("open_view");
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            CountlyEventModel model = Countly.Instance.Views._eventService._eventRepo.Dequeue();

            Assert.AreEqual(CountlyEventModel.ViewEvent, model.Key);
            Assert.IsNull(model.Sum);
            Assert.AreEqual(1, model.Count);
            Assert.IsNull(model.Duration);
            Assert.IsNotNull(model.Segmentation);
            Assert.AreEqual("open_view", model.Segmentation["name"]);
            Assert.AreEqual(1, model.Segmentation["visit"]);
            Assert.AreEqual(1, model.Segmentation["start"]);

            await Countly.Instance.Views.RecordOpenViewAsync("open_view_2");
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            model = Countly.Instance.Views._eventService._eventRepo.Dequeue();

            Assert.AreEqual(CountlyEventModel.ViewEvent, model.Key);
            Assert.IsNull(model.Sum);
            Assert.AreEqual(1, model.Count);
            Assert.IsNull(model.Duration);
            Assert.IsNotNull(model.Segmentation);
            Assert.AreEqual("open_view_2", model.Segmentation["name"]);
            Assert.AreEqual(1, model.Segmentation["visit"]);
            Assert.AreEqual(0, model.Segmentation["start"]);

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
            Countly.Instance.ClearStorage();

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            await Countly.Instance.Views.RecordCloseViewAsync("close_view");
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            await Countly.Instance.Views.RecordOpenViewAsync("open_view");
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

            Countly.Instance.ClearStorage();
            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);

            await Countly.Instance.Views.ReportActionAsync("action", 10, 20, 100, 100);
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            CountlyEventModel model = Countly.Instance.Views._eventService._eventRepo.Dequeue();

            Assert.AreEqual(CountlyEventModel.ViewActionEvent, model.Key);
            Assert.IsNull(model.Sum);
            Assert.AreEqual(1, model.Count);
            Assert.IsNull(model.Duration);
            Assert.IsNotNull(model.Segmentation);

            Assert.AreEqual("action", model.Segmentation["type"]);
            Assert.AreEqual(10, model.Segmentation["x"]);
            Assert.AreEqual(20, model.Segmentation["y"]);
            Assert.AreEqual(100, model.Segmentation["width"]);
            Assert.AreEqual(100, model.Segmentation["height"]);
        }

        /// <summary>
        /// It validates the presence of field 'start' in the first view and after device id change without merge.
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

            Countly.Instance.ClearStorage();
            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Views._eventService._eventRepo.Count);
            Assert.IsTrue(Countly.Instance.Views._isFirstView);

            await Countly.Instance.Views.RecordOpenViewAsync("first_view");
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);
            Assert.IsFalse(Countly.Instance.Views._isFirstView);

            CountlyEventModel model = Countly.Instance.Views._eventService._eventRepo.Dequeue();

            Assert.AreEqual(CountlyEventModel.ViewEvent, model.Key);
            Assert.IsNull(model.Sum);
            Assert.AreEqual(1, model.Count);
            Assert.IsNull(model.Duration);
            Assert.IsNotNull(model.Segmentation);
            Assert.AreEqual("first_view", model.Segmentation["name"]);
            Assert.AreEqual(1, model.Segmentation["visit"]);
            Assert.AreEqual(1, model.Segmentation["start"]);

            await Countly.Instance.Device.ChangeDeviceIdAndEndCurrentSessionAsync("new device id");
            Countly.Instance.Views._eventService._eventRepo.Clear();

            Assert.IsTrue(Countly.Instance.Views._isFirstView);
            await Countly.Instance.Views.RecordOpenViewAsync("second_view_open");
            Assert.IsFalse(Countly.Instance.Views._isFirstView);
            Assert.AreEqual(1, Countly.Instance.Views._eventService._eventRepo.Count);

            model = Countly.Instance.Views._eventService._eventRepo.Dequeue();

            Assert.AreEqual(CountlyEventModel.ViewEvent, model.Key);
            Assert.IsNull(model.Sum);
            Assert.AreEqual(1, model.Count);
            Assert.IsNull(model.Duration);
            Assert.IsNotNull(model.Segmentation);
            Assert.AreEqual("second_view_open", model.Segmentation["name"]);
            Assert.AreEqual(1, model.Segmentation["visit"]);
            Assert.AreEqual(1, model.Segmentation["start"]);
        }

        [TearDown]
        public void End()
        {
            Countly.Instance.ClearStorage();
            Object.DestroyImmediate(Countly.Instance);
        }
    }
}
