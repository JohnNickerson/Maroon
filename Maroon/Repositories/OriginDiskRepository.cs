using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using AssimilationSoftware.Maroon.Annotations;
using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Mappers.Xml;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.Repositories
{
    public class OriginDiskRepository<T> : IMergeRepository<T>, INotifyPropertyChanged where T : ModelObject
    {
        #region Fields

        private IMapper<T> _mapper;
        private OriginXmlSerialiser<T> _updateMapper;
        private List<T> _unsavedUpdates;

        private List<T> _updated;
        private List<T> _items;
        private bool _unsavedChanges;
        private int _progressPercent;

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Constructors
        public OriginDiskRepository(IMapper<T> mapper, string path, string thisMachineName)
        {
            _mapper = mapper;
            _items = new List<T>();
            _updated = new List<T>();
            _unsavedUpdates = new List<T>();

            _updateMapper = new OriginXmlSerialiser<T>(path, thisMachineName, "updates-{0}.xml");
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
            _unsavedUpdates.Add(entity);
            _unsavedChanges = true;
        }

        public void Delete(T entity)
        {
            var gone = (T)entity.Clone();
            gone.IsDeleted = true;
            gone.UpdateRevision();
            _updated.Add(gone);
            _unsavedUpdates.Add(gone);
            _unsavedChanges = true;
        }

        public void Update(T entity)
        {
            if (entity.PrevRevision.HasValue)
            {
                var updated = (T) entity.Clone();
                updated.UpdateRevision();
                _updated.Add(updated);
                _unsavedUpdates.Add(updated);
                _unsavedChanges = true;
            }
            else
            {
                Create(entity);
            }
        }

        public void SaveChanges()
        {
            lock (_mapper)
            {
                // Only write changes if there are any to write.
                if (_unsavedUpdates.Count > 0 || _unsavedChanges)
                {
                    _updateMapper.Serialise(_unsavedUpdates);
                    _unsavedUpdates = new List<T>();
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
                    _unsavedUpdates = new List<T>();
                    _unsavedChanges = false;

                    // Clear pending lists on disk (delete files).
                    _updateMapper.Clear();
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
            var knownRevs = new HashSet<Guid>();
            var seenRevs = new HashSet<Guid>();
            foreach (var i in _items)
            {
                knownRevs.Add(i.RevisionGuid);
            }
            foreach (var u in _updated)
            {
                knownRevs.Add(u.RevisionGuid);
            }

            // Get the list of IDs to check.
            var checkIds = _updated.Select(u => u.ID).Distinct().ToArray();
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
                    if (!p.PrevRevision.HasValue) continue; // New item with no previous revision.

                    if (seenRevs.Contains(p.PrevRevision.Value) || !knownRevs.Contains(p.PrevRevision.Value))
                    {
                        c.IsConflict = true;
                        break;
                    }
                    seenRevs.Add(p.PrevRevision.Value);
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
            _unsavedUpdates.Add(item);
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
            _unsavedUpdates.RemoveAll(u => u.ID == id);
            _unsavedChanges = _unsavedUpdates.Any();
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Properties

        public IEnumerable<T> Items => _updated.Where(u => !_updated.Any(q => q.ID == u.ID && q.LastModified > u.LastModified)).Union(_items).Where(d => !d.IsDeleted);

        public int ProgressPercent
        {
            get => _progressPercent;
            set
            {
                if (_progressPercent == value) return;
                _progressPercent = value;
                OnPropertyChanged(nameof(ProgressPercent));
            }
        }

        #endregion
    }
}
