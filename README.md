# ProTOTP UWP 应用

这是一个基于UWP（通用Windows平台）的TOTP（基于时间的一次性密码）身份验证器应用，用于管理和展示TOTP验证码。

## 项目结构

```
ProTOTP/
├── ProTOTP.sln                           # Visual Studio 解决方案文件
├── ProTOTP/                             # 主项目文件夹
│   ├── ProTOTP.csproj                   # 项目配置文件
│   ├── App.xaml / App.xaml.cs           # 应用程序入口点
│   ├── MainPage.xaml / MainPage.xaml.cs # 主页面UI和逻辑
│   ├── Package.appxmanifest             # 应用包配置
│   ├── Assets/                          # 应用资源图片
│   ├── Properties/                      # 项目属性文件
│   ├── Models/                          # 数据模型
│   │   └── TOTPAccount.cs               # TOTP账户模型
│   ├── Services/                        # 业务逻辑服务
│   │   ├── TOTPService.cs               # TOTP核心服务
│   │   └── TimerManager.cs              # 计时器管理器
│   ├── ViewModels/                      # 视图模型
│   │   └── MainPageViewModel.cs         # 主页面视图模型
│   └── Views/                           # 页面视图
│       └── AddAccountPage.xaml/.cs      # 添加账户页面
```

## 功能特性

1. **TOTP 验证码生成**：支持标准的TOTP算法（RFC 6238），兼容Google Authenticator等主流验证器
2. **多账户管理**：可以同时管理多个TOTP账户
3. **实时验证码**：自动计算并显示当前的6位验证码，每30秒更新一次
4. **进度指示**：显示每个验证码的时间进度条
5. **持久化存储**：将账户信息加密保存到本地
6. **多种哈希算法**：支持SHA-1、SHA-256、SHA-512等哈希算法
7. **自定义时间步长**：支持非标准时间步长（默认30秒）

## 技术架构

- **平台**：UWP (Universal Windows Platform)
- **语言**：C#
- **框架**：.NET Core, Windows 10 SDK
- **UI框架**：XAML + Windows UI Library
- **数据序列化**：JSON.NET

- 编译要求

- Visual Studio 2019 或更高版本
- Windows 10 SDK (10.0.17763.0 或更高)
- .NET Core UWP 支持
- 目标架构: x64 (仅)

## 使用方法

1. 启动应用后点击"添加TOTP账户"按钮
2. 输入账户名称、密钥和其他可选参数
3. 保存后即可看到实时生成的TOTP验证码
4. 可随时添加或删除账户

## 安全说明

本应用的所有账户数据仅存储在本地设备上，不会上传至任何服务器，确保用户数据的安全性。