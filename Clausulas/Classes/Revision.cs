using System;

namespace Clausulas
{
    /// <summary>
    /// Clase para guardar los datos de las revisiones
    /// </summary>
    public class Revision
    {

        public DateTime Fecha { get; set; }         // Fecha de la revisión
        public float Euribor { get; set; }          // Valor del euribor en la revisión
        public float Diferencial { get; set; }      // Valor del diferencial en la revisión
        public float Bonificacion { get; set; }     // Valor de la bonificación en la revisión

    }
}
