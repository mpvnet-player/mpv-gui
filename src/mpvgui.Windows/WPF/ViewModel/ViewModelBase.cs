
using CommunityToolkit.Mvvm.ComponentModel;

namespace mpvgui.Windows.WPF.ViewModel;

public class ViewModelBase : ObservableObject
{
    public Theme Theme => Theme.Current!;
}
