using System;
using System.Windows.Input;

namespace BsfEditor
{
    public class RelayCommand : RelayCommand<object>
    {
        #region Constructors
        public RelayCommand(Action<object> execute) : base(execute)
        {
        }

        public RelayCommand(Action execute) : base(o => execute())
        {
        }

        public RelayCommand(Action execute, Func<bool> canExecute) : base(o => execute(), o => canExecute())
        {
        }
        #endregion
    }

    public class RelayCommand<T> : ICommand
    {
        #region Fields
        private readonly Func<T, bool> _canExecute;
        private readonly Action<T> _execute;
        #endregion

        #region Events
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        #endregion

        #region Constructors
        public RelayCommand(Action<T> execute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute) : this(execute)
        {
            _canExecute = canExecute;
        }
        #endregion

        #region Interface Implementations
        public bool CanExecute(object parameter)
        {
            if (_canExecute == null) return true;
            if (typeof(T).IsEnum && parameter == null)
            {
                //apparently CommandParameters aren't always filled even if they are defined in XAML and not-null in DataBinding, this is an issue for enums.
                return false;
            }
            return _canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }
        #endregion
    }
}
