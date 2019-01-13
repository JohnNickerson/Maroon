using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssimilationSoftware.Maroon.Interfaces;
using AssimilationSoftware.Maroon.Objects;

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
            List<Note> drafts = new List<Note>();
            Note current = null;
            DateTime stamp = DateTime.Now;
            // A dictionary to store parent hashes for later processing.
            var parents = new Dictionary<Note, Guid>();
            if (File.Exists(Filename))
            {
                foreach (string line in File.ReadAllLines(Filename))
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
                        if (IsGuidLine(line))
                        {
                            if (line.Contains(":"))
                            {
                                // Parent relationship.
                                var ids = line.Split(':');
                                current.ID = Guid.Parse(ids[0].Trim());
                                parents[current] = Guid.Parse(ids[1].Trim());
                            }
                            else
                            {
                                // Plain old item ID.
                                current.ID = Guid.Parse(line);
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
                            current.Text += Environment.NewLine;
                            current.Text += line;
                        }
                    }
                }

                var itemMap = new Dictionary<Guid, Note>();
                foreach (var i in drafts)
                {
                    itemMap[i.ID] = i;
                }

                // Process parental relationships.
                foreach (Note n in parents.Keys)
                {
                    if (itemMap.ContainsKey(parents[n]))
                    {
                        n.Parent = itemMap[parents[n]];
                    }
                }
            }
            return drafts;
        }

        private bool IsGuidLine(string line)
        {
            Guid whocares;
            if (Guid.TryParse(line.Trim(), out whocares))
            {
                return true;
            }
            else if (line.Contains(':'))
            {
                foreach (string id in line.Split(':'))
                {
                    if (!Guid.TryParse(id.Trim(), out whocares))
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public Note Load(Guid id)
        {
            // TODO: Optimise?
            var items = LoadAll();
            var matches = from i in items where i.ID == id select i;
            return matches.Any() ? matches.First() : null;
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
            StringBuilder filecontents = new StringBuilder();
            foreach (Note d in items)
            {
                filecontents.AppendLine(d.Timestamp.ToString("h:mm tt d/MM/yyyy"));

                filecontents.AppendLine(d.Text);

                if (d.Tags.Count > 0)
                {
                    filecontents.AppendLine(d.TagString);
                }

                if (d.ParentId != null)
                {
                    filecontents.AppendLine(string.Format("{0}:{1}", d.ID, d.ParentId));
                }
                else
                {
                    filecontents.AppendLine(d.ID.ToString());
                }
                filecontents.AppendLine();
            }

            File.WriteAllText(Filename, filecontents.ToString());
        }
        
        public void Delete(Note item)
        {
            var allitems = LoadAll().ToList();
            var search = (from i in allitems where i.ID == item.ID select i);
            if (search.Count() > 0)
            {
                allitems.Remove(search.First());
            }
            SaveAll(allitems);
        }
    }
}
