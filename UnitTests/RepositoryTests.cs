using System.IO.Abstractions.TestingHelpers;
using AssimilationSoftware.Maroon.DataSources.Text;
using AssimilationSoftware.Maroon.Model;
using AssimilationSoftware.Maroon.Repositories;

namespace AssimilationSoftware.Maroon.UnitTests;

public class RepositoryTests
{
    public RepositoryTests()
    {
    }

    // This isn't ready yet.
    // [Fact]
    // public void GetPendingChangesCount_ShouldReturnTwo_WhenThereAreTwoPendingChanges()
    // {
    //     // Arrange
    //     var mockMapper = new TimeLogCsvSource("time.csv", new MockFileSystem());
    //     var repository1 = new MergeDiskRepository<TimeLogEntry>(mockMapper, ".");
    //     var repository2 = new MergeDiskRepository<TimeLogEntry>(mockMapper, ".");

    //     // Act
    //     repository1.Create(new TimeLogEntry()
    //     {
    //         Billable = true,
    //         Client = "ClientA",
    //         ID = Guid.NewGuid(),
    //         EndTime = DateTime.Now,
    //         StartTime = DateTime.Now.AddHours(-1),
    //         IsDeleted = false,
    //         LastModified = DateTime.Now,
    //         Note = "Test entry 1",
    //         Project = "ProjectA",
    //         PrevRevision = null,
    //         RevisionGuid = Guid.NewGuid(),
    //     });
    //     repository2.Create(new TimeLogEntry()
    //     {
    //         Billable = false,
    //         Client = "ClientB",
    //         ID = Guid.NewGuid(),
    //         EndTime = DateTime.Now,
    //         StartTime = DateTime.Now.AddHours(-2),
    //         IsDeleted = false,
    //         LastModified = DateTime.Now,
    //         Note = "Test entry 2",
    //         Project = "ProjectB",
    //         PrevRevision = null,
    //         RevisionGuid = Guid.NewGuid(),
    //     });
    //     repository1.SaveChanges();
    //     repository2.SaveChanges();

    //     // Assert
    //     repository1.FindAll();
    //     Assert.Equal(2, repository1.GetPendingChanges().Count());
    //     repository2.FindAll();
    //     Assert.Equal(2, repository2.GetPendingChanges().Count());
    // }
}