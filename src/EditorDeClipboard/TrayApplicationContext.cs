using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace EditorDeClipboard;

internal sealed partial class TrayApplicationContext : ApplicationContext
{
    private const string AppName = "Editor de Clipboard";
    private const int ClipboardRetries = 8;
    private const int RetryDelayMilliseconds = 80;
    private static readonly string LineBreak = Environment.NewLine;

    private readonly ContextMenuStrip menu;
    private readonly NotifyIcon trayIcon;

    public TrayApplicationContext()
    {
        menu = CreateMenu();
        trayIcon = new NotifyIcon
        {
            ContextMenuStrip = menu,
            Icon = CreateTrayIcon(),
            Text = AppName,
            Visible = true,
        };

        trayIcon.MouseClick += (_, args) =>
        {
            if (args.Button == MouseButtons.Left)
            {
                menu.Show(Cursor.Position);
            }
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            trayIcon.Visible = false;
            trayIcon.Icon?.Dispose();
            trayIcon.Dispose();
            menu.Dispose();
        }

        base.Dispose(disposing);
    }

    private ContextMenuStrip CreateMenu()
    {
        var contextMenu = new ContextMenuStrip();

        contextMenu.Items.Add(CreateActionItem(
            "Colocar uma virgula no final de cada linha.",
            ClipboardAction.Comma,
            "Virgula no final de cada linha"));

        contextMenu.Items.Add(CreateActionItem(
            "Colocar aspas simples em cada linha.",
            ClipboardAction.Quote,
            "Aspas simples em cada linha"));

        contextMenu.Items.Add(CreateActionItem(
            "Transformar em lista para SQL",
            ClipboardAction.SqlList,
            "Lista para SQL"));

        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add("Fechar", null, (_, _) => ExitThread());

        return contextMenu;
    }

    private ToolStripMenuItem CreateActionItem(string text, ClipboardAction action, string actionLabel)
    {
        return new ToolStripMenuItem(text, null, (_, _) => RunClipboardAction(action, actionLabel));
    }

    private void RunClipboardAction(ClipboardAction action, string actionLabel)
    {
        try
        {
            var input = ReadClipboardText();
            var items = GetClipboardItems(input);

            if (items.Length == 0)
            {
                ShowNotification("Clipboard vazio.", ToolTipIcon.Info);
                return;
            }

            WriteClipboardText(TransformItems(items, action));
            ShowNotification($"{actionLabel} aplicado ao clipboard.", ToolTipIcon.Info);
        }
        catch (Exception exception) when (exception is ExternalException or InvalidOperationException)
        {
            ShowNotification($"Nao foi possivel alterar o clipboard: {exception.Message}", ToolTipIcon.Error);
        }
    }

    private static string ReadClipboardText()
    {
        return RetryClipboardOperation(() =>
            Clipboard.ContainsText(TextDataFormat.UnicodeText)
                ? Clipboard.GetText(TextDataFormat.UnicodeText)
                : string.Empty);
    }

    private static void WriteClipboardText(string text)
    {
        RetryClipboardOperation(() =>
        {
            Clipboard.SetText(text, TextDataFormat.UnicodeText);
            return true;
        });
    }

    private static T RetryClipboardOperation<T>(Func<T> action)
    {
        for (var attempt = 1; attempt <= ClipboardRetries; attempt += 1)
        {
            try
            {
                return action();
            }
            catch (ExternalException) when (attempt < ClipboardRetries)
            {
                Thread.Sleep(RetryDelayMilliseconds);
            }
        }

        return action();
    }

    private static string[] GetClipboardItems(string text)
    {
        return SeparatorRegex()
            .Split(text)
            .Where(item => item.Length > 0)
            .ToArray();
    }

    private static string TransformItems(string[] items, ClipboardAction action)
    {
        return action switch
        {
            ClipboardAction.Comma => AddCommaToEachItem(items),
            ClipboardAction.Quote => QuoteEachItem(items),
            ClipboardAction.SqlList => TurnIntoSqlList(items),
            _ => throw new InvalidOperationException("Acao invalida."),
        };
    }

    private static string AddCommaToEachItem(string[] items)
    {
        return string.Join(
            LineBreak,
            items.Select((item, index) => index == items.Length - 1 ? item : $"{item},"));
    }

    private static string QuoteEachItem(string[] items)
    {
        return string.Join(LineBreak, items.Select(item => $"'{item}'"));
    }

    private static string TurnIntoSqlList(string[] items)
    {
        var quotedItems = items.Select(item => $"'{item.Replace("'", "''")}'");
        return $"({LineBreak}{string.Join($",{LineBreak}", quotedItems)}{LineBreak})";
    }

    private void ShowNotification(string message, ToolTipIcon icon)
    {
        trayIcon.ShowBalloonTip(1500, AppName, message, icon);
    }

    private static Icon CreateTrayIcon()
    {
        using var bitmap = new Bitmap(16, 16);
        using var graphics = Graphics.FromImage(bitmap);
        using var backgroundBrush = new SolidBrush(Color.FromArgb(24, 31, 42));
        using var paperBrush = new SolidBrush(Color.FromArgb(245, 247, 250));
        using var clipBrush = new SolidBrush(Color.FromArgb(37, 99, 235));
        using var lineBrush = new SolidBrush(Color.FromArgb(20, 184, 166));

        graphics.Clear(Color.Transparent);
        graphics.FillRectangle(backgroundBrush, 3, 2, 10, 12);
        graphics.FillRectangle(paperBrush, 4, 3, 8, 10);
        graphics.FillRectangle(clipBrush, 6, 1, 4, 3);
        graphics.FillRectangle(lineBrush, 5, 5, 6, 1);
        graphics.FillRectangle(lineBrush, 5, 8, 6, 1);
        graphics.FillRectangle(lineBrush, 5, 11, 4, 1);

        var handle = bitmap.GetHicon();

        try
        {
            return (Icon)Icon.FromHandle(handle).Clone();
        }
        finally
        {
            DestroyIcon(handle);
        }
    }

    [GeneratedRegex(@"[,\s]+")]
    private static partial Regex SeparatorRegex();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr handle);

    private enum ClipboardAction
    {
        Comma,
        Quote,
        SqlList,
    }
}
