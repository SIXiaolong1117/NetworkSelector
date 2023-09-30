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
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using NetworkSelector.Models;
using System.Xml.Linq;
using NetworkSelector.Methods;

namespace NetworkSelector.Pages.Dialogs
{
    public sealed partial class AddNSConfig : ContentDialog
    {
        public NSModel NSData { get; private set; }
        public AddNSConfig(NSModel nsModel)
        {
            this.InitializeComponent();
            PrimaryButtonClick += MyDialog_PrimaryButtonClick;
            SecondaryButtonClick += MyDialog_SecondaryButtonClick;

            // 初始化Dialog中的字段，使用传入的NSModel对象的属性
            NSData = nsModel;
            configName.Text = NSData.Name;
            ipAddr.Text = NSData.IPAddr;
            mask.Text = NSData.Mask;
            gateway.Text = NSData.Gateway;
            dns1.Text = NSData.DNS1;
            dns2.Text = NSData.DNS2;

            foreach (string interfaceName in NSMethod.ListNetworkInterfaces())
            {
                networkInterfaceName.Items.Add(interfaceName);
            }

            // 设置ComboBox的默认选中项为当前使用的网卡
            if (NSData.Netinterface == null && !string.IsNullOrEmpty(NSMethod.GetCurrentActiveNetworkInterfaceName()))
            {
                networkInterfaceName.SelectedItem = NSMethod.GetCurrentActiveNetworkInterfaceName();
            }
            else
            {
                networkInterfaceName.SelectedItem = NSData.Netinterface;
            }
        }
        private void MyDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // 在"确定"按钮点击事件中保存用户输入的内容
            NSData.Name = configName.Text;
            if (networkInterfaceName.SelectedItem != null)
            {
                NSData.Netinterface = networkInterfaceName.SelectedItem.ToString().Trim();
            }
            NSData.IPAddr = ipAddr.Text;
            NSData.Mask = mask.Text;
            NSData.Gateway = gateway.Text;
            NSData.DNS1 = dns1.Text;
            NSData.DNS2 = dns2.Text;
        }

        private void MyDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // 在"取消"按钮点击事件中不做任何操作
        }
        private void InnerChanged()
        {
        }
        private void TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void networkInterfaceName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }
}
