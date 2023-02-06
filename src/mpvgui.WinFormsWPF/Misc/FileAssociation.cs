
#nullable enable

using Microsoft.Win32;

using mpvgui.Help;

namespace mpvgui.Misc;

public static class FileAssociation
{
    public static void Register(string perceivedType, string[] extensions)
    {
        string exePath = AppInfo.ProcessPath;
        string exeFilename = Path.GetFileName(AppInfo.ProcessPath);
        string exeFilenameNoExt = Path.GetFileNameWithoutExtension(AppInfo.ProcessPath);

        string[] protocols = { "ytdl", "rtsp", "srt", "srtp" };

        if (perceivedType != "unreg")
        {
            foreach (string it in protocols)
            {
                RegistryHelp.SetValue($@"HKCR\{it}", $"{it.ToUpper()} Protocol", "");
                RegistryHelp.SetValue($@"HKCR\{it}\shell\open\command", "", $"\"{exePath}\" \"%1\"");
            }

            RegistryHelp.SetValue(@"HKCU\Software\Microsoft\Windows\CurrentVersion\App Paths\" + exeFilename, "", exePath);
            RegistryHelp.SetValue(@"HKCR\Applications\" + exeFilename, "FriendlyAppName", "mpv-gui media player");
            RegistryHelp.SetValue(@"HKCR\Applications\" + exeFilename + @"\shell\open\command", "", $"\"{exePath}\" \"%1\"");
            RegistryHelp.SetValue(@"HKCR\SystemFileAssociations\video\OpenWithList\" + exeFilename, "", "");
            RegistryHelp.SetValue(@"HKCR\SystemFileAssociations\audio\OpenWithList\" + exeFilename, "", "");
            RegistryHelp.SetValue(@"HKLM\SOFTWARE\RegisteredApplications", "mpv-gui", @"SOFTWARE\Clients\Media\mpv-gui\Capabilities");
            RegistryHelp.SetValue(@"HKLM\SOFTWARE\Clients\Media\mpv-gui\Capabilities", "ApplicationDescription", "mpv-gui media player");
            RegistryHelp.SetValue(@"HKLM\SOFTWARE\Clients\Media\mpv-gui\Capabilities", "ApplicationName", "mpv-gui");

            foreach (string ext in extensions)
            {
                RegistryHelp.SetValue(@"HKCR\Applications\" + exeFilename + @"\SupportedTypes", "." + ext, "");
                RegistryHelp.SetValue(@"HKCR\" + "." + ext, "", exeFilenameNoExt + "." + ext);
                RegistryHelp.SetValue(@"HKCR\" + "." + ext + @"\OpenWithProgIDs", exeFilenameNoExt + "." + ext, "");
                RegistryHelp.SetValue(@"HKCR\" + "." + ext, "PerceivedType", perceivedType);
                RegistryHelp.SetValue(@"HKCR\" + exeFilenameNoExt + "." + ext + @"\shell\open\command", "", $"\"{exePath}\" \"%1\"");
                RegistryHelp.SetValue(@"HKLM\SOFTWARE\Clients\Media\mpv-gui\Capabilities\FileAssociations", "." + ext, exeFilenameNoExt + "." + ext);
            }
        }
        else
        {
            foreach (string i in protocols)
                RegistryHelp.RemoveKey($@"HKCR\{i}");

            RegistryHelp.RemoveKey(@"HKCU\Software\Microsoft\Windows\CurrentVersion\App Paths\" + exeFilename);
            RegistryHelp.RemoveKey(@"HKCR\Applications\" + exeFilename);
            RegistryHelp.RemoveKey(@"HKLM\SOFTWARE\Clients\Media\mpv-gui");
            RegistryHelp.RemoveKey(@"HKCR\SystemFileAssociations\video\OpenWithList\" + exeFilename);
            RegistryHelp.RemoveKey(@"HKCR\SystemFileAssociations\audio\OpenWithList\" + exeFilename);

            RegistryHelp.RemoveValue(@"HKLM\SOFTWARE\RegisteredApplications", "mpv-gui");

            foreach (string id in Registry.ClassesRoot.GetSubKeyNames())
            {
                if (id.StartsWith(exeFilenameNoExt + "."))
                    Registry.ClassesRoot.DeleteSubKeyTree(id);

                RegistryHelp.RemoveValue($@"HKCR\Software\Classes\{id}\OpenWithProgIDs", exeFilenameNoExt + id);
                RegistryHelp.RemoveValue($@"HKLM\Software\Classes\{id}\OpenWithProgIDs", exeFilenameNoExt + id);
            }
        }
    }
}
