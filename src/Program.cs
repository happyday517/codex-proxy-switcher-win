using System.Diagnostics;
using System.Text.Json;

namespace CodexLauncher;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var configStore = LauncherConfigStore.LoadOrCreate();
        if (configStore is null)
        {
            return;
        }

        Application.Run(new LauncherForm(configStore));
    }
}

internal sealed class LauncherForm : Form
{
    private const string NoProxy = "localhost,127.0.0.1,::1";

    private readonly LauncherConfigStore configStore;
    private readonly Label proxyLabel;
    private readonly Label codexPathLabel;

    public LauncherForm(LauncherConfigStore configStore)
    {
        this.configStore = configStore;

        Text = "Codex 启动器";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(540, 286);
        Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

        var title = new Label
        {
            Text = "选择 Codex 启动方式",
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font(Font.FontFamily, 11F, FontStyle.Bold),
            Location = new Point(24, 18),
            Size = new Size(492, 30)
        };

        var note = new Label
        {
            Text = "启动前会关闭已运行的 Codex，避免复用旧进程导致网络模式不生效。",
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(24, 54),
            Size = new Size(492, 24)
        };

        proxyLabel = new Label
        {
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(24, 82),
            Size = new Size(492, 24)
        };

        codexPathLabel = new Label
        {
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(24, 108),
            Size = new Size(492, 36)
        };

        var nativeButton = new Button
        {
            Text = "原生启动",
            Location = new Point(86, 156),
            Size = new Size(150, 44)
        };
        nativeButton.Click += (_, _) => Launch("Native");

        var vpnButton = new Button
        {
            Text = "VPN 启动",
            Location = new Point(304, 156),
            Size = new Size(150, 44)
        };
        vpnButton.Click += (_, _) => Launch("Vpn");

        var changePortButton = new Button
        {
            Text = "修改端口",
            Location = new Point(118, 214),
            Size = new Size(130, 30)
        };
        changePortButton.Click += (_, _) => ChangePort();

        var openConfigButton = new Button
        {
            Text = "打开配置目录",
            Location = new Point(292, 214),
            Size = new Size(130, 30)
        };
        openConfigButton.Click += (_, _) => Process.Start(new ProcessStartInfo
        {
            FileName = configStore.ConfigDirectory,
            UseShellExecute = true
        });

        var producerLabel = new Label
        {
            Text = "制作人：阿懒同学",
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(24, 252),
            Size = new Size(492, 24)
        };

        Controls.Add(title);
        Controls.Add(note);
        Controls.Add(proxyLabel);
        Controls.Add(codexPathLabel);
        Controls.Add(nativeButton);
        Controls.Add(vpnButton);
        Controls.Add(changePortButton);
        Controls.Add(openConfigButton);
        Controls.Add(producerLabel);

        RefreshStatus();
    }

    private void RefreshStatus()
    {
        proxyLabel.Text = "当前代理: " + configStore.Settings.ProxyUrl;

        var codexPath = CodexFinder.TryFindCodexExePath();
        codexPathLabel.Text = string.IsNullOrWhiteSpace(codexPath)
            ? "Codex: 未找到 Windows 商店版 Codex"
            : "Codex: " + CompactPath(codexPath, 74);
    }

    private void ChangePort()
    {
        using var setupForm = new ProxySetupForm(configStore.Settings.ProxyPort);
        if (setupForm.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        configStore.Settings.ProxyPort = setupForm.ProxyPort;
        configStore.Save();
        RefreshStatus();
    }

    private void Launch(string mode)
    {
        try
        {
            var codexExePath = CodexFinder.FindCodexExePath();
            StopExistingCodex();
            StartCodex(codexExePath, mode, configStore.Settings.ProxyUrl);
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "启动失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            RefreshStatus();
        }
    }

    private static void StopExistingCodex()
    {
        foreach (var process in Process.GetProcessesByName("Codex"))
        {
            try
            {
                process.Kill();
                process.WaitForExit(3000);
            }
            catch
            {
                // Best effort. If a process cannot be closed, launching may still work.
            }
        }

        Thread.Sleep(500);
    }

    private static void StartCodex(string codexExePath, string mode, string proxyUrl)
    {
        var processInfo = new ProcessStartInfo(codexExePath)
        {
            WorkingDirectory = Path.GetDirectoryName(codexExePath) ?? AppContext.BaseDirectory,
            UseShellExecute = false
        };

        foreach (var key in new[] { "HTTP_PROXY", "HTTPS_PROXY", "ALL_PROXY", "NO_PROXY" })
        {
            processInfo.Environment.Remove(key);
        }

        if (string.Equals(mode, "Vpn", StringComparison.OrdinalIgnoreCase))
        {
            processInfo.Environment["HTTP_PROXY"] = proxyUrl;
            processInfo.Environment["HTTPS_PROXY"] = proxyUrl;
            processInfo.Environment["ALL_PROXY"] = proxyUrl;
            processInfo.Environment["NO_PROXY"] = NoProxy;
        }

        Process.Start(processInfo);
    }

    private static string CompactPath(string path, int maxLength)
    {
        if (path.Length <= maxLength)
        {
            return path;
        }

        var fileName = Path.GetFileName(path);
        var root = Path.GetPathRoot(path) ?? string.Empty;
        var available = Math.Max(12, maxLength - root.Length - fileName.Length - 6);
        var middle = path[root.Length..^fileName.Length].Trim('\\');
        if (middle.Length > available)
        {
            middle = middle[^available..];
        }

        return root + "...\\" + middle + "\\" + fileName;
    }
}

internal sealed class ProxySetupForm : Form
{
    private readonly TextBox portBox;

    public int ProxyPort { get; private set; }

    public ProxySetupForm(int suggestedPort)
    {
        Text = "配置代理端口";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(420, 190);
        Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

        var title = new Label
        {
            Text = "第一次使用需要填写本地代理端口",
            AutoSize = false,
            Font = new Font(Font.FontFamily, 10F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(24, 18),
            Size = new Size(372, 28)
        };

        var hint = new Label
        {
            Text = "例如 7897、7890 或 9000；启动器会使用 127.0.0.1 加这个端口。",
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(24, 52),
            Size = new Size(372, 38)
        };

        var portLabel = new Label
        {
            Text = "端口号",
            AutoSize = false,
            Location = new Point(90, 105),
            Size = new Size(70, 24),
            TextAlign = ContentAlignment.MiddleLeft
        };

        portBox = new TextBox
        {
            Text = suggestedPort.ToString(),
            Location = new Point(166, 103),
            Size = new Size(160, 24)
        };
        portBox.SelectAll();

        var okButton = new Button
        {
            Text = "保存",
            DialogResult = DialogResult.None,
            Location = new Point(116, 145),
            Size = new Size(90, 30)
        };
        okButton.Click += (_, _) => SavePort();

        var cancelButton = new Button
        {
            Text = "取消",
            DialogResult = DialogResult.Cancel,
            Location = new Point(220, 145),
            Size = new Size(90, 30)
        };

        AcceptButton = okButton;
        CancelButton = cancelButton;

        Controls.Add(title);
        Controls.Add(hint);
        Controls.Add(portLabel);
        Controls.Add(portBox);
        Controls.Add(okButton);
        Controls.Add(cancelButton);
    }

    private void SavePort()
    {
        if (!int.TryParse(portBox.Text.Trim(), out var port) || port < 1 || port > 65535)
        {
            MessageBox.Show(this, "请输入 1 到 65535 之间的端口号。", "端口无效", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            portBox.Focus();
            portBox.SelectAll();
            return;
        }

        ProxyPort = port;
        DialogResult = DialogResult.OK;
        Close();
    }
}

internal sealed class LauncherConfigStore
{
    private const int DefaultProxyPort = 7897;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private LauncherConfigStore(string configDirectory, string settingsPath, LauncherSettings settings)
    {
        ConfigDirectory = configDirectory;
        SettingsPath = settingsPath;
        Settings = settings;
    }

    public string ConfigDirectory { get; }

    public string SettingsPath { get; }

    public LauncherSettings Settings { get; }

    public static LauncherConfigStore? LoadOrCreate()
    {
        var configDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CodexLauncher");
        var settingsPath = Path.Combine(configDirectory, "settings.json");

        Directory.CreateDirectory(configDirectory);

        var settings = Load(settingsPath);
        if (settings is null)
        {
            using var setupForm = new ProxySetupForm(ReadSuggestedPort());
            if (setupForm.ShowDialog() != DialogResult.OK)
            {
                return null;
            }

            settings = new LauncherSettings
            {
                ProxyPort = setupForm.ProxyPort
            };
            Save(settingsPath, settings);
        }

        return new LauncherConfigStore(configDirectory, settingsPath, settings);
    }

    public void Save()
    {
        Save(SettingsPath, Settings);
    }

    private static LauncherSettings? Load(string settingsPath)
    {
        try
        {
            if (!File.Exists(settingsPath))
            {
                return null;
            }

            var settings = JsonSerializer.Deserialize<LauncherSettings>(File.ReadAllText(settingsPath), JsonOptions);
            if (settings is null || settings.ProxyPort < 1 || settings.ProxyPort > 65535)
            {
                return null;
            }

            return settings;
        }
        catch
        {
            return null;
        }
    }

    private static void Save(string settingsPath, LauncherSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(settingsPath)!);
        File.WriteAllText(settingsPath, JsonSerializer.Serialize(settings, JsonOptions));
    }

    private static int ReadSuggestedPort()
    {
        var proxyFile = Path.Combine(AppContext.BaseDirectory, "proxy-url.txt");
        if (!File.Exists(proxyFile))
        {
            return DefaultProxyPort;
        }

        var configured = File.ReadLines(proxyFile)
            .Select(line => line.Trim())
            .FirstOrDefault(line => !string.IsNullOrWhiteSpace(line));

        if (string.IsNullOrWhiteSpace(configured))
        {
            return DefaultProxyPort;
        }

        if (int.TryParse(configured, out var port) && port is >= 1 and <= 65535)
        {
            return port;
        }

        if (Uri.TryCreate(configured, UriKind.Absolute, out var uri) && uri.Port is >= 1 and <= 65535)
        {
            return uri.Port;
        }

        return DefaultProxyPort;
    }
}

internal sealed class LauncherSettings
{
    public string ProxyScheme { get; set; } = "http";

    public string ProxyHost { get; set; } = "127.0.0.1";

    public int ProxyPort { get; set; } = 7897;

    public string ProxyUrl => $"{ProxyScheme}://{ProxyHost}:{ProxyPort}";
}

internal static class CodexFinder
{
    public static string FindCodexExePath()
    {
        var codexPath = TryFindCodexExePath();
        if (!string.IsNullOrWhiteSpace(codexPath))
        {
            return codexPath;
        }

        throw new InvalidOperationException("没有找到 Windows 商店版 Codex。请确认这台电脑已经安装 Codex。");
    }

    public static string? TryFindCodexExePath()
    {
        return FindCodexViaAppxPackage() ?? FindCodexViaWindowsApps();
    }

    private static string? FindCodexViaAppxPackage()
    {
        try
        {
            var processInfo = new ProcessStartInfo("powershell.exe")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            processInfo.ArgumentList.Add("-NoProfile");
            processInfo.ArgumentList.Add("-Command");
            processInfo.ArgumentList.Add("(Get-AppxPackage -Name OpenAI.Codex | Sort-Object Version -Descending | Select-Object -First 1).InstallLocation");

            using var process = Process.Start(processInfo);
            if (process is null)
            {
                return null;
            }

            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit(5000);

            if (string.IsNullOrWhiteSpace(output))
            {
                return null;
            }

            var exePath = Path.Combine(output, "app", "Codex.exe");
            return File.Exists(exePath) ? exePath : null;
        }
        catch
        {
            return null;
        }
    }

    private static string? FindCodexViaWindowsApps()
    {
        try
        {
            var basePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "WindowsApps");

            if (!Directory.Exists(basePath))
            {
                return null;
            }

            var packagePath = Directory
                .GetDirectories(basePath, "OpenAI.Codex_*_x64__2p2nqsd0c76g0")
                .OrderByDescending(Directory.GetLastWriteTimeUtc)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(packagePath))
            {
                return null;
            }

            var exePath = Path.Combine(packagePath, "app", "Codex.exe");
            return File.Exists(exePath) ? exePath : null;
        }
        catch
        {
            return null;
        }
    }
}
