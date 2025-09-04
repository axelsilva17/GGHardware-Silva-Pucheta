using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace GGHardware.Views
{
    public partial class Usuarios : UserControl
    {
        public Usuarios()
        {
            InitializeComponent();
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Validar DNI: solo números
            if (!Regex.IsMatch(txtDNI.Text, @"^\d+$"))
            {
                MessageBox.Show("El DNI debe contener solo números.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

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
            txtDNI.Focus(); // Opcional: enfoca el cursor en el primer campo
        }
    }
}