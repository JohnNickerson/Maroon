using Microsoft.VisualStudio.TestTools.UnitTesting;
using AssimilationSoftware.Maroon.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssimilationSoftware.Maroon.Interfaces;

namespace AssimilationSoftware.Maroon.Repositories.Tests
{
    [TestClass()]
    public class SingleOriginRepositoryTests
    {
        [TestMethod()]
        public void SingleOriginRepositoryTest()
        {
            var mockFile = nameof(SingleOriginRepositoryTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new SingleOriginRepository<MockObj>(mockMapper, mockFile);
        }

        [TestMethod()]
        public void CreateTest()
        {
            var mockFile = nameof(CreateTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new SingleOriginRepository<MockObj>(mockMapper, mockFile);
            mdr.Create(new MockObj());
        }

        [TestMethod()]
        public void DeleteTest()
        {
            var mockFile = nameof(DeleteTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new SingleOriginRepository<MockObj>(mockMapper, mockFile);
            var entity = new MockObj();
            mdr.Create(entity);
            var found = mdr.Find(entity.ID);
            Assert.IsNotNull(found);
            mdr.Delete(found);
            var notFound = mdr.Find(found.ID);
            Assert.IsNull(notFound);
        }

        [TestMethod()]
        public void SaveDeleteTest()
        {
            var mockFile = nameof(DeleteTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new SingleOriginRepository<MockObj>(mockMapper, mockFile);
            var entity = new MockObj();
            mdr.Create(entity);
            var found = mdr.Find(entity.ID);
            Assert.IsNotNull(found);
            mdr.Delete(found);
            mdr.SaveChanges();
            var notFound = mdr.Find(found.ID);
            Assert.IsNull(notFound);
            Assert.IsFalse(mdr.Items.Any(e => e.ID == found.ID));
        }

        [TestMethod()]
        public void FindTest()
        {
            var mockFile = nameof(FindTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new SingleOriginRepository<MockObj>(mockMapper, mockFile);
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
            var mdr = new SingleOriginRepository<MockObj>(mockMapper, mockFile);
            var entity = new MockObj();
            var entity2 = new MockObj();
            mdr.Create(entity);
            mdr.Create(entity2);
            var found = mdr.FindAll();
            Assert.IsNotNull(found);
            Assert.AreEqual(2, found.Count());
        }

        [TestMethod()]
        public void SaveChangesTest()
        {
            var mockFile = nameof(SaveChangesTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new SingleOriginRepository<MockObj>(mockMapper, mockFile);
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
        }

        [TestMethod()]
        public void UpdateTest()
        {
            var mockFile = nameof(UpdateTest);
            IDiskMapper<MockObj> mockMapper = new MockDiskMapper();
            var mdr = new SingleOriginRepository<MockObj>(mockMapper, mockFile);
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
    }
}