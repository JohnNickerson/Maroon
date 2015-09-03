using AssimilationSoftware.Maroon.Commands;
using AssimilationSoftware.Maroon.Commands.Actions;
using AssimilationSoftware.Maroon.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.Objects
{
    public class ActionList : Aggregate
    {
        private CommandQueue _commandHistory;
        private List<ActionItem> _actions;

        public ActionList(CommandQueue commands)
        {
            _commandHistory = commands;
            Rehydrate();
        }

        private void Rehydrate()
        {
            _commandHistory.Read();
            var items = new Dictionary<Guid, ActionItem>();
            foreach (var c in _commandHistory.Commands)
            {
                if (c is AddActionItem)
                {
                    var a = (AddActionItem)c;
                    items.Add(a.ID, new ActionItem {
                        Context = a.Context,
                        Notes = new List<string>() { a.Note },
                        ID = a.ID,
                        Status = "Active",
                        Tags = a.Tags ?? new Dictionary<string, string>(),
                        Title = a.Title,
                        Revision = 0
                    });
                }
                else if (c is CommentOnActionItem)
                {
                    var com = (CommentOnActionItem)c;
                    if (items.ContainsKey(com.ActionId) && items[com.ActionId].Revision == com.SourceRevision)
                    {
                        items[com.ActionId].Notes.Add(com.Comment);
                        items[com.ActionId].Revision++;
                    }
                }
                else if (c is DeferActionItem)
                {
                    var def = (DeferActionItem)c;
                    if (items.ContainsKey(def.ActionId) && items[def.ActionId].Revision == def.SourceRevision)
                    {
                        items[def.ActionId].Status = "Deferred";
                        items[def.ActionId].TickleDate = def.TickleDate;
                        items[def.ActionId].Revision++;
                    }
                }
                else if (c is DeleteActionItem)
                {
                    var del = (DeleteActionItem)c;
                    if (items.ContainsKey(del.ActionId) && items[del.ActionId].Revision == del.SourceRevision)
                    {
                        items.Remove(del.ActionId);
                    }
                }
                else if (c is DoneActionItem)
                {
                    var dundundun = (DoneActionItem)c;
                    if (items.ContainsKey(dundundun.ActionId) && items[dundundun.ActionId].Revision == dundundun.SourceRevision)
                    {
                        items[dundundun.ActionId].Status = "Done";
                        items[dundundun.ActionId].DoneDate = dundundun.Timestamp;
                        items[dundundun.ActionId].Revision++;
                    }
                }
                else if (c is MoveActionItem)
                {
                    var mv = (MoveActionItem)c;
                    if (items.ContainsKey(mv.ActionId) && items[mv.ActionId].Revision == mv.SourceRevision)
                    {
                        items[mv.ActionId].Context = mv.Context;
                        items[mv.ActionId].Revision++;
                    }
                }
                else if (c is RenameActionItem)
                {
                    var yocomo = (RenameActionItem)c;
                    if (items.ContainsKey(yocomo.ActionId) && items[yocomo.ActionId].Revision == yocomo.SourceRevision)
                    {
                        items[yocomo.ActionId].Title = yocomo.Title;
                        items[yocomo.ActionId].Revision++;
                    }
                }
                else if (c is TagActionItem)
                {
                    var tag = (TagActionItem)c;
                    if (items.ContainsKey(tag.ActionId) && items[tag.ActionId].Revision == tag.SourceRevision)
                    {
                        foreach (var t in tag.Tags.Keys)
                        {
                            if (!string.IsNullOrEmpty(tag.Tags[t]))
                            {
                                items[tag.ActionId].Tags[t] = tag.Tags[t];
                            }
                            else if (items[tag.ActionId].Tags.ContainsKey(t))
                            {
                                items[tag.ActionId].Tags.Remove(t);
                            }
                        }
                        items[tag.ActionId].Revision++;
                    }
                }
                else if (c is UndeferActionItem)
                {
                    var uai = (UndeferActionItem)c;
                    if (items.ContainsKey(uai.ActionId) && items[uai.ActionId].Revision == uai.SourceRevision)
                    {
                        items[uai.ActionId].Status = "Active";
                        items[uai.ActionId].Revision++;
                    }
                }
                else if (c is UndoActionItem)
                {
                    var undo = (UndoActionItem)c;
                    if (items.ContainsKey(undo.ActionId) && items[undo.ActionId].Revision == undo.SourceRevision)
                    {
                        items[undo.ActionId].Status = "Active";
                        items[undo.ActionId].DoneDate = null;
                        items[undo.ActionId].Revision++;
                    }
                }
                else if (c is UpVoteActionItem)
                {
                    var up = (UpVoteActionItem)c;
                    if (up.ParentId.HasValue)
                    {
                        if (items.ContainsKey(up.ParentId.Value) && items.ContainsKey(up.ChildId) && items[up.ChildId].Revision == up.SourceRevision)
                        {
                            items[up.ChildId].Parent = items[up.ParentId.Value];
                            items[up.ChildId].Revision++;
                        }
                    }
                    else if (items.ContainsKey(up.ChildId) && items[up.ChildId].Revision == up.SourceRevision)
                    {
                        items[up.ChildId].Parent = null;
                        items[up.ChildId].Revision++;
                    }
                }
                else if (c is MergeActionItems)
                {
                    var merge = (MergeActionItems)c;
                    // Combine the items:
                    // - Make a note about the merge.
                    items[merge.ParentId].Notes.Add(string.Format("Merged with {0} on {1}", merge.ChildId, merge.Timestamp));
                    // - Combine tags.
                    foreach (var tag in items[merge.ChildId].Tags.Keys)
                    {
                        if (items[merge.ParentId].Tags.ContainsKey(tag))
                        {
                            // Parent already has this key.
                            if (items[merge.ParentId].Tags[tag].ToLower() != items[merge.ChildId].Tags[tag].ToLower())
                            {
                                // Tag values mismatch. Add a note.
                                items[merge.ParentId].Notes.Add(string.Format("Merge tag value mismatch: {0}:{1}", tag, items[merge.ChildId].Tags[tag]));
                            }
                        }
                        else
                        {
                            // Add to the parent.
                            items[merge.ParentId].Tags[tag] = items[merge.ChildId].Tags[tag];
                        }
                    }
                    // - Combine notes.
                    foreach (var note in items[merge.ChildId].Notes)
                    {
                        items[merge.ParentId].Notes.Add(note);
                    }
                    // - Update inbound references: priority children
                    foreach (var child in items.Where(i => i.Value.Parent == items[merge.ChildId]))
                    {
                        child.Value.Parent = items[merge.ParentId];
                    }
                    // - Update inbound references: project children
                    foreach (var child in items.Where(i => i.Value.Project == items[merge.ChildId]))
                    {
                        child.Value.Project = items[merge.ParentId];
                    }
                    // - Remove the absorbed item.
                    items.Remove(merge.ChildId);
                }
                else if (c is SetProjectForActionItem)
                {
                    var setparent = (SetProjectForActionItem)c;
                    if (items.ContainsKey(setparent.ProjectId) && items.ContainsKey(setparent.ActionId))
                    {
                        if (items[setparent.ActionId].Revision == setparent.SourceRevision)
                        {
                            items[setparent.ActionId].Parent = items[setparent.ProjectId];
                            items[setparent.ActionId].Revision++;
                        }
                    }
                }
            }
            _actions = items.Values.ToList();
        }

        public ISearchSpecification<ActionItem> SearchSpecification { get; set; }

        public IEnumerable<ActionItem> ActionItems
        {
            get
            {
                //Rehydrate();
                if (SearchSpecification == null)
                {
                    return _actions;
                }
                else
                {
                    return _actions.Where(a => SearchSpecification.IsSatisfiedBy(a));
                }
            }
        }
    }
}
