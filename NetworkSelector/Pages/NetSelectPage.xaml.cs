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
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;
using Windows.Services.Maps;
using System.Net.Mail;

namespace NetworkSelector.Pages
{
    public sealed partial class NetSelectPage : Page
    {
        int ipv6Flag = 1;
        ResourceLoader resourceLoader = new ResourceLoader();
        private DispatcherQueue _dispatcherQueue;
        public NetSelectPage()
        {
            this.InitializeComponent();
            // 获取UI线程的DispatcherQueue
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            // 加载数据
            LoadData();
            DisplayNetworkInfo();
            LoadString();
        }
        private void LoadString()
        {
            // 在子线程中执行任务
            Thread subThread = new Thread(new ThreadStart(() =>
            {
                _dispatcherQueue.TryEnqueue(() =>
                {
                    NeedSelectedTips.CloseButtonContent = resourceLoader.GetString("Confirm");
                    NetworkIsChangeTips.CloseButtonContent = resourceLoader.GetString("Confirm");
                    SaveConfigTips.CloseButtonContent = resourceLoader.GetString("Confirm");
                    ConfirmDelete.Content = resourceLoader.GetString("Confirm");
                    ConfirmReplace.Content = resourceLoader.GetString("Confirm");


                    NotAdminTips.CloseButtonContent = resourceLoader.GetString("Cancel");
                    IsAdminButTips.CloseButtonContent = resourceLoader.GetString("Cancel");
                    CancelDelete.Content = resourceLoader.GetString("Cancel");
                    CancelReplace.Content = resourceLoader.GetString("Cancel");
                });
            }));
            subThread.Start();
        }
        private void LoadData()
        {
            // 在子线程中执行任务
            Thread subThread = new Thread(new ThreadStart(() =>
            {
                // 实例化SQLiteHelper
                SQLiteHelper dbHelper = new SQLiteHelper();
                // 查询数据
                List<NSModel> dataList = dbHelper.QueryData();
                _dispatcherQueue.TryEnqueue(() =>
                {
                    // 将数据列表绑定到ListView
                    dataListView.ItemsSource = dataList;
                });
            }));
            subThread.Start();
        }
        // 导入配置按钮点击
        private async void ImportConfig_Click(object sender, RoutedEventArgs e)
        {
            // WinUI 应用无法在提升状态下打开 FilePicker
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            if (isAdmin)
            {
                IsAdminButTips.IsOpen = true;
            }
            else
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
        }
        private async void ExportConfigFunction()
        {
            // WinUI 应用无法在提升状态下打开 FilePicker
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            if (isAdmin)
            {
                IsAdminButTips.IsOpen = true;
            }
            else
            {
                // 获取NSModel对象
                NSModel selectedModel = (NSModel)dataListView.SelectedItem;
                string result = await NSMethod.ExportConfig(selectedModel);
            }
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
            dialog.PrimaryButtonText = resourceLoader.GetString("DialogAdd");
            dialog.CloseButtonText = resourceLoader.GetString("DialogClose");
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
            dialog.PrimaryButtonText = resourceLoader.GetString("Confirm");
            dialog.CloseButtonText = resourceLoader.GetString("DialogClose");
            // 默认按钮为PrimaryButton
            dialog.DefaultButton = ContentDialogButton.Primary;

            // 显示Dialog并等待其关闭
            ContentDialogResult result = await dialog.ShowAsync();

            // 如果按下了Primary
            if (result == ContentDialogResult.Primary)
            {
                SeletcDHCPConfig(dhcpInterfaceModel.Netinterface);
            }
        }
        private void DisableIPv6Button_Click(object sender, RoutedEventArgs e)
        {
            InProgressing.IsActive = true;
            string cmd;
            // 如果IPv6启用
            if (ipv6Flag == 1)
            {
                cmd = $"Disable-NetAdapterBinding -Name {NSMethod.GetCurrentActiveNetworkInterfaceName()} -ComponentID 'ms_tcpip6'";
            }
            // 如果IPv6未启用
            else
            {
                cmd = $"Enable-NetAdapterBinding -Name {NSMethod.GetCurrentActiveNetworkInterfaceName()} -ComponentID 'ms_tcpip6'";
            }
            // 在子线程中执行任务
            Thread subThread = new Thread(new ThreadStart(() =>
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "PowerShell.exe";
                // 是否使用操作系统shell启动
                startInfo.UseShellExecute = true;
                // 不显示程序窗口
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                // 使用管理员权限启动
                startInfo.Verb = "runas";
                startInfo.Arguments = cmd;
                try
                {
                    Process process = Process.Start(startInfo);
                    // 等待进程执行完毕
                    process.WaitForExit();
                    process.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

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
            string cmd = $"netsh interface ip set address name='{model.Netinterface}' source=static addr='{model.IPAddr}' mask='{model.Mask}' gateway='{model.Gateway}'; netsh interface ip set dns name='{model.Netinterface}' source=static addr='{model.DNS1}' register=primary; netsh interface ip add dns name='{model.Netinterface}' addr='{model.DNS2}' index=2;";
            // 在子线程中执行任务
            Thread subThread = new Thread(new ThreadStart(() =>
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = "PowerShell.exe";
                    // 是否使用操作系统shell启动
                    startInfo.UseShellExecute = true;
                    // 不显示程序窗口
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    // 使用管理员权限启动
                    startInfo.Verb = "runas";
                    startInfo.Arguments = cmd;
                    try
                    {
                        Process process = Process.Start(startInfo);
                        // 等待进程执行完毕
                        process.WaitForExit();
                        process.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

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
            string cmd = $"netsh interface ip set address '{netInterface}' dhcp; netsh interface ip set dns name='{netInterface}' source=dhcp;";
            // 在子线程中执行任务
            Thread subThread = new Thread(new ThreadStart(() =>
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "PowerShell.exe";
                // 是否使用操作系统shell启动
                startInfo.UseShellExecute = true;
                // 不显示程序窗口
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                // 使用管理员权限启动
                startInfo.Verb = "runas";
                startInfo.Arguments = cmd;
                try
                {
                    Process process = Process.Start(startInfo);
                    // 等待进程执行完毕
                    process.WaitForExit();
                    process.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

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
            dialog.PrimaryButtonText = resourceLoader.GetString("DialogChange");
            dialog.CloseButtonText = resourceLoader.GetString("DialogClose");
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
                Process process = Process.Start(startInfo);
                // 等待进程执行完毕
                process.WaitForExit();
                process.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Environment.Exit(0);
        }
        //private static void RestartAsNotAdmin()
        //{
        //}
        private void NotAdminTips_ActionButtonClick(TeachingTip sender, object args)
        {
            RestartAsAdmin();
        }
        private void IsAdminButTips_ActionButtonClick(TeachingTip sender, object args)
        {
            //RestartAsNotAdmin();
        }
        private void DisplayNetworkInfo()
        {
            InProgressing.IsActive = true;
            // 占位符
            DisableIPv6.Content = resourceLoader.GetString("DisableIPv6");
            // 在子线程中执行任务
            Thread subThread = new Thread(new ThreadStart(() =>
            {
                // 获取所有的网络接口
                NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (var networkInterface in networkInterfaces)
                {
                    // 仅处理与目标网络接口名称匹配的网络接口
                    if (NSMethod.GetCurrentActiveNetworkInterfaceName() != null && networkInterface.Name == NSMethod.GetCurrentActiveNetworkInterfaceName())
                    {
                        // 获取网络接口的IP属性
                        IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();

                        // 获取网络接口的名称
                        string interfaceName = networkInterface.Name;

                        // 获取网络接口的描述
                        string interfaceDescription = networkInterface.Description;

                        // 获取网络接口的MAC地址
                        string macAddress = networkInterface.GetPhysicalAddress().ToString();

                        // 检查是否是12个字符的MAC地址
                        if (macAddress.Length == 12)
                        {
                            macAddress = string.Join(":", Enumerable.Range(0, 6).Select(i => macAddress.Substring(i * 2, 2)));
                        }

                        // 获取子网前缀长度
                        string subnetMask = "";
                        string subnet6Mask = "";
                        foreach (UnicastIPAddressInformation ipInfo in networkInterface.GetIPProperties().UnicastAddresses)
                        {
                            // v4
                            if (ipInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                subnetMask = ipInfo.PrefixLength.ToString();
                            }
                            // v6
                            if (ipInfo.Address.AddressFamily == AddressFamily.InterNetworkV6)
                            {
                                subnet6Mask = ipInfo.PrefixLength.ToString();
                            }
                        }
                        // 获取IP地址
                        // v4
                        string ipAddressSrc = ipProperties.UnicastAddresses.FirstOrDefault(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork)?.Address.ToString();
                        string ipAddress = ipAddressSrc + "/" + subnetMask;
                        if (ipAddressSrc == null)
                        {
                            ipAddress = resourceLoader.GetString("IPv4Unconnect");
                        }
                        // v6
                        string ipv6AddressSrc = ipProperties.UnicastAddresses.FirstOrDefault(ip => ip.Address.AddressFamily == AddressFamily.InterNetworkV6)?.Address.ToString();
                        string ipv6Address = ipv6AddressSrc + "/" + subnet6Mask;
                        if (ipv6AddressSrc == null)
                        {
                            ipv6Address = resourceLoader.GetString("IPv6Unconnect");
                            ipv6Flag = 0;
                        }

                        // 获取网关地址
                        // v4
                        string gatewayAddressSrc = ipProperties.GatewayAddresses.FirstOrDefault(ga => ga.Address.AddressFamily == AddressFamily.InterNetwork)?.Address.ToString();
                        string gatewayAddress = gatewayAddressSrc;
                        if (gatewayAddressSrc == null)
                        {
                            gatewayAddress = resourceLoader.GetString("IPv4Unconnect");
                        }
                        // v6
                        string gateway6AddressSrc = ipProperties.GatewayAddresses.FirstOrDefault(ga => ga.Address.AddressFamily == AddressFamily.InterNetworkV6)?.Address.ToString();
                        string gateway6Address = gateway6AddressSrc;
                        if (gateway6AddressSrc == null)
                        {
                            gateway6Address = resourceLoader.GetString("IPv6Unconnect");
                        }

                        // 获取DNS服务器信息
                        string dns = "";
                        foreach (IPAddress dnsAddress in ipProperties.DnsAddresses)
                        {
                            dns += dnsAddress.ToString() + "\n";
                        }

                        // 获取网络接口的类型（以太网、Wi-Fi等）
                        // https://learn.microsoft.com/zh-cn/dotnet/api/system.net.networkinformation.networkinterfacetype?view=net-6.0
                        // https://github.com/tpn/winsdk-10/blob/master/Include/10.0.14393.0/shared/ipifcons.h
                        string interfaceTypeSrc = networkInterface.NetworkInterfaceType.ToString();
                        string interfaceType = interfaceTypeSrc;
                        // 虽然枚举中定义了好几种以太网类型，但实际上一般只返回 Ethernet 类型。
                        // https://learn.microsoft.com/zh-cn/dotnet/api/system.net.networkinformation.networkinterface.networkinterfacetype?view=net-6.0
                        if (interfaceTypeSrc == "Ethernet")
                        {
                            interfaceType = resourceLoader.GetString("TypeEthernet");
                        }
                        else if (interfaceTypeSrc == "Wireless80211")
                        {
                            interfaceType = resourceLoader.GetString("TypeWireless80211");
                        }
                        else if (interfaceTypeSrc == "Loopback")
                        {
                            interfaceType = resourceLoader.GetString("TypeLoopback");
                        }
                        else if (interfaceTypeSrc == "Tunnel")
                        {
                            interfaceType = resourceLoader.GetString("TypeTunnel");
                        }
                        else if (interfaceTypeSrc == "Unknown")
                        {
                            interfaceType = resourceLoader.GetString("TypeUnknown");
                        }
                        // https://github.com/tpn/winsdk-10/blob/9b69fd26ac0c7d0b83d378dba01080e93349c2ed/Include/10.0.14393.0/shared/ipifcons.h#L96C33-L96C33
                        else if (interfaceTypeSrc == "53")
                        {
                            interfaceType = resourceLoader.GetString("Type53");
                        }
                        else
                        {
                            interfaceType = interfaceTypeSrc;
                        }

                        // 获取网络接口的速度（以比特每秒为单位）
                        long interfaceSpeed = networkInterface.Speed;

                        // 要在UI线程上更新UI，使用DispatcherQueue
                        _dispatcherQueue.TryEnqueue(() =>
                        {
                            // 将信息输出到UI
                            List<InterfaceInfoModel> interfaceInfoList = new List<InterfaceInfoModel>();
                            interfaceInfoList.Add(new InterfaceInfoModel
                            {
                                Name = $"{interfaceName}",
                                Description = $"{interfaceDescription}",
                                MACAddress = $"{macAddress}",
                                IPAddress = $"{ipAddress}\n{ipv6Address}",
                                GatewayAddress = $"{gatewayAddress}\n{gateway6Address}",
                                DNS = $"{dns.TrimEnd()}",
                                Type = $"{interfaceType}",
                                Speed = $"{interfaceSpeed / 1000000} Mbps"
                            });
                            dataListView2.ItemsSource = interfaceInfoList;

                            if (ipv6Flag == 1)
                            {
                                DisableIPv6.Content = resourceLoader.GetString("DisableIPv6");
                            }
                            else
                            {
                                DisableIPv6.Content = resourceLoader.GetString("EnableIPv6");
                            }
                            InProgressing.IsActive = false;
                        });
                    }
                    // 无网络
                    else
                    {
                        _dispatcherQueue.TryEnqueue(() =>
                        {
                            InProgressing.IsActive = false;
                        });
                    }
                }
            }));
            subThread.Start();
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
                        Text = resourceLoader.GetString("selectMenuItemText")
                    };
                    selectMenuItem.Click += (sender, e) =>
                    {
                        SeletcThisConfig(selectedItem);
                    };
                    menuFlyout.Items.Add(selectMenuItem);

                    // 添加分割线
                    MenuFlyoutSeparator separator = new MenuFlyoutSeparator();
                    menuFlyout.Items.Add(separator);

                    MenuFlyoutItem exportMenuItem = new MenuFlyoutItem
                    {
                        Text = resourceLoader.GetString("exportMenuItemText")
                    };
                    exportMenuItem.Click += (sender, e) =>
                    {
                        ExportConfigFunction();
                    };
                    menuFlyout.Items.Add(exportMenuItem);

                    MenuFlyoutItem replaceMenuItem = new MenuFlyoutItem
                    {
                        Text = resourceLoader.GetString("replaceMenuItemText")
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
                        Text = resourceLoader.GetString("editMenuItemText")
                    };
                    editMenuItem.Click += (sender, e) =>
                    {
                        EditThisConfig(selectedItem);
                    };
                    menuFlyout.Items.Add(editMenuItem);

                    MenuFlyoutItem copyMenuItem = new MenuFlyoutItem
                    {
                        Text = resourceLoader.GetString("copyMenuItemText")
                    };
                    copyMenuItem.Click += (sender, e) =>
                    {
                        CopyThisConfig(selectedItem);
                    };
                    menuFlyout.Items.Add(copyMenuItem);

                    MenuFlyoutItem deleteMenuItem = new MenuFlyoutItem
                    {
                        Text = resourceLoader.GetString("deleteMenuItemText")
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

                SeletcThisConfig(selectedItem);
            }
        }
        private void OnScrollViewerRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (InProgressing.IsActive == false)
            {
                // 获取右键点击的位置
                var point = e.GetPosition(sender as UIElement);

                // 创建ContextMenu
                MenuFlyout menuFlyout = new MenuFlyout();

                MenuFlyoutItem refreshItem = new MenuFlyoutItem
                {
                    Text = resourceLoader.GetString("refreshItemText")
                };
                refreshItem.Click += (sender, e) =>
                {
                    DisplayNetworkInfo();
                };
                menuFlyout.Items.Add(refreshItem);

                Thread.Sleep(10);

                // 在指定位置显示ContextMenu
                menuFlyout.ShowAt(sender as UIElement, point);
            }
        }
    }
}
