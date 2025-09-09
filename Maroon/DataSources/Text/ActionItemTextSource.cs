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
        result.AppendLine($"\t\t#last-modified:{item.LastModified:O}");
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

    public ActionItem Insert(ActionItem item)
    {
        item.LastModified = DateTime.Now;
        var itemString = Stringify(item);
        _fileSystem.File.AppendAllText(_fileName, itemString);
        _index[item.RevisionGuid] = item;
        return item;
    }

    public IEnumerable<ActionItem> FindAll()
    {
        if (!_reindex)
        {
            foreach (var item in _index.Values)
            {
                yield return item;
            }
            yield break;
        }

        if (!_fileSystem.File.Exists(_fileName))
        {
            yield break;
        }
        _index = [];
        _lastWriteTime = DateTime.MinValue;
        ActionItem? current = null;
        var context = "inbox";
        foreach (var line in _fileSystem.File.ReadLines(_fileName))
        {
            if (line.StartsWith("@"))
            {
                // New context.
                context = line.Substring(1).ToLower();
            }
            else if (line.Trim().StartsWith("#"))
            {
                // Tag or metadata line.
                var parts = line.Trim().Split(new[] { ':' }, 2);
                if (parts.Length > 1)
                {
                    var tag = parts[0].Replace("#", string.Empty).Trim();
                    // TODO: use "TryParse" for better error handling.
                    switch (tag.ToLower())
                    {
                        case "id":
                            if (current != null)
                            {
                                current.ID = Guid.Parse(parts[1]);
                            }
                            break;
                        case "revision":
                            if (current != null)
                            {
                                current.RevisionGuid = Guid.Parse(parts[1]);
                                _index[current.RevisionGuid] = current;
                            }
                            break;
                        case "prev-revision":
                            if (current != null)
                            {
                                current.PrevRevision = Guid.Parse(parts[1]);
                            }
                            break;
                        case "import-hash":
                            if (current != null)
                            {
                                current.ImportHash = parts[1];
                            }
                            break;
                        case "project":
                            if (current != null)
                            {
                                current.ProjectId = Guid.Parse(parts[1]);
                            }
                            break;
                        case "priority-parent":
                            if (current != null)
                            {
                                current.ParentId = Guid.Parse(parts[1]);
                            }
                            break;
                        case "done-date":
                            if (current != null)
                            {
                                current.DoneDate = DateTime.Parse(parts[1]);
                                current.Context = "done";
                            }
                            break;
                        case "tickle-date":
                            if (current != null)
                            {
                                current.TickleDate = DateTime.Parse(parts[1]);
                            }
                            break;
                        case "upvotes":
                            if (current != null)
                            {
                                current.Upvotes = int.Parse(parts[1]);
                            }
                            break;
                        case "merge-revision":
                            if (current != null)
                            {
                                current.MergeRevision = Guid.Parse(parts[1]);
                            }
                            break;
                        case "deleted":
                            if (current != null)
                            {
                                current.IsDeleted = parts[1].Trim().ToLower() == "true";
                            }
                            break;
                        case "last-modified":
                            if (current != null)
                            {
                                current.LastModified = DateTime.Parse(parts[1]);
                                if (_lastWriteTime < current.LastModified)
                                {
                                    _lastWriteTime = current.LastModified;
                                }
                            }
                            // Handle other tags as needed.
                            break;
                        default:
                            if (current != null)
                            {
                                current.Tags[tag] = parts[1].Trim();
                            }
                            break;
                    }
                }
                else
                {
                    // Malformed tag line, treat it as a note.
                    if (current != null)
                    {
                        current.Notes.Add(line.Trim());
                    }
                }
            }
            else if (line.Trim().StartsWith("-"))
            {
                // Note line.
                if (current != null)
                {
                    current.Notes.Add(line.Trim().Substring(1).Trim());
                }
            }
            else if (line.Trim().Length == 0)
            {
                // Empty line, finalize current item.
                if (current != null)
                {
                    yield return current;
                    current = null;
                }
            }
            else
            {
                if (current != null)
                {
                    yield return current;
                }
                // Start a new action item.
                current = new ActionItem
                {
                    Context = context,
                    Title = line.Trim(),
                    Notes = [],
                    Tags = [],
                };
            }
        }
        if (current != null)
        {
            yield return current;
        }
        _reindex = false;
    }

    public ActionItem? FindRevision(Guid id)
    {
        if (_reindex)
        {
            FindAll();
        }
        if (_index.TryGetValue(id, out var result))
        {
            return result;
        }
        return null;
    }

    public DateTime GetLastWriteTime()
    {
        if (_reindex)
        {
            FindAll().ToArray();
        }
        return _lastWriteTime;
    }

    public void Purge(params Guid[] ids)
    {
        // Remove the note from the index and re-write the file without it.
        if (_reindex)
        {
            FindAll().ToArray();
        }
        foreach (var id in ids)
        {
            _index.Remove(id);
        }
        if (!_index.Any())
        {
            _fileSystem.File.Delete(_fileName);
            return;
        }
        string context = string.Empty;
        foreach (var item in _index.Values.OrderBy(s => s.Context).ThenBy(t => t.ID))
        {
            var noteString = Stringify(item, context != item.Context);
            context = item.Context;
            _fileSystem.File.AppendAllText(_fileName, noteString);
        }
    }
}