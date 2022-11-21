using System;
using System.Collections.Generic;
using iBoxDB.LocalServer;
using Plugins.CountlySDK.Models;
using Plugins.CountlySDK.Persistance.Entities;
using UnityEngine;

namespace Plugins.iBoxDB
{
    public class Dao<TEntity> where TEntity : class, IEntity, new()
    {
        protected readonly AutoBox Auto;
        protected readonly string Table;
        private readonly CountlyLogHelper Log;

        public Dao(AutoBox auto, string table, CountlyLogHelper log)
        {
            Auto = auto;
            Table = table;
            Log = log;
        }

        public bool Save(TEntity entity)
        {
            try {
                return Auto.Insert(Table, entity);
            } catch (Exception ex) {
                Log.Error("[Dao] Save: Couldn't complete db operation, [" + ex.Message + "]");
            }

            return false;
        }

        public bool Update(TEntity entity)
        {
            try {
                return Auto.Update(Table, entity);
            } catch (Exception ex) {
                Log.Error("[Dao] Update: Couldn't complete db operation, [" + ex.Message + "]");
            }

            return false;
        }

        public List<TEntity> LoadAll()
        {
            List<TEntity> result = new List<TEntity>();
            try {
                result = Auto.Select<TEntity>("from " + Table + " order by Id asc");
            } catch (Exception ex) {
                Log.Error("[Dao] LoadAll: Couldn't complete db operation, [" + ex.Message + "]");
            }

            return result;
        }

        public void Remove(params object[] key)
        {
            try {
                Auto.Delete(Table, key);
            } catch (Exception ex) {
                Log.Error("[Dao] Remove: Couldn't complete db operation, [" + ex.Message + "]");
            }
        }

        public void RemoveAll()
        {
            try {
                List<TEntity> list = Auto.Select<TEntity>("from " + Table);
                foreach (TEntity entity in list) {
                    Auto.Delete(Table, entity.GetId());
                }
            } catch (Exception ex) {
                Log.Error("[Dao] RemoveAll: Couldn't complete db operation, [" + ex.Message + "]");
            }
        }

        public long GenerateNewId()
        {
            long result;
            try {
                result = Auto.NewId();
            } catch (Exception ex) {
                result = 0;
                Log.Error("[Dao] GenerateNewId: Couldn't complete db operation, [" + ex.Message + "]");
            }

            return result;
        }
    }
}
