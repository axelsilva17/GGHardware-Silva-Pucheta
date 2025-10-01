using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using GGHardware.Data;
using GGHardware.Models;

namespace GGHardware.ViewModels
{
    public class VentasViewModel : INotifyPropertyChanged
    {
        private ApplicationDbContext _context;

        public ObservableCollection<Producto> Productos { get; set; }
        public ObservableCollection<Cliente> ClientesFiltrados { get; set; }
        public ObservableCollection<CarritoItem> Carrito { get; set; }
        public ObservableCollection<TipoComprobante> TiposComprobante { get; set; }
        public ObservableCollection<MetodoPago> MetodosPago { get; set; }

        private Cliente _clienteSeleccionado;
        public Cliente ClienteSeleccionado
        {
            get => _clienteSeleccionado;
            set
            {
                _clienteSeleccionado = value;
                OnPropertyChanged(nameof(ClienteSeleccionado));
            }
        }

        private TipoComprobante _tipoComprobanteSeleccionado;
        public TipoComprobante TipoComprobanteSeleccionado
        {
            get => _tipoComprobanteSeleccionado;
            set
            {
                _tipoComprobanteSeleccionado = value;
                OnPropertyChanged(nameof(TipoComprobanteSeleccionado));
            }
        }

        private MetodoPago _metodoPagoSeleccionado;
        public MetodoPago MetodoPagoSeleccionado
        {
            get => _metodoPagoSeleccionado;
            set
            {
                _metodoPagoSeleccionado = value;
                OnPropertyChanged(nameof(MetodoPagoSeleccionado));
                OnPropertyChanged(nameof(MostrarCampoVuelto));
            }
        }

        private double _montoRecibido;
        public double MontoRecibido
        {
            get => _montoRecibido;
            set
            {
                _montoRecibido = value;
                OnPropertyChanged(nameof(MontoRecibido));
                OnPropertyChanged(nameof(Vuelto));
            }
        }

        private string _observaciones;
        public string Observaciones
        {
            get => _observaciones;
            set
            {
                _observaciones = value;
                OnPropertyChanged(nameof(Observaciones));
            }
        }

        public bool MostrarCampoVuelto => MetodoPagoSeleccionado?.nombre?.ToLower() == "efectivo";

        public double Vuelto => MontoRecibido - Total;

        public double Total => Carrito.Sum(item => (double)item.Subtotal);

        public VentasViewModel()
        {
            _context = new ApplicationDbContext();
            Productos = new ObservableCollection<Producto>();
            ClientesFiltrados = new ObservableCollection<Cliente>();
            Carrito = new ObservableCollection<CarritoItem>();
            TiposComprobante = new ObservableCollection<TipoComprobante>();
            MetodosPago = new ObservableCollection<MetodoPago>();

            CargarProductos();
            CargarTiposComprobante();
            CargarMetodosPago();
        }

        private void CargarProductos()
        {
            Productos.Clear();
            var productos = _context.Producto.ToList();
            foreach (var prod in productos)
            {
                Productos.Add(prod);
            }
        }

        private void CargarTiposComprobante()
        {
            try
            {
                TiposComprobante.Clear();
                var tipos = _context.TiposComprobante.Where(t => t.activo == true).ToList();

                if (!tipos.Any())
                {
                    MessageBox.Show("No hay tipos de comprobante activos en la base de datos.\nPor favor, configure al menos un tipo de comprobante.",
                        "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                foreach (var tipo in tipos)
                {
                    TiposComprobante.Add(tipo);
                }

                if (TiposComprobante.Any())
                    TipoComprobanteSeleccionado = TiposComprobante.First();
                else
                    TipoComprobanteSeleccionado = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar tipos de comprobante:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarMetodosPago()
        {
            try
            {
                MetodosPago.Clear();
                var metodos = _context.MetodosPago.Where(m => m.activo == true).ToList();

                if (!metodos.Any())
                {
                    MessageBox.Show("No hay métodos de pago activos en la base de datos.\nPor favor, configure al menos un método de pago.",
                        "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                foreach (var metodo in metodos)
                {
                    MetodosPago.Add(metodo);
                }

                if (MetodosPago.Any())
                    MetodoPagoSeleccionado = MetodosPago.First();
                else
                    MetodoPagoSeleccionado = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar métodos de pago:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void RecargarClientes()
        {
            // Tu código existente para recargar clientes
        }

        public void FiltrarClientes(string texto)
        {
            ClientesFiltrados.Clear();
            var clientes = _context.Clientes
                .Where(c => c.nombre.Contains(texto) ||
                           c.apellido.Contains(texto) ||
                           c.cuit.Contains(texto))
                .Take(10)
                .ToList();

            foreach (var cliente in clientes)
            {
                ClientesFiltrados.Add(cliente);
            }
        }

        public void SeleccionarCliente(Cliente cliente)
        {
            ClienteSeleccionado = cliente;
        }

        public void AgregarProducto(Producto producto)
        {
            var itemExistente = Carrito.FirstOrDefault(c => c.IdProducto == producto.Id_Producto);

            if (itemExistente != null)
            {
                itemExistente.Cantidad++;
            }
            else
            {
                Carrito.Add(new CarritoItem
                {
                    IdProducto = producto.Id_Producto,
                    Nombre = producto.Nombre,
                    Precio = producto.precio_venta,
                    Cantidad = 1
                });
            }

            OnPropertyChanged(nameof(Total));
        }

        public void AgregarUnaUnidad(CarritoItem item)
        {
            item.Cantidad++;
            OnPropertyChanged(nameof(Total));
        }

        public void QuitarUnaUnidad(CarritoItem item)
        {
            if (item.Cantidad > 1)
            {
                item.Cantidad--;
            }
            else
            {
                Carrito.Remove(item);
            }
            OnPropertyChanged(nameof(Total));
        }

        public void QuitarProducto(CarritoItem item)
        {
            Carrito.Remove(item);
            OnPropertyChanged(nameof(Total));
        }

        public void FinalizarVenta()
        {
            if (ClienteSeleccionado == null)
            {
                MessageBox.Show("Debe seleccionar un cliente", "Advertencia",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Carrito.Any())
            {
                MessageBox.Show("El carrito está vacío", "Advertencia",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TipoComprobanteSeleccionado == null || string.IsNullOrEmpty(TipoComprobanteSeleccionado.Nombre))
            {
                MessageBox.Show("Debe seleccionar un tipo de comprobante válido", "Advertencia",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MetodoPagoSeleccionado == null || string.IsNullOrEmpty(MetodoPagoSeleccionado.nombre))
            {
                MessageBox.Show("Debe seleccionar un método de pago válido", "Advertencia",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar monto recibido si es en efectivo
            if (MostrarCampoVuelto && MontoRecibido < Total)
            {
                MessageBox.Show("El monto recibido no puede ser menor al total", "Advertencia",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 1. Crear la venta (cabecera)
                var venta = new Venta
                {
                    id_Cliente = ClienteSeleccionado.id_cliente,
                    id_Usuario = 1, // TODO: Obtener usuario logueado
                    Fecha = DateTime.Now,
                    Monto = Total,
                    IdTipoComprobante = (int?)TipoComprobanteSeleccionado.id_tipo,
                    MetodoPago = MetodoPagoSeleccionado.nombre,
                    MontoRecibido = MostrarCampoVuelto ? (double?)MontoRecibido : null,
                    Observaciones = Observaciones,
                    Estado = "Completada"
                };

                _context.Venta.Add(venta);
                _context.SaveChanges(); // Guardar para obtener el id_venta

                // 2. Crear los detalles de venta
                foreach (var item in Carrito)
                {
                    var detalle = new DetalleVenta
                    {
                        id_venta = venta.id_venta,
                        id_producto = item.IdProducto,
                        nombre_producto = item.Nombre ?? "Sin nombre",
                        cantidad = item.Cantidad,
                        precio_unitario = (decimal)item.Precio,
                        precio_con_descuento = (decimal)item.PrecioConDescuento,
                        Activo = true
                    };
                    _context.DetalleVenta.Add(detalle);
                }

                _context.SaveChanges();

                string mensajeVuelto = MostrarCampoVuelto ? $"\nVuelto: {Vuelto:C}" : "";
                MessageBox.Show($"Venta registrada correctamente\nTotal: {Total:C}{mensajeVuelto}",
                    "Venta Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

                // Limpiar todo
                CancelarVenta();
            }
            catch (Exception ex)
            {
                string detalleError = ex.InnerException != null ? $"\n\nDetalle: {ex.InnerException.Message}" : "";
                MessageBox.Show($"Error al registrar la venta:\n{ex.Message}{detalleError}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void CancelarVenta()
        {
            try
            {
                Carrito.Clear();
                ClienteSeleccionado = null;
                Observaciones = string.Empty;
                MontoRecibido = 0;

                // Reseleccionar los valores por defecto
                if (TiposComprobante.Any())
                    TipoComprobanteSeleccionado = TiposComprobante.First();

                if (MetodosPago.Any())
                    MetodoPagoSeleccionado = MetodosPago.First();

                OnPropertyChanged(nameof(Total));
                OnPropertyChanged(nameof(Vuelto));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al limpiar venta:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void BuscarProductoPorCodigo(string codigo)
        {
            // Tu código existente
        }

        public void BuscarProducto()
        {
            // Tu código existente
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}