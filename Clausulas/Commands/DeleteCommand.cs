using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Clausulas
{
    /// <summary>
    /// Comando para implementar la opción de eliminar amortizaciones anticipadas
    /// </summary>
    public class DeleteCommand : ICommand
    {
        private AmortizacionesViewModel viewModel;

        public DeleteCommand(AmortizacionesViewModel viewModel)
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
            Anticipado deleteResult = viewModel.SelectedItem;
            if (deleteResult != null)
            {
                Metodos.Anticipos.Remove(deleteResult);
                viewModel.Refresh();
            }
        }
    }
}
