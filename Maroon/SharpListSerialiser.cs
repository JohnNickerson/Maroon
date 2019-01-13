using System;
using System.Collections.Generic;
using Polenter.Serialization;
using System.IO;

namespace AssimilationSoftware.Maroon
{
    public class SharpListSerialiser<T>
    {
        public string FileName { get; set; }

        public Func<T, string> GenerateFileName { get; set; }
        private readonly SharpSerializer _serial;

        public SharpListSerialiser(string path)
            : this(path, null)
        {
        }

        public SharpListSerialiser(string path, Func<T, string> generateFileNameFunction)
        {
            FileName = path;
            GenerateFileName = generateFileNameFunction;
            _serial = new SharpSerializer();
        }

        public List<T> Deserialise()
        {
            var result = new List<T>();

            if (Directory.Exists(FileName))
            {
                foreach (string file in Directory.GetFiles(FileName, "*.xml"))
                {
                    try
                    {
                        result.Add((T) _serial.Deserialize(file));
                    }
                    catch
                    {
                        // Ignore. Not one of our files.
                    }
                }
            }

            return result;
        }

        public void Serialise(List<T> data, bool deleteFirst = false)
        {
            if (deleteFirst)
            {
                foreach (var f in Directory.GetFiles(FileName))
                {
                    File.Delete(f);
                }
            }

            foreach (var d in data)
            {
                var commandFileName = Path.Combine(FileName, $"{GenerateFileName(d)}.xml");
                if (deleteFirst || !File.Exists(commandFileName))
                {
                    _serial.Serialize(d, commandFileName);
                }
            }
        }

        /// <summary>
        /// Adds a single item to the collection.
        /// </summary>
        /// <param name="data">The item to add.</param>
        public void Serialise(T data)
        {
            // This is the shortcut way that you can't do with everything in a single file.
            _serial.Serialize(data, Path.Combine(FileName, $"{GenerateFileName(data)}.xml"));
        }
    }
}
