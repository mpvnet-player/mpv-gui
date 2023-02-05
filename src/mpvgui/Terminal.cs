namespace mpvgui;

public static class Terminal
{
    static int Padding { get; } = 60;

    public static void WriteError(object obj, string module = "mpv-gui")
    {
        Write(obj, module, ConsoleColor.DarkRed, false);
    }

    public static void Write(object obj, string module = "mpv-gui")
    {
        Write(obj, module, ConsoleColor.Black, true);
    }

    public static void Write(object obj, string module, ConsoleColor color, bool useDefaultColor)
    {
        string value = obj + "";

        if (value == "")
            return;

        if (!string.IsNullOrEmpty(module))
            module = "[" + module + "] ";

        if (useDefaultColor)
            Console.ResetColor();
        else
            Console.ForegroundColor = color;

        value = module + value;

        if (value.Length < Padding)
            value = value.PadRight(Padding);

        if (color == ConsoleColor.Red || color == ConsoleColor.DarkRed)
            Console.Error.WriteLine(value);
        else
            Console.WriteLine(value);

        Console.ResetColor();
        Trace.WriteLine(obj);
    }
}
