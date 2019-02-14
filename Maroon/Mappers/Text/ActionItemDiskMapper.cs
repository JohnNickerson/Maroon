using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.Mappers.Text
{
    public class ActionItemDiskMapper : IMapper<ActionItem>
    {
        protected string Filename;
        protected List<ActionItem> Items;
        private DateTime? _lastModTime;

        public ActionItemDiskMapper(string filename)
        {
            Filename = filename;
            _lastModTime = null;
        }

        public ActionItem Load(Guid id)
        {
            Items = LoadAll().ToList();
            var filtered = from i in Items where i.ID == id select i;
            return filtered.FirstOrDefault();
        }

        public IEnumerable<ActionItem> LoadAll()
        {
            if (File.Exists(Filename))
            {
                // Check item cache. If the file hasn't changed since we last read it in full, just return the Items from memory.
                if (new FileInfo(Filename).LastWriteTime == _lastModTime)
                {
                    return Items;
                }

                _lastModTime = null;
            }
            var lines = (File.Exists(Filename) ? File.ReadAllLines(Filename) : new string[] { });
            var priorityParents = new Dictionary<ActionItem, Guid>();
            var projects = new Dictionary<ActionItem, Guid>();

            var idFoundCount = 0;
            Items = new List<ActionItem>();
            var context = string.Empty;
            var currentItem = new ActionItem { Context = context, Title = "(item out of order)" };
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
                                    idFoundCount++;
                                    break;
                                case "priority-parent":
                                    priorityParents[currentItem] = Guid.Parse(ts[1]);
                                    break;
                                case "project":
                                    projects[currentItem] = Guid.Parse(ts[1]);
                                    break;
                                case "tickle-date":
                                    currentItem.TickleDate = DateTime.Parse(ts[1]);
                                    break;
                                case "upvotes":
                                    currentItem.Upvotes = Int32.Parse(ts[1]);
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
                    currentItem = new ActionItem { Context = context, Title = t.Trim() };
                    Items.Add(currentItem);
                }
            }

            var itemMap = new Dictionary<Guid, ActionItem>();
            foreach (var i in Items)
            {
                itemMap[i.ID] = i;
            }

            // Process references.
            foreach (var t in Items)
            {
                if (projects.ContainsKey(t) && itemMap.ContainsKey(projects[t]))
                {
                    t.Project = itemMap[projects[t]];
                }

                if (priorityParents.ContainsKey(t) && itemMap.ContainsKey(priorityParents[t]))
                {
                    t.Parent = itemMap[priorityParents[t]];
                }
            }

            if (idFoundCount != Items.Count)
            {
                // We assigned some IDs, so rewrite the file. This prevents subsequent reads from assigning new, different IDs.
                SaveAll(Items);
            }
            _lastModTime = new FileInfo(Filename).LastWriteTime;
            return Items;
        }

        public void Save(ActionItem item)
        {
            if (Items == null)
            {
                Items = LoadAll().ToList();
            }
            Items.RemoveAll(i => i.ID == item.ID);
            Items.Add(item);
            SaveAll(Items);
        }

        public void SaveAll(IEnumerable<ActionItem> items)
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
                if (i.Project != null)
                {
                    file.AppendLine($"\t\t#project:{i.Project.ID}");
                }

                if (i.Parent != null)
                {
                    file.AppendLine($"\t\t#priority-parent:{i.Parent.ID}");
                }
            }

            File.WriteAllText(Filename, file.ToString());
        }

        public void Delete(ActionItem item)
        {
            var allItems = LoadAll().ToList();

            allItems.RemoveAll(i => i.ID == item.ID);

            SaveAll(allItems);
        }
    }
}
