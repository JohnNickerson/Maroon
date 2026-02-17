using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.Repositories
{
    public class OriginShardRepository<T> : IRepository<T> where T : ModelObject
    {
        #region Fields

        private readonly IDataSource<T> _dataSource;
        private readonly IDataSource<T>[] _otherDataSources;

        private Dictionary<Guid, T> _itemIndex;

        private bool _loaded;

        #endregion

        #region Constructors
        public OriginShardRepository(IDataSource<T> dataSource, params IDataSource<T>[] otherDataSources)
        {
            _dataSource = dataSource;
            _otherDataSources = otherDataSources;
            _loaded = false;
            _itemIndex = new Dictionary<Guid, T>();
        }
        #endregion

        #region Methods
        public T Find(Guid id)
        {
            if (!_loaded)
            {
                FindAll();
            }
            if (_itemIndex.TryGetValue(id, out var item) && !item.IsDeleted)
            {
                return item;
            }
            return null;
        }

        public IEnumerable<T> FindAll()
        {
            if (!_loaded)
            {
                // Load from all data sources.
                var allItems = new Dictionary<Guid, T>();
                foreach (var item in _dataSource.FindAll())
                {
                    if (!allItems.ContainsKey(item.ID) || allItems[item.ID].LastModified < item.LastModified)
                    {
                        allItems[item.ID] = item;
                    }
                }
                foreach (var ds in _otherDataSources ?? Array.Empty<IDataSource<T>>())
                {
                    foreach (var item in ds.FindAll())
                    {
                        if (!allItems.ContainsKey(item.ID) || allItems[item.ID].LastModified < item.LastModified)
                        {
                            allItems[item.ID] = item;
                        }
                    }
                }
                _itemIndex = allItems;
                _loaded = true;
            }
            return _itemIndex.Values.Where(d => !d.IsDeleted);
        }

        public void Create(T entity)
        {
            _itemIndex[entity.ID] = _dataSource.Insert(entity);
        }

        public void Delete(T entity)
        {
            var gone = (T)entity.Clone();
            gone.IsDeleted = true;
            gone.UpdateRevision();
            _dataSource.Insert(gone);
            _itemIndex[entity.ID] = gone;
        }

        public void Update(T entity)
        {
            if (entity.PrevRevision.HasValue)
            {
                var updated = (T)entity.Clone();
                updated.UpdateRevision();
                _dataSource.Insert(updated);
                _itemIndex[entity.ID] = updated;
            }
            else
            {
                Create(entity);
            }
        }

        public void SaveChanges(bool force = false)
        {
            // Changes are saved automatically.
        }

        public IEnumerable<List<T>> FindConflicts()
        {
            // Load all revisions into a dictionary by revision ID.
            var allRevisions = new Dictionary<Guid, T>();
            foreach (var item in _dataSource.FindAll())
            {
                allRevisions[item.RevisionGuid] = item;
            }
            foreach (var ds in _otherDataSources ?? Array.Empty<IDataSource<T>>())
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

        public void Merge(T entity, Guid mergeId)
        {
            entity.UpdateRevision();
            entity.MergeRevision = mergeId;
            _dataSource.Insert(entity);
            _itemIndex[entity.ID] = entity;
        }

        public IEnumerable<Guid> FindObsoleteRevisionIds()
        {
            // For each revision in the local data source, remove it if there is a newer revision somewhere and if all other data sources have been written to after that newer revision.
            // Gather the latest revision for each ID across all non-local data sources.
            foreach (var ds in _otherDataSources ?? Array.Empty<IDataSource<T>>())
            {
                foreach (var item in ds.FindAll())
                {
                    if (!_itemIndex.ContainsKey(item.ID) || _itemIndex[item.ID].LastModified < item.LastModified)
                    {
                        _itemIndex[item.ID] = item;
                    }
                }
            }
            var oldestDataSource = _otherDataSources?.Min(ds => ds.GetLastWriteTime()) ?? DateTime.MinValue;
            foreach (var item in _dataSource.FindAll().ToList())
            {
                // Check if this revision is the latest for its ID.
                if (_itemIndex.TryGetValue(item.ID, out var latest) && latest.RevisionGuid != item.RevisionGuid)
                {
                    // There is a newer revision. Check if the latest revision's LastModified is older than the oldest data source write time.
                    if (latest.LastModified <= oldestDataSource)
                    {
                        // Safe to delete this revision.
                        yield return item.RevisionGuid;
                    }
                }
            }
        }

        public int Compress()
        {
            var purgeRevisions = FindObsoleteRevisionIds().ToList();
            _dataSource.Purge(purgeRevisions.ToArray());
            return purgeRevisions.Count;
        }
        #endregion

        #region Properties

        public IEnumerable<T> Items => _itemIndex.Values.Where(d => !d.IsDeleted);

        #endregion
    }
}
