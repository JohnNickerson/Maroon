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
        public void Construct_SingleOriginRepositoryTest()
        {
            var mdr = new SingleOriginRepository<MockObj>(mockMapper);
            Assert.NotNull(mdr);
        }

        [Fact]
        public void CreateTest()
        {
            var mdr = new SingleOriginRepository<MockObj>(mockMapper);
            mdr.Create(new MockObj());
            Assert.Single(mdr.Items);
        }

        [Fact]
        public void DeleteTest()
        {
            var mdr = new SingleOriginRepository<MockObj>(mockMapper);
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
            var mdr = new SingleOriginRepository<MockObj>(mockMapper);
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
            var mdr = new SingleOriginRepository<MockObj>(mockMapper);
            var entity = new MockObj();
            mdr.Create(entity);
            var found = mdr.Find(entity.ID);
            Assert.NotNull(found);
        }

        [Fact]
        public void FindAllTest()
        {
            var mdr = new SingleOriginRepository<MockObj>(mockMapper);
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
            var mdr = new SingleOriginRepository<MockObj>(mockMapper);
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
            var mdr = new SingleOriginRepository<MockObj>(mockMapper);
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
            IDataSource<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new SingleOriginRepository<MockObj>(mockMapper);
            for (var x = 0; x < 1000000; x++)
            {
                var entity = new MockObj();
                mdr.Create(entity);
            }
            mdr.SaveChanges();
        }

        [Fact]
        public void FindConflictsTest()
        {
            var mdr = new SingleOriginRepository<MockObj>(mockMapper);
            var entity = new MockObj();
            mdr.Create(entity);
            // Create two new revisions with the same prev revision
            var rev2 = (MockObj)entity.Clone();
            rev2.ImportHash = "rev2";
            var rev3 = (MockObj)entity.Clone();
            rev3.ImportHash = "rev3";
            mdr.Update(rev2);
            mdr.Update(rev3);
            mdr.SaveChanges();
            // There should be a conflict now
            Assert.Equal(3, mockMapper.FindAll().Count(t => t.ID == entity.ID));
            var conflicts = mdr.FindConflicts();
            Assert.Single(conflicts);
            Assert.Equal(entity.ID, conflicts.First().First().ID);
            Assert.Equal(2, conflicts.First().Count());
            Assert.Contains(conflicts.First(), c => c.RevisionGuid == rev2.RevisionGuid);
            Assert.Contains(conflicts.First(), c => c.RevisionGuid == rev3.RevisionGuid);
        }

        [Fact]
        public void FindNoConflictsTest()
        {
            var mdr = new SingleOriginRepository<MockObj>(mockMapper);
            var entity = new MockObj();
            mdr.Create(entity);
            // Create a new revision properly
            var rev2 = (MockObj)entity.Clone();
            rev2.ImportHash = "rev2";
            mdr.Update(rev2);
            mdr.SaveChanges();
            // There should be no conflict now
            Assert.Equal(2, mockMapper.FindAll().Count(t => t.ID == entity.ID));
            var conflicts = mdr.FindConflicts();
            Assert.Empty(conflicts);
        }

        [Fact]
        public void MergeTest()
        {
            var mdr = new SingleOriginRepository<MockObj>(mockMapper);
            var entity = new MockObj();
            mdr.Create(entity);
            // Create two new revisions with the same prev revision
            // I don't like having to clone the objects like this. If that's how repositories have to be used, work on a better way.
            // In reality, I don't expect to create multiple revisions without expecting the original to be updated.
            var rev2 = (MockObj)entity.Clone();
            rev2.ImportHash = "rev2";
            var rev3 = (MockObj)entity.Clone();
            rev3.ImportHash = "rev3";
            mdr.Update(rev2);
            mdr.Update(rev3);
            // Merge the two revisions
            var rev4 = (MockObj)rev2.Clone();
            rev4.ImportHash = "rev4";
            mdr.Merge(rev4, rev3.RevisionGuid);
            mdr.SaveChanges();
            // There should be no conflict now
            Assert.Equal(4, mockMapper.FindAll().Count(t => t.ID == entity.ID));
            var conflicts = mdr.FindConflicts();
            Assert.Empty(conflicts);
        }

        [Fact]
        public void MergeNonExistentRevisionTest()
        {
            var mdr = new SingleOriginRepository<MockObj>(mockMapper);
            var entity = new MockObj();
            mdr.Create(entity);
            // Create a new revision properly
            var rev2 = (MockObj)entity.Clone();
            rev2.ImportHash = "rev2";
            mdr.Update(rev2);
            mdr.SaveChanges();
            // Try to merge with a non-existent revision
            var rev3 = (MockObj)rev2.Clone();
            rev3.ImportHash = "rev3";
            var nonExistentGuid = Guid.NewGuid();
            var exception = Record.Exception(() => mdr.Merge(rev3, nonExistentGuid));
            Assert.Null(exception); // Merging with a non-existent revision should not throw an exception
            mdr.SaveChanges();
            // There should be no conflict now
            Assert.Equal(3, mockMapper.FindAll().Count(t => t.ID == entity.ID));
            var conflicts = mdr.FindConflicts();
            Assert.Empty(conflicts);
        }
    }
}