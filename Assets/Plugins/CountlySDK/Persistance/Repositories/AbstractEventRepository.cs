using Plugins.CountlySDK.Helpers;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Persistance.Dao;
using Plugins.CountlySDK.Persistance.Entities;
using Plugins.iBoxDB;
using UnityEngine;

namespace Plugins.CountlySDK.Persistance.Repositories
{
    public abstract class AbstractEventRepository : Repository<EventEntity, CountlyEventModel>
    {
        private readonly SegmentDao _segmentDao;
        protected readonly CountlyConfiguration _config;

        protected AbstractEventRepository(Dao<EventEntity> dao, SegmentDao segmentDao, CountlyConfiguration config) : base(dao, config)
        {
            _config = config;
            _segmentDao = segmentDao;
            
        }

        public override void Initialize()
        {
            base.Initialize();
            foreach (CountlyEventModel model in Models)
            {
                SegmentEntity segmentEntity = _segmentDao.GetByEventId(model.Id);
                if(segmentEntity == null)
                {
                    continue;
                }

                SegmentModel segmentModel = Converter.ConvertSegmentEntityToSegmentModel(segmentEntity);
                @model.Segmentation = segmentModel;
            }
        }

        protected override CountlyEventModel ConvertEntityToModel(EventEntity entity)
        {
            return Converter.ConvertEventEntityToEventModel(entity);
        }    

        protected override EventEntity ConvertModelToEntity(CountlyEventModel model)
        {
            return Converter.ConvertEventModelToEventEntity(model, GenerateNewId());
        }

        public override bool Enqueue(CountlyEventModel model)
        {
            bool res = base.Enqueue(model);
            if (!res)
            {
                return false;
            }

            SegmentModel segmentModel = model.Segmentation;
            if (segmentModel != null)
            {
                SegmentEntity segmentEntity = Converter.ConvertSegmentModelToSegmentEntity(segmentModel, _segmentDao.GenerateNewId());
                segmentEntity.EventId = model.Id;
                _segmentDao.Save(segmentEntity);
            }

            if (_config.EnableConsoleLogging)
            {
                Debug.Log("[" + GetType().Name + "] Event repo enqueue: \n" + model + ", segment: " + segmentModel);
            }
            return true;
        }

        public override CountlyEventModel Dequeue()
        {
            CountlyEventModel @event =  base.Dequeue();
            SegmentEntity segmentEntity = _segmentDao.GetByEventId(@event.Id);
            if (segmentEntity != null)
            {
                SegmentModel segmentModel = Converter.ConvertSegmentEntityToSegmentModel(segmentEntity);
                @event.Segmentation = segmentModel;   
            }
            return @event;
        }

        public override void Clear()
        {
            base.Clear();
            _segmentDao.RemoveAll();
        }
    }
}