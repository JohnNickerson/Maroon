using System.Collections.Generic;
using System.Linq;

namespace AssimilationSoftware.Maroon.Commands
{
    public class CommandQueue
    {
        private string _path;
        private SharpListSerialiser<Command> _serialiser;
        private List<Command> _commands;

        public CommandQueue(string path)
        {
            _path = path;
            _serialiser = new SharpListSerialiser<Command>(path, d => d.CommandID.ToString());
            _commands = new List<Command>();
        }

        public void Add(Command cmd)
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

        public void Delete(Command cmd)
        {
            _commands.Remove(cmd);
        }

        public IEnumerable<Command> Commands
        {
            get
            {
                return _commands.OrderBy(c => c.Timestamp);
            }
        }

        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                _serialiser.FileName = value;
            }
        }
    }
}
