using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Tholi.Desktop.ViewModels;

public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void Changed([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public sealed class RelayCommand(Action execute, Func<bool>? canExecute = null) : ICommand
{
    public bool CanExecute(object? parameter) => canExecute?.Invoke() ?? true;
    public void Execute(object? parameter) { if (CanExecute(parameter)) execute(); }
    public event EventHandler? CanExecuteChanged;
    public void Refresh() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
