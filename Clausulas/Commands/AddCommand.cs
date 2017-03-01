using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Clausulas
{
    /// <summary>
    /// Comando para implementar la opción de añadir Anticipos
    /// </summary>
    public class AddCommand : ICommand
    {
        private AmortizacionesViewModel viewModel;

        public AddCommand(AmortizacionesViewModel viewModel)
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
            if (viewModel.ButtonAddContent == "Nuevo")
            {
                viewModel.addResult = new Anticipado();
                viewModel.addResult.Fecha = DateTime.Now.Date;

                viewModel.SelectedItem = viewModel.addResult;
                viewModel.ButtonAddContent = "Cancelar";
            }
            else
                viewModel.SelectedItem = viewModel.Collection.View.CurrentItem as Anticipado;
        }
    }
}
