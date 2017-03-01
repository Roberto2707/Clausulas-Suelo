using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Clausulas
{
    /// <summary>
    /// Comando para el botón Aceptar de las pantallas que lo implementan
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AcceptCommand<T> : ICommand
        where T : IViewModel
    {
        private T viewModel;

        public AcceptCommand(T viewModel)
        {
            this.viewModel = viewModel;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.AcceptChanges();
        }
    }
}
