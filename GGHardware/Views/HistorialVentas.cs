using System.Windows;
using System.Windows.Controls;
using GGHardware.ViewModels;

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

        private void BuscarVentas_Click(object sender, RoutedEventArgs e)
        {
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
    }
}