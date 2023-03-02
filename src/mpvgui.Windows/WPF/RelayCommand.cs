
using System.Windows.Input;

namespace mpvgui.Windows.WPF;

public class RelayCommand : ICommand
{
    public event EventHandler? CanExecuteChanged;

    Action<object> _executeAction;

    Predicate<object?>? _canExecutePredicate;

    public RelayCommand(Action<object> executeAction, Predicate<object?>? canExecutePredicate = null)
    {
        _executeAction = executeAction;
        _canExecutePredicate = canExecutePredicate;
    }

    public bool CanExecute(object? parameter) => _canExecutePredicate == null || _canExecutePredicate(parameter);

    public void Execute(object? parameter) => _executeAction(parameter!);

    public void OnCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
