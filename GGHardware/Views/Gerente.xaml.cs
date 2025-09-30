using ClosedXML.Excel;
using GGHardware.Data;
using GGHardware.Models;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
                // 1️⃣ Estadísticas generales
                decimal totalVentas = _context.Venta.Sum(v => (decimal?)v.Monto) ?? 0;
                txtTotalVentas.Text = "$" + totalVentas.ToString("N2");

                txtUsuariosActivos.Text = _context.Usuarios.Count(u => u.Activo).ToString();
                txtUsuariosInactivos.Text = _context.Usuarios.Count(u => !u.Activo).ToString();

                // Clientes frecuentes (clientes con más de 5 compras)
                int clientesFrecuentes = _context.Venta
                    .GroupBy(v => v.id_Cliente)
                    .Where(g => g.Count() >= 5)
                    .Count();
                txtClientesFrecuentes.Text = clientesFrecuentes.ToString();

                // 2️⃣ Productos más vendidos
                var productos = _context.DetalleVenta
                    .GroupBy(v => v.nombre_producto)
                    .Select(g => new ProductoVendidos
                    {
                        Nombre = g.Key,
                        Cantidad = g.Sum(v => v.cantidad)
                    })
                    .OrderByDescending(p => p.Cantidad)
                    .Take(10) // Top 10
                    .ToList();

                // 3️⃣ Cargar DataGrid
                dgProductosVendidos.ItemsSource = productos;

                // 4️⃣ Llenar gráfico con ProgressBars
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
                // Diálogo para guardar el archivo
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

        private void ExportarAExcelClosedXML(string rutaArchivo)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Reporte Gerencial");

                // Título
                worksheet.Cell("A1").Value = "REPORTE GERENCIAL - GGHARDWARE";
                worksheet.Range("A1:E1").Merge().Style.Font.Bold = true;

                // Estadísticas
                int row = 3;
                worksheet.Cell($"A{row}").Value = "Total Ventas";
                worksheet.Cell($"B{row}").Value = txtTotalVentas.Text;
                // ... agregar más datos

                workbook.SaveAs(rutaArchivo);
            }
        }
    }

    // Clase auxiliar para DataGrid
    public class ProductoVendidos
    {
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }

    // Clase auxiliar para gráfico de ProgressBars
    public class ProductoGrafico
    {
        public string Nombre { get; set; } = string.Empty;
        public double Cantidad { get; set; } // valor normalizado a 100
    }
}