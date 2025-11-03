using GGHardware.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GGHardware.Views
{
    public partial class InicioView : UserControl
    {
        private void BtnInicioSesion_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new ApplicationDbContext())
            {
                var usuario = context.Usuarios
                    .FirstOrDefault(u => u.correo == txtCorreo.Text && u.contraseña == pbContrasena.Password);

                if (usuario != null)
                {
                    // Verificar si el usuario está activo
                    if (!usuario.Activo)
                    {
                        MessageBox.Show("Su cuenta está inactiva. No puede iniciar sesión.",
                                        "Cuenta Inactiva", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    // Llamar al método de MainWindow
                    var mainWindow = Window.GetWindow(this) as MainWindow;
                    if (mainWindow != null)
                    {
                        mainWindow.IniciarSesion(usuario);
                    }
                }
                else
                {
                    MessageBox.Show("Usuario o contraseña incorrectos.", "Error de Inicio de Sesión", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb.Text == "Usuario o Correo")
            {
                tb.Text = "";
                tb.Foreground = Brushes.Black;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Text = "Usuario o Correo";
                tb.Foreground = Brushes.Gray;
            }
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var pb = sender as PasswordBox;
            if (pb.Tag != null && pb.Tag.ToString() == "Contrasena" && pb.ToolTip?.ToString() == "Contrasena")
            {
                pb.ToolTip = null;
            }
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var pb = sender as PasswordBox;
            if (string.IsNullOrWhiteSpace(pb.Password))
            {
                pb.ToolTip = "Contrasena";
            }
        }
        private void InicioView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnInicioSesion_Click(null, null); // Llama al método que ya tenés
                e.Handled = true; 
            }
        }

        public InicioView()
        {
            InitializeComponent();
            // Atajo Enter para iniciar sesión
            this.PreviewKeyDown += InicioView_PreviewKeyDown;
        }
    }
}