using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.DirectoryServices;
using System.Drawing;
using System.IO;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;

namespace ProfileMigrator
{
    public partial class formMain : Form
    {
        Migrator migrator;
        string oldUserSID;
        string oldUserName;
        string profilePath;
        string identity;

        public formMain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            migrator = new Migrator();

            if (Environment.GetCommandLineArgs().GetUpperBound(0) == 5)
            {
                oldUserSID = Environment.GetCommandLineArgs()[2];
                oldUserName = Environment.GetCommandLineArgs()[3];
                profilePath = Environment.GetCommandLineArgs()[4];
                identity = Environment.GetCommandLineArgs()[5];
            }
            else
            {
                oldUserSID = migrator.getSID(System.Security.Principal.WindowsIdentity.GetCurrent().Name);
                oldUserName = Functions.GetCurrentUser();
                profilePath =  System.Environment.GetEnvironmentVariable("USERPROFILE").Trim();
                identity = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            }

            elevateMe();

            //textSourceSID.Text = oldUserSID;
            textUserName.Text = oldUserName;
        }

        private void buttonMigrate_Click(object sender, EventArgs e)
        {
            migrator.newUserName = textUserName.Text;
            //migrator.oldUserSID = oldUserSID;
            //migrator.profilePath = profilePath;

            //TEST.oldSID = oldSID;
            //TEST.newSID = TEST.GetSidFromXML(textUserName.Text);

            if (migrator.LinkThisPofile(identity, profilePath))
            {
                if (!Windows.Reboot())
                {
                    MessageBox.Show("Unable to automatically restart. Please restart to complete changes");
                }
            }
            else
            {
                MessageBox.Show(migrator.message);
            }

            //MessageBox.Show(TEST.Success.ToString());

            //TEST.translateUserProfile(textSourceSID.Text, textTargetSID.Text);
            //TEST.createProfilePointer(textSourceSID.Text, textTargetSID.Text);
        }

        private void elevateMe()
        {
            string appFile = Environment.GetCommandLineArgs()[0].Replace(".vshost", "");

            string userName = migrator.Administrator_UserName;
            string domain = migrator.Administrator_Domain;
            SecureString password = migrator.Administrator_Password;
            
            //MessageBox.Show(System.Environment.OSVersion.Version.Major.ToString());

            WindowsIdentity user = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(user);
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                if (Functions.MemberOfAdministratorsGroup())
                {
                    var pInfo = new ProcessStartInfo();
                    pInfo.FileName = appFile;
                    pInfo.WorkingDirectory = Path.GetDirectoryName(appFile);
                    pInfo.Arguments = @"SelfElevating """ + @oldUserSID + @""" """ + @oldUserName + @""" """ + @profilePath + @""" """ + @identity + @"""";
                    pInfo.LoadUserProfile = true;
                    pInfo.UseShellExecute = true;
                    if (System.Environment.OSVersion.Version.Major >= 6)
                    {
                        pInfo.Verb = "runas";
                    }

                    Process.Start(pInfo);
                    Environment.Exit(0);
                    //Application.Exit();
                }
                else
                {
                    if (Environment.GetCommandLineArgs().GetUpperBound(0) == 0)
                    {
                        // Try RunAsElevating
                        var pInfo = new ProcessStartInfo();
                        pInfo.FileName = appFile;
                        pInfo.WorkingDirectory = Path.GetDirectoryName(appFile);
                        pInfo.Arguments = @"RunAsElevating """ + @oldUserSID + @""" """ + @oldUserName + @""" """ + @profilePath + @""" """ + @identity + @"""";
                        pInfo.LoadUserProfile = true;
                        pInfo.UseShellExecute = false;
                        pInfo.UserName = userName;
                        pInfo.Password = password;
                        pInfo.Domain = domain;
                        if (System.Environment.OSVersion.Version.Major >= 6)
                        {
                            pInfo.Verb = "runas";
                        }

                        Process.Start(pInfo);
                        Environment.Exit(0);
                    }
                    else
                    {
                        if (Environment.GetCommandLineArgs()[1] == "RunAsElevating")
                        {
                            // RunAsElevating Failed
                            var pInfo = new ProcessStartInfo();
                            pInfo.FileName = appFile;
                            pInfo.WorkingDirectory = Path.GetDirectoryName(appFile);
                            pInfo.Arguments = @"SelfElevating """ + @oldUserSID + @""" """ + @oldUserName + @""" """ + @profilePath + @""" """ + @identity + @"""";
                            pInfo.LoadUserProfile = true;
                            pInfo.UseShellExecute = true;
                            if (System.Environment.OSVersion.Version.Major >= 6)
                            {
                                pInfo.Verb = "runas";
                            }

                            Process.Start(pInfo);
                            Environment.Exit(0);
                            MessageBox.Show("Elevation failed");
                            Environment.Exit(0);
                        }
                        else if (Environment.GetCommandLineArgs()[1] == "SelfElevating")
                        {
                            // Check rights on HKLM Profiles
                            // Message about not being able to join domain
                            MessageBox.Show("Elevation failed");
                            Environment.Exit(0);
                        }
                        else
                        {
                            MessageBox.Show("Unknown");
                            Environment.Exit(0);
                        }
                    }
                }
            };
        }
    }
}
