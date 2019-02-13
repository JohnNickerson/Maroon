using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Mappers.Xml;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.Repositories
{
    public class MergeDiskRepository<T> : IRepository<T> where T : ModelObject
    {
        #region Fields
        private IMapper<T> _mapper;
        private string _path;
        private SharpListSerialiser<T> _updateMapper;
        private SharpListSerialiser<T> _deleteMapper;

        private List<T> _updated;
        private List<T> _deleted;
        private List<T> _items;
        #endregion

        #region Constructors
        public MergeDiskRepository(IMapper<T> mapper, string path)
        {
            _mapper = mapper;
            _path = path;
            _items = new List<T>();
            _updated = new List<T>();
            _deleted = new List<T>();

            _updateMapper = new SharpListSerialiser<T>(path, "update-{0}.xml");
            _deleteMapper = new SharpListSerialiser<T>(path, "delete-{0}.xml");
        }
        #endregion

        public IEnumerable<T> Items => _updated.Union(_items).Except(_deleted);

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
            // 
            return Items;
        }

        public void Create(T entity)
        {
            _updated.Add(entity);
        }

        public void Delete(T entity)
        {
            _deleted.RemoveAll(t => t.ID == entity.ID);
            _deleted.Add(entity);
        }

        public void Update(T entity)
        {
            // Do not remove from the list, or else we can't detect conflicts.
            _updated.Add(entity);
        }

        public void SaveChanges()
        {
            // Only write changes if there are any to write.
            if (_updated.Count > 0 || _deleted.Count > 0)
            {
                // Note: all changes will be in these lists, and the core list does not get updated except via CommitChanges().
                // Therefore, serialising the updated and deleted collections saves all changes.
                _updateMapper.Serialise(_updated);
                _deleteMapper.Serialise(_deleted);
            }
        }

        public void CommitChanges()
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
                _updateMapper.Serialise(_updated, true);
                _deleteMapper.Serialise(_deleted, true);
            }
        }
    }
}
