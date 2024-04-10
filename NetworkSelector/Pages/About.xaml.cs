// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

﻿using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Gaming.Preview.GamesEnumeration;
using System.Net.Http;
using Microsoft.UI.Xaml;

namespace NetworkSelector.Pages
{
    public sealed partial class About : Page
    {
        public About()
        {
            this.InitializeComponent();

            // 在构造函数或其他适当位置设置版本号
            var package = Package.Current;
            var version = package.Id.Version;

            APPVersion.Text = $"{version.Major}.{version.Minor}.{version.Build}";
            //APPVersion.NavigateUri = new System.Uri($"https://github.com/Direct5dom/NetworkSelector/releases/tag/{version.Major}.{version.Minor}.{version.Build}.{version.Revision}");
            //APPVersion.NavigateUri = new System.Uri($"https://www.microsoft.com/store/apps/9PDQC93R0WLF");

            GetList();
        }
        private void AboutAliPay_Click(object sender, RoutedEventArgs e)
        {
            AboutAliPayTips.IsOpen = true;
        }
        private void AboutWePay_Click(object sender, RoutedEventArgs e)
        {
            AboutWePayTips.IsOpen = true;
        }
        private async Task<string> HTTPResponse(string http)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(http);
                if (response.IsSuccessStatusCode)
                {
                    // 从GitHub的响应中读取文件内容
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    return "";
                }
            }
        }
        private async void GetList()
        {
            string nameList = null;
            string stringList = null;
            try
            {
                nameList = await HTTPResponse("https://raw.githubusercontent.com/Direct5dom/Direct5dom/main/README/Sponsor/List");
            }
            catch (Exception ex)
            {
                try
                {
                    nameList = await HTTPResponse("https://gitee.com/XiaolongSI/Direct5dom/raw/main/README/Sponsor/List");
                }
                catch (Exception ex2)
                {
                    nameList = "无法连接至 Github 或 Gitee。";
                }
            }
            try
            {
                stringList = await HTTPResponse("https://raw.githubusercontent.com/Direct5dom/Direct5dom/main/README/Text/List");
            }
            catch (Exception ex)
            {
                try
                {
                    stringList = await HTTPResponse("https://gitee.com/XiaolongSI/Direct5dom/raw/main/README/Text/List");
                }
                catch (Exception ex2)
                {
                    stringList = "";
                }
            }

            string randomLine = null;
            try
            {
                // 使用换行符分割字符串成数组
                string[] lines = stringList.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                // 使用随机数生成器生成一个随机索引
                Random rand = new Random();
                int randomIndex = rand.Next(0, lines.Length);

                // 随机选择一个字符串
                randomLine = lines[randomIndex];
            }
            catch (Exception ex) { }

            NameList.Text = nameList;
            TipsTips.Text = randomLine;
        }
    }
}
