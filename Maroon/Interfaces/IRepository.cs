using System;
using System.Collections.Generic;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.Interfaces
{
    public interface IRepository<T> where T : ModelObject
    {
        IEnumerable<T> Items { get; }

        T? Find(Guid id);

        IEnumerable<T> FindAll();

        void Create(T entity);

        void Delete(T entity);

        void Update(T entity, bool isNew = false);

        void SaveChanges(bool force = false);
    }
}
