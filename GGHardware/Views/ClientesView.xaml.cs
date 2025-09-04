using System.Windows;
using System.Windows.Controls;

namespace GGHardware.Views
{
    public partial class ClientesView : UserControl
    {
        public ClientesView()
        {
            InitializeComponent();
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Aquí irá la lógica para guardar un cliente en la base de datos
            // Por ahora, solo mostraremos un mensaje.
            MessageBox.Show("Lógica de guardar cliente en desarrollo.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            // Aquí irá la lógica para limpiar todos los campos del formulario
            LimpiarCampos();
        }

        private void LimpiarCampos()
        {
            txtNombre.Text = string.Empty;
            txtApellido.Text = string.Empty;
            txtCUIT.Text = string.Empty;
            txtTelefono.Text = string.Empty;
            txtDireccion.Text = string.Empty;
            txtProvincia.Text = string.Empty;
            txtLocalidad.Text = string.Empty;
            cmbCondicionFiscal.SelectedIndex = -1; // Deselecciona la opción
        }
    }
}