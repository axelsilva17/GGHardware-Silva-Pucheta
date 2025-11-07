using GGHardware.Data;
using GGHardware.Models;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace GGHardware.Views
{
    public partial class ClientesView : UserControl
    {
        public ClientesView()
        {
            InitializeComponent();
            CargarClientes();
            this.PreviewKeyDown += ClientesView_PreviewKeyDown;
        }

        private void ClientesView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                
                if (Keyboard.FocusedElement == btnLimpiar)
                {
                    btnLimpiar_Click(btnLimpiar, null);
                }
                else
                {
                    btnGuardar_Click(null, null);
                }

                e.Handled = true; 
            }
        }


        /// <summary>
        /// Valida la entrada de texto para permitir solo letras y espacios.
        /// </summary>
        private void txtLetras_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^a-zA-ZáéíóúÁÉÍÓÚñÑ\\s]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        /// <summary>
        /// Valida la entrada de texto para permitir solo números.
        /// </summary>
        private void txtNumeros_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        /// <summary>
        /// Valida si el formato del email es correcto.
        /// </summary>
        private bool ValidarEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return true; 

            string patron = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, patron);
        }

        /// <summary>
        /// Carga todos los clientes de la base de datos y actualiza el DataGrid.
        /// </summary>
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

        /// <summary>
        /// Maneja el evento de clic para el botón 'Guardar'.
        /// Valida los campos y guarda un nuevo cliente en la base de datos.
        /// </summary>
        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Validaciones de entrada
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
            // Validación de email
            if (!string.IsNullOrWhiteSpace(txtEmail.Text) && !ValidarEmail(txtEmail.Text))
            {
                MessageBox.Show("El formato del email no es válido.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var cliente = new GGHardware.Models.Cliente
            {
                nombre = txtNombre.Text,
                apellido = txtApellido.Text,
                cuit = txtCUIT.Text,
                telefono = txtTelefono.Text,
                direccion = txtDireccion.Text,
                provincia = txtProvincia.Text,
                localidad = txtLocalidad.Text,
                email = txtEmail.Text,
                condicion_fiscal = (cmbCondicionFiscal.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "SinCondicion",
            };

            try
            {
                using (var context = new ApplicationDbContext())
                {
                    context.Clientes.Add(cliente);
                    context.SaveChanges();

                    // Notificar a VentasView
                    ClienteService.Instance.NotificarClienteAgregado(cliente);
                }

                MessageBox.Show("Cliente guardado con éxito.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                LimpiarCampos();
                CargarClientes(); // Actualiza el DataGrid
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Error al guardar cliente: " + ex.Message + "\n\nDetalles: " + ex.InnerException?.Message);
            }
        }

        /// <summary>
        /// Limpia todos los campos de entrada del formulario.
        /// </summary>
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
            txtEmail.Text = string.Empty;
            cmbCondicionFiscal.SelectedIndex = -1; // Deselecciona la opción
            txtNombre.Focus();
        }

        /// <summary>
        /// Maneja la selección de una fila en el DataGrid (no hay lógica por defecto, pero se mantiene para futura implementación).
        /// </summary>
        private void dgClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Puedes agregar lógica aquí para cargar los datos del cliente seleccionado en los campos de texto
            // para su posterior edición.
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (dgClientes.SelectedItem is Cliente clienteSeleccionado)
            {
                var button = sender as Button;
                if (button?.DataContext is GGHardware.Models.Cliente cliente)
                {
                    var mainWindow = Window.GetWindow(this) as MainWindow;
                    if (mainWindow != null)
                    {
                        mainWindow.MainContentBorder.Child = new EditarCliente(cliente.id_cliente);
                    }
                }
            }
            else
            {
                MessageBox.Show("Por favor, selecciona un cliente para editar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Maneja el evento de clic para el botón 'Eliminar'. Elimina el cliente seleccionado de la base de datos.
        /// </summary>
        private void btnCambiarEstado_Click(object sender, RoutedEventArgs e)
        {
            if (dgClientes.SelectedItem is Cliente cliente)
            {
                using (var context = new ApplicationDbContext())
                {
                    var cli = context.Clientes.Find(cliente.id_cliente);
                    if (cli != null)
                    {
                        // Cambiar el estado de Activo
                        cli.Activo = !cli.Activo; // Activo -> Inactivo o Inactivo -> Activo
                        context.SaveChanges();
                    }
                }
                CargarClientes(); // Método que recarga el DataGrid
            }
        }
        public class ClienteService
        {
            private static ClienteService _instance;
            public static ClienteService Instance => _instance ??= new ClienteService();

            public event EventHandler<ClienteEventArgs>  ClienteAgregado;
            public event EventHandler<ClienteEventArgs> ClienteActualizado;

            public void NotificarClienteAgregado(Cliente cliente)
            {
                ClienteAgregado?.Invoke(this, new ClienteEventArgs { Cliente = cliente });
            }

            public void NotificarClienteActualizado(Cliente cliente)
            {
                ClienteActualizado?.Invoke(this, new ClienteEventArgs { Cliente = cliente });
            }
        }

        public class ClienteEventArgs : EventArgs
        {
            public Cliente Cliente { get; set; }
        }
    }

}