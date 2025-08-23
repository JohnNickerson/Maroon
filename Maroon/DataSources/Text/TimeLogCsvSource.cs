using System.IO.Abstractions;
using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Model;
using AssimilationSoftware.Maroon.Mappers.Csv;

namespace AssimilationSoftware.Maroon.DataSources.Text;

public class TimeLogCsvSource : IDataSource<TimeLogEntry>
{
    private IFileSystem _fileSystem;
    private string _fileName;
    private Dictionary<Guid, TimeLogEntry> _index;
    private bool _reindex;
    private DateTime _lastWriteTime;

    public TimeLogCsvSource(string fileName, IFileSystem? fileSystem = null)
    {
        this._fileSystem = fileSystem ?? new FileSystem();
        this._fileName = Environment.ExpandEnvironmentVariables(fileName);
        _index = [];
        _reindex = true;
    }

    private string Stringify(TimeLogEntry obj)
    {
        return $"{obj.StartTime:O},{obj.EndTime:O},{obj.Client},{obj.Project},{obj.Note},{obj.Billable},{obj.LastModified:O},{obj.IsDeleted},{obj.ID},{obj.RevisionGuid},{obj.PrevRevision}";
    }

    public TimeLogEntry Create(TimeLogEntry item)
    {
        item.RevisionGuid = Guid.NewGuid();
        item.LastModified = DateTime.Now;
        var csvLine = Stringify(item);
        _fileSystem.File.AppendAllText(_fileName, csvLine + Environment.NewLine);
        _index[item.RevisionGuid] = item;
        return item;
    }

    public IEnumerable<TimeLogEntry> FindAll()
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
            _reindex = false;
            yield break;
        }
        _index = [];
        _lastWriteTime = DateTime.MinValue;
        var lines = _fileSystem.File.ReadAllLines(_fileName);
        foreach (var line in lines)
        {
            var parts = line.Tokenise();
            if (parts.Count < 11) continue; // Skip invalid lines
            var entry = new TimeLogEntry
            {
                StartTime = DateTime.Parse(parts[0]),
                EndTime = DateTime.Parse(parts[1]),
                Client = parts[2],
                Project = parts[3],
                Note = parts[4],
                Billable = bool.Parse(parts[5]),
                LastModified = DateTime.Parse(parts[6]),
                IsDeleted = bool.Parse(parts[7]),
                ID = Guid.Parse(parts[8]),
                RevisionGuid = Guid.Parse(parts[9]),
                PrevRevision = String.IsNullOrEmpty(parts[10]) ? null : Guid.Parse(parts[10])
            };
            _index[entry.RevisionGuid] = entry;
            if (_lastWriteTime < entry.LastModified)
            {
                _lastWriteTime = entry.LastModified;
            }
            yield return entry;
        }
        _reindex = false;
    }

    public TimeLogEntry? FindRevision(Guid id)
    {
        if (_reindex)
        {
            FindAll().ToList(); // Ensure index is populated
        }
        _index.TryGetValue(id, out var transfer);
        return transfer;
    }

    public TimeLogEntry Update(TimeLogEntry item)
    {
        var updatedLog = item.With(
            RevisionGuid: Guid.NewGuid(),
            LastModified: DateTime.Now,
            PrevRevision: item.RevisionGuid,
            IsDeleted: false
        );
        var csvLine = Stringify(updatedLog);
        _fileSystem.File.AppendAllText(_fileName, csvLine + Environment.NewLine);
        _index[updatedLog.RevisionGuid] = updatedLog;
        return updatedLog;
    }

    public TimeLogEntry Delete(TimeLogEntry item)
    {
        item.IsDeleted = true;
        item.LastModified = DateTime.Now;
        item.PrevRevision = item.RevisionGuid;
        item.RevisionGuid = Guid.NewGuid();
        var csvLine = Stringify(item);
        _fileSystem.File.AppendAllText(_fileName, csvLine + Environment.NewLine);
        _index[item.RevisionGuid] = item;
        return item;
    }

    public void Purge(params Guid[] ids)
    {
        if (_reindex)
        {
            FindAll().ToList(); // Ensure index is populated
        }
        foreach (var id in ids)
        {
            _index.Remove(id);
        }
        if (!_index.Any())
        {
            _fileSystem.File.Delete(_fileName);
        }
        else
        {
            // Rewrite the file without the purged item
            var lines = _index.Values.Select(Stringify).ToList();
            _fileSystem.File.WriteAllLines(_fileName, lines);
        }
    }

    public DateTime GetLastWriteTime()
    {
        if (_reindex)
        {
            FindAll().ToList(); // Ensure index is populated
        }
        return _lastWriteTime;
    }
}