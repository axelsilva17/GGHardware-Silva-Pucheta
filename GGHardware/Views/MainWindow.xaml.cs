using GGHardware.Views;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GGHardware.Data;
using GGHardware.Models;
using System.Linq;

namespace GGHardware
{
    public partial class MainWindow : Window
    {
        public void HabilitarMenu()
        {
            btnInicio.IsEnabled = true;
            btnProductos.IsEnabled = true;
            btnVentas.IsEnabled = true;
            btnConfiguracion.IsEnabled = true;
            btnRegistro.IsEnabled = true;
        }

        public MainWindow()
        {
            InitializeComponent();

            // Mostrar la vista de inicio por defecto
            MainContentBorder.Child = new InicioView();

            // Deshabilitar los botones de navegación hasta que el usuario inicie sesión
            btnInicio.IsEnabled = true;
            btnProductos.IsEnabled = true;
            btnVentas.IsEnabled = true;
            btnConfiguracion.IsEnabled = true;
            btnRegistro.IsEnabled = true;

            // ✅ Probar conexión a la base y a la tabla Usuarios
            ProbarConexionBD();
        }

        private void ProbarConexionBD()
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var usuario = context.Usuarios.FirstOrDefault();

                    if (usuario != null)
                    {
                        MessageBox.Show($"Conectado correctamente ✅\nPrimer usuario: {usuario.Nombre} {usuario.apellido}",
                                        "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Conexión OK ✅ pero la tabla Usuarios está vacía.",
                                        "Información", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error al conectar con la base de datos:\n{ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnProductos_Click(object sender, RoutedEventArgs e)
        {
            MainContentBorder.Child = null;
            ProductoView productoView = new ProductoView();
            MainContentBorder.Child = productoView;
        }

        private void BtnRegistro_Click(object sender, RoutedEventArgs e)
        {
            MainContentBorder.Child = null;
            RegistroView registroView = new RegistroView(this);
            MainContentBorder.Child = registroView;
        }

        private void btnVentas_Click(object sender, RoutedEventArgs e)
        {
            MainContentBorder.Child = null;
            VentasView ventasView = new VentasView();
            MainContentBorder.Child = ventasView;
        }

        private void btnInicio_Click(object sender, RoutedEventArgs e)
        {
            MainContentBorder.Child = null;
            InicioView inicioView = new InicioView();
            MainContentBorder.Child = inicioView;
        }

        private void btnReportes_Click(object sender, RoutedEventArgs e)
        {
            MainContentBorder.Child = new Reportes();
        }

        private void btnBackup_Click(object sender, RoutedEventArgs e)
        {
            MainContentBorder.Child = new GGHardware.Views.Backup();
        }

        private void btnConfiguracion_Click(object sender, RoutedEventArgs e)
        {
            // Lógica para la vista de configuración
        }
    }
}
