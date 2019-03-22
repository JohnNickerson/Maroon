using System;

namespace AssimilationSoftware.Maroon.Model
{
    public class TimeLogEntry : ModelObject
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Client { get; set; }
        public string Project { get; set; }
        public bool Billable { get; set; }
        public string Note { get; set; }
        public override object Clone()
        {
            return new TimeLogEntry
            {
                Billable = Billable,
                Client = Client,
                EndTime = EndTime,
                Note = Note,
                Project = Project,
                StartTime = StartTime,
                RevisionGuid = RevisionGuid,
                Revision = Revision,
                ID = ID,
                LastModified = LastModified
            };
        }
    }
}
