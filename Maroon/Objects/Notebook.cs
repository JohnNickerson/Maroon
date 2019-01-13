using AssimilationSoftware.Maroon.Commands;
using AssimilationSoftware.Maroon.Commands.Notes;
using AssimilationSoftware.Maroon.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using AssimilationSoftware.Maroon.Repositories;

namespace AssimilationSoftware.Maroon.Objects
{
    public class Notebook : Aggregate
    {
        private readonly DiskRepository<Note> _notes;

        public Notebook(CommandQueue commandhistory, DiskRepository<Note> notes)
        {
            CommandHistory = commandhistory;
            _notes = notes;
        }

        public override void Rehydrate()
        {
            // Replay the command history to generate the list of objects.
            CommandHistory.Read();
            _notes.FindAll();
            foreach (var c in CommandHistory.Commands)
            {
                if (c is AddNote)
                {
                    AddNote a = (AddNote)c;
                    _notes.Create(new Note
                    {
                        ID = a.ID,
                        Tags = a.Tags,
                        Text = a.Text,
                        Timestamp = a.Timestamp,
                        Revision = 0
                    });
                }
                else if (c is DeleteNote)
                {
                    var d = (DeleteNote)c;
                    var n = _notes.Find(d.NoteID);
                    if (n != null && n.Revision == d.SourceRevision)
                    {
                        _notes.Delete(_notes.Find(d.NoteID));
                    }
                }
                else if (c is EditNote)
                {
                    var e = (EditNote) c;
                    var n = _notes.Find(e.NoteID);
                    if (n != null && n.Revision == e.SourceRevision)
                    {
                        n.Text = e.NewText;
                        n.Revision++;
                        _notes.Update(n);
                    }
                }
                else if (c is MergeNotes)
                {
                    var m = (MergeNotes)c;
                    var w = _notes.Find(m.MergeWinnerId);
                    var l = _notes.Find(m.MergeLoserId);
                    if (w.Revision == m.SourceRevision)
                    {
                        // Update references in other notes (parent IDs).
                        foreach (var i in _notes.FindAll().Where(k => k.ParentId == m.MergeLoserId))
                        {
                            i.ParentId = m.MergeWinnerId;
                            i.Revision++;
                            _notes.Update(i);
                        }
                        // Merge the two notes.
                        w.Text = string.Format("{0}\n{1}", w.Text, l.Text);
                        w.Tags = w.Tags.Union(l.Tags).ToList();
                        w.Revision++;
                        _notes.Update(w);
                        _notes.Delete(l);
                    }
                }
                else if (c is TagNote)
                {
                    var t = (TagNote)c;
                    var n = _notes.Find(t.NoteID);
                    if (n.Revision == t.SourceRevision)
                    {
                        n.Tags = t.NewTags;
                        _notes.Update(n);
                    }
                }
                else if (c is UpVoteNote)
                {
                    var u = (UpVoteNote)c;
                    var n = _notes.Find(u.ChildNoteID);
                    var p = _notes.Find(u.ParentNoteId);
                    if (n != null && p != null && n.Revision == u.SourceRevision)
                    {
                        n.ParentId = u.ParentNoteId;
                        _notes.Update(n);
                    }
                }
            }
        }

        public ISearchSpecification<Note> SearchSpecification { get; set; }

        public IEnumerable<Note> Notes
        {
            get
            {
                if (SearchSpecification == null)
                {
                    return _notes.Items;
                }
                else
                {
                    return _notes.Items.Where(n => SearchSpecification.IsSatisfiedBy(n));
                }
            }
        }
    }
}
