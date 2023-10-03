using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Dispatching;
using NetworkSelector.Datas;
using NetworkSelector.Models;
using NetworkSelector.Pages.Dialogs;
using System.Threading;
using System.Diagnostics;
using System.Security.Principal;
using Windows.ApplicationModel.DataTransfer;
using NetworkSelector.Methods;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;

namespace NetworkSelector.Pages
{
    public sealed partial class NetSelectPage : Page
    {
        private DispatcherQueue _dispatcherQueue;
        public NetSelectPage()
        {
            this.InitializeComponent();
            // 获取UI线程的DispatcherQueue
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            // 加载数据
            LoadData();
        }
        private void LoadData()
        {
            // 实例化SQLiteHelper
            SQLiteHelper dbHelper = new SQLiteHelper();
            // 查询数据
            List<NSModel> dataList = dbHelper.QueryData();

            // 将数据列表绑定到ListView
            dataListView.ItemsSource = dataList;

            DisplayNetworkInfo();
        }
        // 导入配置按钮点击
        private async void ImportConfig_Click(object sender, RoutedEventArgs e)
        {
            ImportConfig.IsEnabled = false;
            // 实例化SQLiteHelper
            SQLiteHelper dbHelper = new SQLiteHelper();
            // 获取导入的数据
            NSModel nsModel = await NSMethod.ImportConfig();
            if (nsModel != null)
            {
                // 插入新数据
                dbHelper.InsertData(nsModel);
                // 重新加载数据
                LoadData();
            }
            ImportConfig.IsEnabled = true;
        }
        private async void ExportConfigFunction()
        {
            // 获取NSModel对象
            NSModel selectedModel = (NSModel)dataListView.SelectedItem;
            string result = await NSMethod.ExportConfig(selectedModel);
        }
        // 添加/修改配置按钮点击
        private async void AddConfigButton_Click(object sender, RoutedEventArgs e)
        {
            // 创建一个初始的NSModel对象
            NSModel initialNSModelData = new NSModel();

            // 创建一个新的dialog对象
            AddNSConfig dialog = new AddNSConfig(initialNSModelData);
            // 对此dialog对象进行配置
            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.PrimaryButtonText = "添加";
            dialog.CloseButtonText = "关闭";
            // 默认按钮为PrimaryButton
            dialog.DefaultButton = ContentDialogButton.Primary;

            // 显示Dialog并等待其关闭
            ContentDialogResult result = await dialog.ShowAsync();

            // 如果按下了Primary
            if (result == ContentDialogResult.Primary)
            {
                // 实例化SQLiteHelper
                SQLiteHelper dbHelper = new SQLiteHelper();
                // 插入新数据
                dbHelper.InsertData(initialNSModelData);
                // 重新加载数据
                LoadData();
            }
        }
        private async void DHCPConfigButton_Click(object sender, RoutedEventArgs e)
        {
            DHCPInterfaceModel dhcpInterfaceModel = new DHCPInterfaceModel();
            // 创建一个新的dialog对象
            AddDHCPConfig dialog = new AddDHCPConfig(dhcpInterfaceModel);
            // 对此dialog对象进行配置
            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.PrimaryButtonText = "切换至DHCP";
            dialog.CloseButtonText = "关闭";
            // 默认按钮为PrimaryButton
            dialog.DefaultButton = ContentDialogButton.Primary;

            // 显示Dialog并等待其关闭
            ContentDialogResult result = await dialog.ShowAsync();

            // 如果按下了Primary
            if (result == ContentDialogResult.Primary)
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
                if (isAdmin)
                {
                    SeletcDHCPConfig(dhcpInterfaceModel.Netinterface);
                }
                else
                {
                    NotAdminTips.IsOpen = true;
                }
            }
        }
        private void RefreshConfigButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayNetworkInfo();
        }
        private void CopyThisConfig(NSModel model)
        {
            SQLiteHelper dbHelper = new SQLiteHelper();
            dbHelper.InsertData(model);
            // 重新加载数据
            LoadData();
        }
        private void SeletcThisConfig(NSModel model)
        {
            InProgressing.IsActive = true;
            string cmd = "netsh interface ip set address name='" + model.Netinterface + "' source=static addr='" + model.IPAddr + "' mask='" + model.Mask + "' gateway='" + model.Gateway + "'; "
            + "netsh interface ip set dns name='" + model.Netinterface + "' source=static addr='" + model.DNS1 + "' register=primary;"
            + "netsh interface ip add dns name='" + model.Netinterface + "' addr='" + model.DNS2 + "' index=2;";
            // 在子线程中执行任务
            Thread subThread = new Thread(new ThreadStart(() =>
                {
                    Process process = new Process();
                    process.StartInfo.FileName = "PowerShell.exe";
                    process.StartInfo.Arguments = cmd;
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
                        InProgressing.IsActive = false;
                        NetworkIsChangeTips.IsOpen = true;
                        DisplayNetworkInfo();
                    });
                }));
            subThread.Start();
        }
        private void SeletcDHCPConfig(string netInterface)
        {
            InProgressing.IsActive = true;
            string cmd = "netsh interface ip set address '" + netInterface + "' dhcp;"
                + "netsh interface ip set dns name='" + netInterface + "' source=dhcp;";
            // 在子线程中执行任务
            Thread subThread = new Thread(new ThreadStart(() =>
            {
                Process process = new Process();
                process.StartInfo.FileName = "PowerShell.exe";
                process.StartInfo.Arguments = cmd;
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
                    InProgressing.IsActive = false;
                    NetworkIsChangeTips.IsOpen = true;
                    DisplayNetworkInfo();
                });
            }));
            subThread.Start();
        }
        private async void EditThisConfig(NSModel nsModel)
        {
            // 创建一个新的dialog对象
            AddNSConfig dialog = new AddNSConfig(nsModel);
            // 对此dialog对象进行配置
            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.PrimaryButtonText = "修改";
            dialog.CloseButtonText = "关闭";
            // 默认按钮为PrimaryButton
            dialog.DefaultButton = ContentDialogButton.Primary;

            // 显示Dialog并等待其关闭
            ContentDialogResult result = await dialog.ShowAsync();

            // 如果按下了Primary
            if (result == ContentDialogResult.Primary)
            {
                // 实例化SQLiteHelper
                SQLiteHelper dbHelper = new SQLiteHelper();
                // 更新数据
                dbHelper.UpdateData(nsModel);
                // 重新加载数据
                LoadData();
            }
        }
        private void DelThisConfig(NSModel nsModel)
        {
            // 实例化SQLiteHelper
            SQLiteHelper dbHelper = new SQLiteHelper();
            // 删除数据
            dbHelper.DeleteData(nsModel);
            // 重新加载数据
            LoadData();
        }
        private async void ReplaceThisConfig(NSModel nsModel)
        {
            ImportConfig.IsEnabled = false;
            // 实例化SQLiteHelper
            SQLiteHelper dbHelper = new SQLiteHelper();

            // 获取导入的数据
            NSModel nsModel2 = await NSMethod.ImportConfig();

            if (nsModel2 != null)
            {
                // 获取当前配置的ID
                int id = nsModel.Id;
                // 赋给导入的配置
                nsModel2.Id = id;
                // 插入新数据
                dbHelper.UpdateData(nsModel2);
                // 重新加载数据
                LoadData();
            }
            ImportConfig.IsEnabled = true;
        }
        private void ConfirmDelete_Click(object sender, RoutedEventArgs e)
        {
            // 关闭二次确认Flyout
            confirmationDelFlyout.Hide();
            // 获取NSModel对象
            NSModel selectedModel = (NSModel)dataListView.SelectedItem;
            DelThisConfig(selectedModel);
        }

        private void CancelDelete_Click(object sender, RoutedEventArgs e)
        {
            // 关闭二次确认Flyout
            confirmationDelFlyout.Hide();
        }
        private void ConfirmReplace_Click(object sender, RoutedEventArgs e)
        {
            // 关闭二次确认Flyout
            confirmationReplaceFlyout.Hide();
            // 获取NSModel对象
            NSModel selectedModel = (NSModel)dataListView.SelectedItem;
            ReplaceThisConfig(selectedModel);
        }

        private void CancelReplace_Click(object sender, RoutedEventArgs e)
        {
            // 关闭二次确认Flyout
            confirmationReplaceFlyout.Hide();
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
        private void NotAdminTips_ActionButtonClick(TeachingTip sender, object args)
        {
            //localSettings.Values["adminFlag"] = "1";
            RestartAsAdmin();
        }
        private void DisplayNetworkInfo()
        {
            // 获取当前使用的网络接口的名称
            string targetNetworkInterfaceName = NSMethod.GetCurrentActiveNetworkInterfaceName();

            // 获取所有的网络接口
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var networkInterface in networkInterfaces)
            {
                // 仅处理与目标网络接口名称匹配的网络接口
                if (networkInterface.Name == targetNetworkInterfaceName)
                {
                    // 获取网络接口的IP属性
                    IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();

                    // 获取网络接口的名称
                    string interfaceName = networkInterface.Name;

                    // 获取IP地址
                    string ipAddress = ipProperties.UnicastAddresses
                        .FirstOrDefault(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.Address.ToString();

                    // 获取网络接口的MAC地址
                    string macAddress = networkInterface.GetPhysicalAddress().ToString();

                    // 检查是否是12个字符的MAC地址
                    if (macAddress.Length == 12)
                    {
                        macAddress = string.Join(":", Enumerable.Range(0, 6).Select(i => macAddress.Substring(i * 2, 2)));
                    }

                    // 获取子网掩码
                    string subnetMask = ipProperties.UnicastAddresses
                        .FirstOrDefault(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.IPv4Mask.ToString();

                    // 获取网关地址
                    string gatewayAddress = ipProperties.GatewayAddresses
                        .FirstOrDefault()?.Address.ToString();

                    // 获取DNS服务器信息
                    string dns1 = ipProperties.DnsAddresses.FirstOrDefault()?.ToString();
                    string dns2 = ipProperties.DnsAddresses.Skip(1).FirstOrDefault()?.ToString();

                    // 获取网络接口的类型（以太网、Wi-Fi等）
                    NetworkInterfaceType interfaceType = networkInterface.NetworkInterfaceType;

                    // 获取网络接口的速度（以比特每秒为单位）
                    long interfaceSpeed = networkInterface.Speed;

                    // 将信息输出到TextBlock
                    NetworkInterfaceName.Text = $"接口名称: \n{interfaceName}";
                    NetworkInterfaceDescription.Text = $"接口描述: \n{networkInterface.Description}";
                    NetworkInterfaceMACAddress.Text = $"物理地址 (MAC) 地址: \n{macAddress}";
                    NetworkInterfaceIPAddress.Text = $"IP 地址: \n{ipAddress}";
                    NetworkInterfaceSubnetMask.Text = $"子网掩码: \n{subnetMask}";
                    NetworkInterfaceGatewayAddress.Text = $"网关地址: \n{gatewayAddress}";
                    NetworkInterfaceDNS.Text = $"DNS 1: \n{dns1}\n{dns2}";
                    NetworkInterfaceType.Text = $"网络类型: \n{interfaceType.ToString()}";
                    NetworkInterfaceSpeed.Text = $"协商速度: \n{interfaceSpeed / 1000000} Mbps";
                }
            }
        }
        private void OnListViewRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (InProgressing.IsActive == false)
            {
                // 获取右键点击的ListViewItem
                FrameworkElement listViewItem = (sender as FrameworkElement);

                // 获取右键点击的数据对象（NSModel）
                NSModel selectedItem = listViewItem?.DataContext as NSModel;

                if (selectedItem != null)
                {

                    // 将右键点击的项设置为选中项
                    dataListView.SelectedItem = selectedItem;
                    // 创建ContextMenu
                    MenuFlyout menuFlyout = new MenuFlyout();

                    MenuFlyoutItem selectMenuItem = new MenuFlyoutItem
                    {
                        Text = "切换至此配置"
                    };
                    selectMenuItem.Click += (sender, e) =>
                    {
                        WindowsIdentity identity = WindowsIdentity.GetCurrent();
                        WindowsPrincipal principal = new WindowsPrincipal(identity);
                        bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
                        if (isAdmin)
                        {
                            SeletcThisConfig(selectedItem);
                        }
                        else
                        {
                            NotAdminTips.IsOpen = true;
                        }
                    };
                    menuFlyout.Items.Add(selectMenuItem);

                    // 添加分割线
                    MenuFlyoutSeparator separator = new MenuFlyoutSeparator();
                    menuFlyout.Items.Add(separator);

                    MenuFlyoutItem exportMenuItem = new MenuFlyoutItem
                    {
                        Text = "导出配置"
                    };
                    exportMenuItem.Click += (sender, e) =>
                    {
                        ExportConfigFunction();
                    };
                    menuFlyout.Items.Add(exportMenuItem);

                    MenuFlyoutItem replaceMenuItem = new MenuFlyoutItem
                    {
                        Text = "覆盖配置"
                    };
                    replaceMenuItem.Click += (sender, e) =>
                    {
                        // 弹出二次确认Flyout
                        confirmationReplaceFlyout.ShowAt(listViewItem);
                    };
                    menuFlyout.Items.Add(replaceMenuItem);

                    // 添加分割线
                    MenuFlyoutSeparator separator2 = new MenuFlyoutSeparator();
                    menuFlyout.Items.Add(separator2);

                    MenuFlyoutItem editMenuItem = new MenuFlyoutItem
                    {
                        Text = "编辑配置"
                    };
                    editMenuItem.Click += (sender, e) =>
                    {
                        EditThisConfig(selectedItem);
                    };
                    menuFlyout.Items.Add(editMenuItem);

                    MenuFlyoutItem copyMenuItem = new MenuFlyoutItem
                    {
                        Text = "复制配置"
                    };
                    copyMenuItem.Click += (sender, e) =>
                    {
                        CopyThisConfig(selectedItem);
                    };
                    menuFlyout.Items.Add(copyMenuItem);

                    MenuFlyoutItem deleteMenuItem = new MenuFlyoutItem
                    {
                        Text = "删除配置"
                    };
                    deleteMenuItem.Click += (sender, e) =>
                    {
                        // 弹出二次确认Flyout
                        confirmationDelFlyout.ShowAt(listViewItem);
                    };
                    menuFlyout.Items.Add(deleteMenuItem);

                    Thread.Sleep(10);

                    // 在指定位置显示ContextMenu
                    menuFlyout.ShowAt(listViewItem, e.GetPosition(listViewItem));
                }
            }
        }
        private void OnListViewDoubleTapped(object sender, RoutedEventArgs e)
        {
            if (InProgressing.IsActive == false)
            {
                // 处理左键双击事件的代码
                // 获取右键点击的ListViewItem
                FrameworkElement listViewItem = (sender as FrameworkElement);

                // 获取右键点击的数据对象（NSModel）
                NSModel selectedItem = listViewItem?.DataContext as NSModel;

                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
                if (isAdmin)
                {
                    SeletcThisConfig(selectedItem);
                }
                else
                {
                    NotAdminTips.IsOpen = true;
                }
            }
        }

    }
}
