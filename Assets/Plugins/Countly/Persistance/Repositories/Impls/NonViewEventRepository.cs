using Plugins.Countly.Models;
using Plugins.Countly.Persistance.Dao;
using Plugins.Countly.Persistance.Entities;
using Plugins.iBoxDB;

namespace Plugins.Countly.Persistance.Repositories.Impls
{
    public class NonViewEventRepository : AbstractEventRepository
    {
        public NonViewEventRepository(Dao<EventEntity> dao, SegmentDao segmentDao) : base(dao, segmentDao)
        {
        }

        protected override bool ValidateModelBeforeEnqueue(CountlyEventModel model)
        {
            return !model.Key.Equals(CountlyEventModel.ViewEvent);
        }
    }
}