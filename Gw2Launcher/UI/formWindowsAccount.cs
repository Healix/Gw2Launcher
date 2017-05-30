using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Principal;
using System.DirectoryServices.AccountManagement;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;

namespace Gw2Launcher.UI
{
    public partial class formWindowsAccount : Form
    {
        public formWindowsAccount()
        {
            InitializeComponent();

            listAccounts.Enabled = false;
            listAccounts.Items.Add("Loading...");

            Task.Factory.StartNew(new Action(LoadAccounts));
        }

        public formWindowsAccount(string username) : this()
        {
            textAccountName.Text = username;
        }

        public string AccountName
        {
            get;
            private set;
        }

        private void LoadAccounts()
        {
            List<string> users = new List<string>();

            try
            {
                using (var accountSearcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_UserAccount WHERE LocalAccount=true AND Disabled=false AND Lockout=false AND SIDType=1"))
                {
                    using (var accountCollection = accountSearcher.Get())
                    {
                        foreach (var account in accountCollection)
                        {
                            string user = account["Name"].ToString();
                            if (!string.IsNullOrWhiteSpace(user) && !user.Equals("HomeGroupUser$", StringComparison.OrdinalIgnoreCase))
                            {
                                users.Add(user);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }

            try
            {
                this.Invoke(new MethodInvoker(
                    delegate
                    {
                        listAccounts.Items.Clear();

                        foreach (string user in users)
                        {
                            listAccounts.Items.Add(user);
                        }

                        listAccounts.Enabled = true;
                    }));
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }
        }

        private Principal GetAccount(string username)
        {
            PrincipalContext ctx = new PrincipalContext(ContextType.Machine);
            Principal p = Principal.FindByIdentity(ctx, IdentityType.Name, username);

            return p;
        }

        private bool IsAdministrator(Principal p)
        {
            SecurityIdentifier adminSid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
            PrincipalContext ctx = new PrincipalContext(ContextType.Machine);

            bool isMember = p.IsMemberOf(ctx, IdentityType.Sid, adminSid.ToString());

            return isMember;
        }

        private bool CreateAccount(string name, System.Security.SecureString password)
        {
            try
            {
                string _password;
                IntPtr ptr = Marshal.SecureStringToBSTR(password);
                try
                {
                    _password = Marshal.PtrToStringUni(ptr);
                }
                finally
                {
                    Marshal.ZeroFreeBSTR(ptr);
                }
                Util.ProcessUtil.CreateAccount(name, _password);
                bool created = GetAccount(name) != null;
                return created;
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
                return false;
            }
        }

        //private bool CreateAccount(string name, string password)
        //{
        //    try
        //    {
        //        PrincipalContext ctx = new PrincipalContext(ContextType.Machine);
        //        UserPrincipal user = new UserPrincipal(ctx);
        //        user.Name = name;
        //        user.SetPassword(password);
        //        user.Save();

        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        return false;
        //    }
        //}

        private void formWindowsAccount_Load(object sender, EventArgs e)
        {
        }

        private void listAccounts_SelectedIndexChanged(object sender, EventArgs e)
        {
            object i = listAccounts.SelectedItem;

            if (i is string)
            {
                textAccountName.Text = (string)i;
            }
            else if (i is Principal)
            {
                textAccountName.Text = ((Principal)i).Name;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            buttonOK.Enabled = false;
            listAccounts.Enabled = false;
            textAccountName.Enabled = false;

            try
            {
                if (string.IsNullOrWhiteSpace(textAccountName.Text))
                {
                    this.AccountName = null;
                    this.DialogResult = DialogResult.OK;
                    return;
                }

                Principal p = GetAccount(textAccountName.Text);
                if (p != null)
                {
                    this.AccountName = textAccountName.Text;
                    this.DialogResult = DialogResult.OK;
                }
                else
                {
                    DialogResult result = MessageBox.Show(this, "A user with the name \"" + textAccountName.Text + "\" could not be found.\n\nWould you like to create this user now?", "User not found, create?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    switch (result)
                    {
                        case System.Windows.Forms.DialogResult.Cancel:
                            return;
                        case System.Windows.Forms.DialogResult.Yes:
                            formPassword f;
                            System.Security.SecureString password;

                            using (f = new formPassword("New password"))
                            {
                                if (f.ShowDialog(this) != DialogResult.OK)
                                    return;

                                password = f.Password.Copy();

                                if (password.Length == 0)
                                {
                                    password.Dispose();
                                    MessageBox.Show(this, "A password is required to use another user's account", "Password required", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                    return;
                                }
                            }

                            using (password)
                            {
                                using (f = new formPassword("Confirm password"))
                                {
                                    if (f.ShowDialog(this) != DialogResult.OK)
                                        return;

                                    if (!Security.Credentials.Compare(password, f.Password))
                                    {
                                        MessageBox.Show(this, "Passwords do not match", "Wrong password", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                        return;
                                    }
                                }

                                if (!CreateAccount(textAccountName.Text, password))
                                {
                                    MessageBox.Show(this, "An error occured while trying to create the user", "Unable to create the user", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }
                                else
                                {
                                    Security.Credentials.SetPassword(textAccountName.Text, password);
                                    try
                                    {
                                        Util.ProcessUtil.InitializeAccount(textAccountName.Text, password);
                                    }
                                    catch (Exception ex)
                                    {
                                        Util.Logging.Log(ex);
                                        MessageBox.Show(this, "The user has been successfully created, but you will need to first login to the account before it can be used.", "User created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    }
                                }
                            }

                            this.AccountName = textAccountName.Text;
                            this.DialogResult = DialogResult.OK;
                            break;
                        case System.Windows.Forms.DialogResult.No:
                            this.AccountName = textAccountName.Text;
                            this.DialogResult = DialogResult.OK;
                            break;
                    }
                }
            }
            finally
            {
                buttonOK.Enabled = true;
                textAccountName.Enabled = true;
                listAccounts.Enabled = true;
            }
        }
    }
}
