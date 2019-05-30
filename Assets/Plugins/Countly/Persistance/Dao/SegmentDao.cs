using System;
using System.Text;
using iBoxDB.LocalServer;
using Plugins.Countly.Persistance.Entities;
using Plugins.iBoxDB;

namespace Plugins.Countly.Persistance.Dao
{
    public class SegmentDao : Dao<SegmentEntity>
    {
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        
        public SegmentDao(AutoBox auto, string table) : base(auto, table)
        {
        }


        public SegmentEntity GetByEventId(long eventId)
        {
            _stringBuilder.Clear();

            var ql = _stringBuilder.Append("from ").Append(Table).Append(" where EventId = ")
                .Append(eventId).ToString();

            var entities =  Auto.Select<SegmentEntity>(ql);
            if (entities.Count > 1)
            {
                throw new ArgumentException("Only one or zero segment can be assigned to entity with id " + eventId + ". " 
                                            + entities + " segments found.");
            }

            if (entities.Count == 0)
                return null;
            
            return entities[0];
        }
        
    }
}