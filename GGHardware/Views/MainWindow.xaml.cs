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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnProductos_Click(object sender, RoutedEventArgs e)
        {
            // Limpiar el contenido previo
            MainContentBorder.Child = null;

            // Crear la vista de Producto y mostrarla
            ProductoView productoView = new ProductoView();
            MainContentBorder.Child = productoView;
        }
        private void btnUsuarios_Click(object sender, RoutedEventArgs e)
        {
            // Limpiar el contenido previo
            MainContentBorder.Child = null;

            // Crear la vista de Usuarios y mostrarla
            Usuarios usuariosView = new Usuarios();
            MainContentBorder.Child = usuariosView;
        }
        private void btnVentas_Click(object sender, RoutedEventArgs e)
        {
            // Limpia el contenido previo (opcional, pero buena práctica)
            MainContentBorder.Child = null;

            // Crea una nueva instancia de la vista de ventas y la muestra
            VentasView ventasView = new VentasView();
            MainContentBorder.Child = ventasView;
        }
    }
}