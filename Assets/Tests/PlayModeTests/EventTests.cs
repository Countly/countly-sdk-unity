using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Enums;

namespace Assets.Tests.PlayModeTests
{
    public class EventTests
    {
        private readonly string _serverUrl = "https://xyz.com/";
        private readonly string _appKey = "772c091355076ead703f987fee94490";

        private void AssertAnEvent(CountlyEventModel model, string name, double? sum, double count, double? duration, IDictionary<string, object> segmentation)
        {
            Assert.AreEqual(name, model.Key);
            Assert.AreEqual(model.Sum, sum);
            Assert.AreEqual(model.Count, count);

            if (duration != null) {
                Assert.IsTrue(duration <= model.Duration);
            } else {
                Assert.IsNull(model.Duration);
            }

            if (segmentation != null) {
                foreach (KeyValuePair<string, object> entry in model.Segmentation) {
                    Assert.AreEqual(segmentation[entry.Key], entry.Value);
                }

                Assert.AreEqual(segmentation.Count, model.Segmentation.Count);

            } else {
                Assert.IsNull(model.Segmentation);
            }
        }

        /// <summary>
        /// It validates the event repository initial state.
        /// </summary>
        [Test]
        public void TestEventRepoInitialState()
        {
            Countly.Instance.Init(TestUtility.CreateBaseConfig());
            Assert.IsNotNull(Countly.Instance.Events);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

        }

        /// <summary>
        /// It checks the working of event service if no event consent is given.
        /// </summary>
        [Test]
        public async void TestEventConsent()
        {
            CountlyConfiguration configuration = TestUtility.CreateBaseConfig()
                .SetRequiresConsent(true);
            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Events);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
            Assert.AreEqual(0, Countly.Instance.Events._timedEvents.Count);

            await Countly.Instance.Events.RecordEventAsync("test_event");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
            Assert.AreEqual(0, Countly.Instance.Events._timedEvents.Count);

            await Countly.Instance.Events.RecordEventAsync("test_event", segmentation: null, sum: 23, duration: 5);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
            Assert.AreEqual(0, Countly.Instance.Events._timedEvents.Count);

            Countly.Instance.Events.StartEvent("test_event");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
            Assert.AreEqual(0, Countly.Instance.Events._timedEvents.Count);
        }

        /// <summary>
        /// It validates the cancelation of timed events on consent removal.
        /// </summary>
        [Test]
        public void TestTimedEventsCancelationOnConsentRemoval()
        {
            CountlyConfiguration configuration = TestUtility.CreateBaseConfig()
                .SetRequiresConsent(true);
            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Events);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            Countly.Instance.Events.StartEvent("test_event");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
            Assert.AreEqual(0, Countly.Instance.Events._timedEvents.Count);

            Countly.Instance.Consents.GiveConsent(new Consents[] { Consents.Events });

            Countly.Instance.Events.StartEvent("test_event");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
            Assert.AreEqual(1, Countly.Instance.Events._timedEvents.Count);

            Countly.Instance.Events.StartEvent("test_event_1");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
            Assert.AreEqual(2, Countly.Instance.Events._timedEvents.Count);

            Countly.Instance.Consents.RemoveConsent(new Consents[] { Consents.Events });
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
            Assert.AreEqual(0, Countly.Instance.Events._timedEvents.Count);

            Countly.Instance.Events.StartEvent("test_event");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
            Assert.AreEqual(0, Countly.Instance.Events._timedEvents.Count);
        }

        /// <summary>
        /// It validates the cancelation of timed events on changing device id without merge.
        /// </summary>
        [Test]
        public async void TestTimedEventsCancelationOnDeviceIdChange()
        {
            Countly.Instance.Init(TestUtility.CreateBaseConfig());

            Countly.Instance.Events.StartEvent("test_event");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
            Assert.AreEqual(1, Countly.Instance.Events._timedEvents.Count);

            Countly.Instance.Events.StartEvent("test_event_1");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
            Assert.AreEqual(2, Countly.Instance.Events._timedEvents.Count);

            await Countly.Instance.Device.ChangeDeviceIdWithoutMerge("new_device_id");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
            Assert.AreEqual(0, Countly.Instance.Events._timedEvents.Count);
        }

        /// <summary>
        /// It validates functionality of 'Timed Events' methods .
        /// </summary>
        [UnityTest]
        public IEnumerator TestTimedEventMethods()
        {
            Countly.Instance.Init(TestUtility.CreateBaseConfig());
            Assert.IsNotNull(Countly.Instance.Events);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
            Assert.AreEqual(0, Countly.Instance.Events._timedEvents.Count);

            Countly.Instance.Events.StartEvent("test_event");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
            Assert.AreEqual(1, Countly.Instance.Events._timedEvents.Count);

            Countly.Instance.Events.StartEvent("test_event");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
            Assert.AreEqual(1, Countly.Instance.Events._timedEvents.Count);

            Countly.Instance.Events.StartEvent("test_event_1");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
            Assert.AreEqual(2, Countly.Instance.Events._timedEvents.Count);

            Countly.Instance.Events.CancelEvent("test_event_1");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
            Assert.AreEqual(1, Countly.Instance.Events._timedEvents.Count);

            Countly.Instance.Events.CancelEvent("test_event_2");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
            Assert.AreEqual(1, Countly.Instance.Events._timedEvents.Count);

            Countly.Instance.Events.EndEvent("test_event_1");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
            Assert.AreEqual(1, Countly.Instance.Events._timedEvents.Count);

            System.DateTime startTime = System.DateTime.UtcNow;
            do {
                yield return null;
            }
            while ((System.DateTime.UtcNow - startTime).TotalSeconds < 2.1);

            Countly.Instance.Events.EndEvent("test_event");
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);
            Assert.AreEqual(0, Countly.Instance.Events._timedEvents.Count);

            CountlyEventModel model = Countly.Instance.Events._eventRepo.Dequeue();
            AssertAnEvent(model, "test_event", 0, 1, 2.1, null);
        }

        /// <summary>
        /// It validates functionality of method 'RecordEventAsync'.
        /// </summary>
        [UnityTest]
        public IEnumerator TestTimedEventWithSegmentation()
        {
            Countly.Instance.Init(TestUtility.CreateBaseConfig());
            Assert.IsNotNull(Countly.Instance.Events);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
            Assert.AreEqual(0, Countly.Instance.Events._timedEvents.Count);

            Countly.Instance.Events.StartEvent("test_event");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
            Assert.AreEqual(1, Countly.Instance.Events._timedEvents.Count);

            Countly.Instance.Events.StartEvent("test_event_1");
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
            Assert.AreEqual(2, Countly.Instance.Events._timedEvents.Count);

            System.DateTime startTime = System.DateTime.UtcNow;
            do {
                yield return null;
            }
            while ((System.DateTime.UtcNow - startTime).TotalSeconds < 2.5);

            IDictionary<string, object> segmentation = new Dictionary<string, object>();
            segmentation.Add("key1", "value1");
            segmentation.Add("key2", "value2");

            Countly.Instance.Events.EndEvent("test_event", segmentation, 5, 10);
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);
            Assert.AreEqual(1, Countly.Instance.Events._timedEvents.Count);

            CountlyEventModel model = Countly.Instance.Events._eventRepo.Dequeue();
            AssertAnEvent(model, "test_event", 10, 5, 2.5, segmentation);
        }

        /// <summary>
        /// It validates functionality of method 'RecordEventAsync'.
        /// </summary>
        [Test]
        public async void TestEventMethod_RecordEventAsync()
        {
            Countly.Instance.Init(TestUtility.CreateBaseConfig());
            Assert.IsNotNull(Countly.Instance.Events);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("test_event");
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            CountlyEventModel model = Countly.Instance.Events._eventRepo.Dequeue();
            AssertAnEvent(model, "test_event", 0, 1, null, null);

            await Countly.Instance.Events.RecordEventAsync("test_event1", segmentation: null, count: 5, duration: null, sum: null);
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            model = Countly.Instance.Events._eventRepo.Dequeue();
            AssertAnEvent(model, "test_event1", null, 5, null, null);
        }

        /// <summary>
        /// It validates the working of event service if 'EventQueueThreshold' limit reach.
        /// </summary>
        [Test]
        public async void TestEvent_EventQueueThreshold_Limit()
        {
            CountlyConfiguration configuration = TestUtility.CreateBaseConfig()
                .SetEventQueueSizeToSend(3);

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
            Countly.Instance.Init(TestUtility.CreateBaseConfig());

            Assert.IsNotNull(Countly.Instance.Events);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);
            Dictionary<string, object> segments = new Dictionary<string, object>{
                { "key1", "value1"},
                { "key2", "value2"}
            };
            SegmentModel segmentModel = new SegmentModel(segments);
            await Countly.Instance.Events.RecordEventAsync("test_event", segmentation: segmentModel, sum: 23, duration: 5);
            CountlyEventModel model = Countly.Instance.Events._eventRepo.Dequeue();
            AssertAnEvent(model, "test_event", 23, 1, 5, segments);
        }

        /// <summary>
        /// It validates the event limits.
        /// </summary>
        [Test]
        public async void TestEventLimits()
        {
            CountlyConfiguration configuration = TestUtility.CreateBaseConfig()
                .SetMaxKeyLength(4)
                .SetMaxValueSize(6)
                .SetMaxSegmentationValues(2);

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.Events);
            Assert.AreEqual(0, Countly.Instance.Events._eventRepo.Count);

            Dictionary<string, object> segments = new Dictionary<string, object>{
                { "key1", "value1"},
                { "key2_00", "value2_00"},
                { "key3_00", "value3"}
            };

            SegmentModel segmentModel = new SegmentModel(segments);
            await Countly.Instance.Events.RecordEventAsync("test_event", segmentation: segmentModel, sum: 23, duration: 5);
            CountlyEventModel model = Countly.Instance.Events._eventRepo.Dequeue();
            Dictionary<string, object> requireSegments = new Dictionary<string, object>{
                { "key1", "value1"},
                { "key2", "value2"},
            };
            AssertAnEvent(model, "test", 23, 1, 5, requireSegments);
        }

        /// <summary>
        /// It validates the data type of segment items.
        /// </summary>
        [Test]
        public async void TestSegmentItemsDataTypesValidation()
        {
            Countly.Instance.Init(TestUtility.CreateBaseConfig());

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
            Dictionary<string, object> requireSegments = new Dictionary<string, object>{
                { "key1", "value1"},
                { "key2", 1},
                { "key3", 10.0},
                { "key4", true},
            };

            AssertAnEvent(model, "test_event", 23, 1, 5, requireSegments);
        }

        /// <summary>
        /// It validates the mandatory and optional parameters of events.
        /// </summary>
        [Test]
        public async void TestEventsParameters()
        {
            Countly.Instance.Init(TestUtility.CreateBaseConfig());

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
            CountlyConfiguration configuration = TestUtility.CreateBaseConfig()
                .SetRequiresConsent(true);

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
        public async void TestEventMethod_RecordEventAgainstViewKey()
        {
            CountlyConfiguration configuration = TestUtility.CreateBaseConfig()
                .SetRequiresConsent(true);

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
            await Countly.Instance.Events.RecordEventAsync("[CLY]_view", segmentation: null, count: 5, duration: null, sum: 1);
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            model = Countly.Instance.Events._eventRepo.Dequeue();
            AssertAnEvent(model, "[CLY]_view", 1, 5, null, null);
        }

        /// <summary>
        /// It validates 'recordEvent' against action specific key '[CLY]_action'.
        /// </summary>
        [Test]
        public async void TestEventMethod_RecordEventAgainstActionKey()
        {
            CountlyConfiguration configuration = TestUtility.CreateBaseConfig()
                .SetRequiresConsent(true);

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
            AssertAnEvent(model, "[CLY]_action", 0, 1, null, null);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_action", segmentation: null, count: 5, duration: null, sum: 1);
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            model = Countly.Instance.Events._eventRepo.Dequeue();
            AssertAnEvent(model, "[CLY]_action", 1, 5, null, null);
        }

        /// <summary>
        /// It validates 'recordEvent' against action specific key '[CLY]_star_rating'.
        /// </summary>
        [Test]
        public async void TestEventMethod_RecordEventAgainstStarRatingKey()
        {
            CountlyConfiguration configuration = TestUtility.CreateBaseConfig()
                .SetRequiresConsent(true);

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
            AssertAnEvent(model, "[CLY]_star_rating", 0, 1, null, null);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_star_rating", segmentation: null, count: 5, duration: null, sum: 1);
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            model = Countly.Instance.Events._eventRepo.Dequeue();
            AssertAnEvent(model, "[CLY]_star_rating", 1, 5, null, null);
        }

        /// <summary>
        /// It validates 'recordEvent' against push specific key '[CLY]_push_action'.
        /// </summary>
        [Test]
        public async void TestEventMethod_RecordEventAgainstPushActionKey()
        {
            CountlyConfiguration configuration = TestUtility.CreateBaseConfig()
                .SetRequiresConsent(true);

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
            AssertAnEvent(model, "[CLY]_push_action", 0, 1, null, null);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_push_action", segmentation: null, count: 5, duration: null, sum: 1);
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            model = Countly.Instance.Events._eventRepo.Dequeue();
            AssertAnEvent(model, "[CLY]_push_action", 1, 5, null, null);
        }
        /// <summary>
        /// It validates 'recordEvent' against push specific key '[CLY]_orientation'.
        /// </summary>
        [Test]
        public async void TestEventMethod_RecordEventAgainstOrientationKey()
        {
            CountlyConfiguration configuration = TestUtility.CreateBaseConfig()
                .SetRequiresConsent(true);

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
            AssertAnEvent(model, "[CLY]_orientation", 0, 1, null, null);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_orientation", segmentation: null, count: 5, duration: null, sum: 1);
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            model = Countly.Instance.Events._eventRepo.Dequeue();
            AssertAnEvent(model, "[CLY]_orientation", 1, 5, null, null);
        }

        /// <summary>
        /// It validates 'recordEvent' against nps([CLY]_nps) and survey([CLY]_survey) specific keys.
        /// </summary>
        [Test]
        public async void TestEventMethod_RecordEventAsyncWithSpecificKeys()
        {
            CountlyConfiguration configuration = TestUtility.CreateBaseConfig()
                .SetRequiresConsent(true);

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
            AssertAnEvent(model, "[CLY]_survey", 1, 5, null, null);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_nps");
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            model = Countly.Instance.Events._eventRepo.Dequeue();
            AssertAnEvent(model, "[CLY]_nps", 0, 1, null, null);

            await Countly.Instance.Events.RecordEventAsync("[CLY]_nps", segmentation: null, count: 5, duration: null, sum: 1);
            Assert.AreEqual(1, Countly.Instance.Events._eventRepo.Count);

            model = Countly.Instance.Events._eventRepo.Dequeue();
            AssertAnEvent(model, "[CLY]_nps", 1, 5, null, null);
        }

        /// <summary>
        /// It validates 'recordEvent' against specific event keys.
        /// </summary>
        [Test]
        public async void TestEventMethod_RecordEventAgainstSpecificEventKeys()
        {
            CountlyConfiguration configuration = TestUtility.CreateBaseConfig()
                .SetRequiresConsent(true);

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
            AssertAnEvent(model, "event", 1, 5, null, null);
        }

        [SetUp]
        [TearDown]
        public void End()
        {
            TestUtility.TestCleanup();
        }
    }
}