using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using AssimilationSoftware.Maroon.Annotations;
using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Model;
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global

namespace AssimilationSoftware.Maroon.Repositories
{
    public class MergeDiskRepository<T> : IMergeRepository<T> where T : ModelObject
    {
        #region Fields

        [NotNull] protected readonly IDiskMapper<T> _mapper;
        private readonly string _primaryFileName;
        private readonly string _updatesFileSearch = "update-*.txt";

        protected readonly Dictionary<Guid, T> _unsavedUpdates; // Revision ID -> item
        protected Dictionary<Guid, PendingChange<T>> _pending; // ID -> pending change set, both saved and unsaved
        protected Dictionary<Guid, T> _items; // ID -> item, including all pending changes.

        #endregion

        #region Constructors
        public MergeDiskRepository(IDiskMapper<T> mapper, string primaryFileName)
        {
            _mapper = mapper;
            _primaryFileName = primaryFileName;
            var fileWatcher = new FileSystemWatcher(PrimaryPath);
            fileWatcher.Created += ExternalUpdate;
            fileWatcher.Deleted += ExternalCommit;
            _unsavedUpdates = new Dictionary<Guid, T>();
        }

        #endregion

        #region Methods
        public T Find(Guid id)
        {
            if (_items == null)
            {
                LoadAll();
            }
            if (_items.TryGetValue(id, out var result))
            {
                return result.IsDeleted ? null : result;
            }

            return null;
        }

        private void LoadAll()
        {
            _items = new Dictionary<Guid, T>();
            _pending = new Dictionary<Guid, PendingChange<T>>();
            lock (_mapper)
            {
                foreach (var modelObject in _mapper.Read(_primaryFileName))
                {
                    _items[modelObject.ID] = modelObject;
                }
            }
            ApplyChangesFromDisk(UpdateFileNames);
        }

        private void ApplyChangesFromDisk(params string[] updateFileNames)
        {
            IEnumerable<T> updates;
            lock (_mapper)
            {
                updates = _mapper.Read(updateFileNames);
            }
            // Apply updates to _items
            foreach (var update in updates.OrderBy(u => u.LastModified))
            {
                if (!_items.ContainsKey(update.ID))
                {
                    _items[update.ID] = update;
                }
                else if (update.IsDeleted)
                {
                    _items.Remove(update.ID);
                }
                else if (_items[update.ID].LastModified < update.LastModified)
                {
                    _items[update.ID] = update;
                }
            }

            foreach (var item in _items.Where(d => d.Value.IsDeleted).ToArray())
            {
                _items.Remove(item.Key);
            }
        }

        public IEnumerable<T> FindAll()
        {
            return Items;
        }

        public void Create(T entity)
        {
            entity.UpdateRevision(true);
            if (_items == null) LoadAll();
            _items[entity.ID] = entity;
            _unsavedUpdates.Add(entity.RevisionGuid, entity);
            AddPendingChange(entity);
        }

        public void Delete(T entity)
        {
            if (entity == null) return;
            var gone = (T) entity.Clone();
            gone.IsDeleted = true;
            gone.UpdateRevision();
            if (_items == null) LoadAll();
            _unsavedUpdates.Add(gone.RevisionGuid, gone);
            AddPendingChange(gone);
        }

        private void AddPendingChange(T entity)
        {
            if (!_pending.ContainsKey(entity.ID))
            {
                _pending.Add(entity.ID, new PendingChange<T>(entity));
            }
            else
            {
                _pending[entity.ID].AddRevision(entity);
            }

            if (!_items.ContainsKey(entity.ID) || _items[entity.ID].LastModified < entity.LastModified)
            {
                _items[entity.ID] = entity;
            }
        }

        public void Update(T entity, bool isNew = false)
        {
            if (!isNew || entity.PrevRevision.HasValue)
            {
                var updated = (T)entity.Clone();
                updated.UpdateRevision(isNew);
                if (_items == null) LoadAll();
                AddPendingChange(updated);
                _unsavedUpdates.Add(updated.RevisionGuid, updated);
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
                if (_unsavedUpdates.Count > 0 || force)
                {
                    foreach (var u in _unsavedUpdates)
                    {
                        _mapper.Write(new [] {u.Value}, $"update-{u.Value.RevisionGuid}.txt");
                    }
                    _unsavedUpdates.Clear();
                }
            }
        }

        public int CommitChanges()
        {
            lock (_mapper)
            {
                var committedCount = 0;
                // Make sure we've got the latest in memory.
                FindAll();

                // Only write changes if there are any to write.
                if (_items.Any())
                {
                    // Verify no conflicts first. Caller must check and resolve conflicts if they exist.
                    if (FindConflicts().Count > 0) return 0;

                    // Apply changes (encapsulated in the Items property).
                    // Save all.
                    _mapper.Write(_items.Values, _primaryFileName);

                    // Clear the lists.
                    _unsavedUpdates.Clear();
                    _pending.Clear();
                    _items = null;

                    // Clear pending lists on disk (delete files).
                    foreach (var u in UpdateFileNames)
                    {
                        _mapper.Delete(u);
                        committedCount++;
                    }
                }
                return committedCount;
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
            return _pending.Values.ToList();
        }

        public void ResolveConflict(T item)
        {
            Revert(item.ID); // Removes all pending updates.
            item.PrevRevision = Find(item.ID).RevisionGuid;
            if (_items == null) LoadAll();
            _items[item.ID] = item;
            _unsavedUpdates.Add(item.RevisionGuid, item);
            AddPendingChange(item);
        }

        public void ResolveByDelete(Guid id)
        {
            Revert(id);
            Delete(Find(id));
        }

        public void Revert(Guid id)
        {
            // Only remove updates. Newly-created items are in this set, too, with null previous revision IDs.
            foreach (var change in _pending[id].Updates.Where(p => p.Value.PrevRevision.HasValue).ToArray())
            {
                // Remove from memory.
                _pending[id].Updates.Remove(change.Key);
                _unsavedUpdates.Remove(change.Key);
                // Remove from disk, if present.
                _mapper.Delete(string.Format($"update-{change.Key}.txt"));
            }

            _items[id] = _pending[id].OriginalVersion;
        }

        private void ExternalCommit(object sender, FileSystemEventArgs e)
        {
            _items = null; // Force lazy-reload.
        }

        private void ExternalUpdate(object sender, FileSystemEventArgs e)
        {
            // Wait a moment.
            Thread.Sleep(TimeSpan.FromMilliseconds(200));
            ApplyChangesFromDisk(e.FullPath);
        }

        #endregion

        #region Properties

        public IEnumerable<T> Items
        {
            get
            {
                if (_items == null) LoadAll();
                return _items.Values.Where(d => !d.IsDeleted);
            }
        }

        private string[] UpdateFileNames => Directory.GetFiles(PrimaryPath, _updatesFileSearch, SearchOption.TopDirectoryOnly);

        private string PrimaryPath => Path.GetDirectoryName(Path.GetFullPath(_primaryFileName));

        #endregion
    }
}
