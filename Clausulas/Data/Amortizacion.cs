using System;

namespace Clausulas
{
    /// <summary>
    /// Clase para guardar los datos de amortizaciones anticipadas en sqlite
    /// </summary>
    public class Amortizacion
    {

        public int Id { get; set; }                      // Identificador
        public DateTime Fecha { get; set; }              // Fecha de Amortización
        public decimal Importe { get; set; }             // Importe amortizado

        public int IdHipoteca { get; set; }              // Enlace con los datos de la hipoteca
        public virtual Hipoteca Hipoteca { get; set; }   // Enlace con la clase Hipoteca

    }
}
