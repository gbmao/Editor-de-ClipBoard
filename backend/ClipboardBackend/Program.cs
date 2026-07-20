using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ClipboardBackend;

internal static partial class Program
{
    private const int ClipboardRetries = 8;
    private const int RetryDelayMilliseconds = 80;

    [STAThread]
    private static int Main(string[] args)
    {
        
        if (args.Length != 1)
        {
            Console.Error.WriteLine("Informe uma acao: comma, quote ou sql-list.");
            return 1;
        }

        try
        {
            var input = ReadClipboardText();
            var output = args[0] switch
            {
                "comma" => TransformEachLine(input, line => $"{line},"),
                "quote" => TransformEachLine(input, line => $"'{line}'"),
                "sql-list" => TransformToSqlList(input),
                _ => throw new ArgumentException("Acao invalida."),
            };

            WriteClipboardText(output);
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }

    private static string ReadClipboardText()
    {
        return RetryClipboardOperation(() =>
        {
            if (!Clipboard.ContainsText(TextDataFormat.UnicodeText))
            {
                return string.Empty;
            }

            return Clipboard.GetText(TextDataFormat.UnicodeText);
        });
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

    private static string TransformEachLine(string text, Func<string, string> transform)
    {
        if (text.Length == 0)
        {
            return text;
        }
        text = DetectSeparatorsAndTurnIntoLineBreaker(text);
        if (text.Length == 0)
        {
            return text;
        }

        var lineBreak = DetectLineBreak(text);
        var lines = LineBreakRegex().Split(text);
        var lineCount = EndsWithLineBreak(text) ? lines.Length - 1 : lines.Length;

        for (var index = 0; index < lineCount; index += 1)
        {
            lines[index] = transform(lines[index]);
        }
        

        return RemoveLastComma(string.Join(lineBreak, lines));
    }

    private static string RemoveLastComma(string text)
    {
        while (text.Length > 0 && text[^1] == ',')
        {
            text = text[..^1];
        }
        return text;
    }

    private static string TransformToSqlList(string text)
    {
        if (text.Length == 0)
        {
            return text;
        }

        text = DetectSeparatorsAndTurnIntoLineBreaker(text);
        if (text.Length == 0)
        {
            return text;
        }

        var lines = LineBreakRegex()
            .Split(text)
            .Where(line => line.Length > 0)
            .Select(line => $"'{line.Replace("'", "''")}'");

        var lineBreak = DetectLineBreak(text);
        return $"({lineBreak}{string.Join($",{lineBreak}", lines)}{lineBreak})";
    }

    private static string DetectLineBreak(string text)
    {
        if (text.Contains("\r\n"))
        {
            return "\r\n";
        }

        if (text.Contains('\n'))
        {
            return "\n";
        }

        if (text.Contains('\r'))
        {
            return "\r";
        }

        return Environment.NewLine;
    }

    private static string DetectSeparatorsAndTurnIntoLineBreaker(string text)
    {
        var lineBreak = DetectLineBreak(text);
        return SeparatorRegex().Replace(text, lineBreak).Trim('\r', '\n');
    }

    private static bool EndsWithLineBreak(string text)
    {
        return text.EndsWith('\n') || text.EndsWith('\r');
    }

    [GeneratedRegex(@"\r\n|\n|\r")]
    private static partial Regex LineBreakRegex();

    [GeneratedRegex(@"[,\s]+")]
    private static partial Regex SeparatorRegex();
}
