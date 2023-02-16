
namespace mpvgui.WinFormsWPF.WPF.ViewModel;

public class ViewModelBase : ObservableObject
{
    public Theme Theme => Theme.Current!;
}

public class AboutViewModel : ViewModelBase
{
    public RelayCommand CloseCommand { get; } = new (window => ((IWindow)window).Close());
    public string About { get; } = App.About;
}
