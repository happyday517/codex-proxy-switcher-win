using System.Diagnostics;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Text.Json;

namespace CodexProxySwitcher;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            var configStore = LauncherConfigStore.LoadOrCreate();
            if (configStore is null)
            {
                return;
            }

            Application.Run(new LauncherForm(configStore));
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString(), "Codex Proxy Switcher 启动失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

internal sealed class LauncherForm : Form
{
    private const string NoProxy = "localhost,127.0.0.1,::1";

    private readonly LauncherConfigStore configStore;
    private readonly Label proxyLabel;
    private readonly Label codexPathLabel;
    private readonly Label codexStateLabel;

    public LauncherForm(LauncherConfigStore configStore)
    {
        this.configStore = configStore;

        Text = "Codex Proxy Switcher";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = true;
        ClientSize = new Size(720, 440);
        BackColor = Color.FromArgb(17, 20, 34);
        Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        DoubleBuffered = true;
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

        var card = new GlassPanel
        {
            Location = new Point(30, 26),
            Size = new Size(660, 388)
        };

        var logo = new LogoMark
        {
            Location = new Point(34, 32),
            Size = new Size(72, 72)
        };

        var eyebrow = new Label
        {
            Text = "Windows Codex network launcher",
            AutoSize = false,
            ForeColor = Color.FromArgb(168, 219, 255),
            Location = new Point(126, 30),
            Size = new Size(440, 22),
            BackColor = Color.Transparent
        };

        var title = new Label
        {
            Text = "Codex Proxy Switcher",
            AutoSize = false,
            ForeColor = Color.White,
            Font = new Font(Font.FontFamily, 20F, FontStyle.Bold),
            Location = new Point(124, 52),
            Size = new Size(470, 42),
            BackColor = Color.Transparent
        };

        var note = new Label
        {
            Text = "选择原生或 VPN 模式启动 Codex。启动前会关闭已运行的 Codex，避免复用旧进程。",
            AutoSize = false,
            ForeColor = Color.FromArgb(202, 215, 235),
            Location = new Point(126, 94),
            Size = new Size(480, 24),
            BackColor = Color.Transparent
        };

        var statusPanel = new GlassPanel
        {
            Location = new Point(34, 134),
            Size = new Size(592, 82),
            Radius = 18,
            GlassColor = Color.FromArgb(54, 255, 255, 255),
            BorderColor = Color.FromArgb(82, 255, 255, 255)
        };

        var proxyTitleLabel = new Label
        {
            Text = "当前代理",
            AutoSize = false,
            ForeColor = Color.FromArgb(156, 179, 214),
            Location = new Point(20, 16),
            Size = new Size(104, 20),
            BackColor = Color.Transparent
        };

        proxyLabel = new Label
        {
            Text = "",
            AutoSize = false,
            ForeColor = Color.White,
            Font = new Font(Font.FontFamily, 10.5F, FontStyle.Bold),
            Location = new Point(124, 14),
            Size = new Size(420, 24),
            BackColor = Color.Transparent
        };

        codexStateLabel = new Label
        {
            Text = "",
            AutoSize = false,
            ForeColor = Color.FromArgb(128, 240, 192),
            Location = new Point(20, 46),
            Size = new Size(104, 20),
            BackColor = Color.Transparent
        };

        codexPathLabel = new Label
        {
            AutoSize = false,
            ForeColor = Color.FromArgb(218, 228, 244),
            Location = new Point(124, 44),
            Size = new Size(430, 24),
            BackColor = Color.Transparent
        };

        statusPanel.Controls.Add(proxyTitleLabel);
        statusPanel.Controls.Add(proxyLabel);
        statusPanel.Controls.Add(codexStateLabel);
        statusPanel.Controls.Add(codexPathLabel);

        var nativeButton = new ModeButton
        {
            Title = "原生启动",
            Caption = "不注入代理环境变量",
            Text = "原生启动",
            Location = new Point(34, 236),
            Size = new Size(278, 74),
            AccentColor = Color.FromArgb(116, 151, 255),
            AccentColor2 = Color.FromArgb(92, 220, 255)
        };
        nativeButton.Click += (_, _) => Launch("Native");

        var vpnButton = new ModeButton
        {
            Title = "VPN 启动",
            Caption = "为本次 Codex 注入本地代理",
            Text = "VPN 启动",
            Location = new Point(348, 236),
            Size = new Size(278, 74),
            AccentColor = Color.FromArgb(90, 230, 180),
            AccentColor2 = Color.FromArgb(120, 160, 255)
        };
        vpnButton.Click += (_, _) => Launch("Vpn");

        var changePortButton = new GlassButton
        {
            Text = "修改端口",
            Location = new Point(34, 330),
            Size = new Size(132, 36)
        };
        changePortButton.Click += (_, _) => ChangePort();

        var openConfigButton = new GlassButton
        {
            Text = "打开配置目录",
            Location = new Point(180, 330),
            Size = new Size(144, 36)
        };
        openConfigButton.Click += (_, _) => Process.Start(new ProcessStartInfo
        {
            FileName = configStore.ConfigDirectory,
            UseShellExecute = true
        });

        var authorLink = new LinkLabel
        {
            Text = "from hloolx",
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleRight,
            Location = new Point(474, 336),
            Size = new Size(152, 24),
            LinkColor = Color.FromArgb(153, 216, 255),
            ActiveLinkColor = Color.White,
            VisitedLinkColor = Color.FromArgb(153, 216, 255),
            ForeColor = Color.FromArgb(153, 216, 255),
            BackColor = Color.Transparent
        };
        authorLink.Links.Add(5, 5, "https://github.com/hloolx");
        authorLink.LinkClicked += (_, e) =>
        {
            var url = e.Link?.LinkData?.ToString() ?? "https://github.com/hloolx";
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        };

        card.Controls.Add(logo);
        card.Controls.Add(eyebrow);
        card.Controls.Add(title);
        card.Controls.Add(note);
        card.Controls.Add(statusPanel);
        card.Controls.Add(nativeButton);
        card.Controls.Add(vpnButton);
        card.Controls.Add(changePortButton);
        card.Controls.Add(openConfigButton);
        card.Controls.Add(authorLink);
        Controls.Add(card);

        RefreshStatus();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        Activate();
        BringToFront();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using var background = new LinearGradientBrush(
            ClientRectangle,
            Color.FromArgb(12, 16, 30),
            Color.FromArgb(33, 45, 76),
            32F);
        e.Graphics.FillRectangle(background, ClientRectangle);

        using var sheen = new LinearGradientBrush(
            new Rectangle(0, 0, ClientSize.Width, ClientSize.Height),
            Color.FromArgb(42, 78, 194, 255),
            Color.FromArgb(0, 255, 255, 255),
            120F);
        e.Graphics.FillRectangle(sheen, ClientRectangle);

        using var linePen = new Pen(Color.FromArgb(24, 255, 255, 255), 1F);
        for (var y = 34; y < ClientSize.Height; y += 42)
        {
            e.Graphics.DrawLine(linePen, 0, y, ClientSize.Width, y + 18);
        }
    }

    private void RefreshStatus()
    {
        proxyLabel.Text = configStore.Settings.ProxyUrl;

        var codexPath = CodexFinder.TryFindCodexExePath();
        if (string.IsNullOrWhiteSpace(codexPath))
        {
            codexStateLabel.Text = "Codex";
            codexStateLabel.ForeColor = Color.FromArgb(255, 196, 128);
            codexPathLabel.Text = "未找到 Windows 商店版 Codex";
            return;
        }

        codexStateLabel.Text = "已找到";
        codexStateLabel.ForeColor = Color.FromArgb(128, 240, 192);
        codexPathLabel.Text = CompactPath(codexPath, 70);
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

internal sealed class GlassPanel : Panel
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Radius { get; set; } = 26;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color GlassColor { get; set; } = Color.FromArgb(48, 255, 255, 255);

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color BorderColor { get; set; } = Color.FromArgb(96, 255, 255, 255);

    public GlassPanel()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = new Rectangle(0, 0, Width - 1, Height - 1);

        using var path = DrawingHelpers.RoundRect(bounds, Radius);
        using var shadowPath = DrawingHelpers.RoundRect(new Rectangle(4, 8, Width - 9, Height - 13), Radius);
        using var shadowBrush = new SolidBrush(Color.FromArgb(42, 0, 0, 0));
        e.Graphics.FillPath(shadowBrush, shadowPath);

        using var glassBrush = new LinearGradientBrush(
            bounds,
            Color.FromArgb(Math.Min(GlassColor.A + 20, 255), GlassColor),
            GlassColor,
            90F);
        e.Graphics.FillPath(glassBrush, path);

        using var highlightPen = new Pen(BorderColor, 1F);
        e.Graphics.DrawPath(highlightPen, path);

        using var shinePen = new Pen(Color.FromArgb(72, 255, 255, 255), 1F);
        e.Graphics.DrawLine(shinePen, Radius, 1, Width - Radius, 1);
    }
}

internal sealed class LogoMark : Control
{
    public LogoMark()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        DrawingHelpers.DrawCloudLogo(e.Graphics, ClientRectangle, includeBadge: true);
    }
}

internal class GlassButton : Button
{
    private bool hovering;
    private bool pressing;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color AccentColor { get; set; } = Color.FromArgb(132, 196, 255);

    public GlassButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        BackColor = Color.Transparent;
        ForeColor = Color.White;
        Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
        Cursor = Cursors.Hand;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        hovering = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        hovering = false;
        pressing = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs mevent)
    {
        pressing = true;
        Invalidate();
        base.OnMouseDown(mevent);
    }

    protected override void OnMouseUp(MouseEventArgs mevent)
    {
        pressing = false;
        Invalidate();
        base.OnMouseUp(mevent);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        var alpha = pressing ? 74 : hovering ? 66 : 42;

        using var path = DrawingHelpers.RoundRect(rect, 13);
        using var brush = new SolidBrush(Color.FromArgb(alpha, 255, 255, 255));
        using var pen = new Pen(Color.FromArgb(hovering ? 135 : 74, AccentColor), 1F);

        e.Graphics.FillPath(brush, path);
        e.Graphics.DrawPath(pen, path);

        TextRenderer.DrawText(
            e.Graphics,
            Text,
            Font,
            rect,
            ForeColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
}

internal sealed class ModeButton : GlassButton
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Title { get; set; } = "";

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Caption { get; set; } = "";

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color AccentColor2 { get; set; } = Color.FromArgb(125, 255, 220);

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);

        using var path = DrawingHelpers.RoundRect(rect, 20);
        using var baseBrush = new SolidBrush(Color.FromArgb(46, 255, 255, 255));
        e.Graphics.FillPath(baseBrush, path);

        using var accent = new LinearGradientBrush(rect, Color.FromArgb(126, AccentColor), Color.FromArgb(74, AccentColor2), 24F);
        e.Graphics.FillPath(accent, path);

        using var pen = new Pen(Color.FromArgb(112, 255, 255, 255), 1F);
        e.Graphics.DrawPath(pen, path);

        var titleRect = new Rectangle(22, 15, Width - 44, 26);
        var captionRect = new Rectangle(22, 42, Width - 44, 20);
        using var titleFont = new Font(Font.FontFamily, 12F, FontStyle.Bold);

        TextRenderer.DrawText(e.Graphics, Title, titleFont, titleRect, Color.White, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(e.Graphics, Caption, Font, captionRect, Color.FromArgb(230, 243, 255), TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
}

internal static class DrawingHelpers
{
    public static void DrawCloudLogo(Graphics graphics, Rectangle bounds, bool includeBadge)
    {
        var rect = RectangleF.Inflate(bounds, -4, -4);
        using var cloud = CloudPath(rect);
        using var shadow = CloudPath(new RectangleF(rect.X + 1.5F, rect.Y + 3F, rect.Width, rect.Height));
        using var shadowBrush = new SolidBrush(Color.FromArgb(38, 0, 0, 0));
        graphics.FillPath(shadowBrush, shadow);

        using var fill = new LinearGradientBrush(rect, Color.FromArgb(176, 157, 255), Color.FromArgb(50, 65, 246), 92F);
        var blend = new ColorBlend
        {
            Positions = new[] { 0F, 0.52F, 1F },
            Colors = new[]
            {
                Color.FromArgb(178, 160, 255),
                Color.FromArgb(102, 134, 255),
                Color.FromArgb(52, 63, 245)
            }
        };
        fill.InterpolationColors = blend;
        graphics.FillPath(fill, cloud);

        using var shine = new LinearGradientBrush(
            new RectangleF(rect.X, rect.Y, rect.Width, rect.Height * 0.5F),
            Color.FromArgb(72, 255, 255, 255),
            Color.FromArgb(0, 255, 255, 255),
            90F);
        graphics.SetClip(cloud);
        graphics.FillEllipse(shine, rect.X + rect.Width * 0.20F, rect.Y + rect.Height * 0.08F, rect.Width * 0.56F, rect.Height * 0.32F);
        graphics.ResetClip();

        using var symbol = new Pen(Color.White, Math.Max(3.2F, rect.Width * 0.07F))
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        graphics.DrawLine(symbol, rect.X + rect.Width * 0.26F, rect.Y + rect.Height * 0.39F, rect.X + rect.Width * 0.34F, rect.Y + rect.Height * 0.52F);
        graphics.DrawLine(symbol, rect.X + rect.Width * 0.34F, rect.Y + rect.Height * 0.52F, rect.X + rect.Width * 0.26F, rect.Y + rect.Height * 0.65F);
        graphics.DrawLine(symbol, rect.X + rect.Width * 0.48F, rect.Y + rect.Height * 0.57F, rect.X + rect.Width * 0.66F, rect.Y + rect.Height * 0.57F);

        if (!includeBadge)
        {
            return;
        }

        var badgeSize = rect.Width * 0.25F;
        var badge = new RectangleF(rect.Right - badgeSize * 1.02F, rect.Bottom - badgeSize * 0.98F, badgeSize, badgeSize);
        using var badgePath = RoundRect(Rectangle.Round(badge), (int)(badgeSize * 0.36F));
        using var badgeBrush = new LinearGradientBrush(badge, Color.FromArgb(40, 238, 214), Color.FromArgb(72, 142, 255), 35F);
        using var badgeBorder = new Pen(Color.FromArgb(230, 255, 255, 255), Math.Max(1.2F, rect.Width * 0.018F));
        graphics.FillPath(badgeBrush, badgePath);
        graphics.DrawPath(badgeBorder, badgePath);

        using var mini = new Pen(Color.White, Math.Max(1.4F, rect.Width * 0.028F))
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };
        graphics.DrawLine(mini, badge.X + badge.Width * 0.26F, badge.Y + badge.Height * 0.38F, badge.X + badge.Width * 0.72F, badge.Y + badge.Height * 0.38F);
        graphics.DrawLine(mini, badge.X + badge.Width * 0.72F, badge.Y + badge.Height * 0.38F, badge.X + badge.Width * 0.62F, badge.Y + badge.Height * 0.28F);
        graphics.DrawLine(mini, badge.X + badge.Width * 0.72F, badge.Y + badge.Height * 0.38F, badge.X + badge.Width * 0.62F, badge.Y + badge.Height * 0.48F);
        graphics.DrawLine(mini, badge.X + badge.Width * 0.74F, badge.Y + badge.Height * 0.66F, badge.X + badge.Width * 0.28F, badge.Y + badge.Height * 0.66F);
        graphics.DrawLine(mini, badge.X + badge.Width * 0.28F, badge.Y + badge.Height * 0.66F, badge.X + badge.Width * 0.38F, badge.Y + badge.Height * 0.56F);
        graphics.DrawLine(mini, badge.X + badge.Width * 0.28F, badge.Y + badge.Height * 0.66F, badge.X + badge.Width * 0.38F, badge.Y + badge.Height * 0.76F);
    }

    private static GraphicsPath CloudPath(RectangleF rect)
    {
        var path = new GraphicsPath { FillMode = FillMode.Winding };
        path.AddEllipse(rect.X + rect.Width * 0.02F, rect.Y + rect.Height * 0.28F, rect.Width * 0.38F, rect.Height * 0.42F);
        path.AddEllipse(rect.X + rect.Width * 0.16F, rect.Y + rect.Height * 0.06F, rect.Width * 0.42F, rect.Height * 0.48F);
        path.AddEllipse(rect.X + rect.Width * 0.42F, rect.Y + rect.Height * 0.15F, rect.Width * 0.42F, rect.Height * 0.44F);
        path.AddEllipse(rect.X + rect.Width * 0.56F, rect.Y + rect.Height * 0.36F, rect.Width * 0.36F, rect.Height * 0.38F);
        path.AddEllipse(rect.X + rect.Width * 0.14F, rect.Y + rect.Height * 0.52F, rect.Width * 0.34F, rect.Height * 0.34F);
        path.AddEllipse(rect.X + rect.Width * 0.38F, rect.Y + rect.Height * 0.48F, rect.Width * 0.40F, rect.Height * 0.38F);
        path.AddRectangle(new RectangleF(rect.X + rect.Width * 0.20F, rect.Y + rect.Height * 0.34F, rect.Width * 0.54F, rect.Height * 0.42F));
        return path;
    }

    public static GraphicsPath RoundRect(Rectangle bounds, int radius)
    {
        var diameter = radius * 2;
        var path = new GraphicsPath();
        var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));

        path.AddArc(arc, 180, 90);
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();

        return path;
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
        BackColor = Color.FromArgb(20, 24, 40);
        DoubleBuffered = true;
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

        var title = new Label
        {
            Text = "第一次使用需要填写本地代理端口",
            AutoSize = false,
            Font = new Font(Font.FontFamily, 10F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(24, 18),
            Size = new Size(372, 28)
        };

        var hint = new Label
        {
            Text = "例如 7897、7890 或 9000；启动器会使用 127.0.0.1 加这个端口。",
            AutoSize = false,
            ForeColor = Color.FromArgb(205, 218, 238),
            BackColor = Color.Transparent,
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
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Color.FromArgb(205, 218, 238),
            BackColor = Color.Transparent
        };

        portBox = new TextBox
        {
            Text = suggestedPort.ToString(),
            Location = new Point(166, 103),
            Size = new Size(160, 24),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(244, 249, 255),
            ForeColor = Color.FromArgb(20, 24, 40)
        };
        portBox.SelectAll();

        var okButton = new GlassButton
        {
            Text = "保存",
            DialogResult = DialogResult.None,
            Location = new Point(116, 145),
            Size = new Size(90, 30),
            AccentColor = Color.FromArgb(115, 238, 189)
        };
        okButton.Click += (_, _) => SavePort();

        var cancelButton = new GlassButton
        {
            Text = "取消",
            DialogResult = DialogResult.Cancel,
            Location = new Point(220, 145),
            Size = new Size(90, 30),
            AccentColor = Color.FromArgb(160, 188, 255)
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

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using var brush = new LinearGradientBrush(ClientRectangle, Color.FromArgb(22, 28, 48), Color.FromArgb(42, 57, 92), 35F);
        e.Graphics.FillRectangle(brush, ClientRectangle);

        using var border = new Pen(Color.FromArgb(48, 255, 255, 255), 1F);
        e.Graphics.DrawRectangle(border, 0, 0, Width - 1, Height - 1);
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
            "CodexProxySwitcher");
        var settingsPath = Path.Combine(configDirectory, "settings.json");

        Directory.CreateDirectory(configDirectory);
        MigrateLegacySettings(settingsPath);

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

    private static void MigrateLegacySettings(string settingsPath)
    {
        if (File.Exists(settingsPath))
        {
            return;
        }

        var legacyPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            // Migrate settings from versions released before the project was renamed.
            "CodexLauncher",
            "settings.json");

        if (!File.Exists(legacyPath))
        {
            return;
        }

        try
        {
            File.Copy(legacyPath, settingsPath, overwrite: false);
        }
        catch
        {
            // If migration fails, the first-run setup dialog will ask for the port again.
        }
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
