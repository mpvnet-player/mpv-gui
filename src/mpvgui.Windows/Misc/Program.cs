
using System.Windows.Forms;
using System.Threading;

using mpvgui.Windows.Native;

namespace mpvgui.Windows;

static class Program
{
    [STAThread]
    static void Main()
    {
        try
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (App.IsTerminalAttached)
                WinApi.AttachConsole(-1 /*ATTACH_PARENT_PROCESS*/);

            string[] args = Environment.GetCommandLineArgs().Skip(1).ToArray();

            if (args.Length > 0 && args[0] == "--register-file-associations")
            {
                Misc.FileAssociation.Register(args[1], args.Skip(1).ToArray());
                return;
            }

            App.Init();
            Theme.Init();
            Mutex mutex = new Mutex(true, StringHelp.GetMD5Hash(App.ConfPath), out bool isFirst);

            if (Control.ModifierKeys.HasFlag(Keys.Shift))
                App.ProcessInstance = "multi";

            if ((App.ProcessInstance == "single" || App.ProcessInstance == "queue") && !isFirst)
            {
                List<string> args2 = new List<string>();
                args2.Add(App.ProcessInstance);

                foreach (string arg in args)
                {
                    if (!arg.StartsWith("--") && (arg == "-" || arg.Contains("://") ||
                        arg.Contains(":\\") || arg.StartsWith("\\\\")))

                        args2.Add(arg);
                    else if (arg == "--queue")
                        args2[0] = "queue";
                    else if (arg.StartsWith("--command="))
                    {
                        args2[0] = "command";
                        args2.Add(arg.Substring(10));
                    }
                }

                Process[] procs = Process.GetProcessesByName("mpvgui");

                for (int i = 0; i < 20; i++)
                {
                    foreach (Process proc in procs)
                    {
                        if (proc.MainWindowHandle != IntPtr.Zero)
                        {
                            WinApi.AllowSetForegroundWindow(proc.Id);
                            var data = new WinApi.CopyDataStruct();
                            data.lpData = string.Join("\n", args2.ToArray());
                            data.cbData = data.lpData.Length * 2 + 1;
                            WinApi.SendMessage(proc.MainWindowHandle, 0x004A /*WM_COPYDATA*/, IntPtr.Zero, ref data);
                            mutex.Dispose();

                            if (App.IsTerminalAttached)
                                WinApi.FreeConsole();

                            return;
                        }
                    }

                    Thread.Sleep(50);
                }

                mutex.Dispose();
                return;
            }

            Application.Run(new WinForms.MainForm());

            if (App.IsTerminalAttached)
                WinApi.FreeConsole();

            mutex.Dispose();
        }
        catch (Exception ex)
        {
            Terminal.WriteError(ex);
        }
    }
}
