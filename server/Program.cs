using System;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Security.Cryptography;
using System.Net;
using System.Text.RegularExpressions;
using System.Data.SQLite;

namespace Server
{
    class Program
    {
        internal static bool verbose = true;
        internal static bool debug = true;

        private const string LOCALHOST_SERVER_IP = "127.0.0.1";
        private const Int32 DEFAULT_SERVER_PORT = 8888;

        internal const string DATABASE_NAME = "cloudServerDB.sqlite";

        internal const int MAX_STORED_VERSION_PER_FILE = 10;
        internal const int MAX_STORED_VERSION_PER_FILE_SPECIAL_CASE = 15;

        internal const int INFINITE = System.Threading.Timeout.Infinite;
        internal const int NETWORK_TIMEOUT = 10000; // 10 sec

        internal const string m_200_OK = "200_OK";
        internal const string m_401_Unauthorized = "401_Unauthorized";
        internal const string OPEN_CONNECTION = "OPEN_CONNECTION";
        internal const string REGISTRATION = "REGISTRATION";
        internal const string LOGIN = "LOGIN";
        internal const string USERNAME_ALREADY_IN_USE = "USERNAME_ALREADY_IN_USE";
        internal const string SUCCESSFUL_REGISTRATION = "SUCCESSFUL_REGISTRATION";
        internal const string WRONG_PATH_SYNTAX = "WRONG_PATH_SYNTAX";

        internal const string SUCCESSFUL_FILE_CREATION = "SUCCESSFUL_FILE_CREATION";
        internal const string LAST_SNAPSHOT_NOT_FOUND = "LAST_SNAPSHOT_NOT_FOUND";
        internal const string LAST_SNAPSHOT = "LAST_SNAPSHOT";
        internal const string ACK = "ACK";
        internal const string DELETED_FILE_LIST = "DELETED_FILE_LIST";
        internal const string DELETED_FILE_LIST_EMPTY = "DELETED_FILE_LIST_EMPTY";
        internal const string READY = "READY";
        internal const string LAST_VERSION = "LAST_VERSION";
        internal const string ERROR_FILE_CREATION = "ERROR_FILE_CREATION";
        internal const string ERROR_INVALID_PATH = "INVALID_PATH_ERROR";
        internal const string ERROR_COMMUNICATION = "ERROR_COMMUNICATION";
        internal const string FILE_EMPTY = "FILE_EMPTY";

        internal const string NEW_FILE = "NEW_FILE";
        internal const string UPDATE_FILE = "UPDATE_FILE";
        internal const string DELETE_FILE = "DELETE_FILE";
        internal const string RENAME_FILE = "RENAME_FILE";
        internal const string RESTORE_FILE = "RESTORE_FILE";
        internal const string OLD_VERSION_FILE = "OLD_VERSION_FILE";
        internal const string COMPLETE_SNAPSHOT = "COMPLETE_SNAPSHOT";
        internal const string LOGOUT = "LOGOUT";

        internal const string serverDirPath = @"C:\server\";
        internal const string snapshotDirPath = @"C:\server\snapshot\";
        internal const string tmpDirPath = @"C:\server\tmp\";

        static void Main(string[] args)
        {
            int activeClientCount;

            TcpListener serverSocket = null;
            TcpClient clientSocket = null;

            IPAddress serverIP = null;
            Int32 serverPort = 0;
            bool customIP = false;
            bool customPort = false;

            if (args.Length >= 1)
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
                                Console.WriteLine("server: >> Wrong IP syntax");
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
                                Console.WriteLine("server: >> Wrong port syntax");
                                Console.ReadLine();
                                return;
                            }
                            break;

                        case "--local":
                            try
                            {
                                serverIP = IPAddress.Parse(LOCALHOST_SERVER_IP);
                                customIP = true;
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("server: >> Wrong localhost IP syntax");
                                Console.ReadLine();
                                return;
                            }
                            break;

                        case "-h":
                            PrintHelp();
                            return;

                        case "--help":
                            PrintHelp();
                            return;
                    }
                }
            }
            if (customIP == false)
            {
                try
                {
                    serverIP = IPAddress.Parse(GetLocalIPAddress());
                }
                catch (Exception)
                {
                    Console.WriteLine("server: >> Wrong IP syntax");
                    Console.ReadLine();
                    return;
                }
            }
            if (customPort == false) serverPort = DEFAULT_SERVER_PORT;

            try
            {
                try
                {
                    // STEP 1: database initilization
                    PrintVerboseMessage("server: >> Server started");

                    if (File.Exists(DATABASE_NAME) == false)
                    {
                        SQLiteConnection.CreateFile(DATABASE_NAME);
                        PrintVerboseMessage("server: >> Database created");
                    }

                    DirectoryInfo serverDir = new DirectoryInfo(serverDirPath);
                    if (serverDir.Exists == false) serverDir.Create();
                    PrintVerboseMessage("server: >> Server directory path: " + serverDirPath);
                    DirectoryInfo snapshotDir = new DirectoryInfo(snapshotDirPath);
                    if (snapshotDir.Exists == false) snapshotDir.Create();
                    PrintVerboseMessage("server: >> Server snapshot path: " + snapshotDirPath);
                    DirectoryInfo tmpDir = new DirectoryInfo(tmpDirPath);
                    if (tmpDir.Exists == false) tmpDir.Create();
                    PrintVerboseMessage("server: >> Server tmp path: " + tmpDirPath);

                    PrintVerboseMessage("server: >> Connection with database opening...");
                    using (SQLiteConnection dbConnection = new SQLiteConnection("Data Source=cloudServerDB.sqlite;Version=3;"))
                    {
                        dbConnection.Open();
                        DBmanager dbManager = new DBmanager(dbConnection);

                        if (dbManager.dbTableExists("users") == false)
                        {
                            string query = "CREATE TABLE users (username VARCHAR(255) PRIMARY KEY NOT NULL, hash_pwd TEXT NOT NULL, salt TEXT NOT NULL, active INTEGER NOT NULL, backup_path VARCHAR(255) NOT NULL, file_table_name VARCHAR(255) NOT NULL, snapshot_table_name VARCHAR(255) NOT NULL, last_login DATETIME, creation_time DATETIME, last_modification DATETIME);";
                            int modifNumber = dbManager.dbExecuteWriteQuery(query);
                            if (modifNumber == -1) throw new Exception("DB table creation error");
                            PrintVerboseMessage("server: >> Database 'users' table created");
                        }

                        PrintVerboseMessage("server: >> Database ready");
                    }
                    PrintVerboseMessage("server: >> Connection with database closed");

                    // STEP 2: sockets initialization
                    serverSocket = new TcpListener(serverIP, serverPort);
                    PrintVerboseMessage("server: >> Server IP address: " + serverIP);
                    PrintVerboseMessage("server: >> Server port: " + serverPort);
                    activeClientCount = 0;
                    clientSocket = default(TcpClient); // inizializzazione neutrale
                    serverSocket.Start();
                    PrintVerboseMessage("server: >> Main listener socket started");
                    PrintVerboseMessage("server: >> Ready to accept TCP connections from client");

                    while (true)
                    {
                        try
                        {
                            // STEP 3: connection with a client
                            PrintVerboseMessage("server: >> Waiting for a new client...");
                            clientSocket = serverSocket.AcceptTcpClient();
                            PrintVerboseMessage("server: >> Accepted connections from a client");
                            activeClientCount++;
                            PrintVerboseMessage("server: >> Client number " + activeClientCount + " is connected");

                            // STEP 4: new client in a new thread
                            HandleClient client = new HandleClient();
                            client.startClient(clientSocket);
                        }
                        catch (Exception)
                        {
                            if (clientSocket.Connected)
                            {
                                clientSocket.Close();
                                Console.WriteLine("server: >> Connection with client closed");
                            }
                            throw;
                        }
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine("server:Exception >> IOException: " + e.Message);
                    throw;
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Console.WriteLine("server:Exception >> ArgumentOutOfRangeException: " + e.Message);
                    throw;
                }
                catch (ArgumentNullException e)
                {
                    Console.WriteLine("server:Exception >> ArgumentNullException: " + e.Message);
                    throw;
                }
                catch (FormatException e)
                {
                    Console.WriteLine("server:Exception >> FormatException: " + e.Message);
                    throw;
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine("server:Exception >> InvalidOperationException: " + e.Message);
                    throw;
                }
                catch (SocketException e)
                {
                    Console.WriteLine("server:Exception >> SocketException: " + e.Message);
                    throw;
                }
                catch (Exception e)
                {
                    Console.WriteLine("server:Exception >> Execution terminated because of exception: " + e.Message);
                    throw;
                }
            }
            catch (Exception)
            {
                if (clientSocket.Connected)
                {
                    clientSocket.Close();
                    Console.WriteLine("server: >> Connection with client closed");
                }
            }
            finally
            {
                if (clientSocket.Connected)
                {
                    clientSocket.Close();
                    PrintVerboseMessage("server: >> Connection with client closed");
                }

                serverSocket.Stop();
                PrintVerboseMessage("server: >> Main server socket closed");

                PrintVerboseMessage("server: >> Exiting...");
                Console.ReadLine();
            }

        }

        private static void PrintVerboseMessage(string message)
        {
            if (verbose) Console.WriteLine(message);
        }

        private static void PrintDebugMessage(string message)
        {
            if (debug) Console.WriteLine(message);
        }

        public static DateTime Now()
        {
            DateTime localDate = DateTime.Now;
            return localDate;
        }

        public static string sha256(string value)
        {
            StringBuilder Sb = new StringBuilder();

            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }

        public static bool IsValidPath(string path)
        {
            Regex driveCheck = new Regex(@"^[a-zA-Z]:\\$");
            if (!driveCheck.IsMatch(path.Substring(0, 3))) return false;
            string strTheseAreInvalidFileNameChars = new string(Path.GetInvalidPathChars());
            strTheseAreInvalidFileNameChars += @":/?*<>" + "\"";
            Regex containsABadCharacter = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");
            if (containsABadCharacter.IsMatch(path.Substring(3, path.Length - 3))) return false;

            return true;
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "Local IP Address Not Found!";
        }

        public static bool IsValidAlphaNumericString(string str)
        {
            Regex regex = new Regex("^[a-zA-Z0-9]*$");
            if (regex.IsMatch(str)) return true;
            return false;
        }

        public static void PrintHelp()
        {
            Console.WriteLine("\nServer.exe [param]\n");
            Console.WriteLine("    -a   <ip address> \t set a custom IP address for server");
            Console.WriteLine("    -p   <port> \t set a custom port for server");
            Console.WriteLine("    --local \t\t set localhost as IP address");
            Console.WriteLine("    --help \t\t print help");
        }
    }
}
