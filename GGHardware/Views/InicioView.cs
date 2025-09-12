using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GGHardware.Data;

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
                    // Inicio de sesión exitoso
                    MessageBox.Show("Inicio de sesión exitoso.", "Bienvenido", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Cargar la vista principal
                    var mainWindow = Window.GetWindow(this) as MainWindow;
                    if (mainWindow != null)
                    {
                        // Habilitar el menú de navegación
                        mainWindow.HabilitarMenu();

                        // Cargar la vista de inicio dentro del MainContentBorder
                        mainWindow.MainContentBorder.Child = new InicioView();
                    }
                }
                else
                {
                    MessageBox.Show("Usuario o contraseña incorrectos.", "Error de Inicio de Sesión", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnRegistrarse_Click(object sender, RoutedEventArgs e)
        {
            //lógica de navegación a la vista de Registro
            MessageBox.Show("Ir a Registro");
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

        public InicioView()
        {
            InitializeComponent();
        }
    }
}