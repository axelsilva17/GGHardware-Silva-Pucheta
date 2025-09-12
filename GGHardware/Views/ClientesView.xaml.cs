using GGHardware.Data;
using GGHardware.Models;
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
            CargarClientes();
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

        private void CargarClientes()
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    dgClientes.ItemsSource = context.Clientes.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Error al cargar usuarios: " + ex.Message);
            }
        }


        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Validaciones
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

            // Crear objeto Cliente a partir de los campos
            var cliente = new GGHardware.Models.Cliente
            {
                nombre = txtNombre.Text,
                apellido = txtApellido.Text,
                cuit = int.TryParse(txtCUIT.Text, out int cuitVal) ? cuitVal : 0,
                telefono = int.TryParse(txtTelefono.Text, out int telVal) ? telVal : 0,
                direccion = txtDireccion.Text,
                provincia = txtProvincia.Text,
                localidad = txtLocalidad.Text,
                condicion_fiscal = (cmbCondicionFiscal.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "SinCondicion",
            };

            try
            {
                using (var context = new ApplicationDbContext())
                {
                    context.Clientes.Add(cliente);
                    context.SaveChanges();
                }

                MessageBox.Show("Cliente guardado con éxito.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                LimpiarCampos();
                CargarClientes(); // Actualiza el DataGrid
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Error al guardar cliente: " + ex.Message);
            }
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