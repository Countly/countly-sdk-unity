﻿using NUnit.Framework;
using UnityEngine;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK;
using System.Threading.Tasks;
using System.Web;
using System.Collections.Specialized;

namespace Tests
{
    public class DeviceIdTests
    {
        private readonly string _serverUrl = "https://xyz.com/";
        private readonly string _appKey = "772c091355076ead703f987fee94490";

        /// <summary>
        /// It validates device id generated by SDK.
        /// </summary>
        [Test]
        public void TestDeviceIdGeneratedBySDK()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);
            Assert.IsNotNull(Countly.Instance.Device);
            Assert.IsNotNull(Countly.Instance.Device.DeviceId);
            Assert.IsNotEmpty(Countly.Instance.Device.DeviceId);
        }

        /// <summary>
        /// It validates device id provided in config.
        /// </summary>
        [Test]
        public void TestDeviceIdGivenInConfig()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                DeviceId = "device_id"
            };

            Countly.Instance.Init(configuration);
            Assert.IsNotNull(Countly.Instance.Device);
            Assert.AreEqual("device_id", Countly.Instance.Device.DeviceId);

        }

        /// <summary>
        /// It validates the working of methods 'ChangeDeviceIdWithMerge' and 'ChangeDeviceIdWithoutMerge' on giving same device id.
        /// </summary>
        [Test]
        public async void TestSameDeviceIdLogic()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
                DeviceId = "device_id"
            };

            Countly.Instance.Init(configuration);
            Assert.IsNotNull(Countly.Instance.Device);
            Assert.AreEqual("device_id", Countly.Instance.Device.DeviceId);

            Countly.Instance.Device._requestCountlyHelper._requestRepo.Clear();
            await Countly.Instance.Device.ChangeDeviceIdWithMerge("device_id");
            Assert.AreEqual(0, Countly.Instance.Device._requestCountlyHelper._requestRepo.Count);

            await Countly.Instance.Device.ChangeDeviceIdWithoutMerge("device_id");
            Assert.AreEqual(0, Countly.Instance.Device._requestCountlyHelper._requestRepo.Count);

            Assert.AreEqual("device_id", Countly.Instance.Device.DeviceId);

        }


        /// <summary>
        /// It validates the functionality of method 'ChangeDeviceIdWithoutMerge'.
        /// </summary>
        [Test]
        public async void TestDeviceServiceMethod_ChangeDeviceIdWithoutMerge()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
            };

            Countly.Instance.Init(configuration);
            Assert.IsNotNull(Countly.Instance.Consents);

            string oldDeviceId = Countly.Instance.Device.DeviceId;
            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();
            await Countly.Instance.Device.ChangeDeviceIdWithoutMerge("new_device_id");
            //RQ will have begin session and end session requests
            Assert.AreEqual(2, Countly.Instance.Device._requestCountlyHelper._requestRepo.Count);


            CountlyRequestModel requestModel = Countly.Instance.Device._requestCountlyHelper._requestRepo.Dequeue();
            string uri = requestModel.RequestUrl;
            NameValueCollection values = HttpUtility.ParseQueryString(uri);

            Assert.AreEqual("1", values.Get("end_session"));
            Assert.AreEqual(oldDeviceId, values.Get("device_id"));
            Assert.IsNotNull(values.Get("session_duration"));

            requestModel = Countly.Instance.Device._requestCountlyHelper._requestRepo.Dequeue();
            uri = requestModel.RequestUrl;
            values = HttpUtility.ParseQueryString(uri);

            Assert.AreEqual("1", values.Get("begin_session"));
            Assert.AreEqual("new_device_id", values.Get("device_id"));
            Assert.AreEqual("new_device_id", Countly.Instance.Device.DeviceId);

        }

        /// <summary>
        /// It validates functionality of method 'ChangeDeviceIdWithMerge'.
        /// </summary>
        [Test]
        public async void TestDeviceServiceMethod_ChangeDeviceIdWithMerge()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                AppKey = _appKey,
                ServerUrl = _serverUrl,
            };

            Countly.Instance.Init(configuration);
            Assert.IsNotNull(Countly.Instance.Consents);
            
            string oldDeviceId = Countly.Instance.Device.DeviceId;
            Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Clear();
            await Countly.Instance.Device.ChangeDeviceIdWithMerge("new_device_id");
            //RQ will have begin session and end session requests
            Assert.AreEqual(1, Countly.Instance.Device._requestCountlyHelper._requestRepo.Count);


            CountlyRequestModel requestModel = Countly.Instance.Device._requestCountlyHelper._requestRepo.Dequeue();
            string uri = requestModel.RequestUrl;
            NameValueCollection values = HttpUtility.ParseQueryString(uri);

            Assert.AreEqual(oldDeviceId, values.Get("old_device_id"));
            Assert.AreEqual("new_device_id", values.Get("device_id"));
            Assert.AreEqual("new_device_id", Countly.Instance.Device.DeviceId);

        }

        [TearDown]
        public void End()
        {
            Countly.Instance.ClearStorage();
            Object.DestroyImmediate(Countly.Instance);
        }
    }
}
