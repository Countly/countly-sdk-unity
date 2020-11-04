using System;
using System.Text;
using iBoxDB.LocalServer;
using Plugins.CountlySDK.Persistance.Entities;
using Plugins.iBoxDB;

namespace Plugins.CountlySDK.Persistance.Dao
{
    public class EventNumberInSameSessionDao : Dao<EventNumberInSameSessionEntity>
    {
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        
        public EventNumberInSameSessionDao(AutoBox auto, string table) : base(auto, table)
        {
        }

        public EventNumberInSameSessionEntity GetByEventName(string eventKey)
        {
            _stringBuilder.Clear();
            
            var ql = _stringBuilder.Append("from ").Append(Table)
                .Append(" where EventKey==?").ToString();
            
            var entities =  Auto.Select<EventNumberInSameSessionEntity>(ql, eventKey);
            
            if (entities.Count > 1)
            {
                throw new ArgumentException("Only one or zero event can be assigned to entity with key " + eventKey + ". " 
                                            + entities.Count + " events found.");
            }
            
            if (entities.Count == 0)
                return null;

            return entities[0];
        }
    }
}