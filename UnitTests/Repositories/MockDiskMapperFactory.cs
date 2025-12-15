using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Repositories.Tests;

public class MockDiskMapperFactory : IDataSourceFactory<MockObj>
{
    public IDataSource<MockObj> GetSourceForItem(MockObj item)
    {
        throw new NotImplementedException();
    }

    // I think this should actually be a method to return a list of all mappers required.
    public IEnumerable<MockObj> LoadAll()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IDataSource<MockObj>> LoadAllSources()
    {
        throw new NotImplementedException();
    }
}