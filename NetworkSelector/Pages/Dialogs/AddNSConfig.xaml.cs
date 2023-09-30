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
            configName.Text = nsModel.Name;
            ipAddr.Text = nsModel.IPAddr;
            mask.Text = nsModel.Mask;
            gateway.Text = nsModel.Gateway;
            dns1.Text = nsModel.DNS1;
            dns2.Text = nsModel.DNS2;
    }
    private void MyDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
            // 在"确定"按钮点击事件中保存用户输入的内容
            NSData.Name=configName.Text;
            NSData.IPAddr=ipAddr.Text;
            NSData.Mask=mask.Text;
            NSData.Gateway=gateway.Text;
            NSData.DNS1=dns1.Text;
            NSData.DNS2=dns2.Text;
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

    // 列举所有网卡
    private void listNetworkInterface()
    {
    }
}
}
