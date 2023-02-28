// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.VisualBasic.FileIO;
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
using WinRT;
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
        // 保存设置内容到localSettings
        public void saveConfig()
        {
            localSettings.Values[(configName.SelectedItem as string) + "netName"] = netName.Text;
            localSettings.Values[(configName.SelectedItem as string) + "IPAddr"] = IPAddr.Text;
            localSettings.Values[(configName.SelectedItem as string) + "mask"] = mask.Text;
            localSettings.Values[(configName.SelectedItem as string) + "gateway"] = gateway.Text;
            localSettings.Values[(configName.SelectedItem as string) + "DNS1"] = DNS1.Text;
            localSettings.Values[(configName.SelectedItem as string) + "DNS2"] = DNS2.Text;

            // 根据选择自动或手动，写入不同的netshCMD
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
        // 应用保存的设置
        public void applyConfig()
        {
            Process process = new Process();
            process.StartInfo.FileName = "PowerShell.exe";
            process.StartInfo.Arguments = localSettings.Values["netshCMD"] as string;
            //是否使用操作系统shell启动
            process.StartInfo.UseShellExecute = false;
            //是否在新窗口中启动该进程的值 (不显示程序窗口)
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();
            process.Close();
            localSettings.Values["configName"] = configName.SelectedItem;
        }
        // 刷新Content 调取localSettings存储内容
        public void refreshContent()
        {
            netName.Text = localSettings.Values[(configName.SelectedItem as string) + "netName"] as string;
            IPAddr.Text = localSettings.Values[(configName.SelectedItem as string) + "IPAddr"] as string;
            mask.Text = localSettings.Values[(configName.SelectedItem as string) + "mask"] as string;
            gateway.Text = localSettings.Values[(configName.SelectedItem as string) + "gateway"] as string;
            DNS1.Text = localSettings.Values[(configName.SelectedItem as string) + "DNS1"] as string;
            DNS2.Text = localSettings.Values[(configName.SelectedItem as string) + "DNS2"] as string;
        }
        // 刷新Status 调用localSettings存储状态
        public void refreshStatus()
        {
            if ((configName.SelectedItem as string) == "自动")
            {
                optionToHide.Visibility = Visibility.Collapsed;
                IPAddr.IsEnabled = false;
                mask.IsEnabled = false;
                gateway.IsEnabled = false;
                DNS1.IsEnabled = false;
                DNS2.IsEnabled = false;
            }
            else
            {
                optionToHide.Visibility = Visibility.Visible;
                IPAddr.IsEnabled = true;
                mask.IsEnabled = true;
                gateway.IsEnabled = true;
                DNS1.IsEnabled = true;
                DNS2.IsEnabled = true;
            }
            if (localSettings.Values["CMDDisplay"] as string == "是")
            {
                netshCMD.Visibility = Visibility.Visible;
            }
            else if (localSettings.Values["CMDDisplay"] as string == "否")
            {
                netshCMD.Visibility = Visibility.Collapsed;
            }
            else
            {
                localSettings.Values["CMDDisplay"] = "否";
                netshCMD.Visibility = Visibility.Collapsed;
            }
        }
        // 页面初始化
        public NetSetting()
        {
            this.InitializeComponent();

            if (localSettings.Values["configName"] == null)
            {
                configName.SelectedItem = ConfigSelector[0];
                localSettings.Values["configName"] = ConfigSelector[0];
            }
            else
            {
                configName.SelectedItem = localSettings.Values["configName"];
            }

            refreshContent();
            refreshStatus();
        }
        // 保存为预设 按钮
        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            saveConfig();
        }
        // 保存并应用 按钮
        private void saveApplyButton_Click(object sender, RoutedEventArgs e)
        {
            saveConfig();
            applyConfig();
        }
        // 自动或预设切换
        private void configName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            refreshContent();
            refreshStatus();
        }
    }
}
