using NetworkSelector.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage;
using Newtonsoft.Json;
using System.Diagnostics;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;


namespace NetworkSelector.Methods
{
    public class NSMethod
    {
        // 列举所有网卡
        public static List<string> ListNetworkInterfaces()
        {
            List<string> networkInterfaceNames = new List<string>();
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface networkInterface in networkInterfaces)
            {
                networkInterfaceNames.Add(networkInterface.Name);
            }

            return networkInterfaceNames;
        }

        // 获取当前使用的网卡名
        public static string GetCurrentActiveNetworkInterfaceName()
        {
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface nic in networkInterfaces)
            {
                if (nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    IPv4InterfaceStatistics statistics = nic.GetIPv4Statistics();

                    if (statistics.BytesReceived > 0 && statistics.BytesSent > 0)
                    {
                        return nic.Name;
                    }
                }
            }

            // 如果没有找到正在上网的网卡，可以返回一个默认值或抛出异常，具体取决于您的需求。
            return "未找到正在上网的网卡";
        }
        // 是否启用IPv6
        public static bool IsIPv6Enabled()
        {
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface networkInterface in networkInterfaces)
            {
                if (networkInterface.Name == NSMethod.GetCurrentActiveNetworkInterfaceName())
                {
                    IPInterfaceProperties properties = networkInterface.GetIPProperties();

                    foreach (UnicastIPAddressInformation addressInfo in properties.UnicastAddresses)
                    {
                        if (addressInfo.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                        {
                            // 找到IPv6地址
                            return true;
                        }
                    }
                }
            }
            // 未找到IPv6地址
            return false;
        }
        // 导出配置
        public static async Task<string> ExportConfig(NSModel nsModel)
        {
            // 创建一个FileSavePicker
            FileSavePicker savePicker = new FileSavePicker();
            // 获取当前窗口句柄 (HWND) 
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.m_window);
            // 使用窗口句柄 (HWND) 初始化FileSavePicker
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);

            // 为FilePicker设置选项
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            // 用户可以将文件另存为的文件类型下拉列表
            savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".nsconfigx" });
            // 如果用户没有选择文件类型，则默认为
            savePicker.DefaultFileExtension = ".nsconfigx";

            // 默认文件名
            savePicker.SuggestedFileName = nsModel.Name + "_BackUp_" + DateTime.Now.ToString();

            // 打开Picker供用户选择文件
            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                try
                {
                    // 阻止更新文件的远程版本，直到我们完成更改并调用 CompleteUpdatesAsync。
                    CachedFileManager.DeferUpdates(file);
                }
                catch
                {
                    // 当您保存至OneDrive等同步盘目录时，在Windows11上可能引起DeferUpdates错误，备份文件不一定写入正确。
                    return "保存行为完成，但当您保存至OneDrive等同步盘目录时，在Windows11上可能引起DeferUpdates错误，备份文件不一定写入正确。";
                }

                // 将数据序列化为 JSON 格式
                string jsonData = JsonConvert.SerializeObject(nsModel);

                // 写入文件
                await FileIO.WriteTextAsync(file, jsonData);

                // 让Windows知道我们已完成文件更改，以便其他应用程序可以更新文件的远程版本。
                // 完成更新可能需要Windows请求用户输入。
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                if (status == FileUpdateStatus.Complete)
                {
                    // 文件保存成功
                    return "文件保存成功";
                }
                else if (status == FileUpdateStatus.CompleteAndRenamed)
                {
                    // 文件重命名并保存成功
                    return "文件重命名并保存成功";
                }
                else
                {
                    // 文件无法保存！
                    return "无法保存！";
                }
            }
            return "错误！";
        }
        // 导入配置
        public static async Task<NSModel> ImportConfig()
        {
            // 创建一个FileOpenPicker
            var openPicker = new FileOpenPicker();
            // 获取当前窗口句柄 (HWND) 
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.m_window);
            // 使用窗口句柄 (HWND) 初始化FileOpenPicker
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            // 为FilePicker设置选项
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            // 建议打开位置 桌面
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            // 文件类型过滤器
            openPicker.FileTypeFilter.Add(".nsconfigx");

            // 打开选择器供用户选择文件
            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                // 读取 JSON 文件内容
                string jsonData = await FileIO.ReadTextAsync(file);
                // 反序列化JSON数据为NSModel对象
                NSModel importedData = JsonConvert.DeserializeObject<NSModel>(jsonData);
                if (importedData != null)
                {
                    // 成功导入配置数据。 
                    return importedData;
                }
                else
                {
                    // JSON数据无法反序列化为配置数据。 
                    return null;
                }
            }
            else
            {
                // 未选择JSON文件。
                return null;
            }
        }
        // 发送通知
        public static void SendNotification(string title, string content, string additionalContent = null, string buttonText = null, string buttonArgument = null)
        {
            // 创建一个 ToastNotificationContent 对象
            ToastContentBuilder builder = new ToastContentBuilder()
                .AddText(title)
                .AddText(content);

            if (!string.IsNullOrEmpty(additionalContent))
            {
                builder.AddText(additionalContent);
            }

            if (!string.IsNullOrEmpty(buttonText))
            {
                builder.AddButton(new ToastButton()
                    .SetContent(buttonText)
                    .AddArgument("action", buttonArgument));
            }

            // 构建通知
            ToastNotification notification = new ToastNotification(builder.GetToastContent().GetXml());

            // 发送通知
            ToastNotificationManager.CreateToastNotifier().Show(notification);
        }
    }
}
