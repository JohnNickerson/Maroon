using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Repositories;
using AssimilationSoftware.Maroon.Repositories.Tests;

public class RevisionShardRepositoryTests
{
    private IDataSource<MockObj> mockMapper = new MockDiskMapper();
    private IDataSourceFactory<MockObj> mockMapperFactory = new MockDiskMapperFactory();

    [Fact]
    public void Construct_RevisionShardRepositoryTest()
    {
        var rdr = new RevisionShardRepository<MockObj>(mockMapper, mockMapperFactory);
        Assert.NotNull(rdr);
    }
}