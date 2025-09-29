using GGHardware.Data;
using GGHardware.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GGHardware.Views
{
    public partial class Gerente : UserControl
    {
        private readonly ApplicationDbContext _context;

        public Gerente()
        {
            InitializeComponent();
            _context = new ApplicationDbContext(); // tu DbContext
            CargarDatos();
        }

        private void CargarDatos()
        {
            // 1️⃣ Estadísticas generales
            txtTotalVentas.Text = _context.Venta.Sum(v => v.Monto).ToString("N0"); // suma total de ventas
            txtUsuariosActivos.Text = _context.Usuarios.Count(u => u.Activo).ToString();
            txtUsuariosInactivos.Text = _context.Usuarios.Count(u => !u.Activo).ToString();
            txtClientesFrecuentes.Text = _context.Venta
                                              .GroupBy(v => v.id_Cliente)
                                              .OrderByDescending(g => g.Count())
                                              .Take(1)
                                              .Count()
                                              .ToString();

            // 2️⃣ Productos más vendidos
            var productos = _context.DetalleVenta
                            .GroupBy(v => v.nombre_producto)
                            .Select(g => new ProductoVendidos
                            {
                                Nombre = g.Key,
                                Cantidad = g.Sum(v => v.cantidad)
                            })
                            .OrderByDescending(p => p.Cantidad)
                            .ToList();

            // 3️⃣ Cargar DataGrid
            dgProductosVendidos.ItemsSource = productos;

            // 4️⃣ Llenar gráfico con ProgressBars
            int maxCantidad = productos.Any() ? productos.Max(p => p.Cantidad) : 1;

            var barras = productos.Select(p => new ProductoGrafico
            {
                Nombre = p.Nombre,
                Cantidad = (double)p.Cantidad / maxCantidad * 100 
            }).ToList();

            icGraficoProductos.ItemsSource = barras;
        }
    }

    // Clase auxiliar para DataGrid
    public class ProductoVendidos
    {
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }

    // Clase auxiliar para gráfico de ProgressBars
    public class ProductoGrafico
    {
        public string Nombre { get; set; } = string.Empty;
        public double Cantidad { get; set; } // valor normalizado a 100
    }


}
