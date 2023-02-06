
#nullable enable

namespace mpvgui.Misc;

public class Misc
{
    public static void CopyMpvnetCom()
    {
        string dir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).AddSep() +
            "Microsoft\\WindowsApps\\";

        if (File.Exists(dir + "mpvgui.exe") && !File.Exists(dir + "mpvgui.com"))
            File.Copy(Folder.Startup + "mpvgui.com", dir + "mpvgui.com");
    }
}
