﻿
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;

using static mpvgui.libmpv;

namespace mpvgui;

public class PlayerClass
{
    public event Action<string, ArraySegment<string>>? ClientMessage;  // client-message  MPV_EVENT_CLIENT_MESSAGE
    public event Action<mpv_log_level, string>? LogMessage;  // log-message         MPV_EVENT_LOG_MESSAGE
    public event Action<mpv_end_file_reason>? EndFile;       // end-file            MPV_EVENT_END_FILE
    public event Action? Shutdown;                           // shutdown            MPV_EVENT_SHUTDOWN
    public event Action? GetPropertyReply;                   // get-property-reply  MPV_EVENT_GET_PROPERTY_REPLY
    public event Action? SetPropertyReply;                   // set-property-reply  MPV_EVENT_SET_PROPERTY_REPLY
    public event Action? CommandReply;                       // command-reply       MPV_EVENT_COMMAND_REPLY
    public event Action? StartFile;                          // start-file          MPV_EVENT_START_FILE
    public event Action? FileLoaded;                         // file-loaded         MPV_EVENT_FILE_LOADED
    public event Action? VideoReconfig;                      // video-reconfig      MPV_EVENT_VIDEO_RECONFIG
    public event Action? AudioReconfig;                      // audio-reconfig      MPV_EVENT_AUDIO_RECONFIG
    public event Action? Seek;                               // seek                MPV_EVENT_SEEK
    public event Action? PlaybackRestart;                    // playback-restart    MPV_EVENT_PLAYBACK_RESTART

    public event Action? Initialized;
    public event Action? Pause;
    public event Action? ShowMenu;
    public event Action<double>? WindowScaleMpv;
    public event Action<float>? ScaleWindow;
    public event Action<float>? WindowScaleNET;
    public event Action<int>? PlaylistPosChanged;
    public event Action<Size>? VideoSizeChanged;
    public event Action<string>? MoveWindow;

    public Dictionary<string, List<Action>>               PropChangeActions { get; set; } = new Dictionary<string, List<Action>>();
    public Dictionary<string, List<Action<int>>>       IntPropChangeActions { get; set; } = new Dictionary<string, List<Action<int>>>();
    public Dictionary<string, List<Action<bool>>>     BoolPropChangeActions { get; set; } = new Dictionary<string, List<Action<bool>>>();
    public Dictionary<string, List<Action<double>>> DoublePropChangeActions { get; set; } = new Dictionary<string, List<Action<double>>>();
    public Dictionary<string, List<Action<string>>> StringPropChangeActions { get; set; } = new Dictionary<string, List<Action<string>>>();

    public AutoResetEvent ShutdownAutoResetEvent { get; } = new AutoResetEvent(false);
    public AutoResetEvent VideoSizeAutoResetEvent { get; } = new AutoResetEvent(false);
    public IntPtr Handle { get; set; }
    public IntPtr NamedHandle { get; set; }
    public List<MediaTrack> MediaTracks { get; set; } = new List<MediaTrack>();
    public List<TimeSpan> BluRayTitles { get; } = new List<TimeSpan>();
    public object MediaTracksLock { get; } = new object();
    public Size VideoSize { get; set; }
    public TimeSpan Duration;

    public string ConfPath { get => ConfigFolder + "mpv.conf"; }
    public string GPUAPI { get; set; } = "auto";
    public string InputConfPath => ConfigFolder + "input.conf";
    public string Path { get; set; } = "";
    public string VO { get; set; } = "gpu";

    public string VID { get; set; } = "";
    public string AID { get; set; } = "";
    public string SID { get; set; } = "";

    public bool Border { get; set; } = true;
    public bool FileEnded { get; set; }
    public bool Fullscreen { get; set; }
    public bool IsQuitNeeded { set; get; } = true;
    public bool KeepaspectWindow { get; set; }
    public bool Paused { get; set; }
    public bool Shown { get; set; }
    public bool SnapWindow { get; set; }
    public bool TaskbarProgress { get; set; } = true;
    public bool WasInitialSizeSet;
    public bool WindowMaximized { get; set; }
    public bool WindowMinimized { get; set; }

    public int Edition { get; set; }
    public int PlaylistPos { get; set; } = -1;
    public int Screen { get; set; } = -1;
    public int VideoRotate { get; set; }

    public float Autofit { get; set; } = 0.6f;
    public float AutofitSmaller { get; set; } = 0.3f;
    public float AutofitLarger { get; set; } = 0.8f;

    public void Init(IntPtr handle)
    {
        ApplyShowMenuFix();

        Handle = mpv_create();

        var events = Enum.GetValues(typeof(mpv_event_id)).Cast<mpv_event_id>();

        foreach (mpv_event_id i in events)
            mpv_request_event(Handle, i, 0);

        mpv_request_log_messages(Handle, "no");

        App.RunTask(MainEventLoop);

        if (Handle == IntPtr.Zero)
            throw new Exception("error mpv_create");

        if (App.IsTerminalAttached)
        {
            SetPropertyString("terminal", "yes");
            SetPropertyString("input-terminal", "yes");
        }

        SetPropertyInt("osd-duration", 2000);
        SetPropertyLong("wid", handle.ToInt64());

        SetPropertyBool("input-default-bindings", true);
        SetPropertyBool("input-builtin-bindings", false);

        SetPropertyString("watch-later-options", "mute");
        SetPropertyString("screenshot-directory", "~~desktop/");
        SetPropertyString("osd-playing-msg", "${media-title}");
        SetPropertyString("osc", "yes");
        SetPropertyString("force-window", "yes");
        SetPropertyString("config-dir", ConfigFolder);
        SetPropertyString("config", "yes");
        
        //SetPropertyString("input-conf", @"C:\Users\frank\AppData\Roaming\mpv-gui\trash\test-input.conf");

        ProcessCommandLine(true);

        Environment.SetEnvironmentVariable("MPVNET_VERSION", AppInfo.Version.ToString());

        mpv_error err = mpv_initialize(Handle);

        if (err < 0)
            throw new Exception("mpv_initialize error" + BR2 + GetError(err) + BR);

        string idle = GetPropertyString("idle");
        App.Exit = idle == "no" || idle == "once";

        NamedHandle = mpv_create_client(Handle, "mpvgui");

        if (NamedHandle == IntPtr.Zero)
            throw new Exception("mpv_create_client error");

        mpv_request_log_messages(NamedHandle, "terminal-default");

        App.RunTask(EventLoop);

        // otherwise shutdown is raised before media files are loaded,
        // this means Lua scripts that use idle might not work correctly
        SetPropertyString("idle", "yes");

        ObservePropertyDouble("window-scale", value => WindowScaleMpv?.Invoke(value));
      
        ObservePropertyString("path", value => Path = value);

        ObservePropertyBool("pause", value => {
            Paused = value;
            Pause?.Invoke();
        });

        ObservePropertyInt("video-rotate", value => {
            VideoRotate = value;
            UpdateVideoSize("dwidth", "dheight");
        });

        ObservePropertyInt("playlist-pos", value => {
            PlaylistPos = value;
            PlaylistPosChanged?.Invoke(value);

            if (FileEnded && value == -1)
                if (GetPropertyString("keep-open") == "no" && App.Exit)
                    Player.CommandV("quit");
        });

        if (!GetPropertyBool("osd-scale-by-window"))
            App.StartThreshold = 0;

        Initialized?.Invoke();
    }

    public void Destroy()
    {
        mpv_destroy(Handle);
        mpv_destroy(NamedHandle);
    }

    void ApplyShowMenuFix()
    {
        if (App.Settings.ShowMenuFixApplied)
            return;

        if (File.Exists(InputConfPath))
        {
            string content = File.ReadAllText(InputConfPath);

            if (!content.Contains("script-message mpv-gui show-menu") &&
                !content.Contains("script-message-to mpvgui show-menu"))

                File.WriteAllText(InputConfPath, BR + content.Trim() + BR +
                    "MBTN_Right script-message-to mpvgui show-menu" + BR);
        }

        App.Settings.ShowMenuFixApplied = true;
    }

    void ApplyInputDefaultBindingsFix()
    {
        if (App.Settings.InputDefaultBindingsFixApplied)
            return;

        if (File.Exists(ConfPath))
        {
            string content = File.ReadAllText(ConfPath);

            if (content.Contains("input-default-bindings = no"))
                File.WriteAllText(ConfPath, content.Replace("input-default-bindings = no", ""));

            if (content.Contains("input-default-bindings=no"))
                File.WriteAllText(ConfPath, content.Replace("input-default-bindings=no", ""));
        }

        App.Settings.InputDefaultBindingsFixApplied = true;
    }

    public void ProcessProperty(string? name, string? value)
    {
        switch (name)
        {
            case "autofit":
                {
                    if (int.TryParse(value?.Trim('%'), out int result))
                        Autofit = result / 100f;
                }
                break;
            case "autofit-smaller":
                {
                    if (int.TryParse(value?.Trim('%'), out int result))
                        AutofitSmaller = result / 100f;
                }
                break;
            case "autofit-larger":
                {
                    if (int.TryParse(value?.Trim('%'), out int result))
                        AutofitLarger = result / 100f;
                }
                break;
            case "border": Border = value == "yes"; break;
            case "fs":
            case "fullscreen": Fullscreen = value == "yes"; break;
            case "gpu-api": GPUAPI = value!; break;
            case "keepaspect-window": KeepaspectWindow = value == "yes"; break;
            case "screen": Screen = Convert.ToInt32(value); break;
            case "snap-window": SnapWindow = value == "yes"; break;
            case "taskbar-progress": TaskbarProgress = value == "yes"; break;
            case "vo": VO = value!; break;
            case "window-maximized": WindowMaximized = value == "yes"; break;
            case "window-minimized": WindowMinimized = value == "yes"; break;
        }

        if (AutofitLarger > 1)
            AutofitLarger = 1;
    }

    string? _InputConfContent;

    public string InputConfContent {
        get {
            if (_InputConfContent == null)
                _InputConfContent = File.ReadAllText(Player.InputConfPath);
            return _InputConfContent;
        }
    }

    string? _ConfigFolder;

    public string ConfigFolder {
        get {
            if (_ConfigFolder == null)
            {
                _ConfigFolder = Folder.Startup + "portable_config";

                if (!Directory.Exists(_ConfigFolder))
                    _ConfigFolder = Folder.AppData + "mpv-gui";

                if (!Directory.Exists(_ConfigFolder))
                {
                    try {
                        using (Process proc = new Process())
                        {
                            proc.StartInfo.UseShellExecute = false;
                            proc.StartInfo.CreateNoWindow = true;
                            proc.StartInfo.FileName = "powershell.exe";
                            proc.StartInfo.Arguments = $@"-Command New-Item -Path '{_ConfigFolder}' -ItemType Directory";
                            proc.Start();
                            proc.WaitForExit();
                        }
                    } catch (Exception) {}

                    if (!Directory.Exists(_ConfigFolder))
                        Directory.CreateDirectory(_ConfigFolder);
                }

                _ConfigFolder = _ConfigFolder.AddSep();

                // TODO
                // if (!File.Exists(_ConfigFolder + "input.conf"))
                // {
                //     File.WriteAllText(_ConfigFolder + "input.conf", Properties.Resources.input_conf);
                //
                //     string scriptOptsPath = _ConfigFolder + "script-opts" + System.IO.Path.DirectorySeparatorChar;
                //
                //     if (!Directory.Exists(scriptOptsPath))
                //     {
                //         Directory.CreateDirectory(scriptOptsPath);
                //         File.WriteAllText(scriptOptsPath + "console.conf", BR + "scale=1.5" + BR);
                //         string content = BR + "scalewindowed=1.5" + BR + "hidetimeout=2000" + BR +
                //                          "idlescreen=no" + BR;
                //         File.WriteAllText(scriptOptsPath + "osc.conf", content);
                //     }
                // }
            }

            return _ConfigFolder;
        }
    }

    Dictionary<string, string>? _Conf;

    public Dictionary<string, string> Conf {
        get {
            if (_Conf == null)
            {
                ApplyInputDefaultBindingsFix();

                _Conf = new Dictionary<string, string>();

                if (File.Exists(ConfPath))
                    foreach (var i in File.ReadAllLines(ConfPath))
                        if (i.Contains("=") && !i.TrimStart().StartsWith("#"))
                        {
                            string key = i[..i.IndexOf("=")].Trim();
                            string value = i[(i.IndexOf("=") + 1)..].Trim();

                            if (key.StartsWith("-"))
                                key = key.TrimStart('-');

                            if (value.Contains('#') && !value.StartsWith("#") &&
                                !value.StartsWith("'#") && !value.StartsWith("\"#"))

                                value = value[..value.IndexOf("#")].Trim();

                            _Conf[key] = value;
                        }

                foreach (var i in _Conf)
                    ProcessProperty(i.Key, i.Value);
            }

            return _Conf;
        }
    }

    void UpdateVideoSize(string w, string h)
    {
        Size size = new Size(GetPropertyInt(w), GetPropertyInt(h));

        if (size.Width == 0 || size.Height == 0)
            return;

        if (VideoRotate == 90 || VideoRotate == 270)
            size = new Size(size.Height, size.Width);

        if (VideoSize != size)
        {
            VideoSize = size;
            VideoSizeChanged?.Invoke(size);
            VideoSizeAutoResetEvent.Set();
        }
    }

    public void MainEventLoop()
    {
        while (true)
            mpv_wait_event(Handle, -1);
    }

    public void EventLoop()
    {
        while (true)
        {
            IntPtr ptr = mpv_wait_event(NamedHandle, -1);
            mpv_event evt = (mpv_event)Marshal.PtrToStructure(ptr, typeof(mpv_event))!;

            try
            {
                switch (evt.event_id)
                {
                    case mpv_event_id.MPV_EVENT_SHUTDOWN:
                        IsQuitNeeded = false;
                        Shutdown?.Invoke();
                        ShutdownAutoResetEvent.Set();
                        return;
                    case mpv_event_id.MPV_EVENT_LOG_MESSAGE:
                        {
                            var data = (mpv_event_log_message)Marshal.PtrToStructure(evt.data, typeof(mpv_event_log_message))!;

                            if (data.log_level == mpv_log_level.MPV_LOG_LEVEL_INFO)
                            {
                                string prefix = ConvertFromUtf8(data.prefix);

                                if (prefix == "bd")
                                    ProcessBluRayLogMessage(ConvertFromUtf8(data.text));
                            }

                            if (LogMessage != null)
                            {
                                string msg = $"[{ConvertFromUtf8(data.prefix)}] {ConvertFromUtf8(data.text)}";
                                LogMessage.Invoke(data.log_level, msg);
                            }
                        }
                        break;
                    case mpv_event_id.MPV_EVENT_CLIENT_MESSAGE:
                        {
                            var data = (mpv_event_client_message)Marshal.PtrToStructure(evt.data, typeof(mpv_event_client_message))!;
                            string[] args = ConvertFromUtf8Strings(data.args, data.num_args);
                            ClientMessage?.Invoke(args[0], new ArraySegment<string>(args, 1, args.Length - 1));
                        }
                        break;
                    case mpv_event_id.MPV_EVENT_VIDEO_RECONFIG:
                        UpdateVideoSize("dwidth", "dheight");
                        VideoReconfig?.Invoke();
                        break;
                    case mpv_event_id.MPV_EVENT_END_FILE:
                        {
                            var data = (mpv_event_end_file)Marshal.PtrToStructure(evt.data, typeof(mpv_event_end_file))!;
                            var reason = (mpv_end_file_reason)data.reason;
                            EndFile?.Invoke(reason);
                            FileEnded = true;
                        }
                        break;
                    case mpv_event_id.MPV_EVENT_FILE_LOADED:
                        {
                            if (App.AutoPlay && Paused)
                                SetPropertyBool("pause", false);

                            App.QuickBookmark = 0;
                            
                            Duration = TimeSpan.FromSeconds(GetPropertyDouble("duration"));

                            if (App.StartSize == "video")
                                WasInitialSizeSet = false;

                            string path = GetPropertyString("path");

                            if (!FileTypes.Video.Contains(path.Ext()) || FileTypes.Audio.Contains(path.Ext()))
                            {
                                UpdateVideoSize("width", "height");
                                VideoSizeAutoResetEvent.Set();
                            }

                            App.RunTask(UpdateTracks);
                            FileLoaded?.Invoke();
                        }
                        break;
                    case mpv_event_id.MPV_EVENT_PROPERTY_CHANGE:
                        {
                            var data = (mpv_event_property)Marshal.PtrToStructure(evt.data, typeof(mpv_event_property))!;

                            if (data.format == mpv_format.MPV_FORMAT_FLAG)
                            {
                                lock (BoolPropChangeActions)
                                    foreach (var pair in BoolPropChangeActions)
                                        if (pair.Key == data.name)
                                        {
                                            bool value = Marshal.PtrToStructure<int>(data.data) == 1;

                                            foreach (var action in pair.Value)
                                                action.Invoke(value);
                                        }
                            }
                            else if (data.format == mpv_format.MPV_FORMAT_STRING)
                            {
                                lock (StringPropChangeActions)
                                    foreach (var pair in StringPropChangeActions)
                                        if (pair.Key == data.name)
                                        {
                                            string value = ConvertFromUtf8(Marshal.PtrToStructure<IntPtr>(data.data));

                                            foreach (var action in pair.Value)
                                                action.Invoke(value);
                                        }
                            }
                            else if (data.format == mpv_format.MPV_FORMAT_INT64)
                            {
                                lock (IntPropChangeActions)
                                    foreach (var pair in IntPropChangeActions)
                                        if (pair.Key == data.name)
                                        {
                                            int value = Marshal.PtrToStructure<int>(data.data);

                                            foreach (var action in pair.Value)
                                                action.Invoke(value);
                                        }
                            }
                            else if (data.format == mpv_format.MPV_FORMAT_NONE)
                            {
                                lock (PropChangeActions)
                                    foreach (var pair in PropChangeActions)
                                        if (pair.Key == data.name)
                                            foreach (var action in pair.Value)
                                                action.Invoke();
                            }
                            else if (data.format == mpv_format.MPV_FORMAT_DOUBLE)
                            {
                                lock (DoublePropChangeActions)
                                    foreach (var pair in DoublePropChangeActions)
                                        if (pair.Key == data.name)
                                        {
                                            double value = Marshal.PtrToStructure<double>(data.data);

                                            foreach (var action in pair.Value)
                                                action.Invoke(value);
                                        }
                            }
                        }
                        break;
                    case mpv_event_id.MPV_EVENT_GET_PROPERTY_REPLY:
                        GetPropertyReply?.Invoke();
                        break;
                    case mpv_event_id.MPV_EVENT_SET_PROPERTY_REPLY:
                        SetPropertyReply?.Invoke();
                        break;
                    case mpv_event_id.MPV_EVENT_COMMAND_REPLY:
                        CommandReply?.Invoke();
                        break;
                    case mpv_event_id.MPV_EVENT_START_FILE:
                        StartFile?.Invoke();
                        App.RunTask(LoadFolder);
                        break;
                    case mpv_event_id.MPV_EVENT_AUDIO_RECONFIG:
                        AudioReconfig?.Invoke();
                        break;
                    case mpv_event_id.MPV_EVENT_SEEK:
                        Seek?.Invoke();
                        break;
                    case mpv_event_id.MPV_EVENT_PLAYBACK_RESTART:
                        PlaybackRestart?.Invoke();
                        break;
                }
            } catch (Exception ex) {
                Terminal.WriteError(ex);
            }
        }
    }

    void ProcessBluRayLogMessage(string msg)
    {
        lock (BluRayTitles)
        {
            if (msg.Contains(" 0 duration: "))
                BluRayTitles.Clear();

            if (msg.Contains(" duration: "))
            {
                int start = msg.IndexOf(" duration: ") + 11;
                BluRayTitles.Add(new TimeSpan(
                    msg.Substring(start, 2).ToInt(),
                    msg.Substring(start + 3, 2).ToInt(),
                    msg.Substring(start + 6, 2).ToInt()));
            }
        }
    }

    public void SetBluRayTitle(int id) => LoadFiles(new[] { @"bd://" + id }, false, false);

    public void Command(string command)
    {
        mpv_error err = mpv_command_string(Handle, command);

        if (err < 0)
            HandleError(err, "error executing command: " + command);
    }

    public void CommandV(params string[] args)
    {
        int count = args.Length + 1;
        IntPtr[] pointers = new IntPtr[count];
        IntPtr rootPtr = Marshal.AllocHGlobal(IntPtr.Size * count);

        for (int index = 0; index < args.Length; index++)
        {
            var bytes = GetUtf8Bytes(args[index]);
            IntPtr ptr = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            pointers[index] = ptr;
        }

        Marshal.Copy(pointers, 0, rootPtr, count);
        mpv_error err = mpv_command(Handle, rootPtr);

        foreach (IntPtr ptr in pointers)
            Marshal.FreeHGlobal(ptr);

        Marshal.FreeHGlobal(rootPtr);

        if (err < 0)
            HandleError(err, "error executing command: " + string.Join("\n", args));
    }

    public string Expand(string? value)
    {
        if (value == null)
            return "";

        if (!value.Contains("${"))
            return value;

        string[] args = { "expand-text", value };
        int count = args.Length + 1;
        IntPtr[] pointers = new IntPtr[count];
        IntPtr rootPtr = Marshal.AllocHGlobal(IntPtr.Size * count);

        for (int index = 0; index < args.Length; index++)
        {
            var bytes = GetUtf8Bytes(args[index]);
            IntPtr ptr = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            pointers[index] = ptr;
        }

        Marshal.Copy(pointers, 0, rootPtr, count);
        IntPtr resultNodePtr = Marshal.AllocHGlobal(16);
        mpv_error err = mpv_command_ret(Handle, rootPtr, resultNodePtr);

        foreach (IntPtr ptr in pointers)
            Marshal.FreeHGlobal(ptr);

        Marshal.FreeHGlobal(rootPtr);

        if (err < 0)
        {
            HandleError(err, "error executing command: " + string.Join("\n", args));
            Marshal.FreeHGlobal(resultNodePtr);
            return "property expansion error";
        }

        mpv_node resultNode = Marshal.PtrToStructure<mpv_node>(resultNodePtr);
        string ret = ConvertFromUtf8(resultNode.str);
        mpv_free_node_contents(resultNodePtr);
        Marshal.FreeHGlobal(resultNodePtr);
        return ret;
    }

    public bool GetPropertyBool(string name)
    {
        mpv_error err = mpv_get_property(Handle, GetUtf8Bytes(name),
            mpv_format.MPV_FORMAT_FLAG, out IntPtr lpBuffer);

        if (err < 0)
            HandleError(err, "error getting property: " + name);

        return lpBuffer.ToInt32() != 0;
    }

    public void SetPropertyBool(string name, bool value)
    {
        long val = value ? 1 : 0;
        mpv_error err = mpv_set_property(Handle, GetUtf8Bytes(name), mpv_format.MPV_FORMAT_FLAG, ref val);

        if (err < 0)
            HandleError(err, $"error setting property: {name} = {value}");
    }

    public int GetPropertyInt(string name)
    {
        mpv_error err = mpv_get_property(Handle, GetUtf8Bytes(name),
            mpv_format.MPV_FORMAT_INT64, out IntPtr lpBuffer);

        if (err < 0 && App.DebugMode)
            HandleError(err, "error getting property: " + name);

        return lpBuffer.ToInt32();
    }

    public void SetPropertyInt(string name, int value)
    {
        long val = value;
        mpv_error err = mpv_set_property(Handle, GetUtf8Bytes(name), mpv_format.MPV_FORMAT_INT64, ref val);

        if (err < 0)
            HandleError(err, $"error setting property: {name} = {value}");
    }

    public void SetPropertyLong(string name, long value)
    {
        mpv_error err = mpv_set_property(Handle, GetUtf8Bytes(name), mpv_format.MPV_FORMAT_INT64, ref value);

        if (err < 0)
            HandleError(err, $"error setting property: {name} = {value}");
    }

    public long GetPropertyLong(string name)
    {
        mpv_error err = mpv_get_property(Handle, GetUtf8Bytes(name),
            mpv_format.MPV_FORMAT_INT64, out IntPtr lpBuffer);

        if (err < 0)
            HandleError(err, "error getting property: " + name);

        return lpBuffer.ToInt64();
    }

    public double GetPropertyDouble(string name, bool handleError = true)
    {
        mpv_error err = mpv_get_property(Handle, GetUtf8Bytes(name),
            mpv_format.MPV_FORMAT_DOUBLE, out double value);

        if (err < 0 && handleError && App.DebugMode)
            HandleError(err, "error getting property: " + name);

        return value;
    }

    public void SetPropertyDouble(string name, double value)
    {
        double val = value;
        mpv_error err = mpv_set_property(Handle, GetUtf8Bytes(name), mpv_format.MPV_FORMAT_DOUBLE, ref val);

        if (err < 0)
            HandleError(err, $"error setting property: {name} = {value}");
    }

    public string GetPropertyString(string name)
    {
        mpv_error err = mpv_get_property(Handle, GetUtf8Bytes(name),
            mpv_format.MPV_FORMAT_STRING, out IntPtr lpBuffer);

        if (err == 0)
        {
            string ret = ConvertFromUtf8(lpBuffer);
            mpv_free(lpBuffer);
            return ret;
        }

        if (err < 0 && App.DebugMode)
            HandleError(err, "error getting property: " + name);

        return "";
    }

    public void SetPropertyString(string name, string value)
    {
        byte[] bytes = GetUtf8Bytes(value);
        mpv_error err = mpv_set_property(Handle, GetUtf8Bytes(name), mpv_format.MPV_FORMAT_STRING, ref bytes);

        if (err < 0)
            HandleError(err, $"error setting property: {name} = {value}");
    }

    public string GetPropertyOsdString(string name)
    {
        mpv_error err = mpv_get_property(Handle, GetUtf8Bytes(name),
            mpv_format.MPV_FORMAT_OSD_STRING, out IntPtr lpBuffer);

        if (err == 0)
        {
            string ret = ConvertFromUtf8(lpBuffer);
            mpv_free(lpBuffer);
            return ret;
        }

        if (err < 0)
            HandleError(err, "error getting property: " + name);

        return "";
    }

    public void ObservePropertyInt(string name, Action<int> action)
    {
        lock (IntPropChangeActions)
        {
            if (!IntPropChangeActions.ContainsKey(name))
            {
                mpv_error err = mpv_observe_property(NamedHandle, 0, name, mpv_format.MPV_FORMAT_INT64);

                if (err < 0)
                    HandleError(err, "error observing property: " + name);
                else
                    IntPropChangeActions[name] = new List<Action<int>>();
            }

            if (IntPropChangeActions.ContainsKey(name))
                IntPropChangeActions[name].Add(action);
        }
    }

    public void ObservePropertyDouble(string name, Action<double> action)
    {
        lock (DoublePropChangeActions)
        {
            if (!DoublePropChangeActions.ContainsKey(name))
            {
                mpv_error err = mpv_observe_property(NamedHandle, 0, name, mpv_format.MPV_FORMAT_DOUBLE);

                if (err < 0)
                    HandleError(err, "error observing property: " + name);
                else
                    DoublePropChangeActions[name] = new List<Action<double>>();
            }

            if (DoublePropChangeActions.ContainsKey(name))
                DoublePropChangeActions[name].Add(action);
        }
    }

    public void ObservePropertyBool(string name, Action<bool> action)
    {
        lock (BoolPropChangeActions)
        {
            if (!BoolPropChangeActions.ContainsKey(name))
            {
                mpv_error err = mpv_observe_property(NamedHandle, 0, name, mpv_format.MPV_FORMAT_FLAG);

                if (err < 0)
                    HandleError(err, "error observing property: " + name);
                else
                    BoolPropChangeActions[name] = new List<Action<bool>>();
            }

            if (BoolPropChangeActions.ContainsKey(name))
                BoolPropChangeActions[name].Add(action);
        }
    }

    public void ObservePropertyString(string name, Action<string> action)
    {
        lock (StringPropChangeActions)
        {
            if (!StringPropChangeActions.ContainsKey(name))
            {
                mpv_error err = mpv_observe_property(NamedHandle, 0, name, mpv_format.MPV_FORMAT_STRING);

                if (err < 0)
                    HandleError(err, "error observing property: " + name);
                else
                    StringPropChangeActions[name] = new List<Action<string>>();
            }

            if (StringPropChangeActions.ContainsKey(name))
                StringPropChangeActions[name].Add(action);
        }
    }

    public void ObserveProperty(string name, Action action)
    {
        lock (PropChangeActions)
        {
            if (!PropChangeActions.ContainsKey(name))
            {
                mpv_error err = mpv_observe_property(NamedHandle, 0, name, mpv_format.MPV_FORMAT_NONE);

                if (err < 0)
                    HandleError(err, "error observing property: " + name);
                else
                    PropChangeActions[name] = new List<Action>();
            }

            if (PropChangeActions.ContainsKey(name))
                PropChangeActions[name].Add(action);
        }
    }

    public void HandleError(mpv_error err, string msg)
    {
        Terminal.WriteError(msg);
        Terminal.WriteError(GetError(err));
    }

    public void ProcessCommandLine(bool preInit)
    {
        bool shuffle = false;
        var args = Environment.GetCommandLineArgs().Skip(1);

        string[] preInitProperties = { "input-terminal", "terminal", "input-file", "config",
            "config-dir", "input-conf", "load-scripts", "scripts", "player-operation-mode",
            "idle", "log-file", "msg-color", "dump-stats", "msg-level", "really-quiet" };

        foreach (string i in args)
        {
            string arg = i;

            if (arg.StartsWith("-") && arg.Length > 1)
            {
                if (!preInit)
                {
                    if (arg == "--profile=help")
                    {
                        Console.WriteLine(GetProfiles());
                        continue;
                    }
                    else if (arg == "--vd=help" || arg == "--ad=help")
                    {
                        Console.WriteLine(GetDecoders());
                        continue;
                    }
                    else if (arg == "--audio-device=help")
                    {
                        Console.WriteLine(GetPropertyOsdString("audio-device-list"));
                        continue;
                    }
                    else if (arg == "--version")
                    {
                        Console.WriteLine(App.About);
                        continue;
                    }
                    else if (arg == "--input-keylist")
                    {
                        Console.WriteLine(GetPropertyString("input-key-list").Replace(",", BR));
                        continue;
                    }
                    else if (arg.StartsWith("--command="))
                    {
                        Command(arg.Substring(10));
                        continue;
                    }
                }

                if (!arg.StartsWith("--"))
                    arg = "-" + arg;

                if (!arg.Contains("="))
                {
                    if (arg.Contains("--no-"))
                    {
                        arg = arg.Replace("--no-", "--");
                        arg += "=no";
                    }
                    else
                        arg += "=yes";
                }

                string left = arg.Substring(2, arg.IndexOf("=") - 2);
                string right = arg.Substring(left.Length + 3);

                switch (left)
                {
                    case "script":        left = "scripts";        break;
                    case "audio-file":    left = "audio-files";    break;
                    case "sub-file":      left = "sub-files";      break;
                    case "external-file": left = "external-files"; break;
                }

                if (preInit && preInitProperties.Contains(left))
                {
                    ProcessProperty(left, right);

                    if (!App.ProcessProperty(left, right))
                        SetPropertyString(left, right);
                }
                else if (!preInit && !preInitProperties.Contains(left))
                {
                    ProcessProperty(left, right);

                    if (!App.ProcessProperty(left, right))
                    {
                        SetPropertyString(left, right);

                        if (left == "shuffle" && right == "yes")
                            shuffle = true;
                    }
                }
            }
        }

        if (!preInit)
        {
            List<string> files = new List<string>();

            foreach (string i in args)
                if (!i.StartsWith("--") && (i == "-" || i.Contains("://") ||
                    i.Contains(":\\") || i.StartsWith("\\\\") || File.Exists(i)))

                    files.Add(i);

            LoadFiles(files.ToArray(), !App.Queue, false || App.Queue);

            if (shuffle)
            {
                Command("playlist-shuffle");
                SetPropertyInt("playlist-pos", 0);
            }

            if (files.Count == 0 || files[0].Contains("://"))
            {
                VideoSizeChanged?.Invoke(VideoSize);
                VideoSizeAutoResetEvent.Set();
            }
        }
    }

    public DateTime LastLoad;

    public void LoadFiles(string[]? files, bool loadFolder, bool append)
    {
        if (files is null || files.Length == 0)
            return;

        if ((DateTime.Now - LastLoad).TotalMilliseconds < 1000)
            append = true;

        LastLoad = DateTime.Now;

        for (int i = 0; i < files.Length; i++)
        {
            string file = files[i];

            if (string.IsNullOrEmpty(file))
                continue;

            if (file.Contains('|'))
                file = file.Substring(0, file.IndexOf("|"));

            file = ConvertFilePath(file);

            string ext = file.Ext();

            if (OperatingSystem.IsWindows())
            {
                switch (ext)
                {
                    case "avs": LoadAviSynth(); break;
                    case "lnk": file = GetShortcutTarget(file); break;
                }
            }

            if (ext == "iso")
                LoadBluRayISO(file);
            else if(FileTypes.Subtitle.Contains(ext))
                CommandV("sub-add", file);
            else if (!FileTypes.IsMedia(ext) && !file.Contains("://") && Directory.Exists(file) &&
                File.Exists(System.IO.Path.Combine(file, "BDMV\\index.bdmv")))
            {
                Command("stop");
                Thread.Sleep(500);
                SetPropertyString("bluray-device", file);
                CommandV("loadfile", @"bd://");
            }
            else
            {
                if (i == 0 && !append)
                    CommandV("loadfile", file);
                else
                    CommandV("loadfile", file, "append");
            }
        }

        if (string.IsNullOrEmpty(GetPropertyString("path")))
            SetPropertyInt("playlist-pos", 0);
    }

    public static string ConvertFilePath(string path)
    {
        if ((path.Contains(":/") && !path.Contains("://")) || (path.Contains(":\\") && path.Contains('/')))
            path = path.Replace("/", "\\");

        if (!path.Contains(':') && !path.StartsWith("\\\\") && File.Exists(path))
            path = System.IO.Path.GetFullPath(path);

        return path;
    }

    public void LoadBluRayISO(string path)
    {
        Command("stop");
        Thread.Sleep(500);
        SetPropertyString("bluray-device", path);
        LoadFiles(new[] { @"bd://" }, false, false);
    }

    public static void LoadDiskFolder(string path)
    {
        Player.Command("stop");
        Thread.Sleep(500);

        if (Directory.Exists(path + "\\BDMV"))
        {
            Player.SetPropertyString("bluray-device", path);
            Player.LoadFiles(new[] { @"bd://" }, false, false);
        }
        else
        {
            Player.SetPropertyString("dvd-device", path);
            Player.LoadFiles(new[] { @"dvd://" }, false, false);
        }
    }

    static readonly object LoadFolderLockObject = new object();

    public void LoadFolder()
    {
        if (!App.AutoLoadFolder)
            return;

        Thread.Sleep(1000);

        lock (LoadFolderLockObject)
        {
            string path = GetPropertyString("path");

            if (!File.Exists(path) || GetPropertyInt("playlist-count") != 1)
                return;

            string dir = Environment.CurrentDirectory;

            if (path.Contains(":/") && !path.Contains("://"))
                path = path.Replace("/", "\\");

            if (path.Contains('\\'))
                dir = System.IO.Path.GetDirectoryName(path)!;

            List<string> files = FileTypes.GetMediaFiles(Directory.GetFiles(dir)).ToList();
          
            if (OperatingSystem.IsWindows())
                files.Sort(new StringLogicalComparer());

            int index = files.IndexOf(path);
            files.Remove(path);

            foreach (string file in files)
                CommandV("loadfile", file, "append");

            if (index > 0)
                CommandV("playlist-move", "0", (index + 1).ToString());
        }
    }

    bool _wasAviSynthLoaded;

    [SupportedOSPlatform("windows")]
    void LoadAviSynth()
    {
        if (!_wasAviSynthLoaded)
        {
            string? dll = Environment.GetEnvironmentVariable("AviSynthDLL");  // StaxRip sets it in portable mode
            LoadLibrary(File.Exists(dll) ? dll : "AviSynth.dll");
            _wasAviSynthLoaded = true;
        }
    }

    [DllImport("kernel32.dll")]
    public static extern IntPtr LoadLibrary(string path);

    [SupportedOSPlatform("windows")]
    public static string GetShortcutTarget(string path)
    {
        Type? t = Type.GetTypeFromProgID("WScript.Shell");
        dynamic? sh = Activator.CreateInstance(t!);
        return sh?.CreateShortcut(path).TargetPath!;
    }

    static string GetLanguage(string id)
    {
        foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
            if (ci.ThreeLetterISOLanguageName == id || Convert(ci.ThreeLetterISOLanguageName) == id)
                return ci.EnglishName;

        return id;

        static string Convert(string id2) => id2 switch
        {
            "bng" => "ben",
            "ces" => "cze",
            "deu" => "ger",
            "ell" => "gre",
            "eus" => "baq",
            "fra" => "fre",
            "hye" => "arm",
            "isl" => "ice",
            "kat" => "geo",
            "mya" => "bur",
            "nld" => "dut",
            "sqi" => "alb",
            "zho" => "chi",
            _ => id2,
        };
    }

    static string GetNativeLanguage(string name)
    {
        foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
            if (ci.EnglishName == name)
                return ci.NativeName;

        return name;
    }

    public void RaiseScaleWindow(float value) => ScaleWindow?.Invoke(value);

    public void RaiseMoveWindow(string value) => MoveWindow?.Invoke(value);
    
    public void RaiseWindowScaleNET(float value) => WindowScaleNET?.Invoke(value);
    
    public void RaiseShowMenu() => ShowMenu?.Invoke();

    public void UpdateTracks()
    {
        string path = GetPropertyString("path");

        if (!path.ToLowerEx().StartsWithEx("bd://"))
            lock (BluRayTitles)
                BluRayTitles.Clear();

        lock (MediaTracksLock)
            MediaTracks = GetTracks();
    }

    public List<Chapter> GetChapters() {
        List<Chapter> chapters = new List<Chapter>();
        int count = GetPropertyInt("chapter-list/count");

        for (int x = 0; x < count; x++)
        {
            string title = GetPropertyString($"chapter-list/{x}/title");
            double time = GetPropertyDouble($"chapter-list/{x}/time");

            if (string.IsNullOrEmpty(title) ||
                (title.Length == 12 && title.Contains(':') && title.Contains(".")))

                title = "Chapter " + (x + 1);

            chapters.Add(new Chapter() { Title = title, Time = time });
        }

        return chapters;
    }

    public void UpdateExternalTracks()
    { 
        int trackListTrackCount = GetPropertyInt("track-list/count");
        int editionCount = GetPropertyInt("edition-list/count");
        int count = MediaTracks.Where(i => i.Type != "g").Count();

        lock (MediaTracksLock)
        {
            if (count != (trackListTrackCount + editionCount))
            {
                MediaTracks = MediaTracks.Where(i => !i.External).ToList();
                MediaTracks.AddRange(GetTracks(false));
            }
        }
    }

    public List<MediaTrack> GetTracks(bool includeInternal = true, bool includeExternal = true)
    {
        List<MediaTrack> tracks = new List<MediaTrack>();

        int trackCount = GetPropertyInt("track-list/count");

        for (int i = 0; i < trackCount; i++)
        {
            bool external = GetPropertyBool($"track-list/{i}/external");

            if ((external && !includeExternal) || (!external && !includeInternal))
                continue;

            string type = GetPropertyString($"track-list/{i}/type");
            string filename = GetPropertyString($"filename/no-ext");
            string title = GetPropertyString($"track-list/{i}/title").Replace(filename, "");

            title = Regex.Replace(title, @"^[\._\-]", "");

            if (type == "video")
            {
                string codec = GetPropertyString($"track-list/{i}/codec").ToUpperEx();
                if (codec == "MPEG2VIDEO")
                    codec = "MPEG2";
                else if (codec == "DVVIDEO")
                    codec = "DV";
                MediaTrack track = new MediaTrack();
                Add(track, codec);
                Add(track, GetPropertyString($"track-list/{i}/demux-w") + "x" + GetPropertyString($"track-list/{i}/demux-h"));
                Add(track, GetPropertyString($"track-list/{i}/demux-fps").Replace(".000000", "") + " FPS");
                Add(track, GetPropertyBool($"track-list/{i}/default") ? "Default" : null);
                track.Text = "V: " + track.Text.Trim(' ', ',');
                track.Type = "v";
                track.ID = GetPropertyInt($"track-list/{i}/id");
                tracks.Add(track);
            }
            else if (type == "audio")
            {
                string codec = GetPropertyString($"track-list/{i}/codec").ToUpperEx();
                if (codec.Contains("PCM"))
                    codec = "PCM";
                MediaTrack track = new MediaTrack();
                Add(track, GetLanguage(GetPropertyString($"track-list/{i}/lang")));
                Add(track, codec);
                Add(track, GetPropertyInt($"track-list/{i}/audio-channels") + " ch");
                Add(track, GetPropertyInt($"track-list/{i}/demux-samplerate") / 1000 + " kHz");
                Add(track, GetPropertyBool($"track-list/{i}/forced") ? "Forced" : null);
                Add(track, GetPropertyBool($"track-list/{i}/default") ? "Default" : null);
                Add(track, GetPropertyBool($"track-list/{i}/external") ? "External" : null);
                Add(track, title);
                track.Text = "A: " + track.Text.Trim(' ', ',');
                track.Type = "a";
                track.ID = GetPropertyInt($"track-list/{i}/id");
                track.External = external;
                tracks.Add(track);
            }
            else if (type == "sub")
            {
                string codec = GetPropertyString($"track-list/{i}/codec").ToUpperEx();
                if (codec.Contains("PGS"))
                    codec = "PGS";
                else if (codec == "SUBRIP")
                    codec = "SRT";
                else if (codec == "WEBVTT")
                    codec = "VTT";
                else if (codec == "DVB_SUBTITLE")
                    codec = "DVB";
                else if (codec == "DVD_SUBTITLE")
                    codec = "VOB";
                MediaTrack track = new MediaTrack();
                Add(track, GetLanguage(GetPropertyString($"track-list/{i}/lang")));
                Add(track, codec);
                Add(track, GetPropertyBool($"track-list/{i}/forced") ? "Forced" : null);
                Add(track, GetPropertyBool($"track-list/{i}/default") ? "Default" : null);
                Add(track, GetPropertyBool($"track-list/{i}/external") ? "External" : null);
                Add(track, title);
                track.Text = "S: " + track.Text.Trim(' ', ',');
                track.Type = "s";
                track.ID = GetPropertyInt($"track-list/{i}/id");
                track.External = external;
                tracks.Add(track);
            }
        }

        if (includeInternal)
        {
            int editionCount = GetPropertyInt("edition-list/count");

            for (int i = 0; i < editionCount; i++)
            {
                string title = GetPropertyString($"edition-list/{i}/title");
                if (string.IsNullOrEmpty(title))
                    title = "Edition " + i;
                MediaTrack track = new MediaTrack();
                track.Text = "E: " + title;
                track.Type = "e";
                track.ID = i;
                tracks.Add(track);
            }
        }

        return tracks;

        static void Add(MediaTrack track, object? value)
        {
            string str = (value + "").Trim();

            if (str != "" && !track.Text.Contains(str))
                track.Text += " " + str + ",";
        }
    }

    string[]? _profileNames;

    public string[] ProfileNames
    {
        get
        {
            if (_profileNames != null)
                return _profileNames;

            string[] ignore = { "builtin-pseudo-gui", "encoding", "libmpv", "pseudo-gui", "default" };
            string json = GetPropertyString("profile-list");
            return _profileNames = JsonDocument.Parse(json).RootElement.EnumerateArray()
                .Select(it => it.GetProperty("name").GetString())
                .Where(it => !ignore.Contains(it)).ToArray()!;
        }
    }

    public string GetProfiles()
    {
        string json = GetPropertyString("profile-list");
        StringBuilder sb = new StringBuilder();

        foreach (var profile in JsonDocument.Parse(json).RootElement.EnumerateArray())
        {
            sb.Append(profile.GetProperty("name").GetString() + BR2);

            foreach (var it in profile.GetProperty("options").EnumerateArray())
                sb.AppendLine($"    {it.GetProperty("key").GetString()} = {it.GetProperty("value").GetString()}");

            sb.Append(BR);
        }

        return sb.ToString();
    }

    public string GetDecoders()
    {
        var list = JsonDocument.Parse(GetPropertyString("decoder-list")).RootElement.EnumerateArray()
            .Select(it => $"{it.GetProperty("codec").GetString()} - {it.GetProperty("description").GetString()}")
            .OrderBy(it => it);

        return string.Join(BR, list);
    }

    public string GetProtocols() => string.Join(BR, GetPropertyString("protocol-list").Split(',').OrderBy(i => i));

    public string GetDemuxers() => string.Join(BR, GetPropertyString("demuxer-lavf-list").Split(',').OrderBy(i => i));
}
