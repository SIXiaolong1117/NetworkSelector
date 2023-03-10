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

namespace NetworkSelector
{
    public sealed partial class AddConfigDialog : ContentDialog
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public AddConfigDialog()
        {
            this.InitializeComponent();
        }
        public void TextChanged(object sender, TextChangedEventArgs e)
        {
            localSettings.Values["ConfigIDTemp"] = IPAddr.Text + "," + mask.Text + "," + gateway.Text + "," + DNS1.Text + "," + DNS2.Text;
            //Test.Text = localSettings.Values["ConfigIDTemp"] as string;
        }
    }
}
