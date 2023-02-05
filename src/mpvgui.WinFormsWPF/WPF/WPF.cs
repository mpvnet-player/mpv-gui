
using System.Windows;

namespace mpvgui;

public class WPF
{
    public static void Init()
    {
        if (Application.Current == null)
        {
            new Application();

            Application.Current.Resources.MergedDictionaries.Add(
                Application.LoadComponent(new Uri("mpvgui;component/WPF/Resources.xaml",
                    UriKind.Relative)) as ResourceDictionary);
        }
    }


}
