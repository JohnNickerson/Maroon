using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Model;
using LiteDB;

namespace AssimilationSoftware.Maroon.Mappers.LiteDb
{
    public class BaseLiteDbMapper<T> where T : ModelObject
    {
        private readonly LiteDatabase _liteDb;
        private readonly ILiteCollection<T> _allTs;

        public BaseLiteDbMapper(string filename, string collection)
        {
            _liteDb = new LiteDatabase(filename);
            _allTs = _liteDb.GetCollection<T>(collection);
            _allTs.EnsureIndex(n => n.ID);
        }

        ~BaseLiteDbMapper()
        {
            _liteDb.Dispose();
        }

        public T Load(Guid id)
        {
            return _allTs.FindOne(n => n.ID == id);
        }

        public IEnumerable<T> LoadAll()
        {
            return _allTs.FindAll();
        }

        public void Save(T item)
        {
            if (_allTs.Exists(n => n.ID == item.ID))
            {
                _allTs.Update(item);
            }
            else
            {
                _allTs.Insert(item);
            }
        }

        public void SaveAll(IEnumerable<T> items)
        {
            foreach (var n in items)
            {
                // To insert or update as required.
                Save(n);
            }
        }

        public void Delete(T item)
        {
            _allTs.DeleteMany(n => n.ID == item.ID);
        }
    }
}
