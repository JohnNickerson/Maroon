using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Model;
using LiteDB;

namespace AssimilationSoftware.Maroon.Mappers.LiteDb
{
    public class NoteLiteDbMapper : IMapper<Note>
    {
        private string _filename;
        private readonly LiteDatabase _liteDb;
        private readonly LiteCollection<Note> _allNotes;

        public NoteLiteDbMapper(string filename)
        {
            _filename = filename;
            _liteDb = new LiteDatabase(filename);
            _allNotes = _liteDb.GetCollection<Note>("notes");
            _allNotes.EnsureIndex(n => n.ID);
        }

        ~NoteLiteDbMapper()
        {
            _liteDb.Dispose();
        }

        public Note Load(Guid id)
        {
            return _allNotes.FindOne(n => n.ID == id);
        }

        public IEnumerable<Note> LoadAll()
        {
            return _allNotes.FindAll();
        }

        public void Save(Note item)
        {
            if (_allNotes.Exists(n => n.ID == item.ID))
            {
                _allNotes.Update(item);
            }
            else
            {
                _allNotes.Insert(item);
            }
        }

        public void SaveAll(IEnumerable<Note> items)
        {
            foreach (var n in items)
            {
                // To insert or update as required.
                Save(n);
            }
        }

        public void Delete(Note item)
        {
            _allNotes.Delete(n => n.ID == item.ID);
        }
    }
}
