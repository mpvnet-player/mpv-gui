
using System.Windows.Documents;
using System.Windows.Navigation;

using mpvgui.WinFormsWPF.Misc;

namespace mpvgui.WinFormsWPF.WPF;

interface ISettingControl
{
    bool Contains(string searchString);
    SettingBase SettingBase { get; }
}

public class HyperlinkEx : Hyperlink
{
    void HyperLinkEx_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        ProcessHelp.ShellExecute(e.Uri.AbsoluteUri);
    }

    public void SetURL(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return;

        NavigateUri = new Uri(url);
        RequestNavigate += HyperLinkEx_RequestNavigate;
        Inlines.Clear();
        Inlines.Add(url);
    }
}
