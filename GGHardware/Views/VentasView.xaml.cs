using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

            // NUEVO: Agregar atajos de teclado
            this.KeyDown += VentasView_KeyDown;
        }

        private void VentasView_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.RecargarClientes();
            System.Diagnostics.Debug.WriteLine("VentasView cargada - Clientes recargados");
        }

        private void VentasView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true && ViewModel != null)
            {
                ViewModel.RecargarClientes();
                System.Diagnostics.Debug.WriteLine("VentasView visible - Clientes recargados");
            }
        }

        // NUEVO: Manejador de atajos de teclado
        private void VentasView_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F1:
                    txtBuscarCliente.Focus();
                    e.Handled = true;
                    break;

                case Key.F2:
                    txtCodigoProducto.Focus();
                    e.Handled = true;
                    break;

                case Key.F3:
                    if (ViewModel.Carrito.Count > 0 && ViewModel.ClienteSeleccionado != null)
                        FinalizarVenta_Click(sender, e);
                    e.Handled = true;
                    break;

                case Key.Escape:
                    CancelarVenta_Click(sender, e);
                    e.Handled = true;
                    break;
            }
        }

        private void CrearListaClientes()
        {
            lstClientesSugerencias = new ListBox
            {
                Visibility = Visibility.Collapsed,
                Background = (System.Windows.Media.Brush)FindResource("Surface"),
                BorderBrush = (System.Windows.Media.Brush)FindResource("PrimaryBrush"),
                BorderThickness = new Thickness(1),
                MaxHeight = 150,
                Margin = new Thickness(0, 5, 0, 0)
            };

            var mainGrid = (Grid)this.Content;
            var rightBorder = (Border)mainGrid.Children[1];
            var rightGrid = (Grid)rightBorder.Child;

            Grid.SetRow(lstClientesSugerencias, 1);
            Grid.SetColumn(lstClientesSugerencias, 0);

            rightGrid.Children.Insert(2, lstClientesSugerencias);

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

            ViewModel.FiltrarClientes(texto);

            if (ViewModel.ClientesFiltrados.Count > 0)
            {
                lstClientesSugerencias.ItemsSource = ViewModel.ClientesFiltrados;
                lstClientesSugerencias.DisplayMemberPath = "NombreCompleto";
                lstClientesSugerencias.Visibility = Visibility.Visible;
            }
            else
            {
                lstClientesSugerencias.Visibility = Visibility.Collapsed;

                if (texto.Length >= 2)
                {
                    System.Diagnostics.Debug.WriteLine($"No se encontraron clientes para: '{texto}'");
                }
            }
        }

        private void LstClientesSugerencias_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstClientesSugerencias.SelectedItem is Cliente clienteSeleccionado)
            {
                ViewModel.SeleccionarCliente(clienteSeleccionado);
                txtBuscarCliente.Text = clienteSeleccionado.NombreCompleto;
                lstClientesSugerencias.Visibility = Visibility.Collapsed;
            }
        }

        public void ForceReloadClientes()
        {
            ViewModel?.RecargarClientes();
        }

        private void AgregarProducto_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is Producto producto)
            {
                ViewModel.AgregarProducto(producto);
            }
        }

        private void AgregarUnaUnidad_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is CarritoItem item)
            {
                ViewModel.AgregarUnaUnidad(item);
            }
        }

        private void QuitarUnaUnidad_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is CarritoItem item)
            {
                ViewModel.QuitarUnaUnidad(item);
            }
        }

        private void QuitarProducto_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is CarritoItem item)
            {
                ViewModel.QuitarProducto(item);
            }
        }

        private void FinalizarVenta_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.FinalizarVenta();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

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

        private void TxtBuscarCliente_GotFocus(object sender, RoutedEventArgs e)
        {
            // El placeholder se maneja automáticamente con el estilo XAML
        }

        private void TxtBuscarCliente_LostFocus(object sender, RoutedEventArgs e)
        {
            lstClientesSugerencias.Visibility = Visibility.Collapsed;
        }

        //  Búsqueda por código con Enter
        private void TxtCodigoProducto_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (sender is TextBox textBox)
                {
                    string codigo = textBox.Text;
                    ViewModel.BuscarProductoPorCodigo(codigo);
                    textBox.Clear(); // Limpiar para siguiente escaneo
                    textBox.Focus(); // Mantener foco para siguiente producto
                }
            }
        }

        // ACTUALIZADO: Botones de descuento globales
        private void AplicarDescuento5_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AplicarDescuentoGlobal(5);
        }

        private void AplicarDescuento10_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AplicarDescuentoGlobal(10);
        }

        private void AplicarDescuento15_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AplicarDescuentoGlobal(15);
        }

        private void BuscarProducto_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.BuscarProducto();
        }

        private void VerHistorial_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainContentBorder.Child = null;
                mainWindow.MainContentBorder.Child = new HistorialVentasView();
            }
        }
    }
}