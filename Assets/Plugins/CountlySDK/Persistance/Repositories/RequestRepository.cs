﻿using Plugins.CountlySDK.Helpers;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Persistance.Entities;
using Plugins.iBoxDB;

namespace Plugins.CountlySDK.Persistance.Repositories
{
    internal class RequestRepository : Repository<RequestEntity, CountlyRequestModel>
    {
        public RequestRepository(Dao<RequestEntity> dao, CountlyConfiguration config) : base(dao, config)
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