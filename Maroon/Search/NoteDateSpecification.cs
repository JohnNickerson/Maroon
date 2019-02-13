using System;
using AssimilationSoftware.Maroon.Model;

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
