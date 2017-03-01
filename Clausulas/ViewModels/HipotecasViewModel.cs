using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Clausulas
{
    /// <summary>
    /// Clase para crear la vista modelo de Hipotecas
    /// </summary>
    public class HipotecasViewModel : INotifyPropertyChanged, IViewModel
    {

        #region Declaración de Propiedades

        public CollectionViewSource Collection { get; private set; }

        private Hipoteca selectedItem;
        public Hipoteca SelectedItem
        {
            get { return selectedItem; }
            set
            {
                selectedItem = value;
                if (selectedItem != null)
                {
                    RaisePropertyChanged("SelectedItem");
                }
            }
        }

        #endregion

        #region Commands

        public AcceptCommand<HipotecasViewModel> AcceptEvent { get; set; }
        public CancelCommand<HipotecasViewModel> CancelEvent { get; private set; }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Constructor de la Clase

        public HipotecasViewModel()
        {
            Collection = new CollectionViewSource();
            Collection.Source = Metodos.Hipotecas;
            SelectedItem = Collection.View.CurrentItem as Hipoteca;

            AcceptEvent = new AcceptCommand<HipotecasViewModel>(this);
            CancelEvent = new CancelCommand<HipotecasViewModel>(this);
        }

        #endregion

        #region Métodos

        public void AcceptChanges()
        {
            // Valida los cambios de la revisión
            Metodos.IdHipoteca = selectedItem.Id;
            Metodos.CloseWindow<HipotecasViewModel>(true);
        }

        public void CancelChanges()
        {
            // Cancelar los cambios
            Metodos.IdHipoteca = 0;
            Metodos.CloseWindow<HipotecasViewModel>(false);
        }

        #endregion

    }
}
