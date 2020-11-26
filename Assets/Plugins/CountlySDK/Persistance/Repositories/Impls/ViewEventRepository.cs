using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Persistance.Dao;
using Plugins.CountlySDK.Persistance.Entities;
using Plugins.iBoxDB;
using UnityEngine;

namespace Plugins.CountlySDK.Persistance.Repositories.Impls
{
    public class ViewEventRepository : AbstractEventRepository
    {
        
        public ViewEventRepository(Dao<EventEntity> dao, SegmentDao segmentDao, CountlyConfiguration config) : base(dao, segmentDao, config)
        {
        }

        protected override bool ValidateModelBeforeEnqueue(CountlyEventModel model)
        {
            if (_config.EnableConsoleLogging)
            {
                Debug.Log("[ViewEventRepository] Validate model: \n" + model);
            }
            return model.Key.Equals(CountlyEventModel.ViewEvent);
        }
    }
}