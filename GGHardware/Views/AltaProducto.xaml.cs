using GGHardware.Data;
using GGHardware.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace GGHardware.Views
{
    public partial class AltaProducto : UserControl
    {
        public AltaProducto()
        {
            InitializeComponent();
            CargarCategorias();
        }

        private void CargarCategorias()
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var categorias = context.Categoria.ToList();
                    cmbCategoria.ItemsSource = categorias;
                    cmbCategoria.DisplayMemberPath = "nombre";
                    cmbCategoria.SelectedValuePath = "id_categoria"; 
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Error al cargar categorías: " + ex.Message);
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // 🔹 Validaciones básicas
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre del producto es obligatorio.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!double.TryParse(txtPrecioCosto.Text, out double precioCosto))
            {
                MessageBox.Show("El precio de costo debe ser un número válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!double.TryParse(txtPrecioVenta.Text, out double precioVenta))
            {
                MessageBox.Show("El precio de venta debe ser un número válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!double.TryParse(txtStock.Text, out double stock))
            {
                MessageBox.Show("El stock debe ser un número válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!double.TryParse(txtStockMin.Text, out double stockMin))
            {
                MessageBox.Show("El stock mínimo debe ser un número válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (cmbCategoria.SelectedIndex == -1)
            {
                MessageBox.Show("Debe seleccionar una categoría.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var producto = new Producto
                    {
                        Nombre = txtNombre.Text,
                        precio_costo = precioCosto,
                        precio_venta = precioVenta,
                        descripcion = txtDescripcion.Text,
                        Stock = stock,
                        stock_min = stockMin,
                        codigo_barras = txtCodigoBarras.Text,
                        codigo_interno = txtCodigoInterno.Text,
                        id_categoria = (int)cmbCategoria.SelectedValue, 
                        //fecha_creacion = DateTime.Now,
                        //activo = true
                    };

                    context.Producto.Add(producto);
                    context.SaveChanges();
                }

                MessageBox.Show("✅ Producto guardado con éxito.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                LimpiarCampos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error al guardar: {ex.InnerException?.Message ?? ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarCampos();
        }

        private void btnVolver_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainContentBorder.Child = new ProductoView();
            }
        }
        private void LimpiarCampos()
        {
            txtNombre.Text = string.Empty;
            txtPrecioCosto.Text = string.Empty;
            txtPrecioVenta.Text = string.Empty;
            txtDescripcion.Text = string.Empty;
            txtStock.Text = string.Empty;
            txtStockMin.Text = string.Empty;
            txtCodigoBarras.Text = string.Empty;
            txtCodigoInterno.Text = string.Empty;
            cmbCategoria.SelectedIndex = -1;
            txtNombre.Focus();
        }
    }
}
