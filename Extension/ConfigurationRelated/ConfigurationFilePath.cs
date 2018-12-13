using Main.Other;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.ConfigurationRelated
{
    internal sealed class ConfigurationFilePath
    {
        private readonly FileInfo _fileInfo;

        public string FilePath
        {
            get;
        }

        public string FileName
        {
            get
            {
                return
                    _fileInfo.Name;
            }
        }

        public string FolderPath
        {
            get
            {
                return
                    _fileInfo.Directory.FullName;
            }
        }

        public bool IsFileExists
        {
            get
            {
                return
                    File.Exists(FilePath);
            }
        }

        public ConfigurationFilePath(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            FilePath = filePath.GetFullPathToFile();
            _fileInfo = new FileInfo(FilePath);
        }
    }
}
