using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Clausulas
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Declaración de Variables
        
        List<Cuota> cuotas;                                            // Lista de cuotas
        List<Revision> revisionesGrabadas = new List<Revision>();      // Lista de revisiones grabadas en la hipoteca seleccionada o al modificar desde el botón de la barra de herramientas
        List<Anticipado> anticiposGrabados = new List<Anticipado>();   // Lista de amortizaciones grabadas en la hipoteca seleccionada o al modificar desde el botón de la barra de herramientas

        Dictionary<string, float> diccionarioEuribor = new Dictionary<string, float>();  // Diccionario para guardar los valores del euribor. La clave es MM/yyyy y el valor es el porcentaje del euribor en el mes y año indicado

        bool errorEnAplicacion = false;  // Esta variable la utilizo para que no pregunte al salir si ha habido algún error

        Hipoteca hipotecaActual;  // Guarda los datos de la hipoteca seleccionada o grabada

        #endregion

        #region Constructor de la Clase

        public MainWindow()
        {
            InitializeComponent();

            // Declaración de eventos
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
            this.Closed += MainWindow_Closed;
        }

        #endregion

        #region Eventos de la Ventana

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CargarEuribor();
            LimpiarCampos();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (errorEnAplicacion)
            {
                return;
            }
            if (MessageBox.Show("¿Salir de la Aplicación?", "Cálculo de Cuotas. Claúsulas Suelo", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            App.Current.Shutdown();
        }

        #endregion

        #region Eventos de los Controles

        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            // Para eliminar el botón que sale a la derecha para ver más botones
            ToolBar toolBar = sender as ToolBar;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }

            var mainPanelBorder = toolBar.Template.FindName("MainPanelBorder", toolBar) as FrameworkElement;
            if (mainPanelBorder != null)
            {
                mainPanelBorder.Margin = new Thickness(0);
            }
        }

        private void IntegerValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            string texto = e.Text;
            e.Handled = !Metodos.IsTextAllowed(texto, "[0-9]+");
        }

        private void IntegerTextBoxPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string texto = (string)e.DataObject.GetData(typeof(string));
                if (!Metodos.IsTextAllowed(texto, "[0-9]+"))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void DecimalValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = (sender as TextBox);
            string texto = e.Text;
            e.Handled = !Metodos.IsTextAllowed(texto, "[0-9\\.\\,]+");
            if (!e.Handled && (texto == "." || texto == ","))
            {
                // Comprobar que no haya más de un signo de puntuación
                if (textBox.Text.IndexOf('.') >= 0 || textBox.Text.IndexOf(',') >= 0)
                {
                    e.Handled = true;
                }
            }
        }

        private void DecimalTextBoxPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string texto = (string)e.DataObject.GetData(typeof(string));
                if (!Metodos.IsTextAllowed(texto, "[0-9\\.\\,]+"))
                {
                    e.CancelCommand();
                }
                else
                {
                    int count = texto.Count(f => f == '.');
                    count += texto.Count(f => f == ',');
                    if (count > 1)
                    {
                        e.CancelCommand();
                    }
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void OnDecimalTextLostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (sender as TextBox);
            if (!string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Text = decimal.Parse(textBox.Text.Replace('.', ',')).ToString("0.00");
            }
        }

        private void dataGridCuotas_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(DateTime))
            {
                ((DataGridTextColumn)e.Column).Binding.StringFormat = "dd/MM/yyyy";
            }
            if (e.PropertyType == typeof(decimal))
            {
                ((DataGridTextColumn)e.Column).Binding.StringFormat = "0.00";
            }
            if (e.PropertyType == typeof(float))
            {
                ((DataGridTextColumn)e.Column).Binding.StringFormat = "0.000";
            }
        }

        private void btnSeleccionarHipotecas_Click(object sender, RoutedEventArgs e)
        {
            if (Metodos.Hipotecas == null || Metodos.Hipotecas.Count == 0)
            {
                MessageBox.Show("No hay hipotecas grabadas", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            HipotecasWindow ventana = new HipotecasWindow();
            if (ventana.ShowDialog() == true)
            {
                using (var context = new ClausulasContext())
                {
                    // Cargar los datos de la hipoteca
                    hipotecaActual = context.Hipotecas
                        .Where(p => p.Id == Metodos.IdHipoteca)
                        .Include(p => p.Periodos)
                        .Include(p => p.Amortizaciones)
                        .SingleOrDefault();

                }
                // Cargar los datos de las revisiones
                revisionesGrabadas = new List<Revision>();
                foreach (var item in hipotecaActual.Periodos)
                {
                    Revision revision = new Revision();
                    revision.Fecha = item.Fecha;
                    revision.Diferencial = item.Diferencial;
                    revision.Euribor = item.Euribor;
                    revision.Bonificacion = item.Bonificacion;
                    revisionesGrabadas.Add(revision);
                }
                // Cargar los datos de las amortizaciones anticipadas
                anticiposGrabados = new List<Anticipado>();
                foreach (var item in hipotecaActual.Amortizaciones)
                {
                    Anticipado anticipo = new Anticipado();
                    anticipo.Fecha = item.Fecha;
                    anticipo.Importe = item.Importe;
                    anticiposGrabados.Add(anticipo);
                }

                // Mostrar los datos en pantalla
                txtNombre.Text = hipotecaActual.Nombre;
                txtCapital.Text = hipotecaActual.Capital.ToString("0.00");
                txtTiempo.Text = hipotecaActual.Tiempo.ToString();
                dpInicio.SelectedDate = hipotecaActual.Inicio;
                dpInicio.Text = dpInicio.SelectedDate?.ToString("dd/MM/yyyy");
                txtInteresInicial.Text = hipotecaActual.InteresInicial == null ? "" : hipotecaActual.InteresInicial?.ToString("0.00");
                txtPeriodoInicial.Text = hipotecaActual.PeriodoInicial == null ? "" : hipotecaActual.PeriodoInicial?.ToString();
                txtPeriodoRevision.Text = hipotecaActual.PeriodoRevision.ToString();
                txtMesReferencia.Text = hipotecaActual.MesReferencia.ToString();
                txtDiferencial.Text = hipotecaActual.Diferencial.ToString("0.00");
                txtBonificacion.Text = hipotecaActual.Bonificacion == null ? "" : hipotecaActual.Bonificacion?.ToString("0.00");
                txtSuelo.Text = hipotecaActual.Suelo.ToString("0.00");

                dpCalculo.SelectedDate = DateTime.Now.Date;
                dpCalculo.Text = dpCalculo.SelectedDate?.ToString("dd/MM/yyyy");

                GenerarCuotas();
            }

        }

        #endregion

        #region Eventos de la Barra de Herramientas

        private void btnNuevo_Click(object sender, RoutedEventArgs e)
        {
            LimpiarCampos();
        }

        private void btnGrabar_Click(object sender, RoutedEventArgs e)
        {
            if (cuotas == null || cuotas.Count == 0)
            {
                MessageBox.Show("Debe calcular las cuotas antes de poder grabar los cambios", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (!Comprobaciones())
            {
                return;
            }
            if (string.IsNullOrEmpty(txtNombre.Text.Trim()))
            {
                MessageBox.Show("Indique un nombre para guardar la hipoteca", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                txtNombre.Focus();
                return;
            }

            GrabarHipoteca();
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (hipotecaActual == null)
            {
                return;
            }
            if (MessageBox.Show("¿Eliminar Hipoteca?", this.Title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }
            using (var context = new ClausulasContext())
            {
                // Comenzar la transacción
                context.Database.BeginTransaction();

                // Eliminar los datos anteriores
                var hipotecaOrigen = context.Hipotecas
                    .Where(p => p.Id == hipotecaActual.Id)
                    .Include(p => p.Periodos)
                    .Include(p => p.Amortizaciones)
                    .SingleOrDefault();
                var periodosAEliminar = context.Periodos.Where(x => x.IdHipoteca == hipotecaActual.Id);
                var amortizacionesAEliminar = context.Amortizaciones.Where(x => x.IdHipoteca == hipotecaActual.Id);
                context.Periodos.RemoveRange(periodosAEliminar);
                context.Amortizaciones.RemoveRange(amortizacionesAEliminar);
                context.Hipotecas.Remove(hipotecaOrigen);

                context.SaveChanges();

                context.Database.CurrentTransaction.Commit();

                // Cargar lista de hipotecas
                Metodos.Hipotecas = context.Hipotecas.OrderBy(x => x.Id).ToList();
            }
            MessageBox.Show("Hipoteca eliminada", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
            LimpiarCampos();
        }

        private void btnSalir_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnGenerarCuotas_Click(object sender, RoutedEventArgs e)
        {
            if (!Comprobaciones())
            {
                return;
            }
            GenerarCuotas();
        }

        private void btnRevisiones_Click(object sender, RoutedEventArgs e)
        {
            if (!Comprobaciones())
            {
                return;
            }
            // Mostrar ventana con las revisiones
            CalcularRevisiones();
            if (Metodos.Revisiones == null ||Metodos.Revisiones.Count == 0)
            {
                MessageBox.Show("El cálculo de cuotas no incluye revisiones. Compruebe los datos.", "Cálculo de Cuotas. Claúsulas suelo.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            RevisionesWindow ventana = new RevisionesWindow();
            if (ventana.ShowDialog() == true)
            {
                revisionesGrabadas = Metodos.Revisiones;
                GenerarCuotas();
            }
        }

        private void btnAmortizaciones_Click(object sender, RoutedEventArgs e)
        {
            if (!Comprobaciones())
            {
                return;
            }
            // Mostrar ventana con las amortizaciones anticipadas
            if (Metodos.Anticipos == null)
            {
                Metodos.Anticipos = new List<Anticipado>();
            }
            AmortizacionesWindow ventana = new AmortizacionesWindow();
            if (ventana.ShowDialog() == true)
            {
                anticiposGrabados = Metodos.Anticipos;
                GenerarCuotas();
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            /*
            SaveFileDialog ExportarExcel = new SaveFileDialog();
            ExportarExcel.Filter = "Archivo Excel (*.xlsx)|*.xlsx";
            ExportarExcel.Title = "Guardar Excel";
            if (ExportarExcel.ShowDialog() == false)
            {
                return;
            }
            if (File.Exists(ExportarExcel.FileName))
            {
                File.Delete(ExportarExcel.FileName);
            }
            */

                if (cuotas == null || cuotas.Count == 0)
            {
                return;
            }

            ExportToExcel<Cuota> xls = new ExportToExcel<Cuota>();
            xls.dataToPrint = cuotas;
            xls.GenerateReport();
        }

        #endregion

        #region Métodos

        private void LimpiarCampos()
        {
            txtNombre.Text = "";
            txtCapital.Text = "";
            txtTiempo.Text = "";
            dpInicio.Text = "";
            txtInteresInicial.Text = "";
            txtPeriodoInicial.Text = "";
            txtPeriodoRevision.Text = "";
            txtMesReferencia.Text = "";
            txtDiferencial.Text = "";
            txtBonificacion.Text = "";
            txtSuelo.Text = "";
            dpCalculo.DisplayDate = DateTime.Now.Date;
            dpCalculo.SelectedDate = DateTime.Now.Date;

            txbPendienteAmortizar.Text = (0M).ToString("C");
            txbPendienteAmortizarReal.Text = (0M).ToString("C");
            txbDiferenciaAmortizacion.Text = (0M).ToString("C");
            txbDiferenciaAmortizacion2.Text = (0M).ToString("C");
            txbSumaDiferencias.Text = (0M).ToString("C");
            txbReclamacion.Text = (0M).ToString("C");

            hipotecaActual = null;
            dataGridCuotas.ItemsSource = null;

            txtCapital.Focus();
        }

        private void CargarEuribor()
        {
            string fileName = AppDomain.CurrentDomain.BaseDirectory + "Resources\\Euribor.txt";
            if (!File.Exists(fileName))
            {
                MessageBox.Show("No se ha encontrado el fichero 'Euribor.txt'", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                errorEnAplicacion = true;
                this.Close();
                return;
            }
            using (StreamReader reader = File.OpenText(fileName))
            {
                string linea;
                while ((linea = reader.ReadLine()) != null)
                {
                    string[] dato = linea.Split('\t');
                    if (dato.Length == 2)
                    {
                        float euribor;
                        float.TryParse(dato[1], out euribor);
                        diccionarioEuribor.Add(dato[0], euribor);
                    }
                }
            }
        }

        private bool Comprobaciones()
        {
            // Comprobar que los campos estén rellenados correctamente
            if (txtCapital.Text == "")
            {
                MessageBox.Show("Indique el capital inicial", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                txtCapital.Focus();
                return false;
            }
            if (txtTiempo.Text == "")
            {
                MessageBox.Show("Indique el período de amortización", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                txtTiempo.Focus();
                return false;
            }
            if (dpInicio.DisplayDate == null || dpInicio.DisplayDate >= DateTime.Now.Date)
            {
                if (dpInicio.DisplayDate == null)
                    MessageBox.Show("Indique la fecha de la primera cuota", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                else
                    MessageBox.Show("La fecha de la primera cuota no es correcta", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                dpInicio.Focus();
                return false;
            }
            if (dpCalculo.DisplayDate == null || dpCalculo.DisplayDate < dpInicio.DisplayDate)
            {
                if (dpCalculo.DisplayDate == null)
                    MessageBox.Show("Indique la fecha para el cálculo", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                else
                    MessageBox.Show("La fecha para el cálculo no puede ser menor que la fecha de la primera cuota", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                dpCalculo.Focus();
                return false;
            }
            if (txtInteresInicial.Text != "" && txtPeriodoInicial.Text == "")
            {
                MessageBox.Show("Si indica interés inicial, debe indicar los meses a los que se aplica", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                txtPeriodoInicial.Focus();
                return false;
            }
            if (txtInteresInicial.Text == "" && txtPeriodoInicial.Text != "")
            {
                MessageBox.Show("Si indica período inicial, debe indicar el interés que se aplica", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                txtInteresInicial.Focus();
                return false;
            }
            if (txtPeriodoRevision.Text == "")
            {
                MessageBox.Show("Indique cada cuantos meses se recalculan las cuotas", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                txtPeriodoRevision.Focus();
                return false;
            }
            if (txtMesReferencia.Text == "")
            {
                MessageBox.Show("Indique de que meses anteriores se coge el euribor de referencia", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                txtMesReferencia.Focus();
                return false;
            }
            if (txtDiferencial.Text == "")
            {
                MessageBox.Show("Indique el diferencial que se aplica", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                txtDiferencial.Focus();
                return false;
            }
            if (txtSuelo.Text == "")
            {
                MessageBox.Show("Indique la claúsula suelo", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                txtSuelo.Focus();
                return false;
            }
            return true;
        }

        private void GenerarCuotas()
        {
            // Variables de los datos indicados
            decimal capital = decimal.Parse(txtCapital.Text);
            DateTime inicio = dpInicio.SelectedDate ?? dpInicio.DisplayDate;
            int meses = int.Parse(txtTiempo.Text) * 12;

            float interesInicial;
            float.TryParse(txtInteresInicial.Text, out interesInicial);

            int mesesPlazoFijo;
            int.TryParse(txtPeriodoInicial.Text, out mesesPlazoFijo);

            int periodoRevision = int.Parse(txtPeriodoRevision.Text);
            int mesReferencia = int.Parse(txtMesReferencia.Text);
            float diferencial = float.Parse(txtDiferencial.Text);

            float bonificacion;
            float.TryParse(txtBonificacion.Text, out bonificacion);

            float suelo = float.Parse(txtSuelo.Text);
            DateTime fechaFinal = dpCalculo.SelectedDate ?? dpCalculo.DisplayDate;

            decimal capitalReal = capital;
            decimal importeCuota = 0M;
            decimal importeCuotaReal = 0M;
            float interes = 0F;
            float interesReal = 0F;
            bool calcularCuota = false;
            DateTime fechaSiguienteRevision = inicio.AddMonths(mesesPlazoFijo);
            int numero = 1;
            decimal sumaDiferencias = 0M;

            DateTime fecha;

            CalcularRevisiones();

            // Obtener las cuotas
            cuotas = new List<Cuota>();
            fecha = inicio;
            while (fecha <= fechaFinal)
            {
                Cuota cuota = new Cuota();
                // Comprobar si hay anticipos
                cuota.Numero = numero++;
                cuota.Anticipo = null;
                if (anticiposGrabados != null && anticiposGrabados.Count > 0)
                {
                    var anticiposTemp = from a in anticiposGrabados
                                        where a.Fecha <= fecha && a.Fecha > fecha.AddMonths(-1)
                                        select a;
                    foreach (var item in anticiposTemp)
                    {
                        cuota.Anticipo = (cuota.Anticipo ?? 0) + item.Importe;
                    }
                }

                cuota.Fecha = fecha;
                capital -= cuota.Anticipo ?? 0;
                cuota.Capital = capital;

                capitalReal -= cuota.Anticipo ?? 0;
                cuota.CapitalReal = capitalReal;

                calcularCuota = false;
                if (fecha < inicio.AddMonths(mesesPlazoFijo))
                {
                    interes = interesInicial;
                    interesReal = interesInicial;
                    calcularCuota = (fecha == inicio);
                }
                else
                {
                    // Buscar el interés en las revisiones
                    if (fecha == fechaSiguienteRevision)
                    {
                        foreach (var item in Metodos.Revisiones)
                        {
                            if (item.Fecha == fecha)
                            {
                                interes = item.Euribor + item.Diferencial - item.Bonificacion;
                                interesReal = interes;
                                if (interes < suelo)
                                {
                                    interes = suelo;
                                }
                                if (interesReal < 0F)
                                {
                                    interesReal = 0F;
                                }
                                break;
                            }
                        }
                        fechaSiguienteRevision = fechaSiguienteRevision.AddMonths(periodoRevision);
                        calcularCuota = true;
                    }
                }

                cuota.Interes = interes;
                cuota.InteresReal = interesReal;
                if (calcularCuota)
                {
                    if (interes == 0F)
                    {
                        importeCuota = Math.Round(cuota.Capital / meses, 2, MidpointRounding.AwayFromZero);
                    }
                    else
                    {
                        importeCuota = Math.Round((cuota.Capital * Convert.ToDecimal(cuota.Interes / 1200F)) / Convert.ToDecimal(1D - Math.Pow(1 + (cuota.Interes / 1200F), meses * -1)), 2, MidpointRounding.AwayFromZero);
                    }
                    if (interesReal == 0)
                    {
                        importeCuotaReal = Math.Round(cuota.CapitalReal / meses, 2, MidpointRounding.AwayFromZero);
                    }
                    else
                    {
                        importeCuotaReal = Math.Round((cuota.CapitalReal * Convert.ToDecimal(cuota.InteresReal / 1200F)) / Convert.ToDecimal(1D - Math.Pow(1 + (cuota.InteresReal / 1200F), meses * -1)), 2, MidpointRounding.AwayFromZero);
                    }
                }

                cuota.ImporteCuota = importeCuota;
                cuota.Intereses = Math.Round(cuota.Capital * Convert.ToDecimal(cuota.Interes / 1200F), 2, MidpointRounding.AwayFromZero);
                cuota.Amortizacion = cuota.ImporteCuota - cuota.Intereses;

                cuota.ImporteCuotaReal = importeCuotaReal;
                cuota.InteresesReal = Math.Round(cuota.CapitalReal * Convert.ToDecimal(cuota.InteresReal / 1200F), 2, MidpointRounding.AwayFromZero);
                cuota.AmortizacionReal = cuota.ImporteCuotaReal - cuota.InteresesReal;

                cuota.Diferencia = cuota.ImporteCuota - cuota.ImporteCuotaReal;
                sumaDiferencias += cuota.Diferencia;

                cuotas.Add(cuota);

                capital -= cuota.Amortizacion;
                capitalReal -= cuota.AmortizacionReal;

                // Añadir un mes para la próxima cuota
                fecha = fecha.AddMonths(1);
                meses -= 1;
                if (meses == 0 || capital <= 0M)
                {
                    break;
                }
            }

            dataGridCuotas.ItemsSource = cuotas;

            FormatearGrid();

            txbPendienteAmortizar.Text = capital.ToString("C");
            txbPendienteAmortizarReal.Text = capitalReal.ToString("C");
            txbDiferenciaAmortizacion.Text = (capital - capitalReal).ToString("C");
            txbDiferenciaAmortizacion2.Text = txbDiferenciaAmortizacion.Text;
            txbSumaDiferencias.Text = sumaDiferencias.ToString("C");
            txbReclamacion.Text = ((capital - capitalReal) + sumaDiferencias).ToString("C");

            // Cálculo de la cuota:      (Capital * (Interés / 1200)) / (1 - (1 + (Interés / 1200)) ^ -Plazos restantes)  ------ (73100 * (3.9/1200)) / (1-(1+(3.9/1200))^-300)
            // Cálculo de los intereses: Capital * (Interés / 1200)
        }

        private void CalcularRevisiones()
        {
            DateTime inicio = dpInicio.SelectedDate ?? dpInicio.DisplayDate;
            int periodoRevision = int.Parse(txtPeriodoRevision.Text);
            int mesesPlazoFijo;
            int.TryParse(txtPeriodoInicial.Text, out mesesPlazoFijo);
            DateTime fechaFinal = dpCalculo.SelectedDate ?? dpCalculo.DisplayDate;
            int mesReferencia = int.Parse(txtMesReferencia.Text);
            float diferencial = float.Parse(txtDiferencial.Text);

            float bonificacion;
            float.TryParse(txtBonificacion.Text, out bonificacion);

            // Le añado a la fecha inicial los meses de plazo fijo, esta fecha se utiliza como auxiliar para los calculos
            DateTime fecha = inicio.AddMonths(mesesPlazoFijo);
            // Bucle para calcular obtener el cuadro de revisiones
            Metodos.Revisiones = new List<Revision>();
            while (fecha <= fechaFinal)
            {
                // Obtener el euribor en el mes indicado
                float euribor = 0F;
                string key = fecha.AddMonths(mesReferencia * -1).ToString("MM/yyyy");
                if (diccionarioEuribor.ContainsKey(key))
                {
                    euribor = diccionarioEuribor[key];
                }
                Revision revision = new Revision();
                revision.Fecha = fecha;
                revision.Euribor = euribor;
                revision.Diferencial = diferencial;
                revision.Bonificacion = bonificacion;

                // Comprobar si existen revisiones modificadas por el usuario
                if (revisionesGrabadas != null && revisionesGrabadas.Count > 0)
                {
                    foreach (var item in revisionesGrabadas)
                    {
                        if (revision.Fecha == item.Fecha)
                        {
                            revision.Euribor = item.Euribor;
                            revision.Diferencial = item.Diferencial;
                            revision.Bonificacion = item.Bonificacion;
                            break;
                        }
                    }
                }

                Metodos.Revisiones.Add(revision);
                fecha = fecha.AddMonths(periodoRevision);
            }
            // Comprobar si las revisiones grabadas son iguales que las nuevas
            if (revisionesGrabadas != null && revisionesGrabadas.Count != Metodos.Revisiones.Count)
            {
                revisionesGrabadas.Clear();
            }
        }

        private void FormatearGrid()
        {
            dataGridCuotas.Columns[0].Header = "Nº";
            dataGridCuotas.Columns[1].Header = "Anticipado";
            dataGridCuotas.Columns[2].Header = "Fecha";
            dataGridCuotas.Columns[3].Header = "Capital";
            dataGridCuotas.Columns[4].Header = "% interés";
            dataGridCuotas.Columns[5].Header = "Cuota";
            dataGridCuotas.Columns[6].Header = "Intereses";
            dataGridCuotas.Columns[7].Header = "Amortización";
            dataGridCuotas.Columns[8].Header = "---";
            dataGridCuotas.Columns[9].Header = "Capital";
            dataGridCuotas.Columns[10].Header = "% interés";
            dataGridCuotas.Columns[11].Header = "Cuota";
            dataGridCuotas.Columns[12].Header = "Interés";
            dataGridCuotas.Columns[13].Header = "Amortización";
            dataGridCuotas.Columns[14].Header = "Diferencia";
        }

        private void GrabarHipoteca()
        {
            // Cargar los datos de la hipoteca
            if (hipotecaActual == null)
            {
                hipotecaActual = new Hipoteca();
            }
            hipotecaActual.Nombre = txtNombre.Text.Trim();
            hipotecaActual.Capital = decimal.Parse(txtCapital.Text);
            hipotecaActual.Tiempo = int.Parse(txtTiempo.Text);
            hipotecaActual.Inicio = dpInicio.SelectedDate ?? DateTime.Parse(dpInicio.Text);

            hipotecaActual.InteresInicial = null;
            if (!string.IsNullOrEmpty(txtInteresInicial.Text))
            {
                hipotecaActual.InteresInicial = float.Parse(txtInteresInicial.Text);
            }
            hipotecaActual.PeriodoInicial = null;
            if (!string.IsNullOrEmpty(txtPeriodoInicial.Text))
            {
                hipotecaActual.PeriodoInicial = int.Parse(txtPeriodoInicial.Text);
            }
            hipotecaActual.PeriodoRevision = int.Parse(txtPeriodoRevision.Text);
            hipotecaActual.MesReferencia = int.Parse(txtMesReferencia.Text);
            hipotecaActual.Diferencial = float.Parse(txtDiferencial.Text);

            hipotecaActual.Bonificacion = null;
            if (!string.IsNullOrEmpty(txtBonificacion.Text))
            {
                hipotecaActual.Bonificacion = float.Parse(txtBonificacion.Text);
            }
            hipotecaActual.Suelo = float.Parse(txtSuelo.Text);

            using (var context = new ClausulasContext())
            {
                // Comenzar la transacción
                context.Database.BeginTransaction();

                if (hipotecaActual.Id != 0)
                {
                    // Eliminar los datos anteriores
                    var hipotecaOrigen = context.Hipotecas
                        .Where(p => p.Id == hipotecaActual.Id)
                        .Include(p => p.Periodos)
                        .Include(p => p.Amortizaciones)
                        .SingleOrDefault();
                    var periodosAEliminar = context.Periodos.Where(x => x.IdHipoteca == hipotecaActual.Id);
                    var amortizacionesAEliminar = context.Amortizaciones.Where(x => x.IdHipoteca == hipotecaActual.Id);
                    context.Periodos.RemoveRange(periodosAEliminar);
                    context.Amortizaciones.RemoveRange(amortizacionesAEliminar);
                    context.Hipotecas.Remove(hipotecaOrigen);
                }

                hipotecaActual.Id = 0;
                hipotecaActual.Periodos = new List<Periodo>();
                hipotecaActual.Amortizaciones = new List<Amortizacion>();
                context.Hipotecas.Add(hipotecaActual);

                // Añadir los periodos de revisiones a la hipotecta
                foreach (var item in Metodos.Revisiones)
                {
                    Periodo periodo = new Periodo();
                    periodo.Hipoteca = hipotecaActual;
                    periodo.Fecha = item.Fecha;
                    periodo.Euribor = item.Euribor;
                    periodo.Diferencial = item.Diferencial;
                    periodo.Bonificacion = item.Bonificacion;
                    periodo.IdHipoteca = hipotecaActual.Id;
                    hipotecaActual.Periodos.Add(periodo);
                }

                // Añadir las amortizaciones a la hipoteca
                foreach (var item in anticiposGrabados)
                {
                    Amortizacion amortizacion = new Amortizacion();
                    amortizacion.Hipoteca = hipotecaActual;
                    amortizacion.Fecha = item.Fecha;
                    amortizacion.Importe = item.Importe;
                    amortizacion.IdHipoteca = hipotecaActual.Id;
                    hipotecaActual.Amortizaciones.Add(amortizacion);
                }

                context.Entry(hipotecaActual).State = EntityState.Added;
                context.SaveChanges();

                context.Database.CurrentTransaction.Commit();
                // Cargar lista de hipotecas
                Metodos.Hipotecas = context.Hipotecas.OrderBy(x => x.Id).ToList();
            }
            MessageBox.Show("Hipoteca grabada.", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

    }
}
