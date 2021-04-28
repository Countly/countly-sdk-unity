﻿using System.Collections;
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
            Assert.AreEqual(0, Countly.Instance.Views._eventService._viewEventRepo.Count);

        }

        /// <summary>
        /// It checks the working of event service if no event consent is given.
        /// </summary>
        [Test]
        public async void TestEventConsent()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                RequiresConsent = true
            };

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Views);
            Assert.AreEqual(0, Countly.Instance.Events._viewEventRepo.Count);
            Assert.AreEqual(0, Countly.Instance.Events._nonViewEventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("test_event");
            Assert.AreEqual(0, Countly.Instance.Events._nonViewEventRepo.Count);

            await Countly.Instance.Events.ReportCustomEventAsync("test_event", segmentation: null, sum: 23, duration: 5);
            Assert.AreEqual(0, Countly.Instance.Events._nonViewEventRepo.Count);


        }

        /// <summary>
        /// It validates functionality of method 'RecordEventAsync'.
        /// </summary>
        [Test]
        public async void TestEventMethod_RecordEventAsync()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Events);
            Assert.AreEqual(0, Countly.Instance.Events._viewEventRepo.Count);
            Assert.AreEqual(0, Countly.Instance.Events._nonViewEventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("test_event");
            Assert.AreEqual(1, Countly.Instance.Events._nonViewEventRepo.Count);

            CountlyEventModel model = Countly.Instance.Events._nonViewEventRepo.Dequeue();

            Assert.AreEqual("test_event", model.Key);
            Assert.AreEqual(0, model.Sum);
            Assert.AreEqual(1, model.Count);
            Assert.IsNull( model.Duration);
            Assert.IsNull(model.Segmentation);
        }

        /// <summary>
        /// It validates the working of event service if 'EventQueueThreshold' limit reach.
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

            Assert.IsNotNull(Countly.Instance.Events);
            Assert.AreEqual(0, Countly.Instance.Events._viewEventRepo.Count);
            Assert.AreEqual(0, Countly.Instance.Events._nonViewEventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("test_event_1");
            Assert.AreEqual(1, Countly.Instance.Events._nonViewEventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("test_event_2");
            Assert.AreEqual(2, Countly.Instance.Events._nonViewEventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("test_event_3");
            Assert.AreEqual(0, Countly.Instance.Events._nonViewEventRepo.Count);


        }

        /// <summary>
        /// It validates functionality of method 'ReportCustomEventAsync'.
        /// </summary>
        [Test]
        public async void TestEventMethod_ReportCustomEventAsync()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Events);
            Assert.AreEqual(0, Countly.Instance.Events._viewEventRepo.Count);
            Assert.AreEqual(0, Countly.Instance.Events._nonViewEventRepo.Count);


            Dictionary<string, object> segments = new Dictionary<string, object>{
            { "key1", "value1"},
            { "key2", "value2"}
            };

            SegmentModel segmentModel = new SegmentModel(segments);

            await Countly.Instance.Events.RecordEventAsync("test_event", segmentation: segmentModel, sum: 23, duration: 5);

            CountlyEventModel model = Countly.Instance.Events._nonViewEventRepo.Dequeue();

            Assert.AreEqual("test_event", model.Key);
            Assert.AreEqual(23, model.Sum);
            Assert.AreEqual(1, model.Count);
            Assert.AreEqual(5, model.Duration);
            Assert.AreEqual(2, model.Segmentation.Count);
            Assert.AreEqual("value1", model.Segmentation["key1"]);
            Assert.AreEqual("value2", model.Segmentation["key2"]);
        }

        /// <summary>
        /// It validates the data type of segment items.
        /// </summary>
        [Test]
        public async void TestSegmentItemsDataTypesValidation()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Events);
            Assert.AreEqual(0, Countly.Instance.Events._viewEventRepo.Count);
            Assert.AreEqual(0, Countly.Instance.Events._nonViewEventRepo.Count);


            Dictionary<string, object> segments = new Dictionary<string, object>{
            { "key1", "value1"},
            { "key2", 1},
            { "key3", 10.0},
            { "key4", true},
            { "key5", null},// invalid
            { "key6", Countly.Instance} // invalid
            };

            SegmentModel segmentModel = new SegmentModel(segments);

            await Countly.Instance.Events.RecordEventAsync("test_event", segmentation: segmentModel, sum: 23, duration: 5);

            CountlyEventModel model = Countly.Instance.Events._nonViewEventRepo.Dequeue();

            Assert.AreEqual("test_event", model.Key);
            Assert.AreEqual(23, model.Sum);
            Assert.AreEqual(1, model.Count);
            Assert.AreEqual(5, model.Duration);
            Assert.AreEqual(4, model.Segmentation.Count);
            Assert.AreEqual(true, model.Segmentation.ContainsKey("key1"));
            Assert.AreEqual(true, model.Segmentation.ContainsKey("key2"));
            Assert.AreEqual(true, model.Segmentation.ContainsKey("key3"));
            Assert.AreEqual(true, model.Segmentation.ContainsKey("key4"));
            Assert.AreEqual(false, model.Segmentation.ContainsKey("key5"));
            Assert.AreEqual(false, model.Segmentation.ContainsKey("key6"));

        }

        /// <summary>
        /// It validates the mandatory and optional parameters of events.
        /// </summary>
        [Test]
        public async void TestEventsParameters()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Events);
            Assert.AreEqual(0, Countly.Instance.Events._viewEventRepo.Count);
            Assert.AreEqual(0, Countly.Instance.Events._nonViewEventRepo.Count);


            Dictionary<string, object> segments = new Dictionary<string, object>{
            { "key1", "value1"},
            { "key2", "value2"}
            };

            SegmentModel segmentModel = new SegmentModel(segments);


            await Countly.Instance.Events.RecordEventAsync("", segmentation: segmentModel, sum: 23, duration: 5);
            Assert.AreEqual(0, Countly.Instance.Events._nonViewEventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("");
            Assert.AreEqual(0, Countly.Instance.Events._nonViewEventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync(null, segmentation: segmentModel, sum: 23, duration: 5);
            Assert.AreEqual(0, Countly.Instance.Events._nonViewEventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync(" ");
            Assert.AreEqual(0, Countly.Instance.Events._nonViewEventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("key", segmentation: null, sum: 23, duration: 5);
            Assert.AreEqual(1, Countly.Instance.Events._nonViewEventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("key", segmentation: segmentModel, sum: 23, duration: null);
            Assert.AreEqual(2, Countly.Instance.Events._nonViewEventRepo.Count);
        }

        [TearDown]
        public void End()
        {
            Countly.Instance.ClearStorage();
            Object.DestroyImmediate(Countly.Instance);
        }
    }
}
