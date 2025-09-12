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

            // Ejemplo: mostrar clientes en txtBuscarCliente
            txtBuscarCliente.TextChanged += (s, e) =>
            {
                // Podés filtrar aquí
            };
        }

        private void Agregar_Click(object sender, RoutedEventArgs e)
        {
            var producto = (Producto)dgProductos.SelectedItem;
            ViewModel.AgregarProducto(producto);
        }
    }
}