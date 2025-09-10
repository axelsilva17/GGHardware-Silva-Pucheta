using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GGHardware.Views
{
    public partial class Usuarios : UserControl
    {
        public Usuarios()
        {
            InitializeComponent();
        }
        private void btnVolver_Click(object sender, RoutedEventArgs e)
        {
            // Obtener la ventana principal
            var mainWindow = Window.GetWindow(this) as MainWindow;

            if (mainWindow != null)
            {
                // Cargar la vista de registro dentro del MainContentBorder
                mainWindow.MainContentBorder.Child = new RegistroView(mainWindow);
            }
        }

        private void txtDNI_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Solo permitir números (0-9)
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // método para permitir solo letras en Nombre y Apellido
        private void txtLetras_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Permite letras (mayúsculas, minúsculas), letras acentuadas (áéíóúÁÉÍÓÚ), ñÑ y espacios.
            Regex regex = new Regex("[^a-zA-ZáéíóúÁÉÍÓÚñÑ\\s]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {

            // Validar Nombre y Apellido: solo letras
            if (!Regex.IsMatch(txtNombre.Text, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                MessageBox.Show("El nombre solo debe contener letras.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Regex.IsMatch(txtApellido.Text, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                MessageBox.Show("El apellido solo debe contener letras.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validar Correo: debe contener un @
            if (!txtCorreo.Text.Contains("@"))
            {
                MessageBox.Show("El correo electrónico no es válido. Debe contener un '@'.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validar Contraseña: mínimo 6 caracteres
            if (pbContrasena.Password.Length < 6)
            {
                MessageBox.Show("La contraseña debe tener un mínimo de 6 caracteres.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Si todas las validaciones pasan, mostrar mensaje de éxito y limpiar los campos.
            MessageBox.Show("Usuario guardado con éxito.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            LimpiarCampos();
        }

        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarCampos();
        }

        private void LimpiarCampos()
        {
            // Restablece los campos del formulario a su estado inicial.
            txtDNI.Text = string.Empty;
            txtNombre.Text = string.Empty;
            txtApellido.Text = string.Empty;
            txtCorreo.Text = string.Empty;
            pbContrasena.Password = string.Empty;
            cmbRol.SelectedIndex = -1; // Deselecciona el ítem
            txtDNI.Focus(); 
        }
    }
}