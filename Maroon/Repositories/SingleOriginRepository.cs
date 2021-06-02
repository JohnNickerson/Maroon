using System;
using System.Collections.Generic;
using System.Linq;
using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.Repositories
{
    public class SingleOriginRepository<T> : IRepository<T> where T : ModelObject
    {
        private readonly IDiskMapper<T> _mapper;
        private readonly string _filename;
        private Dictionary<Guid, T> _items;

        public SingleOriginRepository(IDiskMapper<T> mapper, string filename)
        {
            _mapper = mapper;
            _filename = filename;
            _items = new Dictionary<Guid, T>();
        }

        public IEnumerable<T> Items
        {
            get
            {
                if (_items == null || _items.Count == 0) _items = _mapper.Read(_filename).ToDictionary(k => k.ID, v => v);
                return _items.Values.Where(i => !i.IsDeleted);
            }
        }

        public void Create(T entity)
        {
            entity.UpdateRevision(true);
            _items[entity.ID] = entity;
        }

        public void Delete(T entity)
        {
            _items[entity.ID].IsDeleted = true;
            _items[entity.ID].UpdateRevision();
            // Leave the item in memory or else deleting the last item causes a lazy-reload on Save.
        }

        public T Find(Guid id)
        {
            if (_items == null || _items.Count == 0) FindAll();
            return _items.TryGetValue(id, out var result) ? result : null;
        }

        public IEnumerable<T> FindAll()
        {
            return Items;
        }

        public void SaveChanges()
        {
            _mapper.Write(Items, _filename);
        }

        public void Update(T entity, bool isNew = false)
        {
            if (!isNew || _items[entity.ID].PrevRevision.HasValue)
            {
                _items[entity.ID].UpdateRevision();
                _items[entity.ID] = entity;
            }
            else
            {
                Create(entity);
            }
        }
    }
}
