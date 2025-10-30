using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GGHardware.Data;

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
                using (var context = new ApplicationDbContext())
                {
                    // Obtener rendimiento por vendedor
                    var rendimiento = context.Usuarios
                        .Where(u => u.Activo && u.RolId == 2) // Solo vendedores activos (ajusta el RolId)
                        .Select(u => new RendimientoVendedor
                        {
                            NombreVendedor = u.Nombre + " " + u.apellido,
                            CantidadVentas = context.Venta
                                .Count(v => v.id_Usuario == u.id_usuario
                                         && v.Estado != "Anulada"
                                         && v.Fecha >= _fechaInicio
                                         && v.Fecha <= _fechaFin),
                            MontoTotal = context.Venta
                                .Where(v => v.id_Usuario == u.id_usuario
                                         && v.Estado != "Anulada"
                                         && v.Fecha >= _fechaInicio
                                         && v.Fecha <= _fechaFin)
                                .Sum(v => (decimal?)v.Monto) ?? 0
                        })
                        .OrderByDescending(r => r.MontoTotal)
                        .ToList();

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