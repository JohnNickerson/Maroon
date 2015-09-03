using AssimilationSoftware.Maroon.Objects;

namespace AssimilationSoftware.Maroon.Events.Notes
{
    public class NoteUpdated : Event
    {
        public Note UpdatedNote { get; set; }
    }
}
