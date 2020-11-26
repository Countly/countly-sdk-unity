using System.Collections.Generic;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Persistance.Entities;
using Plugins.iBoxDB;
using UnityEngine;

namespace Plugins.CountlySDK.Persistance.Repositories
{
    public abstract class Repository<TEntity, TModel> where TEntity : class, IEntity, new() where TModel : IModel
    {
        private readonly Dao<TEntity> _dao;
        private readonly CountlyConfiguration _config;

        protected Repository(Dao<TEntity> dao, CountlyConfiguration config)
        {
            _dao = dao;
            _config = config;
        }
        
        internal Queue<TModel> Models { get; } = new Queue<TModel>();

        internal int Count => Models.Count;

        public virtual void Initialize()
        {
            var entities = _dao.LoadAll();
            foreach (var entity in entities)
            {
                var model = ConvertEntityToModel(entity);
                if (!ValidateModelBeforeEnqueue(model)) continue;

                if (_config.EnableConsoleLogging)
                {
                    Debug.Log("Loaded model: " + model);
                }

                Models.Enqueue(model);
            }
            if (_config.EnableConsoleLogging)
            {
                Debug.Log("Loaded entities of type " + typeof(TEntity).Name + " from db:" + Count);
            }
        }

        public virtual bool Enqueue(TModel model)
        {
            if (!ValidateModelBeforeEnqueue(model))
                return false;
            Models.Enqueue(model);
            var entity = ConvertModelToEntity(model);
            var res = _dao.Save(entity);
            if (!res && _config.EnableConsoleLogging)
            {
                Debug.LogError("Request entity save failed, entity: " + entity);
            }

            return res;
        }

        public virtual TModel Dequeue()
        {
            var model = Models.Dequeue();
//            Debug.Log("Dequeue model " + typeof(TModel) + ", model: \n" + model);
            _dao.Remove(model.Id);
            return model;
        }
        

        public virtual void Clear()
        {
            Models.Clear();
            _dao.RemoveAll();
        }

        protected abstract TModel ConvertEntityToModel(TEntity entity);
        protected abstract TEntity ConvertModelToEntity(TModel model);

        protected long GenerateNewId()
        {
            return _dao.GenerateNewId();
        }

        protected abstract bool ValidateModelBeforeEnqueue(TModel model);
    }
}