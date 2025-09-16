using System.Linq;
using System.Windows.Controls;
using GGHardware.Data; // Asegúrate de tener esta referencia
using System;

namespace GGHardware.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
            CargarVentasDelDia();
        }

        private void CargarVentasDelDia()
        {
            // Verifica que haya un usuario logueado
            if (MainWindow.UsuarioActual != null)
            {
                // Muestra un saludo personalizado
                txtSaludo.Text = $"¡Hola, {MainWindow.UsuarioActual.Nombre}!";

                using (var context = new ApplicationDbContext())
                {
                    // Obtener las ventas del día para el usuario actual
                    // (Ajusta esta consulta a tu modelo de datos)
                    decimal ventasHoy = (decimal)context.Venta
                        .Where(v => v.id_Usuario == MainWindow.UsuarioActual.id_usuario && v.Fecha.Date == DateTime.Today)
                        .Sum(v => v.Monto);

                    // Actualizar el TextBlock con el total de ventas
                    txtVentasDelDia.Text = ventasHoy.ToString("$");
                }
            }
            else
            {
                txtSaludo.Text = "¡Bienvenido!";
                txtVentasDelDia.Text = "$ 0,00";
            }
        }
    }
}