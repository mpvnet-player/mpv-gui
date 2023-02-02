
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
    public static void Execute(string file, string arguments = null)
    {
        using Process proc = new Process();
        proc.StartInfo.FileName = file;
        proc.StartInfo.Arguments = arguments;
        proc.Start();
    }

    public static void ShellExecute(string file, string arguments = null)
    {
        using Process proc = new Process();
        proc.StartInfo.FileName = file;
        proc.StartInfo.Arguments = arguments;
        proc.StartInfo.UseShellExecute = true;
        proc.Start();
    }
}

public class mpvHelp
{
    public static string GetProfiles()
    {
        string json = Player.GetPropertyString("profile-list");
        var o = json.FromJson<List<Dictionary<string, object>>>().OrderBy(i => i["name"]);
        StringBuilder sb = new StringBuilder();

        foreach (Dictionary<string, object> i in o)
        {
            sb.Append(i["name"].ToString() + BR2);

            foreach (Dictionary<string, object> i2 in i["options"] as List<object>)
                sb.AppendLine("   " + i2["key"] + " = " + i2["value"]);

            sb.Append(BR);
        }

        return sb.ToString();
    }

    public static string GetDecoders()
    {
        string json = Player.GetPropertyString("decoder-list");
        var o = json.FromJson<List<Dictionary<string, object>>>().OrderBy(i => i["codec"]);
        StringBuilder sb = new StringBuilder();

        foreach (Dictionary<string, object> i in o)
            sb.AppendLine(i["codec"] + " - " + i["description"]);

        return sb.ToString();
    }

    public static string GetProtocols()
    {
        string list = Player.GetPropertyString("protocol-list");
        return string.Join(BR, list.Split(',').OrderBy(a => a));
    }

    public static string GetDemuxers()
    {
        string list = Player.GetPropertyString("demuxer-lavf-list");
        return string.Join(BR, list.Split(',').OrderBy(a => a));
    }
}
