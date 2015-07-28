using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Polenter.Serialization;
using System.IO;
using AssimilationSoftware.Maroon.Notes;

namespace AssimilationSoftware.Maroon
{
    public class SharpListSerialiser<T>
    {
        public string FileName { get; set; }
        public bool SingleFile { get; set; }
        public Func<T, string> GenerateFileName { get; set; }
        private SharpSerializer _serial;

        public SharpListSerialiser(string path)
            : this(path, true, null)
        {
        }

        public SharpListSerialiser(string path, Func<T, string> generateFileNameFunction)
            : this(path, false, generateFileNameFunction)
        {
        }

        public SharpListSerialiser(string path, bool singlefile, Func<T, string> generateFileNameFunction )
        {
            FileName = path;
            SingleFile = singlefile;
            GenerateFileName = generateFileNameFunction;
            _serial = new SharpSerializer();
        }

        public List<T> Deserialise()
        {
            if (SingleFile)
            {
                return (List<T>) _serial.Deserialize(FileName);
            }
            else
            {
                var result = new List<T>();

                foreach (string file in Directory.GetFiles(FileName, "*.xml"))
                {
                    try
                    {
                        result.Add((T) _serial.Deserialize(file));
                    }
                    catch (InvalidCastException)
                    {
                        // Ignore. Not one of our files.
                    }
                }

                return result;
            }
        }

        public void Serialise(List<T> data, bool deleteFirst = false)
        {
            if (SingleFile)
            {
                if (deleteFirst)
                {
                    File.Delete(FileName);
                }
                _serial.Serialize(data, FileName);
            }
            else
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
                    _serial.Serialize(d, Path.Combine(FileName, string.Format("{0}.xml", GenerateFileName(d))));
                }
            }
        }

        /// <summary>
        /// Adds a single item to the collection.
        /// </summary>
        /// <param name="data">The item to add.</param>
        public void Serialise(T data)
        {
            if (SingleFile)
            {
                // Read, add, write.
                var list = Deserialise();
                list.Add(data);
                Serialise(list, false);
            }
            else
            {
                // This is the shortcut way that you can't do with everything in a single file.
                _serial.Serialize(data, Path.Combine(FileName, string.Format("{0}.xml", GenerateFileName(data))));
            }
        }
    }
}
