# Codex Launcher 项目介绍

Codex Launcher 是一个 Windows 小启动器，用来在启动 Codex 桌面端前选择网络模式：

- 原生启动：不注入代理环境变量，按 Codex 默认网络环境启动。
- VPN 启动：启动 Codex 前临时注入 `HTTP_PROXY`、`HTTPS_PROXY`、`ALL_PROXY` 和 `NO_PROXY`。

制作人：阿懒同学

它不会修改 Windows 全局代理，也不会写入系统级环境变量。代理只影响由这个启动器启动出来的 Codex 进程。

## 适配其他 Windows 电脑

这个启动器的便携逻辑是：

1. 第一次打开时要求输入本地代理端口，例如 `7897`、`7890` 或 `9000`。
2. 程序会把端口保存到当前用户目录：
   `%APPDATA%\CodexLauncher\settings.json`
3. 之后每次打开都会自动读取这个端口。
4. 启动时自动查询这台电脑上的 Windows 商店版 Codex 安装位置。

因此，同一个 exe 拷到另一台 Windows 电脑后，第一次运行会让那台电脑的用户输入自己的代理端口，后续就不用重复输入。

## 自动查找 Codex 的方式

启动器会按顺序尝试：

1. 通过 PowerShell 的 `Get-AppxPackage -Name OpenAI.Codex` 查询安装目录。
2. 在 `C:\Program Files\WindowsApps` 中查找 `OpenAI.Codex_*_x64__2p2nqsd0c76g0`。
3. 在安装目录下启动 `app\Codex.exe`。

如果 Codex 官方后续更改包名或目录结构，可能需要更新匹配规则。

## 项目文件

- `CodexLauncher\Program.cs`：启动器主程序源码。
- `CodexLauncher\CodexLauncher.csproj`：.NET Windows Forms 项目文件。
- `Codex-Launcher.exe`：当前机器可直接运行的启动器。
- `Start-Codex-VPN.bat`：一键代理启动脚本。
- `Start-Codex-Native.bat`：一键原生启动脚本。
- `Start-Codex.ps1`：bat 背后的 PowerShell 启动逻辑。
- `proxy-url.txt`：旧版脚本使用的代理地址配置。

## 推荐分发方式

推荐把发布后的单文件 exe 发给其他 Windows 用户。首次运行时让对方输入自己的代理端口即可。

如果发布的是 framework-dependent 版本，对方电脑需要安装对应 .NET Desktop Runtime。
如果发布的是 self-contained 版本，对方通常不需要额外安装 .NET Runtime，但 exe 文件会明显更大。
