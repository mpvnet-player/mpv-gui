
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Collections;

using Microsoft.Win32;

namespace mpvgui;

public class MediaTrack
{
    public int ID { get; set; }
    public bool External { get; set; }
    public string Text { get; set; } = "";
    public string Type { get; set; }
}

public static class FileTypes
{
    public static string[] Video { get; set; } = "mkv mp4 avi mov flv mpg webm wmv ts vob 264 265 asf avc avs dav h264 h265 hevc m2t m2ts m2v m4v mpeg mpv mts vpy y4m".Split(' ');
    public static string[] Audio { get; set; } = "mp3 flac m4a mka mp2 ogg opus aac ac3 dts dtshd dtshr dtsma eac3 mpa mpc thd w64 wav".Split(' ');
    public static string[] Image { get; set; } = { "jpg", "bmp", "png", "gif", "webp" };
    public static string[] Subtitle { get; } = { "srt", "ass", "idx", "sub", "sup", "ttxt", "txt", "ssa", "smi", "mks" };

    public static bool IsImage(string extension) => Image.Contains(extension);
    public static bool IsAudio(string extension) => Audio.Contains(extension);

    public static bool IsMedia(string extension) =>
        Video.Contains(extension) || Audio.Contains(extension) || Image.Contains(extension);
}

public class Sys
{
    public static bool IsDarkTheme
    {
        get
        {
            object value = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 1);

            if (value is null)
                value = 1;

            return (int)value == 0;
        }
    }
}

public class StringLogicalComparer : IComparer, IComparer<string>
{
    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
    public static extern int StrCmpLogical(string x, string y);

    int IComparer_Compare(object x, object y) => StrCmpLogical(x.ToString(), y.ToString());
    int IComparer.Compare(object x, object y) => IComparer_Compare(x, y);
    int IComparerOfString_Compare(string x, string y) => StrCmpLogical(x, y);
    int IComparer<string>.Compare(string x, string y) => IComparerOfString_Compare(x, y);
}

public class Input
{
    public static string WM_APPCOMMAND_to_mpv_key(int value)
    {
        switch (value)
        {
            case 5: return "SEARCH";       // BROWSER_SEARCH
            case 6: return "FAVORITES";    // BROWSER_FAVORITES
            case 7: return "HOMEPAGE";     // BROWSER_HOME
            case 15: return "MAIL";         // LAUNCH_MAIL
            case 33: return "PRINT";        // PRINT
            case 11: return "NEXT";         // MEDIA_NEXTTRACK
            case 12: return "PREV";         // MEDIA_PREVIOUSTRACK
            case 13: return "STOP";         // MEDIA_STOP
            case 14: return "PLAYPAUSE";    // MEDIA_PLAY_PAUSE
            case 46: return "PLAY";         // MEDIA_PLAY
            case 47: return "PAUSE";        // MEDIA_PAUSE
            case 48: return "RECORD";       // MEDIA_RECORD
            case 49: return "FORWARD";      // MEDIA_FAST_FORWARD
            case 50: return "REWIND";       // MEDIA_REWIND
            case 51: return "CHANNEL_UP";   // MEDIA_CHANNEL_UP
            case 52: return "CHANNEL_DOWN"; // MEDIA_CHANNEL_DOWN
        }

        return null;
    }
}

public class CommandItem : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public string Path { get; set; } = "";
    public string Command { get; set; } = "";

    public string Display
    {
        get
        {
            if (string.IsNullOrEmpty(Path))
            {
                if (Command.Length > 47)
                    return Command.Substring(0, 47) + "...";
                return Command;
            }
            else
                return Path;
        }
    }

    public CommandItem() { }

    public CommandItem(SerializationInfo info, StreamingContext context) { }

    void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    string _Input = "";

    public string Input
    {
        get => _Input;
        set
        {
            _Input = value;
            NotifyPropertyChanged();
        }
    }

    public string Alias
    {
        get
        {
            if (Input.Contains("SHARP") || Input.Contains("sharp") || Input.Contains("Sharp"))
                return "#";
            return null;
        }
    }

    public static ObservableCollection<CommandItem> GetItems(string content)
    {
        var items = new ObservableCollection<CommandItem>();

        if (!string.IsNullOrEmpty(content))
        {
            foreach (string line in content.Split('\r', '\n'))
            {
                string val = line.Trim();

                if (val.StartsWith("#"))
                    continue;

                if (!val.Contains(" "))
                    continue;

                CommandItem item = new CommandItem();
                item.Input = val.Substring(0, val.IndexOf(" "));

                if (item.Input == "_")
                    item.Input = "";

                val = val.Substring(val.IndexOf(" ") + 1);

                if (val.Contains("#menu:"))
                {
                    item.Path = val.Substring(val.IndexOf("#menu:") + 6).Trim();
                    val = val.Substring(0, val.IndexOf("#menu:"));

                    if (item.Path.Contains(";"))
                        item.Path = item.Path.Substring(item.Path.IndexOf(";") + 1).Trim();
                }

                item.Command = val.Trim();

                if (item.Command == "")
                    continue;

                if (item.Command.ToLower() == "ignore")
                    item.Command = "";

                items.Add(item);
            }
        }
        return items;
    }

    static ObservableCollection<CommandItem> _Items;

    public static ObservableCollection<CommandItem> Items
    {
        get
        {
            if (_Items is null)
                _Items = GetItems(File.ReadAllText(Player.InputConfPath));
            return _Items;
        }
    }
}

public class Folder
{
    public static string Startup { get; } = Path.GetDirectoryName(App.ExecutablePath).AddSep();
    public static string AppData { get; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).AddSep();
}

public class Chapter
{
    public string Title { get; set; }
    public double Time { get; set; }

    string _TimeDisplay;

    public string TimeDisplay
    {
        get
        {
            if (_TimeDisplay == null)
            {
                _TimeDisplay = TimeSpan.FromSeconds(Time).ToString();

                if (_TimeDisplay.ContainsEx("."))
                    _TimeDisplay = _TimeDisplay.Substring(0, _TimeDisplay.LastIndexOf("."));
            }

            return _TimeDisplay;
        }
    }
}

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
