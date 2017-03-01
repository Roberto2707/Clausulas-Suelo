using System;
using System.Collections.Generic;

namespace Clausulas
{
    /// <summary>
    /// Clase para guardar los datos de las Hipotecas en sqlite
    /// </summary>
    public class Hipoteca
    {

        #region Declaración de Propiedades

        public int Id { get; set; }                                              // Identificador
        public string Nombre { get; set; }                                       // Nombre

        public decimal Capital { get; set; }                                     // Capital Inicial
        public int Tiempo { get; set; }                                          // Tiempo en años
        public DateTime Inicio { get; set; }                                     // Fecha primer pago
        public float? InteresInicial { get; set; }                               // Interés fijo inicial
        public int? PeriodoInicial { get; set; }                                 // Meses con interés fijo
        public int PeriodoRevision { get; set; }                                 // Nº de meses para cada revisión del interés
        public int MesReferencia { get; set; }                                   // Mes de referencia a coger. Por ejemplo: 1, cogería 1 mes antes de la fecha.
        public float Diferencial { get; set; }                                   // Diferencial aplicado al euribor
        public float? Bonificacion { get; set; }                                 // Bonificación del diferencial (Interés = Diferencial - Bonificación)
        public float Suelo { get; set; }                                         // Claúsula suelo

        public virtual List<Periodo> Periodos { get; set; }               // Lista de períodos de revisiones
        public virtual List<Amortizacion> Amortizaciones { get; set; }    // Lista de amortizaciones

        #endregion

        #region Constructor de la Clase

        public Hipoteca()
        {
            Periodos = new List<Periodo>();
            Amortizaciones = new List<Amortizacion>();
        }

        #endregion

    }
}
