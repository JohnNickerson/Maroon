using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.Mappers.Text
{
    public class NoteDiskMapper : IMapper<Note>
    {
        protected string Filename;

        public NoteDiskMapper(string filename)
        {
            Filename = filename;
        }

        /// <summary>
        /// Reads a collection of blog drafts from a file.
        /// </summary>
        /// <returns>A list of blog drafts from the file, in order.</returns>
        public IEnumerable<Note> LoadAll()
        {
            var drafts = new List<Note>();
            Note current = null;
            DateTime stamp;
            if (File.Exists(Filename))
            {
                foreach (var line in File.ReadAllLines(Filename))
                {
                    if (line.Trim().Length == 0)
                    {
                        if (current != null)
                        {
                            drafts.Add(current);
                            current = null;
                        }
                    }
                    else
                    {
                        if (current == null)
                        {
                            current = new Note();
                        }

                        if (TryParseGuidLine(line, out var id, out var rev, out var parent))
                        {
                            if (id.HasValue)
                            {
                                current.ID = id.Value;
                            }

                            if (rev.HasValue)
                            {
                                current.RevisionGuid = rev.Value;
                            }

                            if (parent.HasValue)
                            {
                                current.ParentId = parent.Value;
                            }
                        }
                        else if (DateTime.TryParse(line, out stamp))
                        {
                            current.Timestamp = stamp;
                        }
                        else if (line.Trim().Split(' ').All(t => t.StartsWith("#")))
                        {
                            // Tags.
                            current.TagString = line;
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
            return drafts;
        }

        private bool TryParseGuidLine(string line, out Guid? noteId, out Guid? revision, out Guid? parentId)
        {
            var re1 = "([A-Z0-9]{8}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{12})";  // SQL GUID 1
            var re2 = "(\\[)";   // Any Single Character 1
            var re4 = "(\\])";   // Any Single Character 2
            var re5 = "(:)"; // Any Single Character 3
            var re6 = "([A-Z0-9]{8}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{12})";  // SQL GUID 2

            // Case 1: ID with revision.
            var r = new Regex(re1 + re2 + re6 + re4 + re5 + re6, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var m = r.Match(line);
            if (m.Success)
            {
                noteId = new Guid(m.Groups[1].ToString());
                revision = Guid.Parse(m.Groups[3].ToString());
                parentId = new Guid(m.Groups[6].ToString());
                return true;
            }

            // Case 2: ID with revision and parent.
            var r2 = new Regex(re1 + re2 + re6 + re4, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var m2 = r2.Match(line);
            if (m2.Success)
            {
                noteId = new Guid(m2.Groups[1].ToString());
                revision = Guid.Parse(m2.Groups[3].ToString());
                parentId = null;
                return true;
            }

            // Backwards-compatible cases: no revision number.
            var r3 = new Regex(re1, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var m3 = r3.Match(line);
            if (m3.Success)
            {
                noteId = new Guid(m3.Groups[1].ToString());
                revision = null;
                parentId = null;
                return true;
            }

            var r4 = new Regex(re1 + re5 + re6, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var m4 = r4.Match(line);
            if (m4.Success)
            {
                noteId = new Guid(m4.Groups[1].ToString());
                revision = null;
                parentId = new Guid(m4.Groups[3].ToString());
                return true;
            }

            // Failed to parse.
            noteId = null;
            revision = null;
            parentId = null;
            return false;
        }

        public Note Load(Guid id)
        {
            var items = LoadAll();
            return items.FirstOrDefault(i => i.ID == id);
        }

        public void Save(Note item)
        {
            // TODO: Optimise.
            var f = LoadAll().ToList();
            f.Add(item);
            SaveAll(f.OrderBy(e => e.Timestamp).ToList());
        }

        public void SaveAll(IEnumerable<Note> items)
        {
            var filecontents = new StringBuilder();
            foreach (var d in items)
            {
                filecontents.AppendLine(d.Timestamp.ToString("s"));

                filecontents.AppendLine(d.Text);

                // Tidy up tags before writing.
                d.Tags.RemoveAll(string.IsNullOrWhiteSpace);
                if (d.Tags.Count > 0)
                {
                    filecontents.AppendLine(d.TagString);
                }

                if (d.ParentId != null)
                {
                    filecontents.AppendLine($"{d.ID}[{d.RevisionGuid}]:{d.ParentId}");
                }
                else
                {
                    filecontents.AppendLine($"{d.ID}[{d.RevisionGuid}]");
                }
                filecontents.AppendLine();
            }

            File.WriteAllText(Filename, filecontents.ToString());
        }

        public void Delete(Note item)
        {
            var allitems = LoadAll().ToList();
            allitems.RemoveAll(i => i.ID == item.ID);
            SaveAll(allitems);
        }
    }
}
