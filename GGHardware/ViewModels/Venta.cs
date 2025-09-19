using GGHardware.Data;
using GGHardware.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace GGHardware.ViewModels
{
    public class VentasViewModel : INotifyPropertyChanged
    {
        private readonly ApplicationDbContext _context;

        // Colecciones
        public ObservableCollection<Producto> Productos { get; set; }
        public ObservableCollection<CarritoItem> Carrito { get; set; }

        public double Total => Carrito.Sum(c => c.Precio * c.Cantidad);

        // Comandos simulados como métodos
        public VentasViewModel()
        {
            _context = new ApplicationDbContext();

            Productos = new ObservableCollection<Producto>(_context.Producto.ToList());
            Carrito = new ObservableCollection<CarritoItem>();
        }

        // Método para filtrar clientes (simulado)
        public void FiltrarClientes(string filtro)
        {
            // Aquí podrías filtrar tu lista de clientes si la tuvieras
            // Por ahora solo ejemplo:
            Console.WriteLine($"Filtrando clientes por: {filtro}");
        }

        // Agregar producto al carrito
        public void AgregarProducto(Producto producto)
        {
            if (producto == null) return;

            var item = Carrito.FirstOrDefault(c => c.IdProducto == producto.Id_Producto);
            if (item != null)
                item.Cantidad++;
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

        // Quitar producto del carrito
        public void QuitarProducto(CarritoItem item)
        {
            if (item == null) return;

            Carrito.Remove(item);
            OnPropertyChanged(nameof(Total));
        }

        // Finalizar venta
        public void FinalizarVenta()
        {
            if (!Carrito.Any())
            {
                MessageBox.Show("El carrito está vacío.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int idCliente = 1; // Cliente fijo para prueba
            int idUsuario = 1; // Usuario fijo para prueba

            var venta = new Venta
            {
                Fecha = DateTime.Now,
                Monto = Total,
                id_Cliente = idCliente,
                id_Usuario = idUsuario
            };

            // Verificar stock y descontar
            foreach (var item in Carrito)
            {
                var producto = _context.Producto.FirstOrDefault(p => p.Id_Producto == item.IdProducto);
                if (producto != null)
                {
                    if (producto.Stock >= item.Cantidad)
                        producto.Stock -= item.Cantidad;
                    else
                    {
                        MessageBox.Show($"No hay suficiente stock para {producto.Nombre}.",
                            "Error de stock", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
            }

            _context.Venta.Add(venta);
            _context.SaveChanges();

            MessageBox.Show($"Venta registrada. Total: ${Total}",
                "Venta finalizada", MessageBoxButton.OK, MessageBoxImage.Information);

            Carrito.Clear();

            // Refrescar productos en pantalla
            Productos = new ObservableCollection<Producto>(_context.Producto.ToList());
            OnPropertyChanged(nameof(Productos));
            OnPropertyChanged(nameof(Total));
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    // Clase para el carrito
    public class CarritoItem
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; } = "";
        public double Precio { get; set; }
        public int Cantidad { get; set; }
    }
}
