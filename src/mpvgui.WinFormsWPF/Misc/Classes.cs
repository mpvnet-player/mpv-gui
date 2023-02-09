
using System.Windows;

namespace mpvgui.WinFormsWPF.Misc;

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

public class StringPair
{
    public string? Name { get; set; }
    public string? Value { get; set; }
}

public class Conf
{
    public static List<SettingBase> LoadConf(string content)
    {
        List<SettingBase> settingsList = new List<SettingBase>();

        foreach (ConfSection? section in ConfParser.Parse(content))
        {
            SettingBase? baseSetting = null;

            if (section.HasName("option"))
            {
                OptionSetting optionSetting = new OptionSetting();
                baseSetting = optionSetting;
                optionSetting.Default = section.GetValue("default");
                optionSetting.Value = optionSetting.Default;

                foreach (var it in section.GetValues("option"))
                {
                    var opt = new OptionSettingOption();

                    if (it.Value.ContainsEx(" "))
                    {
                        opt.Name = it.Value![..it.Value!.IndexOf(" ")];
                        opt.Help = it.Value[it.Value.IndexOf(" ")..].Trim();
                    }
                    else
                        opt.Name = it.Value;

                    if (opt.Name == optionSetting.Default)
                        opt.Text = opt.Name + " (Default)";

                    opt.OptionSetting = optionSetting;
                    optionSetting.Options.Add(opt);
                }
            }
            else
            {
                StringSetting stringSetting = new StringSetting();
                baseSetting = stringSetting;
                stringSetting.Default = section.HasName("default") ? section.GetValue("default") : "";
            }

            baseSetting.Name = section.GetValue("name");
            baseSetting.File = section.GetValue("file");
            baseSetting.Filter = section.GetValue("filter");

            if (section.HasName("help")) baseSetting.Help = section.GetValue("help");
            if (section.HasName("url")) baseSetting.URL = section.GetValue("url");
            if (section.HasName("width")) baseSetting.Width = Convert.ToInt32(section.GetValue("width"));
            if (section.HasName("type")) baseSetting.Type = section.GetValue("type");

            if (baseSetting.Help.ContainsEx("\\n"))
                baseSetting.Help = baseSetting.Help?.Replace("\\n", "\n");

            settingsList.Add(baseSetting);
        }

        return settingsList;
    }
}

public class ConfItem
{
    public string Comment { get; set; } = "";
    public string File { get; set; } = "";
    public string LineComment { get; set; } = "";
    public string Name { get; set; } = "";
    public string Section { get; set; } = "";
    public string Value { get; set; } = "";

    public bool IsSectionItem { get; set; }
    public SettingBase? SettingBase { get; set; }
}

public class ConfParser
{
    public static List<ConfSection> Parse(string content)
    {
        string[] lines = content.Split(BR.ToCharArray(), StringSplitOptions.None);
        var sections = new List<ConfSection>();
        ConfSection? currentGroup = null;

        foreach (string it in lines)
        {
            string line = it.Trim();

            if (line == "")
            {
                currentGroup = new ConfSection();
                sections.Add(currentGroup);
            }
            else if (line.Contains("="))
            {
                string name = line.Substring(0, line.IndexOf("=")).Trim();
                string value = line.Substring(line.IndexOf("=") + 1).Trim();

                currentGroup?.Items.Add(new StringPair() { Name = name, Value = value });
            }
        }

        return sections;
    }
}

public class ConfSection
{
    public List<StringPair> Items { get; set; } = new List<StringPair>();

    public bool HasName(string name)
    {
        foreach (var i in Items)
            if (i.Name == name)
                return true;

        return false;
    }

    public string? GetValue(string name)
    {
        foreach (var i in Items)
            if (i.Name == name)
                return i.Value;

        return null;
    }

    public List<StringPair> GetValues(string name) => Items.Where(i => i.Name == name).ToList();
}

public abstract class SettingBase
{
    public string? Default { get; set; }
    public string? File { get; set; }
    public string? Filter { get; set; }
    public string? Help { get; set; }
    public string? Name { get; set; }
    public string? StartValue { get; set; }
    public string? Type { get; set; }
    public string? URL { get; set; }
    public string? Value { get; set; }

    public int Width { get; set; }
    public ConfItem? ConfItem { get; set; }
}

public class StringSetting : SettingBase
{
}

public class OptionSetting : SettingBase
{
    public List<OptionSettingOption> Options { get; } = new List<OptionSettingOption>();
}

public class OptionSettingOption
{
    public string? Name { get; set; }
    public string? Help { get; set; }

    public OptionSetting? OptionSetting { get; set; }

    string? _Text;

    public string? Text
    {
        get => string.IsNullOrEmpty(_Text) ? Name : _Text;
        set => _Text = value;
    }

    public bool Checked
    {
        get => OptionSetting?.Value == Name;
        set
        {
            if (value)
                OptionSetting!.Value = Name;
        }
    }

    public Visibility Visibility
    {
        get => string.IsNullOrEmpty(Help) ? Visibility.Collapsed : Visibility.Visible;
    }
}
