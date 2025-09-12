using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GGHardware.Views
{
    public partial class ClientesView : UserControl
    {
        public ClientesView()
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

        // Método para permitir solo letras (incluyendo acentos y ñ) y espacios.
        private void txtLetras_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Expresión regular que coincide con cualquier carácter que NO sea una letra
            // (mayúscula o minúscula, incluyendo acentuadas), ñ, o un espacio.
            Regex regex = new Regex("[^a-zA-ZáéíóúÁÉÍÓÚñÑ\\s]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // Método para permitir solo números.
        private void txtNumeros_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Expresión regular que coincide con cualquier carácter que NO sea un dígito.
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {// Validaciones al hacer clic en Guardar
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

            if (!Regex.IsMatch(txtCUIT.Text, @"^\d+$"))
            {
                MessageBox.Show("El CUIT solo debe contener números.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Regex.IsMatch(txtTelefono.Text, @"^\d+$"))
            {
                MessageBox.Show("El teléfono solo debe contener números.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Regex.IsMatch(txtProvincia.Text, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                MessageBox.Show("La provincia solo debe contener letras.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Regex.IsMatch(txtLocalidad.Text, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                MessageBox.Show("La localidad solo debe contener letras.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Si todas las validaciones pasan, mostrar mensaje de éxito
            MessageBox.Show("Cliente guardado con éxito.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            
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
            txtNombre.Focus();
        }

        private void dgClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}