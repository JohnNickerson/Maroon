using System;
using System.Collections.Generic;
using System.Linq;
using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.Repositories
{
    public class DiskRepository<T> : IRepository<T> where T : ModelObject
    {
        protected IMapper<T> _mapper;
        protected Dictionary<Guid, T> _updated;
        protected List<T> _items;

        public DiskRepository(IMapper<T> mapper)
        {
            _mapper = mapper;
            _items = new List<T>();
            _updated = new Dictionary<Guid, T>();
        }

        public IEnumerable<T> Items
        {
            get
            {
                // "Union" returns objects from the first collection first, so "updated" takes precedence here.
                return _updated.Values.Union(_items).Where(i => !i.IsDeleted);
            }
        }

        public void Create(T entity)
        {
            entity.UpdateRevision();
            entity.PrevRevision = null;
            _updated[entity.ID] = entity;
        }

        public void Delete(T entity)
        {
            entity.IsDeleted = true;
            Update(entity);
        }

        public T Find(Guid id)
        {
            if (Items.Any(v => v.ID == id))
            {
                return Items.First(t => t.ID == id);
            }
            var i = _mapper.Load(id);
            if (i != null)
            {
                _items.Add(i);
            }
            return i;
        }

        public IEnumerable<T> FindAll()
        {
            _items = _mapper.LoadAll().ToList();
            return Items;
        }

        public void SaveChanges()
        {
			// Only write changes if there are any to write.
			if (_updated.Count > 0)
			{
				// Load all.
				FindAll();
				// Apply changes.
				// Save all.
				_mapper.SaveAll(Items.ToList());
                // Clear the lists.
                _items = Items.ToList();
				_updated = new Dictionary<Guid, T>();
			}
        }

        public void Update(T entity, bool isNew = false)
        {
            if (!isNew || entity.PrevRevision.HasValue)
            {
                entity.UpdateRevision();
                _updated[entity.ID] = entity;
            }
            else
            {
                Create(entity);
            }
        }
    }
}
