using GGHardware.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GGHardware.Views
{
    public partial class RendimientoVendedoresView : UserControl
    {
        private DateTime _fechaInicio;
        private DateTime _fechaFin;

        public RendimientoVendedoresView()
        {
            InitializeComponent();

            // Por defecto: este mes
            _fechaInicio = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            _fechaFin = DateTime.Today;

            dpFechaDesde.SelectedDate = _fechaInicio;
            dpFechaHasta.SelectedDate = _fechaFin;

            // IMPORTANTE: Cargar datos después de que todo esté inicializado
            this.Loaded += (s, e) => CargarDatos();
        }

        private void CargarDatos()
        {
            try
            {
                // Asegurarnos que las fechas no tengan horas que corten resultados:
                var inicio = _fechaInicio.Date;
                // Hacemos _fechaFin al final del día para incluir todas las ventas del día seleccionado
                var fin = _fechaFin.Date.AddDays(1).AddTicks(-1);

                using (var context = new ApplicationDbContext())
                {
                    // Verificación rápida: cuántas ventas totales hay en el rango (debug)
                    var ventasTotalesEnRango = context.Venta
                        .AsNoTracking()
                        .Count(v => v.Fecha >= inicio && v.Fecha <= fin && v.Estado != "Anulada");
                    Debug.WriteLine($"Ventas totales en rango {inicio} - {fin}: {ventasTotalesEnRango}");

                    // Hacemos una consulta basada en JOIN/GROUP BY desde ventas (más fiable)
                    var ventasPorVendedor = context.Venta
                        .AsNoTracking()
                        .Where(v => v.Fecha >= inicio && v.Fecha <= fin && v.Estado != "Anulada")
                        .GroupBy(v => v.id_Usuario)
                        .Select(g => new
                        {
                            IdUsuario = g.Key,
                            Cantidad = g.Count(),
                            Monto = g.Sum(x => (decimal?)x.Monto) ?? 0
                        })
                        .ToList();

                    // Ahora traemos los vendedores activos y combinamos con los resultados anteriores
                    var vendedores = context.Usuarios
                        .AsNoTracking()
                        .Where(u => u.Activo && u.RolId == 2)
                        .Select(u => new
                        {
                            u.id_usuario,
                            NombreCompleto = u.Nombre + " " + u.apellido
                        })
                        .ToList();

                    // Construimos la lista final
                    var rendimiento = vendedores
                        .Select(v =>
                        {
                            var datos = ventasPorVendedor.FirstOrDefault(x => x.IdUsuario == v.id_usuario);
                            return new RendimientoVendedor
                            {
                                NombreVendedor = v.NombreCompleto,
                                CantidadVentas = datos?.Cantidad ?? 0,
                                MontoTotal = datos?.Monto ?? 0
                            };
                        })
                        .OrderByDescending(r => r.MontoTotal)
                        .ToList();

                    // Debug: mostrar en salida el detalle de cada vendedor
                    foreach (var r in rendimiento)
                    {
                        Debug.WriteLine($"Vendedor: {r.NombreVendedor}, Cant: {r.CantidadVentas}, Monto: {r.MontoTotal}");
                    }

                    dgVendedores.ItemsSource = rendimiento;

                    // Actualizar resumen
                    txtTotalVendido.Text = rendimiento.Sum(r => r.MontoTotal).ToString("C");
                    txtTotalVentas.Text = rendimiento.Sum(r => r.CantidadVentas).ToString();

                    var totalVentas = rendimiento.Sum(r => r.CantidadVentas);
                    var promedioGeneral = totalVentas > 0
                        ? rendimiento.Sum(r => r.MontoTotal) / totalVentas
                        : 0;
                    txtPromedioVenta.Text = promedioGeneral.ToString("C");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmbPeriodo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // AGREGAR ESTA VERIFICACIÓN
            if (pnlFechasPersonalizadas == null) return;

            if (cmbPeriodo.SelectedIndex == 4) // Personalizado
            {
                pnlFechasPersonalizadas.Visibility = Visibility.Visible;
            }
            else
            {
                pnlFechasPersonalizadas.Visibility = Visibility.Collapsed;

                switch (cmbPeriodo.SelectedIndex)
                {
                    case 0: // Hoy
                        _fechaInicio = DateTime.Today;
                        _fechaFin = DateTime.Today;
                        break;
                    case 1: // Esta semana
                        var diasDesdeInicioSemana = (int)DateTime.Today.DayOfWeek;
                        _fechaInicio = DateTime.Today.AddDays(-diasDesdeInicioSemana);
                        _fechaFin = DateTime.Today;
                        break;
                    case 2: // Este mes
                        _fechaInicio = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                        _fechaFin = DateTime.Today;
                        break;
                    case 3: // Este año
                        _fechaInicio = new DateTime(DateTime.Today.Year, 1, 1);
                        _fechaFin = DateTime.Today;
                        break;
                }

                CargarDatos();
            }
        }

        private void btnAplicar_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPeriodo.SelectedIndex == 4) // Personalizado
            {
                if (dpFechaDesde.SelectedDate.HasValue && dpFechaHasta.SelectedDate.HasValue)
                {
                    _fechaInicio = dpFechaDesde.SelectedDate.Value;
                    _fechaFin = dpFechaHasta.SelectedDate.Value;
                    CargarDatos();
                }
                else
                {
                    MessageBox.Show("Seleccione ambas fechas", "Advertencia",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                CargarDatos();
            }
        }

        private void btnVolver_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainContentBorder.Child = new Gerente();
            }
        }
    

    private void Actualizar_Click(object sender, RoutedEventArgs e)
        {
            CargarDatos();
        }

    // Clase auxiliar para el binding
    public class RendimientoVendedor
    {
        public string NombreVendedor { get; set; }
        public int CantidadVentas { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal PromedioVenta => CantidadVentas > 0 ? MontoTotal / CantidadVentas : 0;
    }
}
}