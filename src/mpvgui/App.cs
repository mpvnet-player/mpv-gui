
using System.Threading.Tasks;

namespace mpvgui;

public static class App
{
    public static List<string> TempFiles { get; } = new List<string>();

    public static string ConfPath { get => Player.ConfigFolder + "mpvgui.conf"; }
    public static string ProcessInstance { get; set; } = "single";
    public static string DarkMode { get; set; } = "always";
    public static string DarkTheme { get; set; } = "dark";
    public static string LightTheme { get; set; } = "light";
    public static string StartSize { get; set; } = "height-session";

    public static bool AutoLoadFolder { get; set; } = true;
    public static bool AutoPlay { get; set; }
    public static bool DebugMode { get; set; }
    public static bool Exit { get; set; }
    public static bool IsTerminalAttached { get; } = Environment.GetEnvironmentVariable("_started_from_console") == "yes";
    public static bool MediaInfo { get; set; } = true;
    public static bool Queue { get; set; }
    public static bool RememberVolume { get; set; } = true;
    public static bool RememberWindowPosition { get; set; }

    public static int StartThreshold { get; set; } = 1500;
    public static int RecentCount { get; set; } = 15;

    public static float AutofitAudio { get; set; } = 0.7f;
    public static float AutofitImage { get; set; } = 0.8f;
    public static float MinimumAspectRatio { get; set; }
    public static float MinimumAspectRatioAudio { get; set; }
    public static float QuickBookmark { get; set; }

    static AppSettings _Settings;

    public static AppSettings Settings {
        get {
            if (_Settings == null)
                _Settings = SettingsManager.Load();

            return _Settings;
        }
    }

    public static void Init()
    {
        var useless1 = Player.ConfigFolder;
        var useless2 = Player.Conf;

        foreach (var i in Conf)
            ProcessProperty(i.Key, i.Value, true);

        if (DebugMode)
        {
            string filePath = Player.ConfigFolder + "mpvgui-debug.log";

            if (File.Exists(filePath))
                File.Delete(filePath);

            Trace.Listeners.Add(new TextWriterTraceListener(filePath));
            Trace.AutoFlush = true;
        }

        Player.Shutdown += Player_Shutdown;
        Player.Initialized += Player_Initialized;
    }

    public static void RunTask(Action action)
    {
        Task.Run(() => {
            try {
                action.Invoke();
            } catch (Exception e) {
                Terminal.WriteError(e);
            }
        });
    }

    public static string About => "Copyright (C) 2000-2022 mpv-gui/mpv/mplayer\n" +
        $"mpv-gui {AppInfo.Version}" + GetLastWriteTime(AppInfo.ProcessPath) + "\n" +
        $"{Player.GetPropertyString("mpv-version")}" + GetLastWriteTime(Folder.Startup + "libmpv-2.dll") + "\n" +
        $"ffmpeg {Player.GetPropertyString("ffmpeg-version")}\n" + "\nGPL v2 License";

    static string GetLastWriteTime(string path)
    {
        if (IsStoreVersion)
            return "";

        return $" ({File.GetLastWriteTime(path).ToShortDateString()})";
    }

    static bool IsStoreVersion => Folder.Startup.Contains("FrankSkare.mpv-gui");

    static void Player_Initialized()
    {
        if (RememberVolume)
        {
            Player.SetPropertyInt("volume", Settings.Volume);
            Player.SetPropertyString("mute", Settings.Mute);
        }
    }

    static void Player_Shutdown()
    {
        Settings.Volume = Player.GetPropertyInt("volume");
        Settings.Mute = Player.GetPropertyString("mute");

        SettingsManager.Save(Settings);

        foreach (string file in TempFiles)
            FileHelp.Delete(file);
    }

    static Dictionary<string, string>? _Conf;

    public static Dictionary<string, string> Conf {
        get {
            if (_Conf == null)
            {
                _Conf = new Dictionary<string, string>();

                if (File.Exists(ConfPath))
                    foreach (string i in File.ReadAllLines(ConfPath))
                        if (i.Contains('=') && !i.StartsWith("#"))
                            _Conf[i[..i.IndexOf("=")].Trim()] = i[(i.IndexOf("=") + 1)..].Trim();
            }

            return _Conf;
        }
    }

    public static bool ProcessProperty(string name, string value, bool writeError = false)
    {
        switch (name)
        {
            case "audio-file-extensions": FileTypes.Audio = value.Split(" ,;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries); return true;
            case "auto-load-folder": AutoLoadFolder = value == "yes"; return true;
            case "auto-play": AutoPlay = value == "yes"; return true;
            case "autofit-audio": AutofitAudio = value.Trim('%').ToInt() / 100f; return true;
            case "autofit-image": AutofitImage = value.Trim('%').ToInt() / 100f; return true;
            case "dark-mode": DarkMode = value; return true;
            case "dark-theme": DarkTheme = value.Trim('\'', '"'); return true;
            case "debug-mode": DebugMode = value == "yes"; return true;
            case "image-file-extensions": FileTypes.Image = value.Split(" ,;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries); return true;
            case "light-theme": LightTheme = value.Trim('\'', '"'); return true;
            case "media-info": MediaInfo = value == "yes"; return true;
            case "minimum-aspect-ratio-audio": MinimumAspectRatioAudio = value.ToFloat(); return true;
            case "minimum-aspect-ratio": MinimumAspectRatio = value.ToFloat(); return true;
            case "process-instance": ProcessInstance = value; return true;
            case "queue": Queue = value == "yes"; return true;
            case "recent-count": RecentCount = value.ToInt(); return true;
            case "remember-volume": RememberVolume = value == "yes"; return true;
            case "remember-window-position": RememberWindowPosition = value == "yes"; return true;
            case "start-size": StartSize = value; return true;
            case "start-threshold": StartThreshold = value.ToInt(); return true;
            case "video-file-extensions": FileTypes.Video = value.Split(" ,;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries); return true;

            default:
                if (writeError)
                    Terminal.WriteError($"unknown mpvgui.conf property: {name}");

                return false;
        }
    }

    public static (string Title, string Path) GetTitleAndPath(string input)
    {
        if (input.Contains('|'))
        {
            var a = input.Split('|');
            return (a[1], a[0]);
        }

        return (input, input);
    }
}
