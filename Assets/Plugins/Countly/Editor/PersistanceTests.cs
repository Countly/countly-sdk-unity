using System;
using System.Collections.Generic;
using iBoxDB.LocalServer;
using NUnit.Framework;
using Plugins.Countly.Helpers;
using Plugins.Countly.Models;
using Plugins.Countly.Persistance;
using Plugins.Countly.Persistance.Entities;
using UnityEngine;

namespace Plugins.Countly.Editor
{
//    public class PersistanceTests
//    {
//        private const long LocalAddress = 4;
//        private DB _db;
//
//        [SetUp]
//        public void SetUp()
//        {
//            _db = InitDb(LocalAddress);
//        }
//
//        [TearDown]
//        public void TearDown()
//        {
//            _db.Close();
//        }
//
//        [Test]
//        public void SaveEvent()
//        {
//            var auto =  _db.Open();
//            var @event = new CountlyEventModel("testKey");
//            @event.Timezone = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes;
//            var eventEntity = Converter.ConvertEventModelToEventEntity(@event, @event.Id);
//            
//            var res = auto.Insert(EntityType.ViewEvents.ToString(), eventEntity);
//            Assert.True(res);
//
//            var loadedEvent = auto.Get<EventEntity>(EntityType.ViewEvents.ToString(), eventEntity.Id);
//            Debug.Log("originalEvent: \n" + eventEntity);
//            Debug.Log("loadedEvent: \n" + loadedEvent);
//            Assert.AreEqual(eventEntity, loadedEvent);
//
//            var loadedModel = Converter.ConvertEventEntityToEventModel(loadedEvent);
//              
//            Assert.AreEqual(@event, loadedModel);
//
//        }
//        
//        [Test]
//        public void SaveRequest()
//        {
//            var request = new CountlyRequestModel();
//            request.Id = 1;
//            request.RequestUrl =
//                "https://try.count.ly/i?app_key=be591e70ff9aa4dd0857ff47e31a93b997379934&device_id=954CA5A3-2CBE-573B-B358-E1571A322DD0&timestamp=1558562339197&hour=23&dow=3&crash=%7b%0a++%22_os%22%3a+%22osxeditor%22%2c%0a++%22_os_version%22%3a+%22Mac+OS+X+10.14.3%22%2c%0a++%22_manufacture%22%3a+%22MacBookPro11%2c3%22%2c%0a++%22_device%22%3a+%22nazgul%e2%80%99s+MacBook+Pro%22%2c%0a++%22_resolution%22%3a+%222880+x+1800+%40+60Hz%22%2c%0a++%22_app_version%22%3a+%220.1%22%2c%0a++%22_cpu%22%3a+%22Intel(R)+Core(TM)+i7-4850HQ+CPU+%40+2.30GHz%22%2c%0a++%22_opengl%22%3a+%22OpenGL+ES+3.0+%5bemulated%5d%22%2c%0a++%22_ram_total%22%3a+%2216384%22%2c%0a++%22_bat%22%3a+%220.29%22%2c%0a++%22_orientation%22%3a+%22Portrait%22%2c%0a++%22_online%22%3a+%22False%22%2c%0a++%22_name%22%3a+%22BeginSessionAsync+error%3a+IsSuccess%3a+False%2c+ErrorMessage%3a+Error%3a+NameResolutionFailure%2c+Data%3a+%22%2c%0a++%22_error%22%3a+%22UnityEngine.Debug%3aLogError(Object)%5cnPlugins.Countly.Services.%3cSetDefaults%3ec__async0%3aMoveNext()+(at+Assets%2fPlugins%2fCountly%2fServices%2fInitializationCountlyService.cs%3a55)%5cnSystem.Runtime.CompilerServices.AsyncTaskMethodBuilder%601%3aSetResult(CountlyResponse)%5cnPlugins.Countly.Services.%3cBeginSessionAsync%3ec__async3%3aMoveNext()+(at+Assets%2fPlugins%2fCountly%2fServices%2fSessionCountlyService.cs%3a141)%5cnSystem.Runtime.CompilerServices.AsyncTaskMethodBuilder%601%3aSetResult(CountlyResponse)%5cnPlugins.Countly.Services.%3cExecuteBeginSessionAsync%3ec__async1%3aMoveNext()+(at+Assets%2fPlugins%2fCountly%2fServices%2fSessionCountlyService.cs%3a93)%5cnUnityEngine.UnitySynchronizationContext%3aExecuteTasks()%5cn%22%2c%0a++%22_nonfatal%22%3a+false%2c%0a++%22_logs%22%3a+%22%22%0a%7d";
//            request.RequestDateTime = DateTime.Now;
//            
//
//            var entity = Converter.ConvertRequestModelToRequestEntity(request, 1);
//
//            var auto =  _db.Open();
//            
////            var res = auto.Insert(EntityType.Requests.ToString(), request1);
//            var res = auto.Insert(EntityType.Requests.ToString(), entity);
//            Assert.True(res);
//
////            requestEntity.Uid = Guid.NewGuid().ToString();
////            requestEntity.Json = ""
//////            RequestRepo enqueue failed, request: IsRequestGetType: True, RequestUrl: https://try.count.ly/i?app_key=be591e70ff9aa4dd0857ff47e31a93b997379934&device_id=954CA5A3-2CBE-573B-B358-E1571A322DD0&timestamp=1558561988388&hour=23&dow=3&crash=%7b%0a++%22_os%22%3a+%22osxeditor%22%2c%0a++%22_os_version%22%3a+%22Mac+OS+X+10.14.3%22%2c%0a++%22_manufacture%22%3a+%22MacBookPro11%2c3%22%2c%0a++%22_device%22%3a+%22nazgul%e2%80%99s+MacBook+Pro%22%2c%0a++%22_resolution%22%3a+%222880+x+1800+%40+60Hz%22%2c%0a++%22_app_version%22%3a+%220.1%22%2c%0a++%22_cpu%22%3a+%22Intel(R)+Core(TM)+i7-4850HQ+CPU+%40+2.30GHz%22%2c%0a++%22_opengl%22%3a+%22OpenGL+ES+3.0+%5bemulated%5d%22%2c%0a++%22_ram_total%22%3a+%2216384%22%2c%0a++%22_bat%22%3a+%220.23%22%2c%0a++%22_orientation%22%3a+%22Portrait%22%2c%0a++%22_online%22%3a+%22False%22%2c%0a++%22_name%22%3a+%22BeginSessionAsync+error%3a+IsSuccess%3a+False%2c+ErrorMessage%3a+Error%3a+NameResolutionFailure%2c+Data%3a+%22%2c%0a++%22_error%22%3a+%22UnityEngine.Debug%3aLogError(Object)%5cnPlugins.Countly.Services.%3cSetDefaults%3ec__async0%3aMoveNext()+(at+Assets%2fPlugins%2fCountly%2fServices%2fInitializationCountlyService.cs%3a55)%5cnSystem.Runtime.CompilerServices.AsyncTaskMethodBuilder%601%3aSetResult(CountlyResponse)%5cnPlugins.Countly.Services.%3cBeginSessionAsync%3ec__async3%3aMoveNext()+(at+Assets%2fPlugins%2fCountly%2fServices%2fSessionCountlyService.cs%3a141)%5cnSystem.Runtime.CompilerServices.AsyncTaskMethodBuilder%601%3aSetResult(CountlyResponse)%5cnPlugins.Countly.Services.%3cExecuteBeginSessionAsync%3ec__async1%3aMoveNext()+(at+Assets%2fPlugins%2fCountly%2fServices%2fSessionCountlyService.cs%3a93)%5cnUnityEngine.UnitySynchronizationContext%3aExecuteTasks()%5cn%22%2c%0a++%22_nonfatal%22%3a+false%2c%0a++%22_logs%22%3a+%22%22%0a%7d, RequestData: , RequestDateTime: 5/22/2019 9:53:08 PM, Guid: fc10566e-cac1-4e09-aa4e-147d89d4c57b
//
//        }
//
//        [Test]
//        public void SaveLoadSegments()
//        {
//            var auto = _db.Open();
//
//            var dict = new Dictionary<string, object>()
//            {
//                {"key", 2121}
//            };
//            
//            var segment = new SegmentModel(dict);
//
//            var segmentEntity = Converter.ConvertSegmentModelToSegmentEntity(segment, segment.Id);
//
//            var res = auto.Insert(EntityType.ViewEventSegments.ToString(), segmentEntity);
//            Assert.True(res);
//
//            var loadedSegment = auto.Get<SegmentEntity>(EntityType.ViewEventSegments.ToString(), segmentEntity.Id);
//
//            Debug.Log("originalSegment: \n" + segmentEntity);
//            Debug.Log("loadedSegment: \n" + loadedSegment);
//            
//            Assert.AreEqual(segmentEntity, loadedSegment);
//        }
//        
//        private static DB InitDb(long localAddress)
//        {
//            DB.Root(Application.persistentDataPath);
//            var db = new DB(localAddress);
//
//            db.GetConfig().EnsureTable<CountlyEventModel>("CountlyEvents", "Id");
//            db.GetConfig().EnsureTable<RequestEntity>(EntityType.Requests.ToString(), "Id");
//            db.GetConfig().EnsureTable<EventEntity>(EntityType.ViewEvents.ToString(), "Id");
//            db.GetConfig().EnsureTable<SegmentEntity>(EntityType.ViewEventSegments.ToString(), "Id");
//            db.GetConfig().EnsureTable<UnityDBCS.Player>("Players", "ID");
//            
//            {
//                // [Optional]
//                // if device has small memory & disk
//                db.MinConfig();
//                // smaller DB file size
//                db.GetConfig().DBConfig.FileIncSize = 1;
//            }
//
//            return db;
//        }
//    }
}