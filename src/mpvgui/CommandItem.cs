
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace mpvgui;

public class CommandItem : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public string Path { get; set; } = "";
    public string Command { get; set; } = "";

    public string Display
    {
        get
        {
            if (string.IsNullOrEmpty(Path))
            {
                if (Command.Length > 47)
                    return Command[..47] + "...";

                return Command;
            }
            else
                return Path;
        }
    }

    public CommandItem() { }

    public CommandItem(SerializationInfo info, StreamingContext context) { }

    void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    string _Input = "";

    public string Input
    {
        get => _Input;
        set
        {
            _Input = value;
            NotifyPropertyChanged();
        }
    }

    public string Alias
    {
        get
        {
            if (Input.Contains("SHARP") || Input.Contains("sharp") || Input.Contains("Sharp"))
                return "#";

            return null;
        }
    }

    public static ObservableCollection<CommandItem> GetItems(string content)
    {
        var items = new ObservableCollection<CommandItem>();

        if (!string.IsNullOrEmpty(content))
        {
            foreach (string line in content.Split('\r', '\n'))
            {
                string val = line.Trim();

                if (val.StartsWith("#"))
                    continue;

                if (!val.Contains(" "))
                    continue;

                CommandItem item = new CommandItem();
                item.Input = val.Substring(0, val.IndexOf(" "));

                if (item.Input == "_")
                    item.Input = "";

                val = val.Substring(val.IndexOf(" ") + 1);

                if (val.Contains("#menu:"))
                {
                    item.Path = val.Substring(val.IndexOf("#menu:") + 6).Trim();
                    val = val.Substring(0, val.IndexOf("#menu:"));

                    if (item.Path.Contains(";"))
                        item.Path = item.Path.Substring(item.Path.IndexOf(";") + 1).Trim();
                }

                item.Command = val.Trim();

                if (item.Command == "")
                    continue;

                if (item.Command.ToLower() == "ignore")
                    item.Command = "";

                items.Add(item);
            }
        }
        return items;
    }

    static ObservableCollection<CommandItem> _Items;

    public static ObservableCollection<CommandItem> Items
    {
        get => _Items ??= GetItems(File.ReadAllText(Player.InputConfPath));
    }
}
