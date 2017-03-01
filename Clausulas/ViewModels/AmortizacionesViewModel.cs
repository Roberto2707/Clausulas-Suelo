using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace Clausulas
{
    /// <summary>
    /// Clase para crear la vista modelo de amortizaciones
    /// </summary>
    public class AmortizacionesViewModel : INotifyPropertyChanged, IViewModel
    {

        #region Declaración de Propiedades

        public CollectionViewSource Collection { get; private set; }

        public Anticipado addResult;

        private Anticipado selectedItem;
        public Anticipado SelectedItem
        {
            get { return selectedItem; }
            set
            {
                selectedItem = value;
                if (selectedItem != null)
                {
                    RaisePropertyChanged("SelectedItem");
                }
                ButtonAddContent = "Nuevo";
            }
        }

        private string buttonAddContent;
        public string ButtonAddContent
        {
            get
            {
                return buttonAddContent;
            }

            set
            {
                buttonAddContent = value;
                if (buttonAddContent != null)
                {
                    RaisePropertyChanged("ButtonAddContent");
                }
            }
        }

        #endregion

        #region Commands

        public CancelCommand<AmortizacionesViewModel> CancelEvent { get; private set; }
        public AcceptCommand<AmortizacionesViewModel> AcceptEvent { get; set; }
        public AddCommand AddEvent { get; set; }
        public DeleteCommand DeleteEvent { get; set; }
        public SaveCommand SaveEvent { get; set; }

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

        public AmortizacionesViewModel()
        {
            Collection = new CollectionViewSource();
            if (Metodos.Anticipos == null)
            {
                Metodos.Anticipos = new List<Anticipado>();
            }
            Refresh();
            if (Metodos.Anticipos.Count == 0)
            {
                addResult = new Anticipado();
                addResult.Fecha = DateTime.Now.Date;

                SelectedItem = addResult;
                ButtonAddContent = "Cancelar";
            }
            else
            {
                ButtonAddContent = "Nuevo";
            }

            AcceptEvent = new AcceptCommand<AmortizacionesViewModel>(this);
            CancelEvent = new CancelCommand<AmortizacionesViewModel>(this);
            AddEvent = new AddCommand(this);
            DeleteEvent = new DeleteCommand(this);
            SaveEvent = new SaveCommand(this);
        }

        #endregion

        #region Métodos

        public void Refresh()
        {
            Collection.Source = Metodos.Anticipos;
            Collection.View.Refresh();
            SelectedItem = Collection.View.CurrentItem as Anticipado;
            ButtonAddContent = "Nuevo";
        }

        public void Add()
        {
            Metodos.Anticipos.Add(addResult);
            addResult = null;
            Refresh();
        }

        public void Save()
        { 
            if (ButtonAddContent == "Cancelar")
            {
                Add();
            }
            ButtonAddContent = "Nuevo";
        }

        public void AcceptChanges()
        {
            // Valida los cambios de la amortización
            Metodos.CloseWindow<AmortizacionesViewModel>(true);
        }

        public void CancelChanges()
        {
            // Cancelar los cambios
            Metodos.CloseWindow<AmortizacionesViewModel>(false);
        }

        #endregion

    }
}
