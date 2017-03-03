using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Net;

namespace CloudBackupClient
{
    public class Model
    {
        private const int BUFFER_LENGTH = 10025;
        private static string backupPath;
        private NetworkManager networkManager;
        private NetworkStream networkStream;
        private HandleClient client;
        private IPAddress serverIP;
        private int serverPort;

        public Model(IPAddress serverIP, int serverPort)
        {
            this.serverIP = serverIP;
            this.serverPort = serverPort;
        }

        internal String connect(string username, string password, string operation)
        {
           try
            { 
             try
                {
                networkManager = new NetworkManager();
                TcpClient clientSocket = new TcpClient();
                string returnString = String.Empty;
                string status= OpenConnection(networkManager, clientSocket,returnString,operation);
                if (status != Program._200_OK) return status;

                switch (operation)
                {

                case Program.LOGIN:
                        status = handleLogin(networkManager,clientSocket,returnString,username,password);
                        if (status != Program._200_OK) return status;
                        break;

                    case Program.REGISTRATION:
                        status = handleRegistration(networkManager, clientSocket, returnString, username, password);
                        if (status != Program._200_OK) return status;
                        break;

                        default:
                        Model.PrintHandshakeMessage("Wrong message sended");
                            if (networkStream != null)
                            {
                                networkStream.Flush();
                                clientSocket.Close();
                            }
                            return Program.LOGINERROR;
                       
                        }

                client = new HandleClient(clientSocket, networkStream, networkManager, backupPath);
                client.doHandshake();
                return Program.LOGINTRUE;

            }
            catch (IOException e)
            {
                Console.WriteLine("client:Exception >> IOException: " + e.Message);
                throw;
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.WriteLine("client:Exception >> ArgumentOutOfRangeException: " + e.Message);
                throw;
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("client:Exception >> ArgumentNullException: " + e.Message);
                throw;
            }
            catch (FormatException e)
            {
                Console.WriteLine("client:Exception >> FormatException: " + e.Message);
                throw;
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("client:Exception >> InvalidOperationException: " + e.Message);
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
                Console.WriteLine("An error occured during the communication with the server, Closing the network Stream");
                //close the connection and alert the view
                if (networkStream != null)
                {
                    if (networkStream != null)
                    {
                        networkStream.Flush();
                        networkStream.Close();
                    }
                }         
                return Program.LOGINERROR;
            }
        }

        private string handleRegistration(NetworkManager networkManager, TcpClient clientSocket, string returnString, string username, string password)
        {
            // USERNAME
            networkManager.NetWriteTextMsg(networkStream, username);

            // "200_OK"
            returnString = networkManager.NetReadTextMsg(networkStream);
            Model.PrintHandshakeMessage("Received from server: " + returnString);
            if (returnString != Program._200_OK)
            {
                Model.PrintHandshakeMessage("Wrong message received");
                if (networkStream != null)
                {
                    networkStream.Flush();
                    networkStream.Close();
                }
                return Program.LOGINERROR;
            }

            // PASSWORD
            string newPassowrd = password;
            networkManager.NetWriteTextMsg(networkStream, newPassowrd);

            // "200_OK"
            returnString = networkManager.NetReadTextMsg(networkStream);
            Model.PrintHandshakeMessage("Received from server: " + returnString);
            if (returnString != Program._200_OK)
            {
                Model.PrintHandshakeMessage("Wrong message received");
                if (networkStream != null)
                {
                    networkStream.Flush();
                    networkStream.Close();
                }
                return Program.LOGINERROR;
            }

            // BACKUP PATH
            if (String.IsNullOrEmpty(backupPath) || !IsValidPath(backupPath))
                return Program.LOGINERROR;
            networkManager.NetWriteTextMsg(networkStream, backupPath);

            // "SUCCESSFUL_REGISTRATION" or "USERNAME_ALREADY_IN_USE"
            returnString = networkManager.NetReadTextMsg(networkStream);
            Model.PrintHandshakeMessage("Received from server: " + returnString);

            switch (returnString)
            {
                case Program.USERNAME_ALREADY_IN_USE:
                    Model.PrintHandshakeMessage("User already in use");
                    if (networkStream != null)
                    {
                        networkStream.Flush();
                        clientSocket.Close();
                    }
                    return Program.LOGINALUSED;

                case Program.WRONG_PATH_SYNTAX:
                    Model.PrintHandshakeMessage("User already in use");
                    if (networkStream != null)
                    {
                        networkStream.Flush();
                        clientSocket.Close();
                    }
                    return Program.WRONG_PATH_SYNTAX;

                case Program.SUCCESSFUL_REGISTRATION:
                    break;
                default:
                    Model.PrintHandshakeMessage("Wrong message received");
                    if (networkStream != null)
                    {
                        networkStream.Flush();
                        clientSocket.Close();
                    }
                    return Program.LOGINERROR;
            }
            return Program._200_OK;
        }

        private string handleLogin(NetworkManager networkManager, TcpClient clientSocket, string returnString, string username,string password)
        {
            // USERNAME
            networkManager.NetWriteTextMsg(networkStream, username);
            // "200_OK"
            returnString = networkManager.NetReadTextMsg(networkStream);
            Model.PrintHandshakeMessage("Received from server: " + returnString);
            if (returnString != Program._200_OK)
            {
                Model.PrintHandshakeMessage("Wrong message received");
                if (networkStream != null)
                {
                    networkStream.Flush();
                    clientSocket.Close();
                }
                return Program.LOGINERROR;
            }

            // PASSWORD
            networkManager.NetWriteTextMsg(networkStream, password);
            returnString = networkManager.NetReadTextMsg(networkStream);
            if (returnString == Program._401_UNAUTH)
            {
                Model.PrintHandshakeMessage("Wrong message received");
                if (networkStream != null)
                {
                    networkStream.Flush();
                    clientSocket.Close();
                }
                return Program.LOGINFALSE;
            }
            else
            {
                //receive the path!
                returnString = networkManager.NetReadTextMsg(networkStream);
                Model.PrintHandshakeMessage("Received from server: " + returnString);
                backupPath = returnString;
            }
            return Program._200_OK;

        }

        private string OpenConnection(NetworkManager networkManager, TcpClient clientSocket, string returnString,string operation)
        {

            Model.PrintHandshakeMessage("Client Started");
            clientSocket.Connect(serverIP, serverPort);
            Model.PrintHandshakeMessage("Client connected to "+ serverIP+" at port "+serverPort);
            networkStream = clientSocket.GetStream();
            // "OPEN_CONNECTION"
            networkManager.NetWriteTextMsg(networkStream, Program.OPEN_CONNECTION);
            // "200_OK"
            returnString = networkManager.NetReadTextMsg(networkStream);
            Model.PrintHandshakeMessage("Received from server: " + returnString);
            if (returnString != Program._200_OK)
            {
                Model.PrintHandshakeMessage("Wrong message received");
                if (networkStream != null)
                {
                    networkStream.Flush();
                    clientSocket.Close();
                }
                return Program.LOGINERROR;
            }

            // "LOGIN" or "REGISTRATION"                
            networkManager.NetWriteTextMsg(networkStream, operation);

            // "200_OK"
            returnString = networkManager.NetReadTextMsg(networkStream);
            Model.PrintHandshakeMessage("Received from server: " + returnString);
            if (returnString != Program._200_OK)
            {
                Model.PrintHandshakeMessage("Wrong message received");
                if (networkStream != null)
                {
                    networkStream.Flush();
                    clientSocket.Close();
                }
                return Program.LOGINERROR;
            }
            return Program._200_OK;
        }

        internal HandleClient getClient()
        {
            return client;
        }

     
        public static bool IsValidPath(string path)
        {
            Regex driveCheck = new Regex(@"^[a-zA-Z]:\\$");
            if (!driveCheck.IsMatch(path.Substring(0, 3))) return false;
            string strTheseAreInvalidFileNameChars = new string(Path.GetInvalidPathChars());
            strTheseAreInvalidFileNameChars += @":/?*<>" + "\"";
            Regex containsABadCharacter = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");
            if (containsABadCharacter.IsMatch(path.Substring(3, path.Length - 3))) return false;

            //DirectoryInfo dir = new DirectoryInfo(Path.GetFullPath(path));
            //if (!dir.Exists)
            //    dir.Create();
            return true;
        }
        public static DateTime Now()
        {
            DateTime localDate = DateTime.Now;
            return localDate;
        }

        internal string connect(string username, string password, string status, string path)
        {
            backupPath = path;

            return connect(username, password, status);
        }
        internal string getBackupPath()
        {

            return backupPath;
        }

        //************************ types of debug messages*****************************

        public static void PrintHandshakeMessage(string message)
        {
            if (Program.verbose) Console.WriteLine("Client:HandshakeMessage >> " + message);
        }
        public static void PrintViewMessage(string message)
        {
            if (Program.verbose) Console.WriteLine("Client:WindowMessage >> " + message);
        }
        public static void PrintInputMessage(string message)
        {
            if (Program.verbose) Console.WriteLine("Client:InputMessage >> " + message);
        }
        public static void PrintOutputMessage(string message)
        {
            if (Program.verbose) Console.WriteLine("Client:OutputMessage >> " + message);
        }
    }
}
