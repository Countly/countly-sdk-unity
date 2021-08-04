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
    public class EventTests
    {
        private readonly string _serverUrl = "https://xyz.com/";
        private readonly string _appKey = "772c091355076ead703f987fee94490";

        /// <summary>
        /// It validates the event repository initial state.
        /// </summary>
        [Test]
        public void TestEventRepoInitialState()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);
            Assert.IsNotNull(Countly.Instance.Events);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

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

            Assert.IsNotNull(Countly.Instance.Events);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);


            await Countly.Instance.Events.RecordEventAsync("test_event");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("test_event", segmentation: null, sum: 23, duration: 5);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
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
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("test_event");
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            CountlyEventModel model = Countly.Instance.Events._eventRepo.Dequeue();

            Assert.AreEqual("test_event", model.Key);
            Assert.AreEqual(0, model.Sum);
            Assert.AreEqual(1, model.Count);
            Assert.IsNull(model.Duration);
            Assert.IsNull(model.Segmentation);

            await Countly.Instance.Events.RecordEventAsync("test_event1", segmentation: null, count: 5, duration: null, sum: null);
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            CountlyEventModel model1 = Countly.Instance.Events._eventRepo.Dequeue();

            Assert.AreEqual("test_event1", model1.Key);
            Assert.AreEqual(5, model1.Count);
            Assert.IsNull(model1.Sum);
            Assert.IsNull(model1.Duration);
            Assert.IsNull(model1.Segmentation);
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
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("test_event_1");
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("test_event_2");
            Assert.AreEqual(2, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("test_event_3");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);


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

            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);


            Dictionary<string, object> segments = new Dictionary<string, object>{
            { "key1", "value1"},
            { "key2", "value2"}
            };

            SegmentModel segmentModel = new SegmentModel(segments);

            await Countly.Instance.Events.RecordEventAsync("test_event", segmentation: segmentModel, sum: 23, duration: 5);

            CountlyEventModel model = Countly.Instance.Events._eventRepo.Dequeue();

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
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            Dictionary<string, object> segments = new Dictionary<string, object>{
            { "key1", "value1"},
            { "key2", 1},
            { "key3", 10.0},
            { "key4", true},
            { "key5", null},// invalid
            { "key6", Countly.Instance} // invalid
            };

            await Countly.Instance.Events.RecordEventAsync("test_event", segmentation: segments, sum: 23, duration: 5);

            CountlyEventModel model = Countly.Instance.Events._eventRepo.Dequeue();

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
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);


            Dictionary<string, object> segments = new Dictionary<string, object>{
            { "key1", "value1"},
            { "key2", "value2"}
            };


            await Countly.Instance.Events.RecordEventAsync("", segmentation: segments, sum: 23, duration: 5);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync(null, segmentation: segments, sum: 23, duration: 5);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync(" ");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("key", segmentation: null, sum: 23, duration: 5);
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("key", segmentation: segments, sum: 23, duration: null);
            Assert.AreEqual(2, Countly.Instance.Events._eventRepo.Count);
        }


        /// <summary>
        /// It validates specific keys consents.
        /// </summary>
        [Test]
        public async void TestSpecificKeysConsent()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                RequiresConsent = true,
            };

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Events);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("event");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_view");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_nps");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_action");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_survey");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_push_action");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_orientation");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_star_rating");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

        }

        /// <summary>
        /// It validates 'recordEvent' against view specific key '[CLY]_view'.
        /// </summary>
        [Test]
        public async void TestEventMethod_RecordEventAgaisntViewKey()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                RequiresConsent = true,
            };

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Events);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);


            //[CLY]_view
            Countly.Instance.Consents.GiveConsent(new Consents[] { Consents.Views });

            await Countly.Instance.Events.RecordEventAsync("event");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);


            await Countly.Instance.Events.RecordEventAsync("[CLY]_action");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_push_action");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);


            await Countly.Instance.Events.RecordEventAsync("[CLY]_nps");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_orientation");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_survey");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_star_rating");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_view");
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            CountlyEventModel model = Countly.Instance.Events._eventRepo.Dequeue();

            Assert.AreEqual("[CLY]_view", model.Key);
            Assert.AreEqual(0, model.Sum);
            Assert.AreEqual(1, model.Count);
            Assert.IsNull(model.Duration);
            Assert.IsNull(model.Segmentation);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_view", segmentation: null, count: 5, duration: null, sum: 1);
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            model = Countly.Instance.Events._eventRepo.Dequeue();

            Assert.AreEqual("[CLY]_view", model.Key);
            Assert.AreEqual(1, model.Sum);
            Assert.AreEqual(5, model.Count);
            Assert.IsNull(model.Duration);
            Assert.IsNull(model.Segmentation);

        }

        /// <summary>
        /// It validates 'recordEvent' against action specific key '[CLY]_action'.
        /// </summary>
        [Test]
        public async void TestEventMethod_RecordEventAgaisntActionKey()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                RequiresConsent = true,
            };

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Events);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            Countly.Instance.Consents.GiveConsent(new Consents[] { Consents.Clicks });

            await Countly.Instance.Events.RecordEventAsync("event");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_push_action");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);


            await Countly.Instance.Events.RecordEventAsync("[CLY]_nps");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_orientation");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_survey");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_view");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_star_rating");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_action");
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            CountlyEventModel model = Countly.Instance.Events._eventRepo.Dequeue();

            Assert.AreEqual("[CLY]_action", model.Key);
            Assert.AreEqual(0, model.Sum);
            Assert.AreEqual(1, model.Count);
            Assert.IsNull(model.Duration);
            Assert.IsNull(model.Segmentation);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_action", segmentation: null, count: 5, duration: null, sum: 1);
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            model = Countly.Instance.Events._eventRepo.Dequeue();

            Assert.AreEqual("[CLY]_action", model.Key);
            Assert.AreEqual(1, model.Sum);
            Assert.AreEqual(5, model.Count);
            Assert.IsNull(model.Duration);
            Assert.IsNull(model.Segmentation);
        }

        /// <summary>
        /// It validates 'recordEvent' against action specific key '[CLY]_star_rating'.
        /// </summary>
        [Test]
        public async void TestEventMethod_RecordEventAgaisntStarRatingKey()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                RequiresConsent = true,
            };

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Events);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            Countly.Instance.Consents.GiveConsent(new Consents[] { Consents.StarRating });

            await Countly.Instance.Events.RecordEventAsync("event");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_push_action");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);


            await Countly.Instance.Events.RecordEventAsync("[CLY]_nps");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_orientation");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_survey");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_view");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_action");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_star_rating");
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            CountlyEventModel model = Countly.Instance.Events._eventRepo.Dequeue();

            Assert.AreEqual("[CLY]_star_rating", model.Key);
            Assert.AreEqual(0, model.Sum);
            Assert.AreEqual(1, model.Count);
            Assert.IsNull(model.Duration);
            Assert.IsNull(model.Segmentation);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_star_rating", segmentation: null, count: 5, duration: null, sum: 1);
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            model = Countly.Instance.Events._eventRepo.Dequeue();

            Assert.AreEqual("[CLY]_star_rating", model.Key);
            Assert.AreEqual(1, model.Sum);
            Assert.AreEqual(5, model.Count);
            Assert.IsNull(model.Duration);
            Assert.IsNull(model.Segmentation);
        }

        /// <summary>
        /// It validates 'recordEvent' against push specific key '[CLY]_push_action'.
        /// </summary>
        [Test]
        public async void TestEventMethod_RecordEventAgaisntPushActionKey()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                RequiresConsent = true,
            };

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Events);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            //[CLY]_push_action
            Countly.Instance.Consents.GiveConsent(new Consents[] { Consents.Push });

            await Countly.Instance.Events.RecordEventAsync("event");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_nps");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_orientation");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_survey");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_view");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_star_rating");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_action");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_push_action");
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            CountlyEventModel model = Countly.Instance.Events._eventRepo.Dequeue();

            Assert.AreEqual("[CLY]_push_action", model.Key);
            Assert.AreEqual(0, model.Sum);
            Assert.AreEqual(1, model.Count);
            Assert.IsNull(model.Duration);
            Assert.IsNull(model.Segmentation);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_push_action", segmentation: null, count: 5, duration: null, sum: 1);
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            model = Countly.Instance.Events._eventRepo.Dequeue();

            Assert.AreEqual("[CLY]_push_action", model.Key);
            Assert.AreEqual(1, model.Sum);
            Assert.AreEqual(5, model.Count);
            Assert.IsNull(model.Duration);
            Assert.IsNull(model.Segmentation);
        }
        /// <summary>
        /// It validates 'recordEvent' against push specific key '[CLY]_orientation'.
        /// </summary>
        [Test]
        public async void TestEventMethod_RecordEventAgaisntOrientationKey()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                RequiresConsent = true,
            };

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Events);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            Countly.Instance.Consents.GiveConsent(new Consents[] { Consents.Users });

            await Countly.Instance.Events.RecordEventAsync("event");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_view");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_action");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_Push_Action");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_push_action");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_star_rating");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_survey");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_orientation");
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            CountlyEventModel model = Countly.Instance.Events._eventRepo.Dequeue();

            Assert.AreEqual("[CLY]_orientation", model.Key);
            Assert.AreEqual(0, model.Sum);
            Assert.AreEqual(1, model.Count);
            Assert.IsNull(model.Duration);
            Assert.IsNull(model.Segmentation);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_orientation", segmentation: null, count: 5, duration: null, sum: 1);
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            model = Countly.Instance.Events._eventRepo.Dequeue();

            Assert.AreEqual("[CLY]_orientation", model.Key);
            Assert.AreEqual(1, model.Sum);
            Assert.AreEqual(5, model.Count);
            Assert.IsNull(model.Duration);
            Assert.IsNull(model.Segmentation);

        }

        /// <summary>
        /// It validates 'recordEvent' against nps([CLY]_nps) and survey([CLY]_survey) specific keys.
        /// </summary>
        [Test]
        public async void TestEventMethod_RecordEventAsyncWithSpecificKeys()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                RequiresConsent = true,
            };

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Events);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            Countly.Instance.Consents.GiveConsent(new Consents[] { Consents.Feedback });

            await Countly.Instance.Events.RecordEventAsync("event");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_view");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_action");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_Push_Action");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_push_action");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_star_rating");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_orientation");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_survey");
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            CountlyEventModel model = Countly.Instance.Events._eventRepo.Dequeue();

            Assert.AreEqual("[CLY]_survey", model.Key);
            Assert.AreEqual(0, model.Sum);
            Assert.AreEqual(1, model.Count);
            Assert.IsNull(model.Duration);
            Assert.IsNull(model.Segmentation);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_survey", segmentation: null, count: 5, duration: null, sum: 1);
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            model = Countly.Instance.Events._eventRepo.Dequeue();

            Assert.AreEqual("[CLY]_survey", model.Key);
            Assert.AreEqual(1, model.Sum);
            Assert.AreEqual(5, model.Count);
            Assert.IsNull(model.Duration);
            Assert.IsNull(model.Segmentation);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_nps");
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            model = Countly.Instance.Events._eventRepo.Dequeue();

            Assert.AreEqual("[CLY]_nps", model.Key);
            Assert.AreEqual(0, model.Sum);
            Assert.AreEqual(1, model.Count);
            Assert.IsNull(model.Duration);
            Assert.IsNull(model.Segmentation);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_nps", segmentation: null, count: 5, duration: null, sum: 1);
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            model = Countly.Instance.Events._eventRepo.Dequeue();

            Assert.AreEqual("[CLY]_nps", model.Key);
            Assert.AreEqual(1, model.Sum);
            Assert.AreEqual(5, model.Count);
            Assert.IsNull(model.Duration);
            Assert.IsNull(model.Segmentation);
        }
        /// <summary>
        /// It validates 'recordEvent' against specific event keys.
        /// </summary>
        [Test]
        public async void TestEventMethod_RecordEventAgainstSpecificEventKeys()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                RequiresConsent = true,
            };

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Events);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            Countly.Instance.Consents.GiveConsent(new Consents[] { Consents.Events });

            await Countly.Instance.Events.RecordEventAsync("[CLY]_view");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_action");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_nps");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_survey");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_orientation");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_star_rating");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);


            await Countly.Instance.Events.RecordEventAsync("[CLY]_push_action");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("event");
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            CountlyEventModel model = Countly.Instance.Events._eventRepo.Dequeue();

            Assert.AreEqual("event", model.Key);
            Assert.AreEqual(0, model.Sum);
            Assert.AreEqual(1, model.Count);
            Assert.IsNull(model.Duration);
            Assert.IsNull(model.Segmentation);

            await Countly.Instance.Events.RecordEventAsync("event", segmentation: null, count: 5, duration: null, sum: 1);
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            model = Countly.Instance.Events._eventRepo.Dequeue();

            Assert.AreEqual("event", model.Key);
            Assert.AreEqual(1, model.Sum);
            Assert.AreEqual(5, model.Count);
            Assert.IsNull(model.Duration);
            Assert.IsNull(model.Segmentation);
        }

        [TearDown]
        public void End()
        {
            Countly.Instance.ClearStorage();
            Object.DestroyImmediate(Countly.Instance);
        }
    }
}
