using GGHardware.Data;
using GGHardware.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;

namespace GGHardware.Views
{
    public partial class ProductoView : UserControl
    {
        public ProductoView()
        {
            InitializeComponent();
            CargarProductos();
        }

        private void btnAltaProducto_Click(object sender, RoutedEventArgs e)
        {
            // Cambia el contenido del MainContentBorder a la vista de AltaProducto
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainContentBorder.Child = new AltaProducto();
            }
        }

        private void CargarProductos()
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var productos = context.Producto.Include(p => p.Categoria).ToList();
                    dgProductos.ItemsSource = productos; // <-- Asigna la lista de objetos 'Producto'
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Error al cargar los productos: " + ex.Message);
            }
        }
        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is GGHardware.Models.Producto producto)
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.MainContentBorder.Child = new EditarProducto(producto.Id_Producto);
                }
            }
        }

        private void btnActivarDesactivar_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is GGHardware.Models.Producto producto)
            {
                try
                {
                    using (var context = new ApplicationDbContext())
                    {
                        var prod = context.Producto.Find(producto.Id_Producto);

                        if (prod != null)
                        {
                            prod.Activo = !prod.Activo;
                            context.SaveChanges();
                            CargarProductos(); // Recargar la lista
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cambiar el estado del producto:\n{ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
