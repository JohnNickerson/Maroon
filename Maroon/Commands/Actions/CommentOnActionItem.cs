using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.Commands.Actions
{
    public class CommentOnActionItem : Command
    {
        public Guid ActionId { get; set; }
        public string Comment { get; set; }
    }
}
