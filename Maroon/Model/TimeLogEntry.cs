using System;

namespace AssimilationSoftware.Maroon.Model
{
    public class TimeLogEntry
    {
        public Guid ID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Client { get; set; }
        public string Project { get; set; }
        public bool Billable { get; set; }
    }
}
