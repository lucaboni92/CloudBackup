using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;

namespace CloudBackupClient
{
    public class HandleClient
    {
        public static DateTime lastRead;
        public static string lastHash;
        private const int NumberOfRetries = 3;
        private const int DelayOnRetry = 500;
        private FileSystemWatcher watcher;
        private volatile bool stopInputThread;
        private volatile bool stopOutputThread;
        private volatile bool inputThreadWait; //for the synchro.
        private volatile bool outputThreadWait; //for the synchro.
        private string path_Shared; //mutual exclusion variables
        readonly object myObj; //synchro object (used by the lock and the monitors)
        static object raiseObj;
        private Dictionary<string, FileModel> oldDirectories;
        private Dictionary<string, FileModel> oldFiles;
        private List<string> deletedFileList;
        private static List<string> restoredFile;
        private List<string> oldVersion;
        private static bool verbose;
        private string backupPath;
        private UiMessage uiRestoreFile;
        private UiMessage uiSnapFile;
        private UiMessage uiDelMessage;
        private UiMessage uiVerFile;
        private TcpClient clientSocket;
        private NetworkManager networkManager;
        private NetworkStream networkStream;
        private static Mutex mut;
        private static int size;
        private static int front;
        private static int rear = 0;
        private static FileModel[] events;
        private static Semaphore empty;
        private static Semaphore full;
        internal const string CLIENTPATH = @"C:\client\";
        internal const string CLIENTPATH_VERSIONLIST = @"C:\client\versionList\";
        private bool backUpNotFound;
        private string filecount;
        private bool connectionStatus;
        private int fileNumShared;
        private static int contFile;

        public HandleClient(TcpClient clientSocket, NetworkStream networkStream, NetworkManager networkManager, string backupPath)
        { 
            this.clientSocket = clientSocket;
            raiseObj = new object();
            this.networkStream = networkStream;
            this.networkManager = networkManager;
            this.backupPath = backupPath;
            if (Directory.Exists(backupPath))
                backUpNotFound = false;
            else backUpNotFound = true;
            oldVersion = new List<string>();
            fileNumShared = 0;
            contFile = 0;
            connectionStatus = true;
            oldDirectories = new Dictionary<string, FileModel>();
            oldFiles = new Dictionary<string, FileModel>();
            deletedFileList = new List<string>();
            restoredFile = new List<string>();
            lastRead = DateTime.MinValue;
            lastHash = String.Empty;
            uiDelMessage = new UiMessage();
            uiSnapFile = new UiMessage();
            uiRestoreFile = new UiMessage();
            uiVerFile = new UiMessage();
            stopInputThread = false;
            stopOutputThread = false;
            inputThreadWait = false; //wait from the beginning
            outputThreadWait = false; //do work from the beginning
            myObj = new object(); //synchro object
            verbose = true;
            mut = new Mutex();
            size = 50;
            front = 0;
            rear = 0;
            events = new FileModel[size];
            empty = new Semaphore(size, size);
            full = new Semaphore(0, size);
        }

   
        //second part of the handshake
        internal void doHandshake()
        {
        try { 
            try
            {
                //****************SNAPSHOT*************************   
                DirectoryInfo clientDir = new DirectoryInfo(CLIENTPATH);
                if (clientDir.Exists == false) clientDir.Create();

                /*receive first message from server: LAST_SNAPSHOT || LAST_SNAPSHOT_NOT_FOUND*/
                string operation = networkManager.NetReadTextMsg(networkStream);
                Model.PrintHandshakeMessage("Received from server: " + operation);
                /*create a snapshotMangager object and obtain the current snapshot of the filesystem*/
                HandleSnapshot(clientDir, operation);

                //****************LIST OF DELETED FILES*************************   
                DirectoryInfo fileDeleteList = clientDir.CreateSubdirectory("deleteFileList");
                if (fileDeleteList.Exists == false) fileDeleteList.Create();

                /*receive first message from server: DELETE_FILE_LIST || DELETE_FILE_LIST_EMPTY*/
                string operation2 = networkManager.NetReadTextMsg(networkStream);
                Model.PrintHandshakeMessage("Received from server: " + operation2);
                handleDeleteFilesList(fileDeleteList, operation2);

                /*start the client and the two threads*/
                startClient();

            }
            catch (IOException e)
            {
                Console.WriteLine("client:Exception >> IOException: " + e.Message);
                throw;
            }
            catch (SocketException e)
            {
                Console.WriteLine("client:Exception >> SocketException: " + e.Message);
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine("client:Exception >> Execution terminated because of exception: " + e.Message);
                throw;
            }
        }
            catch (Exception e)
            {
                Console.WriteLine("client:Exception >> Execution terminated because of exception: " + e.Message);
                //close the connection and alert the view
                networkStream.Flush();
                networkStream.Close();
                handleException();
            }
}

        // this method creates the first thread for input comm.
        internal void startClient()
        {
            try
            {
                if (!backUpNotFound)
                    startSystemWatcher();
                Thread inputServerThread = new Thread(doInputCommunication);
                inputServerThread.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine("client:Exception >> Execution terminated because of exception: " + e.Message);
                //close the connection and alert the view
                networkStream.Flush();
                networkStream.Close();
                handleException();
            }
        }
        //input thread for messages coming from server
        private void doInputCommunication()
        {
            string answer = String.Empty;
            try
            {
                answer = networkManager.NetReadTextMsg(networkStream);
            }
            catch (Exception e)
            {
                Console.WriteLine("client:Exception >> Execution terminated because of exception: " + e.Message);
                //close the connection and alert the view
                networkStream.Flush();
                networkStream.Close();
                handleException();
            }

            if (answer != Program.READY && !String.IsNullOrEmpty(answer))
                {
                    networkManager.NetWriteTextMsg(networkStream, Program.ERROR_COMMUNICATION);
                    handleException();
                    //TODO
                }
            
           
            else
            {
                //set the output thread and invokes the function
                try
                {
                    Thread outputServerThread = new Thread(doOutputCommunication);
                    outputServerThread.Start();
                }
                catch(Exception e)
                {
                    Console.WriteLine("client:Exception >> Execution terminated because of exception: " + e.Message);
                    //close the connection and alert the view
                    networkStream.Flush();
                    networkStream.Close();
                    handleException();
                }
                inputThreadWait = true;
                while (!stopInputThread)
                {
                    try
                    {
                        lock (myObj)
                        {
                            //the input waits for a signaling from the output
                            if (inputThreadWait)
                                Monitor.Wait(myObj);
                            Model.PrintInputMessage("Input thread activated");
                            if (!stopInputThread)
                            {
                                answer = networkManager.NetReadTextMsg(networkStream);
                                switch (answer)
                                {
                                    case Program.COMPLETE_SNAPSHOT:
                                        input_CompleteSnapshot();
                                        break;

                                    case Program.RESTORE_FILE:
                                        input_RestoreFile();
                                        break;

                                    case Program._200_OK:
                                        input_200_OK();
                                        break;

                                    default:
                                        input_default(answer);
                                        break;

                                }//end switch
                            }//end lock
                        }//end if
                    }//end try
                    catch (SocketException)
                    {
                    Console.WriteLine("client:inputThread: >> Some network problem occurred");
                        handleException();
                        return;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("client:Exception >> Execution terminated because of exception: " + e.Message);
                        //close the connection and alert the view
                        networkStream.Flush();
                        networkStream.Close();
                        handleException();
                        return;
                    }
                }//end while
                Model.PrintInputMessage("closing Thread input");
            }//end if
        }

        //method for output communication (thread method)
        private void doOutputCommunication()
        {          
            while (!stopOutputThread)
            {
                Model.PrintOutputMessage("Output thread waiting for new events");
                /*check for a new event. Some threads insert the events in an array, 
                this thread extract an event in a safety way.*/
               FileModel e=  checkNewEvents();
                string type = e.GetChangeType();
                lock (myObj) //lock the whole area for synchro
                {
                    if (outputThreadWait)
                        Monitor.Wait(myObj);
                    try
                    {
                        networkManager.NetWriteTextMsg(networkStream, type);
                        string fmServer = this.networkManager.NetReadTextMsg(networkStream);
                        Model.PrintOutputMessage("Received: " + fmServer);
                        if (serverAck(fmServer, e))
                        {
                            String answer;
                            switch (type)
                            {

                                case Program.NEW_FILE:
                                    answer = String.Empty;
                                    output_newFile(answer, e);
                                    break;

                                case Program.DELETE_FILE:
                                    answer = String.Empty;
                                    output_DeleteFile(answer, e);
                                    break;

                                case Program.UPDATE_FILE:
                                    answer = String.Empty;
                                    output_UpdateFile(answer, e);
                                    break;

                                case Program.RENAME_FILE:
                                    answer = String.Empty;
                                    output_RenameFile(answer, e);
                                    break;

                                case Program.RESTORE_FILE:
                                    answer = String.Empty;
                                    output_RestoreFile(answer, e);
                                    break;

                                case Program.OLD_VERSION_FILE:
                                    answer = String.Empty;
                                    output_OldVersionFile(answer, e);
                                    break;

                                case Program.COMPLETE_SNAPSHOT:
                                    answer = String.Empty;
                                    output_CompleteSnapshot(answer, fmServer, e);
                                    break;

                                case Program.LOGOUT:
                                    output_LogOut();
                                    break;
                                default:
                                    Model.PrintOutputMessage("Change " + e.GetChangeType() + " Not Handled");
                                    break;
                            }//end switch
                        }
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine("client:SocketException: >> Some network problem occurred");
                        networkStream.Flush();
                        networkStream.Close();
                        handleException();
                        return;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("client:Exception >> Execution terminated because of exception: " + ex.Message);
                        //close the connection and alert the view
                        networkStream.Flush();
                        networkStream.Close();
                        handleException();
                        return;
                    } 
                }//end lock
            }//end while
            Model.PrintOutputMessage("closing thread output");
        }

        private FileModel checkNewEvents()
        {
            full.WaitOne();
            Model.PrintOutputMessage("leggo nel buffer in posizione " + rear);
            FileModel e = events[rear];
            rear = (rear + 1) % size;
            empty.Release();
            return e;
        }

        /*clear all the temporary files and structure*/
        private void clear()
        {
            //clear folders
            clearFolder(CLIENTPATH);
            
        }

        /*delete temporary files*/
        private void clearFolder(string folderName)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(folderName);


                foreach (FileInfo fi in dir.GetFiles())
                {
                    fi.Delete();
                }

                foreach (DirectoryInfo di in dir.GetDirectories())
                {
                    clearFolder(di.FullName);
                    di.Delete();
                }
                Model.PrintOutputMessage("Folder and temporary files deleted");
            }
            catch (Exception e)
            {
                Console.WriteLine("Client::Message >> Risorse già liberate o non liberabili.");   
            }
        }

        internal UiMessage getUiObject(string type)
        {
            switch (type)
            {
                case Program.UI_DELETE_FILE:
                    return uiDelMessage;
                case Program.UI_LAST_SNAPSHOT_FILE:
                    return uiSnapFile;
                case Program.UI_RESTORE_FILE:
                    return uiRestoreFile;
                case Program.UI_SHOW_VERSION:
                    return uiVerFile;
            }
            return null;
        }
        internal void resetUiObject(string type)
        {
            switch (type)
            {
                case Program.UI_DELETE_FILE:
                    uiDelMessage.reset();
                    break;
                case Program.UI_LAST_SNAPSHOT_FILE:
                    uiSnapFile.reset();
                    break;
                case Program.UI_RESTORE_FILE:
                    uiRestoreFile.reset();
                    break;
                case Program.UI_SHOW_VERSION:
                    uiVerFile.reset();
                    break;
            }
        }
        /*
* Restituisce una lista di stringhe , una entry per ogni valore presente nella stringa
* Formato Stringa: version#version#version#
*/

        private List<String> getListOfVersion(string versionList, string currentVersion)
        {
            List<String> list = new List<String>();
            Char delimiter = '#';

            String[] substrings = versionList.Split(delimiter);

        
                foreach (string strVersion in substrings)
                {
                if (!String.IsNullOrEmpty(strVersion) && (DateTime.Compare(Convert.ToDateTime(strVersion), Convert.ToDateTime(currentVersion)) != 0))
                    list.Add(strVersion);
                }         
            return list;
        }
        /*
         * This method handles the answers that comes from the server and return true both for ack and 200OK.
         * Otherwise it reacts to error messages.
         */
        private bool serverAck(string type, FileModel fm)
        {
            switch (type)
            {
                case(Program.ACK) :
                    Model.PrintOutputMessage("Ack received from server");
                    return true;
                case (Program._200_OK):

                    return true;

                case (Program.ERROR_INVALID_PATH):
                    if (fm.getCont() <= 2)
                    {
                        fm.incCont();
                        Model.PrintOutputMessage("Communication Error, raise again the event.");
                        raiseChangeEvent(fm);
                    }
                    else
                    {
                        Model.PrintOutputMessage("Invalid Path, the element has not a valid path, thereby is discarded");
                    }
                    return false;

                case (Program.ERROR_COMMUNICATION):

                    if (fm.getCont() <= 2)
                    {
                        fm.incCont();
                        Model.PrintOutputMessage("Communication Error, raise again the event.");
                        raiseChangeEvent(fm);
                    }else
                    {
                        Model.PrintOutputMessage("Communication Error, impossible to send the element. Connection with server closed");
                        connectionStatus = false;
                    }
                    return false;

                default:
                    if (fm.GetChangeType() == Program.COMPLETE_SNAPSHOT) return true;
                    Model.PrintOutputMessage("Type "+ type + "Not known");
                    return false;
            }
      
        }
        internal bool getBackUpPathBool()
        {
            return backUpNotFound;
        }
        internal bool getConnectionStatus()
        {
            return connectionStatus;
        }
        
        /********communication with the view************/
        internal Dictionary<string, FileModel> getLastSnapshot()
        {
            
            return oldFiles;
        }
        internal List<string> getList(string type)
        {
            if (type == Program.DELETED_LIST)
                return deletedFileList;
            else
                return oldVersion;
        }
        private void stopInputCommunication()
        {
            if (!stopInputThread)
            {
                stopInputThread = true;
                if (inputThreadWait)
                {
                    inputThreadWait = false;
                    Monitor.PulseAll(myObj);
                }
              
            }
        }
        private void stopOutputCommunication()
        {
            if (!stopOutputThread)
            {
                stopOutputThread = true;
                if (outputThreadWait)
                {
                    outputThreadWait = false;
                    Monitor.PulseAll(myObj);
                }
            }
        }
        internal void startSystemWatcher()
        {
            //this method look for any change in the specific path and print it.

            try
            {
                // Create a new FileSystemWatcher and set its properties.
                watcher = new FileSystemWatcher();
                watcher.Path = backupPath;

                // Watch both files and subdirectories.
                watcher.IncludeSubdirectories = true;
               
                // Watch for the changes specified in some of the NotifyFilters
                //enumeration.
                watcher.NotifyFilter =
                NotifyFilters.FileName |
                NotifyFilters.Size;

                // Watch all files.
                watcher.Filter = "*.*";

                // Add event handlers.
                watcher.Changed += new FileSystemEventHandler(OnChanged);
                watcher.Created += new FileSystemEventHandler(OnChanged);
                watcher.Deleted += new FileSystemEventHandler(OnChanged);
                watcher.Renamed += new RenamedEventHandler(OnRenamed);

                //Start monitoring.
                watcher.EnableRaisingEvents = true;

            }
            catch (IOException e)
            {
                Console.WriteLine("An Exception Occurred :" + e);
            }

            catch (Exception oe)
            {
                Console.WriteLine("An Exception Occurred :" + oe);
            }
        }

        /*raise renamed files event*/
        public static void OnRenamed(object source, RenamedEventArgs e)
        {
            FileModel f = createFileModel(WatcherChangeTypes.Renamed, e.FullPath, e.OldFullPath);
            raiseChangeEvent(f);
        }

        /*raise changes event (new-deleted-updated)*/
        public static void OnChanged(object source, FileSystemEventArgs e)
        {  
            lock (raiseObj)
            {    
                if (raiseEvent(e.ChangeType, e.FullPath))
                {
                    FileModel f = createFileModel(e.ChangeType, e.FullPath);
                    raiseChangeEvent(f);
                }
            }
        }

        /*true if we need to signal the event, false otherwise*/
        private static bool raiseEvent(WatcherChangeTypes changeType, string fullPath)
        {
            if (changeType != WatcherChangeTypes.Deleted)
            {
                for (int i = 1; i <= NumberOfRetries; ++i)
                {
                    try
                    {
                        string hash = GetSHA1Hash(fullPath);
                        if (changeType == WatcherChangeTypes.Changed && hash == lastHash) return false;
                        lastHash = hash;
                        break; 
                    }
                    catch (IOException e)
                    {
                        if (i == NumberOfRetries) 
                            throw;

                        Thread.Sleep(DelayOnRetry);
                    }
                }
            }
            return true;
        }

        public static string GetSHA1Hash(string filePath)
        {
            lock (raiseObj)
            {
                using (var sha1 = new SHA1CryptoServiceProvider())
                    return GetHash(filePath, sha1);
            }
        }

        private static string GetHash(string filePath, HashAlgorithm hasher)
        {
            string result;
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {

                var hash = hasher.ComputeHash(fs);
                var hashStr = Convert.ToBase64String(hash);
                result = hashStr.TrimEnd('=');
            }
            return result;
        }

        /*create a file model object */
        private static FileModel createFileModel(WatcherChangeTypes changeType, string fullPath)
        {
            //it does not raise exceptions.
            FileInfo fi = new FileInfo(fullPath);          
            string lastMod =  fi.LastWriteTime.ToString();  
            string typeOfChange = getChangeType(changeType);         
            FileModel f = new FileModel(fullPath, lastMod, typeOfChange);
            return f;
        }
        /*create a file model object-overr. */
        private static FileModel createFileModel(WatcherChangeTypes changeType, string fullPath,string oldPath)
        {
            //it does not raise exceptions.
            FileInfo fi = new FileInfo(fullPath);
            string lastMod = fi.LastWriteTime.ToString();      
            string typeOfChange = getChangeType(changeType);
            FileModel f = new FileModel(fullPath, lastMod, typeOfChange, oldPath);
            return f;
        }
        /*return the file model change type*/
        private static string getChangeType(WatcherChangeTypes type)
        {
            switch (type)
            {
                case WatcherChangeTypes.Changed:
                    return Program.UPDATE_FILE;

                case WatcherChangeTypes.Created:
                    return Program.NEW_FILE;

                case WatcherChangeTypes.Deleted:
                    return Program.DELETE_FILE;

                case WatcherChangeTypes.Renamed:
                    return Program.RENAME_FILE;

                default:
                    return Program.NO_CHANGED;
            }
        }  
        /*this method ask for previous version of the path received as a parameter*/
        internal void oldVersionFile(string fileToBeRestored)
        {
            string time;
            if (File.Exists(fileToBeRestored)) time = File.GetLastWriteTime(fileToBeRestored).ToString();
            else time = Program.LAST_VERSION;
            FileModel f = new FileModel(fileToBeRestored, time, Program.OLD_VERSION_FILE);
            raiseChangeEvent(f);
        }
        /*this method raise an event of the type "RESTORE_FILE" and it is used both for old version and deleted files*/
        internal void restoreFile(string fileToBeRestored, string version)
        {
            FileModel f = new FileModel(fileToBeRestored, version, Program.RESTORE_FILE);
            raiseChangeEvent(f);
        }
        internal void completeSnapshot()
        {
            FileModel f = new FileModel(Program.COMPLETE_SNAPSHOT);
            raiseChangeEvent(f);
        }
        internal void stopSystemWatcher()
        {
            watcher.EnableRaisingEvents = false;
        }

        private static bool compareFiles(Dictionary<string, FileModel> files, Dictionary<string, FileModel> newFiles)
        {
            bool output = true;
            //CHECK DELETIONS AND MODIFICATIONS

            foreach (KeyValuePair<String, FileModel> f in files)
            {
                if (newFiles.ContainsKey(f.Key))
                {
                    //check if the data is the same
                    FileModel value;
                    if (newFiles.TryGetValue(f.Key, out value))
                    {
                        if (!f.Value.EqualsTo(value))
                        {
                            FileModel fm = f.Value;       
                            fm.SetChangeType(getChangeType(WatcherChangeTypes.Changed));
                          
                            raiseChangeEvent(fm);
                           
                            output = false;
                        }
                    }

                }
                else
                {
                    FileModel fm = f.Value;
                    fm.SetChangeType(getChangeType(WatcherChangeTypes.Deleted));                    
                    raiseChangeEvent(fm);
                    output = false;
                }

            }

            //CHECK ADDITIONS
            foreach (KeyValuePair<String, FileModel> f in newFiles)
            {
                if (!files.ContainsKey(f.Key))
                {
                    FileModel fm = f.Value;
                    fm.SetChangeType(getChangeType(WatcherChangeTypes.Created));      
                    raiseChangeEvent(fm);
                    output = false;
                }
            }
            return output;
        }
        private static void raiseChangeEvent(FileModel filemodel)
        {
            FileModel fileChanged = filemodel;
            empty.WaitOne();
            mut.WaitOne();  
            events[front] = fileChanged;
            front = (front + 1) % size;
            mut.ReleaseMutex();
            full.Release();

        }
        /* This method takes a list of items inside a file (one item per line) and generates the list*/
        private List<string> convertToFile(string[] deletedFiles)
        {
            List<string> LogList = new List<string>(deletedFiles);
            return LogList;
        }

        private void createEventForNewSnapshot(Dictionary<string, FileModel> newFiles)
        {

            foreach (KeyValuePair<String, FileModel> f in newFiles)
            {
                FileModel fm = f.Value;
                fm.SetChangeType(getChangeType(WatcherChangeTypes.Created));
                raiseChangeEvent(fm);
            }
        }
        /*handle the log Out*/
        internal void logOut()
        {
            //stop monitoring the folder
            if (!backUpNotFound)
                watcher.EnableRaisingEvents = false;
            FileModel f = new FileModel(Program.LOGOUT);
            raiseChangeEvent(f);
        }

        internal void handleException()
        {
            connectionStatus = false;
            if (watcher != null)
            {
                if (!backUpNotFound || watcher.EnableRaisingEvents)
                    watcher.EnableRaisingEvents = false;
            }
            if (!stopInputThread)
            {
                stopInputCommunication();
            }
            if (!stopOutputThread)
            {
                outputThreadWait = false;
                stopOutputCommunication();
            }
            clear();
            Console.WriteLine("tutte le risorse correttamente rilasciate");
            
        }

        //***********************OUTPUT METHODS*****************************************
        private void output_LogOut()
        {
            Model.PrintOutputMessage("case: ***** Log Out *****");
            stopInputCommunication();
            stopOutputCommunication();
            clear();
        }

        private void output_newFile(string answer, FileModel e)
        {
            Model.PrintOutputMessage("case: ***** File Created *****");
            Model.PrintOutputMessage("Path is " + e.GetFullPath());
            networkManager.NetWriteTextMsg(networkStream, e.GetFullPath());
            /*check if the path is the same as one of the path in the delete list, in that case remove it*/
            answer = networkManager.NetReadTextMsg(networkStream);
            if (serverAck(answer, e))
            {
                networkManager.NetWriteTextMsg(networkStream, e.GetLastModif());
                answer = networkManager.NetReadTextMsg(networkStream);
                networkManager.NetWriteFile(networkStream, e.GetFullPath());
                answer = networkManager.NetReadTextMsg(networkStream);
                if (serverAck(answer, e))
                {
                    Model.PrintOutputMessage("Server answer with " + answer);
                    if (deletedFileList.Contains(e.GetFullPath())) deletedFileList.Remove(e.GetFullPath());
                }
            }
        }

        private void output_DeleteFile(string answer, FileModel e)
        {
            Model.PrintOutputMessage("case: ***** File Deleted *****");
            Model.PrintOutputMessage("Path is " + e.GetFullPath());
            networkManager.NetWriteTextMsg(networkStream, e.GetFullPath());
            answer = networkManager.NetReadTextMsg(networkStream);
            if (serverAck(answer, e))
            {
                Model.PrintOutputMessage("Server answer with " + answer);
                deletedFileList.Add(e.GetFullPath());
            }
        }

        private void output_UpdateFile(string answer, FileModel e)
        {
            Model.PrintOutputMessage("case: ***** File Updated *****");
            Model.PrintOutputMessage("Path is " + e.GetFullPath());
            networkManager.NetWriteTextMsg(networkStream, e.GetFullPath());
            answer = networkManager.NetReadTextMsg(networkStream);
            if (serverAck(answer, e))
            {
                networkManager.NetWriteTextMsg(networkStream, e.GetLastModif());
                answer = networkManager.NetReadTextMsg(networkStream);
                networkManager.NetWriteFile(networkStream, e.GetFullPath());
                answer = networkManager.NetReadTextMsg(networkStream);
                if (serverAck(answer, e))
                    Model.PrintOutputMessage("Server answer with " + answer);
            }
        }

        private void output_RenameFile(string answer, FileModel e)
        {
            Model.PrintOutputMessage("case: ***** File Renamed *****");
            Model.PrintOutputMessage("Path is " + e.GetFullPath());
            Model.PrintOutputMessage("Old Path was " + e.GetOldPath());

            networkManager.NetWriteTextMsg(networkStream, e.GetOldPath());
            answer = networkManager.NetReadTextMsg(networkStream);
            if (serverAck(answer, e))
            {
                networkManager.NetWriteTextMsg(networkStream, e.GetFullPath());
                answer = networkManager.NetReadTextMsg(networkStream);
                if (serverAck(answer, e))
                {
                    networkManager.NetWriteTextMsg(networkStream, e.GetLastModif());
                    answer = networkManager.NetReadTextMsg(networkStream);
                    if (serverAck(answer, e))
                    {

                        Model.PrintOutputMessage("Server answer with " + answer);
                        if (deletedFileList.Contains(e.GetFullPath())) deletedFileList.Remove(e.GetFullPath());
                    }
                }
            }

        }

        private void output_RestoreFile(string answer, FileModel e)
        {
            Model.PrintOutputMessage("case: ***** Restore FIle *****");
            Model.PrintOutputMessage("Path: " + e.GetFullPath());
            networkManager.NetWriteTextMsg(networkStream, e.GetFullPath());
            answer = networkManager.NetReadTextMsg(networkStream);
            if (serverAck(answer, e))
            {
                Model.PrintOutputMessage("Version: " + e.GetLastModif());
                networkManager.NetWriteTextMsg(networkStream, e.GetLastModif());
                Model.PrintOutputMessage("The second part is handled by the input Communication");
                path_Shared = e.GetFullPath();
                outputThreadWait = true;
                inputThreadWait = false;
                Monitor.Pulse(myObj);
            }

        }

        private void output_OldVersionFile(string answer, FileModel e)
        {
            Model.PrintOutputMessage("case: ***** Old Version File *****");
            Model.PrintOutputMessage("Path is " + e.GetFullPath());
            networkManager.NetWriteTextMsg(networkStream, e.GetFullPath());
            string versionList = networkManager.NetReadTextMsg(networkStream);
            oldVersion = getListOfVersion(versionList, e.GetLastModif());
            uiVerFile.setMessage("File List");
            uiVerFile.setIsSuccessfull(true);
            uiVerFile.setOperationCompleted(true);
            networkManager.NetWriteTextMsg(networkStream, Program.ACK);
            Model.PrintOutputMessage("***** List of old versions *****");
            foreach (string v in oldVersion)
            {
                if (verbose) Console.WriteLine(v);
            }
            Model.PrintOutputMessage("***** End of the List *****");
            answer = String.Empty;
            answer = networkManager.NetReadTextMsg(networkStream);
            if (serverAck(answer, e))
            {
                Model.PrintOutputMessage("Server answer with " + answer);
            }

        }

        private void output_CompleteSnapshot(string answer, string fmServer, FileModel e)
        {
            Model.PrintOutputMessage("case: ***** Complete Snapshot *****");
            answer = string.Empty;
            // filecount = networkManager.NetReadTextMsg(networkStream);
            
            fileNumShared = Convert.ToInt32(fmServer);
            Model.PrintOutputMessage("Numero di file " + fileNumShared);
            //send ack
            networkManager.NetWriteTextMsg(networkStream, Program.ACK);
            string rootDir = networkManager.NetReadTextMsg(networkStream);
            if (rootDir == Program.ERROR_COMMUNICATION)
            {
                Model.PrintOutputMessage("communication error, send again");
                raiseChangeEvent(e);
            }
            else
            {
                Model.PrintOutputMessage("the root directory is " + rootDir);
                Directory.CreateDirectory(rootDir);
                backupPath = rootDir;
                Model.PrintOutputMessage("The second part is handled by the inputCommunication");
                path_Shared = rootDir;
                networkManager.NetWriteTextMsg(networkStream, Program.ACK);
                outputThreadWait = true;
                inputThreadWait = false;
                Monitor.Pulse(myObj);
            }
        }
        //**********************************************************************************


        //****************************** INPUT THREAD METHODS ****************************
        private void input_default(string answer)
        {
            Model.PrintInputMessage("event " + answer + "unknown");
            if (backUpNotFound) backUpNotFound = false;
            outputThreadWait = false;
            inputThreadWait = true;
            Model.PrintInputMessage("sveglio output");
            Monitor.Pulse(myObj);
        }

        private void input_200_OK()
        {
            Model.PrintInputMessage("case: *****200_OK*****");

            //sveglio outputThread.
            if (backUpNotFound)
            {
                //avviene solo nel caso in cui ho eseguito una complete snapshot.
                if (contFile != fileNumShared)
                {
                    backUpNotFound = false;
                    uiSnapFile.setMessage("Impossible to restore all the file. "+ contFile + "files restored over"+ fileNumShared);
                    uiSnapFile.setIsSuccessfull(false);
                    uiSnapFile.setOperationCompleted(true);
                    startSystemWatcher();
                }
                else
                {
                    backUpNotFound = false;
                    uiSnapFile.setMessage("The whole folder has been restored successfully");
                    uiSnapFile.setIsSuccessfull(true);
                    uiSnapFile.setOperationCompleted(true);
                    startSystemWatcher();
                }
            }
            outputThreadWait = false;
            inputThreadWait = true;
            Model.PrintInputMessage("wake up output Thread");
            Monitor.Pulse(myObj);
        }

        private void input_RestoreFile()
        {
            Model.PrintInputMessage("case: *****Restore File*****");
            try
            {
            try
            {
                DirectoryInfo clientDir = new DirectoryInfo(CLIENTPATH);
                if (clientDir.Exists == false) clientDir.Create();
                DirectoryInfo clientDirVersion = clientDir.CreateSubdirectory("fileRestored");
                networkManager.NetWriteTextMsg(networkStream, Program.ACK);
                //versione
                string version = networkManager.NetReadTextMsg(networkStream);
                Model.PrintInputMessage("Version is " + version);
                DateTime data = Convert.ToDateTime(version);
                networkManager.NetWriteTextMsg(networkStream, Program.ACK);
                //read the file from the server
                FileCloud fl = networkManager.NetReadFile(networkStream, clientDirVersion.FullName);
                //copy the file from source to destination.    
                string dirName = System.IO.Path.GetDirectoryName(path_Shared);
                if (!Directory.Exists(dirName)) //se la directory non esiste
                    Directory.CreateDirectory(dirName);
                string text = File.ReadAllText(fl.getLocalPath());
                watcher.EnableRaisingEvents = false;
                File.WriteAllText(path_Shared, text); //genera update
                File.SetLastWriteTime(path_Shared, data);
                watcher.EnableRaisingEvents = true;
                Model.PrintInputMessage("File " + path_Shared + "successfully restored");

                //remove the file from the deleted list if any
            if (deletedFileList.Contains(path_Shared))
            {
                File.SetCreationTime(path_Shared, data); //if i restore a delete file then i set its creation time
                File.SetLastWriteTime(path_Shared, data);
                deletedFileList.Remove(path_Shared);
                uiDelMessage.setMessage("File successfully restored");
                uiDelMessage.setIsSuccessfull(true);
                uiDelMessage.setOperationCompleted(true);
            }
            else
            {
                //it was a restore
                uiRestoreFile.setMessage("File successfully restored");
                uiRestoreFile.setIsSuccessfull(true);
                uiRestoreFile.setOperationCompleted(true);
            }
            //send the ack
            networkManager.NetWriteTextMsg(networkStream, Program.ACK);


            }
            catch (FormatException e)
                {
                    Console.WriteLine("client::FormatException >> Exception occured during the creation of the file");
                    throw new Exception();
                }
            catch (ArgumentNullException e)
            {
                    Console.WriteLine("client::ArgumentNullException >> Exception occured during the creation of the file");
                    throw new Exception();
            }
            catch (ArgumentException e)
            {
                    Console.WriteLine("client::ArgumentException >> Exception occured during the creation of the file");
                    throw new Exception();
            }
            catch (UnauthorizedAccessException e)
            {
                    Console.WriteLine("client::UnauthorizedAccessException >> Exception occured during the creation of the file");
                    throw new Exception();
            }
        }
        catch (Exception e)
        {
                Console.WriteLine("client::Exception >> Exception occured during the file restoring");
                if (deletedFileList.Contains(path_Shared))
                {
                    uiDelMessage.setMessage("Impossibile to restore the file");
                    uiDelMessage.setIsSuccessfull(false);
                    uiDelMessage.setOperationCompleted(true);

                }else
                {
                    uiRestoreFile.setMessage("Impossibile to restore the file");
                    uiRestoreFile.setIsSuccessfull(false);
                    uiRestoreFile.setOperationCompleted(true);
                }
                return;

            } 
        }

        private void input_CompleteSnapshot()
        {
            try {
                try { 
            Model.PrintInputMessage("case: *****Complete Snapshot*****");
            //invocabile SOLO se tutti i file sono stati eliminati
            //ack
            networkManager.NetWriteTextMsg(networkStream, Program.ACK);
            //receive the path
            string path = networkManager.NetReadTextMsg(networkStream);
            Model.PrintInputMessage("Path is " + path);
            if (Model.IsValidPath(path))
            {
                contFile++;
                DirectoryInfo clientDirSnap = new DirectoryInfo(CLIENTPATH_VERSIONLIST);
                if (clientDirSnap.Exists == false) clientDirSnap.Create();
                networkManager.NetWriteTextMsg(networkStream, Program.ACK);
                //versione
                string version = networkManager.NetReadTextMsg(networkStream);
                Model.PrintInputMessage("Version is " + version);
                DateTime data = Convert.ToDateTime(version);
                networkManager.NetWriteTextMsg(networkStream, Program.ACK);
                //file
                FileCloud fileC = networkManager.NetReadFile(networkStream, clientDirSnap.FullName);
                string directName = System.IO.Path.GetDirectoryName(path);
                Model.PrintInputMessage("Directory name is " + directName);
                if (!System.IO.Directory.Exists(directName)) //se la directory non esiste
                    System.IO.Directory.CreateDirectory(directName);
                string textToBeRestored = File.ReadAllText(fileC.getLocalPath());
                File.WriteAllText(path, textToBeRestored);
                File.SetCreationTime(path, data);
                File.SetLastWriteTime(path, data);
                        networkManager.NetWriteTextMsg(networkStream, Program.ACK);
                    }
                }
                catch (FormatException e)
                {
                    Console.WriteLine("client::FormatException >> Exception occured during the creation of the file");
                    throw new Exception();
                }
                catch (ArgumentNullException e)
                {
                    Console.WriteLine("client::ArgumentNullException >> Exception occured during the creation of the file");
                    throw new Exception();
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine("client::ArgumentException >> Exception occured during the creation of the file");
                    throw new Exception();
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine("client::UnauthorizedAccessException >> Exception occured during the creation of the file");
                    throw new Exception();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("client::Exception >> Exception occured during the Complete Snapshot ");
                
                    uiSnapFile.setMessage("Impossibile to restore the complete Snapshot");
                    uiSnapFile.setIsSuccessfull(false);
                    uiSnapFile.setOperationCompleted(true);
                     /*close the connection*/
                     networkStream.Flush();
                     networkStream.Close();
                     handleException();
                return;

            }

        }
        //************************************************************************************************

        //*************************************HANDSHAKE METHODS***************************************

        //handle the snapshot basing on the type of operation
        private void HandleSnapshot(DirectoryInfo clientDir, string operation)
        {
            SnapshotManager snapshotManager;
            String currentSnapshot;
            switch (operation)
            {
                case Program.LAST_SNAPSHOT:
                    /*send ack to the server i.e., ready to receive the file*/
                    networkManager.NetWriteTextMsg(networkStream, Program.ACK);
                    FileCloud fl = networkManager.NetReadFile(networkStream, clientDir.FullName);

                    Model.PrintHandshakeMessage("Received snapshot file from server");
                    /*send the ack to the server*/

                    snapshotManager = new SnapshotManager();
                    networkManager.NetWriteTextMsg(networkStream, Program.ACK);
                    if (!backUpNotFound)
                    {
                        if (fl.hasValidLocalPath())
                        {

                            currentSnapshot = snapshotManager.GetSnapshot(backupPath);
                            string oldSnapshot = System.IO.File.ReadAllText(fl.getLocalPath());
                            Model.PrintHandshakeMessage("*****Begin of Snapshot File*****");
                            if (verbose) Console.WriteLine(oldSnapshot);
                            Model.PrintHandshakeMessage("*****End of Snapshot File*****");
                            oldFiles = snapshotManager.FillListFromSnapshot(oldSnapshot);
                            Dictionary<string, FileModel> newFiles = new Dictionary<string, FileModel>();
                            newFiles = snapshotManager.FillListFromSnapshot(currentSnapshot);
                            compareFiles(oldFiles, newFiles);
                        }
                    }
                    else
                    {
                        string oldSnapshot = System.IO.File.ReadAllText(fl.getLocalPath());

                        Model.PrintHandshakeMessage("*****Begin of Snapshot File*****");
                        if (verbose) Console.WriteLine(oldSnapshot);
                        Model.PrintHandshakeMessage("*****End of Snapshot File*****");
                        oldFiles = snapshotManager.FillListFromSnapshot(oldSnapshot);
                    }
                    break;

                case Program.LAST_SNAPSHOT_NOT_FOUND:
                    // PrintVerboseMessage("received from server "+ Program.LAST_SNAPSHOT_NOT_FOUND);
                    /*send ack to the server and then send all the new file*/
                    if (!backUpNotFound)
                    {
                        networkManager.NetWriteTextMsg(networkStream, Program.ACK);
                        snapshotManager = new SnapshotManager();
                        currentSnapshot = snapshotManager.GetSnapshot(backupPath);
                        Dictionary<string, FileModel> listFiles;
                        listFiles = snapshotManager.FillListFromSnapshot(currentSnapshot);
                        /*Send all the new Events*/
                        createEventForNewSnapshot(listFiles);
                    }
                    break;

                default:
                    Model.PrintHandshakeMessage("OPERATION <" + operation + "> NOT HANDLED.");
                    break;

            }
        }

        private void handleDeleteFilesList(DirectoryInfo fileDeleteList, string operation2)
        {
            switch (operation2)
            {
                case Program.DELETED_FILE_LIST:
                    FileCloud fl = networkManager.NetReadFile(networkStream, fileDeleteList.FullName);
                    /*send the ack to the server*/
                    networkManager.NetWriteTextMsg(networkStream, Program.ACK);

                    if (fl.hasValidLocalPath())
                    {
                        var deletedFiles = System.IO.File.ReadAllLines(fl.getLocalPath());
                        if (deletedFiles.Length != 0)
                            deletedFileList = convertToFile(deletedFiles);
                        /*DEBUGG*/
                        Model.PrintHandshakeMessage("*****Begin of Delete File list*****");
                        foreach (string s in deletedFileList)
                        {
                            Model.PrintHandshakeMessage(s);
                        }
                        Model.PrintHandshakeMessage("*****Delete File list*****");
                    }
                    break;
                case Program.DELETED_FILE_LIST_EMPTY:
                    /*send the ack to the server*/
                    networkManager.NetWriteTextMsg(networkStream, Program.ACK);
                    Model.PrintHandshakeMessage("deleted file empty");
                    break;

                default:
                    Model.PrintHandshakeMessage("Operation <" + operation2 + "> Not Handled.");
                    break;
            }
        }
        //***********************************************************************************

    }
}
