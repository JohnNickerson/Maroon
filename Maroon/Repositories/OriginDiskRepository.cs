using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.Repositories
{
    public class OriginDiskRepository<T> : IMergeRepository<T> where T : ModelObject
    {
        #region Fields

        private readonly IDiskMapper<T> _mapper;
        private readonly string _primaryFileName;
        private readonly string _thisMachineName;
        private const string MachineFileNameSearch = "updates-*.txt";

        private List<T> _localUpdates;
        private List<T> _allUpdates;
        private List<T> _items;

        private bool _unsavedChanges;

        #endregion

        #region Constructors
        public OriginDiskRepository(IDiskMapper<T> primaryMapper, string primaryFileName, string thisMachineName)
        {
            _mapper = primaryMapper;
            _primaryFileName = primaryFileName;
            _thisMachineName = thisMachineName;
            _items = new List<T>();
            _allUpdates = new List<T>();

            _localUpdates = (List<T>)_mapper.Read(ThisMachineFile);
        }
        #endregion

        #region Methods
        public T? Find(Guid id)
        {
            if (Items.Any(v => v.ID == id))
            {
                return Items.First(t => t.ID == id);
            }
            var i = _mapper.Read(_primaryFileName);
            if (i != null)
            {
                _items.AddRange(i);
            }
            else
            {
                // Reload pending changes.
                LoadChanges();
            }
            if (Items.Any(v => v.ID == id))
            {
                return Items.First(t => t.ID == id);
            }

            return null;
        }

        public IEnumerable<T> FindAll()
        {
            _items = _mapper.Read(_primaryFileName).ToList();
            LoadChanges();
            return Items;
        }

        private void LoadChanges()
        {
            // Load changes from disk without needing to save from memory first.
            foreach (var updateFileName in UpdateFileNames)
            {
                foreach (var u in _mapper.Read(updateFileName))
                {
                    if (_allUpdates.All(p => p.RevisionGuid != u.RevisionGuid))
                    {
                        _allUpdates.Add(u);
                    }
                }
            }
        }

        public void Create(T entity)
        {
            entity.UpdateRevision();
            entity.PrevRevision = null;
            _allUpdates.Add((T)entity.Clone());
            _localUpdates.Add((T)entity.Clone());
            _unsavedChanges = true;
        }

        public void Delete(T? entity)
        {
            if (entity is null) return;
            var gone = (T)entity.Clone();
            gone.IsDeleted = true;
            gone.UpdateRevision();
            _allUpdates.Add(gone);
            _localUpdates.Add(gone);
            _unsavedChanges = true;
        }

        public void Update(T entity, bool isNew = false)
        {
            if (!isNew || entity.PrevRevision.HasValue)
            {
                var updated = (T)entity.Clone();
                updated.UpdateRevision();
                _allUpdates.Add(updated);
                _localUpdates.Add(updated);
                _unsavedChanges = true;
            }
            else
            {
                Create(entity);
            }
        }

        public void SaveChanges(bool force = false)
        {
            lock (_mapper)
            {
                // Only write changes if there are any to write.
                if (force || _localUpdates.Count > 0 && _unsavedChanges)
                {
                    _mapper.Write(_localUpdates, ThisMachineFile);
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

                var pendingCount = _allUpdates.Count;

                // Only write changes if there are any to write.
                if (pendingCount > 0)
                {
                    // Verify no conflicts first. Caller must check and resolve conflicts if they exist.
                    if (FindConflicts().Count > 0) return 0;

                    // Apply changes (encapsulated in the Items property).
                    // Save all.
                    _mapper.Write(Items.ToList(), _primaryFileName);

                    // Clear the lists.
                    _items = Items.ToList();
                    _allUpdates = new List<T>();
                    _localUpdates = new List<T>();
                    _unsavedChanges = false;

                    // Clear pending lists on disk (delete files).
                    foreach (var updateFileName in UpdateFileNames)
                    {
                        File.Delete(updateFileName);
                    }
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
            foreach (var u in _allUpdates)
            {
                knownRevs.Add(u.RevisionGuid);
            }

            // Get the list of IDs to check.
            var checkIds = _allUpdates.Select(u => u.ID).Distinct().ToArray();
            foreach (var id in checkIds)
            {
                var c = new PendingChange<T>
                {
                    Id = id,
                };
                // If two or more updates branched from the same revision, that's a conflict.
                result.Add(c);
            }

            return result;
        }

        public void ResolveConflict(T item)
        {
            Revert(item.ID);
            item.PrevRevision = Find(item.ID)?.RevisionGuid;
            _allUpdates.Add(item);
            _localUpdates.Add(item);
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
            foreach (var change in _allUpdates.Where(u => u.ID == id && u.PrevRevision.HasValue).ToList())
            {
                // Remove from memory.
                _allUpdates.Remove(change);
                // Remove from disk, if present.
                foreach (var updateFileName in UpdateFileNames)
                {
                    File.Delete(updateFileName);
                }
            }
            _localUpdates.RemoveAll(u => u.ID == id);
            _unsavedChanges = _localUpdates.Any();
        }
        #endregion

        #region Properties

        public IEnumerable<T> Items => _allUpdates.Where(u => !_allUpdates.Any(q => q.ID == u.ID && q.LastModified > u.LastModified)).Union(_items).Where(d => !d.IsDeleted);

        private string ThisMachineFile => Path.Combine(PrimaryPath, $"updates-{_thisMachineName}.txt");

        private string[] UpdateFileNames => Directory.GetFiles(PrimaryPath, MachineFileNameSearch, SearchOption.TopDirectoryOnly);

        private string? PrimaryPath => Path.GetDirectoryName(Path.GetFullPath(_primaryFileName));

        #endregion
    }
}
