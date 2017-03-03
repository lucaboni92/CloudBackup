using System;
using System.IO;

namespace Server
{
    public class FileCloud
    {
        // Attributes for all files
        private Int64 fileLength = 0;
        private string originalFileName = string.Empty;
        private int receivingTimestamp = 0;
        private string localPath = string.Empty; // path of the file in the local filesystem

        // Attributes for files received from client
        private int idFile = 0;
        private string clientPath = string.Empty; // path of the received file inside client filesystem    
        private DateTime version = DateTime.MinValue;
        private int valid = 0;
        private string fileHash = string.Empty;

        internal void setFileLength(Int64 fileLength)
        {
            this.fileLength = fileLength;
        }

        internal Int64 getFileLength()
        {
            return this.fileLength;
        }

        internal bool hasValidLocalPath()
        {
            if (localPath == string.Empty) return false;
            return Program.IsValidPath(this.localPath);
        }

        internal bool hasValidClientPath()
        {
            if (clientPath == string.Empty) return false;
            return Program.IsValidPath(this.clientPath);
        }

        internal void setOriginalFileName(string originalFileName)
        {
            this.originalFileName = originalFileName;
        }

        internal string getOriginalFileName()
        {
            return this.originalFileName;
        }

        internal void setReceivingTimestamp(int receivingTimestamp)
        {
            this.receivingTimestamp = receivingTimestamp;
        }

        internal int getReceivingTimestamp()
        {
            return this.receivingTimestamp;
        }

        internal void setLocalPath(string filePath)
        {
            this.localPath = filePath;
        }

        internal string getLocalPath()
        {
            return this.localPath;
        }

        internal string getFileName()
        {
            return Path.GetFileName(this.localPath);
        }

        internal string getDirectoryName()
        {
            return Path.GetDirectoryName(this.localPath);
        }

        internal string getFileExtension()
        {
            return Path.GetExtension(this.localPath);
        }

        internal void setClientPath(string clientPath)
        {
            this.clientPath = clientPath;
        }

        internal string getClientPath()
        {
            return this.clientPath;
        }

        internal void setVersion(DateTime version)
        {
            this.version = version;
        }

        internal DateTime getVersion()
        {
            return this.version;
        }

        internal void setIdFile(int idFile)
        {
            this.idFile = idFile;
        }

        internal int getIdFile()
        {
            return this.idFile;
        }

        internal void setValid(int valid)
        {
            this.valid = valid;
        }

        internal int getValid()
        {
            return this.valid;
        }

        internal void setFileHash(string fileHash)
        {
            this.fileHash = fileHash;
        }

        internal string getFileHash()
        {
            return this.fileHash;
        }
    }
}