using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.IO;

namespace Server
{
    internal class DBmanager
    {
        private string dbName;
        private SQLiteConnection dbConnection;

        public DBmanager() { }

        public DBmanager(SQLiteConnection dbConnection)
        {
            this.dbConnection = dbConnection;
        }

        public void setDbName(string dbName)
        {
            this.dbName = dbName;
        }

        public string getDbName()
        {
            if (this.dbName == null) return string.Empty;
            return this.dbName;
        }

        public void dbCreate(string dbName)
        {
            this.dbName = dbName;
            SQLiteConnection.CreateFile(dbName);
        }

        public SQLiteConnection dbOpenConnection()
        {
            SQLiteConnection dbConnection;

            dbConnection = new SQLiteConnection("Data Source=cloudServerDB.sqlite;Version=3;");
            dbConnection.Open();

            this.dbConnection = dbConnection;

            return dbConnection;
        }

        public void dbCloseConnection()
        {
            if (dbConnection != null)
            {
                try
                {
                    dbConnection.Close();
                }
                catch (SQLiteException e)
                {
                    Console.WriteLine("server:DBmanager:dbCloseConnection:Exception >> Closing connection failed: " + e.Message);
                }
                finally
                {
                    dbConnection.Dispose();
                }
            }
        }

        public bool dbTableExists(string tableName)
        {
            string query = "SELECT COUNT(*) name FROM sqlite_master WHERE type = 'table' AND name = '" + tableName + "';";
            try
            {
                if (dbExecuteCountQuery(query) > 0) return true;
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("server:DBmanager:dbTableExists:Exception >> " + e.Message);
                return false;
            }
        }

        public int dbExecuteWriteQuery(string query)
        {
            List<SQLiteCommand> commandList = new List<SQLiteCommand>();
            try
            {
                SQLiteCommand command = dbConnection.CreateCommand();
                command.CommandText = query;
                commandList.Add(command);

                int nModifiedRows = dbExecuteTransactionCommands(commandList);

                return nModifiedRows;
            }
            catch (Exception e)
            {
                Console.WriteLine("server:DBmanager:dbExecuteWriteQuery:Exception >> " + e.Message);
                foreach (SQLiteCommand command in commandList)
                {
                    if (command != null) command.Dispose();
                }
                return -1;
            }
        }

        public int dbExecuteCommand(SQLiteCommand command)
        {
            try
            {
                List<SQLiteCommand> commandList = new List<SQLiteCommand>();
                command.Connection = this.dbConnection;
                commandList.Add(command);

                int nModifiedRows = dbExecuteTransactionCommands(commandList);

                return nModifiedRows;
            }
            catch (Exception e)
            {
                Console.WriteLine("server:DBmanager:dbExecuteCommand:Exception >> " + e.Message);
                if (command != null) command.Dispose();
                return -1;
            }
        }

        public int dbExecuteTransaction(List<string> querys)
        {
            List<SQLiteCommand> commandList = new List<SQLiteCommand>();
            try
            {
                foreach (string query in querys)
                {
                    SQLiteCommand command = dbConnection.CreateCommand();
                    command.CommandText = query;
                    commandList.Add(command);
                }
                int nModifiedRows = dbExecuteTransactionCommands(commandList);

                return nModifiedRows;
            }
            catch (Exception e)
            {
                Console.WriteLine("server:DBmanager:dbExecuteTransaction:Exception >> " + e.Message);
                foreach(SQLiteCommand command in commandList)
                {
                    if (command != null) command.Dispose();
                }
                return -1;
            }
        }

        private int dbExecuteTransactionCommands(List<SQLiteCommand> commands)
        {
            SQLiteTransaction transaction = null;
            int modifiedRows = 0;
            try
            {
                transaction = dbConnection.BeginTransaction();

                foreach(SQLiteCommand command in commands)
                {
                    command.Transaction = transaction;
                    modifiedRows += command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch (SQLiteException e)
            {
                Console.WriteLine("server:DBmanager:dbExecuteTransactionCommands:Exception >> " + e.Message);
                if (transaction != null)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (SQLiteException e2)
                    {
                        Console.WriteLine("server:DBmanager:dbExecuteTransactionCommands:Exception >> Transaction rollback failed: " + e2.Message);
                    }
                    finally
                    {
                        transaction.Dispose();
                    }
                }
            }
            finally
            {
                foreach(SQLiteCommand command in commands)
                {
                    if (command != null) command.Dispose();
                }
                if (transaction != null) transaction.Dispose();
            }

            return modifiedRows;
        }

        public string dbExecuteReadQuery(string query, string column)
        {
            SQLiteCommand command = null;
            try
            {
                string dataString = string.Empty;
                command = new SQLiteCommand(query, this.dbConnection);
                SQLiteDataReader data = command.ExecuteReader();
                if (data.Read())
                {
                    dataString = data.GetString(data.GetOrdinal(column));
                }

                return dataString;
            }
            catch (Exception e)
            {
                Console.WriteLine("server:DBmanager:dbExecuteReadQuery:Exception >> " + e.Message);
                if (command != null) command.Dispose();
                return string.Empty;
            }
        }

        public int dbExecuteCountQuery(string query)
        {
            int countRows = 0;
            SQLiteCommand command = null;
            try
            {
                command = new SQLiteCommand(query, this.dbConnection);
                countRows = Convert.ToInt32(command.ExecuteScalar());

                return countRows;
            }
            catch (System.FormatException e)
            {
                Console.WriteLine("server:DBmanager:dbExecuteCountQuery:Exception >> " + e.Message);
                if (command != null) command.Dispose();
                return -1;
            }
            catch (InvalidCastException e)
            {
                Console.WriteLine("server:DBmanager:dbExecuteCountQuery:Exception >> " + e.Message);
                if (command != null) command.Dispose();
                return -1;
            }
            catch (OverflowException e)
            {
                Console.WriteLine("server:DBmanager:dbExecuteCountQuery:Exception >> " + e.Message);
                if (command != null) command.Dispose();
                return -1;
            }            
        }

        public List<string> dbExecuteGetSnapshotQuery(string query, string column1, string column2, string delimiter)
        {
            SQLiteCommand command = null;
            try
            {
                List<string> list = new List<string>();
                command = new SQLiteCommand(query, this.dbConnection);
                SQLiteDataReader reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

                while (reader.Read())
                {
                    string path = reader.GetString(reader.GetOrdinal(column1));
                    DateTime version = reader.GetDateTime(reader.GetOrdinal(column2));
                    string versionStr = version.ToString("yyyy-MM-dd HH:mm:ss");
                    //string str = version.ToString();
                    //var dateTime = DateTime.ParseExact(str, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    //string versionStr = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                    string row = path + delimiter + versionStr;
                    list.Add(row);
                }

                return list;
            }
            catch (Exception e)
            {
                Console.WriteLine("server:DBmanager:dbExecuteGetSnapshotQuery:Exception >> " + e.Message);
                if (command != null) command.Dispose();
                return new List<string>();
            }
        }

        //public void dbExecuteGetSnapshotQuery(string query, string id_column, string file_column, string path)
        //{
        //    try
        //    {
        //        SQLiteCommand command = new SQLiteCommand(query, this.dbConnection);

        //        FileStream stream;
        //        BinaryWriter writer;

        //        // Size of the BLOB buffer.
        //        int bufferSize = 1024;
        //        // The BLOB byte[] buffer to be filled by GetBytes.
        //        byte[] outByte = new byte[bufferSize];
        //        // The bytes returned from GetBytes.
        //        long retval;
        //        // The starting position in the BLOB output.
        //        long startIndex = 0;

        //        // The publisher id to use in the file name.
        //        int id = 0;

        //        SQLiteDataReader reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

        //        while (reader.Read())
        //        {
        //            // Get the publisher id, which must occur before getting the logo.
        //            id = reader.GetInt32(reader.GetOrdinal(id_column));

        //            // Create a file to hold the output.
        //            stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
        //            writer = new BinaryWriter(stream);

        //            // Reset the starting byte for the new BLOB.
        //            startIndex = 0;

        //            // Read bytes into outByte[] and retain the number of bytes returned.
        //            retval = reader.GetBytes(reader.GetOrdinal(file_column), startIndex, outByte, 0, bufferSize);

        //            // Continue while there are bytes beyond the size of the buffer.
        //            while (retval == bufferSize)
        //            {
        //                writer.Write(outByte);
        //                writer.Flush();

        //                // Reposition start index to end of last buffer and fill buffer.
        //                startIndex += bufferSize;
        //                retval = reader.GetBytes(1, startIndex, outByte, 0, bufferSize);
        //            }

        //            // Write the remaining buffer.
        //            writer.Write(outByte, 0, (int)retval - 1);
        //            writer.Flush();

        //            // Close the output file.
        //            writer.Close();
        //            stream.Close();
        //        }
        //        reader.Close();
        //    }
        //    catch (System.InvalidOperationException e)
        //    {
        //        Console.WriteLine("server:DBmanager:dbExecuteGetFileQuery:Exception >> " + e.Message);
        //        throw;
        //    }
        //    catch (SQLiteException e)
        //    {
        //        Console.WriteLine("server:DBmanager:dbExecuteGetFileQuery:Exception >> " + e.Message);
        //        throw;
        //    }
        //}

        public FileCloud dbExecuteGetFileQuery(string query, string[] parametersList, string outputPath)
        {
            SQLiteCommand command = null;
            FileCloud file = new FileCloud();
            try
            {
                command = new SQLiteCommand(query, this.dbConnection);

                FileStream stream;
                BinaryWriter writer;

                int bufferSize = 1024;
                byte[] outputBuffer = new byte[bufferSize];
                // The bytes returned from GetBytes.
                long retval;
                // The starting position in the BLOB output.
                long startIndex = 0;

                // The publisher id to use in the file name.
                int idFile = 0;
                string path = string.Empty;
                DateTime version = DateTime.MinValue;
                int valid = 0;
                string fileHash = string.Empty;
                Int64 length = 0;

                SQLiteDataReader reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

                // read data
                reader.Read();

                idFile = reader.GetInt32(reader.GetOrdinal(parametersList[0]));
                path = reader.GetString(reader.GetOrdinal(parametersList[1]));
                version = reader.GetDateTime(reader.GetOrdinal(parametersList[2]));
                valid = reader.GetInt32(reader.GetOrdinal(parametersList[3]));
                fileHash = reader.GetString(reader.GetOrdinal(parametersList[4]));
                length = reader.GetInt64(reader.GetOrdinal(parametersList[5]));

                file.setIdFile(idFile);
                file.setClientPath(path);
                file.setVersion(version);
                file.setValid(valid);
                file.setFileHash(fileHash);
                file.setFileLength(length);

                // Create a file to hold the output.
                stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
                writer = new BinaryWriter(stream);

                // Reset the starting byte for the new BLOB.
                startIndex = 0;

                // Read bytes into outByte[] and retain the number of bytes returned.
                retval = reader.GetBytes(reader.GetOrdinal(parametersList[6]), startIndex, outputBuffer, 0, bufferSize);

                // Continue while there are bytes beyond the size of the buffer.
                while (retval == bufferSize)
                {
                    writer.Write(outputBuffer);
                    writer.Flush();

                    // Reposition start index to end of last buffer and fill buffer.
                    startIndex += bufferSize;
                    retval = reader.GetBytes(reader.GetOrdinal(parametersList[6]), startIndex, outputBuffer, 0, bufferSize);
                }

                // Write the remaining buffer.
                writer.Write(outputBuffer, 0, (int)retval);
                writer.Flush();

                // Close the output file.
                writer.Close();
                stream.Close();

                reader.Close();

                return file;
            }
            catch (System.InvalidOperationException e)
            {
                Console.WriteLine("server:DBmanager:dbExecuteGetFileQuery:Exception >> " + e.Message);
                if (command != null) command.Dispose();
                return file;
            }
            catch (SQLiteException e)
            {
                Console.WriteLine("server:DBmanager:dbExecuteGetFileQuery:Exception >> " + e.Message);
                if (command != null) command.Dispose();
                return file;
            }
        }

        public List<string> dbExecuteGetStringListQuery(string query, string column)
        {
            try
            {
                List<string> list = new List<string>();
                using (SQLiteCommand command = dbConnection.CreateCommand())
                {
                    command.CommandText = query;
                    command.CommandType = CommandType.Text;
                    SQLiteDataReader r = command.ExecuteReader();
                    while (r.Read())
                    {
                        list.Add(Convert.ToString(r[column]));
                    }

                    return list;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("server:DBmanager:dbExecuteGetStringListQuery:Exception >> " + e.Message);
                return new List<string>();
            }
        }

        public List<DateTime> dbExecuteGetDateTimeListQuery(string query, string column)
        {
            try
            {
                List<DateTime> list = new List<DateTime>();
                using (SQLiteCommand command = dbConnection.CreateCommand())
                {
                    command.CommandText = query;
                    command.CommandType = CommandType.Text;
                    SQLiteDataReader r = command.ExecuteReader();
                    while (r.Read())
                    {
                        list.Add(Convert.ToDateTime(r[column]));

                    }

                    return list;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("server:DBmanager:dbExecuteGetDateTimeListQuery:Exception >> " + e.Message);
                return new List<DateTime>();
            }
        }

    }
}