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
        protected List<T> _updated;
        protected List<T> _deleted;
        protected List<T> _items;

        public DiskRepository(IMapper<T> mapper)
        {
            _mapper = mapper;
            _items = new List<T>();
            _updated = new List<T>();
            _deleted = new List<T>();
        }

        public IEnumerable<T> Items
        {
            get
            {
                // "Union" returns objects from the first collection first, so "updated" takes precedence here.
                return _updated.Union(_items).Except(_deleted);
            }
        }

        public void Create(T entity)
        {
            _updated.RemoveAll(t => t.ID == entity.ID);
            _updated.Add(entity);
        }

        public void Delete(T entity)
        {
            _deleted.RemoveAll(t => t.ID == entity.ID);
            _deleted.Add(entity);
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
			if (_updated.Count > 0 || _deleted.Count > 0)
			{
				// Load all.
				FindAll();
				// Apply changes.
				// Save all.
				_mapper.SaveAll(Items.ToList());
                // Clear the lists.
                _items = Items.ToList();
				_updated = new List<T>();
				_deleted = new List<T>();
			}
        }

        public void Update(T entity)
        {
            _updated.RemoveAll(t => t.ID == entity.ID);
            _updated.Add(entity);
        }
    }
}
