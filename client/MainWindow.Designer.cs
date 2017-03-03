namespace CloudBackupClient
{
    partial class MainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.LastMod = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Tipo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Nome = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.restoreButton = new System.Windows.Forms.Button();
            this.pathLabel = new System.Windows.Forms.Label();
            this.logOutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.goBack = new System.Windows.Forms.Label();
            this.listFilesView = new System.Windows.Forms.ListView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.deletedListView = new System.Windows.Forms.ListView();
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageList2 = new System.Windows.Forms.ImageList(this.components);
            this.FileRestorePanel = new System.Windows.Forms.Panel();
            this.defaultPanel = new System.Windows.Forms.Panel();
            this.functionList = new System.Windows.Forms.ListBox();
            this.deletedPanel = new System.Windows.Forms.Panel();
            this.DeletedListLabel = new System.Windows.Forms.Label();
            this.restoreFileButton = new System.Windows.Forms.Button();
            this.SnapshotPanel = new System.Windows.Forms.Panel();
            this.SnapshotLabel = new System.Windows.Forms.Label();
            this.restoreSnapshot = new System.Windows.Forms.Button();
            this.SnapshotListView = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.FileVersionpanel = new System.Windows.Forms.Panel();
            this.BackToFileRestoreButton = new System.Windows.Forms.Button();
            this.RestoreFileVersion = new System.Windows.Forms.Button();
            this.FileToBeRestoredLabel = new System.Windows.Forms.Label();
            this.VersionlistView = new System.Windows.Forms.ListView();
            this.Versione = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.menuStrip1.SuspendLayout();
            this.FileRestorePanel.SuspendLayout();
            this.deletedPanel.SuspendLayout();
            this.SnapshotPanel.SuspendLayout();
            this.FileVersionpanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // LastMod
            // 
            this.LastMod.Text = "Data";
            this.LastMod.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.LastMod.Width = 172;
            // 
            // Tipo
            // 
            this.Tipo.Text = "Tipo";
            this.Tipo.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Tipo.Width = 123;
            // 
            // Nome
            // 
            this.Nome.Text = "Nome";
            this.Nome.Width = 219;
            // 
            // restoreButton
            // 
            this.restoreButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.restoreButton.ForeColor = System.Drawing.SystemColors.ButtonShadow;
            this.restoreButton.Location = new System.Drawing.Point(455, 331);
            this.restoreButton.Name = "restoreButton";
            this.restoreButton.Size = new System.Drawing.Size(75, 23);
            this.restoreButton.TabIndex = 10;
            this.restoreButton.Text = "Restore";
            this.restoreButton.UseVisualStyleBackColor = true;
            this.restoreButton.Click += new System.EventHandler(this.restoreButton_Click);
            // 
            // pathLabel
            // 
            this.pathLabel.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.pathLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pathLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pathLabel.Location = new System.Drawing.Point(31, 4);
            this.pathLabel.Name = "pathLabel";
            this.pathLabel.Size = new System.Drawing.Size(500, 23);
            this.pathLabel.TabIndex = 9;
            this.pathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // logOutToolStripMenuItem
            // 
            this.logOutToolStripMenuItem.Name = "logOutToolStripMenuItem";
            this.logOutToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.logOutToolStripMenuItem.Text = "Log Out";
            this.logOutToolStripMenuItem.Click += new System.EventHandler(this.logOutToolStripMenuItem_Click);
            // 
            // goBack
            // 
            this.goBack.Image = ((System.Drawing.Image)(resources.GetObject("goBack.Image")));
            this.goBack.Location = new System.Drawing.Point(3, 8);
            this.goBack.Name = "goBack";
            this.goBack.Size = new System.Drawing.Size(30, 16);
            this.goBack.TabIndex = 8;
            this.goBack.Click += new System.EventHandler(this.goBack_Click);
            // 
            // listFilesView
            // 
            this.listFilesView.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.listFilesView.Alignment = System.Windows.Forms.ListViewAlignment.Default;
            this.listFilesView.AllowColumnReorder = true;
            this.listFilesView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listFilesView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Nome,
            this.Tipo,
            this.LastMod});
            this.listFilesView.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listFilesView.FullRowSelect = true;
            this.listFilesView.GridLines = true;
            this.listFilesView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listFilesView.Location = new System.Drawing.Point(-1, 30);
            this.listFilesView.MultiSelect = false;
            this.listFilesView.Name = "listFilesView";
            this.listFilesView.Size = new System.Drawing.Size(535, 298);
            this.listFilesView.SmallImageList = this.imageList1;
            this.listFilesView.TabIndex = 7;
            this.listFilesView.UseCompatibleStateImageBehavior = false;
            this.listFilesView.View = System.Windows.Forms.View.Details;
            this.listFilesView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.ListView1_ItemSelectionChanged);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "folder2.png");
            this.imageList1.Images.SetKeyName(1, "file.png");
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.logOutToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(760, 24);
            this.menuStrip1.TabIndex = 16;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // deletedListView
            // 
            this.deletedListView.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.deletedListView.Alignment = System.Windows.Forms.ListViewAlignment.Default;
            this.deletedListView.AllowColumnReorder = true;
            this.deletedListView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.deletedListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader7});
            this.deletedListView.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.deletedListView.FullRowSelect = true;
            this.deletedListView.GridLines = true;
            this.deletedListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.deletedListView.Location = new System.Drawing.Point(-1, 30);
            this.deletedListView.MultiSelect = false;
            this.deletedListView.Name = "deletedListView";
            this.deletedListView.Size = new System.Drawing.Size(535, 298);
            this.deletedListView.SmallImageList = this.imageList2;
            this.deletedListView.TabIndex = 10;
            this.deletedListView.UseCompatibleStateImageBehavior = false;
            this.deletedListView.View = System.Windows.Forms.View.Details;
            this.deletedListView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.deletedItemClicked);
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Nome";
            // 
            // imageList2
            // 
            this.imageList2.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList2.ImageStream")));
            this.imageList2.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList2.Images.SetKeyName(0, "file.png");
            // 
            // FileRestorePanel
            // 
            this.FileRestorePanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FileRestorePanel.Controls.Add(this.restoreButton);
            this.FileRestorePanel.Controls.Add(this.pathLabel);
            this.FileRestorePanel.Controls.Add(this.goBack);
            this.FileRestorePanel.Controls.Add(this.listFilesView);
            this.FileRestorePanel.Location = new System.Drawing.Point(213, 49);
            this.FileRestorePanel.Name = "FileRestorePanel";
            this.FileRestorePanel.Size = new System.Drawing.Size(535, 360);
            this.FileRestorePanel.TabIndex = 14;
            this.FileRestorePanel.Visible = false;
            // 
            // defaultPanel
            // 
            this.defaultPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.defaultPanel.Location = new System.Drawing.Point(213, 50);
            this.defaultPanel.Name = "defaultPanel";
            this.defaultPanel.Size = new System.Drawing.Size(535, 360);
            this.defaultPanel.TabIndex = 12;
            // 
            // functionList
            // 
            this.functionList.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.functionList.FormattingEnabled = true;
            this.functionList.ItemHeight = 15;
            this.functionList.Items.AddRange(new object[] {
            "Last Snapshot",
            "File Restore",
            "Deleted Items"});
            this.functionList.Location = new System.Drawing.Point(12, 57);
            this.functionList.Name = "functionList";
            this.functionList.Size = new System.Drawing.Size(180, 319);
            this.functionList.TabIndex = 11;
            this.functionList.SelectedIndexChanged += new System.EventHandler(this.functionList_SelectedIndexChanged);
            // 
            // deletedPanel
            // 
            this.deletedPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.deletedPanel.Controls.Add(this.DeletedListLabel);
            this.deletedPanel.Controls.Add(this.restoreFileButton);
            this.deletedPanel.Controls.Add(this.deletedListView);
            this.deletedPanel.Location = new System.Drawing.Point(213, 50);
            this.deletedPanel.Name = "deletedPanel";
            this.deletedPanel.Size = new System.Drawing.Size(535, 360);
            this.deletedPanel.TabIndex = 15;
            this.deletedPanel.Visible = false;
            // 
            // DeletedListLabel
            // 
            this.DeletedListLabel.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.DeletedListLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.DeletedListLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DeletedListLabel.Location = new System.Drawing.Point(3, 4);
            this.DeletedListLabel.Name = "DeletedListLabel";
            this.DeletedListLabel.Size = new System.Drawing.Size(528, 23);
            this.DeletedListLabel.TabIndex = 13;
            this.DeletedListLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // restoreFileButton
            // 
            this.restoreFileButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.restoreFileButton.ForeColor = System.Drawing.SystemColors.ButtonShadow;
            this.restoreFileButton.Location = new System.Drawing.Point(455, 331);
            this.restoreFileButton.Name = "restoreFileButton";
            this.restoreFileButton.Size = new System.Drawing.Size(75, 23);
            this.restoreFileButton.TabIndex = 11;
            this.restoreFileButton.Text = "Restore";
            this.restoreFileButton.UseVisualStyleBackColor = true;
            this.restoreFileButton.Click += new System.EventHandler(this.RestoreFileButton_click);
            // 
            // SnapshotPanel
            // 
            this.SnapshotPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SnapshotPanel.Controls.Add(this.SnapshotLabel);
            this.SnapshotPanel.Controls.Add(this.restoreSnapshot);
            this.SnapshotPanel.Controls.Add(this.SnapshotListView);
            this.SnapshotPanel.Location = new System.Drawing.Point(212, 49);
            this.SnapshotPanel.Name = "SnapshotPanel";
            this.SnapshotPanel.Size = new System.Drawing.Size(535, 360);
            this.SnapshotPanel.TabIndex = 16;
            this.SnapshotPanel.Visible = false;
            // 
            // SnapshotLabel
            // 
            this.SnapshotLabel.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.SnapshotLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SnapshotLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SnapshotLabel.Location = new System.Drawing.Point(3, 4);
            this.SnapshotLabel.Name = "SnapshotLabel";
            this.SnapshotLabel.Size = new System.Drawing.Size(528, 23);
            this.SnapshotLabel.TabIndex = 12;
            this.SnapshotLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // restoreSnapshot
            // 
            this.restoreSnapshot.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.restoreSnapshot.ForeColor = System.Drawing.SystemColors.ControlText;
            this.restoreSnapshot.Location = new System.Drawing.Point(455, 331);
            this.restoreSnapshot.Name = "restoreSnapshot";
            this.restoreSnapshot.Size = new System.Drawing.Size(75, 23);
            this.restoreSnapshot.TabIndex = 11;
            this.restoreSnapshot.Text = "Restore";
            this.restoreSnapshot.UseVisualStyleBackColor = true;
            this.restoreSnapshot.Click += new System.EventHandler(this.restoreSnap);
            // 
            // SnapshotListView
            // 
            this.SnapshotListView.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.SnapshotListView.Alignment = System.Windows.Forms.ListViewAlignment.Default;
            this.SnapshotListView.AllowColumnReorder = true;
            this.SnapshotListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6});
            this.SnapshotListView.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SnapshotListView.GridLines = true;
            this.SnapshotListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.SnapshotListView.Location = new System.Drawing.Point(-1, 30);
            this.SnapshotListView.MultiSelect = false;
            this.SnapshotListView.Name = "SnapshotListView";
            this.SnapshotListView.Size = new System.Drawing.Size(535, 298);
            this.SnapshotListView.SmallImageList = this.imageList2;
            this.SnapshotListView.TabIndex = 10;
            this.SnapshotListView.UseCompatibleStateImageBehavior = false;
            this.SnapshotListView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Nome";
            this.columnHeader4.Width = 209;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Tipo";
            this.columnHeader5.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader5.Width = 123;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Data";
            this.columnHeader6.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader6.Width = 191;
            // 
            // FileVersionpanel
            // 
            this.FileVersionpanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FileVersionpanel.Controls.Add(this.BackToFileRestoreButton);
            this.FileVersionpanel.Controls.Add(this.RestoreFileVersion);
            this.FileVersionpanel.Controls.Add(this.FileToBeRestoredLabel);
            this.FileVersionpanel.Controls.Add(this.VersionlistView);
            this.FileVersionpanel.Location = new System.Drawing.Point(212, 49);
            this.FileVersionpanel.Name = "FileVersionpanel";
            this.FileVersionpanel.Size = new System.Drawing.Size(535, 360);
            this.FileVersionpanel.TabIndex = 17;
            this.FileVersionpanel.Visible = false;
            // 
            // BackToFileRestoreButton
            // 
            this.BackToFileRestoreButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BackToFileRestoreButton.ForeColor = System.Drawing.SystemColors.WindowText;
            this.BackToFileRestoreButton.Location = new System.Drawing.Point(375, 331);
            this.BackToFileRestoreButton.Name = "BackToFileRestoreButton";
            this.BackToFileRestoreButton.Size = new System.Drawing.Size(75, 23);
            this.BackToFileRestoreButton.TabIndex = 11;
            this.BackToFileRestoreButton.Text = "Go Back";
            this.BackToFileRestoreButton.UseVisualStyleBackColor = true;
            this.BackToFileRestoreButton.Click += new System.EventHandler(this.BackToFileRestoreButton_Click);
            // 
            // RestoreFileVersion
            // 
            this.RestoreFileVersion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RestoreFileVersion.ForeColor = System.Drawing.SystemColors.ButtonShadow;
            this.RestoreFileVersion.Location = new System.Drawing.Point(455, 331);
            this.RestoreFileVersion.Name = "RestoreFileVersion";
            this.RestoreFileVersion.Size = new System.Drawing.Size(75, 23);
            this.RestoreFileVersion.TabIndex = 10;
            this.RestoreFileVersion.Text = "Restore";
            this.RestoreFileVersion.UseVisualStyleBackColor = true;
            this.RestoreFileVersion.Click += new System.EventHandler(this.RestoreFileVersion_Click);
            // 
            // FileToBeRestoredLabel
            // 
            this.FileToBeRestoredLabel.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.FileToBeRestoredLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FileToBeRestoredLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FileToBeRestoredLabel.Location = new System.Drawing.Point(3, 4);
            this.FileToBeRestoredLabel.Name = "FileToBeRestoredLabel";
            this.FileToBeRestoredLabel.Size = new System.Drawing.Size(528, 23);
            this.FileToBeRestoredLabel.TabIndex = 9;
            this.FileToBeRestoredLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // VersionlistView
            // 
            this.VersionlistView.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.VersionlistView.Alignment = System.Windows.Forms.ListViewAlignment.Default;
            this.VersionlistView.AllowColumnReorder = true;
            this.VersionlistView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.VersionlistView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Versione});
            this.VersionlistView.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.VersionlistView.FullRowSelect = true;
            this.VersionlistView.GridLines = true;
            this.VersionlistView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.VersionlistView.Location = new System.Drawing.Point(-1, 30);
            this.VersionlistView.MultiSelect = false;
            this.VersionlistView.Name = "VersionlistView";
            this.VersionlistView.Size = new System.Drawing.Size(535, 298);
            this.VersionlistView.TabIndex = 7;
            this.VersionlistView.UseCompatibleStateImageBehavior = false;
            this.VersionlistView.View = System.Windows.Forms.View.Details;
            this.VersionlistView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.VersionlistView_ItemSelectionChanged);
            // 
            // Versione
            // 
            this.Versione.Text = "Versione";
            this.Versione.Width = 219;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(760, 416);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.functionList);
            this.Controls.Add(this.SnapshotPanel);
            this.Controls.Add(this.FileVersionpanel);
            this.Controls.Add(this.deletedPanel);
            this.Controls.Add(this.FileRestorePanel);
            this.Controls.Add(this.defaultPanel);
            this.MaximumSize = new System.Drawing.Size(776, 455);
            this.MinimumSize = new System.Drawing.Size(776, 455);
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MainWindow";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindow_FormClosing);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.FileRestorePanel.ResumeLayout(false);
            this.deletedPanel.ResumeLayout(false);
            this.SnapshotPanel.ResumeLayout(false);
            this.FileVersionpanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.ColumnHeader LastMod;
        private System.Windows.Forms.ColumnHeader Tipo;
        private System.Windows.Forms.ColumnHeader Nome;
        private System.Windows.Forms.Button restoreButton;
        private System.Windows.Forms.Label pathLabel;
        private System.Windows.Forms.ToolStripMenuItem logOutToolStripMenuItem;
        private System.Windows.Forms.Label goBack;
        private System.Windows.Forms.ListView listFilesView;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ListView deletedListView;
        private System.Windows.Forms.Panel FileRestorePanel;
        private System.Windows.Forms.Panel defaultPanel;
        private System.Windows.Forms.ListBox functionList;
        private System.Windows.Forms.Panel deletedPanel;
        private System.Windows.Forms.Panel SnapshotPanel;
        private System.Windows.Forms.ListView SnapshotListView;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ImageList imageList2;
        private System.Windows.Forms.Button restoreSnapshot;
        private System.Windows.Forms.Button restoreFileButton;
        private System.Windows.Forms.Panel FileVersionpanel;
        private System.Windows.Forms.Button RestoreFileVersion;
        private System.Windows.Forms.Label FileToBeRestoredLabel;
        private System.Windows.Forms.ListView VersionlistView;
        private System.Windows.Forms.ColumnHeader Versione;
        private System.Windows.Forms.Label DeletedListLabel;
        private System.Windows.Forms.Label SnapshotLabel;
        private System.Windows.Forms.Button BackToFileRestoreButton;
        private System.Windows.Forms.ColumnHeader columnHeader7;
    }
}