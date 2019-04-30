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

        protected IMapper<T> _mapper;
        protected SharpListSerialiser<T> _updateMapper;

        protected List<T> _updated;
        protected List<T> _items;
        protected bool _unsavedChanges;
        #endregion

        #region Constructors
        public MergeDiskRepository(IMapper<T> mapper, string path)
        {
            _mapper = mapper;
            _items = new List<T>();
            _updated = new List<T>();

            _updateMapper = new SharpListSerialiser<T>(path, "update-{0}.xml");
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
            else
            {
                // Reload pending changes.
                LoadChanges();
                if (Items.Any(v => v.ID == id))
                {
                    i = Items.First(t => t.ID == id);
                }
            }
            return i;
        }

        public IEnumerable<T> FindAll()
        {
            _items = _mapper.LoadAll().ToList();
            LoadChanges();
            return Items;
        }

        private void LoadChanges()
        {
            // Load changes from disk without needing to save from memory first.
            foreach (var u in _updateMapper.Deserialise())
            {
                if (_updated.All(p => p.RevisionGuid != u.RevisionGuid))
                {
                    _updated.Add(u);
                }
            }
        }

        public void Create(T entity)
        {
            entity.UpdateRevision();
            entity.PrevRevision = null;
            _updated.Add(entity);
            _unsavedChanges = true;
        }

        public void Delete(T entity)
        {
            var gone = (T)entity.Clone();
            gone.IsDeleted = true;
            gone.UpdateRevision();
            _updated.Add(gone);
            _unsavedChanges = true;
        }

        public void Update(T entity)
        {
            var updated = (T)entity.Clone();
            updated.UpdateRevision();
            _updated.Add(updated);
            _unsavedChanges = true;
        }

        public void SaveChanges()
        {
            lock (_mapper)
            {
                // Only write changes if there are any to write.
                if (_updated.Count > 0 || _unsavedChanges)
                {
                    // Note: all changes will be in these lists, and the core list does not get updated except via CommitChanges().
                    // Therefore, saving the updated collection saves all changes.
                    _updateMapper.Serialise(_updated);
                    _unsavedChanges = false;
                }
            }
        }

        public int CommitChanges()
        {
            lock (_mapper)
            {
                // Make sure we've got the latest in memory.
                FindAll();

                var pendingCount = _updated.Count;

                // Only write changes if there are any to write.
                if (pendingCount > 0)
                {
                    // Verify no conflicts first. Caller must check and resolve conflicts if they exist.
                    if (FindConflicts().Count > 0) return 0;

                    // Apply changes (encapsulated in the Items property).
                    // Save all.
                    _mapper.SaveAll(Items.ToList());

                    // Clear the lists.
                    _items = Items.ToList();
                    _updated = new List<T>();

                    // Clear pending lists on disk (delete files).
                    _updateMapper.Serialise(_updated, true);
                }

                return pendingCount;
            }
        }

        /// <summary>
        /// Gets a list containing sets of conflicting edits.
        /// </summary>
        /// <returns></returns>
        /// <remarks>A conflict here is defined as two or more updates or deletes to the same version (ie revision number) of the same object.</remarks>
        public List<PendingChange<T>> FindConflicts()
        {
            return GetPendingChanges().Where(c => c.IsConflict).ToList();
        }

        public List<PendingChange<T>> GetPendingChanges()
        {
            var result = new List<PendingChange<T>>();

            // A set of pending edits comprises a conflict if:
            // 1. Any two (or more) updates have the same source revision ID.
            // 2. Any update has a dangling previous revision ID.

            // Get the list of IDs to check.
            var checkIds = _updated.Select(u => u.ID).Distinct();
            foreach (var id in checkIds)
            {
                var c = new PendingChange<T>
                {
                    Id = id,
                    Updates = _updated.Where(u => u.ID == id).ToList()
                };

                // If two or more updates branched from the same revision, that's a conflict.
                foreach (var p in c.Updates)
                {
                    //if (!p.PrevRevision.HasValue) continue;
                    foreach (var q in c.Updates)
                    {
                        if (p.PrevRevision == q.PrevRevision && p.RevisionGuid != q.RevisionGuid)
                        {
                            c.IsConflict = true;
                        }
                    }
                }

                // An update is based on an unknown revision.
                if (_updated.Any(u => u.PrevRevision.HasValue && _items.All(p => p.RevisionGuid != u.PrevRevision.Value) && _updated.All(p => p.RevisionGuid != u.PrevRevision.Value)))
                {
                    c.IsConflict = true;
                }

                result.Add(c);
            }

            return result;
        }

        public void ResolveConflict(T item)
        {
            Revert(item.ID);
            item.PrevRevision = Find(item.ID).RevisionGuid;
            _updated.Add(item);
            _unsavedChanges = true;
        }

        public void ResolveByDelete(Guid id)
        {
            Revert(id);
            Delete(Find(id));
        }

        public void Revert(Guid id)
        {
            // Only remove updates. Newly-created items are in this set, too, with null previous revision IDs.
            foreach (var change in _updated.Where(u => u.ID == id && u.PrevRevision.HasValue).ToList())
            {
                // Remove from memory.
                _updated.Remove(change);
                // Remove from disk, if present.
                _updateMapper.Delete(change);
            }
        }
        #endregion

        #region Properties

        public IEnumerable<T> Items => _updated.Where(u => !_updated.Any(q => q.ID == u.ID && q.LastModified > u.LastModified)).Union(_items).Where(d => !d.IsDeleted);

        #endregion
    }
}
