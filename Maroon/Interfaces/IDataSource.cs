using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.Interfaces;

public interface IDataSource<T> where T : ModelObject
{
    /// <summary>
    /// Get the list of all revisions in this data source.
    /// </summary>
    IEnumerable<T> FindAll();

    /// <summary>
    /// Find a single revision by revision ID (not record ID).
    /// </summary>
    T? FindRevision(Guid id);

    /// <summary>
    /// Create a whole new record with no previous revision.
    /// </summary>
    T Create(T item);

    /// <summary>
    /// Edit a record by creating a new revision.
    /// </summary>
    /// <param name="item">The updated version of the record.</param>
    /// <returns>The new revision, with bookkeeping properties set.</returns>
    T Update(T item);

    /// <summary>
    /// Mark a record as deleted by creating a new revision with "IsDeleted" = true.
    /// </summary>
    T Delete(T item);

    /// <summary>
    /// Remove a revision from the data source. This should only be used in the "compress" operation.
    /// </summary>
    /// <param name="id"></param>
    void Purge(Guid id);

    /// <summary>
    /// Get the last known write time for the source.
    /// </summary>
    /// <remarks>
    /// This is important for the "compress" operation.
    /// </remarks>
    DateTime GetLastWriteTime();
}