using System;

namespace Clausulas
{
    /// <summary>
    /// Clase para guardar los datos de los periodos de revisión de intereses en sqlite
    /// </summary>
    public class Periodo
    {

        public int Id { get; set; }                      // Identificación
        public DateTime Fecha { get; set; }              // Fecha de aplicación
        public float Euribor { get; set; }               // % Euribor a aplicar
        public float Diferencial { get; set; }           // % Diferencial
        public float Bonificacion { get; set; }          // % Bonificación

        public int IdHipoteca { get; set; }              // Enlace con los datos de la hipoteca
        public virtual Hipoteca Hipoteca { get; set; }   // Enlace con la clase Hipoteca

    }
}
