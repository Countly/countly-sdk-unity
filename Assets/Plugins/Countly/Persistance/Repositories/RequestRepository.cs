using Plugins.Countly.Helpers;
using Plugins.Countly.Models;
using Plugins.Countly.Persistance.Entities;
using Plugins.iBoxDB;

namespace Plugins.Countly.Persistance.Repositories
{
    internal class RequestRepository : Repository<RequestEntity, CountlyRequestModel>
    {
        public RequestRepository(Dao<RequestEntity> dao) : base(dao)
        {
        }

        protected override CountlyRequestModel ConvertEntityToModel(RequestEntity entity)
        {
            return Converter.ConvertRequestEntityToRequestModel(entity);
        }

        protected override RequestEntity ConvertModelToEntity(CountlyRequestModel model)
        {
            return Converter.ConvertRequestModelToRequestEntity(model, GenerateNewId());
        }

        protected override bool ValidateModelBeforeEnqueue(CountlyRequestModel model)
        {
            return true;
        }
    }
}