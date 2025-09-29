using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using GGHardware.Models;
using GGHardware.Data;
using Microsoft.EntityFrameworkCore;

namespace GGHardware.ViewModels
{
    public class VentasViewModel : INotifyPropertyChanged
    {
        // Colecciones observables existentes
        public ObservableCollection<Producto> Productos { get; set; }
        public ObservableCollection<Cliente> Clientes { get; set; }
        public ObservableCollection<Cliente> ClientesFiltrados { get; set; }
        public ObservableCollection<CarritoItem> Carrito { get; set; }

        // NUEVAS colecciones para comprobantes
        public ObservableCollection<TipoComprobante> TiposComprobante { get; set; }
        public ObservableCollection<MetodoPago> MetodosPago { get; set; }

        // Propiedades existentes
        private Cliente _clienteSeleccionado;
        public Cliente ClienteSeleccionado
        {
            get => _clienteSeleccionado;
            set
            {
                _clienteSeleccionado = value;
                OnPropertyChanged();
                ValidarClienteSegunComprobante();
            }
        }

        private double _total;
        public double Total
        {
            get => _total;
            set
            {
                _total = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Vuelto));
            }
        }

        // NUEVAS propiedades para comprobantes
        private TipoComprobante _tipoComprobanteSeleccionado;
        public TipoComprobante TipoComprobanteSeleccionado
        {
            get => _tipoComprobanteSeleccionado;
            set
            {
                _tipoComprobanteSeleccionado = value;
                OnPropertyChanged();
                ValidarClienteSegunComprobante();
            }
        }

        private MetodoPago _metodoPagoSeleccionado;
        public MetodoPago MetodoPagoSeleccionado
        {
            get => _metodoPagoSeleccionado;
            set
            {
                _metodoPagoSeleccionado = value;
                OnPropertyChanged();
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
                OnPropertyChanged();
                OnPropertyChanged(nameof(Vuelto));
            }
        }

        public double Vuelto => MontoRecibido - Total;

        private string _observaciones;
        public string Observaciones
        {
            get => _observaciones;
            set
            {
                _observaciones = value;
                OnPropertyChanged();
            }
        }

        private string _codigoProductoBuscar;
        public string CodigoProductoBuscar
        {
            get => _codigoProductoBuscar;
            set
            {
                _codigoProductoBuscar = value;
                OnPropertyChanged();
            }
        }

        // Propiedades calculadas para la UI
        public bool MostrarCampoVuelto => MetodoPagoSeleccionado?.requiere_vuelto == true;
        public bool PuedeGenerarComprobante => PuedeFinalizarVenta() && TipoComprobanteSeleccionado != null;

        public VentasViewModel()
        {
            // Inicializar colecciones
            Productos = new ObservableCollection<Producto>();
            Clientes = new ObservableCollection<Cliente>();
            ClientesFiltrados = new ObservableCollection<Cliente>();
            Carrito = new ObservableCollection<CarritoItem>();
            TiposComprobante = new ObservableCollection<TipoComprobante>();
            MetodosPago = new ObservableCollection<MetodoPago>();

            // Establecer valores por defecto
            MontoRecibido = 0;

            CargarDatos();
        }

        private void CargarDatos()
        {
            CargarProductosPrueba(); // Mantener productos de prueba por ahora
            CargarClientesDesdeBaseDatos();
            CargarTiposComprobante();
            CargarMetodosPago();
        }

        private void CargarTiposComprobante()
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var tipos = context.TiposComprobante
                        .Where(t => t.activo)
                        .OrderBy(t => t.Nombre)
                        .ToList();

                    TiposComprobante.Clear();
                    foreach (var tipo in tipos)
                    {
                        TiposComprobante.Add(tipo);
                    }

                    // Seleccionar "Ticket" por defecto
                    TipoComprobanteSeleccionado = TiposComprobante.FirstOrDefault(t => t.codigo == "T");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar tipos de comprobante: {ex.Message}");
            }
        }

        public void BuscarProducto()
        {
            BuscarProductoPorCodigo(CodigoProductoBuscar);
        }

        private void CargarMetodosPago()
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var metodos = context.MetodosPago
                        .Where(m => m.activo)
                        .OrderBy(m => m.nombre)
                        .ToList();

                    MetodosPago.Clear();
                    foreach (var metodo in metodos)
                    {
                        MetodosPago.Add(metodo);
                    }

                    // Seleccionar "Efectivo" por defecto
                    MetodoPagoSeleccionado = MetodosPago.FirstOrDefault(m => m.nombre == "Efectivo");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar métodos de pago: {ex.Message}");
            }
        }

        private void CargarClientesDesdeBaseDatos()
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var clientesDB = context.Clientes.OrderBy(c => c.nombre).ThenBy(c => c.apellido).ToList();

                    Clientes.Clear();
                    foreach (var cliente in clientesDB)
                    {
                        Clientes.Add(cliente);
                    }

                    System.Diagnostics.Debug.WriteLine($"Clientes cargados desde BD: {Clientes.Count}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar clientes desde la base de datos: {ex.Message}",
                    "Error de Base de Datos", MessageBoxButton.OK, MessageBoxImage.Error);

                CargarClientesPrueba();
            }
        }

        public void RecargarClientes()
        {
            CargarClientesDesdeBaseDatos();
            ClientesFiltrados.Clear();
        }


        private void CargarProductosPrueba()
        {
            // Productos con códigos para pruebas
            Productos.Add(new Producto
            {
                Id_Producto = 1,
                Nombre = "Mouse Logitech",
                precio_venta = 2500,
                Stock = 10,
                precio_costo = 1500,
                descripcion = "Mouse óptico inalámbrico",
                codigo_barras = "7501234567890",
                codigo_interno = "MSE001"
            });

            Productos.Add(new Producto
            {
                Id_Producto = 2,
                Nombre = "Teclado Mecánico",
                precio_venta = 8900,
                Stock = 5,
                precio_costo = 5000,
                descripcion = "Teclado mecánico RGB",
                codigo_barras = "7501234567891",
                codigo_interno = "TEC001"
            });

            Productos.Add(new Producto
            {
                Id_Producto = 3,
                Nombre = "Monitor 24\"",
                precio_venta = 45000,
                Stock = 3,
                precio_costo = 35000,
                descripcion = "Monitor LED Full HD",
                codigo_barras = "7501234567892",
                codigo_interno = "MON001"
            });

            Productos.Add(new Producto
            {
                Id_Producto = 4,
                Nombre = "Memoria RAM 8GB",
                precio_venta = 15000,
                Stock = 8,
                precio_costo = 10000,
                descripcion = "RAM DDR4 3200MHz",
                codigo_barras = "7501234567893",
                codigo_interno = "RAM001"
            });
        }

        private void CargarClientesPrueba()
        {
            Clientes.Add(new Cliente
            {
                id_cliente = 1,
                nombre = "Juan",
                apellido = "Pérez",
                cuit = "20123456789",
                telefono = "1234567890",
                direccion = "Av. Corrientes 1234",
                provincia = "Corrientes",
                localidad = "Corrientes",
                condicion_fiscal = "Responsable Inscripto"
            });
        }

        // NUEVO: Buscar producto por código
        public void BuscarProductoPorCodigo(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return;

            var producto = Productos.FirstOrDefault(p =>
                p.codigo_barras == codigo ||
                p.codigo_interno == codigo ||
                p.Id_Producto.ToString() == codigo);

            if (producto != null)
            {
                AgregarProducto(producto);
                CodigoProductoBuscar = ""; // Limpiar el campo
            }
            else
            {
                MessageBox.Show($"No se encontró producto con código: {codigo}",
                               "Producto no encontrado", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void AgregarProducto(Producto producto)
        {
            if (producto.Stock <= 0)
            {
                MessageBox.Show("No hay stock disponible para este producto.", "Stock Insuficiente",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var itemExistente = Carrito.FirstOrDefault(x => x.IdProducto == producto.Id_Producto);

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

            producto.Stock--;
            CalcularTotal();
        }

        public void AgregarUnaUnidad(CarritoItem item)
        {
            var producto = Productos.FirstOrDefault(p => p.Id_Producto == item.IdProducto);

            if (producto == null || producto.Stock <= 0)
            {
                MessageBox.Show("No hay más stock disponible para este producto.", "Stock Insuficiente",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            item.Cantidad++;
            producto.Stock--;
            CalcularTotal();
        }

        public void QuitarProducto(CarritoItem item)
        {
            var producto = Productos.FirstOrDefault(p => p.Id_Producto == item.IdProducto);
            if (producto != null)
            {
                producto.Stock += item.Cantidad;
            }

            Carrito.Remove(item);
            CalcularTotal();
        }

        public void QuitarUnaUnidad(CarritoItem item)
        {
            var producto = Productos.FirstOrDefault(p => p.Id_Producto == item.IdProducto);
            if (producto != null)
            {
                producto.Stock++;
            }

            item.Cantidad--;

            if (item.Cantidad <= 0)
            {
                Carrito.Remove(item);
            }

            CalcularTotal();
        }

        // NUEVO: Aplicar descuento
        public void AplicarDescuentoPorcentaje(double porcentaje)
        {
            if (porcentaje < 0 || porcentaje > 100 || Carrito.Count == 0) return;

            foreach (var item in Carrito)
            {
                item.AplicarDescuentoPorcentaje(porcentaje);
            }

            CalcularTotal();
            MessageBox.Show($"Descuento del {porcentaje}% aplicado a todos los productos.",
                           "Descuento Aplicado", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CalcularTotal()
        {
            Total = Carrito.Sum(item => item.Subtotal);
        }

        private void ValidarClienteSegunComprobante()
        {
            if (TipoComprobanteSeleccionado?.requiere_cuit == true &&
                ClienteSeleccionado?.condicion_fiscal == "Consumidor Final")
            {
                MessageBox.Show("Para este tipo de comprobante necesita seleccionar un cliente con CUIT válido.",
                               "Validación de Comprobante", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void FiltrarClientes(string filtro)
        {
            ClientesFiltrados.Clear();

            if (string.IsNullOrWhiteSpace(filtro))
            {
                return;
            }

            var filtroLower = filtro.ToLower().Trim();

            var clientesFiltrados = Clientes.Where(c =>
                c.nombre.ToLower().Contains(filtroLower) ||
                c.apellido.ToLower().Contains(filtroLower) ||
                c.cuit.Contains(filtro) ||
                $"{c.nombre} {c.apellido}".ToLower().Contains(filtroLower) ||
                c.NombreCompleto.ToLower().Contains(filtroLower)
            ).OrderBy(c => c.nombre).ThenBy(c => c.apellido).ToList();

            foreach (var cliente in clientesFiltrados)
            {
                ClientesFiltrados.Add(cliente);
            }

            System.Diagnostics.Debug.WriteLine($"Filtro: '{filtro}' - Total clientes disponibles: {Clientes.Count} - Clientes filtrados encontrados: {ClientesFiltrados.Count}");
        }

        public void SeleccionarCliente(Cliente cliente)
        {
            ClienteSeleccionado = cliente;
        }

        public bool PuedeFinalizarVenta()
        {
            return Carrito.Count > 0 && ClienteSeleccionado != null;
        }

        // MEJORADO: Finalizar venta con comprobante
        public void FinalizarVenta()
        {
            if (!PuedeFinalizarVenta())
            {
                MessageBox.Show("Debe seleccionar un cliente y agregar productos al carrito.",
                    "Venta Incompleta", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TipoComprobanteSeleccionado == null)
            {
                MessageBox.Show("Debe seleccionar un tipo de comprobante.",
                    "Tipo de Comprobante Requerido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MetodoPagoSeleccionado == null)
            {
                MessageBox.Show("Debe seleccionar un método de pago.",
                    "Método de Pago Requerido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar monto recibido para efectivo
            if (MetodoPagoSeleccionado.requiere_vuelto && MontoRecibido < Total)
            {
                MessageBox.Show("El monto recibido debe ser mayor o igual al total de la venta.",
                    "Monto Insuficiente", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var venta = GuardarVentaEnBD();
                MostrarComprobante(venta);
                LimpiarVenta();

                MessageBox.Show("¡Venta registrada exitosamente!", "Venta Completada",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al procesar la venta: {ex.Message}", "Error",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Venta GuardarVentaEnBD()
        {
            using (var context = new ApplicationDbContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        // Generar número de comprobante
                        var numeroComprobante = GenerarNumeroComprobante(context);

                        // Crear venta
                        var venta = new Venta
                        {
                            Fecha = DateTime.Now,
                            Monto = Total,
                            id_Cliente = ClienteSeleccionado.id_cliente,
                            id_Usuario = 1, // TODO: Obtener usuario actual
                            NumeroComprobante = numeroComprobante,
                            IdTipoComprobante = TipoComprobanteSeleccionado.id_tipo,
                            MetodoPago = MetodoPagoSeleccionado.nombre,
                            MontoRecibido = MetodoPagoSeleccionado.requiere_vuelto ? MontoRecibido : null,
                            Observaciones = Observaciones,
                            Estado = "Completada"
                        };

                        context.Venta.Add(venta);
                        context.SaveChanges();

                        // Crear detalle
                        foreach (var item in Carrito)
                        {
                            var detalle = new DetalleVenta
                            {
                                id_venta = venta.id_venta,
                                id_producto = item.IdProducto,
                                nombre_producto = item.Nombre,
                                cantidad = item.Cantidad,
                                precio_unitario = (decimal)item.Precio,
                                precio_con_descuento = item.TieneDescuento ? (decimal?)item.PrecioConDescuento : null
                            };

                            context.DetalleVenta.Add(detalle);
                        }

                        context.SaveChanges();
                        transaction.Commit();

                        // Cargar datos relacionados para mostrar
                        venta.Cliente = ClienteSeleccionado;
                        venta.TipoComprobante = TipoComprobanteSeleccionado;

                        return venta;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private string GenerarNumeroComprobante(ApplicationDbContext context)
        {
            var ultimoNumero = context.Venta
                .Where(v => v.IdTipoComprobante == TipoComprobanteSeleccionado.id_tipo)
                .Max(v => (int?)v.NumeroSecuencial) ?? 0;

            return $"001-{TipoComprobanteSeleccionado.codigo}-{(ultimoNumero + 1):D6}";
        }

        private void MostrarComprobante(Venta venta)
        {
            var comprobante = GenerarTextoComprobante(venta);

            // Por ahora mostrar en MessageBox, después puedes hacer una ventana de impresión
            MessageBox.Show(comprobante, $"Comprobante: {venta.NumeroComprobante}",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private string GenerarTextoComprobante(Venta venta)
        {
            var sb = new StringBuilder();
            sb.AppendLine("================================================");
            sb.AppendLine("             HARDWARE GG");
            sb.AppendLine("         Venta de Mostrador");
            sb.AppendLine("================================================");
            sb.AppendLine($"Comprobante: {venta.NumeroComprobante}");
            sb.AppendLine($"Fecha: {venta.Fecha:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"Cliente: {venta.Cliente.NombreCompleto}");
            if (!string.IsNullOrEmpty(venta.Cliente.cuit))
                sb.AppendLine($"CUIT: {venta.Cliente.cuit}");
            sb.AppendLine("================================================");

            foreach (var item in Carrito)
            {
                sb.AppendLine($"{item.Nombre}");
                var precio = item.TieneDescuento ? item.PrecioConDescuento.Value : item.Precio;
                sb.AppendLine($"  {item.Cantidad} x ${precio:F2} = ${item.Subtotal:F2}");
                if (item.TieneDescuento)
                    sb.AppendLine($"  (Descuento: {item.PorcentajeDescuento:F1}%)");
            }

            sb.AppendLine("================================================");
            sb.AppendLine($"TOTAL: ${Total:F2}");
            if (venta.MontoRecibido.HasValue)
            {
                sb.AppendLine($"Recibido: ${venta.MontoRecibido:F2}");
                sb.AppendLine($"Vuelto: ${venta.Vuelto:F2}");
            }
            sb.AppendLine($"Método de Pago: {venta.MetodoPago}");
            sb.AppendLine("================================================");
            sb.AppendLine("        ¡Gracias por su compra!");
            sb.AppendLine("================================================");

            return sb.ToString();
        }

        public void CancelarVenta()
        {
            foreach (var item in Carrito)
            {
                var producto = Productos.FirstOrDefault(p => p.Id_Producto == item.IdProducto);
                if (producto != null)
                {
                    producto.Stock += item.Cantidad;
                }
            }

            LimpiarVenta();
        }

        private void LimpiarVenta()
        {
            Carrito.Clear();
            ClienteSeleccionado = null;
            ClientesFiltrados.Clear();
            Total = 0;
            MontoRecibido = 0;
            Observaciones = "";
            CodigoProductoBuscar = "";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}