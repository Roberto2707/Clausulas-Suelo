using System;

namespace Clausulas
{
    /// <summary>
    /// Clase para guardar los datos que se muestran en la rejilla y que se exportan a excel
    /// </summary>
    public class Cuota
    {

        public int Numero { get; set; }
        public decimal? Anticipo { get; set; }  // Importe anticipado
        public DateTime Fecha { get; set; }

        public decimal Capital { get; set; }
        public float Interes { get; set; }
        public decimal ImporteCuota { get; set; }
        public decimal Intereses { get; set; }
        public decimal Amortizacion { get; set; }

        public string O { get; set; }   // Esto es una columna que hace de separación entre la amortización actual del banco y la amortización real sin claúsula suelo

        public decimal CapitalReal { get; set; }
        public float InteresReal { get; set; }
        public decimal ImporteCuotaReal { get; set; }
        public decimal InteresesReal { get; set; }
        public decimal AmortizacionReal { get; set; }

        public decimal Diferencia { get; set; }

    }
}
