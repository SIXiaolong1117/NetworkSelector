// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using static System.Net.Mime.MediaTypeNames;

namespace NetworkSelector
{
    public sealed partial class NetSetting : Page
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public List<string> ConfigSelector { get; } = new List<string>()
        {
            "自动",
            "预设1",
            "预设2",
            "预设3",
            "预设4",
            "预设5"
        };
        public void saveConfig()
        {

            localSettings.Values[(configName.SelectedItem as string) + "netName"] = netName.Text;
            localSettings.Values[(configName.SelectedItem as string) + "IPAddr"] = IPAddr.Text;
            localSettings.Values[(configName.SelectedItem as string) + "mask"] = mask.Text;
            localSettings.Values[(configName.SelectedItem as string) + "gateway"] = gateway.Text;
            localSettings.Values[(configName.SelectedItem as string) + "DNS1"] = DNS1.Text;
            localSettings.Values[(configName.SelectedItem as string) + "DNS2"] = DNS2.Text;
        }
        public void applyConfig()
        {
            if ((configName.SelectedItem as string) == "自动")
            {
                localSettings.Values["netshCMD"] =
                    "netsh interface ip set address '" + netName.Text + "' dhcp;"
                    + "netsh interface ip set dns name='" + netName.Text + "' source=dhcp;";
            }
            else
            {
                localSettings.Values["netshCMD"] =
                "netsh interface ip set address name='" + netName.Text + "' source=static addr='" + IPAddr.Text + "' mask='" + mask.Text + "' gateway='" + gateway.Text + "'; "
                + "netsh interface ip set dns name='" + netName.Text + "' source=static addr='" + DNS1.Text + "' register=primary;"
                + "netsh interface ip add dns name='" + netName.Text + "' addr='" + DNS2.Text + "' index=2;";
            }

            netshCMD.Text = localSettings.Values["netshCMD"] as string;
        }
        public void refreshContent()
        {
            netName.Text = localSettings.Values[(configName.SelectedItem as string) + "netName"] as string;
            IPAddr.Text = localSettings.Values[(configName.SelectedItem as string) + "IPAddr"] as string;
            mask.Text = localSettings.Values[(configName.SelectedItem as string) + "mask"] as string;
            gateway.Text = localSettings.Values[(configName.SelectedItem as string) + "gateway"] as string;
            DNS1.Text = localSettings.Values[(configName.SelectedItem as string) + "DNS1"] as string;
            DNS2.Text = localSettings.Values[(configName.SelectedItem as string) + "DNS2"] as string;
        }
        public void refreshStatus()
        {
            if ((configName.SelectedItem as string) == "自动")
            {
                IPAddr.IsEnabled = false;
                mask.IsEnabled = false;
                gateway.IsEnabled = false;
                DNS1.IsEnabled = false;
                DNS2.IsEnabled = false;
            }
            else
            {
                IPAddr.IsEnabled = true;
                mask.IsEnabled = true;
                gateway.IsEnabled = true;
                DNS1.IsEnabled = true;
                DNS2.IsEnabled = true;
            }
        }
        public NetSetting()
        {
            this.InitializeComponent();

            configName.SelectedItem = ConfigSelector[0];

            refreshContent();
            refreshStatus();
        }
        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            saveConfig();
        }
        private void saveApplyButton_Click(object sender, RoutedEventArgs e)
        {
            saveConfig();
            applyConfig();
            //ThreadStart childref = new ThreadStart(saveApplyChildThread);
            //Thread childThread = new Thread(childref);
            //childThread.Start();
            Process process = new Process();
            process.StartInfo.FileName = "PowerShell.exe";
            process.StartInfo.Arguments = localSettings.Values["netshCMD"] as string;
            //是否使用操作系统shell启动
            process.StartInfo.UseShellExecute = true;
            //是否在新窗口中启动该进程的值 (不显示程序窗口)
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();
            process.Close();
        }
        public void saveApplyChildThread()
        {
            saveConfig();
        }
        private void configName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            refreshContent();
            refreshStatus();
        }
    }
}
