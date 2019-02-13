using System;
using AssimilationSoftware.Maroon.Commands;

namespace AssimilationSoftware.Maroon.Objects
{
    [Obsolete("Use repositories.")]
    public abstract class Aggregate
    {
        public CommandQueue CommandHistory { get; protected set; }
        public abstract void Rehydrate();
    }
}