using System.Collections.Generic;
using System.IO;
using AssimilationSoftware.Maroon.Model;
using Polenter.Serialization;

namespace AssimilationSoftware.Maroon.Mappers.Xml
{
    public class OriginXmlSerialiser<T> where T : ModelObject
    {
        #region Fields
        private readonly SharpSerializer _serial;
        private string _path;
        private readonly string _thisMachineName;

        #endregion

        #region Constructors

        public OriginXmlSerialiser(string path, string thisMachineName, string fileNamePattern = "{0}.xml")
        {
            _thisMachineName = thisMachineName;
            Path = path;
            FileNamePattern = fileNamePattern;
            _serial = new SharpSerializer();
        }
        #endregion

        #region Methods
        public List<T> Deserialise()
        {
            var result = new List<T>();

            if (Directory.Exists(Path))
            {
                // Load data from each "updates-{machine}.xml" file.
                foreach (string file in Directory.GetFiles(Path, string.Format(FileNamePattern, "*")))
                {
                    try
                    {
                        result.AddRange((List<T>)_serial.Deserialize(file));
                    }
                    catch
                    {
                        // Ignore. Not one of our files.
                    }
                }
            }

            return result;
        }

        public void Serialise(List<T> data)
        {
            var fileName = System.IO.Path.Combine(Path, string.Format(FileNamePattern, _thisMachineName));
            _serial.Serialize(data, fileName);
        }

        /// <summary>
        /// Adds a single item to the collection.
        /// </summary>
        /// <param name="datum">The item to add.</param>
        public void Serialise(T datum)
        {
            var fileName = System.IO.Path.Combine(Path, string.Format(FileNamePattern, _thisMachineName));
            var allData = (List<T>) _serial.Deserialize(fileName);
            allData.Add(datum);
            _serial.Serialize(allData, fileName);
        }

        /// <summary>
        /// Deletes a change record from the collection. This modifies the update history. It is NOT the way to mark an item as deleted.
        /// </summary>
        /// <param name="datum"></param>
        public void Delete(T datum)
        {
            var fileName = System.IO.Path.Combine(Path, string.Format(FileNamePattern, _thisMachineName));
            var allData = (List<T>)_serial.Deserialize(fileName);
            allData.Remove(datum);
            _serial.Serialize(allData, fileName);
        }

        /// <summary>
        /// Removes all history records. Usually part of a commit, but could be a full rollback of all persisted changes.
        /// </summary>
        public void Clear()
        {
            if (Directory.Exists(Path))
            {
                foreach (string file in Directory.GetFiles(Path, string.Format(FileNamePattern, "*")))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        // Ignore. Not one of our files.
                    }
                }
            }
        }
        #endregion

        #region Properties

        public string Path
        {
            get => string.IsNullOrEmpty(_path) ? "." : _path;
            set => _path = value;
        }

        public string FileNamePattern { get; set; }

        public string SearchString => string.Format(FileNamePattern, "*");
        #endregion
    }
}
