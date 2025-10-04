using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using GGHardware.ViewModels;
using GGHardware.Models;

namespace GGHardware.Views
{
    public partial class HistorialVentasView : UserControl
    {
        public HistorialVentasViewModel ViewModel { get; set; }

        public HistorialVentasView()
        {
            InitializeComponent();
            ViewModel = new HistorialVentasViewModel();
            this.DataContext = ViewModel;
        }

        private void txtBuscarCliente_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            string texto = textBox?.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(texto) || texto.Length < 2)
            {
                popupSugerencias.IsOpen = false;
                return;
            }

            var clientesFiltrados = ViewModel.Clientes
                .Where(c => c.NombreCompleto.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                           (c.cuit != null && c.cuit.Contains(texto)))
                .GroupBy(c => c.id_cliente)
                .Select(g => g.First())
                .Take(10)
                .ToList();

            if (clientesFiltrados.Any())
            {
                lstClientesSugerencias.ItemsSource = clientesFiltrados;
                lstClientesSugerencias.DisplayMemberPath = "NombreCompleto";
                popupSugerencias.IsOpen = true;
            }
            else
            {
                popupSugerencias.IsOpen = false;
            }
        }

        private void LstClientesSugerencias_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstClientesSugerencias.SelectedItem is Cliente clienteSeleccionado)
            {
                ViewModel.ClienteFiltro = clienteSeleccionado;
                txtBuscarCliente.Text = clienteSeleccionado.NombreCompleto;
                popupSugerencias.IsOpen = false;
                lstClientesSugerencias.SelectedItem = null;
            }
        }

        private void TxtBuscarCliente_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtBuscarCliente.Text) && txtBuscarCliente.Text.Length >= 2)
            {
                txtBuscarCliente_TextChanged(sender, null);
            }
        }

        private void TxtBuscarCliente_LostFocus(object sender, RoutedEventArgs e)
        {
            // El Popup se cierra automáticamente con StaysOpen="False"
        }

        private void BuscarVentas_Click(object sender, RoutedEventArgs e)
        {
            popupSugerencias.IsOpen = false;
            ViewModel.BuscarVentas();
        }

        private void ReimprimirComprobante_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ReimprimirComprobante();
        }

        private void AnularVenta_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AnularVenta();
        }

        private void VolverAVentas_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null && MainWindow.UsuarioActual != null)
            {
                mainWindow.MainContentBorder.Child = null;
                mainWindow.MainContentBorder.Child = new VentasView(MainWindow.UsuarioActual.id_usuario);
            }
        }
    }
}