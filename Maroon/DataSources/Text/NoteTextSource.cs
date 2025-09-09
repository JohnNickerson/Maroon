using System.IO.Abstractions;
using System.Text;
using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.DataSources.Text;

public class NoteTextSource : IDataSource<Note>
{
    private IFileSystem _fileSystem;
    private string _fileName;
    private Dictionary<Guid, Note> _index;
    private bool _reindex;
    private DateTime _lastWriteTime;

    public NoteTextSource(string fileName, IFileSystem? fileSystem = null)
    {
        this._fileSystem = fileSystem ?? new FileSystem();
        this._fileName = Environment.ExpandEnvironmentVariables(fileName);
        _index = [];
        _reindex = true;
    }

    private string Stringify(Note item)
    {
        var result = new StringBuilder();
        result.AppendLine(item.Timestamp.ToString("s"));
        result.AppendLine(item.Text);
        item.Tags?.RemoveAll(string.IsNullOrWhiteSpace);
        if (item.Tags?.Count > 0)
        {
            result.AppendLine(item.TagString);
        }
        result.AppendLine(item.ID.ToString());
        // Relationship IDs: [revision;previous;merge]:parent
        result.Append($"[{item.RevisionGuid}");
        if (item.PrevRevision.HasValue)
        {
            result.Append($";{item.PrevRevision}");
        }
        if (item.MergeRevision.HasValue)
        {
            result.Append($";{item.MergeRevision}");
        }
        result.Append("]");
        if (item.ParentId.HasValue)
        {
            result.Append($":{item.ParentId}");
        }
        result.AppendLine();
        result.AppendLine();
        return result.ToString();
    }

    public Note Insert(Note item)
    {
        item.LastModified = DateTime.Now;
        var noteString = Stringify(item);
        _fileSystem.File.AppendAllText(_fileName, noteString);
        _index[item.RevisionGuid] = item;
        return item;
    }

    public IEnumerable<Note> FindAll()
    {
        if (!_reindex)
        {
            foreach (var item in _index.Values)
            {
                yield return item;
            }
        }
        else
        {
            _index = [];
            _lastWriteTime = DateTime.MinValue;
            // Read every revision from the file.
            Note? current = null;
            if (_fileSystem.File.Exists(_fileName))
            {
                foreach (var line in _fileSystem.File.ReadAllLines(_fileName))
                {
                    if (line.Trim().Length == 0)
                    {
                        if (current != null)
                        {
                            _index[current.RevisionGuid] = current;
                            yield return current;
                            current = null;
                        }
                    }
                    else
                    {
                        if (current == null)
                        {
                            current = new();
                        }

                        if (TryParseGuidLine(line, out var id, out var rev, out var prev, out var merge, out var parent))
                        {
                            if (id.HasValue)
                            {
                                current.ID = id.Value;
                            }
                            if (rev.HasValue)
                            {
                                current.RevisionGuid = rev.Value;
                            }
                            if (prev.HasValue)
                            {
                                current.PrevRevision = prev;
                            }
                            if (merge.HasValue)
                            {
                                current.MergeRevision = merge;
                            }
                            if (parent.HasValue)
                            {
                                current.ParentId = parent;
                            }
                        }
                        else
                        {
                            if (DateTime.TryParse(line, out var stamp))
                            {
                                current.Timestamp = stamp;
                                current.LastModified = stamp;
                                if (current.LastModified > _lastWriteTime)
                                {
                                    _lastWriteTime = current.LastModified;
                                }
                            }
                            else if (line.Trim().Split(' ').All(t => t.StartsWith('#')))
                            {
                                // Tags
                                current.TagString = line; // Encapsulated parsing.
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(current.Text))
                                {
                                    current.Text += Environment.NewLine;
                                }
                                current.Text += line;
                            }
                        }
                    }
                }
            }
            if (current != null)
            {
                _index[current.RevisionGuid] = current;
                yield return current;
            }
        }
        _reindex = false;
    }

    public bool TryParseGuidLine(string line, out Guid? id, out Guid? rev, out Guid? prev, out Guid? merge, out Guid? parent)
    {
        bool hasBrackets = line.Contains("[") && line.Contains("]");
        line = line.Replace("[", "").Replace("]", "");
        var bits = line.Split(';', ':');
        var semicolonCount = line.Count(c => c == ';');
        var colonCount = line.Count(c => c == ':');
        var onlyGuids = bits.Select(b => Guid.TryParse(b, out _)).Aggregate((a, b) => a & b);
        id = null;
        rev = null;
        prev = null;
        merge = null;
        parent = null;

        if (onlyGuids)
        {
            if (bits.Length == 1 && !hasBrackets && Guid.TryParse(bits[0], out var singleGuid))
            {
                id = singleGuid;
                rev = null;
                prev = null;
                merge = null;
                parent = null;
                return true;
            }
            rev = Guid.Parse(bits[0]);
            // previous revision might not exist
            if (semicolonCount >= 1)
            {
                prev = Guid.Parse(bits[1]);
            }
            // merge revision might not exist, but can't exist without prev.
            if (semicolonCount == 2)
            {
                merge = Guid.Parse(bits[2]);
            }
            // parent ID might not exist, and can exist without the other two
            if (colonCount == 1)
            {
                parent = Guid.Parse(bits[bits.Length - 1]);
            }
        }
        return onlyGuids;
    }

    public Note? FindRevision(Guid id)
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
        foreach (var item in _index.Values)
        {
            var noteString = Stringify(item);
            _fileSystem.File.AppendAllText(_fileName, noteString);
        }
    }
}