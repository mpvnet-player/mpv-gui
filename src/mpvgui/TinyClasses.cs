
using System.Runtime.InteropServices;
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

public static class OS
{
    public static bool IsDarkTheme
    {
        get
        {
            string key = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            object value = Registry.GetValue(key, "AppsUseLightTheme", 1) ?? 1;
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
    public static string WM_APPCOMMAND_to_mpv_key(int value) => value switch
    {
        5 => "SEARCH",         // BROWSER_SEARCH
        6 => "FAVORITES",      // BROWSER_FAVORITES
        7 => "HOMEPAGE",       // BROWSER_HOME
        15 => "MAIL",          // LAUNCH_MAIL
        33 => "PRINT",         // PRINT
        11 => "NEXT",          // MEDIA_NEXTTRACK
        12 => "PREV",          // MEDIA_PREVIOUSTRACK
        13 => "STOP",          // MEDIA_STOP
        14 => "PLAYPAUSE",     // MEDIA_PLAY_PAUSE
        46 => "PLAY",          // MEDIA_PLAY
        47 => "PAUSE",         // MEDIA_PAUSE
        48 => "RECORD",        // MEDIA_RECORD
        49 => "FORWARD",       // MEDIA_FAST_FORWARD
        50 => "REWIND",        // MEDIA_REWIND
        51 => "CHANNEL_UP",    // MEDIA_CHANNEL_UP
        52 => "CHANNEL_DOWN",  // MEDIA_CHANNEL_DOWN
        _ => null,
    };
}

public class Folder
{
    public static string Startup { get; } = Path.GetDirectoryName(AppInfo.ProcessPath).AddSep();
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
