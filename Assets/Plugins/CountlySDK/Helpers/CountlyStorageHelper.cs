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
        private int _currentVersion = 0;
        private const long _dbNumber = 3;
        private const int _schemaVersion = 1;

        private CountlyLogHelper _logHelper;

        internal SegmentDao ViewSegmentDao { get; private set; }
        internal SegmentDao NonViewSegmentDao { get; private set; }

        internal Dao<ConfigEntity> ConfigDao { get; private set; }
        internal Dao<RequestEntity> RequestDao { get; private set; }
        internal Dao<EventEntity> ViewEventDao { get; private set; }
        internal Dao<EventEntity> NonViewEventDao { get; private set; }
        internal Dao<EventNumberInSameSessionEntity> EventNrInSameSessionDao { get; private set; }

        internal CountlyStorageHelper(CountlyLogHelper logHelper)
        {
            _logHelper = logHelper;

            if (FirstLaunchAppHelper.IsFirstLaunchApp) {
                _currentVersion = _schemaVersion;
            } else {
                _currentVersion = PlayerPrefs.GetInt(Constants.SchemaVersion, 0);
            }
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

            if (_currentVersion < _schemaVersion) {
                db.GetConfig().EnsureTable<EventNumberInSameSessionEntity>(EntityType.EventNumberInSameSessions.ToString(), "Id");
            }

            return db;
        }

        /// <summary>
        /// Open database connection and initialize data access objects.
        /// </summary>
        internal void OpenDB()
        {
            _logHelper.Debug("[CountlyStorageHelper] OpenDB");

            _db = BuildDatabase(_dbNumber);
            DB.AutoBox auto = _db.Open();

            ViewSegmentDao = new SegmentDao(auto, EntityType.ViewEventSegments.ToString(), _logHelper);
            NonViewSegmentDao = new SegmentDao(auto, EntityType.NonViewEventSegments.ToString(), _logHelper);

            ConfigDao = new Dao<ConfigEntity>(auto, EntityType.Configs.ToString(), _logHelper);
            RequestDao = new Dao<RequestEntity>(auto, EntityType.Requests.ToString(), _logHelper);
            ViewEventDao = new Dao<EventEntity>(auto, EntityType.ViewEvents.ToString(), _logHelper);
            NonViewEventDao = new Dao<EventEntity>(auto, EntityType.NonViewEvents.ToString(), _logHelper);
            EventNrInSameSessionDao = new Dao<EventNumberInSameSessionEntity>(auto, EntityType.EventNumberInSameSessions.ToString(), _logHelper);

        }

        /// <summary>
        /// Close database connection.
        /// </summary>
        internal void CloseDB()
        {
            _logHelper.Debug("[CountlyStorageHelper] CloseDB");

            _db.Close();
        }

        /// <summary>
        /// Migrate database schema.
        /// </summary>
        internal void RunMigration()
        {
            _logHelper.Verbose("[CountlyStorageHelper] RunMigration : currentVersion = " + _currentVersion);

            // _schemaVersion = 1 : deletion of the data in the “EventNumberInSameSessionEntity” table
            if (_currentVersion == 0) {
                Migration_EventNumberInSameSessionEntityDataRemoval();
            }

            PlayerPrefs.SetInt(Constants.SchemaVersion, _schemaVersion);
        }

        /// <summary>
        /// Migration to version 1: Deletion Of data in the 'EventNumberInSameSessionEntity' table.
        /// </summary>
        private void Migration_EventNumberInSameSessionEntityDataRemoval()
        {
            EventNrInSameSessionDao.RemoveAll();
            PlayerPrefs.SetInt(Constants.SchemaVersion, 1);
            _logHelper.Verbose("[CountlyStorageHelper] Migration_EventNumberInSameSessionEntityDataRemoval");
        }
    }
}