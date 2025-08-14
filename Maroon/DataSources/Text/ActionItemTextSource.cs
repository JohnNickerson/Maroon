using System.IO.Abstractions;
using System.Text;
using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.DataSources.Text;

public class ActionItemTextSource : IDataSource<ActionItem>
{
    private IFileSystem _fileSystem;
    private string _fileName;
    private Dictionary<Guid, ActionItem> _index;
    private bool _reindex;
    private DateTime _lastWriteTime;

    public ActionItemTextSource(string fileName, IFileSystem? fileSystem = null)
    {
        this._fileSystem = fileSystem ?? new FileSystem();
        this._fileName = Environment.ExpandEnvironmentVariables(fileName);
        _index = [];
        _reindex = true;
    }

    private string Stringify(ActionItem item, bool includeContext = true)
    {
        var result = new StringBuilder();
        if (includeContext)
        {
            result.AppendLine($"@{item.Context}");
        }
        result.AppendLine($"\t{item.Title.Trim()}");
        foreach (var note in item.Notes)
        {
            result.AppendLine($"\t\t- {note}");
        }
        foreach (var tag in item.Tags)
        {
            result.AppendLine($"\t\t#{tag.Key}:{tag.Value}");
        }
        if (item.Upvotes > 0)
        {
            result.AppendLine($"\t\t#upvotes:{item.Upvotes}");
        }

        if (item.DoneDate.HasValue)
        {
            result.AppendLine($"\t\t#done-date:{item.DoneDate.Value:yyyy-MM-dd}");
        }

        if (item.TickleDate.HasValue)
        {
            result.AppendLine($"\t\t#tickle-date:{item.TickleDate.Value:yyyy-MM-dd}");
        }

        result.AppendLine($"\t\t#id:{item.ID}");
        if (item.IsDeleted)
        {
            result.AppendLine("\t\t#deleted:true");
        }
        result.AppendLine($"\t\t#last-modified:{item.LastModified:s}");
        result.AppendLine($"\t\t#revision:{item.RevisionGuid}");
        if (item.PrevRevision.HasValue)
        {
            result.AppendLine($"\t\t#prev-revision:{item.PrevRevision}");
        }
        if (item.MergeRevision.HasValue)
        {
            result.AppendLine($"\t\t#merge-revision:{item.MergeRevision}");
        }
        if (!string.IsNullOrEmpty(item.ImportHash))
        {
            result.AppendLine($"\t\t#import-hash:{item.ImportHash}");
        }
        if (item.ProjectId != null)
        {
            result.AppendLine($"\t\t#project:{item.ProjectId}");
        }

        if (item.ParentId != null)
        {
            result.AppendLine($"\t\t#priority-parent:{item.ParentId}");
        }
        return result.ToString();
    }

    public ActionItem Create(ActionItem item)
    {
        item.PrevRevision = null;
        item.RevisionGuid ??= new Guid();
        item.LastModified = DateTime.Now;
        item.MergeRevision = null;
        item.IsDeleted = false;
        var itemString = Stringify(item);
        _fileSystem.File.AppendAllText(_fileName, itemString);
        _index[item.RevisionGuid.Value] = item;
        return item;
    }

    public ActionItem Delete(ActionItem item)
    {
        item.PrevRevision = item.RevisionGuid;
        item.RevisionGuid = new Guid();
        item.LastModified = DateTime.Now;
        item.MergeRevision = null;
        item.IsDeleted = true;
        var itemString = Stringify(item);
        _fileSystem.File.AppendAllText(_fileName, itemString);
        _index[item.RevisionGuid.Value] = item;
        return item;
    }

    public IEnumerable<ActionItem> FindAll()
    {
        throw new NotImplementedException();
    }

    public ActionItem? FindRevision(Guid id)
    {
        throw new NotImplementedException();
    }

    public DateTime GetLastWriteTime()
    {
        throw new NotImplementedException();
    }

    public void Purge(Guid id)
    {
        throw new NotImplementedException();
    }

    public ActionItem Update(ActionItem item)
    {
        item.PrevRevision = item.RevisionGuid;
        item.RevisionGuid = new Guid();
        item.LastModified = DateTime.Now;
        item.IsDeleted = false;
        var itemString = Stringify(item);
        _fileSystem.File.AppendAllText(_fileName, itemString);
        _index[item.RevisionGuid.Value] = item;
        return item;
    }
}