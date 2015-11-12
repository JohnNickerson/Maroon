using AssimilationSoftware.Maroon.Commands;

namespace AssimilationSoftware.Maroon.Objects
{
    public abstract class Aggregate
    {
        public CommandQueue CommandHistory { get; protected set; }
        public abstract void Rehydrate();
    }
}