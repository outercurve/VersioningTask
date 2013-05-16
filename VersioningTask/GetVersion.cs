using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Versioning
{
    public class GetVersion : Task
    {

        private Version _version = new Version("0.0.1.0");
        private FileInfo _versionFile = null;
        [Output]
        public string Version { get { return _version.ToString(4); } }
        [Output]
        public int Major { get { return _version.Major; } }
        [Output]
        public int Minor { get { return _version.Minor; } }
        [Output]
        public int Build { get { return _version.Build; } }
        [Output]
        public int Revision { get { return _version.Revision; } }

        /// <summary>
        /// The file to store the incrementing version in.
        /// </summary>
        [Required]
        public ITaskItem File { get; set; }

        public override bool Execute()
        {
            _versionFile = new FileInfo(this.File.ItemSpec);

            if (_versionFile.Exists)
            {
                using (StreamReader sr = new StreamReader(_versionFile.FullName))
                {
                    _version = System.Version.Parse(sr.ReadLine());
                }
            }

            return true;
        }
    }
}
