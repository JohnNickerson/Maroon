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
    public class OriginShardRepositoryTests
    {
        private IDataSource<MockObj> mockMapper = new MockDiskMapper();


        [Fact]
        public void Construct_OriginDiskRepositoryTest()
        {
            var mdr = new OriginShardRepository<MockObj>(mockMapper);
            Assert.NotNull(mdr);
        }

        [Fact]
        public void Construct_OriginShardRepository_WithOtherDataSources()
        {
            var mdr = new OriginShardRepository<MockObj>(mockMapper, new MockDiskMapper(), new MockDiskMapper());
            Assert.NotNull(mdr);
        }

        [Fact]
        public void Create_Saves_IntoOriginShard()
        {
            var secondMapper = new MockDiskMapper();
            var mdr = new OriginShardRepository<MockObj>(mockMapper, secondMapper);
            var obj = new MockObj() { ID = Guid.NewGuid(), ImportHash = "Test" };
            mdr.Create(obj);
            var retrieved = mdr.Find(obj.ID);
            Assert.NotNull(retrieved);
            Assert.Equal(obj.ID, retrieved.ID);
            Assert.Equal(obj.ImportHash, retrieved.ImportHash);
            Assert.Single(mockMapper.FindAll());
            Assert.Empty(secondMapper.FindAll());
        }

        [Fact]
        public void Update_Saves_IntoOriginShard()
        {
            var secondMapper = new MockDiskMapper();
            var mdr = new OriginShardRepository<MockObj>(mockMapper, secondMapper);
            var obj = new MockObj() { ID = Guid.NewGuid(), ImportHash = "Test", IsDeleted = false, LastModified = DateTime.UtcNow, RevisionGuid = Guid.NewGuid() };
            secondMapper.Insert(obj);
            var retrievedObj = mdr.Find(obj.ID);
            retrievedObj.ImportHash = "Updated";
            mdr.Update(retrievedObj);
            var retrieved = mdr.Find(obj.ID);
            Assert.NotNull(retrieved);
            Assert.Equal(obj.ID, retrieved.ID);
            Assert.Equal("Updated", retrieved.ImportHash);
            Assert.Single(mockMapper.FindAll());
            Assert.Single(secondMapper.FindAll());
        }

        [Fact]
        public void Delete_WritesToLocalShard_Only()
        {
            var secondMapper = new MockDiskMapper();
            var mdr = new OriginShardRepository<MockObj>(mockMapper, secondMapper);
            var obj = new MockObj() { ID = Guid.NewGuid(), ImportHash = "Test", IsDeleted = false, LastModified = DateTime.UtcNow, RevisionGuid = Guid.NewGuid() };
            secondMapper.Insert(obj);
            var found = mdr.Find(obj.ID);
            Assert.NotNull(found);
            mdr.Delete(found);
            var retrieved = mdr.Find(obj.ID);
            Assert.Null(retrieved);
            Assert.Single(mockMapper.FindAll());
            Assert.Single(secondMapper.FindAll());
        }

        [Fact]
        public void FindAll_MergesDataSources_Correctly()
        {
            var secondMapper = new MockDiskMapper();
            var mdr = new OriginShardRepository<MockObj>(mockMapper, secondMapper);
            var obj1 = new MockObj() { ID = Guid.NewGuid(), ImportHash = "FromOrigin", IsDeleted = false, LastModified = DateTime.UtcNow.AddMinutes(-10), RevisionGuid = Guid.NewGuid() };
            var obj2 = new MockObj() { ID = Guid.NewGuid(), ImportHash = "FromLocal", IsDeleted = false, LastModified = DateTime.UtcNow, RevisionGuid = Guid.NewGuid() };
            var obj3 = new MockObj() { ID = Guid.NewGuid(), ImportHash = "DeletedInLocal", IsDeleted = false, LastModified = DateTime.UtcNow.AddMinutes(-5), RevisionGuid = Guid.NewGuid() };
            mockMapper.Insert(obj1);
            secondMapper.Insert(obj2);
            secondMapper.Insert(obj3);
            // Now delete obj3 in local shard
            var toDelete = mdr.Find(obj3.ID);
            mdr.Delete(toDelete);
            var allItems = mdr.FindAll().ToList();
            Assert.Equal(2, allItems.Count);
            Assert.Contains(allItems, o => o.ID == obj1.ID);
            Assert.Contains(allItems, o => o.ID == obj2.ID);
            Assert.DoesNotContain(allItems, o => o.ID == obj3.ID);
        }

        [Fact]
        public void Items_Property_Works_Correctly()
        {
            var secondMapper = new MockDiskMapper();
            var mdr = new OriginShardRepository<MockObj>(mockMapper, secondMapper);
            var obj1 = new MockObj() { ID = Guid.NewGuid(), ImportHash = "FromOrigin", IsDeleted = false, LastModified = DateTime.UtcNow.AddMinutes(-10), RevisionGuid = Guid.NewGuid() };
            var obj2 = new MockObj() { ID = Guid.NewGuid(), ImportHash = "FromLocal", IsDeleted = false, LastModified = DateTime.UtcNow, RevisionGuid = Guid.NewGuid() };
            var obj3 = new MockObj() { ID = Guid.NewGuid(), ImportHash = "DeletedInLocal", IsDeleted = false, LastModified = DateTime.UtcNow.AddMinutes(-5), RevisionGuid = Guid.NewGuid() };
            mockMapper.Insert(obj1);
            secondMapper.Insert(obj2);
            secondMapper.Insert(obj3);
            // Now delete obj3 in local shard
            var toDelete = mdr.Find(obj3.ID);
            mdr.Delete(toDelete);
            var allItems = mdr.Items.ToList();
            Assert.Equal(2, allItems.Count);
            Assert.Contains(allItems, o => o.ID == obj1.ID);
            Assert.Contains(allItems, o => o.ID == obj2.ID);
            Assert.DoesNotContain(allItems, o => o.ID == obj3.ID);
        }

        [Fact]
        public void Find_Returns_Null_For_Deleted_Items()
        {
            var secondMapper = new MockDiskMapper();
            var mdr = new OriginShardRepository<MockObj>(mockMapper, secondMapper);
            var obj = new MockObj() { ID = Guid.NewGuid(), ImportHash = "ToBeDeleted", IsDeleted = false, LastModified = DateTime.UtcNow, RevisionGuid = Guid.NewGuid() };
            secondMapper.Insert(obj);
            var found = mdr.Find(obj.ID);
            Assert.NotNull(found);
            mdr.Delete(found);
            var retrieved = mdr.Find(obj.ID);
            Assert.Null(retrieved);
        }

        [Fact]
        public void Find_Returns_Null_For_Nonexistent_Items()
        {
            var secondMapper = new MockDiskMapper();
            var mdr = new OriginShardRepository<MockObj>(mockMapper, secondMapper);
            var retrieved = mdr.Find(Guid.NewGuid());
            Assert.Null(retrieved);
        }

        [Fact]
        public void Items_Property_Empty_If_No_Items()
        {
            var secondMapper = new MockDiskMapper();
            var mdr = new OriginShardRepository<MockObj>(mockMapper, secondMapper);
            var allItems = mdr.Items.ToList();
            Assert.Empty(allItems);
        }

        [Fact]
        public void FindAll_Returns_Empty_If_No_Items()
        {
            var secondMapper = new MockDiskMapper();
            var mdr = new OriginShardRepository<MockObj>(mockMapper, secondMapper);
            var allItems = mdr.FindAll().ToList();
            Assert.Empty(allItems);
        }

        [Fact]
        public void Find_Conflicts_Returns_Conflicted_Items()
        {
            var secondMapper = new MockDiskMapper();
            var mdr = new OriginShardRepository<MockObj>(mockMapper, secondMapper);
            var obj = new MockObj() { ID = Guid.NewGuid(), ImportHash = "Conflict", IsDeleted = false, LastModified = DateTime.UtcNow, RevisionGuid = Guid.NewGuid() };
            mockMapper.Insert(obj);
            // Insert a conflicting version in the local shard
            var conflictingObj = new MockObj() { ID = obj.ID, ImportHash = "ConflictLocal", IsDeleted = false, LastModified = DateTime.UtcNow.AddMinutes(1), RevisionGuid = Guid.NewGuid(), PrevRevision = obj.RevisionGuid };
            mockMapper.Insert(conflictingObj);
            // Insert a third version in the remote shard
            var conflictingObj2 = new MockObj() { ID = obj.ID, ImportHash = "ConflictRemote", IsDeleted = false, LastModified = DateTime.UtcNow.AddMinutes(2), RevisionGuid = Guid.NewGuid(), PrevRevision = obj.RevisionGuid };
            secondMapper.Insert(conflictingObj2);
            var conflicts = mdr.FindConflicts().ToList();
            Assert.Single(conflicts);
            var conflictGroup = conflicts[0];
            Assert.Equal(2, conflictGroup.Count());
            Assert.Contains(conflictGroup, c => c.RevisionGuid == conflictingObj.RevisionGuid);
            Assert.Contains(conflictGroup, c => c.RevisionGuid == conflictingObj2.RevisionGuid);
        }

        [Fact]
        public void Find_Conflicts_Returns_Empty_If_No_Conflicts()
        {
            var secondMapper = new MockDiskMapper();
            var mdr = new OriginShardRepository<MockObj>(mockMapper, secondMapper);
            var obj = new MockObj() { ID = Guid.NewGuid(), ImportHash = "NoConflict", IsDeleted = false, LastModified = DateTime.UtcNow, RevisionGuid = Guid.NewGuid() };
            mockMapper.Insert(obj);
            var nonConflictingObj = new MockObj() { ID = Guid.NewGuid(), ImportHash = "AlsoNoConflict", IsDeleted = false, LastModified = DateTime.UtcNow, RevisionGuid = Guid.NewGuid() };
            secondMapper.Insert(nonConflictingObj);
            var conflicts = mdr.FindConflicts().ToList();
            Assert.Empty(conflicts);
        }

        [Fact]
        public void Merge_Writes_To_Local_Store()
        {
            var secondMapper = new MockDiskMapper();
            var mdr = new OriginShardRepository<MockObj>(mockMapper, secondMapper);
            var obj = new MockObj() { ID = Guid.NewGuid(), ImportHash = "ToMerge", IsDeleted = false, LastModified = DateTime.UtcNow, RevisionGuid = Guid.NewGuid() };
            mockMapper.Insert(obj);
            // Create two conflicting edits to merge.
            var edit1 = (MockObj)obj.Clone();
            edit1.ImportHash = "Edit1";
            edit1.UpdateRevision();
            var edit2 = (MockObj)obj.Clone();
            edit2.ImportHash = "Edit2";
            edit2.UpdateRevision();
            mockMapper.Insert(edit1);
            secondMapper.Insert(edit2);
            // Now merge them
            var merged = edit1.Clone() as MockObj;
            merged.ImportHash = "Merged";
            mdr.Merge(merged, edit2.RevisionGuid);
            var retrieved = mdr.Find(merged.ID);
            Assert.NotNull(retrieved);
            Assert.Equal("Merged", retrieved.ImportHash);
            Assert.Single(secondMapper.FindAll());
            var conflicts = mdr.FindConflicts().ToList();
            Assert.Empty(conflicts);
        }

        [Fact]
        public void Compress_Removes_Local_Revisions()
        {
            var secondMapper = new MockDiskMapper();
            var mdr = new OriginShardRepository<MockObj>(mockMapper, secondMapper);
            var obj = new MockObj() { ID = Guid.NewGuid(), ImportHash = "ToCompress", IsDeleted = false, LastModified = DateTime.UtcNow, RevisionGuid = Guid.NewGuid() };
            mockMapper.Insert(obj);
            // Create a local edit
            var edit1 = (MockObj)obj.Clone();
            edit1.ImportHash = "Edit1";
            edit1.UpdateRevision();
            secondMapper.Insert(edit1);
            // Compress
            mdr.Compress();
            // The local edit should be removed
            var retrieved = mdr.Find(obj.ID);
            Assert.NotNull(retrieved);
            Assert.Equal(edit1.RevisionGuid, retrieved.RevisionGuid);
            Assert.Empty(mockMapper.FindAll());
            Assert.Single(secondMapper.FindAll());
        }

        [Fact]
        public void Get_Obsolete_Revisions_Returns_Local_Revisions()
        {
            var secondMapper = new MockDiskMapper();
            var mdr = new OriginShardRepository<MockObj>(mockMapper, secondMapper);
            var obj = new MockObj() { ID = Guid.NewGuid(), ImportHash = "ToCompress", IsDeleted = false, LastModified = DateTime.UtcNow, RevisionGuid = Guid.NewGuid() };
            mockMapper.Insert(obj);
            // Create a local edit
            var edit1 = (MockObj)obj.Clone();
            edit1.ImportHash = "Edit1";
            edit1.UpdateRevision();
            secondMapper.Insert(edit1);
            var obsoleteRevisions = mdr.FindObsoleteRevisionIds().ToList();
            Assert.Single(obsoleteRevisions);
            Assert.Equal(edit1.RevisionGuid, obsoleteRevisions[0]);
        }
    }
}