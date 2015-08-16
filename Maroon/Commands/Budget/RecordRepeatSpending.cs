using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.Commands.Budget
{
    public class RecordRepeatSpending : RecordSpending
    {
        public TimeSpan RepeatPeriod { get; set; }
        public DateTime EndsAt { get; set; }
    }
}
