using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace Clausulas
{
    /// <summary>
    /// Clase para crear la vista modelo de Revisiones
    /// </summary>
    public class RevisionesViewModel : INotifyPropertyChanged, IViewModel
    {

        #region Declaración de Propiedades

        public CollectionViewSource Collection { get; private set; }
        
        private Revision selectedItem;
        public Revision SelectedItem
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

        public CancelCommand<RevisionesViewModel> CancelEvent { get; private set; }
        public AcceptCommand<RevisionesViewModel> AcceptEvent { get; set; }

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

        public RevisionesViewModel()
        {
            Collection = new CollectionViewSource();
            if (Metodos.Revisiones == null)
            {
                Metodos.Revisiones = new List<Revision>();
            }
            Collection.Source = Metodos.Revisiones;
            SelectedItem = Collection.View.CurrentItem as Revision;

            AcceptEvent = new AcceptCommand<RevisionesViewModel>(this);
            CancelEvent = new CancelCommand<RevisionesViewModel>(this);
        }

        #endregion

        #region Métodos

        public void AcceptChanges()
        {
            // Valida los cambios de la revisión
            Metodos.CloseWindow<RevisionesViewModel>(true);
        }

        public void CancelChanges()
        {
            // Cancelar los cambios
            Metodos.CloseWindow<RevisionesViewModel>(false);
        }

        #endregion

    }
}
