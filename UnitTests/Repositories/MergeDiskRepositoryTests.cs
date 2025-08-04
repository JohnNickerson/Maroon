using Xunit;
using AssimilationSoftware.Maroon.Repositories;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssimilationSoftware.Maroon.Interfaces;

namespace AssimilationSoftware.Maroon.Repositories.Tests
{
    public class MergeDiskRepositoryTests
    {
        [Fact]
        public void MergeDiskRepositoryTest()
        {
            var mockFile = nameof(MergeDiskRepositoryTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new MergeDiskRepository<MockObj>(mockMapper, mockFile);
        }

        [Fact]
        public void CreateTest()
        {
            var mockFile = nameof(CreateTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new MergeDiskRepository<MockObj>(mockMapper, mockFile);
            mdr.Create(new MockObj());
        }

        [Fact]
        public void FindTest()
        {
            var mockFile = nameof(FindTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new MergeDiskRepository<MockObj>(mockMapper, mockFile);
            var entity = new MockObj();
            mdr.Create(entity);
            var found = mdr.Find(entity.ID);
            Assert.NotNull(found);
        }

        [Fact]
        public void FindAllTest()
        {
            var mockFile = nameof(FindAllTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new MergeDiskRepository<MockObj>(mockMapper, mockFile);
            var entity = new MockObj();
            var entity2 = new MockObj();
            mdr.Create(entity);
            mdr.Create(entity2);
            var found = mdr.FindAll();
            Assert.NotNull(found);
            Assert.Equal(2, found.Count());
        }

        [Fact]
        public void DeleteTest()
        {
            var mockFile = nameof(DeleteTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new MergeDiskRepository<MockObj>(mockMapper, mockFile);
            var entity = new MockObj();
            mdr.Create(entity);
            var found = mdr.Find(entity.ID);
            Assert.NotNull(found);
            mdr.Delete(found);
            var notFound = mdr.Find(entity.ID);
            Assert.Null(notFound);
        }

        [Fact]
        public void UpdateTest()
        {
            var mockFile = nameof(UpdateTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new MergeDiskRepository<MockObj>(mockMapper, mockFile);
            var entity = new MockObj();
            mdr.Create(entity);
            var found = mdr.Find(entity.ID);
            Assert.NotNull(found);
            found.ImportHash = "changed";
            mdr.Update(found);
            var updated = mdr.Find(entity.ID);
            Assert.NotNull(updated);
            Assert.Equal("changed", updated.ImportHash);
        }

        [Fact]
        public void SaveChangesTest()
        {
            var mockFile = nameof(SaveChangesTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new MergeDiskRepository<MockObj>(mockMapper, mockFile);
            var entity = new MockObj();
            mdr.Create(entity);
            var found = mdr.Find(entity.ID);
            Assert.NotNull(found);
            found.ImportHash = "changed";
            mdr.Update(found);
            mdr.SaveChanges();

            var updated = mdr.Find(entity.ID);
            Assert.NotNull(updated);
            Assert.Equal("changed", updated.ImportHash);
            Assert.Single(mdr.GetPendingChanges());
        }

        [Fact]
        public void CommitChangesTest()
        {
            var mockFile = nameof(CommitChangesTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new MergeDiskRepository<MockObj>(mockMapper, mockFile);
            var entity = new MockObj();
            mdr.Create(entity);
            var found = mdr.Find(entity.ID);
            Assert.NotNull(found);
            found.ImportHash = "changed";
            mdr.Update(found);
            mdr.SaveChanges();
            mdr.CommitChanges();

            var updated = mdr.Find(entity.ID);
            Assert.NotNull(updated);
            Assert.Equal("changed", updated.ImportHash);
            Assert.Empty(mdr.GetPendingChanges());
        }

        [Fact]
        public void FindConflictsTest()
        {
            var mockFile = nameof(FindConflictsTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new MergeDiskRepository<MockObj>(mockMapper, mockFile);
            var entity = new MockObj();
            mdr.Create(entity);
            var found = mdr.Find(entity.ID);
            var found2 = (MockObj) mdr.Find(entity.ID).Clone();
            Assert.NotNull(found);
            Assert.NotNull(found2);

            found.ImportHash = "changed";
            found2.ImportHash = "updated";
            Assert.NotEqual(found.ImportHash, found2.ImportHash);
            
            mdr.Update(found);
            mdr.Update(found2);

            mdr.SaveChanges();
            var conflicts = mdr.FindConflicts();

            Assert.Single(conflicts);
        }

        [Fact]
        public void GetPendingChangesTest()
        {
            var mockFile = nameof(GetPendingChangesTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new MergeDiskRepository<MockObj>(mockMapper, mockFile);
            var entity = new MockObj();
            mdr.Create(entity);
            var found = mdr.Find(entity.ID);
            Assert.NotNull(found);
            found.ImportHash = "changed";
            mdr.Update(found);
            mdr.SaveChanges();

            var updated = mdr.Find(entity.ID);
            Assert.NotNull(updated);
            Assert.Equal("changed", updated.ImportHash);
            Assert.Single(mdr.GetPendingChanges());
        }

        [Fact]
        public void ResolveConflictTest()
        {
            var mockFile = nameof(ResolveConflictTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new MergeDiskRepository<MockObj>(mockMapper, mockFile);
            var entity = new MockObj();
            mdr.Create(entity);
            var found = mdr.Find(entity.ID);
            var found2 = (MockObj)mdr.Find(entity.ID).Clone();
            Assert.NotNull(found);
            Assert.NotNull(found2);

            found.ImportHash = "changed";
            found2.ImportHash = "updated";
            Assert.NotEqual(found.ImportHash, found2.ImportHash);

            mdr.Update(found);
            mdr.Update(found2);

            mdr.SaveChanges();
            var conflicts = mdr.FindConflicts();

            Assert.Single(conflicts);

            mdr.ResolveConflict(found2);
            mdr.SaveChanges();
            conflicts = mdr.FindConflicts();
            Assert.Empty(conflicts);
        }

        [Fact]
        public void ResolveByDeleteTest()
        {
            var mockFile = nameof(ResolveConflictTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new MergeDiskRepository<MockObj>(mockMapper, mockFile);
            var entity = new MockObj();
            mdr.Create(entity);
            var found = mdr.Find(entity.ID);
            var found2 = (MockObj)mdr.Find(entity.ID).Clone();
            Assert.NotNull(found);
            Assert.NotNull(found2);

            found.ImportHash = "changed";
            found2.ImportHash = "updated";
            Assert.NotEqual(found.ImportHash, found2.ImportHash);

            mdr.Update(found);
            mdr.Update(found2);

            mdr.SaveChanges();
            var conflicts = mdr.FindConflicts();

            Assert.Single(conflicts);

            mdr.ResolveByDelete(found2.ID);
            mdr.SaveChanges();
            conflicts = mdr.FindConflicts();
            Assert.Empty(conflicts);
            Assert.Null(mdr.Find(found2.ID));
        }

        [Fact]
        public void RevertTest()
        {
            var mockFile = nameof(RevertTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new MergeDiskRepository<MockObj>(mockMapper, mockFile);
            var entity = new MockObj();
            mdr.Create(entity);
            var found = mdr.Find(entity.ID);
            Assert.NotNull(found);
            found.ImportHash = "changed";
            mdr.Update(found);
            var updated = mdr.Find(entity.ID);
            Assert.NotNull(updated);
            Assert.Equal("changed", updated.ImportHash);

            mdr.Revert(updated.ID);
            var revved = mdr.Find(updated.ID);
            Assert.NotNull(revved);
            Assert.Equal(entity.ImportHash, revved.ImportHash);
        }
    }
}