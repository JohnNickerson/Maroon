using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using AssimilationSoftware.Maroon.Model;
using Polenter.Serialization;

namespace AssimilationSoftware.Maroon.Mappers.Xml
{
    [Obsolete]
    public class SharpListSerialiser<T> where T : ModelObject
    {
        #region Fields
        private readonly SharpSerializer _serial;
        private string _path;
        private IFileSystem _fileSystem;

        #endregion

        #region Constructors
        public SharpListSerialiser(string path, IFileSystem? fileSystem = null) : this(path, "{0}.xml", fileSystem)
        {
        }

        public SharpListSerialiser(string path, string fileNamePattern, IFileSystem? fileSystem = null)
        {
            _fileSystem = fileSystem ?? new FileSystem();
            Path = path;
            FileNamePattern = fileNamePattern;
            _serial = new SharpSerializer();
        }
        #endregion

        #region Methods
        public List<T> Deserialise()
        {
            var result = new List<T>();

            if (_fileSystem.Directory.Exists(Path))
            {
                foreach (string file in _fileSystem.Directory.GetFiles(Path, string.Format(FileNamePattern, "*")))
                {
                    try
                    {
                        result.Add((T)_serial.Deserialize(file));
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
                foreach (var f in _fileSystem.Directory.GetFiles(Path, SearchString))
                {
                    _fileSystem.File.Delete(f);
                }
            }

            foreach (var d in data)
            {
                var fileName = System.IO.Path.Combine(Path, string.Format(FileNamePattern, d.RevisionGuid));
                if (deleteFirst || !_fileSystem.File.Exists(fileName))
                {
                    _serial.Serialize(d, fileName);
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
            _serial.Serialize(data, System.IO.Path.Combine(Path, string.Format(FileNamePattern, data.RevisionGuid)));
        }

        public void Delete(T data)
        {
            var fileName = System.IO.Path.Combine(Path, string.Format(FileNamePattern, data.RevisionGuid));
            _fileSystem.File.Delete(fileName);
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
