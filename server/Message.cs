namespace Server
{
    internal class Message
    {
        internal enum messageType {text, file};

        messageType type;
        string text;
        string fileLocalPath;

        internal Message()
        {
            type = messageType.text;
            text = string.Empty;
            fileLocalPath = string.Empty;
        }

        internal void setMessageType(messageType type)
        {
            this.type = type;
        }

        internal messageType getMessageType()
        {
            return this.type;
        }

        internal bool isTextMessage()
        {
            if (this.type == messageType.text) return true;
            return false;
        }

        internal bool isFileMessage()
        {
            if (this.type == messageType.file) return true;
            return false;
        }

        internal bool setTextContent(string content)
        {
            if (isTextMessage())
            {
                this.text = content;
                return true;
            }
            return false;
        }

        internal string getTextContent()
        {
            if (isTextMessage()) return this.text;
            return string.Empty;
        }

        internal bool setFileContentPath(string localPath)
        {
            if (isFileMessage())
            {
                this.fileLocalPath = localPath;
                return true;
            }
            return false;
        }

        internal string getFileContentPath()
        {
            if (isFileMessage()) return this.fileLocalPath;
            return string.Empty;
        }
    }
}