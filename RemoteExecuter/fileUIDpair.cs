using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteExecuter
{
    class FileUIDpair
    {
        private string fileName, uid;
        private string[] dicomAfixes = new string[6] { "RP.", "CT.", "MR.", "RS.", "RD." , ".dcm" };

        public FileUIDpair(string fileName)
        {
            this.fileName = fileName;
            this.uid = fileName;
            // remove prefixes
            foreach (string prefix in dicomAfixes)
                this.uid = this.uid.Replace(prefix, String.Empty);

        }

        public string Uid
        {
            get { return this.uid; }
        }

        public string FileName
        {
            get { return this.fileName; }
        }
    }
}
