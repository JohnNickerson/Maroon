using System;

namespace AssimilationSoftware.Maroon.Commands.Actions
{
    public class SetProjectForActionItem : Command
    {
        public Guid ActionId { get; set; }
        public Guid ProjectId { get; set; }
    }
}