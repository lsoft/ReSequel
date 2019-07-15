using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfHelpers
{
    public class AsyncRelayCommand<TParameter> : ICommand
        where TParameter : class
    {
        private readonly Func<TParameter, Task> _execute;
        private readonly Func<TParameter, bool> _canExecute;

        private long _isExecuting;

        public AsyncRelayCommand(
            Func<TParameter, Task> execute,
            Func<TParameter, bool> canExecute = null
        )
        {

            this._execute = execute;
            this._canExecute = canExecute ?? (o => true);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        public bool CanExecute(object parameter)
        {
            if (Interlocked.Read(ref _isExecuting) != 0)
            {
                return false;
            }

            return _canExecute(parameter as TParameter);
        }

        public async void Execute(object parameter)
        {
            Interlocked.Exchange(ref _isExecuting, 1);
            RaiseCanExecuteChanged();

            try
            {
                await _execute(parameter as TParameter);
            }
            finally
            {
                Interlocked.Exchange(ref _isExecuting, 0);
                RaiseCanExecuteChanged();
            }
        }
    }
}