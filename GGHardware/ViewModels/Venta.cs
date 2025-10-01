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

            // Suscribirse a cambios en el carrito para actualizar la vista de productos
            Carrito.CollectionChanged += (s, e) => OnPropertyChanged(nameof(Productos));

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

        // NUEVO: Método para obtener stock disponible de un producto
        public double ObtenerStockDisponible(int idProducto)
        {
            var productoDb = _context.Producto.Find(idProducto);
            if (productoDb == null) return 0;

            var itemEnCarrito = Carrito.FirstOrDefault(c => c.IdProducto == idProducto);
            double cantidadEnCarrito = (double)(itemEnCarrito?.Cantidad ?? 0);

            return (double)productoDb.Stock - cantidadEnCarrito;
        }

        public void AgregarProducto(Producto producto)
        {
            // Obtener stock disponible actual
            var stockDisponible = ObtenerStockDisponible(producto.Id_Producto);

            if (stockDisponible <= 0)
            {
                var cantidadEnCarrito = Carrito.FirstOrDefault(c => c.IdProducto == producto.Id_Producto)?.Cantidad ?? 0;
                MessageBox.Show($"No hay más stock disponible de '{producto.Nombre}'.\nStock total: {producto.Stock}\nYa en carrito: {cantidadEnCarrito}",
                    "Stock insuficiente", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
                    Precio = (double)producto.precio_venta,
                    Cantidad = 1
                });
            }

            OnPropertyChanged(nameof(Total));

            // Forzar actualización del DataGrid
            RefrescarProductos();
        }

        private void RefrescarProductos()
        {
            // Crear lista temporal
            var tempList = Productos.ToList();
            Productos.Clear();
            foreach (var prod in tempList)
            {
                Productos.Add(prod);
            }
        }

        public void AgregarUnaUnidad(CarritoItem item)
        {
            // Verificar stock disponible
            var stockDisponible = ObtenerStockDisponible(item.IdProducto);

            if (stockDisponible <= 0)
            {
                var productoDb = _context.Producto.Find(item.IdProducto);
                MessageBox.Show($"No hay más stock disponible de '{item.Nombre}'.\nStock total: {productoDb?.Stock ?? 0}\nYa en carrito: {item.Cantidad}",
                    "Stock insuficiente", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            item.Cantidad++;
            OnPropertyChanged(nameof(Total));
            RefrescarProductos();
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
            RefrescarProductos();
        }

        public void QuitarProducto(CarritoItem item)
        {
            Carrito.Remove(item);
            OnPropertyChanged(nameof(Total));
            RefrescarProductos();
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
                // 1. Validar stock ANTES de guardar
                foreach (var item in Carrito)
                {
                    var producto = _context.Producto.Find(item.IdProducto);
                    if (producto == null)
                    {
                        MessageBox.Show($"Producto '{item.Nombre}' no encontrado en la base de datos.",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (producto.Stock < item.Cantidad)
                    {
                        MessageBox.Show($"Stock insuficiente para '{producto.Nombre}'.\nStock disponible: {producto.Stock}\nCantidad solicitada: {item.Cantidad}",
                            "Stock insuficiente", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                // 2. Crear la venta (cabecera)
                var venta = new Venta
                {
                    id_Cliente = ClienteSeleccionado.id_cliente,
                    id_Usuario = 1, // TODO: Obtener usuario logueado
                    Fecha = DateTime.Now,
                    Monto = Total,
                    IdTipoComprobante = TipoComprobanteSeleccionado.id_tipo, // Ya es int, se asigna directamente
                    MetodoPago = MetodoPagoSeleccionado?.nombre ?? string.Empty,
                    MontoRecibido = MostrarCampoVuelto ? MontoRecibido : (double?)null,
                    Observaciones = Observaciones ?? string.Empty,
                    Estado = "Completada"
                };

                _context.Venta.Add(venta);
                _context.SaveChanges(); // Guardar para obtener el id_venta

                // 3. Crear los detalles de venta Y DESCONTAR STOCK
                foreach (var item in Carrito)
                {
                    var detalle = new DetalleVenta
                    {
                        id_venta = venta.id_venta,
                        id_producto = item.IdProducto,
                        nombre_producto = item.Nombre ?? "Sin nombre",
                        cantidad = item.Cantidad,
                        precio_unitario = (decimal)item.Precio,
                        precio_con_descuento = item.PrecioConDescuento.HasValue ? (decimal)item.PrecioConDescuento.Value : (decimal)item.Precio,
                        Activo = true
                    };
                    _context.DetalleVenta.Add(detalle);

                    // DESCONTAR STOCK
                    var producto = _context.Producto.Find(item.IdProducto);
                    if (producto != null)
                    {
                        producto.Stock -= (int)item.Cantidad;
                    }
                }

                _context.SaveChanges();

                string mensajeVuelto = MostrarCampoVuelto ? $"\nVuelto: {Vuelto:C}" : "";
                MessageBox.Show($"Venta registrada correctamente\nTotal: {Total:C}{mensajeVuelto}",
                    "Venta Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

                // Limpiar todo y recargar productos para actualizar stock en la vista
                CancelarVenta();
                CargarProductos();

                // Actualizar visualmente todos los productos
                OnPropertyChanged(nameof(Productos));
            }
            catch (Exception ex)
            {
                string detalleError = ex.InnerException != null ? $"\n\nDetalle: {ex.InnerException.Message}" : "";
                MessageBox.Show($"Error al registrar la venta:\n{ex.Message}{detalleError}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // NUEVO: Generar ticket/comprobante de venta
        public void GenerarTicket(int idVenta)
        {
            try
            {
                var venta = _context.Venta
                    .Include(v => v.Cliente)
                    .Include(v => v.Usuario)
                    .Include(v => v.TipoComprobante)
                    .Include(v => v.Detalles)
                        .ThenInclude(d => d.Producto)
                    .FirstOrDefault(v => v.id_venta == idVenta);

                if (venta == null)
                {
                    MessageBox.Show("No se encontró la venta", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Generar ticket en formato texto
                var ticket = GenerarTextoTicket(venta);

                // Mostrar en ventana o imprimir
                var ventanaTicket = new Window
                {
                    Title = "Comprobante de Venta",
                    Width = 400,
                    Height = 600,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                var scrollViewer = new System.Windows.Controls.ScrollViewer();
                var textBlock = new System.Windows.Controls.TextBlock
                {
                    Text = ticket,
                    FontFamily = new System.Windows.Media.FontFamily("Courier New"),
                    FontSize = 12,
                    Padding = new Thickness(20),
                    TextWrapping = TextWrapping.Wrap
                };

                scrollViewer.Content = textBlock;
                ventanaTicket.Content = scrollViewer;
                ventanaTicket.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar ticket:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerarTextoTicket(Venta venta)
        {
            var sb = new System.Text.StringBuilder();
            var ancho = 40;

            // Encabezado
            sb.AppendLine(CentrarTexto("GGHardware", ancho));
            sb.AppendLine(CentrarTexto("9 de Julio 1890", ancho));
            sb.AppendLine(new string('=', ancho));
            sb.AppendLine();

            // Información del comprobante
            sb.AppendLine($"Tipo: {venta.TipoComprobanteNombre}");
            sb.AppendLine($"Nro: {venta.NumeroComprobanteFormateado}");
            sb.AppendLine($"Fecha: {venta.Fecha:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"Vendedor: {venta.Usuario?.Nombre ?? "N/A"}");
            sb.AppendLine();

            // Cliente
            sb.AppendLine($"Cliente: {venta.ClienteNombre}");
            if (venta.Cliente != null && !string.IsNullOrEmpty(venta.Cliente.cuit))
                sb.AppendLine($"CUIT: {venta.Cliente.cuit}");
            sb.AppendLine(new string('-', ancho));
            sb.AppendLine();

            // Detalle de productos
            sb.AppendLine("PRODUCTOS");
            sb.AppendLine(new string('-', ancho));

            foreach (var detalle in venta.Detalles)
            {
                sb.AppendLine($"{detalle.nombre_producto}");
                sb.AppendLine($"  {detalle.cantidad} x {detalle.precio_unitario:C} = {detalle.Subtotal:C}");

                if (detalle.TieneDescuento)
                {
                    sb.AppendLine($"  Descuento: {detalle.PorcentajeDescuento:F1}% (-{detalle.MontoDescuento:C})");
                }
            }

            sb.AppendLine(new string('-', ancho));
            sb.AppendLine();

            // Totales
            var totalDescuentos = venta.Detalles.Sum(d => d.MontoDescuento);
            if (totalDescuentos > 0)
            {
                var subtotal = venta.Detalles.Sum(d => (decimal)d.precio_unitario * d.cantidad);
                sb.AppendLine($"Subtotal:        {subtotal,15:C}");
                sb.AppendLine($"Descuentos:      {totalDescuentos,15:C}");
            }

            sb.AppendLine($"TOTAL:           {venta.Monto,15:C}");
            sb.AppendLine();

            // Método de pago
            sb.AppendLine($"Método de pago: {venta.MetodoPago}");
            if (venta.MontoRecibido.HasValue)
            {
                sb.AppendLine($"Recibido:        {venta.MontoRecibido.Value,15:C}");
                sb.AppendLine($"Vuelto:          {venta.Vuelto,15:C}");
            }

            if (!string.IsNullOrEmpty(venta.Observaciones))
            {
                sb.AppendLine();
                sb.AppendLine($"Obs: {venta.Observaciones}");
            }

            sb.AppendLine();
            sb.AppendLine(new string('=', ancho));
            sb.AppendLine(CentrarTexto("¡Gracias por su compra!", ancho));

            return sb.ToString();
        }

        private string CentrarTexto(string texto, int ancho)
        {
            if (texto.Length >= ancho) return texto;
            int espacios = (ancho - texto.Length) / 2;
            return new string(' ', espacios) + texto;
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
            if (string.IsNullOrWhiteSpace(codigo))
            {
                MessageBox.Show("Ingrese un código de producto", "Advertencia",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Buscar por código de barras, código interno o ID
            var producto = _context.Producto
                .FirstOrDefault(p => p.codigo_barras == codigo ||
                                   p.codigo_interno == codigo ||
                                   p.Id_Producto.ToString() == codigo);

            if (producto == null)
            {
                MessageBox.Show($"No se encontró ningún producto con el código: {codigo}",
                    "Producto no encontrado", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Agregar automáticamente al carrito
            AgregarProducto(producto);
        }

        public void BuscarProducto()
        {
            // Abrir ventana de búsqueda avanzada (opcional, por ahora solo mensaje)
            MessageBox.Show("Funcionalidad de búsqueda avanzada\nUsa el campo 'Código' para búsqueda rápida",
                "Búsqueda de productos", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // NUEVO: Aplicar descuento a un item específico
        public void AplicarDescuentoAItem(CarritoItem item, double porcentaje)
        {
            if (item == null) return;

            if (porcentaje < 0 || porcentaje > 100)
            {
                MessageBox.Show("El porcentaje debe estar entre 0 y 100",
                    "Descuento inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            item.AplicarDescuentoPorcentaje(porcentaje);
            OnPropertyChanged(nameof(Total));
        }

        // NUEVO: Aplicar descuento global a todos los items
        public void AplicarDescuentoGlobal(double porcentaje)
        {
            if (!Carrito.Any())
            {
                MessageBox.Show("El carrito está vacío", "Advertencia",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (porcentaje < 0 || porcentaje > 100)
            {
                MessageBox.Show("El porcentaje debe estar entre 0 y 100",
                    "Descuento inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            foreach (var item in Carrito)
            {
                item.AplicarDescuentoPorcentaje(porcentaje);
            }

            OnPropertyChanged(nameof(Total));
            MessageBox.Show($"Se aplicó un descuento del {porcentaje}% a todos los productos",
                "Descuento aplicado", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // NUEVO: Quitar todos los descuentos
        public void QuitarTodosLosDescuentos()
        {
            foreach (var item in Carrito)
            {
                item.QuitarDescuento();
            }
            OnPropertyChanged(nameof(Total));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}