using System.Text;

public static class StringExtensions
{
    public static string Colorize(this string text, params string[] codes)
    {
        if (codes.Length == 0) return text;
        var builder = new StringBuilder();
        const string RESET = "\x1b[0m";

        foreach (var c in codes)
            builder.Append(c);

        builder.Append(text);
        builder.Append(RESET);
        return builder.ToString();
    }
}

public static class StringColors
{
    public const string Black = "\x1b[30m";
    public const string Red = "\x1b[31m";
    public const string Green = "\x1b[32m";
    public const string Yellow = "\x1b[33m";
    public const string Blue = "\x1b[34m";
    public const string Magenta = "\x1b[35m";
    public const string Cyan = "\x1b[36m";
    public const string White = "\x1b[37m";

    public const string BlackBackground = "\x1b[48;5;0m";
    public const string RedBackground = "\x1b[48;5;1m";
    public const string GreenBackground = "\x1b[48;5;2m";
    public const string YellowBackground = "\x1b[48;5;3m";
    public const string BlueBackground = "\x1b[48;5;4m";
    public const string MagentaBackground = "\x1b[48;5;5m";
    public const string CyanBackground = "\x1b[48;5;6m";
    public const string WhiteBackground = "\x1b[48;5;7m";

    public const string DarkRed = "\x1b[38;5;52m";
    public const string DarkGreen = "\x1b[38;5;22m";
    public const string DarkYellow = "\x1b[38;5;58m";
    public const string DarkBlue = "\x1b[38;5;18m";
    public const string DarkMagenta = "\x1b[38;5;90m";
    public const string DarkCyan = "\x1b[38;5;30m";
    public const string DarkWhite = "\x1b[38;5;250m";

    public const string DarkRedBackground = "\x1b[48;5;52m";
    public const string DarkGreenBackground = "\x1b[48;5;22m";
    public const string DarkYellowBackground = "\x1b[48;5;58m";
    public const string DarkBlueBackground = "\x1b[48;5;18m";
    public const string DarkMagentaBackground = "\x1b[48;5;90m";
    public const string DarkCyanBackground = "\x1b[48;5;30m";
    public const string DarkWhiteBackground = "\x1b[48;5;250m";

    public const string Gray = "\x1b[38;5;244m";
    public const string DarkGray = "\x1b[38;5;236m";

    public const string GrayBackground = "\x1b[48;5;244m";
    public const string DarkGrayBackground = "\x1b[48;5;236m";

    public static string GetForegroundColor(byte r, byte g, byte b)
    {
        int index = 16 + (36 * (r / 51)) + (6 * (g / 51)) + (b / 51);
        return $"\x1b[38;5;{index}m";
    }
    
    public static string GetBackgroundColor(byte r, byte g, byte b)
    {
        int index = 16 + (36 * (r / 51)) + (6 * (g / 51)) + (b / 51);
        return $"\x1b[48;5;{index}m";
    }
}