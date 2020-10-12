using Plugins.Countly.Models;
using Plugins.Countly.Persistance.Dao;
using Plugins.Countly.Persistance.Entities;
using Plugins.iBoxDB;
using UnityEngine;

namespace Plugins.Countly.Persistance.Repositories.Impls
{
    public class ViewEventRepository : AbstractEventRepository
    {
        
        public ViewEventRepository(Dao<EventEntity> dao, SegmentDao segmentDao, CountlyConfigModel config) : base(dao, segmentDao, config)
        {
        }

        protected override bool ValidateModelBeforeEnqueue(CountlyEventModel model)
        {
            if (_config.EnableConsoleErrorLogging)
            {
                Debug.Log("[ViewEventRepository] Validate model: \n" + model);
            }
            return model.Key.Equals(CountlyEventModel.ViewEvent);
        }
    }
}