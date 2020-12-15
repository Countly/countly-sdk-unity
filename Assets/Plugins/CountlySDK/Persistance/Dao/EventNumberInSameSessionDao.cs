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
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        
        public EventNumberInSameSessionDao(AutoBox auto, string table, CountlyConfiguration config) : base(auto, table, config)
        {
        }

        public EventNumberInSameSessionEntity GetByEventName(string eventKey)
        {
            _stringBuilder.Clear();
            
            var ql = _stringBuilder.Append("from ").Append(Table)
                .Append(" where EventKey==?").ToString();

            try
            {

                var entities = Auto.Select<EventNumberInSameSessionEntity>(ql, eventKey);

                if (entities.Count > 1)
                {
                    throw new ArgumentException("Only one or zero event can be assigned to entity with key " + eventKey + ". "
                                                + entities.Count + " events found.");
                }

                if (entities.Count == 0)
                    return null;

                return entities[0];
            }
            catch (Exception ex)
            {
                Debug.LogError("[Countly] EventNumberInSameSessionEntity GetByEventName: Couldn't complete db operation, [" + ex.Message + "]");
                return null;
            }
        }
    }
}