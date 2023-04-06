// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

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
using Windows.Storage;

namespace NetworkSelector
{
    public sealed partial class AddGitConfigDialog : ContentDialog
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public AddGitConfigDialog()
        {
            this.InitializeComponent();
        }
        public void TextChanged(object sender, TextChangedEventArgs e)
        {
            localSettings.Values["GitConfigIDTemp"] = IPAddr.Text + "," + IPPort.Text;
            //Test.Text = localSettings.Values["ConfigIDTemp"] as string;
        }
    }
}
