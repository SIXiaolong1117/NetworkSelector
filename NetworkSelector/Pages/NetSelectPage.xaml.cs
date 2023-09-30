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
        private void SeletcConfig(NSModel model)
        {
            if (model == null)
            {
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
                }));
                subThread.Start();
            }
        }
        private void OnListViewRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            // 获取右键点击的ListViewItem
            FrameworkElement listViewItem = (sender as FrameworkElement);

            // 获取右键点击的数据对象（WoLModel）
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
                    SeletcConfig(selectedItem);
                };
                menuFlyout.Items.Add(selectMenuItem);

                // 添加分割线
                MenuFlyoutSeparator separator = new MenuFlyoutSeparator();
                menuFlyout.Items.Add(separator);

                MenuFlyoutItem editMenuItem = new MenuFlyoutItem
                {
                    Text = "编辑配置"
                };
                editMenuItem.Click += (sender, e) =>
                {
                    //EditThisConfig(selectedItem);
                };
                menuFlyout.Items.Add(editMenuItem);

                MenuFlyoutItem deleteMenuItem = new MenuFlyoutItem
                {
                    Text = "删除配置"
                };
                deleteMenuItem.Click += (sender, e) =>
                {
                    //// 保存选中的数据对象以便确认后执行
                    //selectedWoLModel = selectedItem;
                    //// 弹出二次确认Flyout
                    //confirmationFlyout.ShowAt(listViewItem);
                };
                menuFlyout.Items.Add(deleteMenuItem);

                // 在指定位置显示ContextMenu
                menuFlyout.ShowAt(listViewItem, e.GetPosition(listViewItem));
            }

        }
        }
    }
