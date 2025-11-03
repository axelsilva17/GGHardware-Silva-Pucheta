using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using GGHardware.Data;
using GGHardware.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GGHardware.Views
{
    public partial class Gerente : UserControl
    {
        private readonly ApplicationDbContext _context;
        private DateTime? fechaDesde;
        private DateTime? fechaHasta;
        private string categoriaSeleccionada;

        public Gerente()
        {
            InitializeComponent();
            _context = new ApplicationDbContext();

            // Inicializar fechas por defecto (últimos 7 días)
            fechaHasta = DateTime.Now;
            fechaDesde = fechaHasta.Value.AddDays(-7);
            categoriaSeleccionada = "Todas";

            // Cargar categorías 
            CargarCategorias();

            // Cargar datos iniciales
            CargarDatos();
        }

        private void CargarCategorias()
        {
            try
            {
                // Agregar "Todas" como primera opción
                cmbCategoria.Items.Clear();
                cmbCategoria.Items.Add(new ComboBoxItem { Content = "Todas" });

                // Obtener categorías únicas desde la tabla Categorias
                var categorias = _context.Categoria
                    .Select(c => c.nombre)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                foreach (var categoria in categorias)
                {
                    if (!string.IsNullOrEmpty(categoria))
                    {
                        cmbCategoria.Items.Add(new ComboBoxItem { Content = categoria });
                    }
                }

                cmbCategoria.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar categorías: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void cmbPeriodo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pnlFechasPersonalizadas == null || dpFechaDesde == null || dpFechaHasta == null)
                return;

            if (cmbPeriodo.SelectedIndex == 3) 
            {
                pnlFechasPersonalizadas.Visibility = Visibility.Visible;

                // Establecer fechas por defecto en los DatePickers
                dpFechaDesde.SelectedDate = fechaDesde;
                dpFechaHasta.SelectedDate = fechaHasta;
            }
            else
            {
                pnlFechasPersonalizadas.Visibility = Visibility.Collapsed;

                // Calcular fechas según selección
                fechaHasta = DateTime.Now;

                switch (cmbPeriodo.SelectedIndex)
                {
                    case 0: // Últimos 7 días
                        fechaDesde = fechaHasta.Value.AddDays(-7);
                        break;
                    case 1: // Último mes
                        fechaDesde = fechaHasta.Value.AddMonths(-1);
                        break;
                    case 2: // Último año
                        fechaDesde = fechaHasta.Value.AddYears(-1);
                        break;
                }
            }
        }

        private void cmbCategoria_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Verificar que el control esté inicializado
            if (cmbCategoria?.SelectedItem == null)
                return;

            if (cmbCategoria.SelectedItem is ComboBoxItem item)
            {
                categoriaSeleccionada = item.Content.ToString();
            }
        }

        private void btnAplicarFiltros_Click(object sender, RoutedEventArgs e)
        {
            // Si es personalizado, obtener fechas de los DatePickers
            if (cmbPeriodo.SelectedIndex == 3)
            {
                if (dpFechaDesde.SelectedDate.HasValue && dpFechaHasta.SelectedDate.HasValue)
                {
                    fechaDesde = dpFechaDesde.SelectedDate.Value;
                    fechaHasta = dpFechaHasta.SelectedDate.Value;

                    if (fechaDesde > fechaHasta)
                    {
                        MessageBox.Show("La fecha 'Desde' no puede ser mayor que la fecha 'Hasta'.",
                            "Error de Validación",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Debe seleccionar ambas fechas para el período personalizado.",
                        "Error de Validación",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
            }

            // Recargar datos con los filtros aplicados
            CargarDatos();
        }

        private void btnVerVendedores_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainContentBorder.Child = new RendimientoVendedoresView();
            }
        }

        private void CargarDatos()
        {
            try
            {
                // Obtener ventas filtradas por fecha
                var ventasFiltradas = _context.Venta
                    .Where(v => v.Fecha >= fechaDesde && v.Fecha <= fechaHasta)
                    .ToList();

                // Obtener IDs de ventas filtradas
                var idsVentasFiltradas = ventasFiltradas.Select(v => v.id_venta).ToList();

                // Obtener detalles de venta filtrados
                var detallesFiltrados = _context.DetalleVenta
                    .Where(dv => idsVentasFiltradas.Contains(dv.id_venta))
                    .ToList();

                // Aplicar filtro de categoría si no es "Todas"
                if (categoriaSeleccionada != "Todas")
                {
                    // Obtener el ID de la categoría seleccionada
                    var categoriaObj = _context.Categoria
                        .FirstOrDefault(c => c.nombre == categoriaSeleccionada);

                    if (categoriaObj != null)
                    {
                        // CORREGIDO: Obtener los IDs de PRODUCTOS (no categorías)
                        var productosCategoria = _context.Producto
                            .Where(p => p.id_categoria == categoriaObj.id_categoria)
                            .Select(p => p.Id_Producto)  // ← AQUÍ ESTABA EL ERROR
                            .ToList();

                        detallesFiltrados = detallesFiltrados
                            .Where(dv => productosCategoria.Contains(dv.id_producto))
                            .ToList();

                        // Recalcular ventas según productos de la categoría
                        var ventasConCategoria = detallesFiltrados.Select(dv => dv.id_venta).Distinct().ToList();
                        ventasFiltradas = ventasFiltradas.Where(v => ventasConCategoria.Contains(v.id_venta)).ToList();
                    }
                }

                // Estadísticas generales (con filtros aplicados)
                decimal totalVentas = ventasFiltradas.Sum(v => (decimal?)v.Monto) ?? 0;
                txtTotalVentas.Text = "$" + totalVentas.ToString("N2");

                // Usuarios activos e inactivos (sin filtro de fecha)
                txtUsuariosActivos.Text = _context.Usuarios.Count(u => u.Activo).ToString();
                txtUsuariosInactivos.Text = _context.Usuarios.Count(u => !u.Activo).ToString();

                // Clientes frecuentes (con filtros aplicados)
                int clientesFrecuentes = ventasFiltradas
                    .GroupBy(v => v.id_Cliente)
                    .Where(g => g.Count() >= 5)
                    .Count();
                txtClientesFrecuentes.Text = clientesFrecuentes.ToString();

                // Productos más vendidos (con filtros aplicados)
                var productos = detallesFiltrados
                    .GroupBy(v => v.nombre_producto)
                    .Select(g => new ProductoVendidos
                    {
                        Nombre = g.Key,
                        Cantidad = g.Sum(v => v.cantidad)
                    })
                    .OrderByDescending(p => p.Cantidad)
                    .Take(10)
                    .ToList();

                dgProductosVendidos.ItemsSource = productos;

                // Cargar clientes frecuentes con detalle (con filtros aplicados)
                var clientesFrecuentesDetalle = ventasFiltradas
                    .GroupBy(v => v.id_Cliente)
                    .Where(g => g.Count() >= 5)
                    .Select(g => new
                    {
                        ClienteId = g.Key,
                        TotalCompras = g.Count(),
                        MontoTotal = g.Sum(v => v.Monto)
                    })
                    .OrderByDescending(c => c.TotalCompras)
                    .ToList()
                    .Select(c => new
                    {
                        Nombre = _context.Clientes.Find(c.ClienteId)?.nombre ?? "Sin nombre",
                        TotalCompras = c.TotalCompras,
                        MontoTotal = "$" + c.MontoTotal.ToString("N2")
                    })
                    .ToList();

                dgClientesFrecuentes.ItemsSource = clientesFrecuentesDetalle;

                // Llenar gráfico con ProgressBars (con filtros aplicados)
                if (productos.Any())
                {
                    int maxCantidad = productos.Max(p => p.Cantidad);
                    var barras = productos.Select(p => new ProductoGrafico
                    {
                        Nombre = p.Nombre,
                        Cantidad = maxCantidad > 0 ? (double)p.Cantidad / maxCantidad * 100 : 0
                    }).ToList();

                    icGraficoProductos.ItemsSource = barras;
                }
                else
                {
                    icGraficoProductos.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void btnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    Title = "Guardar reporte gerencial en Excel",
                    FileName = $"Reporte_Gerencial_{DateTime.Now:yyyy-MM-dd_HHmm}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    ExportarAExcelClosedXML(saveFileDialog.FileName);
                    MessageBox.Show("Reporte exportado exitosamente.",
                        "Exportación Exitosa",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void btnActualizar_Click(object sender, RoutedEventArgs e)
        {
            CargarDatos();
        }

        private void ExportarAExcelClosedXML(string rutaArchivo)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Reporte Gerencial");

                // TÍTULO PRINCIPAL
                worksheet.Cell("A1").Value = "REPORTE GERENCIAL - GGHARDWARE";
                worksheet.Range("A1:F1").Merge();
                worksheet.Range("A1:F1").Style
                    .Font.SetBold(true)
                    .Font.SetFontSize(16)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#0097A7"))
                    .Font.SetFontColor(XLColor.White);

                worksheet.Cell("A2").Value = $"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm}";
                worksheet.Range("A2:F2").Merge();
                worksheet.Cell("A2").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                // Información de filtros aplicados
                worksheet.Cell("A3").Value = $"Período: {fechaDesde:dd/MM/yyyy} - {fechaHasta:dd/MM/yyyy} | Categoría: {categoriaSeleccionada}";
                worksheet.Range("A3:F3").Merge();
                worksheet.Cell("A3").Style
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Font.SetItalic(true);

                // ESTADÍSTICAS GENERALES
                int row = 5;
                worksheet.Cell($"A{row}").Value = "ESTADÍSTICAS GENERALES";
                worksheet.Range($"A{row}:B{row}").Merge();
                worksheet.Cell($"A{row}").Style.Font.SetBold(true).Font.SetFontSize(14);

                row += 2;
                worksheet.Cell($"A{row}").Value = "Total Ventas:";
                worksheet.Cell($"B{row}").Value = txtTotalVentas.Text;
                worksheet.Cell($"A{row}").Style.Font.SetBold(true);

                row++;
                worksheet.Cell($"A{row}").Value = "Usuarios Activos:";
                worksheet.Cell($"B{row}").Value = txtUsuariosActivos.Text;
                worksheet.Cell($"A{row}").Style.Font.SetBold(true);

                row++;
                worksheet.Cell($"A{row}").Value = "Usuarios Inactivos:";
                worksheet.Cell($"B{row}").Value = txtUsuariosInactivos.Text;
                worksheet.Cell($"A{row}").Style.Font.SetBold(true);

                row++;
                worksheet.Cell($"A{row}").Value = "Clientes Frecuentes (5+ compras):";
                worksheet.Cell($"B{row}").Value = txtClientesFrecuentes.Text;
                worksheet.Cell($"A{row}").Style.Font.SetBold(true);

                // TOP PRODUCTOS VENDIDOS
                row += 3;
                worksheet.Cell($"A{row}").Value = "TOP 10 PRODUCTOS MÁS VENDIDOS";
                worksheet.Range($"A{row}:C{row}").Merge();
                worksheet.Cell($"A{row}").Style.Font.SetBold(true).Font.SetFontSize(14);

                row += 2;
                worksheet.Cell($"A{row}").Value = "Posición";
                worksheet.Cell($"B{row}").Value = "Producto";
                worksheet.Cell($"C{row}").Value = "Cantidad Vendida";

                var headerRange = worksheet.Range($"A{row}:C{row}");
                headerRange.Style
                    .Font.SetBold(true)
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#00BCD4"))
                    .Font.SetFontColor(XLColor.White)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                var productos = dgProductosVendidos.ItemsSource as List<ProductoVendidos>;
                if (productos != null && productos.Any())
                {
                    int posicion = 1;
                    foreach (var producto in productos)
                    {
                        row++;
                        worksheet.Cell($"A{row}").Value = posicion++;
                        worksheet.Cell($"B{row}").Value = producto.Nombre;
                        worksheet.Cell($"C{row}").Value = producto.Cantidad;

                        worksheet.Cell($"A{row}").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        worksheet.Cell($"C{row}").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    }
                }

                // DETALLE DE CLIENTES FRECUENTES
                row += 3;
                worksheet.Cell($"A{row}").Value = "CLIENTES FRECUENTES (DETALLE)";
                worksheet.Range($"A{row}:D{row}").Merge();
                worksheet.Cell($"A{row}").Style.Font.SetBold(true).Font.SetFontSize(14);

                row += 2;
                worksheet.Cell($"A{row}").Value = "Cliente";
                worksheet.Cell($"B{row}").Value = "Email";
                worksheet.Cell($"C{row}").Value = "Total Compras";
                worksheet.Cell($"D{row}").Value = "Monto Total";

                var headerClientes = worksheet.Range($"A{row}:D{row}");
                headerClientes.Style
                    .Font.SetBold(true)
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#FF9800"))
                    .Font.SetFontColor(XLColor.White)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                // Obtener ventas filtradas
                var ventasFiltradas = _context.Venta
                    .Where(v => v.Fecha >= fechaDesde && v.Fecha <= fechaHasta)
                    .ToList();

                var clientesFrecuentes = ventasFiltradas
                    .GroupBy(v => v.id_Cliente)
                    .Where(g => g.Count() >= 5)
                    .Select(g => new
                    {
                        ClienteId = g.Key,
                        TotalCompras = g.Count(),
                        MontoTotal = g.Sum(v => v.Monto)
                    })
                    .OrderByDescending(c => c.TotalCompras)
                    .ToList();

                foreach (var cliente in clientesFrecuentes)
                {
                    row++;
                    var clienteInfo = _context.Clientes.Find(cliente.ClienteId);

                    worksheet.Cell($"A{row}").Value = clienteInfo?.nombre ?? "Sin nombre";
                    worksheet.Cell($"B{row}").Value = clienteInfo?.email ?? "Sin email";
                    worksheet.Cell($"C{row}").Value = cliente.TotalCompras;
                    worksheet.Cell($"D{row}").Value = "$" + cliente.MontoTotal.ToString("N2");

                    worksheet.Cell($"C{row}").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    worksheet.Cell($"D{row}").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                }

                // RENDIMIENTO DE VENDEDORES
                row += 3;
                worksheet.Cell($"A{row}").Value = "RENDIMIENTO DE VENDEDORES";
                worksheet.Range($"A{row}:E{row}").Merge();
                worksheet.Cell($"A{row}").Style.Font.SetBold(true).Font.SetFontSize(14);

                row += 2;
                worksheet.Cell($"A{row}").Value = "Posición";
                worksheet.Cell($"B{row}").Value = "Vendedor";
                worksheet.Cell($"C{row}").Value = "Cant. Ventas";
                worksheet.Cell($"D{row}").Value = "Monto Total";
                worksheet.Cell($"E{row}").Value = "Promedio por Venta";

                var headerVendedores = worksheet.Range($"A{row}:E{row}");
                headerVendedores.Style
                    .Font.SetBold(true)
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#4CAF50"))
                    .Font.SetFontColor(XLColor.White)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                // Obtener rendimiento de vendedores
                var rendimientoVendedores = _context.Usuarios
                    .Where(u => u.Activo && u.RolId == 2) // Solo vendedores activos (ajusta el RolId según tu sistema)
                    .Select(u => new
                    {
                        NombreVendedor = u.Nombre + " " + u.apellido,
                        CantidadVentas = _context.Venta
                            .Count(v => v.id_Usuario == u.id_usuario
                                     && v.Estado != "Anulada"
                                     && v.Fecha >= fechaDesde
                                     && v.Fecha <= fechaHasta),
                        MontoTotal = _context.Venta
                            .Where(v => v.id_Usuario == u.id_usuario
                                     && v.Estado != "Anulada"
                                     && v.Fecha >= fechaDesde
                                     && v.Fecha <= fechaHasta)
                            .Sum(v => (decimal?)v.Monto) ?? 0
                    })
                    .ToList()
                    .Select(v => new
                    {
                        v.NombreVendedor,
                        v.CantidadVentas,
                        v.MontoTotal,
                        PromedioVenta = v.CantidadVentas > 0 ? v.MontoTotal / v.CantidadVentas : 0
                    })
                    .OrderByDescending(v => v.MontoTotal)
                    .ToList();

                int posicionVendedor = 1;
                foreach (var vendedor in rendimientoVendedores)
                {
                    row++;
                    worksheet.Cell($"A{row}").Value = posicionVendedor++;
                    worksheet.Cell($"B{row}").Value = vendedor.NombreVendedor;
                    worksheet.Cell($"C{row}").Value = vendedor.CantidadVentas;
                    worksheet.Cell($"D{row}").Value = "$" + vendedor.MontoTotal.ToString("N2");
                    worksheet.Cell($"E{row}").Value = "$" + vendedor.PromedioVenta.ToString("N2");

                    worksheet.Cell($"A{row}").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    worksheet.Cell($"C{row}").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    worksheet.Cell($"D{row}").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                    worksheet.Cell($"E{row}").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

                    // Resaltar al mejor vendedor
                    if (posicionVendedor == 2) // Primer lugar
                    {
                        worksheet.Range($"A{row}:E{row}").Style
                            .Fill.SetBackgroundColor(XLColor.FromHtml("#FFD700")) // Dorado
                            .Font.SetBold(true);
                    }
                }

                // Agregar totales de vendedores
                if (rendimientoVendedores.Any())
                {
                    row += 2;
                    worksheet.Cell($"A{row}").Value = "TOTALES";
                    worksheet.Cell($"A{row}").Style.Font.SetBold(true);

                    worksheet.Cell($"C{row}").Value = rendimientoVendedores.Sum(v => v.CantidadVentas);
                    worksheet.Cell($"D{row}").Value = "$" + rendimientoVendedores.Sum(v => v.MontoTotal).ToString("N2");

                    var promedioGeneral = rendimientoVendedores.Sum(v => v.CantidadVentas) > 0
                        ? rendimientoVendedores.Sum(v => v.MontoTotal) / rendimientoVendedores.Sum(v => v.CantidadVentas)
                        : 0;
                    worksheet.Cell($"E{row}").Value = "$" + promedioGeneral.ToString("N2");

                    worksheet.Range($"A{row}:E{row}").Style
                        .Font.SetBold(true)
                        .Fill.SetBackgroundColor(XLColor.LightGray);

                    worksheet.Cell($"C{row}").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    worksheet.Cell($"D{row}").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                    worksheet.Cell($"E{row}").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                }


                worksheet.Columns().AdjustToContents();
                // PROTEGER LA HOJA PARA QUE SEA DE SOLO LECTURA
                worksheet.Protect("GGHardware2024");
                workbook.SaveAs(rutaArchivo);
            }
        }
    }

    public class ProductoVendidos
    {
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }

    public class ProductoGrafico
    {
        public string Nombre { get; set; } = string.Empty;
        public double Cantidad { get; set; }
    }
}