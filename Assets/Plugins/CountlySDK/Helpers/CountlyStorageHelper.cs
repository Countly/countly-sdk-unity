using iBoxDB.LocalServer;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Persistance.Dao;
using Plugins.CountlySDK.Persistance.Entities;
using Plugins.iBoxDB;
using UnityEngine;

namespace Plugins.CountlySDK.Helpers
{
    public enum EntityType
    {
        ViewEvents, NonViewEvents, Requests, ViewEventSegments, NonViewEventSegments, Configs, EventNumberInSameSessions
    }

    internal class CountlyStorageHelper
    {
        private DB _db;
        private int _schemaVersion = 1;
        private const long _dbNumber = 3;
        private CountlyConfiguration _configuration;

        internal SegmentDao ViewSegmentDao { get; private set; }
        internal SegmentDao NonViewSegmentDao { get; private set; }

        internal Dao<ConfigEntity> ConfigDao { get; private set; }
        internal Dao<RequestEntity> RequestDao { get; private set; }
        internal Dao<EventEntity> ViewEventDao { get; private set; }
        internal Dao<EventEntity> NonViewEventDao { get; private set; }
        internal Dao<EventNumberInSameSessionEntity> EventNrInSameSessionDao { get; private set; }

        internal CountlyStorageHelper(CountlyConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Create database and tables
        /// </summary>
        private DB BuildDatabase(long dbNumber)
        {
            DB.Root(Application.persistentDataPath);
            DB db = new DB(dbNumber);

            db.GetConfig().EnsureTable<SegmentEntity>(EntityType.Configs.ToString(), "Id");
            db.GetConfig().EnsureTable<RequestEntity>(EntityType.Requests.ToString(), "Id");
            db.GetConfig().EnsureTable<EventEntity>(EntityType.ViewEvents.ToString(), "Id");
            db.GetConfig().EnsureTable<EventEntity>(EntityType.NonViewEvents.ToString(), "Id");
            db.GetConfig().EnsureTable<SegmentEntity>(EntityType.ViewEventSegments.ToString(), "Id");
            db.GetConfig().EnsureTable<SegmentEntity>(EntityType.NonViewEventSegments.ToString(), "Id");
            db.GetConfig().EnsureTable<EventNumberInSameSessionEntity>(EntityType.EventNumberInSameSessions.ToString(), "Id");

            return db;
        }

        /// <summary>
        /// Open database connection and initialize daos.
        /// </summary>
        internal void OpenDB()
        {
            _db = BuildDatabase(_dbNumber);
            DB.AutoBox auto = _db.Open();

            ViewSegmentDao = new SegmentDao(auto, EntityType.ViewEventSegments.ToString(), _configuration);
            NonViewSegmentDao = new SegmentDao(auto, EntityType.NonViewEventSegments.ToString(), _configuration);

            ConfigDao = new Dao<ConfigEntity>(auto, EntityType.Configs.ToString(), _configuration);
            RequestDao = new Dao<RequestEntity>(auto, EntityType.Requests.ToString(), _configuration);
            ViewEventDao = new Dao<EventEntity>(auto, EntityType.ViewEvents.ToString(), _configuration);
            NonViewEventDao = new Dao<EventEntity>(auto, EntityType.NonViewEvents.ToString(), _configuration);
            EventNrInSameSessionDao = new Dao<EventNumberInSameSessionEntity>(auto, EntityType.EventNumberInSameSessions.ToString(), _configuration);

        }

        /// <summary>
        /// Close database connection.
        /// </summary>
        internal void CloseDB()
        {
            _db.Close();
        }

        /// <summary>
        /// Migrate database schema.
        /// </summary>
        internal void RunMigration()
        {
            int currentVersion;
            if (FirstLaunchAppHelper.IsFirstLaunchApp) {
                currentVersion = _schemaVersion;
            } else {
                currentVersion = PlayerPrefs.GetInt(Constants.SchemaVersion, 0);
            }

            // _schemaVersion = 1 : deletion of the data in the “EventNumberInSameSessionEntity” table
            if (currentVersion == 0) {
                EventNrInSameSessionDao.RemoveAll();
                PlayerPrefs.SetInt(Constants.SchemaVersion, 1);
            }

            PlayerPrefs.SetInt(Constants.SchemaVersion, _schemaVersion);
        }
    }
}