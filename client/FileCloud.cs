using System;
using System.IO;

namespace CloudBackupClient
{
    public class FileCloud
    {
        // Attributes for all files
        private Int64 fileLength = 0;
        private string originalFileName = string.Empty;
        private int receivingTimestamp = 0;
        private string localPath = ""; // path of the file in the local filesystem

        // Attributes for files received from client
        private string clientPath = ""; // path of the received file inside client filesystem
        private DateTime version = DateTime.MinValue;

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
            if (this.localPath == "") return false;
            return Model.IsValidPath(this.localPath);
        }

        internal bool hasValidClientPath()
        {
            if (this.clientPath == "") return false;
            return Model.IsValidPath(this.clientPath);
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
    }
}