namespace CloudBackupClient
{
    partial class Login
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
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.logInPanel = new System.Windows.Forms.Panel();
            this.newAccountLabel = new System.Windows.Forms.Label();
            this.PasswordLabel = new System.Windows.Forms.Label();
            this.PasswordBox = new System.Windows.Forms.TextBox();
            this.UsernameBox = new System.Windows.Forms.TextBox();
            this.loginButton = new System.Windows.Forms.Button();
            this.Usernamelabel = new System.Windows.Forms.Label();
            this.RegistrationPanel = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.newLabelPsw2 = new System.Windows.Forms.TextBox();
            this.BackToLogin = new System.Windows.Forms.Button();
            this.folderPath = new System.Windows.Forms.Button();
            this.newPassword = new System.Windows.Forms.Label();
            this.newLabelPsw = new System.Windows.Forms.TextBox();
            this.newLabUsername = new System.Windows.Forms.TextBox();
            this.newUserButton = new System.Windows.Forms.Button();
            this.newPath = new System.Windows.Forms.Label();
            this.newLabelPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.newUsername = new System.Windows.Forms.Label();
            this.logInPanel.SuspendLayout();
            this.RegistrationPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // logInPanel
            // 
            this.logInPanel.Controls.Add(this.newAccountLabel);
            this.logInPanel.Controls.Add(this.PasswordLabel);
            this.logInPanel.Controls.Add(this.PasswordBox);
            this.logInPanel.Controls.Add(this.UsernameBox);
            this.logInPanel.Controls.Add(this.loginButton);
            this.logInPanel.Controls.Add(this.Usernamelabel);
            this.logInPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logInPanel.Location = new System.Drawing.Point(0, 0);
            this.logInPanel.Name = "logInPanel";
            this.logInPanel.Size = new System.Drawing.Size(426, 268);
            this.logInPanel.TabIndex = 0;
            // 
            // newAccountLabel
            // 
            this.newAccountLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.newAccountLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.newAccountLabel.Location = new System.Drawing.Point(102, 160);
            this.newAccountLabel.Name = "newAccountLabel";
            this.newAccountLabel.Size = new System.Drawing.Size(225, 23);
            this.newAccountLabel.TabIndex = 22;
            this.newAccountLabel.Text = "New Account";
            this.newAccountLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.newAccountLabel.Click += new System.EventHandler(this.newAccountLabel_Click);
            // 
            // PasswordLabel
            // 
            this.PasswordLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PasswordLabel.Location = new System.Drawing.Point(102, 97);
            this.PasswordLabel.Name = "PasswordLabel";
            this.PasswordLabel.Size = new System.Drawing.Size(225, 17);
            this.PasswordLabel.TabIndex = 21;
            this.PasswordLabel.Text = "Password";
            this.PasswordLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // PasswordBox
            // 
            this.PasswordBox.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PasswordBox.Location = new System.Drawing.Point(102, 120);
            this.PasswordBox.Name = "PasswordBox";
            this.PasswordBox.Size = new System.Drawing.Size(225, 25);
            this.PasswordBox.TabIndex = 20;
            this.PasswordBox.UseSystemPasswordChar = true;
            // 
            // UsernameBox
            // 
            this.UsernameBox.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UsernameBox.Location = new System.Drawing.Point(104, 65);
            this.UsernameBox.Name = "UsernameBox";
            this.UsernameBox.Size = new System.Drawing.Size(225, 25);
            this.UsernameBox.TabIndex = 19;
            // 
            // loginButton
            // 
            this.loginButton.BackColor = System.Drawing.SystemColors.ControlLight;
            this.loginButton.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.loginButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.loginButton.Location = new System.Drawing.Point(168, 227);
            this.loginButton.Name = "loginButton";
            this.loginButton.Size = new System.Drawing.Size(92, 32);
            this.loginButton.TabIndex = 18;
            this.loginButton.Text = "Login";
            this.loginButton.UseVisualStyleBackColor = false;
            this.loginButton.Click += new System.EventHandler(this.loginButton_Click);
            // 
            // Usernamelabel
            // 
            this.Usernamelabel.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Usernamelabel.Location = new System.Drawing.Point(102, 41);
            this.Usernamelabel.Name = "Usernamelabel";
            this.Usernamelabel.Size = new System.Drawing.Size(225, 17);
            this.Usernamelabel.TabIndex = 17;
            this.Usernamelabel.Text = "User Name";
            this.Usernamelabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // RegistrationPanel
            // 
            this.RegistrationPanel.Controls.Add(this.label2);
            this.RegistrationPanel.Controls.Add(this.newLabelPsw2);
            this.RegistrationPanel.Controls.Add(this.BackToLogin);
            this.RegistrationPanel.Controls.Add(this.folderPath);
            this.RegistrationPanel.Controls.Add(this.newPassword);
            this.RegistrationPanel.Controls.Add(this.newLabelPsw);
            this.RegistrationPanel.Controls.Add(this.newLabUsername);
            this.RegistrationPanel.Controls.Add(this.newUserButton);
            this.RegistrationPanel.Controls.Add(this.newPath);
            this.RegistrationPanel.Controls.Add(this.newLabelPath);
            this.RegistrationPanel.Controls.Add(this.label1);
            this.RegistrationPanel.Controls.Add(this.newUsername);
            this.RegistrationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RegistrationPanel.Location = new System.Drawing.Point(0, 0);
            this.RegistrationPanel.Name = "RegistrationPanel";
            this.RegistrationPanel.Size = new System.Drawing.Size(426, 268);
            this.RegistrationPanel.TabIndex = 1;
            this.RegistrationPanel.Visible = false;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 135);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 20);
            this.label2.TabIndex = 57;
            this.label2.Text = "Password:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // newLabelPsw2
            // 
            this.newLabelPsw2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.newLabelPsw2.Location = new System.Drawing.Point(104, 135);
            this.newLabelPsw2.Name = "newLabelPsw2";
            this.newLabelPsw2.Size = new System.Drawing.Size(245, 25);
            this.newLabelPsw2.TabIndex = 50;
            this.newLabelPsw2.UseSystemPasswordChar = true;
            // 
            // BackToLogin
            // 
            this.BackToLogin.BackColor = System.Drawing.SystemColors.ControlLight;
            this.BackToLogin.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BackToLogin.ForeColor = System.Drawing.SystemColors.ControlText;
            this.BackToLogin.Location = new System.Drawing.Point(16, 227);
            this.BackToLogin.Name = "BackToLogin";
            this.BackToLogin.Size = new System.Drawing.Size(92, 27);
            this.BackToLogin.TabIndex = 53;
            this.BackToLogin.Text = "Exit";
            this.BackToLogin.UseVisualStyleBackColor = false;
            this.BackToLogin.Click += new System.EventHandler(this.BackToLogin_Click);
            // 
            // folderPath
            // 
            this.folderPath.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.folderPath.Location = new System.Drawing.Point(355, 177);
            this.folderPath.Name = "folderPath";
            this.folderPath.Size = new System.Drawing.Size(56, 26);
            this.folderPath.TabIndex = 54;
            this.folderPath.Text = "Browse";
            this.folderPath.UseVisualStyleBackColor = true;
            this.folderPath.Click += new System.EventHandler(this.folderPath_Click);
            // 
            // newPassword
            // 
            this.newPassword.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.newPassword.Location = new System.Drawing.Point(12, 89);
            this.newPassword.Name = "newPassword";
            this.newPassword.Size = new System.Drawing.Size(86, 20);
            this.newPassword.TabIndex = 50;
            this.newPassword.Text = "Password:";
            this.newPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // newLabelPsw
            // 
            this.newLabelPsw.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.newLabelPsw.Location = new System.Drawing.Point(104, 89);
            this.newLabelPsw.Name = "newLabelPsw";
            this.newLabelPsw.Size = new System.Drawing.Size(245, 25);
            this.newLabelPsw.TabIndex = 49;
            this.newLabelPsw.UseSystemPasswordChar = true;
            // 
            // newLabUsername
            // 
            this.newLabUsername.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.newLabUsername.Location = new System.Drawing.Point(104, 44);
            this.newLabUsername.Name = "newLabUsername";
            this.newLabUsername.Size = new System.Drawing.Size(245, 25);
            this.newLabUsername.TabIndex = 48;
            // 
            // newUserButton
            // 
            this.newUserButton.BackColor = System.Drawing.SystemColors.ControlLight;
            this.newUserButton.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.newUserButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.newUserButton.Location = new System.Drawing.Point(322, 227);
            this.newUserButton.Name = "newUserButton";
            this.newUserButton.Size = new System.Drawing.Size(92, 27);
            this.newUserButton.TabIndex = 52;
            this.newUserButton.Text = "Confirm";
            this.newUserButton.UseVisualStyleBackColor = false;
            this.newUserButton.Click += new System.EventHandler(this.newUserButton_Click);
            // 
            // newPath
            // 
            this.newPath.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.newPath.Location = new System.Drawing.Point(16, 180);
            this.newPath.Name = "newPath";
            this.newPath.Size = new System.Drawing.Size(82, 19);
            this.newPath.TabIndex = 52;
            this.newPath.Text = "Path:";
            this.newPath.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // newLabelPath
            // 
            this.newLabelPath.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.newLabelPath.Location = new System.Drawing.Point(104, 178);
            this.newLabelPath.Name = "newLabelPath";
            this.newLabelPath.Size = new System.Drawing.Size(245, 25);
            this.newLabelPath.TabIndex = 51;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.SystemColors.WindowText;
            this.label1.Location = new System.Drawing.Point(4, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(141, 17);
            this.label1.TabIndex = 55;
            this.label1.Text = "Create a new Account";
            // 
            // newUsername
            // 
            this.newUsername.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.newUsername.Location = new System.Drawing.Point(8, 43);
            this.newUsername.Name = "newUsername";
            this.newUsername.Size = new System.Drawing.Size(90, 25);
            this.newUsername.TabIndex = 46;
            this.newUsername.Text = "User Name:";
            this.newUsername.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Login
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(426, 268);
            this.Controls.Add(this.RegistrationPanel);
            this.Controls.Add(this.logInPanel);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(442, 307);
            this.MinimumSize = new System.Drawing.Size(442, 307);
            this.Name = "Login";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Login";
            this.logInPanel.ResumeLayout(false);
            this.logInPanel.PerformLayout();
            this.RegistrationPanel.ResumeLayout(false);
            this.RegistrationPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Panel logInPanel;
        private System.Windows.Forms.Label newAccountLabel;
        private System.Windows.Forms.Label PasswordLabel;
        private System.Windows.Forms.TextBox PasswordBox;
        private System.Windows.Forms.TextBox UsernameBox;
        private System.Windows.Forms.Button loginButton;
        private System.Windows.Forms.Label Usernamelabel;
        private System.Windows.Forms.Panel RegistrationPanel;
        private System.Windows.Forms.Button BackToLogin;
        private System.Windows.Forms.Button folderPath;
        private System.Windows.Forms.Label newPassword;
        private System.Windows.Forms.TextBox newLabelPsw;
        private System.Windows.Forms.TextBox newLabUsername;
        private System.Windows.Forms.Button newUserButton;
        private System.Windows.Forms.Label newPath;
        private System.Windows.Forms.TextBox newLabelPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label newUsername;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox newLabelPsw2;
    }
}