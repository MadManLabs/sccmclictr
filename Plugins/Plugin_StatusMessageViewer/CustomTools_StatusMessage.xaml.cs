﻿using System;
using System.Windows;

using System.Diagnostics;

using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.IO;
using System.Management;


namespace AgentActionTools
{
    /// <summary>
    /// Interaction logic for CustomTools_CMResource.xaml
    /// </summary>
    public partial class CustomTools_CMResource : System.Windows.Controls.UserControl
    {
        public CustomTools_CMResource()
        {
            InitializeComponent();

            btOpenCMStatView.IsEnabled = SCCMCliCtr.Customization.CheckLicense();
            if (!btOpenCMStatView.IsEnabled)
            {
                btOpenCMStatView.ToolTip = "Please make a donation to get access to this feature !";
            }

            if (!File.Exists(System.IO.Path.Combine(SCCMConsolePath, @"bin\i386\statview.exe")))
            {
                btOpenCMStatView.IsEnabled = false;
                btOpenCMStatView.ToolTip = "ConfigMgr Admin Console is missing !";
            }
        }

        private void btOpenCMStatView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Type t = System.Reflection.Assembly.GetEntryAssembly().GetType("ClientCenter.Common", false, true);
                //Get the Hostname
                System.Reflection.PropertyInfo pInfo = t.GetProperty("Hostname");
                string sHost = (string)pInfo.GetValue(null, null).ToString().Split('.')[0];

                Process pRemote = new Process();

                string sServer = SCCMServer.Replace(@"\", "");
                string sSiteCode = SCCMSiteCode(sServer);

                pRemote.StartInfo.FileName = Properties.Settings.Default.RemoteCommand;
                if (Properties.Settings.Default.RequireAdminConsole)
                    pRemote.StartInfo.WorkingDirectory = System.IO.Path.Combine(SCCMConsolePath, @"bin\i386");
                pRemote.StartInfo.Arguments = string.Format(Properties.Settings.Default.RemoteArgument, sHost, sServer, sSiteCode);
                pRemote.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                pRemote.Start();

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public string SCCMConsolePath
        {
            get
            {
                try
                {
                    //RegistryKey rAdminUI = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\ConfigMgr\Setup");
                    string sArchitecture = System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE").ToLower();
                    RegistryKey rAdminUI = null;
                    switch (sArchitecture)
                    {
                        case "x86":
                            rAdminUI = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\ConfigMgr10\Setup");
                            break;
                        case "amd64":
                            rAdminUI = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\ConfigMgr10\Setup");
                            break;
                        case "ia64":
                            rAdminUI = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\ConfigMgr10\Setup");
                            break;
                    }
                    if (rAdminUI != null)
                    {
                        string sUIPath = rAdminUI.GetValue("UI Installation Directory", "").ToString();
                        if (Directory.Exists(sUIPath))
                        {
                            return sUIPath;
                        }
                    }
                }
                catch { }
                return "";
            }
        }

        [DllImport("User32.dll", SetLastError = true)]
        public static extern int SetForegroundWindow(IntPtr hwnd);

        public string SCCMServer
        {
            get
            {
                try
                {
                    string sArchitecture = System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE").ToLower();
                    RegistryKey rAdminCon = null;
                    switch (sArchitecture)
                    {
                        case "x86":
                            rAdminCon = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\ConfigMgr10\AdminUI\Connection");
                            break;
                        case "amd64":
                            rAdminCon = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\ConfigMgr10\AdminUI\Connection");
                            break;
                        case "ia64":
                            rAdminCon = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\ConfigMgr10\AdminUI\Connection");
                            break;
                    }
                    if (rAdminCon != null)
                    {
                        string sServer = rAdminCon.GetValue("Server", "").ToString();
                        return sServer;
                    }
                }
                catch { }
                return "";
            }
        }

        public string SCCMSiteCode(string sServer)
        {
            try
            {
                ManagementObjectSearcher MOS = new ManagementObjectSearcher(string.Format(@"\\{0}\root\sms", sServer), "SELECT * FROM SMS_ProviderLocation");
                foreach (ManagementObject MO in MOS.Get())
                {
                    return MO["SiteCode"].ToString();
                }
            }
            catch { }
            return "";
        }


    }
}
