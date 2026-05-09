# Codex Proxy Switcher for Windows

from [hloolx](https://github.com/hloolx)

Codex Proxy Switcher for Windows 是一个轻量级 Windows 启动器，用来在启动 OpenAI Codex 桌面端前选择网络模式：

- 原生启动：不注入代理环境变量，按 Codex 默认网络环境启动。
- VPN 启动：为本次 Codex 进程临时注入 `HTTP_PROXY`、`HTTPS_PROXY`、`ALL_PROXY` 和 `NO_PROXY`。

它不会修改 Windows 全局代理，也不会写入系统级环境变量。代理配置只影响由本启动器启动出来的 Codex 进程。

## 功能

- 首次运行输入本地代理端口，例如 `7897`、`7890`、`9000`。
- 端口保存到 `%APPDATA%\CodexLauncher\settings.json`，以后自动读取。
- 自动查询 Windows 商店版 Codex 的安装路径。
- 支持“原生启动”和“VPN 启动”两种模式。
- 可随时在界面中修改代理端口。
- 保留一键启动用的 `.bat` / `.ps1` 脚本。

## 使用

如果你拿到的是编译好的 exe：

1. 双击 `Codex-Proxy-Switcher.exe`。
2. 第一次输入你的本地代理端口。
3. 选择“原生启动”或“VPN 启动”。

如果你使用脚本版本：

1. 修改 `proxy-url.txt`，例如：

   ```txt
   http://127.0.0.1:7897
   ```

2. 双击 `Start-Codex-VPN.bat`。

## 构建

需要 .NET 9 SDK。

```powershell
.\build.ps1
```

构建产物会输出到：

```txt
dist\Codex-Proxy-Switcher.exe
```

如果需要自包含版本：

```powershell
.\build.ps1 -SelfContained
```

## Codex 查找逻辑

启动器会按顺序尝试：

1. 通过 PowerShell 的 `Get-AppxPackage -Name OpenAI.Codex` 查询安装目录。
2. 在 `C:\Program Files\WindowsApps` 中查找 `OpenAI.Codex_*_x64__2p2nqsd0c76g0`。
3. 在安装目录下启动 `app\Codex.exe`。

如果 Codex 官方后续更改包名或目录结构，需要更新匹配规则。

## 类似项目调研

这个项目和现有项目的区别在于：它只做一件事，让 Windows 版 Codex 桌面端启动时可选择是否注入本地代理环境变量。

- [openai/codex](https://github.com/openai/codex)：OpenAI 官方 Codex 项目，包含 CLI、文档和核心实现。
- [Finesssee/ProxyPilot](https://github.com/Finesssee/ProxyPilot)：面向 AI 编程工具的 Windows-native CLIProxyAPI 分支，范围更大，包含 TUI、系统托盘和多提供商 OAuth。
- [siddhantparadox/codexmanager](https://github.com/siddhantparadox/codexmanager)：Codex 配置和资产管理器，重点是编辑 Codex 本地配置、技能和会话。
- [MisakiMei-hub/fmclient-codex-launchers](https://github.com/MisakiMei-hub/fmclient-codex-launchers)：包含固定端口的 Codex 代理启动修复；本项目进一步提供首次端口配置、持久化和原生/VPN 模式选择。

## 许可证

MIT License
