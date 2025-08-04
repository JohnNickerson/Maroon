using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.Mappers.Text
{
    public class ActionItemTextMapper : IDiskMapper<ActionItem>
    {
        private IFileSystem _fileSystem;

        public ActionItemTextMapper(IFileSystem? fileSystem = null)
        {
            _fileSystem = fileSystem ?? new FileSystem();
        }

        private IEnumerable<ActionItem> LoadAll(string filename)
        {
            var lines = (_fileSystem.File.Exists(filename) ? _fileSystem.File.ReadAllLines(filename) : new string[] { });

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
                    yield return (currentItem);
                }
            }
        }

        public IEnumerable<ActionItem> Read(params string[] fileNames)
        {
            return fileNames.SelectMany(LoadAll);
        }

        public void Write(IEnumerable<ActionItem> items, string filename)
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

            _fileSystem.File.WriteAllText(filename, file.ToString());
        }

        public void Delete(string filename)
        {
            _fileSystem.File.Delete(filename);
        }
    }
}
