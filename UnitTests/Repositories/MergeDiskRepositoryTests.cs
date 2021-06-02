using Microsoft.VisualStudio.TestTools.UnitTesting;
using AssimilationSoftware.Maroon.Repositories;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssimilationSoftware.Maroon.Interfaces;

namespace AssimilationSoftware.Maroon.Repositories.Tests
{
    [TestClass()]
    public class MergeDiskRepositoryTests
    {
        [TestMethod()]
        public void MergeDiskRepositoryTest()
        {
            var mockFile = nameof(MergeDiskRepositoryTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new MergeDiskRepository<MockObj>(mockMapper, mockFile);
        }

        [TestMethod()]
        public void CreateTest()
        {
            var mockFile = nameof(CreateTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new MergeDiskRepository<MockObj>(mockMapper, mockFile);
            mdr.Create(new MockObj());
        }

        [TestMethod()]
        public void FindTest()
        {
            var mockFile = nameof(FindTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new MergeDiskRepository<MockObj>(mockMapper, mockFile);
            var entity = new MockObj();
            mdr.Create(entity);
            var found = mdr.Find(entity.ID);
            Assert.IsNotNull(found);
        }

        [TestMethod()]
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
            Assert.IsNotNull(found);
            Assert.AreEqual(2, found.Count());
        }

        [TestMethod()]
        public void DeleteTest()
        {
            var mockFile = nameof(DeleteTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new MergeDiskRepository<MockObj>(mockMapper, mockFile);
            var entity = new MockObj();
            mdr.Create(entity);
            var found = mdr.Find(entity.ID);
            Assert.IsNotNull(found);
            mdr.Delete(found);
            var notFound = mdr.Find(entity.ID);
            Assert.IsNull(notFound);
        }

        [TestMethod()]
        public void UpdateTest()
        {
            var mockFile = nameof(UpdateTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new MergeDiskRepository<MockObj>(mockMapper, mockFile);
            var entity = new MockObj();
            mdr.Create(entity);
            var found = mdr.Find(entity.ID);
            Assert.IsNotNull(found);
            found.ImportHash = "changed";
            mdr.Update(found);
            var updated = mdr.Find(entity.ID);
            Assert.IsNotNull(updated);
            Assert.AreEqual("changed", updated.ImportHash);
        }

        [TestMethod()]
        public void SaveChangesTest()
        {
            var mockFile = nameof(SaveChangesTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new MergeDiskRepository<MockObj>(mockMapper, mockFile);
            var entity = new MockObj();
            mdr.Create(entity);
            var found = mdr.Find(entity.ID);
            Assert.IsNotNull(found);
            found.ImportHash = "changed";
            mdr.Update(found);
            mdr.SaveChanges();

            var updated = mdr.Find(entity.ID);
            Assert.IsNotNull(updated);
            Assert.AreEqual("changed", updated.ImportHash);
            Assert.AreEqual(1, mdr.GetPendingChanges().Count);
        }

        [TestMethod()]
        public void CommitChangesTest()
        {
            var mockFile = nameof(CommitChangesTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new MergeDiskRepository<MockObj>(mockMapper, mockFile);
            var entity = new MockObj();
            mdr.Create(entity);
            var found = mdr.Find(entity.ID);
            Assert.IsNotNull(found);
            found.ImportHash = "changed";
            mdr.Update(found);
            mdr.SaveChanges();
            mdr.CommitChanges();

            var updated = mdr.Find(entity.ID);
            Assert.IsNotNull(updated);
            Assert.AreEqual("changed", updated.ImportHash);
            Assert.AreEqual(0, mdr.GetPendingChanges().Count);
        }

        [TestMethod()]
        public void FindConflictsTest()
        {
            var mockFile = nameof(FindConflictsTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new MergeDiskRepository<MockObj>(mockMapper, mockFile);
            var entity = new MockObj();
            mdr.Create(entity);
            var found = mdr.Find(entity.ID);
            var found2 = (MockObj) mdr.Find(entity.ID).Clone();
            Assert.IsNotNull(found);
            Assert.IsNotNull(found2);

            found.ImportHash = "changed";
            found2.ImportHash = "updated";
            Assert.AreNotEqual(found.ImportHash, found2.ImportHash);
            
            mdr.Update(found);
            mdr.Update(found2);

            mdr.SaveChanges();
            var conflicts = mdr.FindConflicts();

            Assert.AreEqual(1, conflicts.Count);
        }

        [TestMethod()]
        public void GetPendingChangesTest()
        {
            var mockFile = nameof(GetPendingChangesTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new MergeDiskRepository<MockObj>(mockMapper, mockFile);
            var entity = new MockObj();
            mdr.Create(entity);
            var found = mdr.Find(entity.ID);
            Assert.IsNotNull(found);
            found.ImportHash = "changed";
            mdr.Update(found);
            mdr.SaveChanges();

            var updated = mdr.Find(entity.ID);
            Assert.IsNotNull(updated);
            Assert.AreEqual("changed", updated.ImportHash);
            Assert.AreEqual(1, mdr.GetPendingChanges().Count);
        }

        [TestMethod()]
        public void ResolveConflictTest()
        {
            var mockFile = nameof(ResolveConflictTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new MergeDiskRepository<MockObj>(mockMapper, mockFile);
            var entity = new MockObj();
            mdr.Create(entity);
            var found = mdr.Find(entity.ID);
            var found2 = (MockObj)mdr.Find(entity.ID).Clone();
            Assert.IsNotNull(found);
            Assert.IsNotNull(found2);

            found.ImportHash = "changed";
            found2.ImportHash = "updated";
            Assert.AreNotEqual(found.ImportHash, found2.ImportHash);

            mdr.Update(found);
            mdr.Update(found2);

            mdr.SaveChanges();
            var conflicts = mdr.FindConflicts();

            Assert.AreEqual(1, conflicts.Count);

            mdr.ResolveConflict(found2);
            mdr.SaveChanges();
            conflicts = mdr.FindConflicts();
            Assert.AreEqual(0, conflicts.Count);
        }

        [TestMethod()]
        public void ResolveByDeleteTest()
        {
            var mockFile = nameof(ResolveConflictTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new MergeDiskRepository<MockObj>(mockMapper, mockFile);
            var entity = new MockObj();
            mdr.Create(entity);
            var found = mdr.Find(entity.ID);
            var found2 = (MockObj)mdr.Find(entity.ID).Clone();
            Assert.IsNotNull(found);
            Assert.IsNotNull(found2);

            found.ImportHash = "changed";
            found2.ImportHash = "updated";
            Assert.AreNotEqual(found.ImportHash, found2.ImportHash);

            mdr.Update(found);
            mdr.Update(found2);

            mdr.SaveChanges();
            var conflicts = mdr.FindConflicts();

            Assert.AreEqual(1, conflicts.Count);

            mdr.ResolveByDelete(found2.ID);
            mdr.SaveChanges();
            conflicts = mdr.FindConflicts();
            Assert.AreEqual(0, conflicts.Count);
            Assert.IsNull(mdr.Find(found2.ID));
        }

        [TestMethod()]
        public void RevertTest()
        {
            var mockFile = nameof(RevertTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new MergeDiskRepository<MockObj>(mockMapper, mockFile);
            var entity = new MockObj();
            mdr.Create(entity);
            var found = mdr.Find(entity.ID);
            Assert.IsNotNull(found);
            found.ImportHash = "changed";
            mdr.Update(found);
            var updated = mdr.Find(entity.ID);
            Assert.IsNotNull(updated);
            Assert.AreEqual("changed", updated.ImportHash);

            mdr.Revert(updated.ID);
            var revved = mdr.Find(updated.ID);
            Assert.IsNotNull(revved);
            Assert.AreEqual(entity.ImportHash, revved.ImportHash);
        }
    }
}