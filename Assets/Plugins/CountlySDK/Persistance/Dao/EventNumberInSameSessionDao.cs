using System;
using System.Text;
using iBoxDB.LocalServer;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Persistance.Entities;
using Plugins.iBoxDB;
using UnityEngine;

namespace Plugins.CountlySDK.Persistance.Dao
{
    public class EventNumberInSameSessionDao : Dao<EventNumberInSameSessionEntity>
    {
        private readonly CountlyConfiguration configuration;
        private readonly StringBuilder _stringBuilder = new StringBuilder();

        public EventNumberInSameSessionDao(AutoBox auto, string table, CountlyConfiguration config) : base(auto, table, config)
        {
            configuration = config;
        }

        public EventNumberInSameSessionEntity GetByEventName(string eventKey)
        {
            _stringBuilder.Clear();

            string ql = _stringBuilder.Append("from ").Append(Table)
                .Append(" where EventKey==?").ToString();

            try {

                System.Collections.Generic.List<EventNumberInSameSessionEntity> entities = Auto.Select<EventNumberInSameSessionEntity>(ql, eventKey);

                if (entities.Count > 1) {
                    throw new ArgumentException("Only one or zero event can be assigned to entity with key " + eventKey + ". "
                                                + entities.Count + " events found.");
                }

                if (entities.Count == 0) {
                    return null;
                }

                return entities[0];
            } catch (Exception ex) {
                if (configuration.EnableConsoleLogging) {
                    Debug.LogError("[Countly] EventNumberInSameSessionDao GetByEventName: Couldn't complete db operation, [" + ex.Message + "]");

                }
                return null;
            }
        }
    }
}