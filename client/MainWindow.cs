using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows.Forms;

namespace CloudBackupClient
{
    public partial class MainWindow : Form
    {
        private Model m;
        private string path;
        private List<String> currentPath = new List<string>();
        private Login login;
        private string fileToBeRestored = "prova";  //a selected path for which we ask previous version.
        private string deleteFileToBeRestored; //a deleted path to be restored
        private string versionToBeRestored; //a specific name of a version to be restored;
        private HandleClient hc;
        private string timerCase;  //the specific case for which the timer has been called
        private System.Timers.Timer timer;
        private const int TIMER_CONT_MAX = 10;
        private bool backupNotFound;
        private bool snapshotNotEmpty;
        private bool operationIsRun;
        private static int timerCont;
        private List<string> previousVersion;
        private delegate void SetSnapTimer(); //for thread safety
        private SetSnapTimer setSnapTimer;
        private delegate void SetRestoreTimer(); //for thread safety
        private SetRestoreTimer setRestoreTimer;
        private delegate void SetDeleteTimer(); //for thread safety
        private SetDeleteTimer setDeleteTimer;

        public MainWindow(Model m, HandleClient hc, Login l)
        {
            InitializeComponent();
            operationIsRun = false;
            this.login = l;
            this.hc = hc;
            this.m = m;
            timerCont = 0;
            backupNotFound = false;
            snapshotNotEmpty = false;
            path = m.getBackupPath();
            currentPath.Add(path);
            previousVersion = new List<string>();
            SnapshotLabel.Text = "Last snapshot";
            DeletedListLabel.Text = "List of deleted files";
            SetTimer();  //for monitoring changes in the model
            populatelist(path);
        }

        //********************************Event based methods********************************

        //event when the item click on an item in the main list.
        private void ListView1_ItemSelectionChanged(Object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (hc.getConnectionStatus() && !operationIsRun)
            {
                try
                {
                    String newPath = e.Item.Name;
                    FileAttributes attr = File.GetAttributes(newPath);
                    if (attr.HasFlag(FileAttributes.Directory))
                    {
                        currentPath.Add(newPath);
                        populatelist(newPath);
                    }
                    else
                    {
                        fileToBeRestored = e.Item.Name;
                        pathLabel.Text = newPath;
                        restoreButton.ForeColor = System.Drawing.SystemColors.WindowText;
                    }
                }
                catch (FileNotFoundException)
                {
                    MessageBox.Show("the selected item has just been deleted.");
                    populatelist(currentPath.First());
                }
            }
            else if (operationIsRun) ;
            else mainWindow_Exception();

        }

        //event raised when the user click one of the previous version.
        private void VersionlistView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (hc.getConnectionStatus() && !operationIsRun)
            {
                versionToBeRestored = e.Item.Name;
                RestoreFileVersion.ForeColor = System.Drawing.SystemColors.WindowText;
            } else if (operationIsRun) ;
            else mainWindow_Exception();

        }

        //event raised when the user click one of the deletedItems.
        private void deletedItemClicked(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (hc.getConnectionStatus() && !operationIsRun)
            {
                deleteFileToBeRestored = e.Item.Name;
                restoreFileButton.ForeColor = System.Drawing.SystemColors.WindowText;
            }
            else if (operationIsRun) ;
            else mainWindow_Exception();
        }

        //event raised when the user click on the button in the FileVersion Panel.
        private void BackToFileRestoreButton_Click(object sender, EventArgs e)
        {
            if (hc.getConnectionStatus() && !operationIsRun)
            {
                FileVersionpanel.Visible = false;
                FileRestorePanel.Visible = true;
                VersionlistView.Items.Clear();
                FileToBeRestoredLabel.Text = string.Empty;
            }
            else if (operationIsRun) ;
            else mainWindow_Exception();
        }

        private void goBack_Click(object sender, EventArgs e)
        {
            if (hc.getConnectionStatus()&& !operationIsRun)
            {
                if (!currentPath.Last().Equals(path))
                {
                    currentPath.RemoveAt(currentPath.Count - 1);
                    String newPath = currentPath.Last();
                    populatelist(newPath);
                }
            }
            else if (operationIsRun) ;
            else mainWindow_Exception();
        }

        private void functionList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (hc.getConnectionStatus()&& !operationIsRun)
            {
                String currentFunction = functionList.SelectedItem.ToString();

                if (!backupNotFound)
                {
                    switch (currentFunction)
                    {
                        case Program.LASTSNAPSHOTFUNC:
                            SnapshotPanel.Visible = true;
                            FileRestorePanel.Visible = false;
                            defaultPanel.Visible = false;
                            deletedPanel.Visible = false;
                            FileVersionpanel.Visible = false;
                            populateListFromModel(Program.LASTSNAPSHOTFUNC);
                            break;

                        case Program.FILE_RESTORE:
                            FileRestorePanel.Visible = true;
                            deletedPanel.Visible = false;
                            defaultPanel.Visible = false;
                            SnapshotPanel.Visible = false;
                            FileVersionpanel.Visible = false;
                            populatelist(path);
                            break;

                        case Program.DELETED_ITEMS:

                            defaultPanel.Visible = false;
                            deletedPanel.Visible = true;
                            FileRestorePanel.Visible = false;
                            SnapshotPanel.Visible = false;
                            FileVersionpanel.Visible = false;
                            populateListFromModel(Program.DELETED_ITEMS);
                            break;
                        default:
                            defaultPanel.Visible = true;
                            break;
                    }
                }
            }
            else if (operationIsRun) ;
            else mainWindow_Exception();
        }

        //This method show a list of previous version for a specific file.
        private void restoreButton_Click(object sender, EventArgs e)
        {
            if (hc.getConnectionStatus()&&!operationIsRun)
            {
                if (!String.IsNullOrEmpty(fileToBeRestored) && restoreButton.ForeColor == System.Drawing.SystemColors.WindowText)
                {
                    if (MessageBox.Show("Show previous versions of" + fileToBeRestored + " ?", "Previous versions", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        hc.oldVersionFile(fileToBeRestored);
                        timerCase = Program.UI_SHOW_VERSION;
                        operationIsRun = true;
                        timer.Start();
                    }
                }
            }
            else if (operationIsRun) ;
            else mainWindow_Exception();
        }

        /*restore of a selected file in the delete list */
        private void RestoreFileButton_click(object sender, EventArgs e)
        {
            if (hc.getConnectionStatus() &&!operationIsRun)
            {
                if (!String.IsNullOrEmpty(deleteFileToBeRestored) && restoreFileButton.ForeColor == System.Drawing.SystemColors.WindowText)
                    if (MessageBox.Show("Restore file " + deleteFileToBeRestored + " ?", "Restore file", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        hc.restoreFile(deleteFileToBeRestored, Program.LAST_VERSION);
                        timerCase = Program.UI_DELETE_FILE;
                        operationIsRun = true;
                        timer.Start();
                    }
            }
            else if (operationIsRun) ;
            else mainWindow_Exception();
        }

        /*restore of a selected version in the previous versions list */
        private void RestoreFileVersion_Click(object sender, EventArgs e)
        {
            if (hc.getConnectionStatus() && !operationIsRun)
            {
                if (!String.IsNullOrEmpty(versionToBeRestored) && RestoreFileVersion.ForeColor == System.Drawing.SystemColors.WindowText)
                    if (MessageBox.Show("Restore version " + versionToBeRestored + " of " + fileToBeRestored, "Restore file", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        hc.restoreFile(fileToBeRestored, versionToBeRestored);
                        timerCase = Program.UI_RESTORE_FILE;
                        operationIsRun = true;
                        timer.Start();
                    }
            }
            else if (operationIsRun) ;
            else mainWindow_Exception();
        }


        //********************************methods for populating the view********************************

        //populate the corresponding listView.
        public void populateListFromModel(string list)
        {
            switch (list)
            {
                case Program.LASTSNAPSHOTFUNC:
                    populateLastSnapshot();

                    break;

                case Program.DELETED_ITEMS:
                    populateDeleteList();
                    break;
            }
        }

        //populate snapshot ListView
        private void populateLastSnapshot()
        {
            Dictionary<string, FileModel> files = hc.getLastSnapshot();
            if (files.Count != 0)
            {
                snapshotNotEmpty = true;
                SnapshotListView.Items.Clear();
                ListViewItem.ListViewSubItem[] subItems;
                ListViewItem item = null;

                foreach (KeyValuePair<String, FileModel> f in files)
                {
                    item = new ListViewItem(f.Value.GetFullPath(), 0);
                    item.Name = f.Value.GetFullPath();
                    subItems = new ListViewItem.ListViewSubItem[]
                              {new ListViewItem.ListViewSubItem(item, "File"),
                       new ListViewItem.ListViewSubItem(item,
                    f.Value.GetLastModif())};
                    item.SubItems.AddRange(subItems);
                    SnapshotListView.Items.Add(item);
                }

                SnapshotListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            }
            else
            {
                SnapshotListView.Items.Clear();
            }
        }

        //populate deleteListView
        private void populateDeleteList()
        {
            restoreFileButton.ForeColor = System.Drawing.SystemColors.ButtonShadow;
            List<string> deletedFiles = hc.getList(Program.DELETED_LIST);
            if (deletedFiles.Count != 0)
            {
                deletedListView.Items.Clear();
                ListViewItem dItem = null;
                foreach (string deleteItem in deletedFiles)
                {
                    dItem = new ListViewItem(deleteItem, 0);
                    dItem.Name = deleteItem;
                    deletedListView.Items.Add(dItem);
                }
                deletedListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            }
            else
            {
                deletedListView.Items.Clear();
            }
        }

        /*restore the whole snapshot*/
        private void restoreSnap(object sender, EventArgs e)
        {
            if (hc.getConnectionStatus() && !operationIsRun)
            {
                if (MessageBox.Show("Restore the whole last snapshot?", "Restore snapshot", MessageBoxButtons.YesNo) == DialogResult.Yes && snapshotNotEmpty && backupNotFound)
                {
                    hc.completeSnapshot();
                    operationIsRun=true;
                    timerCase = Program.UI_LAST_SNAPSHOT_FILE;
                    timer.Start();

                }
                else if (!snapshotNotEmpty)
                {
                    MessageBox.Show("No elements in the snapshot", "Snapshot Empty");
                }
                else if (!backupNotFound)
                {
                    MessageBox.Show("The whole restore is available only when the back up folder has been deleted/lost.", "Snapshot service not available");
                }
            }
            else if (operationIsRun) ;
            else mainWindow_Exception();
        }
        /*Show the list of previous file*/
        internal void showVersionList()
        {

            if (FileVersionpanel.InvokeRequired)
            {
                setSnapTimer = new SetSnapTimer(showVersionList);
                this.Invoke(setSnapTimer);
            }
            else
            {
                RestoreFileVersion.ForeColor = System.Drawing.SystemColors.ButtonShadow;
                List<string> oldVersion = null;
                oldVersion = hc.getList(Program.VERSION_LIST);
                FileRestorePanel.Visible = false;
                FileVersionpanel.Visible = true;

                if (oldVersion.Count != 0)
                {
                    VersionlistView.Items.Clear();
                    FileToBeRestoredLabel.Text = fileToBeRestored;
                    ListViewItem vItem = null;

                    foreach (string version in oldVersion)
                    {
                        vItem = new ListViewItem(version, 0);
                        vItem.Name = version;
                        VersionlistView.Items.Add(vItem);
                    }
                    VersionlistView.Refresh();
                }
                else
                {
                    VersionlistView.Refresh();
                    if (MessageBox.Show("The specified file has no other versions available", "No further versions") == DialogResult.OK)
                    {
                        VersionlistView.Items.Clear();
                        FileToBeRestoredLabel.Text = string.Empty;
                        backFromVersionPanel();
                    }
                }
                if (!FileVersionpanel.Visible) FileVersionpanel.Visible = true;
            }
        }

        internal void populatelist()
        {
            populatelist(currentPath.Last());
        }

        /*This method populate the main list*/
        internal void populatelist(string fullPath)
        {
            restoreButton.ForeColor = System.Drawing.SystemColors.ButtonShadow;
            pathLabel.Text = fullPath;
            listFilesView.Items.Clear();
            if (Directory.Exists(fullPath))
            {
                backupNotFound = false;
                System.IO.DirectoryInfo nodeDirInfo = new DirectoryInfo(fullPath);
                ListViewItem.ListViewSubItem[] subItems;
                ListViewItem item = null;
                foreach (DirectoryInfo dir in nodeDirInfo.GetDirectories())
                {
                    item = new ListViewItem(dir.Name, 0);
                    item.Name = dir.FullName;
                    subItems = new ListViewItem.ListViewSubItem[]
                              {new ListViewItem.ListViewSubItem(item, "Directory"),
                   new ListViewItem.ListViewSubItem(item,
                dir.LastAccessTime.ToShortDateString())};
                    item.SubItems.AddRange(subItems);
                    listFilesView.Items.Add(item);
                }
                foreach (FileInfo file in nodeDirInfo.GetFiles())
                {
                    item = new ListViewItem(file.Name, 1);
                    item.Name = file.FullName;
                    subItems = new ListViewItem.ListViewSubItem[]
                              { new ListViewItem.ListViewSubItem(item, "File"),
                   new ListViewItem.ListViewSubItem(item,
                file.LastAccessTime.ToShortDateString())};
                    item.SubItems.AddRange(subItems);
                    listFilesView.Items.Add(item);
                }
                listFilesView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            }
            else
            {
                //the path does not exists anymore
                pathDoesNotExists();
            }
        }

        /*This method come back from the version panel and update the view*/
        private void backFromVersionPanel()
        {
            if (FileVersionpanel.InvokeRequired)
            {
                setRestoreTimer = new SetRestoreTimer(backFromVersionPanel);
                this.Invoke(setRestoreTimer);
            }
            else
            {
                this.FileVersionpanel.Visible = false;
                this.FileRestorePanel.Visible = true;
            }
        }
        /*This method come back from the delete panel and update the view*/
        private void backFromDeletePanel()
        {
            if (deletedPanel.InvokeRequired)
            {
                setDeleteTimer = new SetDeleteTimer(backFromDeletePanel);
                this.Invoke(setDeleteTimer);
            }
            else
            {
                populatelist(path);
                populateListFromModel(Program.DELETED_ITEMS);
            }

        }

        /*This method come back from the snapshot panel and update the view*/
        private void backFromLastSnapshot()
        {
            if (SnapshotPanel.InvokeRequired)
            {
                setSnapTimer = new SetSnapTimer(backFromLastSnapshot);
                this.Invoke(setSnapTimer);
            }
            else
            {
                backupNotFound = false;
                path = m.getBackupPath();
                populatelist(path);
                SnapshotPanel.Visible = false;
                FileRestorePanel.Visible = true;
                defaultPanel.Visible = false;
                deletedPanel.Visible = false;
                FileVersionpanel.Visible = false;
            }
        }
        //********************************methods for handling the view********************************

        //handle exception
        private void mainWindow_Exception()
        {
            if (FileRestorePanel.InvokeRequired)
            {
                setSnapTimer = new SetSnapTimer(mainWindow_Exception);
                this.Invoke(setSnapTimer);
            }
            else
            {
                MessageBox.Show("The connection with the server has been lost, for consistency reason, logOut needs to be executed.", "Error: Connection Lost");
                Dispose();
                Close();
                login.Show();
            }
        }


        //handle logOut
        private void logOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (hc.getConnectionStatus() && !operationIsRun)
            {
                hc.logOut();
                Dispose();
                Close();
                login.Show();
            }
            else if (operationIsRun) ;
            else mainWindow_Exception();
        }

        /*Handle the closing of the MainForm and the consequent log out */
        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (hc.getConnectionStatus()&&!operationIsRun)
            {
                hc.logOut();
                e.Cancel = true;
                Dispose();
                Close();
                login.Dispose();
                login.Close();
            }
            else if (operationIsRun) ;
            else mainWindow_Exception();
        }

        //handling the case in which the main folder has been deleted or there isn't
        private void pathDoesNotExists()
        {
            //show the right panel and a message
            backupNotFound = true;
            SnapshotPanel.Visible = true;
            FileRestorePanel.Visible = false;
            defaultPanel.Visible = false;
            deletedPanel.Visible = false;
            FileVersionpanel.Visible = false;
            MessageBox.Show("The back up folder does not exists. It is possible to restore the last snapshot from the server", "Back up folder not found");
            populateListFromModel(Program.LASTSNAPSHOTFUNC);
        }

        /*This method set the timer for checking updates from the model*/
        private void SetTimer()
        {
            int wait = 1 * 500; //check each 1/2 second.          
            timer = new System.Timers.Timer(wait);
            timer.Elapsed += timer_Elapsed;
            // We don't want the timer to start ticking again till we tell it to.
            timer.AutoReset = false;
        }

        /*This method is raised when the timer elaps and check the specific updates from the model*/
        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (hc.getConnectionStatus()) { 
            UiMessage uiMessage;
            Model.PrintViewMessage("timer elapsed");
            if (!String.IsNullOrEmpty(timerCase))
            {
                switch (timerCase)
                {
                    case Program.UI_LAST_SNAPSHOT_FILE:
                        if (hc.getUiObject(Program.UI_LAST_SNAPSHOT_FILE).getOperationCompleted())
                        {
                            timer.Stop();
                                timerCont = 0;
                                uiMessage = hc.getUiObject(Program.UI_LAST_SNAPSHOT_FILE);

                            MessageBox.Show(uiMessage.getMessage(), "Last Snapshot");
                            hc.resetUiObject(Program.UI_LAST_SNAPSHOT_FILE);
                            backFromLastSnapshot();
                                operationIsRun = false;
                            }
                        else
                        {
                                reLaunchTimer(Program.UI_LAST_SNAPSHOT_FILE);
                               
                        }
                        break;
                    case Program.UI_RESTORE_FILE:
                        if (hc.getUiObject(Program.UI_RESTORE_FILE).getOperationCompleted())
                        {
                            timer.Stop();
                                timerCont = 0;
                                uiMessage = hc.getUiObject(Program.UI_RESTORE_FILE);
                            MessageBox.Show(uiMessage.getMessage(), "File Restored Box");
                            hc.resetUiObject(Program.UI_RESTORE_FILE);
                            backFromVersionPanel();
                                operationIsRun = false;
                            }
                        else
                        {
                                reLaunchTimer(Program.UI_RESTORE_FILE);
                            }
                        break;

                    case Program.UI_SHOW_VERSION:
                        if (hc.getUiObject(Program.UI_SHOW_VERSION).getOperationCompleted())
                        {
                            timer.Stop();
                                timerCont = 0;
                                hc.resetUiObject(Program.UI_SHOW_VERSION);
                            showVersionList();
                                operationIsRun = false;
                            }
                        else
                        {
                                reLaunchTimer(Program.UI_SHOW_VERSION);
                            }
                        break;
                    case Program.UI_DELETE_FILE:
                        if (hc.getUiObject(Program.UI_DELETE_FILE).getOperationCompleted())
                        {
                            timer.Stop();
                                timerCont = 0;
                                MessageBox.Show(hc.getUiObject(Program.UI_DELETE_FILE).getMessage(), "File Restored Box");
                            hc.resetUiObject(Program.UI_DELETE_FILE);
                            backFromDeletePanel();
                                operationIsRun = false;
                            }
                        else
                        {
                                reLaunchTimer(Program.UI_DELETE_FILE);
                            }
                        break;
                }
            }
            }
            else mainWindow_Exception();
        }

        private void reLaunchTimer(string type)
        {
            if (timerCont <= TIMER_CONT_MAX)
            {
                timerCont++;
                timer.Start();
                
            }else
            {
                Model.PrintViewMessage("stop timer after " + timerCont + "times, operation not completed");
                timer.Stop();
                timerCont = 0;
                operationIsRun = false;
                showMessage(type);
            }
        }

        private void showMessage(string type)
        {
            MessageBox.Show("The operation has not been completed. Please try again");
            switch (type)
            {
                case Program.UI_DELETE_FILE:
                    backFromDeletePanel();
                    break;
                    case Program.UI_RESTORE_FILE:
                    backFromVersionPanel();
                    break;
            }
        }
    }

}