using System.Collections;
using System.Text;
using iBoxDB.LocalServer;
using NUnit.Framework;
using Plugins.CountlySDK.Helpers;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Persistance;
using Plugins.CountlySDK.Persistance.Dao;
using Plugins.CountlySDK.Persistance.Entities;
using UnityEngine;

namespace Plugins.CountlySDK.Editor
{
    public class EventNumberInSameSessionHelperTest
    {
        private const long LocalAddress = 4;
        private DB _db;
        private AutoBox _auto;
        private EventNumberInSameSessionHelper _eventNumberInSameSessionHelper;

        private static readonly string Table = EntityType.EventNumberInSameSessions.ToString();


//        [SetUp]
        public void SetUp()
        {
            _db = InitDb(LocalAddress);
            _auto = _db.Open();
            var dao = new EventNumberInSameSessionDao(_auto, Table);
            _eventNumberInSameSessionHelper = new EventNumberInSameSessionHelper(dao);
            _eventNumberInSameSessionHelper.RemoveAllEvents();
        }

//        [TearDown]
        public void TearDown()
        {
            _eventNumberInSameSessionHelper.RemoveAllEvents();
            _db.Close();
        }

//        [TestCaseSource(typeof(CountlyEventModelDataProvider), nameof(CountlyEventModelDataProvider.NewEventValidationData))]
        public void UseNumberInSameSession_NewEvent(CountlyEventModel @event)
        {
            _eventNumberInSameSessionHelper.IncreaseNumberInSameSession(@event);


            var ql = new StringBuilder().Append("from ").Append(Table)
                .Append(" where EventKey==?").ToString();

            var entities = _auto.Select<EventNumberInSameSessionEntity>(ql, @event.Key);
            Assert.NotNull(entities);
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual(@event.Key, entities[0].EventKey);

            Assert.NotNull(@event.Segmentation);
            Assert.True(@event.Segmentation.ContainsKey(EventNumberInSameSessionHelper.NumberInSameSessionSegment));

            Assert.AreEqual(1, entities[0].Number);
            Assert.AreEqual(1, @event.Segmentation[EventNumberInSameSessionHelper.NumberInSameSessionSegment]);
        }
        
//        [TestCaseSource(typeof(CountlyEventModelDataProvider), nameof(CountlyEventModelDataProvider.OldEventValidationData))]
        public void UseNumberInSameSession_OldEvent(CountlyEventModel @event)
        {
            const int number = 3;
             var entity = new EventNumberInSameSessionEntity
             {
                 Id = _auto.NewId(),
                 EventKey = @event.Key,
                 Number = 3
             };
             _auto.Insert(Table, entity);
             
            _eventNumberInSameSessionHelper.IncreaseNumberInSameSession(@event);


            var ql = new StringBuilder().Append("from ").Append(Table)
                .Append(" where EventKey==?").ToString();

            var entities = _auto.Select<EventNumberInSameSessionEntity>(ql, @event.Key);
            Assert.NotNull(entities);
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual(@event.Key, entities[0].EventKey);

            Assert.NotNull(@event.Segmentation);
            Assert.True(@event.Segmentation.ContainsKey(EventNumberInSameSessionHelper.NumberInSameSessionSegment));

            Assert.AreEqual(number + 1, entities[0].Number);
            Assert.AreEqual(number + 1, @event.Segmentation[EventNumberInSameSessionHelper.NumberInSameSessionSegment]);
        }

        private static DB InitDb(long localAddress)
        {
            DB.Root(Application.persistentDataPath);
            var db = new DB(localAddress);

            db.GetConfig()
                .EnsureTable<EventNumberInSameSessionEntity>(Table, "Id");
            {
                // [Optional]
                // if device has small memory & disk
                db.MinConfig();
                // smaller DB file size
                db.GetConfig().DBConfig.FileIncSize = 1;
            }

            return db;
        }

        private class CountlyEventModelDataProvider
        {
            
            public static IEnumerable NewEventValidationData
            {
                get
                {
                    yield return new TestCaseData(new CountlyEventModel("testEvent")).SetName("New event: Simple test event without segments");
                    yield return new TestCaseData(new CountlyEventModel("game_started")).SetName("New event: Game started event without segments");
                    yield return new TestCaseData(new CountlyEventModel
                    {
                        Key = "game_ended",
                        Segmentation = new SegmentModel
                        {
                            {"seg", 1},
                        }
                    }).SetName("New event: Game ended event with segments");;
                }
            }
            
            public static IEnumerable OldEventValidationData
            {
                get
                {
                    yield return new TestCaseData(new CountlyEventModel("testEvent")).SetName("Old event: Simple test event without segments");
                    yield return new TestCaseData(new CountlyEventModel("game_started")).SetName("Old event: Game started event without segments");
                    yield return new TestCaseData(new CountlyEventModel
                    {
                        Key = "game_ended",
                        Segmentation = new SegmentModel
                        {
                            {"seg", 1},
                        }
                    }).SetName("Old event: Game ended event with segments");;
                }
            }
            
        }
    }
}