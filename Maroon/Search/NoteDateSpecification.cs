using AssimilationSoftware.Maroon.Notes;
using AssimilationSoftware.Maroon.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.Search
{
    public class NoteDateSpecification : ISearchSpecification<Note>
    {
        private DateTime _date;

        public NoteDateSpecification(DateTime date)
        {
            _date = date.Date;
        }

        public bool IsSatisfiedBy(Note item)
        {
            return item.Timestamp.Date == _date;
        }
    }
}
