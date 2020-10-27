using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.Mappers.Text
{
    public class ActionItemTextMapper : IDiskMapper<ActionItem>
    {
        protected List<ActionItem> Items;
        private DateTime? _lastModTime;

        public ActionItemTextMapper()
        {
            _lastModTime = null;
        }

        public ActionItem Load(Guid id, string filename)
        {
            Items = LoadAll(filename).ToList();
            var filtered = from i in Items where i.ID == id select i;
            return filtered.FirstOrDefault();
        }

        public IEnumerable<ActionItem> LoadAll(string filename)
        {
            if (File.Exists(filename))
            {
                // Check item cache. If the file hasn't changed since we last read it in full, just return the Items from memory.
                if (new FileInfo(filename).LastWriteTime == _lastModTime)
                {
                    return Items;
                }
                // File changed since we last saw it. Read it again and note the time later.
                _lastModTime = null;
            }
            var lines = (File.Exists(filename) ? File.ReadAllLines(filename) : new string[] { });

            Items = new List<ActionItem>();
            var context = string.Empty;
            var currentItem = new ActionItem { Context = context, Title = "(item out of order)", Done = false, ID = Guid.NewGuid() };
            foreach (var t in lines)
            {
                if (t.StartsWith("@"))
                {
                    // New context.
                    context = t.Substring(1).ToLower();
                }
                else if (t.Trim().StartsWith("#"))
                {
                    // Tag name.
                    var ts = t.Trim().Split(new[] { ':' }, 2);
                    if (ts.Length > 1)
                    {
                        var tag = ts[0].Replace("#", string.Empty).Trim();
                        // Process "special" tags like parent item IDs.
                        try
                        {
                            switch (tag.ToLower())
                            {
                                case "done-date":
                                    currentItem.DoneDate = DateTime.Parse(ts[1]);
                                    break;
                                case "id":
                                    currentItem.ID = Guid.Parse(ts[1]);
                                    break;
                                case "priority-parent":
                                    currentItem.ParentId = Guid.Parse(ts[1]);
                                    break;
                                case "project":
                                    currentItem.ProjectId = Guid.Parse(ts[1]);
                                    break;
                                case "tickle-date":
                                    currentItem.TickleDate = DateTime.Parse(ts[1]);
                                    break;
                                case "upvotes":
                                    currentItem.Upvotes = Int32.Parse(ts[1]);
                                    break;
                                case "revision":
                                    currentItem.RevisionGuid = Guid.Parse(ts[1]);
                                    break;
                                case "prev-revision":
                                    currentItem.PrevRevision = Guid.Parse(ts[1]);
                                    break;
                                case "import-hash":
                                    currentItem.ImportHash = ts[1];
                                    break;
                                default:
                                    currentItem.Tags[tag] = ts[1].Trim();
                                    break;
                            }
                        }
                        catch
                        {
                            // Guid or DateTime parser error. Just store it as a normal tag.
                            currentItem.Tags[tag] = ts[1].Trim();
                        }
                    }
                    else
                    {
                        // Malformed. Just treat it as a note.
                        currentItem.Notes.Add(t.Trim());
                    }
                }
                else if (t.Trim().StartsWith("-"))
                {
                    // Note.
                    currentItem.Notes.Add(t.Trim().Substring(1).Trim());
                }
                else
                {
                    // New item.
                    currentItem = new ActionItem { Context = context, Title = t.Trim(), ID = Guid.NewGuid() };
                    Items.Add(currentItem);
                }
            }
            // Note the last write time of the file for subsequent reads. If we can avoid reading it again, do that.
            _lastModTime = new FileInfo(filename).LastWriteTime;
            return Items;
        }

        public void Save(ActionItem item, string filename, bool overwrite = false)
        {
            if (Items == null)
            {
                Items = LoadAll(filename).ToList();
            }
            Items.RemoveAll(i => i.ID == item.ID);
            Items.Add(item);
            SaveAll(Items, filename);
        }

        public void SaveAll(IEnumerable<ActionItem> items, string filename, bool overwrite = false)
        {
            var file = new StringBuilder();
            var currentContext = string.Empty;
            foreach (var i in items.OrderBy(s => s.Context).ThenBy(t => t.ID))
            {
                if (currentContext != i.Context)
                {
                    file.AppendLine($@"@{i.Context}");
                    currentContext = i.Context;
                }

                file.AppendLine($"\t{i.Title.Trim()}");
                foreach (var note in i.Notes)
                {
                    file.AppendLine($"\t\t- {note.Trim()}");
                }

                foreach (var key in i.Tags.Keys)
                {
                    file.AppendLine($"\t\t#{key}:{i.Tags[key]}");
                }

                if (i.Upvotes > 0)
                {
                    file.AppendLine($"\t\t#upvotes:{i.Upvotes}");
                }

                if (i.DoneDate.HasValue)
                {
                    file.AppendLine($"\t\t#done-date:{i.DoneDate.Value:yyyy-MM-dd}");
                }

                if (i.TickleDate.HasValue)
                {
                    file.AppendLine($"\t\t#tickle-date:{i.TickleDate.Value:yyyy-MM-dd}");
                }

                file.AppendLine($"\t\t#id:{i.ID}");
                file.AppendLine($"\t\t#revision:{i.RevisionGuid}");
                if (i.PrevRevision.HasValue)
                {
                    file.AppendLine($"\t\t#prev-revision:{i.PrevRevision}");
                }
                if (!string.IsNullOrEmpty(i.ImportHash))
                {
                    file.AppendLine($"\t\t#import-hash:{i.ImportHash}");
                }
                if (i.ProjectId != null)
                {
                    file.AppendLine($"\t\t#project:{i.ProjectId}");
                }

                if (i.ParentId != null)
                {
                    file.AppendLine($"\t\t#priority-parent:{i.ParentId}");
                }
            }

            File.WriteAllText(filename, file.ToString());
        }

        public void Delete(ActionItem item, string filename)
        {
            var allItems = LoadAll(filename).ToList();

            allItems.RemoveAll(i => i.ID == item.ID);

            SaveAll(allItems, filename);
        }
    }
}
