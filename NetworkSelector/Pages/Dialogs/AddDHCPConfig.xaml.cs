using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using NetworkSelector.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using NetworkSelector.Methods;

namespace NetworkSelector.Pages.Dialogs
{
    public sealed partial class AddDHCPConfig : ContentDialog
    {
        public DHCPInterfaceModel DHCPInterfaceData { get; private set; }
        public AddDHCPConfig(DHCPInterfaceModel dhcpInterfaceModel)
        {
            this.InitializeComponent();

            PrimaryButtonClick += MyDialog_PrimaryButtonClick;
            DHCPInterfaceData = dhcpInterfaceModel;

            foreach (string interfaceName in NSMethod.ListNetworkInterfaces())
            {
                networkInterfaceName.Items.Add(interfaceName);
            }

            // 设置ComboBox的默认选中项为当前使用的网卡
            if (!string.IsNullOrEmpty(NSMethod.GetCurrentActiveNetworkInterfaceName()))
            {
                networkInterfaceName.SelectedItem = NSMethod.GetCurrentActiveNetworkInterfaceName();
            }
        }
        private void MyDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // 在"确定"按钮点击事件中保存用户输入的内容
            if (networkInterfaceName.SelectedItem != null)
            {
                DHCPInterfaceData.Netinterface = networkInterfaceName.SelectedItem.ToString().Trim();
            }
        }
        
    }
}
