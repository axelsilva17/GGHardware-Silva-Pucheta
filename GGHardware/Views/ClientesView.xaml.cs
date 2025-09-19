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
        }

        /// <summary>
        /// Maneja el evento de clic para el botón 'Volver' y navega a la vista de Registro.
        /// </summary>
        private void btnVolver_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;

            if (mainWindow != null)
            {
                mainWindow.MainContentBorder.Child = new RegistroView(mainWindow);
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
            // Validaciones de entrada (mantengo tu lógica original)
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

        /// <summary>
        /// Limpia todos los campos de entrada del formulario.
        /// </summary>
        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarCampos();
        }

        /// <summary>
        /// Borra el contenido de los TextBoxes y el ComboBox.
        /// </summary>
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

        /// <summary>
        /// Maneja la selección de una fila en el DataGrid (no hay lógica por defecto, pero se mantiene para futura implementación).
        /// </summary>
        private void dgClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Puedes agregar lógica aquí para cargar los datos del cliente seleccionado en los campos de texto
            // para su posterior edición.
        }

        /// <summary>
        /// Maneja el evento de clic para el botón 'Editar'. Edita el cliente seleccionado en el DataGrid.
        /// </summary>
        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (dgClientes.SelectedItem is Cliente clienteSeleccionado)
            {
                // Implementa aquí la lógica para editar el cliente
                // Puedes cargar los datos en los campos de texto y luego, al hacer clic en 'Guardar',
                // actualizar el cliente existente en lugar de agregar uno nuevo.
                MessageBox.Show($"Lógica de edición para el cliente: {clienteSeleccionado.nombre} {clienteSeleccionado.apellido}");
            }
            else
            {
                MessageBox.Show("Por favor, selecciona un cliente para editar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Maneja el evento de clic para el botón 'Eliminar'. Elimina el cliente seleccionado de la base de datos.
        /// </summary>
        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (dgClientes.SelectedItem is Cliente clienteSeleccionado)
            {
                var resultado = MessageBox.Show($"¿Estás seguro de que quieres eliminar a {clienteSeleccionado.nombre} {clienteSeleccionado.apellido}?", "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (resultado == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var context = new ApplicationDbContext())
                        {
                            context.Entry(clienteSeleccionado).State = EntityState.Deleted;
                            context.SaveChanges();
                            CargarClientes(); // Refresca el DataGrid
                            MessageBox.Show("Cliente eliminado con éxito.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"❌ Error al eliminar cliente: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Por favor, selecciona un cliente para eliminar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}