// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using Windows.Storage;
using Microsoft.UI.Dispatching;
using static PInvoke.Kernel32;
using Windows.Storage.Pickers;
using System.IO;
using System.Text;
using Windows.Storage.Provider;
using System.Text.RegularExpressions;

namespace NetworkSelector
{
    public sealed partial class NetSetting : Page
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private DispatcherQueue _dispatcherQueue;

        public NetSetting()
        {
            this.InitializeComponent();

            // 获取UI线程的DispatcherQueue
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            if (localSettings.Values["configName"] == null)
            {
                configName.SelectedItem = ConfigSelector[0];
                localSettings.Values["configName"] = ConfigSelector[0];
                refreshContent("自动");
            }
            else
            {
                configName.SelectedItem = localSettings.Values["configName"];
                refreshContent(localSettings.Values["configName"].ToString());
            }

            // adminFlag = 1 以管理员身份重启
            if (localSettings.Values["adminFlag"] as string == null)
            {
                localSettings.Values["adminFlag"] = "0";
            }
            else if (localSettings.Values["adminFlag"] as string == "1")
            {
                localSettings.Values["adminFlag"] = "0";
                applyConfig();
            }
        }
        public List<string> ConfigSelector { get; } = new List<string>()
        {
            "自动",
            "预设1",
            "预设2",
            "预设3",
            "预设4",
            "预设5",
            "预设6",
            "预设7",
            "预设8",
            "预设9",
            "预设10"
        };
        // 应用保存的设置
        public void applyConfig()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

            if (isAdmin)
            {
                netshChildThread();
            }
            else
            {
                NotAdminTips.IsOpen = true;
            }
        }
        private void netshChildThread()
        {
            // 关闭“应用配置”按钮
            applyConfigButton.IsEnabled = false;
            // 在子线程中执行任务
            Thread subThread = new Thread(new ThreadStart(() =>
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
                // 要在UI线程上更新UI，使用DispatcherQueue
                _dispatcherQueue.TryEnqueue(() =>
                {
                    NetworkIsChangeTips.IsOpen = true;
                    // 打开“应用配置”按钮
                    applyConfigButton.IsEnabled = true;
                });
            }));
            subThread.Start();
        }

        private void refreshCMD(string netInterface, string IPAddr, string Mask, string Gateway, string DNS1, string DNS2)
        {
            // 根据选择自动或手动，写入不同的netshCMD
            if ((configName.SelectedItem as string) == "自动")
            {
                localSettings.Values["netshCMD"] =
                    "netsh interface ip set address '" + netInterface + "' dhcp;"
                    + "netsh interface ip set dns name='" + netInterface + "' source=dhcp;";
            }
            else
            {
                localSettings.Values["netshCMD"] =
                "netsh interface ip set address name='" + netInterface + "' source=static addr='" + IPAddr + "' mask='" + Mask + "' gateway='" + Gateway + "'; "
                + "netsh interface ip set dns name='" + netInterface + "' source=static addr='" + DNS1 + "' register=primary;"
                + "netsh interface ip add dns name='" + netInterface + "' addr='" + DNS2 + "' index=2;";
            }
        }
        // 刷新Content 调取localSettings存储内容
        public void refreshContent(string ConfigNameStr)
        {
            // 隐藏覆盖 显示导入
            ImportConfig.Visibility = Visibility.Visible;
            ImportAndReplaceConfig.Visibility = Visibility.Collapsed;

            addConfigButton.Content = "添加配置";
            delConfigButton.IsEnabled = false;
            ExportConfig.IsEnabled = false;
            ImportConfig.IsEnabled = true;
            ImportAndReplaceConfig.IsEnabled = true;
            addConfigButton.IsEnabled = true;
            delConfigButton.IsEnabled = true;
            applyConfigButton.IsEnabled = true;
            List<Item> items = new List<Item>();
            string configInner = localSettings.Values["ConfigID" + ConfigNameStr] as string;
            if (configInner != null)
            {
                string[] configInnerSplit = configInner.Split(',');
                //IPAddr.Text + "," + mask.Text + "," + gateway.Text + "," + DNS1.Text + "," + DNS2.Text + "," + configName.Text + "," + netInterface.Text;
                string configName = configInnerSplit[5];
                string netInterface = configInnerSplit[6];
                string IPAddr = configInnerSplit[0];
                string Mask = configInnerSplit[1];
                string Gateway = configInnerSplit[2];
                string DNS1 = configInnerSplit[3];
                string DNS2 = configInnerSplit[4];
                if (ConfigNameStr != "自动")
                {
                    items.Add(new Item(
                        "配置名称：" + configName,
                        "网卡：" + netInterface,
                        "IP 地址：" + IPAddr,
                        "子网掩码：" + Mask,
                        "网关：" + Gateway,
                        "首选 DNS：" + DNS1,
                        "次选 DNS：" + DNS2
                        ));

                    // 隐藏导入 显示覆盖
                    ImportConfig.Visibility = Visibility.Collapsed;
                    ImportAndReplaceConfig.Visibility = Visibility.Visible;

                    delConfigButton.IsEnabled = true;
                    ExportConfig.IsEnabled = true;
                    addConfigButton.Content = "修改配置";
                    refreshCMD(netInterface, IPAddr, Mask, Gateway, DNS1, DNS2);
                }
                else
                {
                    items.Add(new Item(
                        "配置名称：DHCP",
                        "网卡：" + netInterface,
                        "IP 地址：DHCP",
                        "子网掩码：DHCP",
                        "网关：DHCP",
                        "首选 DNS：DHCP",
                        "次选 DNS：DHCP"
                        ));
                    ImportConfig.IsEnabled = false;
                    ImportAndReplaceConfig.IsEnabled = false;
                    addConfigButton.Content = "修改配置";
                    refreshCMD(netInterface, "DHCP", "DHCP", "DHCP", "DHCP", "DHCP");
                }
            }
            else
            {
                if (ConfigNameStr != "自动")
                {
                    items.Add(new Item(
                        "配置名称：",
                        "网卡：",
                        "IP 地址：",
                        "子网掩码：",
                        "网关：",
                        "首选 DNS：",
                        "次选 DNS："
                        ));
                    addConfigButton.Content = "添加配置";
                    delConfigButton.IsEnabled = false;
                    applyConfigButton.IsEnabled = false;
                    refreshCMD("", "", "", "", "", "");
                }
                else
                {
                    items.Add(new Item(
                        "配置名称：DHCP",
                        "网卡：",
                        "IP 地址：DHCP",
                        "子网掩码：DHCP",
                        "网关：DHCP",
                        "首选 DNS：DHCP",
                        "次选 DNS：DHCP"
                        ));
                    ImportConfig.IsEnabled = false;
                    ImportAndReplaceConfig.IsEnabled = false;
                    delConfigButton.IsEnabled = false;
                    applyConfigButton.IsEnabled = false;
                    addConfigButton.Content = "修改配置";
                    refreshCMD("", "DHCP", "DHCP", "DHCP", "DHCP", "DHCP");
                }
            }
            MyGridView.ItemsSource = items;
        }
        // 自动或预设切换
        private void configName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((configName.SelectedItem as string) == "自动")
            {
                localSettings.Values["DHCPStatus"] = "True";
            }
            else
            {
                localSettings.Values["DHCPStatus"] = "False";
            }
            refreshContent(configName.SelectedItem.ToString());
        }
        private async void addConfigButton_Click(object sender, RoutedEventArgs e)
        {
            string ConfigNameStr = configName.SelectedItem.ToString();
            localSettings.Values["ConfigIDTemp"] = localSettings.Values["ConfigID" + ConfigNameStr];

            AddConfigDialog configDialog = new AddConfigDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            configDialog.XamlRoot = this.XamlRoot;
            configDialog.Style = Microsoft.UI.Xaml.Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            //configDialog.Title = "添加一个 IP 测试";
            configDialog.PrimaryButtonText = "添加";
            configDialog.CloseButtonText = "关闭";
            configDialog.DefaultButton = ContentDialogButton.Primary;
            //dialog.Content = new AddPingDialog();

            var result = await configDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                localSettings.Values["ConfigID" + ConfigNameStr] = localSettings.Values["ConfigIDTemp"];
                refreshContent(ConfigNameStr);
            }
        }
        private void applyConfigButton_Click(object sender, RoutedEventArgs e)
        {
            localSettings.Values["configName"] = configName.SelectedItem;
            applyConfig();
        }
        private void delConfigButton_Click(object sender, RoutedEventArgs e)
        {
            string ConfigNameStr = configName.SelectedItem.ToString();
            localSettings.Values["ConfigID" + ConfigNameStr] = null;
            refreshContent(ConfigNameStr);
            if (this.delConfigButton.Flyout is Flyout f)
            {
                f.Hide();
            }
        }
        private void NotAdminTips_ActionButtonClick(TeachingTip sender, object args)
        {
            localSettings.Values["adminFlag"] = "1";
            RestartAsAdmin();
        }
        // 添加/修改配置按钮点击
        private async void AddConfigButton_Click(object sender, RoutedEventArgs e)
        {
            string ConfigIDNum = configName.SelectedItem.ToString();
            // 将ConfigIDTemp所存储的字符串设置为当前配置所存储的字符串
            // 这样可以实现“修改”的操作
            localSettings.Values["ConfigIDTemp"] = localSettings.Values["ConfigID" + ConfigIDNum];

            // 创建一个新的dialog对象
            AddConfigDialog configDialog = new AddConfigDialog();

            // 对此dialog对象进行配置
            configDialog.XamlRoot = this.XamlRoot;
            configDialog.Style = Microsoft.UI.Xaml.Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            // 根据内容有无来决定PrimaryButton的内容
            if (addConfigButton.Content.ToString() == "修改配置")
            {
                configDialog.PrimaryButtonText = "修改";
            }
            else
            {
                configDialog.PrimaryButtonText = "添加";
            }
            configDialog.CloseButtonText = "关闭";
            // 默认按钮为PrimaryButton
            configDialog.DefaultButton = ContentDialogButton.Primary;

            // 异步获取按下哪个按钮
            var result = await configDialog.ShowAsync();

            // 如果按下了Primary
            if (result == ContentDialogResult.Primary)
            {
                // 将ConfigIDTemp写入到当前配置ID下的localSettings
                localSettings.Values["ConfigID" + ConfigIDNum] = localSettings.Values["ConfigIDTemp"];
                // 刷新UI
                refreshContent(ConfigIDNum);
            }
        }
        // 删除配置按钮点击
        private void DelConfigButton_Click(object sender, RoutedEventArgs e)
        {
            string ConfigIDNum = configName.SelectedItem.ToString();
            // 删除配置
            // 清空指定ConfigIDNum的localSettings
            localSettings.Values["ConfigID" + ConfigIDNum] = null;
            // 刷新UI
            refreshContent(ConfigIDNum);
            // 隐藏提示Flyout
            if (this.delConfigButton.Flyout is Flyout f)
            {
                f.Hide();
            }
        }
        // 导入配置按钮点击
        private async void ImportConfig_Click(object sender, RoutedEventArgs e)
        {
            string ConfigIDNum = configName.SelectedItem.ToString();

            // 创建一个FileOpenPicker
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
            // 获取当前窗口句柄 (HWND) 
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.m_window);
            // 使用窗口句柄 (HWND) 初始化FileOpenPicker
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            // 为FilePicker设置选项
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            // 建议打开位置 桌面
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            // 文件类型过滤器
            openPicker.FileTypeFilter.Add(".nsconfig");

            // 打开选择器供用户选择文件
            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                var path = file.Path;
                localSettings.Values["ConfigID" + ConfigIDNum] = File.ReadAllText(path, Encoding.UTF8);
                // 处理掉一些非法字符
                localSettings.Values["ConfigID" + ConfigIDNum] = Regex.Replace(localSettings.Values["ConfigID" + ConfigIDNum] as string, @"\r\n?|\n", "");
                // 刷新UI
                refreshContent(ConfigIDNum);
            }
        }
        // 导出配置按钮点击
        private async void ExportConfig_Click(object sender, RoutedEventArgs e)
        {
            // 从localSettings中读取字符串
            string ConfigIDNum = configName.SelectedItem.ToString();
            string configInner = localSettings.Values["ConfigID" + ConfigIDNum] as string;
            // 如果字符串非空
            if (configInner != null)
            {
                // 分割字符串
                string[] configInnerSplit = configInner.Split(',');
                // 传入的字符串结构：
                // IPAddr.Text + "," + mask.Text + "," + gateway.Text + "," + DNS1.Text + "," + DNS2.Text + "," + configName.Text + "," + netInterface.Text;
                string configName = configInnerSplit[5];

                string configContent = localSettings.Values["ConfigID" + ConfigIDNum].ToString();

                // 创建一个FileSavePicker
                FileSavePicker savePicker = new Windows.Storage.Pickers.FileSavePicker();
                // 获取当前窗口句柄 (HWND) 
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.m_window);
                // 使用窗口句柄 (HWND) 初始化FileSavePicker
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);

                // 为FilePicker设置选项
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                // 用户可以将文件另存为的文件类型下拉列表
                savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".nsconfig" });
                // 默认文件名
                savePicker.SuggestedFileName = configName + "_BackUp_" + DateTime.Now.ToString();

                // 打开Picker供用户选择文件
                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    // 阻止更新文件的远程版本，直到我们完成更改并调用 CompleteUpdatesAsync。
                    CachedFileManager.DeferUpdates(file);

                    // 写入文件
                    using (var stream = await file.OpenStreamForWriteAsync())
                    {
                        using (var tw = new StreamWriter(stream))
                        {
                            tw.WriteLine(configContent);
                        }
                    }

                    // 让Windows知道我们已完成文件更改，以便其他应用程序可以更新文件的远程版本。
                    // 完成更新可能需要Windows请求用户输入。
                    FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                    if (status == FileUpdateStatus.Complete)
                    {
                        // 文件保存成功
                        SaveConfigTips.Title = "保存成功！";
                        SaveConfigTips.IsOpen = true;
                    }
                    else if (status == FileUpdateStatus.CompleteAndRenamed)
                    {
                        // 文件重命名并保存成功
                        SaveConfigTips.Title = "重命名并保存成功！";
                        SaveConfigTips.IsOpen = true;
                    }
                    else
                    {
                        // 文件无法保存！
                        SaveConfigTips.Title = "无法保存！";
                        SaveConfigTips.IsOpen = true;
                    }
                }
            }
        }
        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            string ConfigNameStr = configName.SelectedItem.ToString();
            //localSettings.Values[ConfigNameStr + "netName"] = netName.Text;
            refreshContent(ConfigNameStr);
        }
        private static void RestartAsAdmin()
        {
            string appPath = Process.GetCurrentProcess().MainModule.FileName;

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = appPath;
            startInfo.UseShellExecute = true;
            startInfo.Verb = "runas"; // 使用管理员权限启动

            try
            {
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine("重新启动失败: " + ex.Message);
            }
            Environment.Exit(0);
        }
    }
    public class Item
    {
        // 配置名称
        public string ConfigName { get; set; }
        // 网卡名称
        public string NetInterface { get; set; }
        // IP地址
        public string IPAddr { get; set; }
        // 子网掩码
        public string Mask { get; set; }
        // 网关
        public string Gateway { get; set; }
        // DNS
        public string DNS1 { get; set; }
        public string DNS2 { get; set; }
        public Item(string configName, string netInterface, string ipAddr, string mask, string gateway, string dns1, string dns2)
        {
            ConfigName = configName;
            NetInterface = netInterface;
            IPAddr = ipAddr;
            Mask = mask;
            Gateway = gateway;
            DNS1 = dns1;
            DNS2 = dns2;
        }
    }
}
