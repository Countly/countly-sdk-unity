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
        private readonly CountlyConfiguration Configuration;

        public Dao(AutoBox auto, string table, CountlyConfiguration configuration)
        {
            Auto = auto;
            Table = table;
            Configuration = configuration;
        }

        public bool Save(TEntity entity)
        {
            try
            {
                return Auto.Insert(Table, entity);
            }
            catch (Exception ex)
            {
                if (Configuration.EnableConsoleLogging)
                {
                    Debug.LogError("[Countly] Dao Save: Couldn't complete db operation, [" + ex.Message + "]");
                }
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
                if (Configuration.EnableConsoleLogging)
                {
                    Debug.LogError("[Countly] Dao Update: Couldn't complete db operation, [" + ex.Message + "]");
                }
                
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
                if (Configuration.EnableConsoleLogging)
                {
                    Debug.LogError("[Countly] Dao LoadAll: Couldn't complete db operation, [" + ex.Message + "]");
                }
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
                if (Configuration.EnableConsoleLogging)
                {
                    Debug.LogError("[Countly] Dao Remove: Couldn't complete db operation, [" + ex.Message + "]");
                }
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
                if (Configuration.EnableConsoleLogging)
                {
                    Debug.LogError("[Countly] Dao RemoveAll: Couldn't complete db operation, [" + ex.Message + "]");
                }
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
                if (Configuration.EnableConsoleLogging)
                {
                    Debug.LogError("[Countly] Dao GenerateNewId: Couldn't complete db operation, [" + ex.Message + "]");
                }
            }

            return result;
        }
    }
}