using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CloudBackupClient
{

    static class Program
    {
        private const string DEFAULT_SERVER_IP = "127.0.0.1";
        private const Int32 DEFAULT_SERVER_PORT = 8888;
        public static bool verbose = true;
        internal const string LOGOUT = "LOGOUT";
        internal const string login = "LOGIN";
        internal const int NETWORK_TIMEOUT = 5000; // 5 sec
        internal const string registration = "REGISTRATION";
        internal const string ERROR_COMMUNICATION = "ERROR_COMMUNICATION";
        internal const string LOGINTRUE = "LOGIN_TRUE";
        internal const string LOGINFALSE = "LOGIN_FALSE";
        internal const string LOGINERROR = "LOGIN_ERROR";
        internal const string LOGINALUSED = "LOGIN_ALUSED";
        internal const string _200_OK = "200_OK";
        internal const string OPEN_CONNECTION = "OPEN_CONNECTION";
        internal const string LOGIN = "LOGIN";
        internal const string _401_UNAUTH = "401_Unauthorized";
        internal const string REGISTRATION = "REGISTRATION";
        internal const string ERROR_FILE_CREATION = "ERROR_FILE_CREATION";
        internal const string SUCCESSFUL_FILE_CREATION = "SUCCESSFUL_FILE_CREATION";
        internal const string LAST_SNAPSHOT_NOT_FOUND = "LAST_SNAPSHOT_NOT_FOUND";
        internal const string LAST_SNAPSHOT = "LAST_SNAPSHOT";
        internal const string DELETED_FILE_LIST = "DELETED_FILE_LIST";
        internal const string READY = "READY";
        internal const string DELETED_FILE_LIST_EMPTY = "DELETED_FILE_LIST_EMPTY";
        internal const string ACK = "ACK";
        internal const string USERNAME_ALREADY_IN_USE = "USERNAME_ALREADY_IN_USE";
        internal const string WRONG_PATH_SYNTAX = "WRONG_PATH_SYNTAX";
        internal const string SUCCESSFUL_REGISTRATION = "SUCCESSFUL_REGISTRATION";
        internal const string NEW_FILE = "NEW_FILE";
        internal const string UPDATE_FILE = "UPDATE_FILE";
        internal const string DELETE_FILE = "DELETE_FILE";
        internal const string RENAME_FILE = "RENAME_FILE";
        internal const string RESTORE_FILE = "RESTORE_FILE";
        internal const string OLD_VERSION_FILE = "OLD_VERSION_FILE";
        internal const string ERROR_INVALID_PATH = "INVALID_PATH_ERROR";
        internal const string COMPLETE_SNAPSHOT = "COMPLETE_SNAPSHOT";
        internal const string NO_CHANGED = "NO_CHANGED";
        internal const string CHANGED_PATH = "CHANGED_PATH";
        internal const string LAST_VERSION = "LAST_VERSION";
        internal const string VERSION_LIST = "version_list";
        internal const string DELETED_LIST = "deleted_list";
        internal const string FILE_ALREADY_SAVED = "FILE_ALREADY_SAVED";
        internal const string UI_LAST_SNAPSHOT_FILE = "LAST_SNAPSHOT_TIMER";
        internal const string UI_RESTORE_FILE = "RESTORE_TIMER";
        internal const string UI_DELETE_FILE = "DELETE_TIMER";
        internal const string UI_SHOW_VERSION = "UI_SHOW_VERSION";
        internal const string CHANGE_FOLDER = "Change Folder";
        internal const string DELETED_ITEMS = "Deleted Items";
        internal const string FILE_RESTORE = "File Restore";
        internal const string LASTSNAPSHOTFUNC = "Last Snapshot";

        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        [STAThread]

        static void Main(string[] args)
        {
            try {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                IPAddress serverIP = null;
                Int32 serverPort = 0;
                bool customIP = false;
                bool customPort = false;

                if (args.Length >= 2)
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        string arg = args[i];
                        switch (arg)
                        {
                            case "-a":
                                try
                                {
                                    i++;
                                    serverIP = IPAddress.Parse(args[i]);
                                    customIP = true;
                                }
                                catch (Exception)
                                {
                                    Console.WriteLine("client: >> Wrong IP syntax");
                                    Console.ReadLine();
                                    return;
                                }
                                break;

                            case "-p":
                                try
                                {
                                    i++;
                                    serverPort = Convert.ToInt32(args[i]);
                                    if (serverPort < 1 || serverPort > 65535) throw new FormatException();
                                    customPort = true;
                                }
                                catch (Exception)
                                {
                                    Console.WriteLine("client: >> Wrong port syntax");
                                    Console.ReadLine();
                                    return;
                                }
                                break;
                        }
                    }
                }
                if (customIP == false) serverIP = IPAddress.Parse(DEFAULT_SERVER_IP);
                if (customPort == false) serverPort = DEFAULT_SERVER_PORT;


                Model m = new Model(serverIP, serverPort);
                Application.Run(new Login(m));


            }catch(Exception e)
            {
                Console.WriteLine("Exception NOT handled " +e.GetType()+" came until main");
                MessageBox.Show("An error occured while the application was running. The application will be closed");
                return;
            }
  }
    }
}
