using System;
using System.Collections.Generic;
using System.Linq;
using AssimilationSoftware.Maroon.Model;

namespace AssimilationSoftware.Maroon.Mappers.Csv
{
    public class TimeLogCsvMapper : CsvDiskMapper<TimeLogEntry>
    {
        public TimeLogCsvMapper(string filename) : base(filename)
        {
        }

        public override string FieldsHeader => "Start,End,Client,Ticket,Notes,Chargeable,ID,Rev,Prev,Hash,Deleted";

        public override TimeLogEntry FromTokens(string[] tokens)
        {
            return new TimeLogEntry
            {
                StartTime = DateTime.Parse(tokens[0]),
                EndTime = DateTime.Parse(tokens[1]),
                LastModified = DateTime.Parse(tokens[1]),
                Client = tokens[2],
                Project = tokens[3],
                Note = tokens[4],
                Billable = bool.Parse(tokens[5]),
                ID = new Guid(tokens[6]),
                RevisionGuid = Guid.Parse(tokens[7]),
                PrevRevision = string.IsNullOrEmpty(tokens[8]) ? (Guid?) null : Guid.Parse(tokens[8]),
                ImportHash = tokens.Length>8?tokens[9]:null,
                IsDeleted = bool.Parse(tokens[10])
            };
        }

        public override string ToCsv(TimeLogEntry obj)
        {
            return
                $"{obj.StartTime:s},{obj.EndTime:s},{obj.Client},{obj.Project},{obj.Note},{obj.Billable},{obj.ID},{obj.RevisionGuid},{obj.PrevRevision},{obj.ImportHash},{obj.IsDeleted}";
        }

        public override void SaveAll(IEnumerable<TimeLogEntry> list)
        {
            // Just sort so that the file saves in order.
            base.SaveAll(list.OrderBy(t => t.StartTime));
        }
    }
}
