
using CommunityToolkit.Mvvm.ComponentModel;

namespace mpvgui.WinFormsWPF.WPF.ViewModel;

public class ViewModelBase : ObservableObject
{
    public Theme Theme => Theme.Current!;
}
