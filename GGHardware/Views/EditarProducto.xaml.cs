using GGHardware.Data;
using GGHardware.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace GGHardware.Views
{
    public partial class EditarProducto : UserControl
    {
        private int _idProducto;

        public EditarProducto(int idProducto)
        {
            InitializeComponent();
            _idProducto = idProducto;
            CargarDatosProducto(idProducto);
        }

        private void CargarDatosProducto(int idProducto)
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    // Obtener el producto, incluyendo la categoría
                    var producto = context.Producto
                                          .Include(p => p.Categoria)
                                          .FirstOrDefault(p => p.Id_Producto == idProducto);

                    if (producto != null)
                    {
                        // Cargar las categorías en el ComboBox
                        var categorias = context.Categoria.ToList();
                        cmbCategoria.ItemsSource = categorias;
                        cmbCategoria.DisplayMemberPath = "nombre";
                        cmbCategoria.SelectedValuePath = "id_categoria";

                        var proveedores = context.Proveedores.ToList();
                        cmbProveedor.ItemsSource = proveedores;
                        cmbProveedor.DisplayMemberPath = "razon_social";
                        cmbProveedor.SelectedValuePath = "id_proveedor";

                        // Rellenar los campos del formulario con los datos del producto
                        txtIdProducto.Text = producto.Id_Producto.ToString();
                        txtNombre.Text = producto.Nombre;
                        txtDescripcion.Text = producto.descripcion;
                        txtPrecioCosto.Text = producto.precio_costo.ToString("F2", CultureInfo.InvariantCulture);
                        txtPrecioVenta.Text = producto.precio_venta.ToString("F2", CultureInfo.InvariantCulture);
                        txtStock.Text = producto.Stock.ToString("F2", CultureInfo.InvariantCulture);
                        txtStockMin.Text = producto.stock_min.ToString("F2", CultureInfo.InvariantCulture);
                        txtCodigoBarras.Text = producto.codigo_barras;
                        txtCodigoInterno.Text = producto.codigo_interno;

                        // Seleccionar la categoría del producto en el ComboBox
                        if (producto.id_categoria != 0)
                        {
                            cmbCategoria.SelectedValue = producto.id_categoria;
                        }

                        if (producto.id_proveedor != 0)
                        { 
                            cmbProveedor.SelectedValue = producto.id_proveedor;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Producto no encontrado.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        
                        var mainWindow = Window.GetWindow(this) as MainWindow;
                        if (mainWindow != null)
                        {
                            mainWindow.MainContentBorder.Child = new ProductoView();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Error al cargar los datos del producto: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnGuardarCambios_Click(object sender, RoutedEventArgs e)
        {
            // Validar que los campos no estén vacíos
            if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtPrecioCosto.Text) ||
                string.IsNullOrWhiteSpace(txtPrecioVenta.Text) ||
                string.IsNullOrWhiteSpace(txtStock.Text) ||
                string.IsNullOrWhiteSpace(txtStockMin.Text) ||
                cmbCategoria.SelectedValue == null || cmbProveedor.SelectedValue == null)
            {
                MessageBox.Show("Por favor, completa todos los campos obligatorios.", "Error de validación", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validar que los campos numéricos sean válidos
            if (!float.TryParse(txtPrecioCosto.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out float precioCosto) ||
                !float.TryParse(txtPrecioVenta.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out float precioVenta) ||
                !float.TryParse(txtStock.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out float stock) ||
                !float.TryParse(txtStockMin.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out float stockMin))
            {
                MessageBox.Show("El precio, stock y stock mínimo deben ser números válidos.", "Error de formato", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (var context = new ApplicationDbContext())
                {
                    // Buscar el producto a actualizar en el contexto
                    var producto = context.Producto.FirstOrDefault(p => p.Id_Producto == _idProducto);

                    if (producto != null)
                    {
                        // Actualizar las propiedades del producto
                        producto.Nombre = txtNombre.Text;
                        producto.descripcion = txtDescripcion.Text;
                        producto.precio_costo = (decimal)precioCosto;
                        producto.precio_venta = (decimal)precioVenta;
                        producto.Stock = (int)stock;
                        producto.stock_min = (int)stockMin;
                        producto.codigo_barras = txtCodigoBarras.Text;
                        producto.codigo_interno = txtCodigoInterno.Text;
                        producto.id_categoria = (int)cmbCategoria.SelectedValue;
                        producto.id_proveedor = (int)cmbProveedor.SelectedValue;

                        context.SaveChanges();

                        MessageBox.Show("✅ Producto actualizado con éxito.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Volver a la vista de productos después de guardar
                        var mainWindow = Window.GetWindow(this) as MainWindow;
                        if (mainWindow != null)
                        {
                            mainWindow.MainContentBorder.Child = new ProductoView();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error al actualizar el producto: {ex.InnerException?.Message ?? ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnVolver_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainContentBorder.Child = new ProductoView();
            }
        }
    }
}