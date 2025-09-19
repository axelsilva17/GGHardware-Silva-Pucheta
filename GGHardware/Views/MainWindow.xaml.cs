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
using MaterialDesignThemes.Wpf;

namespace GGHardware
{
    public partial class MainWindow : Window
    {
        public static Usuario? UsuarioActual { get; set; }

        public void HabilitarMenu()
        {
            mainMenu.IsEnabled = true;
            mainMenu.Opacity = 1.0;
        }
        private void ToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            var theme = Application.Current.Resources.MergedDictionaries
                .OfType<BundledTheme>()
                .FirstOrDefault();

            if (theme != null)
            {
                theme.BaseTheme = theme.BaseTheme == BaseTheme.Dark
                    ? BaseTheme.Light
                    : BaseTheme.Dark;
            }
        }


        public MainWindow()
        {
            InitializeComponent();

            // Mostrar la vista de inicio por defecto
            MainContentBorder.Child = new InicioView();

            // Deshabilitar los botones de navegación hasta que el usuario inicie sesión
            // Deshabilita los botones y haz el menú semitransparente
            mainMenu.IsEnabled = false; // mainMenu es el nombre de tu panel de navegación
            mainMenu.Opacity = 0.5; // El valor 0.5 lo hace semitransparente
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
