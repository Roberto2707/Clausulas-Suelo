using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Clausulas
{
    /// <summary>
    /// Clase para reemplazar el punto decimal por coma y devolver un valor 0,00
    /// </summary>
    public class DecimalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = null;
            if (value != null)
            {
                decimal valor;
                decimal.TryParse(value.ToString().Replace(".", ","), out valor);
                result = valor.ToString("0.00");
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = null;
            if (value != null)
            {
                decimal valor;
                decimal.TryParse(value.ToString().Replace(".", ","), out valor);
                result = valor.ToString("0.00");
            }
            return result;
        }

    }
}
