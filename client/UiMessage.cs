namespace CloudBackupClient
{
    internal class UiMessage
    {
        private string message;
        private bool operationCompleted;
        private bool isSuccessfull;
        public UiMessage()
        {
            reset();
        }
        public bool getOperationCompleted()
        {
            return operationCompleted;
        }
        public bool getIsSuccessfull()
        {
            return isSuccessfull;
        }
        public string getMessage()
        {
            return message;
        }
        public void setOperationCompleted(bool value)
        {
            operationCompleted = value;
        }
        public void setIsSuccessfull(bool value)
        {
            isSuccessfull = value;
        }
        public void setMessage(string value)
        {
            message = value;
        }
        public void reset()
        {
            message = string.Empty;
            operationCompleted = false;
            isSuccessfull = false;        
        }
    }
}