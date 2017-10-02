using EvilDICOM.Core.Helpers;
using EvilDICOM.Core.Logging;
using EvilDICOM.Network;
using EvilDICOM.Network.Helpers;
using EvilDICOM.Network.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Security.AccessControl;
using System.Security.Principal;
using EvilDICOM.Network.DIMSE.IOD;

namespace DICOMGrabber
{
    class Program
    {
        static Entity daemon = new Entity("ARIADB", "10.0.129.139", 57347);
        static string AEtitle = "DCMGRB";
        static string ipadress = "10.0.129.22";
        static int port = 124;

        [STAThread]
        public static void Main(string[] args)
        {
            List <IDpair> idPairs = new List<IDpair>();
            var IDlist = new List<string>();
            if (args.Length == 1)
                IDlist = args[0].Split(';').ToList();
            else
                throw new Exception("Wrong number of inputs");


            // Split in pairs
            foreach (string line in IDlist)
            { 
                idPairs.Add(new IDpair(line));
            }

            // Create all required entities
            //var me = new Entity("EvilDICOMC", "10.0.129.139", 50401);
            //var me = Entity.CreateLocal("DCMGRBC", 51167);
            var me = Entity.CreateLocal("DCMGRBer", 50400);
            var scu = new DICOMSCU(me);

            //var scp = new DICOMSCP
            Entity reciever = new Entity(AEtitle, ipadress, port);
            DICOMSCP scp = new DICOMSCP(reciever);
            //var scpEntity = Entity.CreateLocal("EvilDICOM", 50400);
            //var scpEntity = new Entity("EvilDICOM", "10.0.129.139", 50400);
            //var scp = new FileWriterSCP(scpEntity, outPath);
            //var scp = new DICOMSCP(scpEntity);
            //scp.SupportedAbstractSyntaxes = AbstractSyntax.ALL_RADIOTHERAPY_STORAGE;
            //scp.ListenForIncomingAssociations(true);
            //var logger = new ConsoleLogger(scp.Logger, ConsoleColor.Red);
            var qb = new QueryBuilder(scu, daemon);
            ushort msgId = 1;

            // Loop over ID pairs and pull files from ARIA OIS to finalPath
            foreach (IDpair idPair in idPairs)
            {
                try
                {
                    //qb.SendImage(new EvilDICOM.Network.DIMSE.IOD.CFindImageIOD() { PatientId = idPair.Id, SOPInstanceUID = idPair.Uid }, "EvilDICOM", ref msgId);
                    CFindImageIOD iod = new CFindImageIOD() { PatientId = idPair.Id, SOPInstanceUID = idPair.Uid };
                    scu.SendCMoveImage(daemon, iod, scp.ApplicationEntity.AeTitle, ref msgId);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + "\n" + idPair.Uid);
                }
            }
        }

        private static void Log(string logFile, string logText)
        {
            using (System.IO.StreamWriter file =
                System.IO.File.AppendText(logFile))
            {
                file.WriteLine(logText);
            }
        }
    }
}
