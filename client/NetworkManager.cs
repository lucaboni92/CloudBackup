using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CloudBackupClient
{
    // library of functions without any data related to a specific client
    public class NetworkManager
    {
        private const int TXT_BUFFER_SIZE = (4 * 1024) - 1;
        private const int FILE_BUFFER_SIZE = 2 * 1024;

        public void NetWriteTextMsg(NetworkStream networkStream, string msg)
        {
            try
            {
                byte[] sendBuffer = Encoding.ASCII.GetBytes(msg + "?$");
                networkStream.Write(sendBuffer, 0, sendBuffer.Length);
            }
            catch (Exception)
            {
                throw new SocketException();
            }
        }

        public string NetReadTextMsg(NetworkStream networkStream)
        {
            return NetReadTextMsg(networkStream, Program.NETWORK_TIMEOUT);
        }

        public string NetReadTextMsg(NetworkStream networkStream, int timeout)
        {
            string clientMsg = string.Empty;
            try
            {
                try
                {
                    networkStream.ReadTimeout = timeout;
                    byte[] recvBuffer = new byte[TXT_BUFFER_SIZE];
                    Int32 readBytes = networkStream.Read(recvBuffer, 0, recvBuffer.Length);
                    clientMsg = System.Text.Encoding.ASCII.GetString(recvBuffer, 0, readBytes);
                    clientMsg = clientMsg.Substring(0, clientMsg.IndexOf("?$"));
                }
                catch (IOException e)
                {
                    Console.WriteLine("client:{0}:NetworkManager >> Timeout. Nothing received in {1} seconds", Thread.CurrentThread.Name, Program.NETWORK_TIMEOUT / 1000);
                    throw;
                }
                catch (ObjectDisposedException e)
                {
                    Console.WriteLine("client:{0}:NetworkManager >> Trying to access a deleted object", Thread.CurrentThread.Name);
                    throw;
                }
            }catch (Exception e)
            {
                Console.WriteLine("client:{0}:NetworkManager:Exception >> {1}", Thread.CurrentThread.Name, e.Message);
                throw new SocketException();
            }

            return clientMsg;
        }

        public void NetWriteFile(NetworkStream networkStream, string fullPath)
        {
            try
            {
                string ack = string.Empty;
                using (Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                {

                    // Send file length (Int64)
                    networkStream.Write(BitConverter.GetBytes(stream.Length), 0, 8);
                    ack = NetReadTextMsg(networkStream, Program.NETWORK_TIMEOUT);

                    // Send file name with extension
                    string fileName = Path.GetFileName(fullPath);

                    NetWriteTextMsg(networkStream, fileName);
                    ack = NetReadTextMsg(networkStream, Program.NETWORK_TIMEOUT);

                    // Send file
                    Byte[] buffer = new Byte[FILE_BUFFER_SIZE];

                    int readLength = stream.Read(buffer, 0, buffer.Length);
                    while (readLength > 0)
                    {
                        networkStream.Write(buffer, 0, readLength);
                        readLength = stream.Read(buffer, 0, buffer.Length);
                    }
                    ack = NetReadTextMsg(networkStream, Program.NETWORK_TIMEOUT);
                    Console.WriteLine("client:NetworkManager >> sent " + (Path.GetExtension(fullPath)).ToUpper() + " file: " + fileName);
                }
            }catch(Exception e)
            {
                Console.WriteLine("client:{0}:NetworkManager:Exception >> {1}", Thread.CurrentThread.Name, e.Message);
                throw new SocketException();
            }
        }

        public FileCloud NetReadFile(NetworkStream networkStream, string directoryPath)
        {
            return NetReadFile(networkStream, directoryPath, Program.NETWORK_TIMEOUT);
        }

        public FileCloud NetReadFile(NetworkStream networkStream, string directoryPath, int timeout)
        {
            FileCloud newFile = new FileCloud();
        try { 
            try
            {
                networkStream.ReadTimeout = timeout;
                Byte[] buffer = new Byte[FILE_BUFFER_SIZE];

                // Read file length (Int64)
                networkStream.Read(buffer, 0, 8);
                Int64 fileLength = BitConverter.ToInt64(buffer, 0);
                newFile.setFileLength(fileLength);
                NetWriteTextMsg(networkStream, Program.ACK);

                // Read file name
                string fileName = NetReadTextMsg(networkStream);
                newFile.setOriginalFileName(fileName);
                //TODO string receivingTimestampStr = Program.Now().ToString();
                int receivingTimestamp = Model.Now().Millisecond;
                newFile.setReceivingTimestamp(receivingTimestamp);
                string localPath = string.Empty;
                if (directoryPath[directoryPath.Length - 1] == '\\')
                {
                    localPath = directoryPath + receivingTimestamp + "_" + fileName;
                }
                else
                {
                    localPath = directoryPath + @"\" + receivingTimestamp + "_" + fileName;
                }
                newFile.setLocalPath(localPath);
                NetWriteTextMsg(networkStream, Program.ACK);

                // Read file
                Int64 receivedBytes = 0;
                using (Stream stream = new FileStream(localPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    if (fileLength > 0)
                    {
                        int readLength;
                        do
                        {
                            readLength = networkStream.Read(buffer, 0, buffer.Length);
                            stream.Write(buffer, 0, readLength);
                            receivedBytes += readLength;
                        }
                        while (receivedBytes < fileLength);
                    }
                    NetWriteTextMsg(networkStream, Program.ACK);
                }
            }
            catch (IOException)
            {
                Console.WriteLine("client:{0}:NetworkManager >> Timeout. Nothing received in {1} seconds", Thread.CurrentThread.Name, Program.NETWORK_TIMEOUT / 1000);
                throw;
            }
                 }
            catch(Exception e)
            {
                Console.WriteLine("client:{0}:NetworkManager:Exception >> {1}", Thread.CurrentThread.Name, e.Message);
                throw new SocketException();
            } 

            return newFile;
        }

        

    }
}