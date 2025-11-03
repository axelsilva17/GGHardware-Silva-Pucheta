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
            if (MainWindow.UsuarioActual != null)
            {
                txtSaludo.Text = $"¡Hola, {MainWindow.UsuarioActual.Nombre}!";
                //SI INICIA UN SUPERVISOR NO MOSTRAR VENTAS DEL DIA
                if (MainWindow.UsuarioActual.id_usuario == 1)
                {
                    txtVentasDelDia.Text = string.Empty;
                    txtCantidadVentas.Text = string.Empty;
                    return; // salir del método
                }

                using (var context = new ApplicationDbContext())
                {
                    var ventasDelDia = context.Venta
                        .Where(v => v.id_Usuario == MainWindow.UsuarioActual.id_usuario
                                 && v.Fecha.Date == DateTime.Today
                                 && v.Estado != "Anulada")
                        .ToList();

                    decimal ventasHoy = ventasDelDia.Sum(v => v.Monto);
                    int cantidadVentas = ventasDelDia.Count;

                    txtVentasDelDia.Text = ventasHoy.ToString("C");
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
