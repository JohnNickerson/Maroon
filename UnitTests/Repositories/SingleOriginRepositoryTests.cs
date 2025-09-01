using Xunit;
using AssimilationSoftware.Maroon.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssimilationSoftware.Maroon.Interfaces;

namespace AssimilationSoftware.Maroon.Repositories.Tests
{
    public class SingleOriginRepositoryTests
    {
        private IDataSource<MockObj> mockMapper = new MockDiskMapper();


        [Fact]
        public void SingleOriginRepositoryTest()
        {
            var mockFile = nameof(SingleOriginRepositoryTest);
            var mdr = new SingleOriginRepository<MockObj>(mockMapper, mockFile);
        }

        [Fact]
        public void CreateTest()
        {
            var mockFile = nameof(CreateTest);
            var mdr = new SingleOriginRepository<MockObj>(mockMapper, mockFile);
            mdr.Create(new MockObj());
        }

        [Fact]
        public void DeleteTest()
        {
            var mockFile = nameof(DeleteTest);
            var mdr = new SingleOriginRepository<MockObj>(mockMapper, mockFile);
            var entity = new MockObj();
            mdr.Create(entity);
            var found = mdr.Find(entity.ID);
            Assert.NotNull(found);
            mdr.Delete(found);
            var notFound = mdr.Find(found.ID);
            Assert.Null(notFound);
        }

        [Fact]
        public void SaveDeleteTest()
        {
            var mockFile = nameof(DeleteTest);
            var mdr = new SingleOriginRepository<MockObj>(mockMapper, mockFile);
            var entity = new MockObj();
            mdr.Create(entity);
            var found = mdr.Find(entity.ID);
            Assert.NotNull(found);
            mdr.Delete(found);
            mdr.SaveChanges();
            var notFound = mdr.Find(found.ID);
            Assert.Null(notFound);
            Assert.DoesNotContain(mdr.Items, e => e.ID == found.ID);
        }

        [Fact]
        public void FindTest()
        {
            var mockFile = nameof(FindTest);
            var mdr = new SingleOriginRepository<MockObj>(mockMapper, mockFile);
            var entity = new MockObj();
            mdr.Create(entity);
            var found = mdr.Find(entity.ID);
            Assert.NotNull(found);
        }

        [Fact]
        public void FindAllTest()
        {
            var mockFile = nameof(FindAllTest);
            var mdr = new SingleOriginRepository<MockObj>(mockMapper, mockFile);
            var entity = new MockObj();
            var entity2 = new MockObj();
            mdr.Create(entity);
            mdr.Create(entity2);
            var found = mdr.FindAll();
            Assert.NotNull(found);
            Assert.Equal(2, found.Count());
        }

        [Fact]
        public void SaveChangesTest()
        {
            var mockFile = nameof(SaveChangesTest);
            var mdr = new SingleOriginRepository<MockObj>(mockMapper, mockFile);
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
        }

        [Fact]
        public void UpdateTest()
        {
            var mockFile = nameof(UpdateTest);
            var mdr = new SingleOriginRepository<MockObj>(mockMapper, mockFile);
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
        public void UpdateBulkTest()
        {
            var mockFile = nameof(UpdateTest);
            IDataSource<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new SingleOriginRepository<MockObj>(mockMapper, mockFile);
            for (var x = 0; x < 1000000; x++)
            {
                var entity = new MockObj();
                mdr.Create(entity);
            }
            mdr.SaveChanges();
        }
    }
}