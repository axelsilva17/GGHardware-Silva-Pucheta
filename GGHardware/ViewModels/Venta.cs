using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using GGHardware.Data;
using GGHardware.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Net;
using System.Net.Mail;
using Microsoft.Win32;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Font;
using iText.IO.Font.Constants;

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
        public ObservableCollection<DetalleVenta> DetallesVenta { get; set; }


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

        private decimal _montoRecibido;
        public decimal MontoRecibido
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

        public decimal Vuelto => MontoRecibido - Total;

        public decimal Total => Carrito.Sum(item => item.Subtotal);

        public VentasViewModel()
        {
            _context = new ApplicationDbContext();
            Productos = new ObservableCollection<Producto>();
            ClientesFiltrados = new ObservableCollection<Cliente>();
            Carrito = new ObservableCollection<CarritoItem>();
            TiposComprobante = new ObservableCollection<TipoComprobante>();
            MetodosPago = new ObservableCollection<MetodoPago>();
            DetallesVenta = new ObservableCollection<DetalleVenta>();

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

        public int ObtenerStockDisponible(int idProducto)
        {
            var productoDb = _context.Producto.Find(idProducto);
            if (productoDb == null) return 0;

            var itemEnCarrito = Carrito.FirstOrDefault(c => c.IdProducto == idProducto);
            int cantidadEnCarrito = itemEnCarrito?.Cantidad ?? 0;

            return (int)productoDb.Stock - cantidadEnCarrito;
        }

        public void AgregarProducto(Producto producto)
        {
            var stockDisponible = ObtenerStockDisponible(producto.Id_Producto);

            if (stockDisponible <= 0)
            {
                var cantidadEnCarrito = Carrito.FirstOrDefault(c => c.IdProducto == producto.Id_Producto)?.Cantidad ?? 0;
                MessageBox.Show($"No hay más stock disponible de '{producto.Nombre}'.\nStock total: {(int)producto.Stock}...Ya en carrito: {cantidadEnCarrito}",
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
                    Precio = (int)producto.precio_venta,
                    Cantidad = 1
                });
            }

            OnPropertyChanged(nameof(Total));
            RefrescarProductos();
        }

        private void RefrescarProductos()
        {
            var tempList = Productos.ToList();
            Productos.Clear();
            foreach (var prod in tempList)
            {
                Productos.Add(prod);
            }
        }

        public void AgregarUnaUnidad(CarritoItem item)
        {
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

            if (MostrarCampoVuelto && MontoRecibido < Total)
            {
                MessageBox.Show("El monto recibido no puede ser menor al total", "Advertencia",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
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

                var venta = new Venta
                {
                    id_Cliente = ClienteSeleccionado.id_cliente,
                    id_Usuario = 1,
                    Fecha = DateTime.Now,
                    Monto = Total,
                    IdTipoComprobante = TipoComprobanteSeleccionado.id_tipo,
                    MetodoPago = MetodoPagoSeleccionado?.nombre ?? string.Empty,
                    MontoRecibido = MostrarCampoVuelto ? MontoRecibido : (decimal?)null,
                    Observaciones = Observaciones ?? string.Empty,
                    Estado = "Completada"
                };

                _context.Venta.Add(venta);
                _context.SaveChanges();

                foreach (var item in Carrito)
                {
                    var detalle = new DetalleVenta
                    {
                        id_venta = venta.id_venta,
                        id_producto = item.IdProducto,
                        nombre_producto = item.Nombre ?? "Sin nombre",
                        cantidad = item.Cantidad,
                        precio_unitario = item.Precio,
                        precio_con_descuento = item.PrecioConDescuento ?? item.Precio,
                        Activo = true
                    };
                    _context.DetalleVenta.Add(detalle);

                    var producto = _context.Producto.Find(item.IdProducto);
                    if (producto != null)
                    {
                        producto.Stock -= item.Cantidad;
                    }
                }

                _context.SaveChanges();

                string mensajeVuelto = MostrarCampoVuelto ? $"\nVuelto: {Vuelto:C}" : "";
                MessageBox.Show($"Venta registrada correctamente\nTotal: {Total:C}{mensajeVuelto}",
                    "Venta Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

                CancelarVenta();
                CargarProductos();
                OnPropertyChanged(nameof(Productos));
            }
            catch (Exception ex)
            {
                string detalleError = ex.InnerException != null ? $"\n\nDetalle: {ex.InnerException.Message}" : "";
                MessageBox.Show($"Error al registrar la venta:\n{ex.Message}{detalleError}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

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

                // Mostrar menú de opciones
                var ventanaOpciones = new Window
                {
                    Title = "Opciones de Comprobante",
                    Width = 350,
                    Height = 250,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.NoResize
                };

                var stack = new System.Windows.Controls.StackPanel
                {
                    Margin = new Thickness(20)
                };

                var titulo = new System.Windows.Controls.TextBlock
                {
                    Text = $"Comprobante de Venta #{venta.id_venta}",
                    FontSize = 16,
                    FontWeight = System.Windows.FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 20)
                };
                stack.Children.Add(titulo);

                // Botón Ver en pantalla
                var btnVer = new System.Windows.Controls.Button
                {
                    Content = "Ver en Pantalla",
                    Height = 40,
                    Margin = new Thickness(0, 5, 0, 5)
                };
                btnVer.Click += (s, e) =>
                {
                    MostrarEnPantalla(venta);
                    ventanaOpciones.Close();
                };
                stack.Children.Add(btnVer);

                // Botón Guardar como PDF
                var btnPDF = new System.Windows.Controls.Button
                {
                    Content = "Guardar como PDF",
                    Height = 40,
                    Margin = new Thickness(0, 5, 0, 5)
                };
                btnPDF.Click += (s, e) =>
                {
                    GuardarComoPDF(venta);
                    ventanaOpciones.Close();
                };
                stack.Children.Add(btnPDF);

                // Botón Enviar por Email
                var btnEmail = new System.Windows.Controls.Button
                {
                    Content = "Enviar por Email",
                    Height = 40,
                    Margin = new Thickness(0, 5, 0, 5)
                };
                btnEmail.Click += (s, e) =>
                {
                    EnviarComprobantePorEmail(venta);
                    ventanaOpciones.Close();
                };
                stack.Children.Add(btnEmail);

                ventanaOpciones.Content = stack;
                ventanaOpciones.ShowDialog();
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

            sb.AppendLine(CentrarTexto("GGHardware", ancho));
            sb.AppendLine(CentrarTexto("9 de Julio 1890", ancho));
            sb.AppendLine(new string('=', ancho));
            sb.AppendLine();

            sb.AppendLine($"Tipo: {venta.TipoComprobanteNombre}");
            sb.AppendLine($"Nro: {venta.NumeroComprobanteFormateado}");
            sb.AppendLine($"Fecha: {venta.Fecha:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"Vendedor: {venta.Usuario?.Nombre ?? "N/A"}");
            sb.AppendLine();

            sb.AppendLine($"Cliente: {venta.ClienteNombre}");
            if (venta.Cliente != null && !string.IsNullOrEmpty(venta.Cliente.cuit))
                sb.AppendLine($"CUIT: {venta.Cliente.cuit}");
            sb.AppendLine(new string('-', ancho));
            sb.AppendLine();

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

            var totalDescuentos = venta.Detalles.Sum(d => d.MontoDescuento);
            if (totalDescuentos > 0)
            {
                var subtotal = venta.Detalles.Sum(d => d.precio_unitario * d.cantidad);
                sb.AppendLine($"Subtotal:        {subtotal,15:C}");
                sb.AppendLine($"Descuentos:      {totalDescuentos,15:C}");
            }

            sb.AppendLine($"TOTAL:           {venta.Monto,15:C}");
            sb.AppendLine();

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

            AgregarProducto(producto);
        }

        public void BuscarProducto()
        {
            MessageBox.Show("Funcionalidad de búsqueda avanzada\nUsa el campo 'Código' para búsqueda rápida",
                "Búsqueda de productos", MessageBoxButton.OK, MessageBoxImage.Information);
        }

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



        //Metodos para manejar envios de coomprobantes*******
        private void MostrarEnPantalla(Venta venta)
        {
            var ticket = GenerarTextoTicket(venta);

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
        
        private void GuardarComoPDF(Venta venta)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = $"Comprobante_{venta.id_venta}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                DefaultExt = "pdf"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    using (var writer = new PdfWriter(saveDialog.FileName))
                    using (var pdf = new PdfDocument(writer))
                    using (var document = new Document(pdf))
                    {
                        var font = PdfFontFactory.CreateFont(StandardFonts.COURIER);
                        document.SetFont(font).SetFontSize(10);

                        var texto = GenerarTextoTicket(venta);
                        document.Add(new Paragraph(texto));
                    }

                    MessageBox.Show($"Comprobante guardado exitosamente en:\n{saveDialog.FileName}",
                        "PDF Generado", MessageBoxButton.OK, MessageBoxImage.Information);

                    if (MessageBox.Show("¿Desea abrir el archivo PDF?", "Abrir PDF",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = saveDialog.FileName,
                            UseShellExecute = true
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al guardar PDF:\n{ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EnviarComprobantePorEmail(Venta venta)
        {
            try
            {
                if (string.IsNullOrEmpty(venta.Cliente?.email))
                {
                    MessageBox.Show("El cliente no tiene un email registrado.",
                        "Email no disponible", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Generar el contenido del ticket
                string contenidoTicket = GenerarTextoTicket(venta);

                // Preparar asunto y cuerpo del email
                string asunto = Uri.EscapeDataString($"Comprobante de Venta #{venta.id_venta} - GGHardware");

                string cuerpoEmail = $"Estimado/a {venta.Cliente.NombreCompleto},\n\n" +
                                   $"Adjuntamos el comprobante de su compra realizada el {venta.Fecha:dd/MM/yyyy HH:mm}.\n\n" +
                                   "----------------------------------------\n\n" +
                                   contenidoTicket + "\n\n" +
                                   "----------------------------------------\n\n" +
                                   "Gracias por su compra.\n\n" +
                                   "Saludos,\nGGHardware";

                string cuerpo = Uri.EscapeDataString(cuerpoEmail);

                // Construir URL de Gmail
                string gmailUrl = $"https://mail.google.com/mail/?view=cm&fs=1&to={venta.Cliente.email}&su={asunto}&body={cuerpo}";

                // Abrir Gmail en el navegador
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = gmailUrl,
                    UseShellExecute = true
                });

                MessageBox.Show($"Se abrió Gmail en su navegador para enviar el comprobante a:\n{venta.Cliente.email}\n\nPuede adjuntar el PDF manualmente si lo desea.",
                    "Gmail Abierto", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir Gmail:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GuardarPDFTemporal(Venta venta, string rutaArchivo)
        {
            using (var writer = new PdfWriter(rutaArchivo))
            using (var pdf = new PdfDocument(writer))
            using (var document = new Document(pdf))
            {
                var font = PdfFontFactory.CreateFont(StandardFonts.COURIER);
                document.SetFont(font).SetFontSize(10);
                var texto = GenerarTextoTicket(venta);
                document.Add(new Paragraph(texto));
            }
        }

        //************************************************/
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
        public void ReimprimirComprobante()
        {
            if (VentaSeleccionada == null)
            {
                MessageBox.Show("Seleccione una venta para imprimir",
                    "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Usar el mismo método del ViewModel de ventas
            var ventasVM = new VentasViewModel();
            ventasVM.GenerarTicket(VentaSeleccionada.id_venta);
        }

      


    }
}