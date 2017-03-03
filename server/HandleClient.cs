using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    internal class HandleClient
    {
        private TcpClient clientSocket;
        private NetworkStream networkStream;
        private DBmanager dbManager;
        private NetworkManager networkManager;
        private EventManager eventManager;
        private string username;
        private string password;
        private string backupPath;

        string fileTableName;
        string snapshotTableName;

        string tmpUserDirPath;

        int head;
        int tail;
        int size;
        Message[] messages;
        Semaphore empty;
        Semaphore full;

        volatile bool userLoggedIn;

        public void startClient(TcpClient clientSocket)
        {
            try
            {
                try
                {
                    this.clientSocket = clientSocket;

                    GC.Collect();
                    
                    Thread handshakeThread = new Thread(doHandshake);
                    handshakeThread.Start();
                }
                catch (ArgumentNullException e)
                {
                    Console.WriteLine("server:" + Thread.CurrentThread.Name + ":ArgumentNullException >> New thread generation failed");
                    Console.WriteLine("server:" + Thread.CurrentThread.Name + ":ArgumentNullException >> " + e.Message);
                    throw;
                }
                catch (ThreadStateException e)
                {
                    Console.WriteLine("server:" + Thread.CurrentThread.Name + ":ThreadStateException >> New thread start failed");
                    Console.WriteLine("server:" + Thread.CurrentThread.Name + ":ThreadStateException >> " + e.Message);
                    throw;
                }
                catch (OutOfMemoryException e)
                {
                    Console.WriteLine("server:" + Thread.CurrentThread.Name + ":OutOfMemoryException >> New thread start failed");
                    Console.WriteLine("server:" + Thread.CurrentThread.Name + ":OutOfMemoryException >> " + e.Message);
                    throw;
                }
                catch (Exception e)
                {
                    Console.WriteLine("server:" + Thread.CurrentThread.Name + ":Exception >> " + e.Message);
                    throw;
                }
            }
            catch (Exception)
            {
                CloseClientConnection();
            }
        }

        private void doHandshake()
        {
            try
            {
                PrintVerboseMessage("server:handshakeThread: >> Thread <Handshake> is running...");
                if (Thread.CurrentThread.Name == null) Thread.CurrentThread.Name = "handshakeThread";

                dbManager = new DBmanager();
                networkManager = new NetworkManager();
                eventManager = new EventManager();

                dbManager.dbOpenConnection();

                NetworkStream networkStream = clientSocket.GetStream();
                this.networkStream = networkStream;

                userLoggedIn = false;

                head = 0;
                tail = 0;
                size = 50;
                messages = new Message[size];
                empty = new Semaphore(size, size);
                full = new Semaphore(0, size);

                // "OPEN_CONNECTION"
                string clientRequest = networkManager.NetReadTextMsg(networkStream);
                PrintVerboseMessage("server:handshakeThread: >> Received message: " + clientRequest);

                if (clientRequest != Program.OPEN_CONNECTION)
                {
                    PrintVerboseMessage("server:handshakeThread: >> Wrong initialization message received");
                    throw new SocketException();
                }

                // "200_OK"
                networkManager.NetWriteTextMsg(networkStream, Program.m_200_OK);

                // "REGISTRATION" or "LOGIN"
                username = string.Empty;
                password = string.Empty;
                backupPath = string.Empty;

                do
                {
                    string operation = networkManager.NetReadTextMsg(networkStream);
                    PrintVerboseMessage("server:handshakeThread: >> Requested operation: " + operation);
                    switch (operation)
                    {
                        case Program.REGISTRATION:
                            OperationRegistration();
                            break;

                        case Program.LOGIN:
                            OperationLogin();
                            break;

                        default:
                            PrintVerboseMessage("server:handshakeThread: >> Wrong operation request received");
                            throw new SocketException();
                    }
                }
                while (userLoggedIn == false);

                tmpUserDirPath = Program.tmpDirPath + username + @"\";
                Directory.CreateDirectory(tmpUserDirPath);

                // step A - send last snapshot to client (if exists)
                SendLastSnapshotToClient();

                // step B - send list of deleted files to client
                SendListDeletedFilesToClient();

                // step C - handshake terminated
                PrintVerboseMessage("server:handshakeThread: >> Handshake terminated");
                Thread outputClientThread = new Thread(doOutputCommunication);
                outputClientThread.Start();
                Thread inputClientThread = new Thread(doInputCommunication);
                inputClientThread.Start();

                /*
                 * uncomment the following code to force HandshakeThread to wait the termination of InputThread and OutputThread
                 *
                outputClientThread.Join();
                PrintVerboseMessage("server:handshakeThread: >> Output thread terminated");
                inputClientThread.Join();
                PrintVerboseMessage("server:handshakeThread: >> Input thread terminated");
                */
                return;
            }
            catch (SocketException)
            {
                Console.WriteLine("server:handshakeThread: >> Some network problem occurred");;
                CloseClientConnection();
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine("server:handshakeThread:Exception >> " + e.Message);
                CloseClientConnection();
                return;
            }
        }

        private void doOutputCommunication()
        {
            PrintVerboseMessage("server:outputThread: >> Thread <OutputCommunication> is running...");
            try
            {
                if (Thread.CurrentThread.Name == null) Thread.CurrentThread.Name = "outputThread";

                // READY
                this.networkManager.NetWriteTextMsg(networkStream, Program.READY);

                // step D - output thread sync with input thread
                eventManager.event2.Set();

                while (userLoggedIn)
                {
                    Message message = ConsumeMessage();
                    SendMessage(networkStream, message);
                }
            }
            catch (SocketException)
            {
                if (userLoggedIn)
                {
                    Console.WriteLine("server:outputThread: >> Some network problem occurred");
                    Logout();
                }
                else
                {
                    Console.WriteLine("server:outputThread: >> Connection already closed from another thread");
                }
                return;
            }
            catch (Exception e)
            {
                if (userLoggedIn)
                {
                    Console.WriteLine("server:outputThread:Exception >> " + e.Message);
                    Logout();
                }
                else
                {
                    Console.WriteLine("server:outputThread: >> Connection already closed from another thread");
                }
                return;
            }
            return;
        }

        private void doInputCommunication()
        {
            // step D - input thread sync with output thread
            WaitHandle.WaitAll(new WaitHandle[] { eventManager.event2 });
            PrintVerboseMessage("server:inputThread: >> Thread <InputCommunication> is running...");

            try
            {
                if (Thread.CurrentThread.Name == null) Thread.CurrentThread.Name = "inputThread";

                // step E - server wait for client's requests
                string ack = string.Empty;
                int errorCounter = 0;

                while (userLoggedIn)
                {
                    PrintVerboseMessage("server:inputThread: >> Waiting for a new operation request...");
                    string operation = this.networkManager.NetReadTextMsg(networkStream, Program.INFINITE);

                    switch (operation)
                    {
                        case Program.NEW_FILE:
                            errorCounter = 0;
                            OperationNewFile();
                            break;

                        case Program.UPDATE_FILE:
                            errorCounter = 0;
                            OperationUpdateFile();
                            break;

                        case Program.DELETE_FILE:
                            errorCounter = 0;
                            OperationDeleteFile();
                            break;

                        case Program.RENAME_FILE:
                            errorCounter = 0;
                            OperationRenameFile();
                            break;

                        case Program.RESTORE_FILE:
                            errorCounter = 0;
                            OperationRestoreFile();
                            break;

                        case Program.OLD_VERSION_FILE:
                            errorCounter = 0;
                            OperationOldVersionFile();
                            break;

                        case Program.COMPLETE_SNAPSHOT:
                            errorCounter = 0;
                            OperationCompleteSnapshot();
                            break;

                        case Program.LOGOUT:
                            errorCounter = 0;
                            OperationLogout();
                            break;

                        default:
                            PrintVerboseMessage("server:inputThread: >> **********UNKNOWN OPERATION**********");
                            PrintVerboseMessage("server:inputThread: >> Received string: " + operation);
                            errorCounter++;
                            if (errorCounter >= 10) throw new SocketException();
                            break;
                    }
                }
            }
            catch (SocketException)
            {
                if (userLoggedIn)
                {
                    Console.WriteLine("server:inputThread: >> Some network problem occurred");
                    Logout();
                }
                else
                {
                    Console.WriteLine("server:outputThread: >> Connection already closed from another thread");
                }
                return;
            }
            catch(Exception e)
            {
                if (userLoggedIn)
                {
                    Console.WriteLine("server:inputThread:Exception >> " + e.Message);
                    Logout();
                }
                else
                {
                    Console.WriteLine("server:outputThread: >> Connection already closed from another thread");
                }
                return;
            }
            return;
        }

        private void PrintVerboseMessage(string message)
        {
            if (Program.verbose) Console.WriteLine(message);
        }

        private void PrintDebugMessage(string message)
        {
            if (Program.debug) Console.WriteLine(message);
        }

        private int CreateNewUser(string username, string password, string backupPath)
        {
            if (Program.IsValidAlphaNumericString(username) == false) return 1;
            if (Program.IsValidAlphaNumericString(password) == false) return 2;
            if (UserExists(username)) return 1;

            int length = 32;
            string salt = GenerateSalt(length);
            string saltedPassword = password + salt;

            string query1 = @"INSERT INTO users (username, hash_pwd, salt, active, backup_path, file_table_name, snapshot_table_name, last_login, creation_time, last_modification) 
                VALUES ('" + username + "', '" + Program.sha256(saltedPassword) + "', '" + salt + "', " + 1 + ", '" + backupPath + "', '" + fileTableName + "', '" + snapshotTableName + "', '" + "NULL" + "', '" + Program.Now() + "', '" + Program.Now() + "' );";
            string query2 = "CREATE TABLE " + fileTableName + " (id_file INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, path VARCHAR(255) NOT NULL, version DATETIME NOT NULL, valid INTEGER NOT NULL, file_hash TEXT, length BIGINT NOT NULL, file BLOB);";
            string query3 = "CREATE TABLE " + snapshotTableName + " (id_snapshot INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, version DATETIME NOT NULL, file_snapshot BLOB);";

            List<string> querys = new List<string>();
            querys.Add(query1);
            querys.Add(query2);
            querys.Add(query3);

            int modifiedRows = dbManager.dbExecuteTransaction(querys);
            if(modifiedRows < querys.Count)
            {
                if(modifiedRows == -1) PrintVerboseMessage("server:" + Thread.CurrentThread.Name + ":CreateNewUser >> Server internal error. Invalid operation.");
                else PrintVerboseMessage("server:" + Thread.CurrentThread.Name + ":CreateNewUser >> " + modifiedRows + " lines modified instead of " + querys.Count);
                return 3;
            }
            PrintVerboseMessage("server:" + Thread.CurrentThread.Name + ": >> New user created");

            return 0;
        }

        private bool UserExists(string username)
        {
            int countRows = 0;
            string query = "SELECT COUNT(*) FROM users WHERE username='" + username + "';";
            countRows = dbManager.dbExecuteCountQuery(query);
            if (countRows > 0) return true;

            return false;
        }

        private string GenerateSalt(int length)
        {
            Random randomGenerator = new Random();
            const string symbols = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            char[] buffer = new char[length];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = symbols[randomGenerator.Next(symbols.Length)];
            }
            string salt = new string(buffer);

            return salt;
        }

        private string GetBackupPath(string username)
        {
            string query = "SELECT backup_path FROM users WHERE username='" + username + "';";
            string data = dbManager.dbExecuteReadQuery(query, "backup_path");

            return data;
        }

        private bool Login(string username, string password)
        {
            if (Program.IsValidAlphaNumericString(username) == false) return false;
            if (Program.IsValidAlphaNumericString(password) == false) return false;
            int countRows = 0;
            string query = "SELECT COUNT(*) FROM users WHERE username='" + username + "';";
            countRows = dbManager.dbExecuteCountQuery(query);
            if (countRows == 1)
            {
                query = "SELECT salt FROM users WHERE username='" + username + "';";
                string salt = dbManager.dbExecuteReadQuery(query, "salt");

                countRows = 0;
                query = "SELECT COUNT(*) FROM users WHERE username='" + username + "' AND hash_pwd='" + Program.sha256(password + salt) + "';";
                countRows = dbManager.dbExecuteCountQuery(query);
                if (countRows == 1)
                {
                    countRows = 0;
                    var command = new SQLiteCommand();
                    command.CommandText = "UPDATE users SET last_login = @time WHERE username= @username AND hash_pwd= @hashpassword;";
                    command.Parameters.AddWithValue("@time", Program.Now());
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@hashpassword", Program.sha256(password + salt));
                    countRows = dbManager.dbExecuteCommand(command);
                    if (countRows < 1) return false;

                    return true;
                }
            }

            return false;
        }

        private void Logout()
        {
            // "LOGOUT"
            PrintVerboseMessage("server:" + Thread.CurrentThread.Name + ": >> **********LOGOUT**********");
            userLoggedIn = false;

            // save a snapshot updated version in database
            string snapshotLocalPath = Program.snapshotDirPath + @"snapshot_" + username + ".txt";
            List<string> logoutfileList = CalculateSnapshotListFromDb();
            string returnMsg = CreateSnapshotFileFromList(logoutfileList, snapshotLocalPath);
            if (returnMsg == Program.SUCCESSFUL_FILE_CREATION)
            {
                PrintVerboseMessage("server:" + Thread.CurrentThread.Name + ": >> Snapshot created and temporarily saved in: " + snapshotLocalPath);

                // save snapshot file in snapshot database table
                bool savedSnapshot = PutSnapshotInDb(snapshotLocalPath);
                if (savedSnapshot) PrintVerboseMessage("server:" + Thread.CurrentThread.Name + ": >> Snapshot created and saved in database");
                else PrintVerboseMessage("server:" + Thread.CurrentThread.Name + ": >> WARNING! Impossible to save current snapshot in database");
            }

            // close connection and clean temporary file
            CloseClientConnection();

            PrintVerboseMessage("server:" + Thread.CurrentThread.Name + ": >> Logout successfully executed");
            PrintVerboseMessage("server:" + Thread.CurrentThread.Name + ": >> *************************************");
        }

        private void CloseClientConnection()
        {
            string threadName = string.Empty;
            if (Thread.CurrentThread.Name != null) threadName = Thread.CurrentThread.Name;
            else threadName = "mainThread";
            Console.WriteLine("server:" + threadName + ": >> Closing connection with the client...");

            userLoggedIn = false;
            try
            {
                ProduceMessage(NewTextMessage(Program.LOGOUT));

                // close database connection
                dbManager.dbCloseConnection();

                // close network connection
                networkStream.Flush();
                networkStream.Close();
                clientSocket.Close();

                // remove user temporary files
                DeleteFileInsideDirectory(tmpUserDirPath);
                GC.Collect();
                PrintVerboseMessage("server:" + threadName + ": >> Environment cleaned from temporary files");
            }
            catch(Exception e)
            {
                Console.WriteLine("server:" + threadName + ":Exception: >> " + e.Message);
            }
        }

        private void SendLastSnapshotToClient()
        {
            string snapshotLocalPath = Program.snapshotDirPath + @"snapshot_" + username + ".txt";

            // server calculate last snapshot from DB data
            bool snapshotEmpty = true;

            PrintVerboseMessage("server:handshakeThread: >> Analyzing database to calculate snapshot...");
            List<string> fileList = CalculateSnapshotListFromDb();
            string returnMsg = CreateSnapshotFileFromList(fileList, snapshotLocalPath);
            if (returnMsg == Program.SUCCESSFUL_FILE_CREATION) snapshotEmpty = false;
            else if (returnMsg == Program.FILE_EMPTY) snapshotEmpty = true;
            else
            {
                PrintVerboseMessage("server:handshakeThread: >> Impossible to complete handshake: snapshot creation error");
                throw new SocketException();
            }

            // send last snapshot to the client
            string ack = string.Empty;
            if (snapshotEmpty == false)
            {
                PrintVerboseMessage("server:handshakeThread: >> Snapshot read from database and temporarily saved in: " + snapshotLocalPath);

                // save snapshot file in snapshot database table
                bool savedSnapshot = PutSnapshotInDb(snapshotLocalPath);
                if (savedSnapshot == false) PrintVerboseMessage("server:handshakeThread: >> WARNING! Impossible to save current snapshot in database");

                PrintVerboseMessage("server:handshakeThread: >> Snapshot file saved in database");

                // "LAST_SNAPSHOT"
                this.networkManager.NetWriteTextMsg(networkStream, Program.LAST_SNAPSHOT);
                ack = this.networkManager.NetReadTextMsg(networkStream);
                if (ack != Program.ACK)
                {
                    PrintVerboseMessage("server:handshakeThread: >> Impossible to complete handshake: missing ACK after last snapshot message");
                    throw new SocketException();
                }

                // <<FILE>>
                this.networkManager.NetWriteFile(networkStream, snapshotLocalPath);
                ack = string.Empty;
                ack = this.networkManager.NetReadTextMsg(networkStream);
                if (ack != Program.ACK)
                {
                    PrintVerboseMessage("server:handshakeThread: >> Impossible to complete handshake: missing ACK after last snapshot file");
                    throw new SocketException();
                }
                PrintVerboseMessage("server:handshakeThread: >> Snapshot correctly sent to client");
            }
            else
            {
                PrintVerboseMessage("server:handshakeThread: >> Can't find any file to create snapshot for " + username + " in database");

                // "LAST_SNAPSHOT_NOT_FOUND"
                this.networkManager.NetWriteTextMsg(networkStream, Program.LAST_SNAPSHOT_NOT_FOUND);
                returnMsg = string.Empty;
                returnMsg = this.networkManager.NetReadTextMsg(networkStream);

                if (returnMsg != Program.ACK)
                {
                    PrintVerboseMessage("server:handshakeThread: >> Unexpected message. Closing connection...");
                    PrintDebugMessage("Received message: " + returnMsg);
                    throw new SocketException();
                }
            }
        }

        private void SendListDeletedFilesToClient()
        {
            // get list of deleted file
            string deletedFileListPath = tmpUserDirPath + @"deletedFileList.txt";
            int deletedFileNumber = GetDeletedFileList(deletedFileListPath);

            // send list of deleted file to the client -> "DELETED_FILE_LIST" and "<<FILE>>"
            string ack = string.Empty;
            if (deletedFileNumber > 0)
            {
                this.networkManager.NetWriteTextMsg(networkStream, Program.DELETED_FILE_LIST);
                this.networkManager.NetWriteFile(networkStream, deletedFileListPath);
            }
            else
            {
                this.networkManager.NetWriteTextMsg(networkStream, Program.DELETED_FILE_LIST_EMPTY);
            }
            ack = this.networkManager.NetReadTextMsg(networkStream);
            if (ack != Program.ACK)
            {
                PrintVerboseMessage("server:handshakeThread: >> Impossible to complete handshake: missing ACK after deleted file list");
                throw new SocketException();
            }
            if (deletedFileNumber > 0) PrintVerboseMessage("server:handshakeThread: >> List of deleted files correctly sent to client");
            else PrintVerboseMessage("server:handshakeThread: >> List of deleted files is empty");
        }

        private List<string> CalculateSnapshotListFromDb()
        {
            string query = "SELECT DISTINCT path AS currentpath, version FROM " + fileTableName + " WHERE valid=1 AND version= (SELECT MAX(version) FROM " + fileTableName + " WHERE path = currentpath);";
            List<string> fileList = dbManager.dbExecuteGetSnapshotQuery(query, "currentpath", "version", "*");

            return fileList;
        }

        private string CreateSnapshotFileFromList(List<string> fileList, string outputPath)
        {
            if (fileList.Count == 0) return Program.FILE_EMPTY;

            using(FileStream stream = File.Open(outputPath, FileMode.Create))
            {}

            using (StreamWriter sw = File.AppendText(outputPath))
            {
                sw.WriteLine("SNAPSHOT");
                foreach (string s in fileList)
                {
                    sw.WriteLine("<" + s + ">");
                }
            }

            PrintDebugMessage("server:CalculateSnapshotFromDb: >> List of valid files: ");
            if (Program.debug) PrintFile(outputPath);

            if (File.Exists(outputPath) == false) return Program.ERROR_FILE_CREATION;
            return Program.SUCCESSFUL_FILE_CREATION;
        }

        private bool PutSnapshotInDb(string path)
        {
            byte[] file = FileToByteArray(path);

            var command = new SQLiteCommand();
            command.CommandText = "INSERT INTO " + snapshotTableName + " (version, file_snapshot) VALUES (@time , @file);";
            command.Parameters.AddWithValue("@time", Program.Now());
            command.Parameters.Add("@file", DbType.Binary, 20).Value = file;
            int modifiedRows = dbManager.dbExecuteCommand(command);
            if (modifiedRows != 1) return false;

            return true;
        }

        private int FileExistsInDb(string path) // returns the number of different versions stored for this file
        {
            int countRows = 0;
            string query = "SELECT COUNT(*) FROM " + fileTableName + " WHERE path='" + path + "';";
            countRows = this.dbManager.dbExecuteCountQuery(query);
            
            return countRows;
        }

        private bool IsFileDuplicated(string path, string version)
        {
            int countRows = 0;
            string query = "SELECT COUNT(*) FROM " + fileTableName + " WHERE path='" + path + "' AND version='" + version + "';";
            countRows = this.dbManager.dbExecuteCountQuery(query);

            if (countRows > 0) return true;
            return false;
        }

        private bool RemoveVersionOfFile(string path, string version)
        {
            string query = "DELETE FROM " + fileTableName + " WHERE path= '" + path + "' AND version='" + version + "';";
            int modifiedRows = dbManager.dbExecuteWriteQuery(query);
            if (modifiedRows == -1) return false;
            return true;
        }

        private bool RemoveOldestVersionOfFile(string path) // remove the oldest version stored of the selected file
        {
            int differentVersionsOfThisFile = FileExistsInDb(path);
            if (differentVersionsOfThisFile > 0)
            {
                string query = "DELETE FROM " + fileTableName + " WHERE path= '" + path + "' AND version= (SELECT MIN (version) FROM " + fileTableName + " WHERE path= '" + path + "');";
                int modifiedRows = dbManager.dbExecuteWriteQuery(query);
                if (modifiedRows == -1) return false;
                return true;
            }

            return false;
        }

        private bool AddFile(FileCloud file)
        {
            string clientPath = file.getClientPath();
            byte[] fileByte = FileToByteArray(file.getLocalPath());

            int fileHash = fileByte.GetHashCode();

            // If the number of version is equal to MAX_STORED_VERSION_PER_FILE, the oldest version is deleted before adding the new one
            int differentVersionsOfThisFile = FileExistsInDb(clientPath);
            if (differentVersionsOfThisFile == -1) return false;
            if(differentVersionsOfThisFile > 0)
            {
                /*
                 * ERROR CASE MANAGEMENT:
                 * If there is another file with equal path and equal version overwrite it
                 */
                string version = file.getVersion().ToString("yyyy-MM-dd HH:mm:ss");
                if (IsFileDuplicated(clientPath, version))
                {
                    RemoveVersionOfFile(clientPath, version);
                    differentVersionsOfThisFile--;
                }

                // If there are too many versions already stored
                if (differentVersionsOfThisFile >= Program.MAX_STORED_VERSION_PER_FILE)
                {
                    RemoveOldestVersionOfFile(clientPath);
                }
            }

            var command = new SQLiteCommand();
            command.CommandText = "INSERT INTO " + fileTableName + " (path, version, valid, file_hash, length, file) VALUES (@path, @version, @valid, @fileHash, @length, @file);";
            command.Parameters.AddWithValue("@path", clientPath);
            command.Parameters.AddWithValue("@version", file.getVersion());
            command.Parameters.AddWithValue("@valid", 1);
            command.Parameters.AddWithValue("@fileHash", fileHash.ToString());
            command.Parameters.AddWithValue("@length", fileByte.Length);
            command.Parameters.Add("@file", DbType.Binary, 20).Value = fileByte;

            int modifiedRows = dbManager.dbExecuteCommand(command);
            if (modifiedRows != 1) return false;

            return true;
        }

        private bool RenameFile(string oldPath, string newPath)
        {
            var command = new SQLiteCommand();
            command.CommandText = "UPDATE " + fileTableName + " SET path=@newPath WHERE path=@oldPath;";
            command.Parameters.AddWithValue("@newPath", newPath);
            command.Parameters.AddWithValue("@oldPath", oldPath);

            int modifiedRows = dbManager.dbExecuteCommand(command);
            if (modifiedRows < 1) return false;

            return true;
        }

        private bool UpdateLastVersionOFFile(string path, DateTime newVersion)
        {
            string lastVersion = GetLastVersionOfFile(path);

            var command = new SQLiteCommand();
            command.CommandText = "UPDATE " + fileTableName + " SET version=@newVersion WHERE path=@path AND version=@lastVersion;";
            command.Parameters.AddWithValue("@newVersion", newVersion);
            command.Parameters.AddWithValue("@path", path);
            command.Parameters.AddWithValue("@lastVersion", lastVersion);

            int modifiedRows = dbManager.dbExecuteCommand(command);
            if (modifiedRows < 1) return false;

            return true;
        }

        private bool SetFileAsDeleated(string path) // all versions of this file are marked as DELEATED (valid=0)
        {
            int differentVersionsOfThisFile = FileExistsInDb(path);
            if(differentVersionsOfThisFile > 0)
            {
                var command = new SQLiteCommand();
                command.CommandText = "UPDATE " + fileTableName + " SET valid=0 WHERE path=@path;";
                command.Parameters.AddWithValue("@path", path);

                int modifiedRows = dbManager.dbExecuteCommand(command);
                if (modifiedRows < 1) return false;
                return true;
            }

            return false;
        }

        private bool SetFileAsRestored(string path) // all versions of this file are marked as RESTORED (valid=1)
        {
            int differentVersionsOfThisFile = FileExistsInDb(path);
            if (differentVersionsOfThisFile > 0)
            {
                var command = new SQLiteCommand();
                command.CommandText = "UPDATE " + fileTableName + " SET valid=1 WHERE path=@path;";
                command.Parameters.AddWithValue("@path", path);

                int modifiedRows = dbManager.dbExecuteCommand(command);
                if (modifiedRows < 1) return false;
                return true;
            }

            return false;
        }

        private int GetDeletedFileList(string outputPath)
        {
            string query = @"SELECT DISTINCT path FROM " + fileTableName + " WHERE valid=0;";
            List<string> deletedFileList = dbManager.dbExecuteGetStringListQuery(query, "path");

            using (FileStream stream = File.Open(outputPath, FileMode.Create))
            { }

            File.WriteAllLines(outputPath, deletedFileList);
            PrintDebugMessage("server:" + Thread.CurrentThread.Name + ": >> List of deleted files: ");
            if (Program.debug) PrintFile(outputPath);

            return deletedFileList.Count;
        }

        private string GetLastVersionOfFile(string path)
        {
            string query = @"SELECT MAX(version) as version FROM " + fileTableName + " WHERE path= '" + path + "';";
            string strMaxVersion = dbManager.dbExecuteReadQuery(query, "version");

            return strMaxVersion;
        }

        private string GetFileFromVersion(string path, string version)
        {
            string outputPath = tmpUserDirPath + Path.GetFileName(path);

            string[] parametersList = new string[7];
            parametersList[0] = "id_file";
            parametersList[1] = "path";
            parametersList[2] = "version";
            parametersList[3] = "valid";
            parametersList[4] = "file_hash";
            parametersList[5] = "length";
            parametersList[6] = "file";
            string query = "SELECT id_file, path, version, valid, file_hash, length, file FROM " + fileTableName + " WHERE path='" + path + "' AND version like '" + version + "';";
            dbManager.dbExecuteGetFileQuery(query, parametersList, outputPath);
            
            if (File.Exists(outputPath) == false) return Program.ERROR_FILE_CREATION;

            return outputPath;
        }

        private List<string> GetOldVersionsOfFileList(string path)
        {
            string query = "SELECT version FROM " + fileTableName + " WHERE path= '" + path + "' ORDER BY version;";
            List<string> oldVersionsListStr = dbManager.dbExecuteGetStringListQuery(query, "version");
            List<string> oldVersionsListConverted = new List<string>();

            foreach (string str in oldVersionsListStr)
            {
                var dateTime = DateTime.ParseExact(str, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                string textFormat = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                oldVersionsListConverted.Add(textFormat);
            }

            return oldVersionsListConverted;
        }

        private void ProduceMessage(Message m)
        {
            empty.WaitOne();

            // write in shared buffer in 'head' position
            messages[head] = m;

            head = (head + 1) % size;
            full.Release(1);
        }

        private Message ConsumeMessage()
        {
            full.WaitOne();

            //read from shared buffer in 'tail' position
            Message m = messages[tail];

            tail = (tail + 1) % size;
            empty.Release(1);

            return m;
        }

        private void WriteListToFile(List<string> list, string outputPath)
        {
            File.WriteAllLines(outputPath, list);
        }

        private void PrintFile(string path)
        {
            if (File.Exists(path))
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                byte[] b = new byte[1024];
                UTF8Encoding temp = new UTF8Encoding(true);
                while (fs.Read(b, 0, b.Length) > 0)
                {
                    Console.WriteLine(temp.GetString(b));
                }
                fs.Close();
            }
        }

        private byte[] FileToByteArray(string path)
        {
            byte[] file;
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new BinaryReader(stream))
                {
                    file = reader.ReadBytes((int)stream.Length);
                }
            }

            return file;
        }

        public void DeleteFileInsideDirectory(string targetDirectory)
        {
            string[] files = Directory.GetFiles(targetDirectory);
            string[] dirs = Directory.GetDirectories(targetDirectory);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }
        }

        public void DeleteDirectory(string targetDirectory)
        {
            string[] files = Directory.GetFiles(targetDirectory);
            string[] dirs = Directory.GetDirectories(targetDirectory);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(targetDirectory, false);
        }

        private Message NewTextMessage(string text)
        {
            // text message
            Message textMessage = new Message();
            textMessage.setMessageType(Message.messageType.text);
            textMessage.setTextContent(text);

            return textMessage;
        }

        private Message NewFileMessage(string path)
        {
            // file message
            Message fileMessage = new Message();
            fileMessage.setMessageType(Message.messageType.file);
            fileMessage.setFileContentPath(path);

            return fileMessage;
        }

        public void SendMessage(NetworkStream networkStream, Message message)
        {
            Message.messageType type = message.getMessageType();
            switch (type)
            {
                case Message.messageType.text:
                    networkManager.NetWriteTextMsg(networkStream, message.getTextContent());
                    break;

                case Message.messageType.file:
                    networkManager.NetWriteFile(networkStream, message.getFileContentPath());
                    eventManager.event1.Set();
                    eventManager.event3.Set();
                    break;
            }
        }

        private void OperationRegistration()
        {
            int userCreation = int.MinValue;
            bool validPath = false;

            // "200_OK"
            networkManager.NetWriteTextMsg(networkStream, Program.m_200_OK);

            // <- USERNAME
            username = networkManager.NetReadTextMsg(networkStream);
            username = username.Trim();
            fileTableName = username + "_file_table";
            snapshotTableName = username + "_snapshot_table";
            PrintVerboseMessage("server:handshakeThread: >> Username: " + username);

            // -> "200_OK"
            networkManager.NetWriteTextMsg(networkStream, Program.m_200_OK);

            // <- PASSWORD
            password = networkManager.NetReadTextMsg(networkStream);
            password = password.Trim();
            PrintVerboseMessage("server:handshakeThread: >> Password: ********");

            // -> "200_OK"
            networkManager.NetWriteTextMsg(networkStream, Program.m_200_OK);

            // <- BACKUP_PATH
            backupPath = networkManager.NetReadTextMsg(networkStream);
            backupPath = backupPath.Trim();
            validPath = Program.IsValidPath(backupPath);
            PrintVerboseMessage("server:handshakeThread: >> Backup path: " + backupPath);
            if (validPath == false)
            {
                networkManager.NetWriteTextMsg(networkStream, Program.WRONG_PATH_SYNTAX);
                PrintVerboseMessage("server:handshakeThread: >> Error message: " + Program.WRONG_PATH_SYNTAX);
                userLoggedIn = false;
                return;
            }
                        
            userCreation = CreateNewUser(username, password, backupPath);
            if(userCreation != 0)
            {
                userLoggedIn = false;
                if (userCreation == 1)
                {
                    networkManager.NetWriteTextMsg(networkStream, Program.USERNAME_ALREADY_IN_USE);
                    PrintVerboseMessage("server:handshakeThread: >> New user creation failed: " + Program.USERNAME_ALREADY_IN_USE);
                }
                if (userCreation == 2)
                {
                    networkManager.NetWriteTextMsg(networkStream, Program.USERNAME_ALREADY_IN_USE);
                    PrintVerboseMessage("server:handshakeThread: >> New user creation failed: wrong password character set: " + Program.USERNAME_ALREADY_IN_USE);
                }
                if (userCreation == 3)
                {
                    PrintVerboseMessage("server:handshakeThread: >> Database transaction failed");
                    throw new SocketException();
                }
                return;
            }

            networkManager.NetWriteTextMsg(networkStream, Program.SUCCESSFUL_REGISTRATION);
            userLoggedIn = true;
            PrintVerboseMessage("server:handshakeThread: >> Successful registation of user: " + username);
            networkStream.Flush();
        }

        private void OperationLogin()
        {
            // -> 200_OK
            networkManager.NetWriteTextMsg(networkStream, Program.m_200_OK);

            // <- USERNAME
            username = networkManager.NetReadTextMsg(networkStream);
            username = username.Trim();
            this.fileTableName = username + "_file_table";
            this.snapshotTableName = username + "_snapshot_table";
            PrintVerboseMessage("server:handshakeThread: >> Username: " + username);

            // -> 200_OK
            networkManager.NetWriteTextMsg(networkStream, Program.m_200_OK);

            // <- PASSWORD
            password = networkManager.NetReadTextMsg(networkStream);
            PrintVerboseMessage("server:handshakeThread: >> Password: ********");

            bool loginResulr = Login(username, password);
            if (loginResulr == false)
            {
                PrintVerboseMessage("server:handshakeThread: >> Login failed: wrong username or password");
                networkManager.NetWriteTextMsg(networkStream, Program.m_401_Unauthorized);
                userLoggedIn = false;
                return;
            }
           
            // -> 200_OK
            networkManager.NetWriteTextMsg(networkStream, Program.m_200_OK);

            // send client the current path of user
            backupPath = GetBackupPath(username);
            if (backupPath == string.Empty)
            {
                PrintVerboseMessage("server:handshakeThread: >> Server internal error");
                throw new SocketException();
            }
            networkManager.NetWriteTextMsg(networkStream, backupPath);

            userLoggedIn = true;
            PrintVerboseMessage("server:handshakeThread: >> User " + username + " successful logged in");
            networkStream.Flush();
        }

        private void OperationNewFile()
        {
            // "NEW_FILE"
            ProduceMessage(NewTextMessage(Program.ACK));
            PrintVerboseMessage("server:inputThread: >> **********NEW_FILE**********");

            // PATH
            string clientPath = networkManager.NetReadTextMsg(networkStream);
            if (Program.IsValidPath(clientPath) == false)
            {
                ProduceMessage(NewTextMessage(Program.ERROR_INVALID_PATH));
                PrintVerboseMessage("server:inputThread: >> Received path is not valid: " + clientPath);
                return;
            }
            ProduceMessage(NewTextMessage(Program.ACK));
            PrintVerboseMessage("server:inputThread: >> Path on client filesystem: " + clientPath);

            // VERSION (datetime)
            string newFileStrVersion = this.networkManager.NetReadTextMsg(networkStream);
            DateTime newFileVersion = Convert.ToDateTime(newFileStrVersion);
            ProduceMessage(NewTextMessage(Program.ACK));
            PrintVerboseMessage("server:inputThread: >> Version (datetime) of the file: " + newFileVersion);

            // <<FILE>>
            FileCloud newFile = this.networkManager.NetReadFile(networkStream, tmpUserDirPath);

            newFile.setClientPath(clientPath);
            newFile.setVersion(newFileVersion);
            PrintVerboseMessage("server:inputThread: >> Original file name: " + newFile.getOriginalFileName());
            PrintVerboseMessage("server:inputThread: >> Current file name: " + newFile.getFileName());
            PrintVerboseMessage("server:inputThread: >> File size: " + newFile.getFileLength() + " bytes");

            /* SPECIAL CASE MANAGEMENT:
             * if a new file is created with a name and path equals to name and path of a previously deleted file,
             * the new file becomes a new version of that file and the deleted file must be valid again as a
             * previous version of the new one.
             */
            bool fileAlreadyKnown = SetFileAsRestored(clientPath); // the method check by itself if the file alredy exists
            if (fileAlreadyKnown)
            {
                PrintVerboseMessage("server:inputThread: >> File name already known. A deleted version of this file was already present in database");

                // If the number of version is equal to MAX_STORED_VERSION_PER_FILE_SPECIAL_CASE, the oldest versions are deleted
                while (FileExistsInDb(clientPath) > Program.MAX_STORED_VERSION_PER_FILE_SPECIAL_CASE)
                {
                    RemoveOldestVersionOfFile(clientPath);
                }
            }

                // store the received file in DB
                bool addResult = AddFile(newFile);
            if (addResult == false)
            {
                ProduceMessage(NewTextMessage(Program.ERROR_COMMUNICATION));
                PrintVerboseMessage("server:inputThread: >> Error occurred while adding file in db");
                return;
            }
            PrintVerboseMessage("server:inputThread: >> File successfully added to database");
            ProduceMessage(NewTextMessage(Program.m_200_OK));
            PrintVerboseMessage("server:inputThread: >> ****************************");
        }

        private void OperationUpdateFile()
        {
            // "UPDATE_FILE"
            ProduceMessage(NewTextMessage(Program.ACK));
            PrintVerboseMessage("server:inputThread: >> **********UPDATE_FILE**********");

            // PATH
            string updateFilePath = networkManager.NetReadTextMsg(networkStream);
            if (Program.IsValidPath(updateFilePath) == false)
            {
                ProduceMessage(NewTextMessage(Program.ERROR_INVALID_PATH));
                PrintVerboseMessage("server:inputThread: >> Received path is not valid: " + updateFilePath);
                return;
            }
            int versionFileNumber = FileExistsInDb(updateFilePath);
            if (versionFileNumber <= 0)
            {
                ProduceMessage(NewTextMessage(Program.ERROR_INVALID_PATH));
                if(versionFileNumber == -1) PrintVerboseMessage("server:inputThread: >> Server internal error");
                else PrintVerboseMessage("server:inputThread: >> Received path is not in the database: " + updateFilePath);
                return;
            }
            ProduceMessage(NewTextMessage(Program.ACK));
            PrintVerboseMessage("server:inputThread: >> File to update: " + updateFilePath);

            // VERSION
            string updateStrVersion = networkManager.NetReadTextMsg(networkStream);
            DateTime updateFileVersion = Convert.ToDateTime(updateStrVersion);
            ProduceMessage(NewTextMessage(Program.ACK));
            PrintVerboseMessage("server:inputThread: >> New file version is: " + updateStrVersion);

            // <<FILE>>
            FileCloud updatedFile = networkManager.NetReadFile(networkStream, tmpUserDirPath);

            updatedFile.setClientPath(updateFilePath);
            updatedFile.setVersion(updateFileVersion);
            PrintVerboseMessage("server:inputThread: >> Original file name: " + updatedFile.getOriginalFileName());
            PrintVerboseMessage("server:inputThread: >> Current file name: " + updatedFile.getFileName());
            PrintVerboseMessage("server:inputThread: >> File size: " + updatedFile.getFileLength() + " bytes");

            // store the received file in DB
            bool updateResult = AddFile(updatedFile);
            if (updateResult == false)
            {
                ProduceMessage(NewTextMessage(Program.ERROR_COMMUNICATION));
                PrintVerboseMessage("server:inputThread: >> Error occurred while adding new version of file in db");
                return;
            }
            PrintVerboseMessage("server:inputThread: >> Verison of the file successfully added to database");
            ProduceMessage(NewTextMessage(Program.m_200_OK));
            PrintVerboseMessage("server:inputThread: >> *******************************");
        }

        private void OperationDeleteFile()
        {
            // "DELETE_FILE"
            ProduceMessage(NewTextMessage(Program.ACK));
            PrintVerboseMessage("server:inputThread: >> **********DELETE_FILE**********");

            // PATH
            string deleteFilePath = networkManager.NetReadTextMsg(networkStream);
            if (Program.IsValidPath(deleteFilePath) == false)
            {
                ProduceMessage(NewTextMessage(Program.ERROR_INVALID_PATH));
                PrintVerboseMessage("server:inputThread: >> Received path is not valid: " + deleteFilePath);
                return; ;
            }
            int versionFileNumber = FileExistsInDb(deleteFilePath);
            if (versionFileNumber <= 0)
            {
                ProduceMessage(NewTextMessage(Program.ERROR_INVALID_PATH));
                if (versionFileNumber == -1) PrintVerboseMessage("server:inputThread: >> Server internal error");
                else PrintVerboseMessage("server:inputThread: >> Received path is not in the database: " + deleteFilePath);
                return;
            }
            PrintVerboseMessage("server:inputThread: >> File to set as deleted is " + deleteFilePath);

            bool deleteResult = SetFileAsDeleated(deleteFilePath);
            if (deleteResult == false)
            {
                ProduceMessage(NewTextMessage(Program.ERROR_COMMUNICATION));
                PrintVerboseMessage("server:inputThread: >> Failed to delete " + deleteFilePath);
                return;
            }
            ProduceMessage(NewTextMessage(Program.m_200_OK));
            PrintVerboseMessage("server:inputThread: >> File " + deleteFilePath + " successfully set as deleted");
            PrintVerboseMessage("server:inputThread: >> *******************************");
        }

        private void OperationRenameFile()
        {
            // "RENAME_FILE"
            ProduceMessage(NewTextMessage(Program.ACK));
            PrintVerboseMessage("server:inputThread: >> **********RENAME_FILE**********");

            // OLD PATH
            string renameOldPath = networkManager.NetReadTextMsg(networkStream);
            if (Program.IsValidPath(renameOldPath) == false)
            {
                ProduceMessage(NewTextMessage(Program.ERROR_INVALID_PATH));
                PrintVerboseMessage("server:inputThread: >> Received old-path is not valid: " + renameOldPath);
                return;
            }
            int versionFileNumber = FileExistsInDb(renameOldPath);
            if (versionFileNumber <= 0)
            {
                ProduceMessage(NewTextMessage(Program.ERROR_INVALID_PATH));
                if(versionFileNumber == -1) PrintVerboseMessage("server:inputThread: >> Server internal error");
                else PrintVerboseMessage("server:inputThread: >> Received old-path is not in the database: " + renameOldPath);
                return;
            }
            ProduceMessage(NewTextMessage(Program.ACK));
            PrintVerboseMessage("server:inputThread: >> File to rename is " + renameOldPath);

            // NEW PATH
            string renameNewPath = networkManager.NetReadTextMsg(networkStream);
            if (Program.IsValidPath(renameNewPath) == false)
            {
                ProduceMessage(NewTextMessage(Program.ERROR_INVALID_PATH));
                PrintVerboseMessage("server:inputThread: >> Received old-path is not valid: " + renameNewPath);
                return;
            }
            ProduceMessage(NewTextMessage(Program.ACK));
            PrintVerboseMessage("server:inputThread: >> New name of the file is " + renameNewPath);

            // VERSION
            string renameStrVersion = networkManager.NetReadTextMsg(networkStream);
            DateTime renameFileVersion = Convert.ToDateTime(renameStrVersion);
            PrintVerboseMessage("server:inputThread: >> New file version is " + renameStrVersion);

            // rename all file with oldPath
            bool renameResult = RenameFile(renameOldPath, renameNewPath);
            if (renameResult == false)
            {
                ProduceMessage(NewTextMessage(Program.ERROR_INVALID_PATH));
                PrintVerboseMessage("server:inputThread: >> Failed to rename " + renameOldPath);
                return;
            }

            // update the version of the most recent file in database
            renameResult = UpdateLastVersionOFFile(renameNewPath, renameFileVersion);
            if (renameResult == false)
            {
                ProduceMessage(NewTextMessage(Program.ERROR_INVALID_PATH));
                PrintVerboseMessage("server:inputThread: >> Failed to update version of " + renameNewPath);
                return;
            }

            /* SPECIAL CASE MANAGEMENT:
             * if a file is renamed with a name and path equals to name and path of a previously deleted file,
             * the file becomes a new version of the deleted file and the deleted file must be valid again as a
             * previous version of the new one.
             */
            bool fileAlreadyKnown = SetFileAsRestored(renameNewPath); // the method check by itself if the file alredy exists
            if (fileAlreadyKnown)
            {
                PrintVerboseMessage("server:inputThread: >> File name already known. A deleted version of this file was already present in database");

                // If the number of version is equal to MAX_STORED_VERSION_PER_FILE_SPECIAL_CASE, the oldest versions are deleted
                while (FileExistsInDb(renameNewPath) > Program.MAX_STORED_VERSION_PER_FILE_SPECIAL_CASE)
                {
                    RemoveOldestVersionOfFile(renameNewPath);
                }
            }

                ProduceMessage(NewTextMessage(Program.m_200_OK));
            PrintVerboseMessage("server:inputThread: >> File " + renameOldPath + " successfully renamed in " + renameNewPath);
            PrintVerboseMessage("server:inputThread: >> *******************************");
        }

        private void OperationRestoreFile()
        {
            // "RESTORE_FILE"
            ProduceMessage(NewTextMessage(Program.ACK));
            PrintVerboseMessage("server:inputThread: >> **********RESTORE_FILE**********");

            // PATH
            string restoreFilePath = networkManager.NetReadTextMsg(networkStream);
            if (Program.IsValidPath(restoreFilePath) == false)
            {
                ProduceMessage(NewTextMessage(Program.ERROR_INVALID_PATH));
                PrintVerboseMessage("server:inputThread: >> Received path is not valid: " + restoreFilePath);
                return;
            }
            int versionFileNumber = FileExistsInDb(restoreFilePath);
            if (versionFileNumber <= 0)
            {
                ProduceMessage(NewTextMessage(Program.ERROR_INVALID_PATH));
                if(versionFileNumber == -1) PrintVerboseMessage("server:inputThread: >> Server internal error");
                else PrintVerboseMessage("server:inputThread: >> Received path is not in the database: " + restoreFilePath);
                return;
            }
            ProduceMessage(NewTextMessage(Program.ACK));
            PrintVerboseMessage("server:inputThread: >> Client requests to restore " + restoreFilePath);

            // VERSION o "LAST_VERSION"
            string restoreStrVersion = networkManager.NetReadTextMsg(networkStream);
            string restoredLocalPath = string.Empty;
            if (restoreStrVersion == Program.LAST_VERSION)
            {
                PrintVerboseMessage("server:inputThread: >> Client requests to restore last version of the file");
                restoreStrVersion = GetLastVersionOfFile(restoreFilePath);
                if(restoreStrVersion == string.Empty)
                {
                    PrintVerboseMessage("server:inputThread: >> Server internal error");
                    return;
                }
            }
            PrintVerboseMessage("server:inputThread: >> Version to restore is " + restoreStrVersion);

            // get selected version of the file from db
            restoredLocalPath = GetFileFromVersion(restoreFilePath, restoreStrVersion);
            if (restoredLocalPath == Program.ERROR_FILE_CREATION)
            {
                ProduceMessage(NewTextMessage(Program.ERROR_COMMUNICATION));
                PrintVerboseMessage("server:inputThread: >> Requested version is not in the database: " + restoreFilePath);
                return;
            }
            PrintVerboseMessage("server:inputThread: >> Version to restore successfully read from database");

            // "RESTORE_FILE" to warn the client about the incoming file
            ProduceMessage(NewTextMessage(Program.RESTORE_FILE));
            string ack = string.Empty;
            ack = networkManager.NetReadTextMsg(networkStream);
            if (ack != Program.ACK)
            {
                ProduceMessage(NewTextMessage(Program.ERROR_COMMUNICATION));
                PrintVerboseMessage("server:inputThread: >> Communication error: missing ACK message");
                return;
            }

            // VERSION to warn the client about the incoming file
            ProduceMessage(NewTextMessage(restoreStrVersion));
            ack = string.Empty;
            ack = networkManager.NetReadTextMsg(networkStream);
            if (ack != Program.ACK)
            {
                ProduceMessage(NewTextMessage(Program.ERROR_COMMUNICATION));
                PrintVerboseMessage("server:inputThread: >> Communication error: missing ACK message");
                return;
            }

            // <<FILE>>
            eventManager.event1.Reset();
            ProduceMessage(NewFileMessage(restoredLocalPath));
            WaitHandle.WaitAll(new WaitHandle[] { eventManager.event1 });
            //PrintDebugMessage("server:inputThread: >> Wait unlocked");
            ack = string.Empty;
            ack = networkManager.NetReadTextMsg(networkStream);
            if (ack != Program.ACK)
            {
                ProduceMessage(NewTextMessage(Program.ERROR_COMMUNICATION));
                PrintVerboseMessage("server:inputThread: >> Communication error: missing ACK message");
                return;
            }
            PrintVerboseMessage("server:inputThread: >> File " + restoreFilePath + " successfully sent to client");

            // set file as restored in db
            bool restoreResult = SetFileAsRestored(restoreFilePath);
            if (restoreResult == false)
            {
                ProduceMessage(NewTextMessage(Program.ERROR_COMMUNICATION));
                PrintVerboseMessage("server:inputThread: >> Failed to db restore " + restoreFilePath);
                return;
            }
            PrintVerboseMessage("server:inputThread: >> File " + restoreFilePath + " successfully set again as valid in database");

            ProduceMessage(NewTextMessage(Program.m_200_OK));
            PrintVerboseMessage("server:inputThread: >> File " + restoreFilePath + " successfully restored");
            PrintVerboseMessage("server:inputThread: >> ********************************");
        }

        private void OperationOldVersionFile()
        {
            // "OLD_VERSION_FILE"
            ProduceMessage(NewTextMessage(Program.ACK));
            PrintVerboseMessage("server:inputThread: >> **********OLD_VERSION_FILE**********");

            // PATH
            string oldVersionFilePath = networkManager.NetReadTextMsg(networkStream);
            if (Program.IsValidPath(oldVersionFilePath) == false)
            {
                ProduceMessage(NewTextMessage(Program.ERROR_INVALID_PATH));
                PrintVerboseMessage("server:inputThread: >> Received path is not valid: " + oldVersionFilePath);
                return;
            }
            int versionFileNumber = FileExistsInDb(oldVersionFilePath);
            if (versionFileNumber <= 0)
            {
                ProduceMessage(NewTextMessage(Program.ERROR_INVALID_PATH));
                if(versionFileNumber == -1) PrintVerboseMessage("server:inputThread: >> Server internal error");
                else PrintVerboseMessage("server:inputThread: >> Received path is not in the database: " + oldVersionFilePath);
                return;
            }
            PrintVerboseMessage("server:inputThread: >> Requested old available versions of file " + oldVersionFilePath);

            // get old versions list for requested file from db
            List<string> oldVersionsList = GetOldVersionsOfFileList(oldVersionFilePath);

            StringBuilder sb = new StringBuilder();
            foreach (string str in oldVersionsList)
            {
                sb.Append(str);
                sb.Append("#");
            }
            ProduceMessage(NewTextMessage(sb.ToString()));
            PrintVerboseMessage("server:inputThread: >> Available old versions are: " + sb.ToString());

            string ack = string.Empty;
            ack = networkManager.NetReadTextMsg(networkStream);
            if (ack != Program.ACK)
            {
                ProduceMessage(NewTextMessage(Program.ERROR_COMMUNICATION));
                PrintVerboseMessage("server:inputThread: >> Communication error: missing ACK message");
                return;
            }
            ProduceMessage(NewTextMessage(Program.m_200_OK));
            PrintVerboseMessage("server:inputThread: >> Old versions list of " + oldVersionFilePath + " file successfully sent to client");
            PrintVerboseMessage("server:inputThread: >> ************************************");
        }

        private void OperationCompleteSnapshot()
        {
            // "COMPLETE_SNAPSHOT"
            PrintVerboseMessage("server:inputThread: >> **********COMPLETE_SNAPSHOT**********");

            // NUMBER OF FILES
            List<string> fileList = CalculateSnapshotListFromDb();
            int numberOfFile = fileList.Count;
            ProduceMessage(NewTextMessage(numberOfFile.ToString()));
            string ack = string.Empty;
            ack = networkManager.NetReadTextMsg(networkStream);
            if (ack != Program.ACK)
            {
                ProduceMessage(NewTextMessage(Program.ERROR_COMMUNICATION));
                PrintVerboseMessage("server:inputThread: >> Communication error: missing ACK message");
                return;
            }
            PrintVerboseMessage("server:inputThread: >> Database contains " + numberOfFile + " different files for user " + username);

            // ROOT DIRECTORY
            string userRootDir = GetBackupPath(username);
            ProduceMessage(NewTextMessage(userRootDir));
            ack = string.Empty;
            ack = networkManager.NetReadTextMsg(networkStream);
            if (ack != Program.ACK)
            {
                ProduceMessage(NewTextMessage(Program.ERROR_COMMUNICATION));
                PrintVerboseMessage("server:inputThread: >> Communication error: missing ACK message");
                return;
            }
            PrintVerboseMessage("server:inputThread: >> Root directory of " + username + "'s backup path is " + userRootDir);

            // string structure inside list: "currentPath*version"
            foreach (string s in fileList)
            {
                // "COMPLETE_SNAPSHOT"
                ProduceMessage(NewTextMessage(Program.COMPLETE_SNAPSHOT));
                ack = string.Empty;
                ack = networkManager.NetReadTextMsg(networkStream);
                if (ack != Program.ACK)
                {
                    ProduceMessage(NewTextMessage(Program.ERROR_COMMUNICATION));
                    PrintVerboseMessage("server:inputThread: >> Communication error: missing ACK message");
                    return;
                }

                char[] delimiterChars = { '*' };
                string[] splitted = s.Split(delimiterChars);

                // PATH
                string currentFilePath = splitted[0]; // element [0] = file path
                PrintDebugMessage("server:inputThread: >> splitted file path: " + currentFilePath);
                ProduceMessage(NewTextMessage(currentFilePath));
                ack = string.Empty;
                ack = networkManager.NetReadTextMsg(networkStream);
                if (ack != Program.ACK)
                {
                    ProduceMessage(NewTextMessage(Program.ERROR_COMMUNICATION));
                    PrintVerboseMessage("server:inputThread: >> Communication error: missing ACK message");
                    return;
                }

                // VERSION
                string currentFileVersionStr = splitted[1]; // element [1] = file version
                PrintDebugMessage("server:inputThread: >> splitted file version: " + currentFileVersionStr);
                ProduceMessage(NewTextMessage(currentFileVersionStr));
                ack = string.Empty;
                ack = networkManager.NetReadTextMsg(networkStream);
                if (ack != Program.ACK)
                {
                    ProduceMessage(NewTextMessage(Program.ERROR_COMMUNICATION));
                    PrintVerboseMessage("server:inputThread: >> Communication error: missing ACK message");
                    return;
                }

                // <<FILE>>
                string currentLocalPath = GetFileFromVersion(currentFilePath, currentFileVersionStr);
                if (currentLocalPath == Program.ERROR_FILE_CREATION)
                {
                    ProduceMessage(NewTextMessage(Program.ERROR_COMMUNICATION));
                    PrintVerboseMessage("server:inputThread: >> Requested version is not in the database: " + currentFilePath);
                    return;
                }
                PrintVerboseMessage("server:inputThread: >> " + Path.GetFileName(currentFilePath) + " successfully read from database");

                eventManager.event3.Reset();
                ProduceMessage(NewFileMessage(currentLocalPath));
                WaitHandle.WaitAll(new WaitHandle[] { eventManager.event3 });
                PrintDebugMessage("server:inputThread: >> Wait unlocked");
                ack = string.Empty;
                ack = networkManager.NetReadTextMsg(networkStream);
                if (ack != Program.ACK)
                {
                    ProduceMessage(NewTextMessage(Program.ERROR_COMMUNICATION));
                    PrintVerboseMessage("server:inputThread: >> Communication error: missing ACK message");
                    return;
                }
            }

            ProduceMessage(NewTextMessage(Program.m_200_OK));
            PrintVerboseMessage("server:inputThread: >> *************************************");
        }

        private void OperationLogout()
        {
            ProduceMessage(NewTextMessage(Program.m_200_OK));
            PrintVerboseMessage("server:" + Thread.CurrentThread.Name + ": >> Logout requested from " + username);
            Logout();
        }
  
    }
}
