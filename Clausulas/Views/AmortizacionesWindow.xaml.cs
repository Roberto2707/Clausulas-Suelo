using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Clausulas
{
    /// <summary>
    /// Lógica de interacción para AmortizacionesWindow.xaml
    /// </summary>
    public partial class AmortizacionesWindow : Window
    {
        public AmortizacionesWindow()
        {
            InitializeComponent();
            this.DataContext = new AmortizacionesViewModel();
        }
    }
}
