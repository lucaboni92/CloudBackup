using System;
using System.Collections.Generic;
using System.IO;

namespace CloudBackupClient
{
    internal class SnapshotManager
    {
        public Dictionary<string, FileModel> FillListFromSnapshot(string oldSnapshot)
        {
            Dictionary<string, FileModel> list = new Dictionary<string, FileModel>();

            Char delimiter = '<';

            String[] substrings = oldSnapshot.Split(delimiter);
            foreach (string substring in substrings)
            {
                if (substring != "SNAPSHOT")
                {
                    Char newDelimiter = '*';
                    String[] subSubstrings = substring.Split(newDelimiter);
                    if (subSubstrings.Length == 2)
                    {
                        
                        //FULLPATH
                        string fullpath = subSubstrings[0];
                        //DATA
                        string rawData = subSubstrings[1];
                        string data = rawData.Replace(">", String.Empty);
                      //  Console.WriteLine("data " + data);
                        FileModel f = new FileModel(fullpath, data);
                        list.Add(fullpath,f);
                    }
                
                   
                }
            }
            return list;
        }
        public string GetSnapshot(String fullPath)
        {

            System.IO.DirectoryInfo nodeDirInfo = new DirectoryInfo(fullPath);

            string snapshot = "SNAPSHOT";

            DirectoryInfo d1 = new DirectoryInfo(fullPath);
            FileSystemInfo[] FSInfo = d1.GetFileSystemInfos();


            if (FSInfo == null)
            {
                throw new ArgumentNullException("FSInfo");
            }

            // Iterate through each item.
            snapshot = ListDirectoriesAndFiles(FSInfo, snapshot);

            return snapshot;


        }
        private static string ListDirectoriesAndFiles(FileSystemInfo[] FSInfo, string snapshot)
        {

            foreach (FileSystemInfo i in FSInfo)
            {
                // Check to see if this is a DirectoryInfo object.

                if (i is DirectoryInfo)
                {
                    DirectoryInfo dInfo = (DirectoryInfo)i;
                    string inside = ListDirectoriesAndFiles(dInfo.GetFileSystemInfos(), "");
                    snapshot += inside;
                }
                else if (i is FileInfo)
                {

                    string file = "\n";
                    file += "<";
                    file += i.FullName;
                    file += "*" + i.LastWriteTime.ToString();
                    snapshot += file + ">";

                }

            }
            return snapshot;
        }



    }
}