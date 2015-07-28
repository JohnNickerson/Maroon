using AssimilationSoftware.Maroon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssimilationSoftware.Maroon.Notes;

namespace AssimilationSoftware.Maroon
{
    public class CommandQueue
    {
        private string _path;
        private SharpListSerialiser<Command> _serialiser;
        private List<Command> _commands;

        public CommandQueue(string path)
        {
            _path = path;
            _serialiser = new SharpListSerialiser<Command>(path, false, (Command d) => d.CommandID.ToString());
            Read();
        }

        public void Add(AddNote cmd)
        {
            _commands.Add(cmd);
        }

        public void Read()
        {
            _commands = _serialiser.Deserialise();
        }

        public void Write()
        {
            _serialiser.Serialise(_commands);
        }

        public IEnumerable<Command> Commands
        {
            get
            {
                return _commands;
            }
        }

        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                Read();
            }
        }
    }
}
