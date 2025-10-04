using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using GGHardware.Data;
using GGHardware.Models;
using Microsoft.EntityFrameworkCore;

namespace GGHardware.ViewModels
{
    public class HistorialVentasViewModel : INotifyPropertyChanged
    {
        private ApplicationDbContext _context;

        public ObservableCollection<Venta> Ventas { get; set; }
        public ObservableCollection<Cliente> Clientes { get; set; }

        private DateTime _fechaDesde;
        public DateTime FechaDesde
        {
            get => _fechaDesde;
            set
            {
                _fechaDesde = value;
                OnPropertyChanged(nameof(FechaDesde));
            }
        }

        private DateTime _fechaHasta;
        public DateTime FechaHasta
        {
            get => _fechaHasta;
            set
            {
                _fechaHasta = value;
                OnPropertyChanged(nameof(FechaHasta));
            }
        }

        private Cliente _clienteFiltro;
        public Cliente ClienteFiltro
        {
            get => _clienteFiltro;
            set
            {
                _clienteFiltro = value;
                OnPropertyChanged(nameof(ClienteFiltro));
            }
        }

        private string _estadoFiltro;
        public string EstadoFiltro
        {
            get => _estadoFiltro;
            set
            {
                _estadoFiltro = value;
                OnPropertyChanged(nameof(EstadoFiltro));
            }
        }

        private Venta _ventaSeleccionada;
        public Venta VentaSeleccionada
        {
            get => _ventaSeleccionada;
            set
            {
                _ventaSeleccionada = value;
                OnPropertyChanged(nameof(VentaSeleccionada));
                CargarDetallesVenta();
            }
        }

        public ObservableCollection<DetalleVenta> DetallesVenta { get; set; }

        public HistorialVentasViewModel()
        {
            _context = new ApplicationDbContext();
            Ventas = new ObservableCollection<Venta>();
            Clientes = new ObservableCollection<Cliente>();
            DetallesVenta = new ObservableCollection<DetalleVenta>();

            // Filtros por defecto: último mes
            FechaHasta = DateTime.Today;
            FechaDesde = DateTime.Today.AddMonths(-1);

            CargarClientes();
            BuscarVentas();
        }

        public decimal MontoTotal
        {
            get
            {
                try
                {
                    if (Ventas == null || Ventas.Count == 0) return 0;

                    decimal total = 0;
                    foreach (var venta in Ventas.Where(v => v.Estado != "Anulada"))
                    {
                        total += venta.Monto;
                    }
                    return total;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error en MontoTotal: {ex.Message}");
                    return 0;
                }
            }
        }

        private void CargarClientes()
        {
            Clientes.Clear();

            var clientes = _context.Clientes.OrderBy(c => c.nombre).ToList();
            foreach (var cliente in clientes)
            {
                Clientes.Add(cliente);
            }

            // No seleccionar ninguno por defecto (mostrará todos)
            ClienteFiltro = null;
        }

        public void BuscarVentas()
        {
            try
            {
                Ventas.Clear();

                var query = _context.Venta
                    .Include(v => v.Cliente)
                    .Include(v => v.Usuario)
                    .Include(v => v.TipoComprobante)
                    .Include(v => v.Detalles)
                    .Where(v => v.Fecha >= FechaDesde && v.Fecha <= FechaHasta.AddDays(1));

                // Filtro por cliente (solo si se seleccionó uno)
                if (ClienteFiltro != null && ClienteFiltro.id_cliente > 0)
                {
                    query = query.Where(v => v.id_Cliente == ClienteFiltro.id_cliente);
                }

                // Filtro por estado
                if (!string.IsNullOrEmpty(EstadoFiltro) && EstadoFiltro != "Todos")
                {
                    query = query.Where(v => v.Estado == EstadoFiltro);
                }

                var ventas = query.OrderByDescending(v => v.Fecha).ToList();

                foreach (var venta in ventas)
                {
                    Ventas.Add(venta);
                }

                // Notificar cambio en MontoTotal después de cargar ventas
                OnPropertyChanged(nameof(MontoTotal));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar ventas:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarDetallesVenta()
        {
            DetallesVenta.Clear();

            if (VentaSeleccionada == null) return;

            try
            {
                var detalles = _context.DetalleVenta
                    .Include(d => d.Producto)
                    .Where(d => d.id_venta == VentaSeleccionada.id_venta && d.Activo)
                    .ToList();

                foreach (var detalle in detalles)
                {
                    DetallesVenta.Add(detalle);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar detalles:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ReimprimirComprobante()
        {
            if (VentaSeleccionada == null)
            {
                MessageBox.Show("Seleccione una venta para reimprimir",
                    "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Usar el mismo método del ViewModel de ventas
            var ventasVM = new VentasViewModel(MainWindow.UsuarioActual.id_usuario);
            ventasVM.GenerarTicket(VentaSeleccionada.id_venta);
        }

        public void AnularVenta()
        {
            if (VentaSeleccionada == null)
            {
                MessageBox.Show("Seleccione una venta para anular",
                    "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (VentaSeleccionada.Estado == "Anulada")
            {
                MessageBox.Show("Esta venta ya está anulada",
                    "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var resultado = MessageBox.Show(
                $"¿Está seguro que desea anular la venta #{VentaSeleccionada.id_venta}?\n\n" +
                "Esta acción no se puede deshacer y se devolverá el stock de los productos.",
                "Confirmar Anulación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (resultado != MessageBoxResult.Yes) return;

            try
            {
                // Devolver stock
                var detalles = _context.DetalleVenta
                    .Where(d => d.id_venta == VentaSeleccionada.id_venta && d.Activo)
                    .ToList();

                foreach (var detalle in detalles)
                {
                    var producto = _context.Producto.Find(detalle.id_producto);
                    if (producto != null)
                    {
                        producto.Stock += detalle.cantidad;
                    }
                }

                // Marcar venta como anulada
                VentaSeleccionada.Estado = "Anulada";
                _context.SaveChanges();

                MessageBox.Show("Venta anulada correctamente. Se ha devuelto el stock de los productos.",
                    "Venta Anulada", MessageBoxButton.OK, MessageBoxImage.Information);

                // Actualizar lista
                BuscarVentas();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al anular venta:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}