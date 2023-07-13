using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Text;

namespace ProfileMigrator
{
    class Logging
    {
        private static string logName = "ProfileMigrator";
        private static string sourceName = "ProfileMigrator";

        public static void CreateEventLog()
        {
            if (!EventLog.SourceExists(sourceName))
            {
                try
                {
                    EventLog.CreateEventSource(sourceName, logName);
                }
                catch { }
            }
        }
        public static void DeleteEventLog()
        {
            if (EventLog.SourceExists(sourceName))
            {
                try
                {
                    EventLog.Delete(logName);
                    EventLog.DeleteEventSource(sourceName);
                }
                catch { }
            }
        }
        public static void WriteEventLog(int eventId, string message, EventLogEntryType type)
        {
            try
            {
                if (!EventLog.SourceExists(sourceName))
                {
                    CreateEventLog();
                }
            }
            catch
            {

            }

            try
            {
                EventLog.WriteEntry(sourceName, message, type, eventId);

                System.IO.StreamWriter file = new System.IO.StreamWriter("ProfileMigrator.log", true);
                file.WriteLine(string.Format("<{0}> <{1}>   <{2}>   <{3}>", DateTime.Now, eventId, type.ToString(), message));
                file.Close();
            }
            catch
            {

            }
        }
    }
    class Functions
    {
        static public bool RegisterDll(string filePath)
        {
            try
            {
                //'/s' : Specifies regsvr32 to run silently and to not display any message boxes.
                string arg_fileinfo = "/s" + " " + "\"" + filePath + "\"";

                //MessageBox.Show("regsvr32.exe " + arg_fileinfo);

                Process reg = new Process();
                //This file registers .dll files as command components in the registry.
                reg.StartInfo.FileName = "regsvr32.exe";
                reg.StartInfo.Arguments = arg_fileinfo;
                reg.StartInfo.UseShellExecute = false;
                reg.StartInfo.CreateNoWindow = true;
                reg.StartInfo.RedirectStandardOutput = true;
                reg.Start();
                reg.WaitForExit();
                reg.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
        static public int GetOSArchitecture()
        {
            string pa = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
            return ((String.IsNullOrEmpty(pa) || String.Compare(pa, 0, "x86", 0, 3, true) == 0) ? 32 : 64);
        }
        static private bool RecurseCopyKey(RegistryKey sourceKey, RegistryKey destinationKey)
        {
            bool returnValue = true;

            try
            {
                foreach (string valueName in sourceKey.GetValueNames())
                {
                    destinationKey.SetValue(valueName, sourceKey.GetValue(valueName));
                }

                foreach (string sourceSubKeyName in sourceKey.GetSubKeyNames())
                {
                    if (!RecurseCopyKey(sourceKey.OpenSubKey(sourceSubKeyName), destinationKey.CreateSubKey(sourceSubKeyName)))
                    {
                        returnValue = false;
                    }
                }
            }
            catch
            {
                returnValue = false;
            }

            return returnValue;
        }
        static public bool CopyKey(RegistryKey parentKey, string sourceKeyName, string targetKeyName)
        {
            bool returnValue = true;

            try
            {
                if (!Functions.RecurseCopyKey(parentKey.OpenSubKey(sourceKeyName), parentKey.CreateSubKey(targetKeyName)))
                {
                    returnValue = false;
                }
            }
            catch
            {
                returnValue = false;
            }

            return returnValue;
        }
        static public string GetCurrentUser()
        {
            string returnValue = "";

            try
            {
                //If current user name match user name in XML show, else don't
                returnValue = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

                if (returnValue.IndexOf("\\") != -1)
                {
                    returnValue = returnValue.Split('\\')[1];
                }
            }
            catch
            {
                returnValue = "";
            }

            return returnValue;
        }
        static public bool MemberOfAdministratorsGroup()
        {
            using (DirectoryEntry groupEntry = new DirectoryEntry("WinNT://./Administrators,group"))
            {
                foreach (object member in (IEnumerable)groupEntry.Invoke("Members"))
                {
                    using (DirectoryEntry memberEntry = new DirectoryEntry(member))
                    {
                        if (memberEntry.Name.ToLower() == Functions.GetCurrentUser().ToLower())
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        static public bool SetDefaultUserNameAndDomain(string userName, string domainName)
        {
            try
            {
                RegistryKey registryKey;
                string keyPath;

                try
                {
                    keyPath = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon";
                    registryKey = Registry.LocalMachine.OpenSubKey(keyPath, true);

                    registryKey.SetValue("AltDefaultUserName", userName);
                    registryKey.SetValue("DefaultUserName", userName);
                    registryKey.SetValue("AltDefaultDomainName", domainName);
                    registryKey.SetValue("DefaultDomainName", domainName);
                }
                catch
                {
                    return false;
                }

                try
                {
                    keyPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Authentication\\LogonUI";
                    registryKey = Registry.LocalMachine.OpenSubKey(keyPath, true);

                    registryKey.SetValue("LastLoggedOnUser", string.Format("{0}\\{1}", domainName, userName));
                    registryKey.SetValue("LastLoggedOnSAMUser", string.Format("{0}\\{1}", domainName, userName));
                }
                catch
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
    class Windows
    {
        [Flags]
        public enum ExitWindows : uint
        {
            // ONE of the following five:
            LogOff = 0x00,
            ShutDown = 0x01,
            Reboot = 0x02,
            PowerOff = 0x08,
            RestartApps = 0x40,
            // plus AT MOST ONE of the following two:
            Force = 0x04,
            ForceIfHung = 0x10,
        }

        [Flags]
        enum ShutdownReason : uint
        {
            MajorApplication = 0x00040000,
            MajorHardware = 0x00010000,
            MajorLegacyApi = 0x00070000,
            MajorOperatingSystem = 0x00020000,
            MajorOther = 0x00000000,
            MajorPower = 0x00060000,
            MajorSoftware = 0x00030000,
            MajorSystem = 0x00050000,

            MinorBlueScreen = 0x0000000F,
            MinorCordUnplugged = 0x0000000b,
            MinorDisk = 0x00000007,
            MinorEnvironment = 0x0000000c,
            MinorHardwareDriver = 0x0000000d,
            MinorHotfix = 0x00000011,
            MinorHung = 0x00000005,
            MinorInstallation = 0x00000002,
            MinorMaintenance = 0x00000001,
            MinorMMC = 0x00000019,
            MinorNetworkConnectivity = 0x00000014,
            MinorNetworkCard = 0x00000009,
            MinorOther = 0x00000000,
            MinorOtherDriver = 0x0000000e,
            MinorPowerSupply = 0x0000000a,
            MinorProcessor = 0x00000008,
            MinorReconfig = 0x00000004,
            MinorSecurity = 0x00000013,
            MinorSecurityFix = 0x00000012,
            MinorSecurityFixUninstall = 0x00000018,
            MinorServicePack = 0x00000010,
            MinorServicePackUninstall = 0x00000016,
            MinorTermSrv = 0x00000020,
            MinorUnstable = 0x00000006,
            MinorUpgrade = 0x00000003,
            MinorWMI = 0x00000015,

            FlagUserDefined = 0x40000000,
            FlagPlanned = 0x80000000
        }

        const int PrivilegeEnabled = 0x00000002;
        const int TokenQuery = 0x00000008;
        const int AdjustPrivileges = 0x00000020;
        const string ShutdownPrivilege = "SeShutdownPrivilege";

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct TokenPrivileges
        {
            public int PrivilegeCount;
            public long Luid;
            public int Attributes;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ExitWindowsEx(ExitWindows uFlags, ShutdownReason dwReason);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern int OpenProcessToken(
            IntPtr processHandle, int desiredAccess, ref IntPtr tokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern int LookupPrivilegeValue(
            string systemName, string name, ref long luid);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern int AdjustTokenPrivileges(
            IntPtr tokenHandle, bool disableAllPrivileges, ref TokenPrivileges newState,
            int bufferLength, IntPtr previousState, IntPtr length);

        static private void ElevatePrivileges()
        {
            IntPtr currentProcess = GetCurrentProcess();
            IntPtr tokenHandle = IntPtr.Zero;

            int result = OpenProcessToken(
                currentProcess, AdjustPrivileges | TokenQuery, ref tokenHandle);
            if (result == 0) throw new Win32Exception(Marshal.GetLastWin32Error());

            TokenPrivileges tokenPrivileges;
            tokenPrivileges.PrivilegeCount = 1;
            tokenPrivileges.Luid = 0;
            tokenPrivileges.Attributes = PrivilegeEnabled;

            result = LookupPrivilegeValue(null, ShutdownPrivilege, ref tokenPrivileges.Luid);
            if (result == 0) throw new Win32Exception(Marshal.GetLastWin32Error());

            result = AdjustTokenPrivileges(
                tokenHandle, false, ref tokenPrivileges, 0, IntPtr.Zero, IntPtr.Zero);
            if (result == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        static public bool Reboot()
        {
            try
            {
                ElevatePrivileges();
                return ExitWindowsEx(ExitWindows.Reboot, ShutdownReason.MajorOther | ShutdownReason.MinorOther | ShutdownReason.FlagPlanned);
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
