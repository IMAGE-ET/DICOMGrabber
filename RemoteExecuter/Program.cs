using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.IO;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Threading;
using System.Windows.Forms;
using System.Security;

namespace RemoteExecuter
{
    class Program
    {
        private static string remote_computer_name = "RS30012524.REG.SKANE.SE";
        //private static string remote_computer_name = "ECLIPSE12.REG.SKANE.SE";
        private static string process = @"C:\Program Files\DICOMGrabber\DICOMGrabber.exe";
        private static string sharePath = @"\\mtdb001\VA_TRANSFER\Aria export_import\DICOMdump\";
        //private static string username = "dicomdaemon";
        //private static string password = "EAAAAKsJTI5YG2h2Pxm7XFi+4FJTC+1I2VIkPJM1RN4tzFBK";
        //private static string password = "EAAAAOr0lp0SNwNldSl5bcjeB5EAWSiJfnid7v1LA2QT1n4N";
        //private static string password = "EAAAAOKWKt5fOdXUW8ilfdSkbyKDVbUgj/zQgpBpLFcqZXHo";
        //private static string username = "150801";
        //private static string password = "EAAAAHbDcxdgRitIGvg2EC14UsdPDPmISnutkN5dPgMyw0R6";
        //private static string password = "EAAAAL42KMGYWEJ3YF9Eugm4MaopAupIwINeSDfmV4Z9o4CZ";
        private static string key = "lkajdgh";

        [STAThread]
        static void Main(string[] args)
        {
            try {

                Console.WriteLine("Pls key in your Login ID");
                string username = Console.ReadLine();
                Console.WriteLine("Pls key in your Password");
                var password = Crypto.EncryptStringAES(ReadPassword(), key);

                string UIDfile;
                string finalPath = String.Empty;
                // some defensive programming

                // Check input arguments
                if (args.Length != 2)
                {
                    // Open UI dialogues to select files/paths
                    OpenFileDialog uid = new OpenFileDialog();
                    uid.Title = "Select file containing ID-UID pairs";
                    uid.ShowDialog();
                    UIDfile = uid.FileName;

                    FolderBrowserDialog fbd = new FolderBrowserDialog();
                    fbd.Description = "Select the folder where you want your files";
                    fbd.SelectedPath = uid.FileName;
                    if (fbd.ShowDialog() == DialogResult.OK)
                        finalPath = fbd.SelectedPath;
                }
                else
                {
                    // sort input arguments
                    // first input is path to file with UIDs
                    // second input is final path of files
                    UIDfile = args[0];
                    finalPath = args[1];
                }

                // Create dir
                Directory.CreateDirectory(finalPath);

                // Read the UIDfile to string
                string[] UIDdata = File.ReadAllLines(UIDfile);
                // Get UID list from UIDdata
                string[] UIDs = UIDlist.GetUIDs(UIDdata);

                // Build connection
                ConnectionOptions connection = new ConnectionOptions();
                connection.Impersonation = ImpersonationLevel.Impersonate;
                connection.EnablePrivileges = true;
                connection.Authentication = AuthenticationLevel.Packet;
                connection.Username = username;
                connection.SecurePassword = ConvertToSecureString(Crypto.DecryptStringAES(password, key));
                ManagementPath mp = new ManagementPath();
                mp.NamespacePath = @"\root\cimv2";
                mp.Server = remote_computer_name;
                ManagementScope wmiScope = new ManagementScope(mp, connection);
                //ManagementScope wmiScope = new ManagementScope(String.Format("\\\\{0}\\root\\cimv2", remote_computer_name), connection);
                // Connect
                wmiScope.Connect();
            
                // Define process
                ManagementClass wmiProcess = new ManagementClass(wmiScope, new ManagementPath("Win32_Process"), new ObjectGetOptions());
                ManagementBaseObject inParams = wmiProcess.GetMethodParameters("Create");

                // input arguments
                inParams["CommandLine"] = process + " " + String.Join(";", UIDdata).Replace("\t", ","); // + "\"" + String.Join("\n", UIDdata) + "\"";

                // Invoke
                ManagementBaseObject outParams = wmiProcess.InvokeMethod("Create", inParams, null);
    
                //Console.WriteLine("Creation of the process returned: " + outParams["returnValue"]);
                //Console.WriteLine("Process ID: " + outParams["processId"]);

                // Check that the size of sharePath is consistent
                long dirSize = 0;
                while (dirSize != GetDirSize(sharePath))
                {
                    int sleepTime = 15000; // time to sleep (ms)
                    Console.WriteLine(String.Format("Sleeping {0} seconds...", sleepTime / 1000));
                    dirSize = GetDirSize(sharePath);
                    Thread.Sleep(sleepTime); // sleep for 15 secs
                }

                // Create list of file/UID pairs
                List<FileUIDpair> fileUIDpairs = new List<FileUIDpair>();
                // Get list of files
                var fileList = Directory.GetFiles(sharePath, "*.dcm")
                    .Select(fileName => Path.GetFileName(fileName)).ToArray();
                // Add to list
                foreach (string item in fileList)
                    fileUIDpairs.Add(new FileUIDpair(item));

                // loop through file/UID pairs and see if the UID is in UIDdata
                foreach (FileUIDpair fup in fileUIDpairs)
                {
                    if (UIDs.Any(fup.Uid.Contains))
                    {
                        try
                        {
                            File.Move(Path.Combine(sharePath, fup.FileName), Path.Combine(finalPath, fup.FileName));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
                Thread.Sleep(5 * 1000);
            }

        }

        public static string ReadPassword()
        {
            string password = ""; ConsoleKeyInfo info = Console.ReadKey(true); while (info.Key != ConsoleKey.Enter)
            {
                if (info.Key != ConsoleKey.Backspace) { Console.Write("*"); password += info.KeyChar; }
                else if (info.Key == ConsoleKey.Backspace)
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        // remove one character from the list of password characters                          
                        password = password.Substring(0, password.Length - 1);
                        // get the location of the cursor                          
                        int pos = Console.CursorLeft;
                        // move the cursor to the left by one character                          
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                        // replace it with space                          
                        Console.Write(" ");
                        // move the cursor to the left by one character again                          
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                    }
                }
                info = Console.ReadKey(true);
            }
            // add a new line because user pressed enter at the end of their password              
            Console.WriteLine();
            return password;
        } 

        private static long GetDirSize(string dir)
        {
            DirectoryInfo d = new DirectoryInfo(dir);
            return DirSize(d);
        }

        private static long DirSize(DirectoryInfo d)
        {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di);
            }
            return size;
        }

        private static SecureString ConvertToSecureString(string password)
        {
            if (password == null)
                throw new ArgumentNullException("password");

            var securePassword = new SecureString();

            foreach (char c in password)
                securePassword.AppendChar(c);

            securePassword.MakeReadOnly();
            return securePassword;
        }
    }

}
