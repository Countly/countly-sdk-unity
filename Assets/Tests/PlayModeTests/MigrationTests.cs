﻿using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK;
using Newtonsoft.Json;
using System.Web;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using Plugins.CountlySDK.Helpers;
using UnityEditor.PackageManager.Requests;
using System.Linq;

namespace Tests
{
    public class MigrationTests
    {
        private readonly string _serverUrl = "https://xyz.com/";
        private readonly string _appKey = "772c091355076ead703f987fee94490";

        /// <summary>
        /// It validates the request migration on empty request repo.
        /// </summary>
        [Test]
        public void TestMigrationOnEmptyRequestRepo()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);
            Countly.Instance.RequestHelper._requestRepo.Clear();

            Assert.AreEqual(2, Countly.Instance.StorageHelper.SchemaVersion);

            Countly.Instance.StorageHelper.CurrentVersion = 1;
            Countly.Instance.StorageHelper.RunMigration();

            int schemaVersion = PlayerPrefs.GetInt(Constants.SchemaVersion);
            Assert.AreEqual(2, schemaVersion);
            Assert.AreEqual(2, Countly.Instance.StorageHelper.CurrentVersion);
            Assert.AreEqual(Countly.Instance.StorageHelper.SchemaVersion, Countly.Instance.StorageHelper.CurrentVersion);

            Assert.AreEqual(0, Countly.Instance.RequestHelper._requestRepo.Count);

        }

         /// <summary>
        /// It validates the request after migrating old GET request to new GET request format.
        /// </summary>
        [Test]
        public void TestStoreGETRequestsAfterMigration()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);
            Countly.Instance.RequestHelper._requestRepo.Clear();

            string url = "https://xyz.com/i?app_key=772c091355076ead703f987fee94490&device_id=57049b51faf44804a10967f54d8f8420&sdk_name=csharp-unity-editor&sdk_version=20.11.5&timestamp=1633595280409&hour=13&dow=4&tz=300&consent=%7b%0a++%22crashes%22%3a+true%2c%0a++%22events%22%3a+true%2c%0a++%22clicks%22%3a+true%2c%0a++%22star-rating%22%3a+true%2c%0a++%22views%22%3a+true%2c%0a++%22users%22%3a+true%2c%0a++%22sessions%22%3a+true%2c%0a++%22push%22%3a+true%2c%0a++%22remote-config%22%3a+true%2c%0a++%22location%22%3a+true%2c%0a++%22feedback%22%3a+true%0a%7d&checksum256=a3c63ddd0fa788eb05c75752533fdb8083960c4c35fb0ed5a689b631d2beb194";
            CountlyRequestModel request = new CountlyRequestModel(url,  null);
            Countly.Instance.RequestHelper.AddRequestToQueue(request);
            Countly.Instance.StorageHelper.CurrentVersion = 1;
            Countly.Instance.StorageHelper.RunMigration();

            Assert.AreEqual(1, Countly.Instance.RequestHelper._requestRepo.Count);

            CountlyRequestModel requestModel = Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Dequeue();

            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            Dictionary<string, string> queryParams = collection.AllKeys.ToDictionary(t => t, t => collection[t]);

            JObject json = JObject.FromObject(queryParams);

            int schemaVersion = PlayerPrefs.GetInt(Constants.SchemaVersion);
            Assert.AreEqual(2, schemaVersion);
            Assert.AreEqual(2, Countly.Instance.StorageHelper.CurrentVersion);
            Assert.AreEqual(Countly.Instance.StorageHelper.SchemaVersion, Countly.Instance.StorageHelper.CurrentVersion);

            Assert.AreEqual("772c091355076ead703f987fee94490", json.GetValue("app_key").ToObject<string>());
            Assert.AreEqual("57049b51faf44804a10967f54d8f8420", json.GetValue("device_id").ToObject<string>());
            Assert.AreEqual("csharp-unity-editor", json.GetValue("sdk_name").ToObject<string>());
            Assert.AreEqual("20.11.5", json.GetValue("sdk_version").ToObject<string>());
            Assert.IsTrue(json.ContainsKey("consent"));
            Assert.IsFalse(json.ContainsKey("checksum256"));
            Assert.IsNull(requestModel.RequestUrl);

        }


        /// <summary>
        /// It validates the orders and format of request after migrating old GET requests to new GET request format.
        /// </summary>
        [Test]
        public void TestMultipleGETRequestsAfterMigration()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);
            Countly.Instance.RequestHelper._requestRepo.Clear();

            string url = "https://xyz.com/i?app_key=772c091355076ead703f987fee94490&device_id=57049b51faf44804a10967f54d8f8420&sdk_name=csharp-unity-editor&sdk_version=20.11.5&timestamp=1633595280409&hour=13&dow=4&tz=300&consent=%7b%0a++%22crashes%22%3a+true%2c%0a++%22events%22%3a+true%2c%0a++%22clicks%22%3a+true%2c%0a++%22star-rating%22%3a+true%2c%0a++%22views%22%3a+true%2c%0a++%22users%22%3a+true%2c%0a++%22sessions%22%3a+true%2c%0a++%22push%22%3a+true%2c%0a++%22remote-config%22%3a+true%2c%0a++%22location%22%3a+true%2c%0a++%22feedback%22%3a+true%0a%7d&checksum256=a3c63ddd0fa788eb05c75752533fdb8083960c4c35fb0ed5a689b631d2beb194";
            CountlyRequestModel request = new CountlyRequestModel(url,  null);
            Countly.Instance.RequestHelper.AddRequestToQueue(request);

            url = "https://xyz.com/i?app_key=772c091355076ead703f987fee94490&device_id=57049b51faf44804a10967f54d8f8420&sdk_name=csharp-unity-editor&sdk_version=20.11.4&timestamp=1633595280409&hour=13&dow=4&tz=300&consent=%7b%0a++%22crashes%22%3a+true%2c%0a++%22events%22%3a+true%2c%0a++%22clicks%22%3a+true%2c%0a++%22star-rating%22%3a+true%2c%0a++%22views%22%3a+true%2c%0a++%22users%22%3a+true%2c%0a++%22sessions%22%3a+true%2c%0a++%22push%22%3a+true%2c%0a++%22remote-config%22%3a+true%2c%0a++%22location%22%3a+true%2c%0a++%22feedback%22%3a+true%0a%7d";
            request = new CountlyRequestModel(url,  null);
            Countly.Instance.RequestHelper.AddRequestToQueue(request);

            url = "https://xyz.com/i?app_key=772c091355076ead703f987fee94490&device_id=57049b51faf44804a10967f54d8f8420&sdk_name=csharp-unity-editor&sdk_version=20.11.3&timestamp=1633595280409&hour=13&dow=4&tz=300&consent=%7b%0a++%22crashes%22%3a+true%2c%0a++%22events%22%3a+true%2c%0a++%22clicks%22%3a+true%2c%0a++%22star-rating%22%3a+true%2c%0a++%22views%22%3a+true%2c%0a++%22users%22%3a+true%2c%0a++%22sessions%22%3a+true%2c%0a++%22push%22%3a+true%2c%0a++%22remote-config%22%3a+true%2c%0a++%22location%22%3a+true%2c%0a++%22feedback%22%3a+true%0a%7d&checksum256=a3c63ddd0fa788eb05c75752533fdb8083960c4c35fb0ed5a689b631d2beb194";
            request = new CountlyRequestModel(url,  null);
            Countly.Instance.RequestHelper.AddRequestToQueue(request);

            Countly.Instance.StorageHelper.CurrentVersion = 1;
            Countly.Instance.StorageHelper.RunMigration();

            Assert.AreEqual(3, Countly.Instance.RequestHelper._requestRepo.Count);

            int schemaVersion = PlayerPrefs.GetInt(Constants.SchemaVersion);
            Assert.AreEqual(2, schemaVersion);
            Assert.AreEqual(2, Countly.Instance.StorageHelper.CurrentVersion);
            Assert.AreEqual(Countly.Instance.StorageHelper.SchemaVersion, Countly.Instance.StorageHelper.CurrentVersion);



            CountlyRequestModel requestModel = Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Dequeue();

            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            Dictionary<string, string> queryParams = collection.AllKeys.ToDictionary(t => t, t => collection[t]);

            JObject json = JObject.FromObject(queryParams);
            

            Assert.AreEqual("772c091355076ead703f987fee94490", json.GetValue("app_key").ToObject<string>());
            Assert.AreEqual("57049b51faf44804a10967f54d8f8420", json.GetValue("device_id").ToObject<string>());
            Assert.AreEqual("csharp-unity-editor", json.GetValue("sdk_name").ToObject<string>());
            Assert.AreEqual("20.11.5", json.GetValue("sdk_version").ToObject<string>());
            Assert.IsTrue(json.ContainsKey("consent"));
            Assert.IsFalse(json.ContainsKey("checksum256"));
            Assert.IsNull(requestModel.RequestUrl);

            requestModel = Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Dequeue();
            collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            queryParams = collection.AllKeys.ToDictionary(t => t, t => collection[t]);

            json = JObject.FromObject(queryParams);

            Assert.AreEqual("772c091355076ead703f987fee94490", json.GetValue("app_key").ToObject<string>());
            Assert.AreEqual("57049b51faf44804a10967f54d8f8420", json.GetValue("device_id").ToObject<string>());
            Assert.AreEqual("csharp-unity-editor", json.GetValue("sdk_name").ToObject<string>());
            Assert.AreEqual("20.11.4", json.GetValue("sdk_version").ToObject<string>());
            Assert.IsTrue(json.ContainsKey("consent"));
            Assert.IsFalse(json.ContainsKey("checksum256"));
            Assert.IsNull(requestModel.RequestUrl);

            requestModel = Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Dequeue();
            collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            queryParams = collection.AllKeys.ToDictionary(t => t, t => collection[t]);

            json = JObject.FromObject(queryParams);

            Assert.AreEqual("772c091355076ead703f987fee94490", json.GetValue("app_key").ToObject<string>());
            Assert.AreEqual("57049b51faf44804a10967f54d8f8420", json.GetValue("device_id").ToObject<string>());
            Assert.AreEqual("csharp-unity-editor", json.GetValue("sdk_name").ToObject<string>());
            Assert.AreEqual("20.11.3", json.GetValue("sdk_version").ToObject<string>());
            Assert.IsTrue(json.ContainsKey("consent"));
            Assert.IsFalse(json.ContainsKey("checksum256"));
            Assert.IsNull(requestModel.RequestUrl);

        }

         /// <summary>
        /// It validates the order and format of requests after migrating old POST requests to new POST request format.
        /// </summary>
        [Test]
        public void TestMultiplePostRequestsAfterMigration()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);
            Countly.Instance.RequestHelper._requestRepo.Clear();

            string data = "{\"app_key\":\"772c091355076ead703f987fee94490\",\"device_id\":\"57049b51faf44874a10967f54d8f8420\",\"sdk_name\":\"csharp-unity-editor\",\"sdk_version\":\"20.11.5\",\"timestamp\":1633595280409,\"hour\":13,\"dow\":4,\"tz\":\"300\",\"consent\":{\"crashes\":true,\"events\":true,\"clicks\":true,\"star-rating\":true,\"views\":true,\"users\":true,\"sessions\":true,\"push\":true,\"remote-config\":true,\"location\":true,\"feedback\":true},\"checksum256\":\"a3c63ddd0fa788eb05c75752533fdb8083960c4c35fb0ed5a689b631d2beb194\"}";
            CountlyRequestModel request = new CountlyRequestModel(_serverUrl,  data);
            Countly.Instance.RequestHelper.AddRequestToQueue(request);

            data = "{\"app_key\":\"772c091355076ead703f987fee94490\",\"device_id\":\"57049b51faf44874a10967f54d8f8420\",\"sdk_name\":\"csharp-unity-editor\",\"sdk_version\":\"20.11.4\",\"timestamp\":1633595280409,\"hour\":13,\"dow\":4,\"tz\":\"300\",\"consent\":{\"crashes\":true,\"events\":true,\"clicks\":true,\"star-rating\":true,\"views\":true,\"users\":true,\"sessions\":true,\"push\":true,\"remote-config\":true,\"location\":true,\"feedback\":true},\"checksum256\":\"a3c63ddd0fa788eb05c75752533fdb8083960c4c35fb0ed5a689b631d2beb194\"}";
             request = new CountlyRequestModel(_serverUrl,  data);
            Countly.Instance.RequestHelper.AddRequestToQueue(request);

            data = "{\"app_key\":\"772c091355076ead703f987fee94490\",\"device_id\":\"57049b51faf44874a10967f54d8f8420\",\"sdk_name\":\"csharp-unity-editor\",\"sdk_version\":\"20.11.3\",\"timestamp\":1633595280409,\"hour\":13,\"dow\":4,\"tz\":\"300\",\"consent\":{\"crashes\":true,\"events\":true,\"clicks\":true,\"star-rating\":true,\"views\":true,\"users\":true,\"sessions\":true,\"push\":true,\"remote-config\":true,\"location\":true,\"feedback\":true},\"checksum256\":\"a3c63ddd0fa788eb05c75752533fdb8083960c4c35fb0ed5a689b631d2beb194\"}";
            request = new CountlyRequestModel(_serverUrl,  data);
            Countly.Instance.RequestHelper.AddRequestToQueue(request);

            Countly.Instance.StorageHelper.CurrentVersion = 1;
            Countly.Instance.StorageHelper.RunMigration();

            int schemaVersion = PlayerPrefs.GetInt(Constants.SchemaVersion);
            Assert.AreEqual(2, schemaVersion);
            Assert.AreEqual(2, Countly.Instance.StorageHelper.CurrentVersion);
            Assert.AreEqual(Countly.Instance.StorageHelper.SchemaVersion, Countly.Instance.StorageHelper.CurrentVersion);

            Assert.AreEqual(3, Countly.Instance.RequestHelper._requestRepo.Count);

            CountlyRequestModel requestModel = Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Dequeue();
            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            Dictionary<string, string> queryParams = collection.AllKeys.ToDictionary(t => t, t => collection[t]);

            JObject json = JObject.FromObject(queryParams);

            Assert.AreEqual("772c091355076ead703f987fee94490", json.GetValue("app_key").ToObject<string>());
            Assert.AreEqual("57049b51faf44874a10967f54d8f8420", json.GetValue("device_id").ToObject<string>());
            Assert.AreEqual("csharp-unity-editor", json.GetValue("sdk_name").ToObject<string>());
            Assert.AreEqual("20.11.5", json.GetValue("sdk_version").ToObject<string>());
            Assert.IsTrue(json.ContainsKey("consent"));
            Assert.IsFalse(json.ContainsKey("checksum256"));
            Assert.IsNull(requestModel.RequestUrl);

            requestModel = Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Dequeue();
            collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            queryParams = collection.AllKeys.ToDictionary(t => t, t => collection[t]);

            json = JObject.FromObject(queryParams);
            Assert.AreEqual("772c091355076ead703f987fee94490", json.GetValue("app_key").ToObject<string>());
            Assert.AreEqual("57049b51faf44874a10967f54d8f8420", json.GetValue("device_id").ToObject<string>());
            Assert.AreEqual("csharp-unity-editor", json.GetValue("sdk_name").ToObject<string>());
            Assert.AreEqual("20.11.4", json.GetValue("sdk_version").ToObject<string>());
            Assert.IsTrue(json.ContainsKey("consent"));
            Assert.IsFalse(json.ContainsKey("checksum256"));
            Assert.IsNull(requestModel.RequestUrl);

            requestModel = Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Dequeue();
            collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            queryParams = collection.AllKeys.ToDictionary(t => t, t => collection[t]);

            json = JObject.FromObject(queryParams);

            Assert.AreEqual("772c091355076ead703f987fee94490", json.GetValue("app_key").ToObject<string>());
            Assert.AreEqual("57049b51faf44874a10967f54d8f8420", json.GetValue("device_id").ToObject<string>());
            Assert.AreEqual("csharp-unity-editor", json.GetValue("sdk_name").ToObject<string>());
            Assert.AreEqual("20.11.3", json.GetValue("sdk_version").ToObject<string>());
            Assert.IsTrue(json.ContainsKey("consent"));
            Assert.IsFalse(json.ContainsKey("checksum256"));
            Assert.IsNull(requestModel.RequestUrl);

        }

           /// <summary>
        /// It validates the request after migrating old POST request to new POST request format.
        /// </summary>
        [Test]
        public void TestStorePostRequestsAfterMigration()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);
            Countly.Instance.RequestHelper._requestRepo.Clear();

            string data = "{\"app_key\":\"772c091355076ead703f987fee94490\",\"device_id\":\"57049b51faf44874a10967f54d8f8420\",\"sdk_name\":\"csharp-unity-editor\",\"sdk_version\":\"20.11.5\",\"timestamp\":1633595280409,\"hour\":13,\"dow\":4,\"tz\":\"300\",\"consent\":{\"crashes\":true,\"events\":true,\"clicks\":true,\"star-rating\":true,\"views\":true,\"users\":true,\"sessions\":true,\"push\":true,\"remote-config\":true,\"location\":true,\"feedback\":true},\"checksum256\":\"a3c63ddd0fa788eb05c75752533fdb8083960c4c35fb0ed5a689b631d2beb194\"}";
            CountlyRequestModel request = new CountlyRequestModel(_serverUrl,  data);
            Countly.Instance.RequestHelper.AddRequestToQueue(request);
            Countly.Instance.StorageHelper.CurrentVersion = 1;
            Countly.Instance.StorageHelper.RunMigration();

            Assert.AreEqual(1, Countly.Instance.RequestHelper._requestRepo.Count);

            CountlyRequestModel requestModel = Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Dequeue();
            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            Dictionary<string, string> queryParams = collection.AllKeys.ToDictionary(t => t, t => collection[t]);

            JObject json = JObject.FromObject(queryParams);

            int schemaVersion = PlayerPrefs.GetInt(Constants.SchemaVersion);
            Assert.AreEqual(2, schemaVersion);
            Assert.AreEqual(2, Countly.Instance.StorageHelper.CurrentVersion);
            Assert.AreEqual(Countly.Instance.StorageHelper.SchemaVersion, Countly.Instance.StorageHelper.CurrentVersion);

            Assert.AreEqual("772c091355076ead703f987fee94490", json.GetValue("app_key").ToObject<string>());
            Assert.AreEqual("57049b51faf44874a10967f54d8f8420", json.GetValue("device_id").ToObject<string>());
            Assert.AreEqual("csharp-unity-editor", json.GetValue("sdk_name").ToObject<string>());
            Assert.AreEqual("20.11.5", json.GetValue("sdk_version").ToObject<string>());
            Assert.IsTrue(json.ContainsKey("consent"));
            Assert.IsFalse(json.ContainsKey("checksum256"));
            Assert.IsNull(requestModel.RequestUrl);

        }


              /// <summary>
        /// It validates the order and format of requests after migrating old POST and GET requests to new request format.
        /// </summary>
        [Test]
        public void TestMultiplePostAndGetRequestsAfterMigration()
        {
            CountlyConfiguration configuration = new CountlyConfiguration {
                ServerUrl = _serverUrl,
                AppKey = _appKey,
            };

            Countly.Instance.Init(configuration);
            Countly.Instance.RequestHelper._requestRepo.Clear();

            string url = "https://xyz.com/i?app_key=772c091355076ead703f987fee94490&device_id=57049b51faf44874a10967f54d8f8420&sdk_name=csharp-unity-editor&sdk_version=20.11.5&timestamp=1633595280409&hour=13&dow=4&tz=300&consent=%7b%0a++%22crashes%22%3a+true%2c%0a++%22events%22%3a+true%2c%0a++%22clicks%22%3a+true%2c%0a++%22star-rating%22%3a+true%2c%0a++%22views%22%3a+true%2c%0a++%22users%22%3a+true%2c%0a++%22sessions%22%3a+true%2c%0a++%22push%22%3a+true%2c%0a++%22remote-config%22%3a+true%2c%0a++%22location%22%3a+true%2c%0a++%22feedback%22%3a+true%0a%7d&checksum256=a3c63ddd0fa788eb05c75752533fdb8083960c4c35fb0ed5a689b631d2beb194";
            CountlyRequestModel request = new CountlyRequestModel(url,  null);
            Countly.Instance.RequestHelper.AddRequestToQueue(request);

            string data = "{\"app_key\":\"772c091355076ead703f987fee94490\",\"device_id\":\"57049b51faf44874a10967f54d8f8420\",\"sdk_name\":\"csharp-unity-editor\",\"sdk_version\":\"20.11.4\",\"timestamp\":1633595280409,\"hour\":13,\"dow\":4,\"tz\":\"300\",\"consent\":{\"crashes\":true,\"events\":true,\"clicks\":true,\"star-rating\":true,\"views\":true,\"users\":true,\"sessions\":true,\"push\":true,\"remote-config\":true,\"location\":true,\"feedback\":true},\"checksum256\":\"a3c63ddd0fa788eb05c75752533fdb8083960c4c35fb0ed5a689b631d2beb194\"}";
            request = new CountlyRequestModel(_serverUrl,  data);
            Countly.Instance.RequestHelper.AddRequestToQueue(request);

            url = "https://xyz.com/i?app_key=772c091355076ead703f987fee94490&device_id=57049b51faf44874a10967f54d8f8420&sdk_name=csharp-unity-editor&sdk_version=20.11.3&timestamp=1633595280409&hour=13&dow=4&tz=300&consent=%7b%0a++%22crashes%22%3a+true%2c%0a++%22events%22%3a+true%2c%0a++%22clicks%22%3a+true%2c%0a++%22star-rating%22%3a+true%2c%0a++%22views%22%3a+true%2c%0a++%22users%22%3a+true%2c%0a++%22sessions%22%3a+true%2c%0a++%22push%22%3a+true%2c%0a++%22remote-config%22%3a+true%2c%0a++%22location%22%3a+true%2c%0a++%22feedback%22%3a+true%0a%7d&checksum256=a3c63ddd0fa788eb05c75752533fdb8083960c4c35fb0ed5a689b631d2beb194";
            request = new CountlyRequestModel(url,  null);
            Countly.Instance.RequestHelper.AddRequestToQueue(request);

            url = "https://xyz.com/i?app_key=772c091355076ead703f987fee94490&device_id=57049b51faf44874a10967f54d8f8420&sdk_name=csharp-unity-editor&sdk_version=20.11.2&timestamp=1633595280409&hour=13&dow=4&tz=300&consent=%7b%0a++%22crashes%22%3a+true%2c%0a++%22events%22%3a+true%2c%0a++%22clicks%22%3a+true%2c%0a++%22star-rating%22%3a+true%2c%0a++%22views%22%3a+true%2c%0a++%22users%22%3a+true%2c%0a++%22sessions%22%3a+true%2c%0a++%22push%22%3a+true%2c%0a++%22remote-config%22%3a+true%2c%0a++%22location%22%3a+true%2c%0a++%22feedback%22%3a+true%0a%7d";
            request = new CountlyRequestModel(url,  null);
            Countly.Instance.RequestHelper.AddRequestToQueue(request);

            data = "{\"app_key\":\"772c091355076ead703f987fee94490\",\"device_id\":\"57049b51faf44874a10967f54d8f8420\",\"sdk_name\":\"csharp-unity-editor\",\"sdk_version\":\"20.11.1\",\"timestamp\":1633595280409,\"hour\":13,\"dow\":4,\"tz\":\"300\",\"consent\":{\"crashes\":true,\"events\":true,\"clicks\":true,\"star-rating\":true,\"views\":true,\"users\":true,\"sessions\":true,\"push\":true,\"remote-config\":true,\"location\":true,\"feedback\":true},\"checksum256\":\"a3c63ddd0fa788eb05c75752533fdb8083960c4c35fb0ed5a689b631d2beb194\"}";

            request = new CountlyRequestModel(_serverUrl,  data);
            Countly.Instance.RequestHelper.AddRequestToQueue(request);

            data = "{\"app_key\":\"772c091355076ead703f987fee94490\",\"device_id\":\"57049b51faf44874a10967f54d8f8420\",\"sdk_name\":\"csharp-unity-editor\",\"sdk_version\":\"20.11.0\",\"timestamp\":1633595280409,\"hour\":13,\"dow\":4,\"tz\":\"300\",\"consent\":{\"crashes\":true,\"events\":true,\"clicks\":true,\"star-rating\":true,\"views\":true,\"users\":true,\"sessions\":true,\"push\":true,\"remote-config\":true,\"location\":true,\"feedback\":true},\"checksum256\":\"a3c63ddd0fa788eb05c75752533fdb8083960c4c35fb0ed5a689b631d2beb194\"}";
            request = new CountlyRequestModel(_serverUrl,  data);
            Countly.Instance.RequestHelper.AddRequestToQueue(request);

            Countly.Instance.StorageHelper.CurrentVersion = 1;
            Countly.Instance.StorageHelper.RunMigration();

            int schemaVersion = PlayerPrefs.GetInt(Constants.SchemaVersion);
            Assert.AreEqual(2, schemaVersion);
            Assert.AreEqual(2, Countly.Instance.StorageHelper.CurrentVersion);
            Assert.AreEqual(Countly.Instance.StorageHelper.SchemaVersion, Countly.Instance.StorageHelper.CurrentVersion);

            Assert.AreEqual(6, Countly.Instance.RequestHelper._requestRepo.Count);

            CountlyRequestModel requestModel = Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Dequeue();
            NameValueCollection collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            Dictionary<string, string> queryParams = collection.AllKeys.ToDictionary(t => t, t => collection[t]);

            JObject json = JObject.FromObject(queryParams);

            Assert.AreEqual("772c091355076ead703f987fee94490", json.GetValue("app_key").ToObject<string>());
            Assert.AreEqual("57049b51faf44874a10967f54d8f8420", json.GetValue("device_id").ToObject<string>());
            Assert.AreEqual("csharp-unity-editor", json.GetValue("sdk_name").ToObject<string>());
            Assert.AreEqual("20.11.5", json.GetValue("sdk_version").ToObject<string>());
            Assert.IsTrue(json.ContainsKey("consent"));
            Assert.IsFalse(json.ContainsKey("checksum256"));
            Assert.IsNull(requestModel.RequestUrl);

            requestModel = Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Dequeue();
            collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            queryParams = collection.AllKeys.ToDictionary(t => t, t => collection[t]);

            json = JObject.FromObject(queryParams);
            Assert.AreEqual("772c091355076ead703f987fee94490", json.GetValue("app_key").ToObject<string>());
            Assert.AreEqual("57049b51faf44874a10967f54d8f8420", json.GetValue("device_id").ToObject<string>());
            Assert.AreEqual("csharp-unity-editor", json.GetValue("sdk_name").ToObject<string>());
            Assert.AreEqual("20.11.4", json.GetValue("sdk_version").ToObject<string>());
            Assert.IsTrue(json.ContainsKey("consent"));
            Assert.IsFalse(json.ContainsKey("checksum256"));
            Assert.IsNull(requestModel.RequestUrl);

            requestModel = Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Dequeue();
            collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            queryParams = collection.AllKeys.ToDictionary(t => t, t => collection[t]);

            json = JObject.FromObject(queryParams);
            Assert.AreEqual("772c091355076ead703f987fee94490", json.GetValue("app_key").ToObject<string>());
            Assert.AreEqual("57049b51faf44874a10967f54d8f8420", json.GetValue("device_id").ToObject<string>());
            Assert.AreEqual("csharp-unity-editor", json.GetValue("sdk_name").ToObject<string>());
            Assert.AreEqual("20.11.3", json.GetValue("sdk_version").ToObject<string>());
            Assert.IsTrue(json.ContainsKey("consent"));
            Assert.IsFalse(json.ContainsKey("checksum256"));
            Assert.IsNull(requestModel.RequestUrl);

            requestModel = Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Dequeue();
            collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            queryParams = collection.AllKeys.ToDictionary(t => t, t => collection[t]);

            json = JObject.FromObject(queryParams);
            Assert.AreEqual("772c091355076ead703f987fee94490", json.GetValue("app_key").ToObject<string>());
            Assert.AreEqual("57049b51faf44874a10967f54d8f8420", json.GetValue("device_id").ToObject<string>());
            Assert.AreEqual("csharp-unity-editor", json.GetValue("sdk_name").ToObject<string>());
            Assert.AreEqual("20.11.2", json.GetValue("sdk_version").ToObject<string>());
            Assert.IsTrue(json.ContainsKey("consent"));
            Assert.IsFalse(json.ContainsKey("checksum256"));
            Assert.IsNull(requestModel.RequestUrl);


            requestModel = Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Dequeue();
            collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            queryParams = collection.AllKeys.ToDictionary(t => t, t => collection[t]);

            json = JObject.FromObject(queryParams);
            Assert.AreEqual("772c091355076ead703f987fee94490", json.GetValue("app_key").ToObject<string>());
            Assert.AreEqual("57049b51faf44874a10967f54d8f8420", json.GetValue("device_id").ToObject<string>());
            Assert.AreEqual("csharp-unity-editor", json.GetValue("sdk_name").ToObject<string>());
            Assert.AreEqual("20.11.1", json.GetValue("sdk_version").ToObject<string>());
            Assert.IsTrue(json.ContainsKey("consent"));
            Assert.IsFalse(json.ContainsKey("checksum256"));
            Assert.IsNull(requestModel.RequestUrl);

            requestModel = Countly.Instance.CrashReports._requestCountlyHelper._requestRepo.Dequeue();
            collection = HttpUtility.ParseQueryString(requestModel.RequestData);
            queryParams = collection.AllKeys.ToDictionary(t => t, t => collection[t]);

            json = JObject.FromObject(queryParams);
            Assert.AreEqual("772c091355076ead703f987fee94490", json.GetValue("app_key").ToObject<string>());
            Assert.AreEqual("57049b51faf44874a10967f54d8f8420", json.GetValue("device_id").ToObject<string>());
            Assert.AreEqual("csharp-unity-editor", json.GetValue("sdk_name").ToObject<string>());
            Assert.AreEqual("20.11.0", json.GetValue("sdk_version").ToObject<string>());
            Assert.IsTrue(json.ContainsKey("consent"));
            Assert.IsFalse(json.ContainsKey("checksum256"));
            Assert.IsNull(requestModel.RequestUrl);

        }

        [TearDown]
        public void End()
        {
            Countly.Instance.ClearStorage();
            Object.DestroyImmediate(Countly.Instance);
        }
    }
}