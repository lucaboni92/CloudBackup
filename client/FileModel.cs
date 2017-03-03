using System;

namespace CloudBackupClient
{
    /* 
     * classe che gestisce gli eventi (type) inviati dal client al server:
     * 
     * 
     */
    public class FileModel
    {
        private string fullPath;
        private string lastModif;
        private string changeType;
        private int cont;
        private string oldPath; //SOLO PER IL RENAMED

        /*costruttore per il last snapshot*/
        public FileModel(string changeType)
        {
            this.fullPath = null;
            this.lastModif = null;
            this.changeType = changeType;
            this.oldPath = null;
            cont = 0;
            
        }
        public FileModel(string fullPath, string lastModif)
        {
            this.fullPath = fullPath;
            this.lastModif = lastModif;
            this.changeType = Program.NO_CHANGED;
            this.oldPath = null;
            cont = 0;

        }
        public FileModel(string fullPath, string lastModif, string changeType)
        {
            this.fullPath = fullPath;
            this.lastModif = lastModif;
            this.changeType = changeType;
            this.oldPath = null;
            cont = 0;
        }
        public FileModel(string fullPath, string lastModif, string changeType, string oldPath)
        {
            this.fullPath = fullPath;
            this.lastModif = lastModif;
            this.changeType = changeType;
            this.oldPath = oldPath;
            cont = 0;

        }
        internal string GetChangeType()
        {
            return this.changeType;
        }
        internal void SetChangeType(string changeType)
        {
            this.changeType = changeType;
        }

        internal void SetFullPath(string fullPath)
        {
            this.fullPath = fullPath;
        }

        internal string GetFullPath()
        {
            return this.fullPath;
        }
        internal void SetOldPath(string oldPath)
        {
            this.oldPath = oldPath;
        }

        internal string GetOldPath()
        {
            
            return this.oldPath;
        }

        internal void SetLastModif(string lastWriteTimeUtc)
        {
            this.lastModif = lastWriteTimeUtc;
        }

        internal string GetLastModif()
        {
            
            return this.lastModif;
            
        }

        internal bool EqualsTo(FileModel otherFile)
        {
            if (this.fullPath != otherFile.fullPath) return false;
           // if (this.lastModif != otherFile.lastModif ) return false;
            if (DateTime.Compare(Convert.ToDateTime(this.lastModif), Convert.ToDateTime(otherFile.lastModif)) != 0) return false;
            return true;
        }

        internal int getCont()
        {
            return cont;
        }

        internal void incCont()
        {
            cont++;
        }
    }
}