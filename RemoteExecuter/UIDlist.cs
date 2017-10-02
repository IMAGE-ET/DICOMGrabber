using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteExecuter
{
    class UIDlist
    {
        public static string[] GetUIDs(string[] UIDdata)
        {
            List<string> UIDs = new List<string>();
            foreach (string data in UIDdata)
                UIDs.Add(data.Split('\t')[1]);

            return UIDs.ToArray();
        }
    }
}
