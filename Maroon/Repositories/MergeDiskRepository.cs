using System;
using System.Collections.Generic;
using System.Linq;
using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Mappers.Xml;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.Repositories
{
    public class MergeDiskRepository<T> : IMergeRepository<T> where T : ModelObject
    {
        #region Fields
        private IMapper<T> _mapper;
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
            _items = new List<T>();
            _updated = new List<T>();
            _deleted = new List<T>();

            _updateMapper = new SharpListSerialiser<T>(path, "update-{0}.xml");
            _deleteMapper = new SharpListSerialiser<T>(path, "delete-{0}.xml");
        }
        #endregion

        #region Methods
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
            _updated = _updateMapper.Deserialise();
            _deleted = _deleteMapper.Deserialise();

            return Items;
        }

        public void Create(T entity)
        {
            entity.LastModified = DateTime.Now;
            entity.Revision++;
            _updated.Add(entity);
        }

        public void Delete(T entity)
        {
            // Multiple deletes won't cause a conflict, so keep the data simple and make sure there's only one per object.
            _deleted.RemoveAll(t => t.ID == entity.ID);
            entity.LastModified = DateTime.Now;
            entity.Revision++;
            _deleted.Add(entity);
        }

        public void Update(T entity)
        {
            // Do not remove from the list, or else we can't detect conflicts.
            entity.LastModified = DateTime.Now;
            entity.Revision++;
            _updated.Add(entity);
        }

        public void SaveChanges()
        {
            // Only write changes if there are any to write.
            if (_updated.Count > 0 || _deleted.Count > 0)
            {
                // Note: all changes will be in these lists, and the core list does not get updated except via CommitChanges().
                // Therefore, saving the updated and deleted collections saves all changes.
                _updateMapper.Serialise(_updated);
                _deleteMapper.Serialise(_deleted);
            }
        }

        public void CommitChanges()
        {
            SaveChanges(); // So we don't stomp on anything in memory.
            // Load all, in case other changes have been merged in.
            FindAll();

            // Only write changes if there are any to write.
            if (_updated.Count > 0 || _deleted.Count > 0)
            {
                // Verify no conflicts first. Caller must check and resolve conflicts if they exist.
                if (FindConflicts().Count > 0) return;

                // Apply changes (encapsulated in the Items property).
                // Save all.
                _mapper.SaveAll(Items.ToList());

                // Clear the lists.
                _items = Items.ToList();
                _updated = new List<T>();
                _deleted = new List<T>();

                // Clear pending lists on disk (delete files).
                _updateMapper.Serialise(_updated, true);
                _deleteMapper.Serialise(_deleted, true);
            }
        }

        /// <summary>
        /// Gets a list containing sets of conflicting edits.
        /// </summary>
        /// <returns></returns>
        /// <remarks>A conflict here is defined as two or more updates or deletes to the same version (ie revision number) of the same object.</remarks>
        public List<Conflict<T>> FindConflicts()
        {
            var result = new List<Conflict<T>>();

            // A set of pending edits comprises a conflict if:
            // 1. There is a gap in the list of revision numbers.
            // 2. There are two edits with the same revision number.

            // Get the list of IDs to check.
            var checkIds = _updated.Select(u => u.ID).Union(_deleted.Select(d => d.ID)).Distinct();
            foreach (var id in checkIds)
            {
                // Verify pending updates and deletes form a coherent chain.
                var baseRevision = _items.FirstOrDefault(i => i.ID == id)?.Revision ?? _updated.OrderBy(u => u.Revision).FirstOrDefault(u => u.ID == id)?.Revision - 1;
                if (!baseRevision.HasValue) continue;

                baseRevision++;
                // Look for exactly one update or delete with the next revision number.
                while (_updated.Count(u => u.ID == id && u.Revision == baseRevision) +
                       _deleted.Count(d => d.ID == id && d.Revision == baseRevision) == 1)
                {
                    baseRevision++;
                }
                // If there are any updates or deletes left with equal or higher revision numbers, there is a conflict.
                var c = new Conflict<T>
                {
                    Updates = _updated.Where(u => u.ID == id && u.Revision >= baseRevision).ToList(),
                    Deletes = _deleted.Where(d => d.ID == id && d.Revision >= baseRevision).ToList()
                };
                if (c.Updates.Count + c.Deletes.Count > 0)
                {
                    result.Add(c);
                }
            }

            return result;
        }

        public void ResolveConflict(T item)
        {
            _deleted.RemoveAll(d => d.ID == item.ID);
            _updated.RemoveAll(u => u.ID == item.ID);
            Update(item);
        }

        public void ResolveByDelete(Guid id)
        {
            _deleted.RemoveAll(d => d.ID == id);
            _updated.RemoveAll(u => u.ID == id);
            Delete(Find(id));
        }

        public void Revert(Guid id)
        {
            _deleted.RemoveAll(d => d.ID == id);
            _updated.RemoveAll(u => u.ID == id);
        }
        #endregion

        #region Properties

        public IEnumerable<T> Items => _updated.Union(_items).Except(_deleted);

        #endregion
    }
}
