# Network Selector

个人的一个 C# + WinUI3 的练手项目，可以在多个网络配置预设中快速切换。

因为微软没有在 Windows 中提供保存网络设置为预设的功能，对于需要经常切换网关服务器/代理服务器的用户来说十分的麻烦，于是我就开发了这个小工具。

## 项目预览

<img src="./README/屏幕截图 2023-02-11 015534.png" style="max-width: 30%;" />

## ⬇下载

您可以直接到 [Releases · Direct5dom/NetworkSelector](https://github.com/Direct5dom/NetworkSelector/releases) 下载我已经打包好的安装包。

安装需要注意的是，要右键“使用 PowrShell 运行” `Install.ps1`，而不是直接双击`WSAFileLink.msix`。

## 🛠️获取源码

要构建此项目，您需要将项目源码克隆到本地。

您可以使用 Git 命令行：

```
git clone git@github.com:Direct5dom/NetworkSelector.git
```

或者更方便的，使用 Visual Studio 的“克隆存储库”克隆本仓库。

使用 Visual Studio 打开项目根目录的`NetworkSelector.sln`，即可进行调试和打包。

## ⚖️License

[MIT License](https://github.com/Direct5dom/NetworkSelector/blob/master/LICENSE)