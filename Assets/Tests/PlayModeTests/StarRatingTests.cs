﻿using NUnit.Framework;
using UnityEngine;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Enums;
using System.Collections.Generic;

namespace Assets.Tests.PlayModeTests
{
    public class StarRatingTests
    {
        private readonly string _serverUrl = "https://xyz.com/";
        private readonly string _appKey = "772c091355076ead703f987fee94490";

        private void AssertStarRatingModel(CountlyEventModel model, IDictionary<string, object> segmentation)
        {
            Assert.AreEqual(CountlyEventModel.StarRatingEvent, model.Key);
            Assert.IsNull(model.Sum);
            Assert.AreEqual(1, model.Count);
            Assert.IsNull(model.Duration);
            Assert.IsNotNull(model.Segmentation);

            foreach (KeyValuePair<string, object> entry in segmentation) {
                Assert.AreEqual(entry.Value, model.Segmentation[entry.Key]);
            }
        }

        /// <summary>
        /// It validates the event repository initial state.
        /// </summary>
        [Test]
        public void TestStarRatingRepoInitialState()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);

            Assert.IsNotNull(Countly.Instance.StarRating);
            Assert.AreEqual(0, Countly.Instance.StarRating._eventCountlyService._eventRepo.Count);

        }

        /// <summary>
        /// It validates the dependency of 'Event Consent' on StarRating Service.
        /// </summary>
        [Test]
        public async void TestStarRating_CheckEventConsentDependency()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                RequiresConsent = true,
            };

            configuration.GiveConsent(new Consents[] { Consents.StarRating });

            Countly.Instance.Init(configuration);

            Countly.Instance.StarRating._eventCountlyService._eventRepo.Clear();
            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();

            Assert.IsNotNull(Countly.Instance.StarRating);
            Assert.AreEqual(0, Countly.Instance.StarRating._eventCountlyService._eventRepo.Count);

            await Countly.Instance.StarRating.ReportStarRatingAsync("android", "0.1", 3);
            Assert.AreEqual(1, Countly.Instance.StarRating._eventCountlyService._eventRepo.Count);

            CountlyEventModel model = Countly.Instance.StarRating._eventCountlyService._eventRepo.Dequeue();

            IDictionary<string, object> segmentation = new Dictionary<string, object>();
            segmentation.Add("platform", "android");
            segmentation.Add("app_version", "0.1");
            segmentation.Add("rating", 3);

            AssertStarRatingModel(model, segmentation);
        }

        /// <summary>
        /// It checks the working of StarRating service if no StarRating consent is given.
        /// </summary>
        [Test]
        public async void TestStarRatingConsent()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                RequiresConsent = true
            };


            Countly.Instance.Init(configuration);

            Countly.Instance.StarRating._eventCountlyService._eventRepo.Clear();
            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();

            Assert.IsNotNull(Countly.Instance.StarRating);
            Assert.AreEqual(0, Countly.Instance.StarRating._eventCountlyService._eventRepo.Count);

            await Countly.Instance.StarRating.ReportStarRatingAsync("android", "0.1", 3);
            Assert.AreEqual(0, Countly.Instance.StarRating._eventCountlyService._eventRepo.Count);
        }

        /// <summary>
        /// It validates functionality of method 'ReportStarRatingAsync'.
        /// </summary>
        [Test]
        public async void TestStarRatingMethod_ReportStarRatingAsync()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);

            Countly.Instance.StarRating._eventCountlyService._eventRepo.Clear();
            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();

            Assert.IsNotNull(Countly.Instance.StarRating);
            await Countly.Instance.StarRating.ReportStarRatingAsync("android", "0.1", 5);
            Assert.AreEqual(1, Countly.Instance.StarRating._eventCountlyService._eventRepo.Count);

            CountlyEventModel model = Countly.Instance.StarRating._eventCountlyService._eventRepo.Dequeue();

            Assert.AreEqual(CountlyEventModel.StarRatingEvent, model.Key);
            Assert.IsNull(model.Sum);
            Assert.AreEqual(1, model.Count);
            Assert.IsNull(model.Duration);
            Assert.IsNotNull(model.Segmentation);
            Assert.AreEqual("android", model.Segmentation["platform"]);
            Assert.AreEqual("0.1", model.Segmentation["app_version"]);
            Assert.AreEqual(5, model.Segmentation["rating"]);

            IDictionary<string, object> segmentation = new Dictionary<string, object>();
            segmentation.Add("platform", "android");
            segmentation.Add("app_version", "0.1");
            segmentation.Add("rating", 5);

            AssertStarRatingModel(model, segmentation);

        }

        /// <summary>
        /// It validates the parameters of 'ReportStarRatingAsync' method.
        /// </summary>
        [Test]
        public async void TestStarRating_ValidatesParams()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);
            Countly.Instance.StarRating._eventCountlyService._eventRepo.Clear();
            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();

            Assert.IsNotNull(Countly.Instance.StarRating);
            Assert.AreEqual(0, Countly.Instance.StarRating._eventCountlyService._eventRepo.Count);


            await Countly.Instance.StarRating.ReportStarRatingAsync("", "0.1", 4);
            Assert.AreEqual(0, Countly.Instance.StarRating._eventCountlyService._eventRepo.Count);

            await Countly.Instance.StarRating.ReportStarRatingAsync(null, "0.1", 4);
            Assert.AreEqual(0, Countly.Instance.StarRating._eventCountlyService._eventRepo.Count);

            await Countly.Instance.StarRating.ReportStarRatingAsync("android", "", 4);
            Assert.AreEqual(0, Countly.Instance.StarRating._eventCountlyService._eventRepo.Count);

            await Countly.Instance.StarRating.ReportStarRatingAsync("android", null, 4);
            Assert.AreEqual(0, Countly.Instance.StarRating._eventCountlyService._eventRepo.Count);

            await Countly.Instance.StarRating.ReportStarRatingAsync("android", "0.1", 0);
            Assert.AreEqual(0, Countly.Instance.StarRating._eventCountlyService._eventRepo.Count);

            await Countly.Instance.StarRating.ReportStarRatingAsync("android", "0.1", 6);
            Assert.AreEqual(0, Countly.Instance.StarRating._eventCountlyService._eventRepo.Count);

            await Countly.Instance.StarRating.ReportStarRatingAsync("android", "0.1", 4);
            Assert.AreEqual(1, Countly.Instance.StarRating._eventCountlyService._eventRepo.Count);

        }

        /// <summary>
        /// It validates 'EventQueueThreshold' limit.
        /// </summary>
        [Test]
        public async void TestStarRating_EventQueueThreshold_Limit()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                EventQueueThreshold = 3
            };

            Countly.Instance.Init(configuration);
            Countly.Instance.StarRating._eventCountlyService._eventRepo.Clear();
            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();

            Assert.IsNotNull(Countly.Instance.StarRating);
            Assert.AreEqual(0, Countly.Instance.StarRating._eventCountlyService._eventRepo.Count);

            Countly.Instance.Views.StartView("view");
            Assert.AreEqual(1, Countly.Instance.StarRating._eventCountlyService._eventRepo.Count);

            await Countly.Instance.StarRating.ReportStarRatingAsync("android", "0.1", 4);
            Assert.AreEqual(2, Countly.Instance.StarRating._eventCountlyService._eventRepo.Count);

            await Countly.Instance.Events.RecordEventAsync("test_event");
            Assert.AreEqual(0, Countly.Instance.StarRating._eventCountlyService._eventRepo.Count);
        }

        [SetUp]
        [TearDown]
        public void End()
        {
            TestUtility.TestCleanup();
        }
    }
}
