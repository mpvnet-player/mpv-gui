
using mpvgui.WinForms;
using System.Globalization;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace mpvgui;

public class Command
{
    public static Dictionary<string, Action<string[]>> Commands { get; } = new()
    {
        ["load-audio"] = LoadAudio,
        ["load-sub"] = LoadSubtitle,
        ["move-window"] = (args) => Player.RaiseMoveWindow(args[0]),
        ["open-clipboard"] = OpenFromClipboard,
        ["open-conf-folder"] = (args) => ProcessHelp.ShellExecute(Player.ConfigFolder),
        ["open-files"] = OpenFiles,
        ["open-optical-media"] = Open_DVD_Or_BD_Folder,
        ["playlist-add"] = PlaylistAdd,
        ["play-pause"] = PlayPause,
        ["reg-file-assoc"] = RegisterFileAssociations,
        ["scale-window"] = (args) => Player.RaiseScaleWindow(float.Parse(args[0], CultureInfo.InvariantCulture)),
        ["shell-execute"] = (args) => ProcessHelp.ShellExecute(args[0]),
        ["show-about"] = (args) => ShowDialog(typeof(AboutWindow)),
        ["show-audio-devices"] = (args) => Msg.ShowInfo(Player.GetPropertyOsdString("audio-device-list")),
        ["show-conf-editor"] = (args) => ShowDialog(typeof(ConfWindow)),
        ["show-info"] = ShowInfo,
        ["show-input-editor"] = (args) => ShowDialog(typeof(InputWindow)),
        ["show-menu"] = (args) => Player.RaiseShowMenu(),
        ["show-profiles"] = (args) => Msg.ShowInfo(mpvHelp.GetProfiles()),
        ["show-progress"] = ShowProgress,
        ["show-text"] = (args) => ShowText(args[0], Convert.ToInt32(args[1]), Convert.ToInt32(args[2])),
        ["window-scale"] = (args) => Player.RaiseWindowScaleNET(float.Parse(args[0], CultureInfo.InvariantCulture)),
    };

    static void LoadSubtitle(string[] args)
    {
        using var d = new OpenFileDialog();
        string path = Player.GetPropertyString("path");

        if (File.Exists(path))
            d.InitialDirectory = Path.GetDirectoryName(path);

        d.Multiselect = true;

        if (d.ShowDialog() == DialogResult.OK)
            foreach (string filename in d.FileNames)
                Player.CommandV("sub-add", filename);
    }

    static void ShowDialog(Type winType)
    {
        Window win = Activator.CreateInstance(winType) as Window;
        new WindowInteropHelper(win).Owner = MainForm.Instance.Handle;
        win.ShowDialog();
    }

    static void OpenFiles(string[] args)
    {
        bool append = false;

        foreach (string arg in args)
            if (arg == "append")
                append = true;

        using var d = new OpenFileDialog() { Multiselect = true };

        if (d.ShowDialog() == DialogResult.OK)
            Player.LoadFiles(d.FileNames, true, append);
    }

    static void Open_DVD_Or_BD_Folder(string[] args)
    {
        var dialog = new FolderBrowser();

        if (dialog.Show())
            Player.LoadDiskFolder(dialog.SelectedPath);
    }

    static void PlayPause(string[] args)
    {
        int count = Player.GetPropertyInt("playlist-count");

        if (count > 0)
            Player.Command("cycle pause");
        else if (App.Settings.RecentFiles.Count > 0)
        {
            foreach (string i in App.Settings.RecentFiles)
            {
                if (i.Contains("://") || File.Exists(i))
                {
                    Player.LoadFiles(new[] { i }, true, false);
                    break;
                }
            }
        }
    }

    static void ShowInfo(string[] args)
    {
        if (Player.PlaylistPos == -1)
            return;

        string text;
        long fileSize = 0;
        string path = Player.GetPropertyString("path");

        if (File.Exists(path))
        {
            if (FileTypes.Audio.Contains(path.Ext()))
            {
                text = Player.GetPropertyOsdString("filtered-metadata");
                Player.CommandV("show-text", text, "5000");
                return;
            }
            else if (FileTypes.Image.Contains(path.Ext()))
            {
                fileSize = new FileInfo(path).Length;
                text = "Width: " + Player.GetPropertyInt("width") + "\n" +
                       "Height: " + Player.GetPropertyInt("height") + "\n" +
                       "Size: " + Convert.ToInt32(fileSize / 1024.0) + " KB\n" +
                       "Type: " + path.Ext().ToUpper();

                Player.CommandV("show-text", text, "5000");
                return;
            }
            else
            {
                Player.Command("script-message-to mpvgui show-media-info osd");
                return;
            }
        }

        if (path.Contains("://")) path = Player.GetPropertyString("media-title");
        string videoFormat = Player.GetPropertyString("video-format").ToUpper();
        string audioCodec = Player.GetPropertyString("audio-codec-name").ToUpper();
        int width = Player.GetPropertyInt("video-params/w");
        int height = Player.GetPropertyInt("video-params/h");
        TimeSpan len = TimeSpan.FromSeconds(Player.GetPropertyDouble("duration"));
        text = path.FileName() + "\n";
        text += FormatTime(len.TotalMinutes) + ":" + FormatTime(len.Seconds) + "\n";
        if (fileSize > 0) text += Convert.ToInt32(fileSize / 1024.0 / 1024.0) + " MB\n";
        text += $"{width} x {height}\n";
        text += $"{videoFormat}\n{audioCodec}";
        Player.CommandV("show-text", text, "5000");
    }

    static string FormatTime(double value) => ((int)value).ToString("00");

    static void ShowProgress(string[] args)
    {
        TimeSpan position = TimeSpan.FromSeconds(Player.GetPropertyDouble("time-pos"));
        TimeSpan duration = TimeSpan.FromSeconds(Player.GetPropertyDouble("duration"));

        string text = FormatTime(position.TotalMinutes) + ":" +
                      FormatTime(position.Seconds) + " / " +
                      FormatTime(duration.TotalMinutes) + ":" +
                      FormatTime(duration.Seconds) + "    " +
                      DateTime.Now.ToString("H:mm dddd d MMMM", CultureInfo.InvariantCulture);

        Player.CommandV("show-text", text, "5000");
    }

    static void OpenFromClipboard(string[] args)
    {
        if (System.Windows.Forms.Clipboard.ContainsFileDropList())
        {
            string[] files = System.Windows.Forms.Clipboard.GetFileDropList().Cast<string>().ToArray();
            Player.LoadFiles(files, false, false);
        }
        else
        {
            string clipboard = System.Windows.Forms.Clipboard.GetText();
            List<string> files = new List<string>();

            foreach (string i in clipboard.Split(BR.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                if (i.Contains("://") || File.Exists(i))
                    files.Add(i);

            if (files.Count == 0)
            {
                Terminal.WriteError("The clipboard does not contain a valid URL or file.");
                return;
            }

            Player.LoadFiles(files.ToArray(), false, false);
        }
    }

    static void LoadAudio(string[] args)
    {
        using var d = new OpenFileDialog();
        string path = Player.GetPropertyString("path");

        if (File.Exists(path))
            d.InitialDirectory = Path.GetDirectoryName(path);

        d.Multiselect = true;

        if (d.ShowDialog() == DialogResult.OK)
            foreach (string i in d.FileNames)
                Player.CommandV("audio-add", i);
    }

    static void ShowText(string text, int duration = 0, int fontSize = 0)
    {
        if (string.IsNullOrEmpty(text))
            return;

        if (duration == 0)
            duration = Player.GetPropertyInt("osd-duration");

        if (fontSize == 0)
            fontSize = Player.GetPropertyInt("osd-font-size");

        Player.Command("show-text \"${osd-ass-cc/0}{\\\\fs" + fontSize +
            "}${osd-ass-cc/1}" + text + "\" " + duration);
    }
    
    static void RegisterFileAssociations(string[] args)
    {
        string perceivedType = args[0];
        string[] extensions = {};

        switch (perceivedType)
        {
            case "video": extensions = FileTypes.Video; break;
            case "audio": extensions = FileTypes.Audio; break;
            case "image": extensions = FileTypes.Image; break;
        }

        try
        {
            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = System.Windows.Forms.Application.ExecutablePath;
                proc.StartInfo.Arguments = "--register-file-associations " +
                    perceivedType + " " + string.Join(" ", extensions);
                proc.StartInfo.Verb = "runas";
                proc.StartInfo.UseShellExecute = true;
                proc.Start();
                proc.WaitForExit();

                if (proc.ExitCode == 0)
                    Msg.ShowInfo("File associations were successfully " + 
                        (perceivedType == "unreg" ? "removed" : "created") +
                        ".\n\nFile Explorer icons will refresh after process restart.");
                else
                    Msg.ShowError("Error creating file associations.");
            }
        } catch { }
    }
    
    static void PlaylistAdd(string[] args)
    {
        int pos = Player.PlaylistPos;
        int count = Player.GetPropertyInt("playlist-count");

        if (count < 2)
            return;

        pos = pos + Convert.ToInt32(args[0]);

        if (pos < 0)
            pos = count - 1;

        if (pos > count - 1)
            pos = 0;

        Player.SetPropertyInt("playlist-pos", pos);
    }
}
