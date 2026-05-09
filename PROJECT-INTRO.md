# Codex Launcher 项目介绍

Codex Launcher 是一个 Windows 小启动器，用来在启动 Codex 桌面端前选择网络模式：

- 原生启动：不注入代理环境变量，按 Codex 默认网络环境启动。
- VPN 启动：启动 Codex 前临时注入 `HTTP_PROXY`、`HTTPS_PROXY`、`ALL_PROXY` 和 `NO_PROXY`。

from [hloolx](https://github.com/hloolx)

它不会修改 Windows 全局代理，也不会写入系统级环境变量。代理只影响由这个启动器启动出来的 Codex 进程。

## 适配其他 Windows 电脑

这个启动器的便携逻辑是：

1. 第一次打开时要求输入本地代理端口，例如 `7897`、`7890` 或 `9000`。
2. 程序会把端口保存到当前用户目录：
   `%APPDATA%\CodexProxySwitcher\settings.json`
3. 之后每次打开都会自动读取这个端口。
4. 启动时自动查询这台电脑上的 Windows 商店版 Codex 安装位置。

因此，同一个 exe 拷到另一台 Windows 电脑后，第一次运行会让那台电脑的用户输入自己的代理端口，后续就不用重复输入。

## 自动查找 Codex 的方式

启动器会按顺序尝试：

1. 通过 PowerShell 的 `Get-AppxPackage -Name OpenAI.Codex` 查询安装目录。
2. 在 `C:\Program Files\WindowsApps` 中查找 `OpenAI.Codex_*_x64__2p2nqsd0c76g0`。
3. 在安装目录下启动 `app\Codex.exe`。

如果 Codex 官方后续更改包名或目录结构，可能需要更新匹配规则。

## 解决的问题

这个项目用于缓解 Windows 桌面版 Codex 因网络没有正确走代理而出现的连接问题，例如：

- 启动后长时间显示 `Reconnecting`。
- 聊天或执行任务时反复断线。
- 代理软件可用，但 Codex 桌面端没有自动使用它。
- 用户不想修改 Windows 全局代理，只想让 Codex 单独走代理。

它通过“进程级环境变量”实现，只影响由启动器启动的 Codex 进程。

## API Key 登录说明

本项目不管理 API key，也不改变 Codex 登录方式。它只负责启动时的网络出口选择。

如果 API key 登录/使用失败是网络连接问题，VPN 启动模式可能有帮助；如果是 key 无效、额度不足、组织权限、模型权限或外部浏览器登录页打不开，则需要分别处理认证或浏览器网络问题。

## 为什么这样做

采用“进程级环境变量”而不是修改 Windows 全局代理，主要是为了降低影响范围：

1. 只影响本启动器启动出来的 Codex。
2. 可以随时通过“原生启动”回到默认网络。
3. 不需要长期管理员权限。
4. 不影响浏览器、Git、包管理器或其他应用。
5. 端口配置保存在用户目录，方便在不同 Windows 电脑上首次配置后长期使用。

## 最佳使用方式

1. 先启动代理软件。
2. 确认本地 HTTP 代理端口。
3. 第一次打开启动器时输入端口号。
4. 日常使用优先点击“VPN 启动”。
5. 网络不需要代理时点击“原生启动”。
6. 任务栏建议固定本启动器，而不是原始 Codex 图标。

## 代码来源检查

当前源码没有复制其他开源项目代码。README 中列出的类似项目只用于对比定位。

项目中保留了一个旧配置目录名 `CodexLauncher`，这是为了迁移早期版本用户的端口设置，不是其他项目痕迹。新版本使用 `CodexProxySwitcher` 作为项目名、命名空间和配置目录。

## 项目文件

- `src\Program.cs`：启动器主程序源码。
- `src\CodexProxySwitcher.csproj`：.NET Windows Forms 项目文件。
- `Codex-Launcher.exe`：当前机器可直接运行的启动器。
- `Start-Codex-VPN.bat`：一键代理启动脚本。
- `Start-Codex-Native.bat`：一键原生启动脚本。
- `Start-Codex.ps1`：bat 背后的 PowerShell 启动逻辑。
- `proxy-url.txt`：旧版脚本使用的代理地址配置。

## 推荐分发方式

推荐直接发仓库里的 `release\Codex-Proxy-Switcher.exe`，或者发 GitHub Releases 中的压缩包。首次运行时让对方输入自己的代理端口即可。

当前推荐发布的是 self-contained 版本，对方通常不需要额外安装 .NET Runtime，但 exe 文件会明显更大。
