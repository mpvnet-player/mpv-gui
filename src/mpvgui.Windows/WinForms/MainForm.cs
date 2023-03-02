﻿
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;

using MsgBoxEx;

using WpfControls = System.Windows.Controls;

using mpvgui.Windows.WPF;
using mpvgui.Windows.Misc;

using static mpvgui.Windows.Native.WinApi;

namespace mpvgui.Windows.WinForms;

public partial class MainForm : Form
{
    public SnapManager SnapManager = new SnapManager();
    public IntPtr MpvWindowHandle { get; set; }
    public Dictionary<string, WpfControls.MenuItem> MenuItemDuplicate = new Dictionary<string, WpfControls.MenuItem>();

    public static Form? Instance;
    WpfControls.ContextMenu? ContextMenu { get; set; }
    AutoResetEvent MenuAutoResetEvent { get; } = new AutoResetEvent(false);
    Point _lastCursorPosition;
    Taskbar? _taskbar;

    int _lastCursorChanged;
    int _lastCycleFullscreen;
    int _taskbarButtonCreatedMessage;

    bool _wasMaximized;

    public MainForm()
    {
        InitializeComponent();

        try
        {
            Instance = this;

            Player.FileLoaded += Player_FileLoaded;
            Player.MoveWindow += Player_MoveWindow;
            Player.Pause += Player_Pause;
            Player.PlaylistPosChanged += Player_PlaylistPosChanged;
            Player.ScaleWindow += Player_ScaleWindow;
            Player.Seek += UpdateProgressBar;
            Player.ShowMenu += Player_ShowMenu;
            Player.Shutdown += Player_Shutdown;
            Player.VideoSizeChanged += Player_VideoSizeChanged;
            Player.WindowScaleMpv += Player_WindowScaleMpv;
            Player.WindowScaleNET += Player_WindowScaleNET;
            Player.ClientMessage += Player_ClientMessage;

            if (Player.GPUAPI != "vulkan")
                Init();

            AppDomain.CurrentDomain.UnhandledException += (sender, e) => Terminal.WriteError(e.ExceptionObject);
            Application.ThreadException += (sender, e) => Terminal.WriteError(e.Exception);

            _taskbarButtonCreatedMessage = RegisterWindowMessage("TaskbarButtonCreated");

            if (Player.Screen > -1)
            {
                int targetIndex = Player.Screen;
                Screen[] screens = Screen.AllScreens;

                if (targetIndex < 0)
                    targetIndex = 0;

                if (targetIndex > screens.Length - 1)
                    targetIndex = screens.Length - 1;

                Screen screen = screens[Array.IndexOf(screens, screens[targetIndex])];
                Rectangle target = screen.Bounds;
                Left = target.X + (target.Width - Width) / 2;
                Top = target.Y + (target.Height - Height) / 2;
            }

            if (!Player.Border)
                FormBorderStyle = FormBorderStyle.None;

            Point pos = App.Settings.WindowPosition;

            if ((pos.X != 0 || pos.Y != 0) && App.RememberWindowPosition)
            {
                Left = pos.X - Width / 2;
                Top = pos.Y - Height / 2;

                Point location = App.Settings.WindowLocation;

                if (location.X == -1) Left = pos.X;
                if (location.X ==  1) Left = pos.X - Width;
                if (location.Y == -1) Top = pos.Y;
                if (location.Y ==  1) Top = pos.Y - Height;
            }

            if (Player.WindowMaximized)
            {
                SetFormPosAndSize(true);
                WindowState = FormWindowState.Maximized;
            }

            if (Player.WindowMinimized)
            {
                SetFormPosAndSize(true);
                WindowState = FormWindowState.Minimized;
            }
        }
        catch (Exception ex)
        {
            Msg.ShowException(ex);
        }
    }

    void Player_ClientMessage(string key, ArraySegment<string> args)
    {
        if (!Command.Commands.ContainsKey(key))
            return;

        BeginInvoke(() => Command.Commands[key].Invoke(args.ToArray()));
    }

    void Player_MoveWindow(string direction)
    {
        BeginInvoke(() => {
            Screen screen = Screen.FromControl(this);
            Rectangle workingArea = GetWorkingArea(Handle, screen.WorkingArea);

            switch (direction)
            {
                case "left":
                    Left = workingArea.Left;
                    break;
                case "top":
                    Top = 0;
                    break;
                case "right":
                    Left = workingArea.Width - Width + workingArea.Left;
                    break;
                case "bottom":
                    Top = workingArea.Height - Height;
                    break;
                case "center":
                    Left = (screen.Bounds.Width - Width) / 2;
                    Top = (screen.Bounds.Height - Height) / 2;
                    break;
            }
        });
    }

    void Player_PlaylistPosChanged(int pos)
    {
        if (pos == -1)
            SetTitle();
    }

    void Init()
    {
        Player.Init(Handle);

        // bool methods not working correctly
        Player.ObserveProperty("window-maximized", PropChangeWindowMaximized);
        Player.ObserveProperty("window-minimized", PropChangeWindowMinimized);

        Player.ObservePropertyBool("border", PropChangeBorder);
        Player.ObservePropertyBool("fullscreen", PropChangeFullscreen);
        Player.ObservePropertyBool("keepaspect-window", value => Player.KeepaspectWindow = value);
        Player.ObservePropertyBool("ontop", PropChangeOnTop);

        Player.ObservePropertyString("sid", PropChangeSid);
        Player.ObservePropertyString("aid", PropChangeAid);
        Player.ObservePropertyString("vid", PropChangeVid);

        Player.ObservePropertyString("title", PropChangeTitle);

        Player.ObservePropertyInt("edition", PropChangeEdition);

        Player.ProcessCommandLine(false);
    }

    void Player_ShowMenu()
    {
        BeginInvoke(() => {
            if (IsMouseInOsc())
                return;

            ShowCursor();
            UpdateMenu();
            ContextMenu!.IsOpen = true;
        });
    }

    void Player_ScaleWindow(float scale) {
        BeginInvoke(() => {
            int w, h;

            if (KeepSize())
            {
                w = (int)(ClientSize.Width * scale);
                h = (int)(ClientSize.Height * scale);
            }
            else
            {
                w = (int)(ClientSize.Width * scale);
                h = (int)Math.Ceiling(w * Player.VideoSize.Height / (double)Player.VideoSize.Width);
            }

            SetSize(w, h, Screen.FromControl(this), false);
        });
    }

    void Player_WindowScaleNET(float scale)
    {
        BeginInvoke(() => {
            SetSize(
                (int)(Player.VideoSize.Width * scale),
                (int)Math.Ceiling(Player.VideoSize.Height * scale),
                Screen.FromControl(this), false);
            Player.Command($"show-text \"window-scale {scale.ToString(CultureInfo.InvariantCulture)}\"");
        });
    }

    void Player_WindowScaleMpv(double scale)
    {
        if (!Player.Shown)
            return;

        BeginInvoke(() => {
            SetSize(
                (int)(Player.VideoSize.Width * scale),
                (int)Math.Ceiling(Player.VideoSize.Height * scale),
                Screen.FromControl(this), false);
        });
    }

    void Player_Shutdown() => BeginInvoke(Close);
    
    void Player_VideoSizeChanged(Size value) => BeginInvoke(() =>
    {
        if (!KeepSize())
            SetFormPosAndSize();
    });

    void PropChangeFullscreen(bool value) => BeginInvoke(() => CycleFullscreen(value));

    bool IsFullscreen => WindowState == FormWindowState.Maximized && FormBorderStyle == FormBorderStyle.None;

    bool KeepSize() => App.StartSize == "session" || App.StartSize == "always";

    bool IsMouseInOsc()
    {
        Point pos = PointToClient(MousePosition);
        float top = 0;

        if (!Player.Border)
            top = ClientSize.Height * 0.1f;

        return pos.Y > ClientSize.Height * 0.78 || pos.Y < top;
    }

    void UpdateMenu()
    {
        Player.UpdateExternalTracks();

        lock (Player.MediaTracksLock)
        {
            var trackMenuItem = FindMenuItem("Track");

            if (trackMenuItem != null)
            {
                trackMenuItem.Items.Clear();

                var audTracks = Player.MediaTracks.Where(track => track.Type == "a");
                var subTracks = Player.MediaTracks.Where(track => track.Type == "s");
                var vidTracks = Player.MediaTracks.Where(track => track.Type == "v");
                var ediTracks = Player.MediaTracks.Where(track => track.Type == "e");

                foreach (MediaTrack track in vidTracks)
                {
                    var mi = new WpfControls.MenuItem() { Header = track.Text.Replace("_", "__") };
                    mi.Click += (sender, args) => Player.CommandV("set", "vid", track.ID.ToString());
                    mi.IsChecked = Player.VID == track.ID.ToString();
                    trackMenuItem.Items.Add(mi);
                }

                if (vidTracks.Any())
                    trackMenuItem.Items.Add(new WpfControls.Separator());

                foreach (MediaTrack track in audTracks)
                {
                    var mi = new WpfControls.MenuItem() { Header = track.Text.Replace("_", "__") };
                    mi.Click += (sender, args) => Player.CommandV("set", "aid", track.ID.ToString());
                    mi.IsChecked = Player.AID == track.ID.ToString();
                    trackMenuItem.Items.Add(mi);
                }

                if (subTracks.Any())
                    trackMenuItem.Items.Add(new WpfControls.Separator());

                foreach (MediaTrack track in subTracks)
                {
                    var mi = new WpfControls.MenuItem() { Header = track.Text.Replace("_", "__") };
                    mi.Click += (sender, args) => Player.CommandV("set", "sid", track.ID.ToString());
                    mi.IsChecked = Player.SID == track.ID.ToString();
                    trackMenuItem.Items.Add(mi);
                }

                if (subTracks.Any())
                {
                    var mi = new WpfControls.MenuItem() { Header = "S: No subtitles" };
                    mi.Click += (sender, args) => Player.CommandV("set", "sid", "no");
                    mi.IsChecked = Player.SID == "no";
                    trackMenuItem.Items.Add(mi);
                }

                if (ediTracks.Any())
                    trackMenuItem.Items.Add(new WpfControls.Separator());

                foreach (MediaTrack track in ediTracks)
                {
                    var mi = new WpfControls.MenuItem() { Header = track.Text.Replace("_", "__") };
                    mi.Click += (sender, args) => Player.CommandV("set", "edition", track.ID.ToString());
                    mi.IsChecked = Player.Edition == track.ID;
                    trackMenuItem.Items.Add(mi);
                }
            }
        }

        var chaptersMenuItem = FindMenuItem("Chapters");

        if (chaptersMenuItem != null)
        {
            chaptersMenuItem.Items.Clear();

            foreach (Chapter chapter in Player.GetChapters())
            {
                var mi = new WpfControls.MenuItem
                {
                    Header = chapter.Title,
                    InputGestureText = chapter.TimeDisplay
                };
                
                mi.Click += (sender, args) => Player.CommandV("seek", chapter.Time.ToString(CultureInfo.InvariantCulture), "absolute");
                chaptersMenuItem.Items.Add(mi);
            }
        }

        var recentMenuItem = FindMenuItem("Recent");

        if (recentMenuItem != null)
        {
            recentMenuItem.Items.Clear();

            foreach (string path in App.Settings.RecentFiles)
            {
                var file = App.GetTitleAndPath(path);
                var mi = MenuHelp.Add(recentMenuItem.Items, file.Title.ShortPath(100));

                if (mi != null)
                    mi.Click += (sender, args) =>
                        Player.LoadFiles(new[] { file.Path }, true, false);
            }

            recentMenuItem.Items.Add(new WpfControls.Separator());
            var clearMenuItem = new WpfControls.MenuItem() { Header = "Clear List" };
            clearMenuItem.Click += (sender, args) => App.Settings.RecentFiles.Clear();
            recentMenuItem.Items.Add(clearMenuItem);
        }

        var titlesMenuItem = FindMenuItem("Titles");

        if (titlesMenuItem != null)
        {
            titlesMenuItem.Items.Clear();

            lock (Player.BluRayTitles)
            {
                List<(int Index, TimeSpan Length)> items = new List<(int, TimeSpan)>(); 

                for (int i = 0; i < Player.BluRayTitles.Count; i++)
                    items.Add((i, Player.BluRayTitles[i]));

                var titleItems = items.OrderByDescending(item => item.Length)
                                      .Take(20)
                                      .OrderBy(item => item.Index);

                foreach (var item in titleItems)
                {
                    if (item.Length != TimeSpan.Zero)
                    {
                        var mi = MenuHelp.Add(titlesMenuItem.Items, $"Title {item.Index + 1}");

                        if (mi != null)
                        {
                            mi.InputGestureText = item.Length.ToString();
                            mi.Click += (sender, args) => Player.SetBluRayTitle(item.Index);
                        }
                    }
                }
            }
        }

        var profilesMenuItem = FindMenuItem("Profile");

        if (profilesMenuItem != null)
        {
            profilesMenuItem.Items.Clear();

            foreach (string profile in Player.ProfileNames)
            {
                if (!profile.StartsWith("extension."))
                {
                    var mi = MenuHelp.Add(profilesMenuItem.Items, profile);

                    if (mi != null)
                    {
                        mi.Click += (sender, args) =>
                        {
                            Player.CommandV("show-text", profile);
                            Player.CommandV("apply-profile", profile);
                        };
                    }
                }
            }
        }
    }

    public WpfControls.MenuItem? FindMenuItem(string text) => FindMenuItem(text, ContextMenu?.Items);

    WpfControls.MenuItem? FindMenuItem(string text, WpfControls.ItemCollection? items)
    {
        foreach (object item in items!)
        {
            if (item is WpfControls.MenuItem mi)
            {
                if (mi.Header.ToString().StartsWithEx(text) && mi.Header.ToString().TrimEx() == text)
                    return mi;

                if (mi.Items.Count > 0)
                {
                    WpfControls.MenuItem? val = FindMenuItem(text, mi.Items);

                    if (val != null)
                        return val;
                }
            }
        }

        return null;
    }

    void SetFormPosAndSize(bool force = false, bool checkAutofit = true)
    {
        if (!force)
        {
            if (WindowState != FormWindowState.Normal)
                return;

            if (Player.Fullscreen)
            {
                CycleFullscreen(true);
                return;
            }
        }

        Screen screen = Screen.FromControl(this);
        Rectangle workingArea = GetWorkingArea(Handle, screen.WorkingArea);
        int autoFitHeight = Convert.ToInt32(workingArea.Height * Player.Autofit);

        if (App.AutofitAudio > 1)
            App.AutofitAudio = 1;

        if (App.AutofitImage > 1)
            App.AutofitImage = 1;

        bool isAudio = FileTypes.IsAudio(Player.Path.Ext());
        
        if (isAudio)
            autoFitHeight = Convert.ToInt32(workingArea.Height * App.AutofitAudio);

        if (FileTypes.IsImage(Player.Path.Ext()))
            autoFitHeight = Convert.ToInt32(workingArea.Height * App.AutofitImage);

        if (Player.VideoSize.Height == 0 || Player.VideoSize.Width == 0)
            Player.VideoSize = new Size((int)(autoFitHeight * (16 / 9f)), autoFitHeight);

        float minAspectRatio = isAudio ? App.MinimumAspectRatioAudio : App.MinimumAspectRatio;

        if (minAspectRatio != 0 && Player.VideoSize.Width / (float)Player.VideoSize.Height < minAspectRatio)
            Player.VideoSize = new Size((int)(autoFitHeight * minAspectRatio), autoFitHeight);

        Size videoSize = Player.VideoSize;

        int height = videoSize.Height;
        int width  = videoSize.Width;

        if (App.StartSize == "previous")
            App.StartSize = "height-session";

        if (Player.WasInitialSizeSet)
        {
            if (KeepSize())
            {
                width = ClientSize.Width;
                height = ClientSize.Height;
            }
            else if (App.StartSize == "height-always" || App.StartSize == "height-session")
            {
                height = ClientSize.Height;
                width = height * videoSize.Width / videoSize.Height;
            }
            else if (App.StartSize == "width-always" || App.StartSize == "width-session")
            {
                width = ClientSize.Width;
                height = (int)Math.Ceiling(width * videoSize.Height / (double)videoSize.Width);
            }
        }
        else
        {
            Size windowSize = App.Settings.WindowSize;

            if (App.StartSize == "height-always" && windowSize.Height != 0)
            {
                height = windowSize.Height;
                width = height * videoSize.Width / videoSize.Height;
            }
            else if (App.StartSize == "height-session" || App.StartSize == "session")
            {
                height = autoFitHeight;
                width = height * videoSize.Width / videoSize.Height;
            }
            else if(App.StartSize == "width-always" && windowSize.Height != 0)
            {
                width = windowSize.Width;
                height = (int)Math.Ceiling(width * videoSize.Height / (double)videoSize.Width);
            }
            else if (App.StartSize == "width-session")
            {
                width = autoFitHeight / 9 * 16;
                height = (int)Math.Ceiling(width * videoSize.Height / (double)videoSize.Width);
            }
            else if (App.StartSize == "always" && windowSize.Height != 0)
            {
                height = windowSize.Height;
                width = windowSize.Width;
            }

            Player.WasInitialSizeSet = true;
        }

        SetSize(width, height, screen, checkAutofit);
    }

    void SetSize(int width, int height, Screen screen, bool checkAutofit = true)
    {
        Rectangle workingArea = GetWorkingArea(Handle, screen.WorkingArea);

        int maxHeight = workingArea.Height - (Height - ClientSize.Height) - 2;
        int maxWidth = workingArea.Width - (Width - ClientSize.Width);

        int startWidth = width;
        int startHeight = height;

        if (checkAutofit)
        {
            if (height < maxHeight * Player.AutofitSmaller)
            {
                height = Convert.ToInt32(maxHeight * Player.AutofitSmaller);
                width = Convert.ToInt32(height * startWidth / (double)startHeight);
            }

            if (height > maxHeight * Player.AutofitLarger)
            {
                height = Convert.ToInt32(maxHeight * Player.AutofitLarger);
                width = Convert.ToInt32(height * startWidth / (double)startHeight);
            }
        }

        if (width > maxWidth)
        {
            width = maxWidth;
            height = (int)Math.Ceiling(width * startHeight / (double)startWidth);
        }

        if (height > maxHeight)
        {
            height = maxHeight;
            width = Convert.ToInt32(height * startWidth / (double)startHeight);
        }

        if (height < maxHeight * 0.1)
        {
            height = Convert.ToInt32(maxHeight * 0.1);
            width = Convert.ToInt32(height * startWidth / (double)startHeight);
        }

        Point middlePos = new Point(Left + Width / 2, Top + Height / 2);
        var rect = new Rect(new Rectangle(screen.Bounds.X, screen.Bounds.Y, width, height));
        AddWindowBorders(Handle, ref rect, GetDpi(Handle));

        int left = middlePos.X - rect.Width / 2;
        int top = middlePos.Y - rect.Height / 2;

        Rectangle currentRect = new Rectangle(Left, Top, Width, Height);

        if (GetHorizontalLocation(screen) == -1) left = Left;
        if (GetHorizontalLocation(screen) ==  1) left = currentRect.Right - rect.Width;

        if (GetVerticalLocation(screen) == -1) top = Top;
        if (GetVerticalLocation(screen) ==  1) top = currentRect.Bottom - rect.Height;

        Screen[] screens = Screen.AllScreens;

        int minLeft   = screens.Select(val => GetWorkingArea(Handle, val.WorkingArea).X).Min();
        int maxRight  = screens.Select(val => GetWorkingArea(Handle, val.WorkingArea).Right).Max();
        int minTop    = screens.Select(val => GetWorkingArea(Handle, val.WorkingArea).Y).Min();
        int maxBottom = screens.Select(val => GetWorkingArea(Handle, val.WorkingArea).Bottom).Max();

        if (left < minLeft)
            left = minLeft;

        if (left + rect.Width > maxRight)
            left = maxRight - rect.Width;

        if (top < minTop)
            top = minTop;

        if (top + rect.Height > maxBottom)
            top = maxBottom - rect.Height;

        uint SWP_NOACTIVATE = 0x0010;
        SetWindowPos(Handle, IntPtr.Zero, left, top, rect.Width, rect.Height, SWP_NOACTIVATE);
    }

    public void CycleFullscreen(bool enabled)
    {
        _lastCycleFullscreen = Environment.TickCount;
        Player.Fullscreen = enabled;

        if (enabled)
        {
            if (WindowState != FormWindowState.Maximized || FormBorderStyle != FormBorderStyle.None)
            {
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;

                if (_wasMaximized)
                {
                    Rectangle bounds = Screen.FromControl(this).Bounds;
                    uint SWP_SHOWWINDOW = 0x0040;
                    IntPtr HWND_TOP= IntPtr.Zero;
                    SetWindowPos(Handle, HWND_TOP, bounds.X, bounds.Y, bounds.Width, bounds.Height, SWP_SHOWWINDOW);
                }
            }
        }
        else
        {
            if (WindowState == FormWindowState.Maximized && FormBorderStyle == FormBorderStyle.None)
            {
                if (_wasMaximized)
                    WindowState = FormWindowState.Maximized;
                else
                {
                    WindowState = FormWindowState.Normal;
                    
                    if (!Player.WasInitialSizeSet)
                        SetFormPosAndSize();
                }

                FormBorderStyle = Player.Border ? FormBorderStyle.Sizable : FormBorderStyle.None;

                if (!KeepSize())
                    SetFormPosAndSize();
            }
        }
    }

    public int GetHorizontalLocation(Screen screen)
    {
        Rectangle workingArea = GetWorkingArea(Handle, screen.WorkingArea);
        Rectangle rect = new Rectangle(Left - workingArea.X, Top - workingArea.Y, Width, Height);

        if (workingArea.Width / (float)Width < 1.1)
            return 0;

        if (rect.X * 3 < workingArea.Width - rect.Right)
            return -1;

        if (rect.X > (workingArea.Width - rect.Right) * 3)
            return 1;

        return 0;
    }

    public int GetVerticalLocation(Screen screen)
    {
        Rectangle workingArea = GetWorkingArea(Handle, screen.WorkingArea);
        Rectangle rect = new Rectangle(Left - workingArea.X, Top - workingArea.Y, Width, Height);

        if (workingArea.Height / (float)Height < 1.1)
            return 0;

        if (rect.Y * 3 < workingArea.Height - rect.Bottom)
            return -1;

        if (rect.Y > (workingArea.Height - rect.Bottom) * 3)
            return 1;

        return 0;
    }

    public void BuildMenu()
    {
        var items = CommandItem.GetItems(Player.InputConfContent);

        if (!Player.InputConfContent.Contains("#menu:"))
        {
            var defaultItems = CommandItem.GetItems(Properties.Resources.input_conf);

            foreach (CommandItem item in items)
                foreach (CommandItem defaultItem in defaultItems)
                    if (item.Command == defaultItem.Command)
                        defaultItem.Input = item.Input;

            items = defaultItems;
        }

        foreach (CommandItem item in items)
        {
            CommandItem tempItem = item;

            if (string.IsNullOrEmpty(tempItem.Path))
                continue;

            if (MenuItemDuplicate.ContainsKey(tempItem.Path))
            {
                var mi = MenuItemDuplicate[tempItem.Path];
                mi.InputGestureText = mi.InputGestureText + ", " + tempItem.Input;
            }
            else
            {
                var menuItem = MenuHelp.Add(ContextMenu?.Items, tempItem.Path);             

                if (menuItem != null)
                {
                    MenuItemDuplicate[tempItem.Path] = menuItem;
                    menuItem.Click += (sender, args) => {
                        try {
                            App.RunTask(() => {
                                MenuAutoResetEvent.WaitOne();
                                System.Windows.Application.Current.Dispatcher.Invoke(
                                    DispatcherPriority.Background, new Action(delegate { }));
                                if (!string.IsNullOrEmpty(tempItem.Command))
                                    Player.Command(tempItem.Command);                
                            });
                        }
                        catch (Exception ex) {
                            Msg.ShowException(ex);
                        }
                    };

                    menuItem.InputGestureText = tempItem.Input;
                }
            }
        }
    }

    void Player_FileLoaded()
    {
        BeginInvoke(() => {
            SetTitleInternal();

            int interval = (int)(Player.Duration.TotalMilliseconds / 100);

            if (interval < 100)
                interval = 100;

            if (interval > 1000)
                interval = 1000;

            ProgressTimer.Interval = interval;
            UpdateProgressBar();
        });

        string path = Player.GetPropertyString("path");

        path = PlayerClass.ConvertFilePath(path);

        if (path.Contains("://"))
        {
            string title = Player.GetPropertyString("media-title");

            if (!string.IsNullOrEmpty(title) && path != title)
                path = path + "|" + title;
        }

        if (!string.IsNullOrEmpty(path) && path != @"bd://" && path != @"dvd://")
        {
            if (App.Settings.RecentFiles.Contains(path))
                App.Settings.RecentFiles.Remove(path);

            App.Settings.RecentFiles.Insert(0, path);

            while (App.Settings.RecentFiles.Count > App.RecentCount)
                App.Settings.RecentFiles.RemoveAt(App.RecentCount);
        }
    }

    void SetTitle() => BeginInvoke(SetTitleInternal);

    void SetTitleInternal()
    {
        string? title = Title;

        if (title == "${filename}" && Player.Path.ContainsEx("://"))
            title = "${media-title}";

        string text = Player.Expand(title);

        if (text == "(unavailable)" || Player.PlaylistPos == -1)
            text = "mpv-gui";

        Text = text;
    }

    public void Voodoo()
    {
        Message m = new Message() { Msg = 0x0202 }; // WM_LBUTTONUP
        SendMessage(Handle, m.Msg, m.WParam, m.LParam);
    }

    void SaveWindowProperties()
    {
        if (WindowState == FormWindowState.Normal && Player.Shown)
        {
            SavePosition();
            App.Settings.WindowSize = ClientSize;
        }
    }

    void SavePosition()
    {
        Point pos = new Point(Left + Width / 2, Top + Height / 2);
        Screen screen = Screen.FromControl(this);

        int x = GetHorizontalLocation(screen);
        int y = GetVerticalLocation(screen);

        if (x == -1) pos.X = Left;
        if (x ==  1) pos.X = Left + Width;
        if (y == -1) pos.Y = Top;
        if (y ==  1) pos.Y = Top + Height;

        App.Settings.WindowPosition = pos;
        App.Settings.WindowLocation = new Point(x, y);
    }

    protected override CreateParams CreateParams {
        get {
            CreateParams cp = base.CreateParams;
            cp.Style |= 0x00020000 /* WS_MINIMIZEBOX */;
            return cp;
        }
    }

    string? _title;

    public string? Title {
        get => _title;
        set {
            if (string.IsNullOrEmpty(value))
                return;

            if (value.EndsWith("} - mpv"))
                value = value.Replace("} - mpv", "} - mpv-gui");

            _title = value;
        }
    }

    protected override void WndProc(ref Message m)
    {
        switch (m.Msg)
        {
            case 0x100: // WM_KEYDOWN
            case 0x101: // WM_KEYUP
            case 0x104: // WM_SYSKEYDOWN
            case 0x105: // WM_SYSKEYUP
            case 0x201: // WM_LBUTTONDOWN
            case 0x202: // WM_LBUTTONUP
            case 0x204: // WM_RBUTTONDOWN
            case 0x205: // WM_RBUTTONUP
            case 0x207: // WM_MBUTTONDOWN
            case 0x208: // WM_MBUTTONUP
            case 0x20a: // WM_MOUSEWHEEL
            case 0x20b: // WM_XBUTTONDOWN
            case 0x20c: // WM_XBUTTONUP
            case 0x20e: // WM_MOUSEHWHEEL
            case 0x2a3: // WM_MOUSELEAVE
                if (MpvWindowHandle == IntPtr.Zero)
                    MpvWindowHandle = FindWindowEx(Handle, IntPtr.Zero, "mpv", null);

                if (MpvWindowHandle != IntPtr.Zero)
                    m.Result = SendMessage(MpvWindowHandle, m.Msg, m.WParam, m.LParam);
                break;
            case 0x51: // WM_INPUTLANGCHANGE
                ActivateKeyboardLayout(m.LParam, 0x00000100u /*KLF_SETFORPROCESS*/);
                break;
            case 0x319: // WM_APPCOMMAND
                {
                    string? value = Input.WM_APPCOMMAND_to_mpv_key((int)(m.LParam.ToInt64() >> 16 & ~0xf000));

                    if (value != null)
                    {
                        Player.Command("keypress " + value);
                        m.Result = new IntPtr(1);
                        return;
                    }
                }
                break;
            case 0x312: // WM_HOTKEY
                GlobalHotkey.Execute(m.WParam.ToInt32());
                break;
            case 0x200: // WM_MOUSEMOVE
                if (Environment.TickCount - _lastCycleFullscreen > 500)
                {
                    Point pos = PointToClient(Cursor.Position);
                    Player.Command($"mouse {pos.X} {pos.Y}");
                }

                if (IsCursorPosDifferent(_lastCursorPosition))
                    ShowCursor();
                break;
            case 0x203: // WM_LBUTTONDBLCLK
                {
                    Point pos = PointToClient(Cursor.Position);
                    Player.Command($"mouse {pos.X} {pos.Y} 0 double");
                }
                break;
            case 0x2E0: // WM_DPICHANGED
                {
                    if (!Player.Shown)
                        break;

                    Rect rect = Marshal.PtrToStructure<Rect>(m.LParam);
                    SetWindowPos(Handle, IntPtr.Zero, rect.Left, rect.Top, rect.Width, rect.Height, 0);
                }
                break;
            case 0x214: // WM_SIZING
                if (Player.KeepaspectWindow)
                {
                    Rect rc = Marshal.PtrToStructure<Rect>(m.LParam);
                    Rect r = rc;
                    SubtractWindowBorders(Handle, ref r, GetDpi(Handle));

                    int c_w = r.Right - r.Left, c_h = r.Bottom - r.Top;
                    Size videoSize = Player.VideoSize;

                    if (videoSize == Size.Empty)
                        videoSize = new Size(16, 9);

                    float aspect = videoSize.Width / (float)videoSize.Height;
                    int d_w = (int)(c_h * aspect - c_w);
                    int d_h = (int)(c_w / aspect - c_h);

                    int[] d_corners = { d_w, d_h, -d_w, -d_h };
                    int[] corners = { rc.Left, rc.Top, rc.Right, rc.Bottom };
                    int corner = GetResizeBorder(m.WParam.ToInt32());

                    if (corner >= 0)
                        corners[corner] -= d_corners[corner];

                    Marshal.StructureToPtr(new Rect(corners[0], corners[1], corners[2], corners[3]), m.LParam, false);
                    m.Result = new IntPtr(1);
                }
                return;
            case 0x4A: // WM_COPYDATA
                {
                    var copyData = (CopyDataStruct)m.GetLParam(typeof(CopyDataStruct))!;
                    string[] args = copyData.lpData.Split('\n');
                    string mode = args[0];
                    args = args.Skip(1).ToArray();

                    switch (mode)
                    {
                        case "single":
                            Player.LoadFiles(args, true, false);
                            break;
                        case "queue":
                            foreach (string file in args)
                                Player.CommandV("loadfile", file, "append");
                            break;
                        case "command":
                            Player.Command(args[0]);
                            break;
                    }

                    Activate();
                }
                return;
            case 0x84: // WM_NCHITTEST
                // resize borderless window
                if (!Player.Border && !Player.Fullscreen) {
                    const int HTCLIENT = 1;
                    const int HTLEFT = 10;
                    const int HTRIGHT = 11;
                    const int HTTOP = 12;
                    const int HTTOPLEFT = 13;
                    const int HTTOPRIGHT = 14;
                    const int HTBOTTOM = 15;
                    const int HTBOTTOMLEFT = 16;
                    const int HTBOTTOMRIGHT = 17;

                    int x = (short)(m.LParam.ToInt32() & 0xFFFF); // LoWord
                    int y = (short)(m.LParam.ToInt32() >> 16);    // HiWord

                    Point pt = PointToClient(new Point(x, y));
                    Size cs = ClientSize;
                    m.Result = HTCLIENT;
                    int distance = FontHeight / 3;

                    if (pt.X >= cs.Width - distance && pt.Y >= cs.Height - distance && cs.Height >= distance)
                        m.Result = HTBOTTOMRIGHT;
                    else if (pt.X <= distance && pt.Y >= cs.Height - distance && cs.Height >= distance)
                        m.Result = HTBOTTOMLEFT;
                    else if (pt.X <= distance && pt.Y <= distance && cs.Height >= distance)
                        m.Result = HTTOPLEFT;
                    else if (pt.X >= cs.Width - distance && pt.Y <= distance && cs.Height >= distance)
                        m.Result = HTTOPRIGHT;
                    else if (pt.Y <= distance && cs.Height >= distance)
                        m.Result = HTTOP;
                    else if (pt.Y >= cs.Height - distance && cs.Height >= distance)
                        m.Result = HTBOTTOM;
                    else if (pt.X <= distance && cs.Height >= distance)
                        m.Result = HTLEFT;
                    else if (pt.X >= cs.Width - distance && cs.Height >= distance)
                        m.Result = HTRIGHT;

                    return;
                }
                break;
            case 0x231: // WM_ENTERSIZEMOVE
            case 0x005: // WM_SIZE
                if (Player.SnapWindow)
                    SnapManager.OnSizeAndEnterSizeMove(this);
                break;
            case 0x216: // WM_MOVING
                if (Player.SnapWindow)
                    SnapManager.OnMoving(ref m);
                break;
        }

        if (m.Msg == _taskbarButtonCreatedMessage && Player.TaskbarProgress)
        {
            _taskbar = new Taskbar(Handle);
            ProgressTimer.Start();
        }

        // beep sound when closed using taskbar due to exception
        if (!IsDisposed)
            base.WndProc(ref m);
    }

    void CursorTimer_Tick(object sender, EventArgs e)
    {
        if (IsCursorPosDifferent(_lastCursorPosition))
        {
            _lastCursorPosition = MousePosition;
            _lastCursorChanged = Environment.TickCount;
        }
        else if (((Environment.TickCount - _lastCursorChanged > 1500 &&
            !IsMouseInOsc()) || Environment.TickCount - _lastCursorChanged > 5000) &&
            ClientRectangle.Contains(PointToClient(MousePosition)) &&
            ActiveForm == this && !ContextMenu!.IsVisible)

            HideCursor();
    }

    void ProgressTimer_Tick(object sender, EventArgs e) => UpdateProgressBar();

    void UpdateProgressBar()
    {
        if (Player.TaskbarProgress && _taskbar != null)
            _taskbar.SetValue(Player.GetPropertyDouble("time-pos", false), Player.Duration.TotalSeconds);
    }

    void PropChangeOnTop(bool value) => BeginInvoke(() => TopMost = value);

    void PropChangeAid(string value) => Player.AID = value;

    void PropChangeSid(string value) => Player.SID = value;

    void PropChangeVid(string value) => Player.VID = value;

    void PropChangeTitle(string value) { Title = value; SetTitle(); }
    
    void PropChangeEdition(int value) => Player.Edition = value;
    
    void PropChangeWindowMaximized()
    {
        if (!Player.Shown)
            return;

        BeginInvoke(() =>
        {
            Player.WindowMaximized = Player.GetPropertyBool("window-maximized");

            if (Player.WindowMaximized && WindowState != FormWindowState.Maximized)
                WindowState = FormWindowState.Maximized;
            else if (!Player.WindowMaximized && WindowState == FormWindowState.Maximized)
                WindowState = FormWindowState.Normal;
        });
    }

    void PropChangeWindowMinimized()
    {
        if (!Player.Shown)
            return;

        BeginInvoke(() =>
        {
            Player.WindowMinimized = Player.GetPropertyBool("window-minimized");

            if (Player.WindowMinimized && WindowState != FormWindowState.Minimized)
                WindowState = FormWindowState.Minimized;
            else if (!Player.WindowMinimized && WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Normal;
        });
    }

    void PropChangeBorder(bool enabled) {
        Player.Border = enabled;

        BeginInvoke(() => {
            if (!IsFullscreen)
            {
                if (Player.Border && FormBorderStyle == FormBorderStyle.None)
                    FormBorderStyle = FormBorderStyle.Sizable;

                if (!Player.Border && FormBorderStyle == FormBorderStyle.Sizable)
                    FormBorderStyle = FormBorderStyle.None;
            }
        });
    }

    void Player_Pause()
    {
        if (_taskbar != null && Player.TaskbarProgress)
            _taskbar.SetState(Player.Paused ? TaskbarStates.Paused : TaskbarStates.Normal);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (Player.GPUAPI != "vulkan")
            Player.VideoSizeAutoResetEvent.WaitOne(App.StartThreshold);
        _lastCycleFullscreen = Environment.TickCount;
        SetFormPosAndSize();
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        Voodoo();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        if (Player.GPUAPI == "vulkan")
            Init();

        if (WindowState == FormWindowState.Maximized)
            Player.SetPropertyBool("window-maximized", true);

        ApplicationHelp.Init();
        Theme.UpdateWpfColors();
        MessageBoxEx.MessageForeground = Theme.Current?.GetBrush("heading");
        MessageBoxEx.MessageBackground = Theme.Current?.GetBrush("background");
        MessageBoxEx.ButtonBackground  = Theme.Current?.GetBrush("highlight");
        ContextMenu = new WpfControls.ContextMenu();
        ContextMenu.Closed += ContextMenu_Closed;
        ContextMenu.UseLayoutRounding = true;
        BuildMenu();
        System.Windows.Application.Current.ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;
        Cursor.Position = new Point(Cursor.Position.X + 1, Cursor.Position.Y);
        GlobalHotkey.RegisterGlobalHotkeys(Handle);
        App.RunTask(Misc.Misc.CopyMpvnetCom);
        Player.Shown = true;
    }

    void ContextMenu_Closed(object sender, System.Windows.RoutedEventArgs e) => MenuAutoResetEvent.Set();

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        SaveWindowProperties();
        
        if (FormBorderStyle != FormBorderStyle.None)
        {
            if (WindowState == FormWindowState.Maximized)
                _wasMaximized = true;
            else if (WindowState == FormWindowState.Normal)
                _wasMaximized = false;
        }

        if (Player.Shown)
        {
            if (WindowState == FormWindowState.Minimized)
                Player.SetPropertyBool("window-minimized", true);
            else if (WindowState == FormWindowState.Normal)
            {
                Player.SetPropertyBool("window-maximized", false);
                Player.SetPropertyBool("window-minimized", false);
            }
            else if (WindowState == FormWindowState.Maximized)
                Player.SetPropertyBool("window-maximized", true);
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        if (Player.IsQuitNeeded)
            Player.CommandV("quit");

        if (!Player.ShutdownAutoResetEvent.WaitOne(10000))
            Msg.ShowError("Shutdown thread failed to complete within 10 seconds.");

        Player.Destroy();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (WindowState == FormWindowState.Normal &&
            e.Button == MouseButtons.Left && !IsMouseInOsc())
        {
            var HTCAPTION = new IntPtr(2);
            ReleaseCapture();
            PostMessage(Handle, 0xA1 /* WM_NCLBUTTONDOWN */, HTCAPTION, IntPtr.Zero);
        }

        if (Width - e.Location.X < 10 && e.Location.Y < 10)
            Player.CommandV("quit");
    }

    protected override void OnMove(EventArgs e)
    {
        base.OnMove(e);
        SaveWindowProperties();
    }

    protected override void OnDragEnter(DragEventArgs e)
    {
        base.OnDragEnter(e);

        if (e.Data!.GetDataPresent(DataFormats.FileDrop) || e.Data.GetDataPresent(DataFormats.Text))
            e.Effect = DragDropEffects.Copy;
    }

    protected override void OnDragDrop(DragEventArgs e)
    {
        base.OnDragDrop(e);

        if (e.Data!.GetDataPresent(DataFormats.FileDrop))
            Player.LoadFiles(e.Data.GetData(DataFormats.FileDrop) as string[], true, false);
        else if (e.Data.GetDataPresent(DataFormats.Text))
            Player.LoadFiles(new[] { e.Data.GetData(DataFormats.Text)!.ToString() }, true, false);
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        ShowCursor();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        // prevent annoying beep using alt key
        if (ModifierKeys == Keys.Alt)
            e.SuppressKeyPress = true;

        base.OnKeyDown(e);
    }

    static bool _isCursorVisible = true;

    static void ShowCursor()
    {
        if (!_isCursorVisible)
        {
            Cursor.Show();
            _isCursorVisible = true;
        }
    }

    static void HideCursor()
    {
        if (_isCursorVisible)
        {
            Cursor.Hide();
            _isCursorVisible = false;
        }
    }

    static bool IsCursorPosDifferent(Point screenPos) =>
        Math.Abs(screenPos.X - MousePosition.X) > 10 || Math.Abs(screenPos.Y - MousePosition.Y) > 10;

    static int GetDpi(IntPtr hwnd)
    {
        if (Environment.OSVersion.Version >= WindowsTen1607 && hwnd != IntPtr.Zero)
            return GetDpiForWindow(hwnd);
        else
            using (Graphics gx = Graphics.FromHwnd(hwnd))
                return GetDeviceCaps(gx.GetHdc(), 88 /*LOGPIXELSX*/);
    }
}
