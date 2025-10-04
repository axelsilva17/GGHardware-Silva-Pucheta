using ClosedXML.Excel;
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

        public Gerente()
        {
            InitializeComponent();
            _context = new ApplicationDbContext();
            CargarDatos();
        }

        private void CargarDatos()
        {
            try
            {
                // Estadísticas generales
                decimal totalVentas = _context.Venta.Sum(v => (decimal?)v.Monto) ?? 0;
                txtTotalVentas.Text = "$" + totalVentas.ToString("N2");

                txtUsuariosActivos.Text = _context.Usuarios.Count(u => u.Activo).ToString();
                txtUsuariosInactivos.Text = _context.Usuarios.Count(u => !u.Activo).ToString();

                // Clientes frecuentes
                int clientesFrecuentes = _context.Venta
                    .GroupBy(v => v.id_Cliente)
                    .Where(g => g.Count() >= 5)
                    .Count();
                txtClientesFrecuentes.Text = clientesFrecuentes.ToString();

                // Productos más vendidos
                var productos = _context.DetalleVenta
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

                // Cargar clientes frecuentes con detalle
                var clientesFrecuentesDetalle = _context.Venta
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

                // Llenar gráfico con ProgressBars
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

                // ESTADÍSTICAS GENERALES
                int row = 4;
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

                var clientesFrecuentes = _context.Venta
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

                worksheet.Columns().AdjustToContents();
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