using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using GGHardware.Models;
using GGHardware.Data;
using Microsoft.EntityFrameworkCore;

namespace GGHardware.ViewModels
{
    public class VentasViewModel : INotifyPropertyChanged
    {
        // Colecciones observables para la UI
        public ObservableCollection<Producto> Productos { get; set; }
        public ObservableCollection<Cliente> Clientes { get; set; }
        public ObservableCollection<Cliente> ClientesFiltrados { get; set; }
        public ObservableCollection<CarritoItem> Carrito { get; set; }

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

        private double _total;
        public double Total
        {
            get => _total;
            set
            {
                _total = value;
                OnPropertyChanged(nameof(Total));
            }
        }

        public VentasViewModel()
        {
            Productos = new ObservableCollection<Producto>();
            Clientes = new ObservableCollection<Cliente>();
            ClientesFiltrados = new ObservableCollection<Cliente>();
            Carrito = new ObservableCollection<CarritoItem>();

            CargarDatos();
        }

        private void CargarDatos()
        {
            CargarProductosPrueba(); // Mantener productos de prueba por ahora
            CargarClientesDesdeBaseDatos(); // CAMBIO: Cargar clientes reales desde BD
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
                System.Windows.MessageBox.Show($"Error al cargar clientes desde la base de datos: {ex.Message}",
                    "Error de Base de Datos", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);

                // En caso de error, cargar algunos clientes de prueba como fallback
                CargarClientesPrueba();
            }
        }

        // Método público para recargar clientes (llamar cuando se agregue un cliente nuevo)
        public void RecargarClientes()
        {
            CargarClientesDesdeBaseDatos();
            ClientesFiltrados.Clear(); // Limpiar filtrados también
        }

        private void CargarProductosPrueba()
        {
            // Cargar productos de prueba (mantener como está)
            Productos.Add(new Producto { Id_Producto = 1, Nombre = "Mouse Logitech", precio_venta = 2500, Stock = 10, precio_costo = 1500, descripcion = "Mouse óptico inalámbrico" });
            Productos.Add(new Producto { Id_Producto = 2, Nombre = "Teclado Mecánico", precio_venta = 8900, Stock = 5, precio_costo = 5000, descripcion = "Teclado mecánico RGB" });
            Productos.Add(new Producto { Id_Producto = 3, Nombre = "Monitor 24\"", precio_venta = 45000, Stock = 3, precio_costo = 35000, descripcion = "Monitor LED Full HD" });
            Productos.Add(new Producto { Id_Producto = 4, Nombre = "Memoria RAM 8GB", precio_venta = 15000, Stock = 8, precio_costo = 10000, descripcion = "RAM DDR4 3200MHz" });
        }

        private void CargarClientesPrueba()
        {
            // Solo como fallback en caso de error al acceder a la BD
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

        public void AgregarProducto(Producto producto)
        {
            if (producto.Stock <= 0)
            {
                System.Windows.MessageBox.Show("No hay stock disponible para este producto.", "Stock Insuficiente",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            // Verificar si el producto ya está en el carrito usando tu modelo
            var itemExistente = Carrito.FirstOrDefault(x => x.IdProducto == producto.Id_Producto);

            if (itemExistente != null)
            {
                // Si ya existe, aumentar cantidad
                itemExistente.Cantidad++;
            }
            else
            {
                // Si no existe, agregar nuevo item usando tu modelo CarritoItem
                Carrito.Add(new CarritoItem
                {
                    IdProducto = producto.Id_Producto,
                    Nombre = producto.Nombre,
                    Precio = producto.precio_venta,
                    Cantidad = 1
                });
            }

            // Reducir stock del producto
            producto.Stock--;

            // Actualizar total
            CalcularTotal();
        }

        // NUEVO: Método para agregar una unidad de un producto ya existente en el carrito
        public void AgregarUnaUnidad(CarritoItem item)
        {
            // Buscar el producto para verificar stock
            var producto = Productos.FirstOrDefault(p => p.Id_Producto == item.IdProducto);

            if (producto == null || producto.Stock <= 0)
            {
                System.Windows.MessageBox.Show("No hay más stock disponible para este producto.", "Stock Insuficiente",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            // Aumentar cantidad en el carrito
            item.Cantidad++;

            // Reducir stock del producto
            producto.Stock--;

            // Actualizar total
            CalcularTotal();
        }

        public void QuitarProducto(CarritoItem item)
        {
            // Devolver stock al producto usando tu modelo
            var producto = Productos.FirstOrDefault(p => p.Id_Producto == item.IdProducto);
            if (producto != null)
            {
                producto.Stock += item.Cantidad;
            }

            // Quitar del carrito
            Carrito.Remove(item);

            // Actualizar total
            CalcularTotal();
        }

        // NUEVO: Método para quitar solo una unidad del producto
        public void QuitarUnaUnidad(CarritoItem item)
        {
            // Devolver una unidad al stock
            var producto = Productos.FirstOrDefault(p => p.Id_Producto == item.IdProducto);
            if (producto != null)
            {
                producto.Stock++;
            }

            // Reducir cantidad en el carrito
            item.Cantidad--;

            // Si la cantidad llega a 0, quitar el item completamente
            if (item.Cantidad <= 0)
            {
                Carrito.Remove(item);
            }

            // Actualizar total
            CalcularTotal();
        }

        private void CalcularTotal()
        {
            Total = Carrito.Sum(item => item.Precio * item.Cantidad);
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

            // Debug para verificar
            System.Diagnostics.Debug.WriteLine($"Filtro: '{filtro}' - Total clientes disponibles: {Clientes.Count} - Clientes filtrados encontrados: {ClientesFiltrados.Count}");

            if (ClientesFiltrados.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("No se encontraron clientes. Verificar si hay clientes en la base de datos.");
            }
        }

        public void SeleccionarCliente(Cliente cliente)
        {
            ClienteSeleccionado = cliente;
        }

        public bool PuedeFinalizarVenta()
        {
            return Carrito.Count > 0 && ClienteSeleccionado != null;
        }

        public void FinalizarVenta()
        {
            if (!PuedeFinalizarVenta())
            {
                System.Windows.MessageBox.Show("Debe seleccionar un cliente y agregar productos al carrito.",
                    "Venta Incompleta", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            // Crear la venta usando tu modelo
            var venta = new Venta
            {
                Fecha = DateTime.Now,
                Monto = Total,
                id_Cliente = ClienteSeleccionado.id_cliente, // Usando tu campo id_cliente
                id_Usuario = 1, // Asumiendo usuario logueado
                Cliente = ClienteSeleccionado
            };

            // Aquí guardarías la venta en la base de datos
            System.Windows.MessageBox.Show($"Venta finalizada correctamente.\nTotal: {Total:C}\nCliente: {ClienteSeleccionado.nombre} {ClienteSeleccionado.apellido}",
                "Venta Exitosa", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

            // Limpiar carrito y cliente seleccionado
            LimpiarVenta();
        }

        public void CancelarVenta()
        {
            // Devolver stock a los productos
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
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}