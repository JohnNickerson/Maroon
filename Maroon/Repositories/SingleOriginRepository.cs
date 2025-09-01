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
        [Obsolete("Don't use this")]
        private readonly string _filename;
        private Dictionary<Guid, T> _items;
        private bool _hasChanges;
        private bool _loaded;

        public SingleOriginRepository(IDataSource<T> mapper, string filename)
        {
            _dataSource = mapper;
            _filename = filename;
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
            entity.UpdateRevision(true);
            _items[entity.ID] = entity;
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

        public IEnumerable<HashSet<T>> FindConflicts()
        {
            throw new NotImplementedException();
        }

        public void Merge(T entity, Guid mergeId)
        {
            throw new NotImplementedException();
        }

        public void SaveChanges(bool force = false)
        {
            // Changes are now saved immediately on Create/Update/Delete.
        }

        public void Update(T entity, bool isNew = false)
        {
            if (!isNew || _items[entity.ID].PrevRevision.HasValue)
            {
                _items[entity.ID].UpdateRevision();
                _items[entity.ID] = entity;
                _hasChanges = true;
            }
            else
            {
                Create(entity);
            }
        }
    }
}
