using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GGHardware.Models;
using GGHardware.ViewModels;

namespace GGHardware.Views
{
    public partial class VentasView : UserControl
    {
        public VentasViewModel ViewModel { get; set; }
        private ListBox lstClientesSugerencias;

        public VentasView()
        {
            InitializeComponent();
            ViewModel = new VentasViewModel();
            this.DataContext = ViewModel;

            CrearListaClientes();

            // Suscribirse a eventos para recargar datos
            this.Loaded += VentasView_Loaded;
            this.IsVisibleChanged += VentasView_IsVisibleChanged;
        }

        private void VentasView_Loaded(object sender, RoutedEventArgs e)
        {
            // Recargar clientes cuando la vista se carga por primera vez
            ViewModel.RecargarClientes();
            System.Diagnostics.Debug.WriteLine("VentasView cargada - Clientes recargados");
        }

        private void VentasView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Recargar clientes cada vez que la vista se hace visible
            if ((bool)e.NewValue == true && ViewModel != null)
            {
                ViewModel.RecargarClientes();
                System.Diagnostics.Debug.WriteLine("VentasView visible - Clientes recargados");
            }
        }

        private void CrearListaClientes()
        {
            // Crear ListBox para mostrar sugerencias de clientes
            lstClientesSugerencias = new ListBox
            {
                Visibility = Visibility.Collapsed,
                Background = (Brush)FindResource("Surface"),
                BorderBrush = (Brush)FindResource("PrimaryBrush"),
                BorderThickness = new Thickness(1),
                MaxHeight = 150,
                Margin = new Thickness(0, 5, 0, 0)
            };

            // Acceder a la estructura correcta basada en tu XAML
            var mainGrid = (Grid)this.Content;
            var rightBorder = (Border)mainGrid.Children[1];
            var rightGrid = (Grid)rightBorder.Child;

            // Configurar la posición en la fila 1
            Grid.SetRow(lstClientesSugerencias, 1);
            Grid.SetColumn(lstClientesSugerencias, 0);

            // Agregar después del TextBox
            rightGrid.Children.Insert(2, lstClientesSugerencias);

            // Evento de selección de cliente
            lstClientesSugerencias.SelectionChanged += LstClientesSugerencias_SelectionChanged;
        }

        private void txtBuscarCliente_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            string texto = textBox.Text;

            if (string.IsNullOrWhiteSpace(texto))
            {
                lstClientesSugerencias.Visibility = Visibility.Collapsed;
                return;
            }

            // Filtrar clientes
            ViewModel.FiltrarClientes(texto);

            // Mostrar u ocultar lista de sugerencias
            if (ViewModel.ClientesFiltrados.Count > 0)
            {
                lstClientesSugerencias.ItemsSource = ViewModel.ClientesFiltrados;
                lstClientesSugerencias.DisplayMemberPath = "NombreCompleto";
                lstClientesSugerencias.Visibility = Visibility.Visible;
            }
            else
            {
                lstClientesSugerencias.Visibility = Visibility.Collapsed;

                // Si no hay resultados, mostrar mensaje temporal
                if (texto.Length >= 2) // Solo si el usuario ha escrito al menos 2 caracteres
                {
                    System.Diagnostics.Debug.WriteLine($"No se encontraron clientes para: '{texto}'");
                }
            }
        }

        private void LstClientesSugerencias_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstClientesSugerencias.SelectedItem is Cliente clienteSeleccionado)
            {
                // Seleccionar cliente en el ViewModel
                ViewModel.SeleccionarCliente(clienteSeleccionado);

                // Actualizar texto del TextBox
                txtBuscarCliente.Text = clienteSeleccionado.NombreCompleto;

                // Ocultar lista de sugerencias
                lstClientesSugerencias.Visibility = Visibility.Collapsed;

                // Mostrar información del cliente seleccionado
                MostrarInfoCliente();
            }
        }

        private void MostrarInfoCliente()
        {
            if (ViewModel.ClienteSeleccionado != null)
            {
                var cliente = ViewModel.ClienteSeleccionado;
                MessageBox.Show($"Cliente seleccionado:\nNombre: {cliente.nombre} {cliente.apellido}\nCUIT: {cliente.cuit}\nDirección: {cliente.direccion}\nCondición Fiscal: {cliente.condicion_fiscal}",
                    "Cliente Seleccionado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Método público para forzar recarga (opcional, para llamar desde otras vistas)
        public void ForceReloadClientes()
        {
            ViewModel?.RecargarClientes();
        }

        // Evento para el botón Agregar en el DataGrid
        private void AgregarProducto_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is Producto producto)
            {
                ViewModel.AgregarProducto(producto);
            }
        }

        // NUEVO: Evento para agregar una unidad más
        private void AgregarUnaUnidad_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is CarritoItem item)
            {
                ViewModel.AgregarUnaUnidad(item);
            }
        }

        // NUEVO: Evento para quitar solo una unidad
        private void QuitarUnaUnidad_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is CarritoItem item)
            {
                ViewModel.QuitarUnaUnidad(item);
            }
        }

        // Evento para el botón Quitar del carrito (quitar todo)
        private void QuitarProducto_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is CarritoItem item)
            {
                ViewModel.QuitarProducto(item);
            }
        }

        // Evento para el botón Finalizar Venta
        private void FinalizarVenta_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.FinalizarVenta();
        }

        // Evento para el botón Cancelar
        private void CancelarVenta_Click(object sender, RoutedEventArgs e)
        {
            var resultado = MessageBox.Show("¿Está seguro que desea cancelar la venta?",
                "Cancelar Venta", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                ViewModel.CancelarVenta();
                txtBuscarCliente.Text = "";
            }
        }

        // Eventos para el placeholder del TextBox
        private void TxtBuscarCliente_GotFocus(object sender, RoutedEventArgs e)
        {
            // El placeholder se maneja automáticamente con el estilo XAML
        }

        private void TxtBuscarCliente_LostFocus(object sender, RoutedEventArgs e)
        {
            // Ocultar sugerencias cuando pierde el foco
            lstClientesSugerencias.Visibility = Visibility.Collapsed;
        }
    }
}