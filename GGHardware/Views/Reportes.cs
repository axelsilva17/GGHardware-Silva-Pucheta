using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GGHardware.Data;
using GGHardware.Models;
using System.Collections.Generic;
using ClosedXML.Excel;
using Microsoft.Win32;

namespace GGHardware.Views
{
    public partial class Reportes : UserControl
    {
        private List<ReporteVentaDetalle> _todasLasVentas;

        public Reportes()
        {
            InitializeComponent();

            // Establecer fechas por defecto (todo hasta hoy)
            dpFechaInicio.SelectedDate = null; // Sin fecha inicio = desde el principio
            dpFechaFin.SelectedDate = DateTime.Today;

            this.Loaded += Reportes_Loaded;
        }

        private void Reportes_Loaded(object sender, RoutedEventArgs e)
        {
            CargarTodasLasVentas();
            AplicarFiltro();
        }

        private void btnAplicarFiltro_Click(object sender, RoutedEventArgs e)
        {
            AplicarFiltro();
        }

        private void CargarTodasLasVentas()
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    _todasLasVentas = (from dv in context.DetalleVenta
                                       join v in context.Venta on dv.id_venta equals v.id_venta
                                       join c in context.Clientes on v.id_Cliente equals c.id_cliente
                                       join p in context.Producto on dv.id_producto equals p.Id_Producto
                                       orderby v.Fecha descending
                                       select new ReporteVentaDetalle
                                       {
                                           Fecha = v.Fecha,
                                           Cliente = c.nombre + " " + c.apellido,
                                           Producto = dv.nombre_producto,
                                           Cantidad = dv.cantidad,
                                           Total = (dv.precio_con_descuento ?? dv.precio_unitario) * dv.cantidad
                                       }).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar ventas: {ex.Message}\n\nDetalle: {ex.InnerException?.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _todasLasVentas = new List<ReporteVentaDetalle>();
            }
        }

        private void AplicarFiltro()
        {
            try
            {
                if (_todasLasVentas == null || !_todasLasVentas.Any())
                {
                    MostrarSinDatos();
                    return;
                }

                // Filtrar por fechas
                var ventasFiltradas = _todasLasVentas.AsEnumerable();

                // Si hay fecha de inicio, filtrar
                if (dpFechaInicio.SelectedDate.HasValue)
                {
                    DateTime fechaInicio = dpFechaInicio.SelectedDate.Value.Date;
                    ventasFiltradas = ventasFiltradas.Where(v => v.Fecha >= fechaInicio);
                }

                // Si hay fecha fin, filtrar
                if (dpFechaFin.SelectedDate.HasValue)
                {
                    DateTime fechaFin = dpFechaFin.SelectedDate.Value.Date.AddDays(1).AddTicks(-1);
                    ventasFiltradas = ventasFiltradas.Where(v => v.Fecha <= fechaFin);
                }

                var listaFiltrada = ventasFiltradas.ToList();

                if (!listaFiltrada.Any())
                {
                    MostrarSinDatos();

                    string periodo = "";
                    if (dpFechaInicio.SelectedDate.HasValue && dpFechaFin.SelectedDate.HasValue)
                    {
                        periodo = $"Período: {dpFechaInicio.SelectedDate.Value:dd/MM/yyyy} - {dpFechaFin.SelectedDate.Value:dd/MM/yyyy}";
                    }
                    else if (dpFechaInicio.SelectedDate.HasValue)
                    {
                        periodo = $"Desde: {dpFechaInicio.SelectedDate.Value:dd/MM/yyyy}";
                    }
                    else if (dpFechaFin.SelectedDate.HasValue)
                    {
                        periodo = $"Hasta: {dpFechaFin.SelectedDate.Value:dd/MM/yyyy}";
                    }

                    MessageBox.Show($"No se encontraron ventas en el período seleccionado.\n\n{periodo}",
                        "Sin resultados", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Calcular resumen
                var resumen = new ResumenVentas
                {
                    VentasTotales = listaFiltrada.Sum(r => r.Total),
                    ProductosVendidos = listaFiltrada.Sum(r => r.Cantidad),
                    ClientesNuevos = listaFiltrada.Select(r => r.Cliente).Distinct().Count(),
                    TotalRegistros = listaFiltrada.Count
                };

                // Mostrar datos
                dgReporte.ItemsSource = listaFiltrada;
                txtVentasDia.Text = $"${resumen.VentasTotales:N2}";
                txtProductosVendidos.Text = resumen.ProductosVendidos.ToString();
                txtClientesNuevos.Text = resumen.ClientesNuevos.ToString();
                txtRegistrosTotal.Text = $"Total de registros: {resumen.TotalRegistros}";
                txtTotalGeneral.Text = $"${resumen.VentasTotales:N2}";

                // Guardar reporte solo cuando hay datos
                if (dpFechaInicio.SelectedDate.HasValue && dpFechaFin.SelectedDate.HasValue)
                {
                    using (var context = new ApplicationDbContext())
                    {
                        GuardarReporteEnBD(context, dpFechaInicio.SelectedDate.Value, dpFechaFin.SelectedDate.Value, resumen);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al filtrar: {ex.Message}\n\nDetalle: {ex.InnerException?.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MostrarSinDatos()
        {
            dgReporte.ItemsSource = null;
            txtVentasDia.Text = "$0.00";
            txtProductosVendidos.Text = "0";
            txtClientesNuevos.Text = "0";
            txtRegistrosTotal.Text = "Total de registros: 0";
            txtTotalGeneral.Text = "$0.00";
        }

        private void GuardarReporteEnBD(ApplicationDbContext context, DateTime fechaInicio, DateTime fechaFin, ResumenVentas resumen)
        {
            try
            {
                var reporte = new Reporte
                {
                    TipoReporte = "VentasPeriodo",
                    FechaGeneracion = DateTime.Now,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    UsuarioID = 1,
                    TotalVentas = resumen.VentasTotales,
                    TotalProductos = resumen.ProductosVendidos,
                    TotalRegistros = resumen.TotalRegistros,
                    Observaciones = $"Reporte generado desde {fechaInicio:dd/MM/yyyy} hasta {fechaFin:dd/MM/yyyy}",
                    Activo = true
                };

                context.Reportes.Add(reporte);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar reporte: {ex.Message}");
            }
        }

        private void btnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validar que haya datos para exportar
                if (dgReporte.ItemsSource == null || !((List<ReporteVentaDetalle>)dgReporte.ItemsSource).Any())
                {
                    MessageBox.Show("No hay datos para exportar. Por favor, aplique primero los filtros.",
                        "Sin Datos",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    Title = "Guardar reporte de ventas en Excel",
                    FileName = $"Reporte_Ventas_{DateTime.Now:yyyy-MM-dd_HHmm}.xlsx"
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
                var worksheet = workbook.Worksheets.Add("Reporte de Ventas");

                // TÍTULO PRINCIPAL
                worksheet.Cell("A1").Value = "REPORTE DE VENTAS - GGHARDWARE";
                worksheet.Range("A1:E1").Merge();
                worksheet.Range("A1:E1").Style
                    .Font.SetBold(true)
                    .Font.SetFontSize(16)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#0097A7"))
                    .Font.SetFontColor(XLColor.White);

                worksheet.Cell("A2").Value = $"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm}";
                worksheet.Range("A2:E2").Merge();
                worksheet.Cell("A2").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                // Información del período
                string periodo = "";
                if (dpFechaInicio.SelectedDate.HasValue && dpFechaFin.SelectedDate.HasValue)
                {
                    periodo = $"Período: {dpFechaInicio.SelectedDate.Value:dd/MM/yyyy} - {dpFechaFin.SelectedDate.Value:dd/MM/yyyy}";
                }
                else if (dpFechaInicio.SelectedDate.HasValue)
                {
                    periodo = $"Desde: {dpFechaInicio.SelectedDate.Value:dd/MM/yyyy}";
                }
                else if (dpFechaFin.SelectedDate.HasValue)
                {
                    periodo = $"Hasta: {dpFechaFin.SelectedDate.Value:dd/MM/yyyy}";
                }
                else
                {
                    periodo = "Todas las fechas";
                }

                worksheet.Cell("A3").Value = periodo;
                worksheet.Range("A3:E3").Merge();
                worksheet.Cell("A3").Style
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Font.SetItalic(true);

                // RESUMEN GENERAL
                int row = 5;
                worksheet.Cell($"A{row}").Value = "RESUMEN GENERAL";
                worksheet.Range($"A{row}:B{row}").Merge();
                worksheet.Cell($"A{row}").Style.Font.SetBold(true).Font.SetFontSize(14);

                row += 2;
                worksheet.Cell($"A{row}").Value = "Total Ventas:";
                worksheet.Cell($"B{row}").Value = txtVentasDia.Text;
                worksheet.Cell($"A{row}").Style.Font.SetBold(true);

                row++;
                worksheet.Cell($"A{row}").Value = "Productos Vendidos:";
                worksheet.Cell($"B{row}").Value = txtProductosVendidos.Text;
                worksheet.Cell($"A{row}").Style.Font.SetBold(true);

                row++;
                worksheet.Cell($"A{row}").Value = "Clientes Únicos:";
                worksheet.Cell($"B{row}").Value = txtClientesNuevos.Text;
                worksheet.Cell($"A{row}").Style.Font.SetBold(true);

                row++;
                worksheet.Cell($"A{row}").Value = "Total de Registros:";
                worksheet.Cell($"B{row}").Value = txtRegistrosTotal.Text.Replace("Total de registros: ", "");
                worksheet.Cell($"A{row}").Style.Font.SetBold(true);

                // DETALLE DE VENTAS
                row += 3;
                worksheet.Cell($"A{row}").Value = "DETALLE DE VENTAS";
                worksheet.Range($"A{row}:E{row}").Merge();
                worksheet.Cell($"A{row}").Style.Font.SetBold(true).Font.SetFontSize(14);

                row += 2;
                worksheet.Cell($"A{row}").Value = "Fecha";
                worksheet.Cell($"B{row}").Value = "Cliente";
                worksheet.Cell($"C{row}").Value = "Producto";
                worksheet.Cell($"D{row}").Value = "Cantidad";
                worksheet.Cell($"E{row}").Value = "Total";

                var headerRange = worksheet.Range($"A{row}:E{row}");
                headerRange.Style
                    .Font.SetBold(true)
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#00BCD4"))
                    .Font.SetFontColor(XLColor.White)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                // Datos de ventas
                var ventas = dgReporte.ItemsSource as List<ReporteVentaDetalle>;
                if (ventas != null && ventas.Any())
                {
                    foreach (var venta in ventas)
                    {
                        row++;
                        worksheet.Cell($"A{row}").Value = venta.Fecha.ToString("dd/MM/yyyy");
                        worksheet.Cell($"B{row}").Value = venta.Cliente;
                        worksheet.Cell($"C{row}").Value = venta.Producto;
                        worksheet.Cell($"D{row}").Value = venta.Cantidad;
                        worksheet.Cell($"E{row}").Value = "$" + venta.Total.ToString("N2");

                        worksheet.Cell($"A{row}").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        worksheet.Cell($"D{row}").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        worksheet.Cell($"E{row}").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                    }

                    // Total general
                    row += 2;
                    worksheet.Cell($"D{row}").Value = "TOTAL:";
                    worksheet.Cell($"D{row}").Style.Font.SetBold(true);
                    worksheet.Cell($"E{row}").Value = txtTotalGeneral.Text;
                    worksheet.Cell($"E{row}").Style
                        .Font.SetBold(true)
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right)
                        .Fill.SetBackgroundColor(XLColor.LightGray);
                }

                // TOP 10 PRODUCTOS MÁS VENDIDOS
                row += 3;
                worksheet.Cell($"A{row}").Value = "TOP 10 PRODUCTOS MÁS VENDIDOS";
                worksheet.Range($"A{row}:C{row}").Merge();
                worksheet.Cell($"A{row}").Style.Font.SetBold(true).Font.SetFontSize(14);

                row += 2;
                worksheet.Cell($"A{row}").Value = "Posición";
                worksheet.Cell($"B{row}").Value = "Producto";
                worksheet.Cell($"C{row}").Value = "Cantidad Total";

                var headerProductos = worksheet.Range($"A{row}:C{row}");
                headerProductos.Style
                    .Font.SetBold(true)
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#FF9800"))
                    .Font.SetFontColor(XLColor.White)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                if (ventas != null && ventas.Any())
                {
                    var topProductos = ventas
                        .GroupBy(v => v.Producto)
                        .Select(g => new
                        {
                            Producto = g.Key,
                            CantidadTotal = g.Sum(v => v.Cantidad)
                        })
                        .OrderByDescending(p => p.CantidadTotal)
                        .Take(10)
                        .ToList();

                    int posicion = 1;
                    foreach (var producto in topProductos)
                    {
                        row++;
                        worksheet.Cell($"A{row}").Value = posicion++;
                        worksheet.Cell($"B{row}").Value = producto.Producto;
                        worksheet.Cell($"C{row}").Value = producto.CantidadTotal;

                        worksheet.Cell($"A{row}").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        worksheet.Cell($"C{row}").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                        // Resaltar el top 3
                        if (posicion <= 4)
                        {
                            var color = posicion == 2 ? "#FFD700" : posicion == 3 ? "#C0C0C0" : "#CD7F32";
                            worksheet.Range($"A{row}:C{row}").Style
                                .Fill.SetBackgroundColor(XLColor.FromHtml(color))
                                .Font.SetBold(true);
                        }
                    }
                }

                // TOP 10 CLIENTES
                row += 3;
                worksheet.Cell($"A{row}").Value = "TOP 10 CLIENTES POR VOLUMEN DE COMPRA";
                worksheet.Range($"A{row}:D{row}").Merge();
                worksheet.Cell($"A{row}").Style.Font.SetBold(true).Font.SetFontSize(14);

                row += 2;
                worksheet.Cell($"A{row}").Value = "Posición";
                worksheet.Cell($"B{row}").Value = "Cliente";
                worksheet.Cell($"C{row}").Value = "Cant. Compras";
                worksheet.Cell($"D{row}").Value = "Total Gastado";

                var headerClientes = worksheet.Range($"A{row}:D{row}");
                headerClientes.Style
                    .Font.SetBold(true)
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#4CAF50"))
                    .Font.SetFontColor(XLColor.White)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                if (ventas != null && ventas.Any())
                {
                    var topClientes = ventas
                        .GroupBy(v => v.Cliente)
                        .Select(g => new
                        {
                            Cliente = g.Key,
                            CantidadCompras = g.Count(),
                            TotalGastado = g.Sum(v => v.Total)
                        })
                        .OrderByDescending(c => c.TotalGastado)
                        .Take(10)
                        .ToList();

                    int posicionCliente = 1;
                    foreach (var cliente in topClientes)
                    {
                        row++;
                        worksheet.Cell($"A{row}").Value = posicionCliente++;
                        worksheet.Cell($"B{row}").Value = cliente.Cliente;
                        worksheet.Cell($"C{row}").Value = cliente.CantidadCompras;
                        worksheet.Cell($"D{row}").Value = "$" + cliente.TotalGastado.ToString("N2");

                        worksheet.Cell($"A{row}").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        worksheet.Cell($"C{row}").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        worksheet.Cell($"D{row}").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                    }
                }

                // Ajustar columnas
                worksheet.Columns().AdjustToContents();

                // PROTEGER LA HOJA PARA QUE SEA DE SOLO LECTURA
                worksheet.Protect("GGHardware2024");

                workbook.SaveAs(rutaArchivo);
            }
        }
    }
}