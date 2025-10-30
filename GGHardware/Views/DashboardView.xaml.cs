using System.Linq;
using System.Windows.Controls;
using GGHardware.Data;
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
                    var ventasDelDia = context.Venta
                        .Where(v => v.id_Usuario == MainWindow.UsuarioActual.id_usuario
                                 && v.Fecha.Date == DateTime.Today
                                 && v.Estado != "Anulada") // Excluir ventas anuladas
                        .ToList();

                    // Calcular el total de ventas
                    decimal ventasHoy = ventasDelDia.Sum(v => v.Monto);

                    // Contar la cantidad de ventas
                    int cantidadVentas = ventasDelDia.Count;

                    // Actualizar los TextBlocks
                    txtVentasDelDia.Text = ventasHoy.ToString("C"); // Formato moneda
                    txtCantidadVentas.Text = cantidadVentas == 1
                        ? "1 venta realizada"
                        : $"{cantidadVentas} ventas realizadas";
                }
            }
            else
            {
                txtSaludo.Text = "¡Bienvenido!";
                txtVentasDelDia.Text = "$ 0,00";
                txtCantidadVentas.Text = "0 ventas realizadas";
            }
        }
    }
}