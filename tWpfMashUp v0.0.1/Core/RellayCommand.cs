using System;
using System.Windows.Input;

namespace tWpfMashUp_v0._0._1.Core
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> execute;
        private readonly Func<object, bool> canExecute;
        public bool IsEnabled { get; set; }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.canExecute = canExecute;
            this.execute = execute;
            IsEnabled = true;
        }

        public void Execute(object parameter) => execute(parameter);

        public bool CanExecute(object parameter) => canExecute == null || canExecute(parameter);
    }
}
