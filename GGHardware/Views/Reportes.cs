using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GGHardware.Data;
using GGHardware.Models;
using System.Collections.Generic;

namespace GGHardware.Views
{
    public partial class Reportes : UserControl
    {
        private List<ReporteVentaDetalle> _todasLasVentas; // Guardar todas las ventas en memoria

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