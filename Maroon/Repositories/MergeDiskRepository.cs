using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using AssimilationSoftware.Maroon.Annotations;
using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Model;
using LiteDB;
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global

namespace AssimilationSoftware.Maroon.Repositories
{
    public class RevisionShardRepository<T> : IRepository<T> where T : ModelObject
    {
        #region Fields

        [NotNull] protected readonly IDataSource<T> _mapper;
        private readonly IDataSourceFactory<T> _dataSourceFactory;

        protected Dictionary<Guid, T> _items; // ID -> item, including all pending changes.
        private bool _loaded = false;

        #endregion

        #region Constructors
        public RevisionShardRepository(IDataSource<T> mapper, IDataSourceFactory<T> dataSourceFactory)
        {
            _mapper = mapper;
            _dataSourceFactory = dataSourceFactory;
            _items = new();
        }

        #endregion

        #region Methods
        public T Find(Guid id)
        {
            EnsureLoaded();
            if (_items.TryGetValue(id, out var result))
            {
                return result.IsDeleted ? null : result;
            }

            return null;
        }

        private void LoadAll()
        {
            _items = new Dictionary<Guid, T>();
            foreach (var item in _mapper.FindAll())
            {
                if (!_items.ContainsKey(item.ID) || _items[item.ID].LastModified < item.LastModified)
                {
                    _items[item.ID] = item;
                }
            }
            foreach (var dataSource in _dataSourceFactory.LoadAllSources())
            {
                foreach (var revision in dataSource.FindAll())
                {
                    if (!_items.ContainsKey(revision.ID) || _items[revision.ID].LastModified < revision.LastModified)
                    {
                        _items[revision.ID] = revision;
                    }
                }
            }
            _loaded = true;
        }

        public IEnumerable<T> FindAll()
        {
            EnsureLoaded();
            return Items;
        }

        public void Create(T entity)
        {
            EnsureLoaded();
            _items[entity.ID] = _dataSourceFactory.GetSourceForItem(entity).Insert(entity);
        }

        public void Delete(T entity)
        {
            if (entity == null) return;
            EnsureLoaded();
            var gone = (T)entity.Clone();
            gone.IsDeleted = true;
            gone.UpdateRevision();
            _dataSourceFactory.GetSourceForItem(gone).Insert(gone);
            _items[entity.ID] = gone;
        }

        public void Update(T entity)
        {
            if (entity.PrevRevision.HasValue)
            {
                EnsureLoaded();
                var updated = (T)entity.Clone();
                updated.UpdateRevision();
                _dataSourceFactory.GetSourceForItem(updated).Insert(updated);
                _items[entity.ID] = updated;
            }
            else
            {
                Create(entity);
            }
        }

        public void SaveChanges(bool force = false)
        {
            // Obsolete.
        }

        // This may be useful for the Compress operation.
        public void Compress()
        {
            // Purge all revisions from other sources if they are superseded by newer revisions and have no conflicts.
            EnsureLoaded();
            var conflictIDs = new HashSet<Guid>(FindConflicts().Select(g => g.First().ID));
            var committedCount = 0;

            // For every revision in the main data source,
            // if it is in the conflict IDs, skip it.
            // if it is not the most recent revision for its ID, purge it.
            // if it is the most recent revision for its ID, but is deleted, purge it.
            foreach (var item in _mapper.FindAll())
            {
                if (conflictIDs.Contains(item.ID)) continue;
                if (_items.TryGetValue(item.ID, out var current))
                {
                    if (current.RevisionGuid != item.RevisionGuid || current.IsDeleted)
                    {
                        _mapper.Purge(item.RevisionGuid);
                        committedCount++;
                    }
                }
            }

            // For every revision in each auxiliary data source,
            // if it is in the conflict IDs, skip it.
            // if it is not the most recent revision for its ID, purge it.
            // if it is the most recent revision for its ID, but is deleted, purge it.
            // if it is not deleted, move it to the main data source.
            foreach (var ds in _dataSourceFactory.LoadAllSources())
            {
                foreach (var item in ds.FindAll())
                {
                    if (conflictIDs.Contains(item.ID)) continue;
                    if (_items.TryGetValue(item.ID, out var current))
                    {
                        if (current.RevisionGuid != item.RevisionGuid || current.IsDeleted)
                        {
                            ds.Purge(item.RevisionGuid);
                            committedCount++;
                        }
                        else
                        {
                            // Move to main data source.
                            _mapper.Insert(item);
                            ds.Purge(item.RevisionGuid);
                            committedCount++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets a list containing sets of conflicting edits.
        /// </summary>
        /// <returns></returns>
        /// <remarks>A conflict here is defined as two or more updates or deletes to the same version (ie revision number) of the same object.</remarks>
        public IEnumerable<List<T>> FindConflicts()
        {
            // Load all revisions into a dictionary by revision ID.
            var allRevisions = new Dictionary<Guid, T>();
            foreach (var item in _mapper.FindAll())
            {
                allRevisions[item.RevisionGuid] = item;
            }
            foreach (var ds in _dataSourceFactory.LoadAllSources())
            {
                foreach (var item in ds.FindAll())
                {
                    allRevisions[item.RevisionGuid] = item;
                }
            }
            // Find all revision IDs that are used as PrevRevision or MergeRevision.
            var usedRevisions = new HashSet<Guid>(
                allRevisions.Values
                      .Where(i => i.PrevRevision.HasValue || i.MergeRevision.HasValue)
                      .SelectMany(i => new[] { i.PrevRevision, i.MergeRevision })
                      .Distinct()
                      .Where(r => r.HasValue)
                      .Select(r => r.Value)
            );
            // The result: return groups of items with the same ID, and more than one revision, that are not deleted and not used as a previous or merge revision.
            var conflicts = allRevisions.Values
                                .Where(i => !i.IsDeleted && !usedRevisions.Contains(i.RevisionGuid))
                                .GroupBy(i => i.ID)
                                .Where(g => g.Count() > 1)
                                .Select(g => new List<T>(g))
                                .ToList();
            return conflicts;
        }

        public void Merge(T item, Guid mergeId)
        {
            EnsureLoaded();
            item.UpdateRevision();
            item.MergeRevision = mergeId;
            _dataSourceFactory.GetSourceForItem(item).Insert(item);
            _items[item.ID] = item;
        }

        private void EnsureLoaded()
        {
            if (!_loaded)
            {
                LoadAll();
            }
        }

        private void ExternalCommit(object sender, FileSystemEventArgs e)
        {
            _loaded = false;
        }

        private void ExternalUpdate(object sender, FileSystemEventArgs e)
        {
            _loaded = false;
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

        #endregion
    }
}
