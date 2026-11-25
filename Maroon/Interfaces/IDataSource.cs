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
    /// Add a new revision.
    /// </summary>
    T Insert(T item);

    /// <summary>
    /// Remove a revision from the data source. This should only be used in the "compress" operation.
    /// </summary>
    /// <param name="id"></param>
    void Purge(params Guid[] ids);

    /// <summary>
    /// Get the last known write time for the source.
    /// </summary>
    /// <remarks>
    /// This is important for the "compress" operation.
    /// </remarks>
    DateTime GetLastWriteTime();
}