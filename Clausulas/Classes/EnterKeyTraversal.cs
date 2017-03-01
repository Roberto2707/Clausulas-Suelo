using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Clausulas
{
    /// <summary>
    /// Clase para que al pulsar intro funcione igual que el tabulador
    /// </summary>
    public class EnterKeyTraversal
    {

        #region Declaraciones de Propiedades

        public static readonly DependencyProperty IsEnabledProperty =
           DependencyProperty.RegisterAttached("IsEnabled", typeof(bool),
           typeof(EnterKeyTraversal), new UIPropertyMetadata(false, IsEnabledChanged));

        #endregion

        #region Métodos

        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnabledProperty, value);
        }

        static void ue_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var ue = e.OriginalSource as FrameworkElement;

            // Dependiendo de la tecla pulsada se comporta de diferente forma
            switch (e.Key)
            {
                case Key.Enter:
                    if (ue is TextBox)
                    {
                        // Compruebo si es un textbox multilínea, si es multilínea el intro funciona como el tab, pero si se pulsa control+intro, añade un retorno de línea.
                        if ((ue as TextBox).AcceptsReturn)
                        {
                            TextBox t = (ue as TextBox);
                            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                            {
                                e.Handled = true;
                                int comienzo = t.SelectionStart + 2;
                                string texto = t.Text;
                                t.Text = texto.Substring(0, t.SelectionStart) + "\r\n" + texto.Substring(t.SelectionStart);
                                t.SelectionStart = comienzo;
                                t.SelectionLength = 0;
                                return;
                            }
                        }
                    }
                    e.Handled = true;
                    ue.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    break;

                case Key.Down:
                    if (ue is TextBox)
                    {
                        if ((ue as TextBox).AcceptsReturn)
                        {
                            // Comprobar si la posición del cursor está después de un avance de línea
                            TextBox t = (ue as TextBox);
                            int comienzo = t.SelectionStart;
                            string texto = t.Text.Substring(comienzo);
                            if (texto.LastIndexOf("\n") >= 0)
                            {
                                return;
                            }
                        }
                    }
                    e.Handled = true;
                    ue.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    break;

                case Key.Up:
                    if (ue is TextBox)
                    {
                        if ((ue as TextBox).AcceptsReturn)
                        {
                            // Comprobar si la posición del cursor está después de un avance de línea
                            TextBox t = (ue as TextBox);
                            int comienzo = t.SelectionStart;
                            string texto = t.Text.Substring(0, comienzo);
                            if (texto.LastIndexOf("\n") >= 0)
                            {
                                return;
                            }
                        }
                    }
                    e.Handled = true;
                    ue.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
                    break;

                case Key.F12:
                    // Si se pulsa f12 se borra el contenido
                    if (ue is TextBox)
                        (ue as TextBox).Text = "";
                    if (ue is PasswordBox)
                        (ue as PasswordBox).Password = "";
                    break;

                case Key.Escape:
                    Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive).Close();
                    break;
            }
        }

        private static void ue_Unloaded(object sender, RoutedEventArgs e)
        {
            var ue = sender as FrameworkElement;
            if (ue == null) return;
            ue.Unloaded -= ue_Unloaded;
            ue.PreviewKeyDown -= ue_PreviewKeyDown;
            ue.LostFocus -= ue_LostFocus;
        }

        static void IsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ue = d as FrameworkElement;
            if (ue == null) return;
            if ((bool)e.NewValue)
            {
                ue.Unloaded += ue_Unloaded;
                ue.PreviewKeyDown += ue_PreviewKeyDown;
                ue.LostFocus += ue_LostFocus;
            }
            else
            {
                ue.PreviewKeyDown -= ue_PreviewKeyDown;
                ue.LostFocus -= ue_LostFocus;
            }
        }

        private static void ue_LostFocus(object sender, RoutedEventArgs e)
        {
            var ue = e.OriginalSource as FrameworkElement;
            if (ue is TextBox)
            {
                (ue as TextBox).Text = (ue as TextBox).Text.Trim();
            }
        }

        #endregion

    }
}
