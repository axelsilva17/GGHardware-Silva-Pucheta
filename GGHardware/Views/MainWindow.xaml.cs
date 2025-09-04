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
        }

        private void btnProductos_Click(object sender, RoutedEventArgs e)
        {
            // Limpiar el contenido previo
            MainContentBorder.Child = null;

            // Crear la vista de Producto y mostrarla
            ProductoView productoView = new ProductoView();
            MainContentBorder.Child = productoView;
        }
        private void BtnRegistro_Click(object sender, RoutedEventArgs e)
        {
            // Limpiar el contenido previo
            MainContentBorder.Child = null;

            /// Crear la vista de registro y pasar una referencia de esta ventana
            RegistroView registroView = new RegistroView(this);
            MainContentBorder.Child = registroView;
        }
        private void btnVentas_Click(object sender, RoutedEventArgs e)
        {
            // Limpia el contenido previo (opcional, pero buena práctica)
            MainContentBorder.Child = null;

            // Crea una nueva instancia de la vista de ventas y la muestra
            VentasView ventasView = new VentasView();

            MainContentBorder.Child = ventasView;
        }

        private void btnInicio_Click(object sender, RoutedEventArgs e)
        {
            // Limpia el contenido actual para asegurar que no haya nada residual.
            MainContentBorder.Child = null;

            // Crea una nueva instancia de InicioView.
            InicioView inicioView = new InicioView();

            // Asigna la nueva vista al borde de contenido principal.
            MainContentBorder.Child = inicioView;
        }

        // Agregado para futuras implementaciones
        private void btnConfiguracion_Click(object sender, RoutedEventArgs e)
        {
            // Lógica para la vista de configuración
        }
    }
}