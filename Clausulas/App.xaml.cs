using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Clausulas
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {

        #region Api de Windows

        [DllImport("user32", CharSet = CharSet.Unicode)]
        static extern IntPtr FindWindow(string cls, string win);
        [DllImport("user32")]
        static extern IntPtr SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32")]
        static extern bool IsIconic(IntPtr hWnd);
        [DllImport("user32")]
        static extern bool OpenIcon(IntPtr hWnd);

        #endregion

        #region Constructor de la Clase

        public App()
        {
            // Indicar que el programa sólo finaliza cuando el usuario lo indica
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        #endregion

        #region Eventos de la Aplicación

        /// <summary>
        /// Punto de Entrada de la aplicación
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            bool isNew;
            // Con el objeto mutex se comprueba si hay otra instancia de la aplicación ejecutándose
            var mutex = new Mutex(true, "{a0ea513c-652b-43a9-926e-7d836a486731}", out isNew);
            if (!isNew)
            {
                ActivateOtherWindow();
                Shutdown();
            }
            else
            {
                using (var context = new ClausulasContext())
                {
                    // Cargar lista de hipotecas
                    Metodos.Hipotecas = context.Hipotecas.OrderBy(x => x.Id).ToList();
                }

                // Cargar la ventana principal
                MainWindow main = new MainWindow();
                main.Show();
            }
        }

        #endregion

        #region Métodos

        /// <summary>
        /// Activar la aplicación si ya se está ejecutando (Se usan métodos de la Api de Windows)
        /// </summary>
        private static void ActivateOtherWindow()
        {
            var other = FindWindow(null, "Cálculo de Cuotas. Claúsulas Suelo");
            if (other != IntPtr.Zero)
            {
                SetForegroundWindow(other);
                if (IsIconic(other))
                    OpenIcon(other);
            }
        }

        #endregion

    }
}
