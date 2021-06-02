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
    public class NoteDiskMapper : IDiskMapper<Note>
    {
        private bool TryParseGuidLine(string line, out Guid? noteId, out Guid? revision, out Guid? parentId, out Guid? prevRevision)
        {
            var id = "([A-Z0-9]{8}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{12})";  // SQL GUID 1
            var lSquare = "(\\[)";   // Any Single Character 1
            var rSquare = "(\\])";   // Any Single Character 2
            var colon = "(:)"; // Any Single Character 3
            var semicolon = "(;)";

            // ID with revision, previous revision, and parent. GUID[GUID;GUID]:GUID
            var r6 = new Regex(id + lSquare + id + semicolon + id + rSquare + colon + id, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var m6 = r6.Match(line);
            if (m6.Success)
            {
                noteId = new Guid(m6.Groups[1].ToString());
                revision = new Guid(m6.Groups[3].ToString());
                prevRevision = new Guid(m6.Groups[5].ToString());
                parentId = new Guid(m6.Groups[7].ToString());
                return true;
            }

            // ID with revision and previous revision. GUID[GUID;GUID]
            var r5 = new Regex(id + lSquare + id + semicolon + id + rSquare, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var m5 = r5.Match(line);
            if (m5.Success)
            {
                noteId = new Guid(m5.Groups[1].ToString());
                revision = new Guid(m5.Groups[3].ToString());
                prevRevision = new Guid(m5.Groups[5].ToString());
                parentId = null;
                return true;
            }

            // Case 1: ID with revision and parent. GUID[GUID]:[GUID]
            var r = new Regex(id + lSquare + id + rSquare + colon + id, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var m = r.Match(line);
            if (m.Success)
            {
                noteId = new Guid(m.Groups[1].ToString());
                revision = Guid.Parse(m.Groups[3].ToString());
                parentId = new Guid(m.Groups[6].ToString());
                prevRevision = null;
                return true;
            }

            // Case 2: ID with revision. GUID[GUID]
            var r2 = new Regex(id + lSquare + id + rSquare, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var m2 = r2.Match(line);
            if (m2.Success)
            {
                noteId = new Guid(m2.Groups[1].ToString());
                revision = Guid.Parse(m2.Groups[3].ToString());
                parentId = null;
                prevRevision = null;
                return true;
            }

            // Backwards-compatible cases: no revision id. GUID
            var r3 = new Regex(id, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var m3 = r3.Match(line);
            if (m3.Success)
            {
                noteId = new Guid(m3.Groups[1].ToString());
                revision = null;
                parentId = null;
                prevRevision = null;
                return true;
            }

            // GUID:GUID
            var r4 = new Regex(id + colon + id, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var m4 = r4.Match(line);
            if (m4.Success)
            {
                noteId = new Guid(m4.Groups[1].ToString());
                revision = null;
                parentId = new Guid(m4.Groups[3].ToString());
                prevRevision = null;
                return true;
            }

            // Failed to parse.
            noteId = null;
            revision = null;
            parentId = null;
            prevRevision = null;
            return false;
        }

        private IEnumerable<Note> LoadAll(string filename)
        {
            var drafts = new List<Note>();
            Note current = null;
            if (File.Exists(filename))
            {
                foreach (var line in File.ReadAllLines(filename))
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

                        if (TryParseGuidLine(line, out var id, out var rev, out var parent, out var prevRevision))
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

                            if (prevRevision.HasValue)
                            {
                                current.PrevRevision = prevRevision;
                            }
                        }
                        else
                        {
                            if (DateTime.TryParse(line, out var stamp))
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
            }
            return drafts;
        }

        public void Save(Note item, string filename)
        {
            var f = Read(filename).ToList();
            f.Add(item);
            Write(f.OrderBy(e => e.Timestamp).ToList(), filename);
        }

        public IEnumerable<Note> Read(params string[] fileNames)
        {
            return fileNames.SelectMany(LoadAll);
        }

        public void Write(IEnumerable<Note> items, string filename)
        {
            var fileContents = new StringBuilder();
            foreach (var d in items)
            {
                fileContents.AppendLine(d.Timestamp.ToString("s"));

                fileContents.AppendLine(d.Text);

                // Tidy up tags before writing.
                d.Tags?.RemoveAll(string.IsNullOrWhiteSpace);
                if (d.Tags?.Count > 0)
                {
                    fileContents.AppendLine(d.TagString);
                }

                fileContents.Append(d.ID.ToString());
                if (d.PrevRevision.HasValue)
                {
                    fileContents.AppendFormat("[{0};{1}]", d.RevisionGuid, d.PrevRevision);
                }
                else
                {
                    fileContents.AppendFormat("[{0}]", d.RevisionGuid);
                }

                if (d.ParentId.HasValue)
                {
                    fileContents.AppendFormat(":{0}", d.ParentId);
                }

                fileContents.AppendLine();
                fileContents.AppendLine();
            }

            if (fileContents.Length == 0 && File.Exists(filename))
            {
                File.Delete(filename);
            }
            else
            {
                File.WriteAllText(filename, fileContents.ToString());
            }
        }

        public void Delete(string filename)
        {
            File.Delete(filename);
        }
    }
}
