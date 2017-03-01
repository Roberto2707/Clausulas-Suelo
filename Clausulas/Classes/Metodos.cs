using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;

namespace Clausulas
{
    /// <summary>
    /// Clase estática para definir propiedades y métodos globales
    /// </summary>
    public static class Metodos
    {

        #region Declaración de Propiedades

        public static List<Revision> Revisiones { get; set; }
        public static List<Anticipado> Anticipos { get; set; }
        public static List<Hipoteca> Hipotecas { get; set; }

        /// <summary>
        /// Propiedad para guardar el id de la hipoteca seleccionada
        /// </summary>
        public static int IdHipoteca { get; set; }

        #endregion

        #region Métodos

        /// <summary>
        /// Devuelve si el texto cumple o no el patrón
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static bool IsTextAllowed(string text, string pattern)
        {
            Regex regex = new Regex(pattern); //regex that matches allowed text
            return (regex.Match(text).ToString() == text);
        }

        /// <summary>
        /// Cerrar la ventana actual si coincide el datacontext pasado
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        public static void CloseWindow<T>(bool result)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext is T)
                {
                    window.DialogResult = result;
                    window.Close();
                    return;
                }
            }
        }

        #endregion

    }
}
