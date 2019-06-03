using System.Collections.Generic;
using iBoxDB.LocalServer;
using Plugins.Countly.Persistance.Entities;

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
            return Auto.Insert(Table, entity);
        }

        public List<TEntity> LoadAll()
        {
            return Auto.Select<TEntity>("from " + Table);
        }

        public void Remove(params object[] key)
        {
            Auto.Delete(Table, key);
        }

        public void RemoveAll()
        {
            var list = Auto.Select<TEntity>("from " + Table);
            foreach (var entity in list)
            {
                Auto.Delete(Table, entity.GetId());    
            }
        }

        public long GenerateNewId()
        {
            return Auto.NewId();
        }
    }
}