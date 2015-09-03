using AssimilationSoftware.Maroon.Objects;

namespace AssimilationSoftware.Maroon.Events.Notes
{
    public class NoteCreated : Event
    {
        public Note NewNote { get; set; }
    }
}
