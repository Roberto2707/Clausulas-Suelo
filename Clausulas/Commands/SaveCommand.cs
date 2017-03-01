using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Clausulas
{
    /// <summary>
    /// Comando para implementar el botón para añadir amortizaciones anticipadas
    /// </summary>
    public class SaveCommand : ICommand
    {
        private AmortizacionesViewModel viewModel;

        public SaveCommand(AmortizacionesViewModel viewModel)
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
            viewModel.Save();
        }
    }
}
