using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data;
using System.IO;

namespace CloudBackupClient
{
    public partial class Login : Form
    {
        private Model m;
        //construttori------------------
        public Login()
        {
            InitializeComponent();
        }

        public Login(Model m)
        {
            InitializeComponent();
            this.m = m;
            Model.PrintViewMessage("Client application starts at the Login page");
        }
        //LoginPanel controls------------------

        private void loginButton_Click(object sender, EventArgs e)
        {
            if (UsernameBox.Text == "" || PasswordBox.Text == "")
            {
                MessageBox.Show("Please provide UserName and Password.", "ERROR");
                return;
            }

            String status = m.connect(UsernameBox.Text, PasswordBox.Text, Program.login);
           Model.PrintViewMessage("Stato connessione: " + status);
            switch (status)
            {
                case Program.LOGINERROR:
                    MessageBox.Show("An error occurred during the connection with Server.","ERROR");
                    clearLoginPanel();
                    break;
                case Program.LOGINFALSE:
                    MessageBox.Show("Wrong Username or Password.", "ERROR");
                    clearLoginPanel();
                    break;
                case Program.LOGINALUSED:
                    MessageBox.Show("Username not available.", "ERROR");
                    clearLoginPanel();
                    break;
                case Program.LOGINTRUE:
                    runApplication();                 
                    break;

                default:
                    MessageBox.Show("Internal Error.", "ERROR");
                    clearLoginPanel();
                    break;
            }
        }
        private void newAccountLabel_Click(object sender, EventArgs e)
        {
            clearLoginPanel();
            logInPanel.Visible = false;
            RegistrationPanel.Visible = true;

        }

        //Registration Panel controls------------------

        private void newUserButton_Click(object sender, EventArgs e)
        {
            if (newLabUsername.Text == "" || newLabelPsw.Text == "" || newLabelPsw2.Text == "" || newLabelPath.Text == "")
            {
                MessageBox.Show("Please fill all the fields.", "ERROR");
                clearNewAccountPanel();
                return;
            }
            //check if pswd are equals
            if (!newLabelPsw.Text.Equals(newLabelPsw2.Text))
            {
                MessageBox.Show("the password are different.", "ERROR");
                newLabelPsw.Clear();
                newLabelPsw2.Clear();
                return;
            }
            //CHECK IF THE FOLDER PATH IS VALID 
            try
            {
                FileAttributes attr = File.GetAttributes(newLabelPath.Text);
                if (!attr.HasFlag(FileAttributes.Directory))
                {
                    MessageBox.Show("The specified path is not a valid folder.", "ERROR");
                    newLabelPath.Clear();
                    return;
                }
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("The specified path is not a valid folder.","ERROR");
                newLabelPath.Clear();
                return;
            }
            // contact the server 
            String status = m.connect(newLabUsername.Text, newLabelPsw.Text, Program.registration, newLabelPath.Text);

            Model.PrintViewMessage("stato connessione:, "+ status);
            switch (status)
            {
                case Program.LOGINERROR:
                    MessageBox.Show("An error occurred during the connection with Server.", "ERROR");
                    clearNewAccountPanel();
                    break;
                case Program.LOGINFALSE:
                    MessageBox.Show("Wrong Username or Password.", "ERROR");
                    clearNewAccountPanel();
                    break;
                case Program.LOGINALUSED:
                    MessageBox.Show("Username not available.", "ERROR");
                    newLabUsername.Clear();
                    break;
                case Program.WRONG_PATH_SYNTAX:
                    MessageBox.Show("The Path is not valid.", "ERROR");
                    newLabelPath.Clear();
                    break;
                case Program.LOGINTRUE:

                    runApplication();

                    break;

                default:
                    MessageBox.Show("Internal Error.", "ERROR");
                    clearNewAccountPanel();
                    break;
            }
        }

        private void BackToLogin_Click(object sender, EventArgs e)
        {
            clearNewAccountPanel();
            RegistrationPanel.Visible = false;
            logInPanel.Visible = true;
        }

        private void folderPath_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                this.newLabelPath.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        //further methods-------------------------------------

        private void runApplication()
        {
            resetView();
            HandleClient hc = m.getClient();
            MainWindow main = new MainWindow(m,hc,this);
            this.Hide();
            main.Show();
        }

        private void resetView()
        {
            clearLoginPanel();
            clearNewAccountPanel();
            logInPanel.Visible = true;
            RegistrationPanel.Visible = false;
        }

        private void clearLoginPanel()
        {
            UsernameBox.Clear();
            PasswordBox.Clear();

        }
        private void clearNewAccountPanel()
        {
            newLabelPath.Clear();
            newLabelPsw.Clear();
            newLabelPsw2.Clear();
            newLabUsername.Clear();
        }
    }
}
