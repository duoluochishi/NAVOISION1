using System;
using System.Windows.Input;

namespace NV.CT.Service.UI.Util
{
    public class DelegateCommand : ICommand
    {
        Action _action;
        Action<object> _actionWithParameter;
        Func<bool> _canAction;
        public DelegateCommand(Action action)
        {
            _action = action;
        }

        public DelegateCommand(Action<object> action)
        {
            _actionWithParameter = action;
        }

        public DelegateCommand(Action action, Func<bool> canAction)
        {
            _action = action;
            _canAction = canAction;
        }

        public DelegateCommand(Action<object> action, Func<bool> canAction)
        {
            _actionWithParameter = action;
            _canAction = canAction;
        }

        public bool CanExecute(object parameter)
        {
            return (_canAction == null) ? true : _canAction.Invoke();
        }
        public void Execute(object parameter)
        {
            _action?.Invoke();
            _actionWithParameter?.Invoke(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
