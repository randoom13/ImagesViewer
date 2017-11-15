using System;
using System.Windows.Input;

namespace ImagesViewer.ViewModels
{
    public sealed class RelayCommand<T> : ICommand
    {
        private readonly Predicate<T> _canExecuteMethod;
        private readonly Action<T> _executeMethod;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute", "Execute delegate cannot be null");
            _executeMethod = execute;
            _canExecuteMethod = canExecute;
        }

        public RelayCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        public bool CanExecute(object parameter)
        {
            return _canExecuteMethod == null || _canExecuteMethod.Invoke((T)parameter);
        }

        public void Execute(object parameter)
        {
            _executeMethod.Invoke((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecuteMethod != null)
                    CommandManager.RequerySuggested += value;
            }
            remove
            {
                if (_canExecuteMethod != null)
                    CommandManager.RequerySuggested -= value;
            }
        }
    }
}
