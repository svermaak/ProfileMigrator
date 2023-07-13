using System;
using System.Collections.Generic;
using System.Text;
using SetACLCOMLibrary;
using Microsoft.Win32;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using System.Management;
using System.Configuration;
using System.Xml;
using System.Windows.Forms;
using System.Security;
using System.IO;
using System.Diagnostics;

namespace ProfileMigrator
{
    class Migrator
    {
        //SHOULD PUT UNLOCK KEY IN PROPERTY
        bool _success = false;
        string _oldUserSID = "";
        string _newUserSID = "";
        string _newUserName = "";
        string _newDomain = "";
        string _message = "";
        string _profilePath = "";
        string _newDomainDN = "";

        Encryption64 encryption64 = new Encryption64();

        public string DecryptPassword()
        {
            return ConfigurationManager.AppSettings["Setting1"];
            //encryption64.Decrypt(
        }

        public bool Success
        {
            get
            {
                return this._success;
            }
            set
            {
                this._success = value;
            }
        }
        public string oldUserSID
        {
            get
            {
                return this._oldUserSID;
            }
            set
            {
                this._oldUserSID = value.Trim();
            }
        }
        public string newUserSID
        {
            get
            {
                return this._newUserSID;
            }
            set
            {
                this._newUserSID = value.Trim();
            }
        }
        public string newUserName
        {
            get
            {
                return this._newUserName;
            }
            set
            {
                this._newUserName = value.Trim();
            }
        }
        public string newDomain
        {
            get
            {
                return @ConfigurationManager.AppSettings["Target_Domain"];
            }
        }
        public string message
        {
            get
            {
                return this._message;
            }
        }
        public string profilePath
        {
            get
            {
                return this._profilePath;
            }
            set
            {
                this._profilePath = value.Trim();
            }
        }
        public string Source_UserName
        {
            get
            {
                return @ConfigurationManager.AppSettings["Source_UserName"];
            }
        }
        public SecureString Source_Password
        {
            get
            {
                Encryption64 encryption64 = new Encryption64();

                string passwordPre = encryption64.Decrypt(@ConfigurationManager.AppSettings["Source_Password"], @"5r!zaV3Pm*Gl");

                char[] passwordChars = passwordPre.ToCharArray();
                passwordPre = "";

                SecureString password = new SecureString();
                foreach (char c in passwordChars)
                {
                    password.AppendChar(c);
                }

                return @password;
            }
        }
        public string Source_Domain
        {
            get
            {
                return @ConfigurationManager.AppSettings["Source_Domain"];
            }
        }
        public static string Target_UserName
        {
            get
            {
                return @ConfigurationManager.AppSettings["Target_UserName"];
            }
        }
        public static SecureString Target_Password
        {
            get
            {
                Encryption64 encryption64 = new Encryption64();

                string passwordPre = encryption64.Decrypt(@ConfigurationManager.AppSettings["Target_Password"], @"5r!zaV3Pm*Gl");

                char[] passwordChars = passwordPre.ToCharArray();
                passwordPre = "";

                SecureString password = new SecureString();
                foreach (char c in passwordChars)
                {
                    password.AppendChar(c);
                }

                return @password;
            }
        }
        public static string Target_Domain
        {
            get
            {
                return @ConfigurationManager.AppSettings["Target_Domain"];
            }
        }
        public static string Target_FQDN
        {
            get
            {
                return @ConfigurationManager.AppSettings["Target_FQDN"];
            }
        }
        public static string targetDomainDN
        {
            get
            {
                return @ConfigurationManager.AppSettings["Target_DomainDN"];
            }
        }
        public string Administrator_UserName
        {
            get
            {
                return @ConfigurationManager.AppSettings["Administrator_UserName"];
            }
        }
        public SecureString Administrator_Password
        {
            get
            {
                Encryption64 encryption64 = new Encryption64();

                string passwordPre = encryption64.Decrypt(@ConfigurationManager.AppSettings["Administrator_Password"], @"5r!zaV3Pm*Gl");

                char[] passwordChars = passwordPre.ToCharArray();
                passwordPre = "";

                SecureString password = new SecureString();
                foreach (char c in passwordChars)
                {
                    password.AppendChar(c);
                }

                return @password;
            }
        }
        public string Administrator_Domain
        {
            get
            {
                return @ConfigurationManager.AppSettings["Administrator_Domain"];
            }
        }

        private bool CheckParameters()
        {
            if (string.IsNullOrEmpty(oldUserSID))
            {
                return false;
            }
            if (string.IsNullOrEmpty(newUserSID))
            {
                return false;
            }
            return true;
        }

        public bool LinkThisPofile(string identity,string profilePath)
        {
            //MessageBox.Show(CheckParameters().ToString());

            //***** Add a ping target domain

            //use identity to get old user name to get old sid
            _oldUserSID = getSID(identity);

            Logging.WriteEventLog(0, string.Format("Register SetACL: {0}", registerSetACL()), EventLogEntryType.Information);

            string newUserSID = "";
            if (newUserSID == "") { newUserSID = GetUserSIDFromTargetDomain(_newUserName); }
            if (newUserSID == "") { newUserSID = GetSidFromXML(_newUserName); }

            if (newUserSID != "")
            {
                _newUserSID = newUserSID;
                Logging.WriteEventLog(0, string.Format("Old SID: {0}", _oldUserSID), EventLogEntryType.Information);
                Logging.WriteEventLog(0, string.Format("New SID: {0}", _newUserSID), EventLogEntryType.Information);
                Logging.WriteEventLog(0, string.Format("Profile Path: {0}", profilePath), EventLogEntryType.Information);

                _message = _newUserSID;
                if ((_oldUserSID != "") && (_newUserSID != "") && (_profilePath != ""))
                {
                    Logging.WriteEventLog(0, string.Format("Translate User Profile: {0}", translateUserProfile(_oldUserSID, _newUserSID).ToString()), EventLogEntryType.Information);
                    Logging.WriteEventLog(0, string.Format("Translate User Hive: {0}", translateUserHive(_oldUserSID, _newUserSID)), EventLogEntryType.Information);
                    Logging.WriteEventLog(0, string.Format("Create Profile Pointer: {0}", createProfilePointer(_oldUserSID, _newUserSID)), EventLogEntryType.Information);
                    Logging.WriteEventLog(0, string.Format("Set Default User Name and Domain: {0}", Functions.SetDefaultUserNameAndDomain(newUserName, newDomain)), EventLogEntryType.Information);
                    Logging.WriteEventLog(0, string.Format("Join Domain", JoinAndSetName(Environment.MachineName).ToString()), EventLogEntryType.Information);
                    Logging.WriteEventLog(0, "Profile linked to the specified user", EventLogEntryType.Information);
                    _message = "Profile linked to the specified user";
                    return true;
                }
                else
                {

                }
            }
            else
            {

            }
            Logging.WriteEventLog(0, "Profile cannot be linked to the specified user", EventLogEntryType.Information);
            _message = "Profile cannot be linked to the specified user";
            return false;
        }
        //public bool MigrateProfile(string profilePath,string oldSID)
        //{
        //    //***** Add a ping target domain

        //    Logging.WriteEventLog(0, string.Format("Register SetACL: {0}", registerSetACL()), EventLogEntryType.Information);

        //    string newUserSID = "";
        //    if (newUserSID == "") { newUserSID = GetUserSIDFromTargetDomain(_newUserName); }
        //    if (newUserSID == "") { newUserSID = GetSidFromXML(_newUserName); }

        //    if (newUserSID != "")
        //    {
        //        _newUserSID = newUserSID;
        //        Logging.WriteEventLog(0, string.Format("Old SID: {0}", _oldUserSID), EventLogEntryType.Information);
        //        Logging.WriteEventLog(0, string.Format("New SID: {0}", _newUserSID), EventLogEntryType.Information);
        //        Logging.WriteEventLog(0, string.Format("Profile Path: {0}", profilePath), EventLogEntryType.Information);

        //        _message = _newUserSID;
        //        if ((_oldUserSID != "") && (_newUserSID != "") && (_profilePath != ""))
        //        {
        //            Logging.WriteEventLog(0, string.Format("Translate User Profile: {0}", translateUserProfile(_oldUserSID, _newUserSID).ToString()), EventLogEntryType.Information);
        //            Logging.WriteEventLog(0, string.Format("Translate User Hive: {0}", translateUserHive(_oldUserSID, _newUserSID)), EventLogEntryType.Information);
        //            Logging.WriteEventLog(0, string.Format("Create Profile Pointer: {0}", createProfilePointer(_oldUserSID, _newUserSID)), EventLogEntryType.Information);
        //            Logging.WriteEventLog(0, string.Format("Set Default User Name and Domain: {0} {1} {2}", SetDefaultUserNameAndDomain(newUserName, newDomain), newUserName, newDomain), EventLogEntryType.Information);
        //            Logging.WriteEventLog(0, string.Format("Join Domain", JoinAndSetName(Environment.MachineName).ToString()), EventLogEntryType.Information);
        //            Logging.WriteEventLog(0, "Profile linked to the specified user", EventLogEntryType.Information);
        //            _message = "Profile linked to the specified user";
        //            return true;
        //        }
        //        else
        //        {

        //        }
        //    }
        //    else
        //    {

        //    }
        //    Logging.WriteEventLog(0, "Profile cannot be linked to the specified user", EventLogEntryType.Information);
        //    _message = "Profile cannot be linked to the specified user";
        //    return false;
        //}
        public bool translateUserHive(string oldSID, string newSID)
        {


            //Get current user profile path
            string profilePath = System.Environment.GetEnvironmentVariable("USERPROFILE").Trim();

            // Create a new SetACL instance
            SetACLCOMServer setACL = new SetACLCOMServer();

            // Attach a handler routine to SetACL's message event so we can receive anything SetACL wants to tell us
            setACL.MessageEvent += new _ISetACLCOMServerEvents_MessageEventEventHandler(setacl_MessageEvent);

            // Enable sending events
            int returnCode = setACL.SendMessageEvents(true);
            if (returnCode != (int)RETCODES.RTN_OK) { } //Do something about the error 

            try
            {
                // Grant full permission for new SID to HKCU, excluding "Policies"
                returnCode = setACL.SetObject(@"HKEY_USERS\" + oldSID, (int)SE_OBJECT_TYPE.SE_REGISTRY_KEY);
                returnCode = setACL.SetAction((int)ACTIONS.ACTN_ADDACE);
                returnCode = setACL.SetRecursion((int)RECURSION.RECURSE_CONT_OBJ);
                returnCode = setACL.AddAction((int)ACTIONS.ACTN_SETINHFROMPAR);
                returnCode = setACL.AddACE(newSID, "Full", (int)INHERITANCE.INHPARYES, false, (int)ACCESS_MODE.GRANT_ACCESS, (int)SDINFO.ACL_DACL);
                setACL.AddObjectFilter("Policies");
                returnCode = setACL.Run();

                // Grant read permission for new SID to "Policies" in HKCU
                returnCode = setACL.SetObject(@"HKEY_USERS\" + oldSID + @"\Software\Policies", (int)SE_OBJECT_TYPE.SE_REGISTRY_KEY);
                returnCode = setACL.SetAction((int)ACTIONS.ACTN_ADDACE);
                returnCode = setACL.SetRecursion((int)RECURSION.RECURSE_CONT_OBJ);
                returnCode = setACL.AddAction((int)ACTIONS.ACTN_SETINHFROMPAR);
                returnCode = setACL.AddACE(newSID, "Read", (int)INHERITANCE.INHPARYES, false, (int)ACCESS_MODE.GRANT_ACCESS, (int)SDINFO.ACL_DACL);
                returnCode = setACL.Run();
                returnCode = setACL.SetObject(@"HKEY_USERS\" + oldSID + @"\Software\Policies", (int)SE_OBJECT_TYPE.SE_REGISTRY_KEY);
                returnCode = setACL.SetAction((int)ACTIONS.ACTN_RESETCHILDPERMS);
                returnCode = setACL.SetObjectFlags((int)INHERITANCE.INHPARYES, (int)INHERITANCE.INHPARYES, true, true);
                returnCode = setACL.Run();
            }
            catch
            {

            }

            return true;
        }
        public bool translateUserProfile(string oldSID, string newSID)
        {
            //Get current user profile path
            //string profilePath = System.Environment.GetEnvironmentVariable("USERPROFILE").Trim();
            profilePath = _profilePath;

            // Create a new SetACL instance
            SetACLCOMServer setACL = new SetACLCOMServer();

            // Attach a handler routine to SetACL's message event so we can receive anything SetACL wants to tell us
            setACL.MessageEvent += new _ISetACLCOMServerEvents_MessageEventEventHandler(setacl_MessageEvent);

            // Enable sending events
            int returnCode = setACL.SendMessageEvents(true);
            if (returnCode != (int)RETCODES.RTN_OK) { } //Do something about the error 

            try
            {
                // Grant full permission for new SID to profile files
                returnCode = setACL.SetObject(@profilePath, (int)SE_OBJECT_TYPE.SE_FILE_OBJECT);
                returnCode = setACL.SetAction((int)ACTIONS.ACTN_ADDACE);
                returnCode = setACL.SetRecursion((int)RECURSION.RECURSE_CONT_OBJ);
                returnCode = setACL.AddAction((int)ACTIONS.ACTN_SETINHFROMPAR);
                returnCode = setACL.AddACE(newSID, "Full", (int)INHERITANCE.INHPARYES, false, (int)ACCESS_MODE.GRANT_ACCESS, (int)SDINFO.ACL_DACL);
                returnCode = setACL.Run();
            }
            catch
            {

            }

            return true;
        }
        static void setacl_MessageEvent(string message)
        {
            // For demo purposes, just print the message
            Console.WriteLine(message);
        }
        public bool createProfilePointer(string sourceSID, string TargetSID)
        {
            return Functions.CopyKey(Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList", true), sourceSID, TargetSID);
        }
        public string getSID(string login)
        {
            string domain = "";
            string userName = "";

            if (login.IndexOf(@"\") == -1)
            {
                domain = System.Environment.MachineName;
                userName = login;
            }
            else
            {
                domain = login.Split('\\')[0];
                userName = login.Split('\\')[1];
            }

            DirectoryEntry obDirEntry = new DirectoryEntry(@"WinNT://" + @domain + @"/" + @userName);
            PropertyCollection coll = obDirEntry.Properties;
            byte[] obVal = (byte[])obDirEntry.Properties["objectSid"][0];
            return convertByteToStringSid(obVal);
        }
        private string GetUserSIDFromTargetDomain(string userName)
        {
            string returnValue = "";

            try
            {
                // define the root of your search
                DirectoryEntry root = new DirectoryEntry("LDAP://" + Target_FQDN, Target_Domain + @"\" + Target_UserName, encryption64.Decrypt(@ConfigurationManager.AppSettings["Target_Password"], @"5r!zaV3Pm*Gl"));

                // set up DirectorySearcher  
                DirectorySearcher srch = new DirectorySearcher(root);
                //srch.Filter = "(objectCategory=Person)";
                srch.Filter = ("(SAMAccountName=" + userName + ")");
                srch.SearchScope = SearchScope.Subtree;

                // define properties to load
                srch.PropertiesToLoad.Add("objectSid");
                srch.PropertiesToLoad.Add("displayName");

                // search the directory
                SearchResult result = srch.FindOne();

                // grab the data - if present
                if (result.Properties["objectSid"] != null && result.Properties["objectSid"].Count > 0)
                {
                    //byte[] obVal = (byte[])obDirEntry.Properties["objectSid"][0];
                    //return convertByteToStringSid(obVal);

                    //byte[] obVal =(byte[])result.Properties["objectSid"][0];
                    returnValue = convertByteToStringSid((byte[])result.Properties["objectSid"][0]);
                    //sid = result.Properties["objectSid"][0].ToString();
                }
            }
            catch
            {
                returnValue = "";
            }

            return returnValue;
        }
        private string convertByteToStringSid(byte[] sidBytes)
        {
            string returnValue = "";

            try
            {
                StringBuilder convertedSID = new StringBuilder();

                convertedSID.Append("S-");
                convertedSID.Append(sidBytes[0].ToString());
                convertedSID.Append("-");
                if ((sidBytes[6] != 0) || (sidBytes[5] != 0))
                {
                    string auth = String.Format("0x{0:2x}{1:2x}{2:2x}{3:2x}{4:2x}{5:2x}", Convert.ToInt16(sidBytes[1]), Convert.ToInt16(sidBytes[2]), Convert.ToInt16(sidBytes[3]), Convert.ToInt16(sidBytes[4]), Convert.ToInt16(sidBytes[5]), Convert.ToInt16(sidBytes[6]));
                    convertedSID.Append(auth);
                }
                else
                {
                    Int64 iVal = (Convert.ToInt32(sidBytes[1]) + Convert.ToInt32(sidBytes[2] << 8) + Convert.ToInt32(sidBytes[3] << 16) + Convert.ToInt32(sidBytes[4] << 24));
                    convertedSID.Append(iVal.ToString());
                }

                for (int i = 0; i < Convert.ToInt32(sidBytes[7]); i++)
                {
                    convertedSID.Append("-");
                    convertedSID.Append(BitConverter.ToUInt32(sidBytes, 8 + i * 4).ToString());
                }

                returnValue = convertedSID.ToString();
            }
            catch
            {
                returnValue = "";
            }

            return returnValue;
        }

        public static bool JoinAndSetName(string newName)
        {
            // Get WMI object for this machine
            using (ManagementObject wmiObject = new ManagementObject(new ManagementPath("Win32_ComputerSystem.Name='" + Environment.MachineName + "'")))
            {
                try
                {
                    // Obtain in-parameters for the method
                    ManagementBaseObject inParams = wmiObject.GetMethodParameters("JoinDomainOrWorkgroup");
                    inParams["Name"] = Target_FQDN;
                    Encryption64 encryption64 = new Encryption64();
                    inParams["Password"] = encryption64.Decrypt(@ConfigurationManager.AppSettings["Target_Password"], @"5r!zaV3Pm*Gl");
                    inParams["UserName"] = Target_UserName;
                    inParams["FJoinOptions"] = 35; // Magic number: 3 = join to domain and create computer account

                    // 1 (0x1) Default. Joins a computer to a domain. If this value is not specified, the join is a computer to a workgroup.
                    // 2 (0x2) Creates an account on a domain.
                    // 4 (0x4) Deletes an account when a domain exists.
                    // 16 (0x10) Not supported on supported versions of Windows.
                    // 32 (0x20) Allows a join to a new domain, even if the computer is already joined to a domain.
                    // 64 (0x40) Performs an unsecured join.
                    // 128 (0x80) The machine, not the user, password passed. This option is only valid for unsecure joins.
                    // 256 (0x100) Writing SPN and DnsHostName attributes on the computer object should be deferred until the rename that follows the join.
                    // 262144 (0x40000) The APIs were invoked during install.

                    // Execute the method and obtain the return values.
                    ManagementBaseObject joinParams = wmiObject.InvokeMethod("JoinDomainOrWorkgroup", inParams, null);

                    // Did it work?
                    if ((uint)(joinParams.Properties["ReturnValue"].Value) != 0)
                    {
                        // Join to domain didn't work
                        MessageBox.Show(string.Format("Normal fail {0}", joinParams.Properties["ReturnValue"].Value));
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    // Join to domain didn't work
                    MessageBox.Show(string.Format("Exception fail {0}", ex.Message));
                    return false;
                }

                // Join to domain worked - now change name
                if (Environment.MachineName.Trim().ToUpper() != newName.Trim().ToUpper())
                {
                    ManagementBaseObject inputArgs = wmiObject.GetMethodParameters("Rename");
                    inputArgs["Name"] = newName.Trim();
                    inputArgs["Password"] = "domain_account_password";
                    inputArgs["UserName"] = "domain_account";

                    // Set the name
                    ManagementBaseObject nameParams = wmiObject.InvokeMethod("Rename", inputArgs, null);

                    if ((uint)(nameParams.Properties["ReturnValue"].Value) != 0)
                    {
                        // Name change didn't work
                        return false;
                    }

                    // All ok
                    return true;
                }
                else
                {
                    // No name change required
                    return true;
                }
            }
        }

        public string GetSidFromXML(string userName)
        {
            XmlDocument objXmlDocument = new XmlDocument();
            XmlNodeList objXmlNodeList;

            objXmlDocument.Load("Users.xml"); // DO THIS WITH FULL PATH
            objXmlNodeList = objXmlDocument.SelectNodes("/Users/User");


            foreach (XmlNode objXmlNode in objXmlNodeList)
            {
                if (objXmlNode.ChildNodes.Item(0).InnerText.Trim().Replace("***AmPeRsAnD***", "&").ToUpper() == userName.Trim().ToUpper())
                {
                    return objXmlNode.ChildNodes.Item(1).InnerText;
                }
            }
            return "";
        }
        





        private bool registerSetACL()
        {
            try
            {
                string setACLPath = "";

                if (Functions.GetOSArchitecture() == 64)
                {
                    setACLPath = AppDomain.CurrentDomain.BaseDirectory + @"64 bit\SetACL.dll";
                }
                else
                {
                    setACLPath = AppDomain.CurrentDomain.BaseDirectory + @"32 bit\SetACL.dll";
                }

                return Functions.RegisterDll(setACLPath);
            }
            catch
            {

            }
            return false;
        }

        //private void elevateMe()
        //{
        //    string appFile = Environment.GetCommandLineArgs()[0].Replace(".vshost", "");

        //    string userName = migrator.Administrator_UserName;
        //    string domain = migrator.Administrator_Domain;
        //    SecureString password = migrator.Administrator_Password;

        //    //MessageBox.Show(System.Environment.OSVersion.Version.Major.ToString());

        //    WindowsIdentity user = WindowsIdentity.GetCurrent();
        //    WindowsPrincipal principal = new WindowsPrincipal(user);
        //    if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
        //    {
        //        if (Functions.MemberOfAdministratorsGroup())
        //        {
        //            var pInfo = new ProcessStartInfo();
        //            pInfo.FileName = appFile;
        //            pInfo.WorkingDirectory = Path.GetDirectoryName(appFile);
        //            pInfo.Arguments = @"SelfElevating """ + @oldUserSID + @""" """ + @oldUserName + @""" """ + @profilePath + @"""";
        //            pInfo.LoadUserProfile = true;
        //            pInfo.UseShellExecute = true;
        //            if (System.Environment.OSVersion.Version.Major >= 6)
        //            {
        //                pInfo.Verb = "runas";
        //            }

        //            Process.Start(pInfo);
        //            Environment.Exit(0);
        //            //Application.Exit();
        //        }
        //        else
        //        {
        //            if (Environment.GetCommandLineArgs().GetUpperBound(0) == 0)
        //            {
        //                // Try RunAsElevating
        //                var pInfo = new ProcessStartInfo();
        //                pInfo.FileName = appFile;
        //                pInfo.WorkingDirectory = Path.GetDirectoryName(appFile);
        //                pInfo.Arguments = @"RunAsElevating """ + @oldUserSID + @""" """ + @oldUserName + @""" """ + @profilePath + @"""";
        //                pInfo.LoadUserProfile = true;
        //                pInfo.UseShellExecute = false;
        //                pInfo.UserName = userName;
        //                pInfo.Password = password;
        //                pInfo.Domain = domain;
        //                if (System.Environment.OSVersion.Version.Major >= 6)
        //                {
        //                    pInfo.Verb = "runas";
        //                }

        //                Process.Start(pInfo);
        //                Environment.Exit(0);
        //            }
        //            else
        //            {
        //                if (Environment.GetCommandLineArgs()[1] == "RunAsElevating")
        //                {
        //                    // RunAsElevating Failed
        //                    var pInfo = new ProcessStartInfo();
        //                    pInfo.FileName = appFile;
        //                    pInfo.WorkingDirectory = Path.GetDirectoryName(appFile);
        //                    pInfo.Arguments = @"SelfElevating """ + @oldUserSID + @""" """ + @oldUserName + @""" """ + @profilePath + @"""";
        //                    pInfo.LoadUserProfile = true;
        //                    pInfo.UseShellExecute = true;
        //                    if (System.Environment.OSVersion.Version.Major >= 6)
        //                    {
        //                        pInfo.Verb = "runas";
        //                    }

        //                    Process.Start(pInfo);
        //                    Environment.Exit(0);
        //                    MessageBox.Show("Elevation failed");
        //                    Environment.Exit(0);
        //                }
        //                else if (Environment.GetCommandLineArgs()[1] == "SelfElevating")
        //                {
        //                    // Check rights on HKLM Profiles
        //                    // Message about not being able to join domain
        //                    MessageBox.Show("Elevation failed");
        //                    Environment.Exit(0);
        //                }
        //                else
        //                {
        //                    MessageBox.Show("Unknown");
        //                    Environment.Exit(0);
        //                }
        //            }
        //        }
        //    };
        //}
    }
}
