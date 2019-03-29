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

        public override string FieldsHeader => "Start,End,Client,Ticket,Notes,Chargeable,ID,Rev";

        public override TimeLogEntry FromTokens(string[] tokens)
        {
            return new TimeLogEntry
            {
                StartTime = DateTime.Parse(tokens[0]),
                EndTime = DateTime.Parse(tokens[1]),
                Client = tokens[2],
                Project = tokens[3],
                Note = tokens[4],
                Billable = bool.Parse(tokens[5]),
                ID = new Guid(tokens[6]),
                Revision = int.Parse(tokens[7]),
                LastModified = DateTime.Parse(tokens[1])
            };
        }

        public override string ToCsv(TimeLogEntry obj)
        {
            return string.Format("{0:s},{1:s},{2},{3},{4},{5},{6},{7}", obj.StartTime, obj.EndTime, obj.Client, obj.Project,
                obj.Note, obj.Billable, obj.ID, obj.Revision);
        }

        public override void SaveAll(IEnumerable<TimeLogEntry> list)
        {
            // Just sort so that the file saves in order.
            base.SaveAll(list.OrderBy(t => t.StartTime));
        }
    }
}
