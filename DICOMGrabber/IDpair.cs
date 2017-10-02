using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICOMGrabber
{
    class IDpair
    {
        private string id, uid;

        public IDpair(string text)
        {
            // split text to id and uid
            var fields = text.Split(',').ToList();
            fields.RemoveAll(str => String.IsNullOrWhiteSpace(str));
            this.id = fields[0].Trim();
            this.uid = fields[1].Trim();
        }

        public string Id
        {
            get { return this.id; }
        }

        public string Uid
        {
            get { return this.uid; }
        }
    }
}
