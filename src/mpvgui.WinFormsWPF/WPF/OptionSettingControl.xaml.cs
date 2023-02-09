
using System.Windows;
using System.Windows.Controls;

using mpvgui.WinFormsWPF.Misc;

namespace mpvgui.WinFormsWPF.WPF;

public partial class OptionSettingControl : UserControl, ISettingControl
{
    OptionSetting OptionSetting;

    public OptionSettingControl(OptionSetting optionSetting)
    {
        OptionSetting = optionSetting;
        InitializeComponent();
        DataContext = this;
        TitleTextBox.Text = optionSetting.Name;

        if (string.IsNullOrEmpty(optionSetting.Help))
            HelpTextBox.Visibility = Visibility.Collapsed;

        HelpTextBox.Text = optionSetting.Help;
        ItemsControl.ItemsSource = optionSetting.Options;

        if (string.IsNullOrEmpty(optionSetting.URL))
            LinkTextBlock.Visibility = Visibility.Collapsed;

        Link.SetURL(optionSetting.URL);
    }

    public Theme? Theme => Theme.Current;

    string? _searchableText;

    public string SearchableText {
        get {
            if (_searchableText == null)
            {
                _searchableText = TitleTextBox.Text + HelpTextBox.Text;

                foreach (var i in OptionSetting.Options)
                    _searchableText += i.Text + i.Help + i.Name;

                _searchableText = _searchableText.ToLower();
            }

            return _searchableText;
        }
    }

    public SettingBase SettingBase => OptionSetting;
    public bool Contains(string searchString) => SearchableText.Contains(searchString.ToLower());
}
