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

        public VentasView()
        {
            InitializeComponent();
            ViewModel = new VentasViewModel();
            this.DataContext = ViewModel;

            // Ejemplo: manejar cambios para filtrar clientes
            txtBuscarCliente.TextChanged += (s, e) =>
            {
                if (txtBuscarCliente.Text != "Buscar Cliente (DNI/Nombre)")
                {
                    // Aquí podés filtrar tu lista de clientes en ViewModel
                    ViewModel.FiltrarClientes(txtBuscarCliente.Text);
                }
            };
        }

        private void TxtBuscarCliente_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtBuscarCliente.Text == "Buscar Cliente (DNI/Nombre)")
            {
                txtBuscarCliente.Text = "";
                txtBuscarCliente.Foreground = Brushes.Black;
            }
        }

        private void TxtBuscarCliente_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBuscarCliente.Text))
            {
                txtBuscarCliente.Text = "Buscar Cliente (DNI/Nombre)";
                txtBuscarCliente.Foreground = Brushes.Gray;
            }
        }

        private void Agregar_Click(object sender, RoutedEventArgs e)
        {
            if (dgProductos.SelectedItem is Producto producto)
            {
                ViewModel.AgregarProducto(producto);
            }
        }

        private void txtBuscarCliente_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
