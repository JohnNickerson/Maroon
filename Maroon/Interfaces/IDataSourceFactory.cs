using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Model;

public interface IDataSourceFactory<T> where T : ModelObject
{
    IEnumerable<IDataSource<T>> LoadAllSources();

    IDataSource<T> GetSourceForItem(T item);
}