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

        internal TimeLogEntry With(Guid? RevisionGuid, DateTime? LastModified, Guid? PrevRevision, bool? IsDeleted)
        {
            return new TimeLogEntry
            {
                ID = this.ID,
                StartTime = this.StartTime,
                EndTime = this.EndTime,
                Client = this.Client,
                Project = this.Project,
                Billable = this.Billable,
                Note = this.Note,
                RevisionGuid = RevisionGuid ?? this.RevisionGuid,
                LastModified = LastModified ?? this.LastModified,
                PrevRevision = PrevRevision,
                IsDeleted = IsDeleted ?? this.IsDeleted,
                ImportHash = this.ImportHash
            };
        }
    }
}
