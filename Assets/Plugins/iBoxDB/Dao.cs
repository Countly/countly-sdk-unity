using System;
using System.Collections.Generic;
using iBoxDB.LocalServer;
using Plugins.CountlySDK.Helpers;
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

            if (!CountlyStorageHelper.IsDbOpen) {
                Log.Warning("[Dao] Save: Couldn't complete db operation.");
                return false;
            }

            return Auto.Insert(Table, entity);
        }

        public bool Update(TEntity entity)
        {

            if (!CountlyStorageHelper.IsDbOpen) {
                Log.Warning("[Dao] Update: Couldn't complete db operation.");
                return false;
            }

            return Auto.Update(Table, entity);
        }

        public List<TEntity> LoadAll()
        {
            List<TEntity> result = new List<TEntity>();

            if (!CountlyStorageHelper.IsDbOpen) {
                Log.Warning("[Dao] LoadAll: Couldn't complete db operation.");
                return result;
            }

            result = Auto.Select<TEntity>("from " + Table);

            return result;
        }

        public void Remove(params object[] key)
        {
            if (!CountlyStorageHelper.IsDbOpen) {
                Log.Warning("[Dao] Remove: Couldn't complete db operation.");
                return;
            }

            Auto.Delete(Table, key);
        }

        public void RemoveAll()
        {

            if (!CountlyStorageHelper.IsDbOpen) {
                Log.Warning("[Dao] RemoveAll: Couldn't complete db operation.");
                return;
            }

            List<TEntity> list = Auto.Select<TEntity>("from " + Table);
            foreach (TEntity entity in list) {
                Auto.Delete(Table, entity.GetId());
            }
        }

        public long GenerateNewId()
        {

            if (!CountlyStorageHelper.IsDbOpen) {
                Log.Warning("[Dao] GenerateNewId: Couldn't complete db operation.");
                return 0;
            }

            return Auto.NewId();
        }
    }
}
