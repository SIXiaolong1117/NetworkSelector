// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using System.Net;
using Validation;
using Windows.Services.Maps;
using System.Threading.Tasks;
using System.Net.NetworkInformation;

namespace NetworkSelector
{
    public sealed partial class AddConfigDialog : ContentDialog
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public AddConfigDialog()
        {
            this.InitializeComponent();

            string configInner = localSettings.Values["ConfigIDTemp"] as string;
            if (configInner != null)
            {
                // IPAddr.Text + "," + mask.Text + "," + gateway.Text + "," + DNS1.Text + "," + DNS2.Text + "," + configName.Text + "," + networkInterfaceName.SelectedItem as string;
                string[] configInnerSplit = configInner.Split(',');
                configName.Text = configInnerSplit[5];
                IPAddr.Text = configInnerSplit[0];
                mask.Text = configInnerSplit[1];
                gateway.Text = configInnerSplit[2];
                DNS1.Text = configInnerSplit[3];
                DNS2.Text = configInnerSplit[4];
            }

            if (localSettings.Values["DHCPStatus"] as string == "True")
            {
                IPAddr.IsEnabled = false;
                mask.IsEnabled = false;
                gateway.IsEnabled = false;
                DNS1.IsEnabled = false;
                DNS2.IsEnabled = false;
                configName.IsEnabled = false;
                networkInterfaceName.IsEnabled = true;
            }
            else
            {
                IPAddr.IsEnabled = true;
                mask.IsEnabled = true;
                gateway.IsEnabled = true;
                DNS1.IsEnabled = true;
                DNS2.IsEnabled = true;
                configName.IsEnabled = true;
                networkInterfaceName.IsEnabled = true;
            }
            listNetworkInterface();
        }
        private void InnerChanged()
        {
            localSettings.Values["ConfigIDTemp"] = IPAddr.Text + "," + mask.Text + "," + gateway.Text + "," + DNS1.Text + "," + DNS2.Text + "," + configName.Text + "," + networkInterfaceName.SelectedItem as string;
        }
        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            InnerChanged();
        }

        private void networkInterfaceName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InnerChanged();
        }

        // 列举所有网卡
        private void listNetworkInterface()
        {
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface networkInterface in networkInterfaces)
            {
                //DevCMD.Text += "网卡名称: " + networkInterface.Name;
                networkInterfaceName.Items.Add(networkInterface.Name);
            }
        }
    }
}
