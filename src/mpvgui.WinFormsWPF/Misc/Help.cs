
using System.Windows.Forms;

using Microsoft.Win32;

public class RegistryHelp
{
    public static string ApplicationKey { get; } = @"HKCU\Software\" + Application.ProductName;

    public static void SetInt(string name, object value)
    {
        SetValue(ApplicationKey, name, value);
    }

    public static void SetString(string name, string value)
    {
        SetValue(ApplicationKey, name, value);
    }

    public static void SetValue(string name, object value)
    {
        using (RegistryKey regKey = GetRootKey(ApplicationKey).CreateSubKey(ApplicationKey.Substring(5), RegistryKeyPermissionCheck.ReadWriteSubTree))
            regKey.SetValue(name, value);
    }

    public static void SetValue(string path, string name, object value)
    {
        using (RegistryKey regKey = GetRootKey(path).CreateSubKey(path.Substring(5), RegistryKeyPermissionCheck.ReadWriteSubTree))
            regKey.SetValue(name, value);
    }

    public static string GetString(string name, string defaultValue = "")
    {
        object value = GetValue(ApplicationKey, name, defaultValue);
        return !(value is string) ? defaultValue : value.ToString();
    }

    public static int GetInt(string name, int defaultValue = 0)
    {
        object value = GetValue(ApplicationKey, name, defaultValue);
        return !(value is int) ? defaultValue : (int)value;
    }

    public static object GetValue(string name) => GetValue(ApplicationKey, name, null);

    public static object GetValue(string path, string name, object defaultValue = null)
    {
        using (RegistryKey regKey = GetRootKey(path).OpenSubKey(path.Substring(5)))
            return regKey == null ? null : regKey.GetValue(name, defaultValue);
    }

    public static void RemoveKey(string path)
    {
        try
        {
            GetRootKey(path).DeleteSubKeyTree(path.Substring(5), false);
        }
        catch { }
    }

    public static void RemoveValue(string path, string name)
    {
        try
        {
            using (RegistryKey regKey = GetRootKey(path).OpenSubKey(path.Substring(5), true))
                if (regKey != null)
                    regKey.DeleteValue(name, false);
        }
        catch { }
    }

    static RegistryKey GetRootKey(string path)
    {
        switch (path.Substring(0, 4))
        {
            case "HKLM": return Registry.LocalMachine;
            case "HKCU": return Registry.CurrentUser;
            case "HKCR": return Registry.ClassesRoot;
            default: throw new Exception();
        }
    }
}

public class FileAssociation
{
    static string ExePath = App.ExecutablePath;
    static string ExeFilename = Path.GetFileName(App.ExecutablePath);
    static string ExeFilenameNoExt = Path.GetFileNameWithoutExtension(App.ExecutablePath);

    public static void Register(string perceivedType, string[] extensions)
    {
        string[] protocols = { "ytdl", "rtsp", "srt", "srtp" };

        if (perceivedType != "unreg")
        {
            foreach (string i in protocols)
            {
                RegistryHelp.SetValue($@"HKCR\{i}", $"{i.ToUpper()} Protocol", "");
                RegistryHelp.SetValue($@"HKCR\{i}\shell\open\command", null, $"\"{ExePath}\" \"%1\"");
            }

            RegistryHelp.SetValue(@"HKCU\Software\Microsoft\Windows\CurrentVersion\App Paths\" + ExeFilename, null, ExePath);
            RegistryHelp.SetValue(@"HKCR\Applications\" + ExeFilename, "FriendlyAppName", "mpv-gui media player");
            RegistryHelp.SetValue(@"HKCR\Applications\" + ExeFilename + @"\shell\open\command", null, $"\"{ExePath}\" \"%1\"");
            RegistryHelp.SetValue(@"HKCR\SystemFileAssociations\video\OpenWithList\" + ExeFilename, null, "");
            RegistryHelp.SetValue(@"HKCR\SystemFileAssociations\audio\OpenWithList\" + ExeFilename, null, "");
            RegistryHelp.SetValue(@"HKLM\SOFTWARE\RegisteredApplications", "mpv-gui", @"SOFTWARE\Clients\Media\mpv-gui\Capabilities");
            RegistryHelp.SetValue(@"HKLM\SOFTWARE\Clients\Media\mpv-gui\Capabilities", "ApplicationDescription", "mpv-gui media player");
            RegistryHelp.SetValue(@"HKLM\SOFTWARE\Clients\Media\mpv-gui\Capabilities", "ApplicationName", "mpv-gui");

            foreach (string ext in extensions)
            {
                RegistryHelp.SetValue(@"HKCR\Applications\" + ExeFilename + @"\SupportedTypes", "." + ext, "");
                RegistryHelp.SetValue(@"HKCR\" + "." + ext, null, ExeFilenameNoExt + "." + ext);
                RegistryHelp.SetValue(@"HKCR\" + "." + ext + @"\OpenWithProgIDs", ExeFilenameNoExt + "." + ext, "");
                RegistryHelp.SetValue(@"HKCR\" + "." + ext, "PerceivedType", perceivedType);
                RegistryHelp.SetValue(@"HKCR\" + ExeFilenameNoExt + "." + ext + @"\shell\open\command", null, $"\"{ExePath}\" \"%1\"");
                RegistryHelp.SetValue(@"HKLM\SOFTWARE\Clients\Media\mpv-gui\Capabilities\FileAssociations", "." + ext, ExeFilenameNoExt + "." + ext);
            }
        }
        else
        {
            foreach (string i in protocols)
                RegistryHelp.RemoveKey($@"HKCR\{i}");

            RegistryHelp.RemoveKey(@"HKCU\Software\Microsoft\Windows\CurrentVersion\App Paths\" + ExeFilename);
            RegistryHelp.RemoveKey(@"HKCR\Applications\" + ExeFilename);
            RegistryHelp.RemoveKey(@"HKLM\SOFTWARE\Clients\Media\mpv-gui");
            RegistryHelp.RemoveKey(@"HKCR\SystemFileAssociations\video\OpenWithList\" + ExeFilename);
            RegistryHelp.RemoveKey(@"HKCR\SystemFileAssociations\audio\OpenWithList\" + ExeFilename);

            RegistryHelp.RemoveValue(@"HKLM\SOFTWARE\RegisteredApplications", "mpv-gui");

            foreach (string id in Registry.ClassesRoot.GetSubKeyNames())
            {
                if (id.StartsWith(ExeFilenameNoExt + "."))
                    Registry.ClassesRoot.DeleteSubKeyTree(id);

                RegistryHelp.RemoveValue($@"HKCR\Software\Classes\{id}\OpenWithProgIDs", ExeFilenameNoExt + id);
                RegistryHelp.RemoveValue($@"HKLM\Software\Classes\{id}\OpenWithProgIDs", ExeFilenameNoExt + id);
            }
        }
    }
}
