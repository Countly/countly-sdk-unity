using System;
using System.Collections.Generic;
using iBoxDB.LocalServer;
using Plugins.CountlySDK.Persistance.Entities;
using UnityEngine;

namespace Plugins.iBoxDB
{
    public class Dao<TEntity> where TEntity : class, IEntity, new()
    {
        protected readonly AutoBox Auto;
        protected readonly string Table;

        public Dao(AutoBox auto, string table)
        {
            Auto = auto;
            Table = table;
        }

        public bool Save(TEntity entity)
        {
            try
            {
                return Auto.Insert(Table, entity);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

            return false;
        }

        public bool Update(TEntity entity)
        {
            try
            {
                return Auto.Update(Table, entity);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

            return false;
        }

        public List<TEntity> LoadAll()
        {
            List<TEntity> result = new List<TEntity>();
            try
            {
                result = Auto.Select<TEntity>("from " + Table);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

            return result;
        }

        public void Remove(params object[] key)
        {
            try
            {
                Auto.Delete(Table, key);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        public void RemoveAll()
        {
            try
            {
                var list = Auto.Select<TEntity>("from " + Table);
                foreach (var entity in list)
                {
                    Auto.Delete(Table, entity.GetId());
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        public long GenerateNewId()
        {
            long result;
            try
            {
                result = Auto.NewId();
            }
            catch (Exception ex)
            {
                result = 0;
                Debug.LogError(ex.Message);
            }

            return result;
        }
    }
}