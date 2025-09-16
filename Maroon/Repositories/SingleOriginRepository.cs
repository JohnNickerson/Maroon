using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.Repositories
{
    public class SingleOriginRepository<T> : IRepository<T> where T : ModelObject
    {
        private readonly IDataSource<T> _dataSource;
        private Dictionary<Guid, T> _items;
        private bool _hasChanges;
        private bool _loaded;

        public SingleOriginRepository(IDataSource<T> mapper)
        {
            _dataSource = mapper;
            _items = new Dictionary<Guid, T>();
            _hasChanges = false;
            _loaded = false;
        }

        public IEnumerable<T> Items
        {
            get
            {
                if (!_loaded)
                {
                    if (_hasChanges)
                    {
                        // Preserve the existing data.
                        var onDisk = _dataSource.FindAll();
                        _items = _items.Values.Union(onDisk).ToDictionary(k => k.ID, v => v);
                    }
                    else
                    {
                        _items = _dataSource.FindAll().ToDictionary(k => k.ID, v => v);
                    }
                    _loaded = true;
                }
                return _items.Values.Where(i => !i.IsDeleted);
            }
        }

        public void Compress()
        {
            // Remove deleted items and obsolete revisions.
            var oldRevisions = _items.Values
                .Where(i => i.PrevRevision.HasValue || i.MergeRevision.HasValue)
                .Select(i => i.PrevRevision ?? i.MergeRevision)
                .Where(r => r.HasValue)
                .Distinct()
                .ToList();
            _items = _items.Where(kvp => !kvp.Value.IsDeleted &&
                                  !oldRevisions.Contains(kvp.Value.RevisionGuid))
                           .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public void Create(T entity)
        {
            _items[entity.ID] = _dataSource.Insert(entity);
            _hasChanges = true;
        }

        public void Delete(T entity)
        {
            _items[entity.ID].IsDeleted = true;
            _items[entity.ID].UpdateRevision();
            _hasChanges = true;
        }

        public T Find(Guid id)
        {
            if (!_loaded) FindAll();
            var hasValue = _items.TryGetValue(id, out var result);
            return hasValue && !result.IsDeleted ? result : null;
        }

        public IEnumerable<T> FindAll()
        {
            return Items;
        }

        public IEnumerable<List<T>> FindConflicts()
        {
            // There really shouldn't be any conflicts in a single-origin repository.
            // But if there are, return them.
            // A conflict is defined as multiple revisions with the same PrevRevision, and not deleted, and not later merged.
            // Find all revisions that are not used as another's previous revision or merge revision.
            // Group by ID and return if each set has more than one item.
            // We need to work from the full set of revisions, not just the ones in Items.
            var allRevisions = _dataSource.FindAll().ToDictionary(k => k.RevisionGuid, v => v);

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
            _items[entity.ID] = _dataSource.Insert(entity);
            _hasChanges = true;
        }

        public void SaveChanges(bool force = false)
        {
            // Changes are now saved immediately on Create/Update/Delete.
        }

        public void Update(T entity)
        {
            entity.UpdateRevision();
            _items[entity.ID] = _dataSource.Insert(entity);
            _hasChanges = true;
        }
    }
}
