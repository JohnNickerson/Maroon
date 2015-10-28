using AssimilationSoftware.Maroon.Commands;
using AssimilationSoftware.Maroon.Commands.Notes;
using AssimilationSoftware.Maroon.Search;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AssimilationSoftware.Maroon.Objects
{
    public class Notebook : Aggregate
    {
        private readonly CommandQueue _commandHistory;
        private List<Note> _notes;

        public Notebook(CommandQueue commandhistory)
        {
            _commandHistory = commandhistory;
        }

        private void Rehydrate()
        {
            // Replay the command history to generate the list of objects.
            _commandHistory.Read();
            var items = new Dictionary<Guid, Note>();
            foreach (var c in _commandHistory.Commands)
            {
                if (c is AddNote)
                {
                    AddNote a = (AddNote)c;
                    items[a.ID] = new Note
                    {
                        ID = a.ID,
                        Tags = a.Tags,
                        Text = a.Text,
                        Timestamp = a.Timestamp,
                        Revision = 0
                    };
                }
                else if (c is DeleteNote)
                {
                    var d = (DeleteNote)c;
                    if (items.ContainsKey(d.NoteID) && items[d.NoteID].Revision == d.SourceRevision)
                    {
                        items.Remove(d.NoteID);
                    }
                }
                else if (c is EditNote)
                {
                    var e = (EditNote)c;
                    if (items[e.NoteID].Revision == e.SourceRevision)
                    {
                        items[e.NoteID].Text = e.NewText;
                        items[e.NoteID].Revision = e.SourceRevision + 1;
                    }
                }
                else if (c is MergeNotes)
                {
                    var m = (MergeNotes)c;
                    if (items[m.MergeWinnerId].Revision == m.SourceRevision)
                    {
                        // Update references in other notes (parent IDs).
                        foreach (var i in items.Keys.Where(k => items[k].ParentId == m.MergeLoserId))
                        {
                            items[i].ParentId = m.MergeWinnerId;
                            items[i].Revision++;
                        }
                        // Merge the two notes.
                        items[m.MergeWinnerId].Text = string.Format("{0}\n{1}", items[m.MergeWinnerId].Text, items[m.MergeLoserId].Text);
                        foreach (var t in items[m.MergeLoserId].Tags)
                        {
                            if (!items[m.MergeWinnerId].Tags.Contains(t))
                            {
                                items[m.MergeWinnerId].Tags.Add(t);
                            }
                        }
                        items[m.MergeWinnerId].Revision++;
                        items.Remove(m.MergeLoserId);
                    }
                }
                else if (c is TagNote)
                {
                    var t = (TagNote)c;
                    if (items[t.NoteID].Revision == t.SourceRevision)
                    {
                        items[t.NoteID].Tags = t.NewTags;
                    }
                }
                else if (c is UpVoteNote)
                {
                    var u = (UpVoteNote)c;
                    if (items.ContainsKey(u.ChildNoteID) && items[u.ChildNoteID].Revision == u.SourceRevision)
                    {
                        items[u.ChildNoteID].ParentId = u.ParentNoteId;
                    }
                }
            }
            _notes = items.Values.ToList();
        }

        public ISearchSpecification<Note> SearchSpecification { get; set; }

        public IEnumerable<Note> Notes
        {
            get
            {
                if (SearchSpecification == null)
                {
                    return _notes;
                }
                else
                {
                    return _notes.Where(n => SearchSpecification.IsSatisfiedBy(n));
                }
            }
        }
    }
}
