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

        // Propiedad corregida para manejar la colección de productos
        public ObservableCollection<Producto> Productos { get; set; }
        public ObservableCollection<CarritoItem> Carrito { get; set; }
        public double Total => Carrito.Sum(c => c.Precio * c.Cantidad);

        // Comandos
        public RelayCommand AgregarProductoCommand { get; set; }
        public RelayCommand QuitarProductoCommand { get; set; }
        public RelayCommand FinalizarVentaCommand { get; set; }

        public VentasViewModel()
        {
            _context = new ApplicationDbContext();

            // Inicializa la colección de productos correctamente
            Productos = new ObservableCollection<Producto>(_context.Producto.ToList());
            Carrito = new ObservableCollection<CarritoItem>();

            // Inicializar comandos
            AgregarProductoCommand = new RelayCommand(o => AgregarProducto(o as Producto));
            QuitarProductoCommand = new RelayCommand(o => QuitarProducto(o as CarritoItem));
            FinalizarVentaCommand = new RelayCommand(o => FinalizarVenta());
        }

        public void AgregarProducto(Producto producto)
        {
            if (producto == null) return;

            var item = Carrito.FirstOrDefault(c => c.IdProducto == producto.Id_Producto);
            if (item != null) item.Cantidad++;
            else
            {
                Carrito.Add(new CarritoItem
                {
                    IdProducto = producto.Id_Producto,
                    Nombre = producto.Nombre,
                   Precio = producto.precio_venta, // Usar Precio_venta para el precio
                    Cantidad = 1
                });
            }
            OnPropertyChanged(nameof(Total));
        }

        private void QuitarProducto(CarritoItem item)
        {
            if (item == null) return;

            Carrito.Remove(item);
            OnPropertyChanged(nameof(Total));
        }

        private void FinalizarVenta()
        {
            if (!Carrito.Any())
            {
                MessageBox.Show("El carrito está vacío.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Para probar, cliente y usuario fijos
            int idCliente = 1;
            int idUsuario = 1;

            var venta = new Venta
            {
                Fecha = DateTime.Now,
                Monto = Total,
                id_Cliente = idCliente,
                id_Usuario = idUsuario
            };

            // 🔹 Descontar stock de los productos
            foreach (var item in Carrito)
            {
                // Accede a la colección de productos desde el contexto
                var producto = _context.Producto.FirstOrDefault(p => p.Id_Producto == item.IdProducto);
                if (producto != null)
                {
                    if (producto.Stock >= item.Cantidad)
                    {
                        producto.Stock -= item.Cantidad;
                    }
                    else
                    {
                        MessageBox.Show($"No hay suficiente stock para {producto.Nombre}.",
                            "Error de stock", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return; // corta la venta si falta stock
                    }
                }
            }

            // Guardar la venta
            _context.Venta.Add(venta);
            _context.SaveChanges();

            MessageBox.Show($"Venta registrada. Total: ${Total}",
                "Venta finalizada", MessageBoxButton.OK, MessageBoxImage.Information);

            Carrito.Clear();

            // 🔹 Refrescar productos en pantalla después de guardar
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
}