
using System.Security.Cryptography;
using System.Text;

namespace mpvgui;

public static class StringHelp
{
    public static string GetMD5Hash(string txt)
    {
        using MD5 md5 = MD5.Create();
        byte[] inputBuffer = Encoding.UTF8.GetBytes(txt);
        byte[] hashBuffer = md5.ComputeHash(inputBuffer);
        return BitConverter.ToString(md5.ComputeHash(inputBuffer)).Replace("-", "");
    }
}

public static class FileHelp
{
    public static void Delete(string path)
    {
        try {
            if (File.Exists(path))
                File.Delete(path);
        } catch (Exception ex) {
            Terminal.WriteError("Failed to delete file:" + BR + path + BR + ex.Message);
        }
    }
}

public static class ProcessHelp
{
    public static void Execute(string file, string arguments = "", bool shellExecute = false)
    {
        using Process proc = new Process();
        proc.StartInfo.FileName = file;
        proc.StartInfo.Arguments = arguments;
        proc.StartInfo.UseShellExecute = shellExecute;
        proc.Start();
    }

    public static void ShellExecute(string file, string arguments = "") => Execute(file, arguments, true);
}
