using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GGHardware.Data;
using GGHardware.Models;

namespace GGHardware.Views
{
    public partial class Reportes : UserControl
    {
        public Reportes()
        {
            InitializeComponent();

            dpFechaInicio.SelectedDate = DateTime.Today;
            dpFechaFin.SelectedDate = DateTime.Today;

            this.Loaded += Reportes_Loaded;
        }

        private void Reportes_Loaded(object sender, RoutedEventArgs e)
        {
            CargarReporte();
        }

        private void btnAplicarFiltro_Click(object sender, RoutedEventArgs e)
        {
            CargarReporte();
        }

        private void CargarReporte()
        {
            try
            {
                if (!dpFechaInicio.SelectedDate.HasValue || !dpFechaFin.SelectedDate.HasValue)
                {
                    MessageBox.Show("Por favor seleccione ambas fechas", "Advertencia",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DateTime fechaInicio = dpFechaInicio.SelectedDate.Value.Date; // 00:00:00
                DateTime fechaFin = dpFechaFin.SelectedDate.Value.Date.AddDays(1).AddTicks(-1); // 23:59:59.9999999

                if (fechaInicio > fechaFin)
                {
                    MessageBox.Show("La fecha de inicio no puede ser mayor a la fecha fin",
                        "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (var context = new ApplicationDbContext())
                {
                    var reporteDetalle = (from dv in context.DetalleVenta
                                          join v in context.Venta on dv.id_venta equals v.id_venta
                                          join c in context.Clientes on v.id_Cliente equals c.id_cliente
                                          join p in context.Producto on dv.id_producto equals p.Id_Producto
                                          where v.Fecha >= fechaInicio
                                             && v.Fecha <= fechaFin
                                          orderby v.Fecha descending
                                          select new ReporteVentaDetalle
                                          {
                                              Fecha = v.Fecha,
                                              Cliente = c.nombre + " " + c.apellido,
                                              Producto = dv.nombre_producto,
                                              Cantidad = dv.cantidad,
                                              Total = (dv.precio_con_descuento ?? dv.precio_unitario) * dv.cantidad
                                          }).ToList();

                    if (!reporteDetalle.Any())
                    {
                        dgReporte.ItemsSource = null;
                        txtVentasDia.Text = "$0.00";
                        txtProductosVendidos.Text = "0";
                        txtClientesNuevos.Text = "0";
                        txtRegistrosTotal.Text = "Total de registros: 0";
                        txtTotalGeneral.Text = "$0.00";

                        MessageBox.Show($"No se encontraron ventas en el período seleccionado.\n\n" +
                                      $"Período: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}",
                            "Sin resultados", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    var resumen = new ResumenVentas
                    {
                        VentasTotales = reporteDetalle.Sum(r => r.Total),
                        ProductosVendidos = reporteDetalle.Sum(r => r.Cantidad),
                        ClientesNuevos = reporteDetalle.Select(r => r.Cliente).Distinct().Count(),
                        TotalRegistros = reporteDetalle.Count
                    };

                    dgReporte.ItemsSource = reporteDetalle;

                    txtVentasDia.Text = $"${resumen.VentasTotales:N2}";
                    txtProductosVendidos.Text = resumen.ProductosVendidos.ToString();
                    txtClientesNuevos.Text = resumen.ClientesNuevos.ToString();
                    txtRegistrosTotal.Text = $"Total de registros: {resumen.TotalRegistros}";
                    txtTotalGeneral.Text = $"${resumen.VentasTotales:N2}";

                    GuardarReporteEnBD(context, fechaInicio, fechaFin, resumen);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}\n\nDetalle: {ex.InnerException?.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                    UsuarioID = 1, // TODO: Obtener del usuario logueado
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
    }
}