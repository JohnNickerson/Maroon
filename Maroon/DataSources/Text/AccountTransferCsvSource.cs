using System.IO.Abstractions;
using System.Text;
using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Mappers.Csv;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.DataSources.Text;

public class AccountTransferCsvSource : IDataSource<AccountTransfer>
{
    private const string ColumnHeaders = "Date,FromAccount,ToAccount,Description,Category,Amount,LastModified,ID,IsDeleted,RevisionGuid,PrevRevision,MergeRevision,ImportHash";
    private IFileSystem _fileSystem;
    private string _fileName;
    private Dictionary<Guid, AccountTransfer> _index;
    private bool _reindex;
    private DateTime _lastWriteTime;

    public AccountTransferCsvSource(string fileName, IFileSystem? fileSystem = null)
    {
        this._fileSystem = fileSystem ?? new FileSystem();
        this._fileName = Environment.ExpandEnvironmentVariables(fileName);
        _index = [];
        _reindex = true;
    }

    private string Stringify(AccountTransfer obj)
    {
        return $"{obj.Date:yyyy-MM-dd},{obj.FromAccount},{obj.ToAccount},{obj.Description},{obj.Category},{obj.Amount},{obj.LastModified:O},{obj.ID},{obj.IsDeleted},{obj.RevisionGuid},{obj.PrevRevision},{obj.MergeRevision},{obj.ImportHash}";
    }

    public AccountTransfer Insert(AccountTransfer item)
    {
        item.LastModified = DateTime.Now;
        var csvLine = Stringify(item);
        EnsureFileExists();
        _fileSystem.File.AppendAllText(_fileName, csvLine + Environment.NewLine);
        _index[item.RevisionGuid] = item;
        return item;
    }

    private void EnsureFileExists()
    {
        if (!_fileSystem.File.Exists(_fileName))
        {
            _fileSystem.File.WriteAllText(_fileName, ColumnHeaders + Environment.NewLine);
        }
    }

    public IEnumerable<AccountTransfer> FindAll()
    {
        if (!_reindex)
        {
            foreach (var item in _index.Values)
            {
                yield return item;
            }
            yield break;
        }

        _index = [];
        _lastWriteTime = DateTime.MinValue;
        if (!_fileSystem.File.Exists(_fileName))
        {
            yield break;
        }
        foreach (var line in _fileSystem.File.ReadAllLines(_fileName).Skip(1))
        {
            var parts = line.Tokenise();
            if (parts.Count < 10)
            {
                continue; // Skip malformed lines
            }
            var transfer = new AccountTransfer
            {
                Date = DateTime.Parse(parts[0]),
                FromAccount = parts[1],
                ToAccount = parts[2],
                Description = parts[3],
                Category = parts[4],
                Amount = decimal.Parse(parts[5]),
                LastModified = DateTime.Parse(parts[6]),
                ID = Guid.Parse(parts[7]),
                IsDeleted = bool.Parse(parts[8]),
                RevisionGuid = Guid.Parse(parts[9]),
                PrevRevision = Guid.TryParse(parts[10], out var prevRev) ? prevRev : (Guid?)null,
                MergeRevision = Guid.TryParse(parts[11], out var mergeRev) ? mergeRev : (Guid?)null,
                ImportHash = parts[12]
            };
            _index[transfer.RevisionGuid] = transfer;
            if (_lastWriteTime < transfer.LastModified)
            {
                _lastWriteTime = transfer.LastModified;
            }
            yield return transfer;
        }
        _reindex = false;
    }

    public AccountTransfer? FindRevision(Guid id)
    {
        if (_reindex)
        {
            FindAll().ToList(); // Ensure index is populated
        }
        _index.TryGetValue(id, out var transfer);
        return transfer;
    }

    public void Purge(params Guid[] ids)
    {
        if (_reindex)
        {
            FindAll().ToList(); // Ensure index is populated
        }
        foreach (var guid in ids)
        {
            _index.Remove(guid);
        }
        if (!_index.Any())
        {
            _fileSystem.File.Delete(_fileName);
        }
        else
        {
            // Rewrite the file without the purged item
            var lines = new List<string>
            {
                ColumnHeaders
            };
            lines.AddRange(_index.Values.Select(Stringify));
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