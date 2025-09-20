using GGHardware.Data;
using GGHardware.Models;
using GGHardware.Views;
using MaterialDesignThemes.Wpf;
using System.Linq;
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
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace GGHardware
{
    public partial class MainWindow : Window
    {

        private bool _isDarkTheme = false;
        private readonly PaletteHelper _paletteHelper = new PaletteHelper();
        public static Usuario? UsuarioActual { get; set; }

        public void HabilitarMenu()
        {
            mainMenu.IsEnabled = true;
            mainMenu.Opacity = 1.0;
        }

        public MainWindow()
        {
            InitializeComponent();
            ApplyLightTheme();

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


        private void ToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            _isDarkTheme = !_isDarkTheme;

            if (_isDarkTheme)
            {
                ApplyDarkTheme();
            }
            else
            {
                ApplyLightTheme();
            }
        }

        private void ApplyDarkTheme()
        {
            // Cambiar colores a modo oscuro (Azules con texto blanco)
            Application.Current.Resources["PrimaryBrush"] = new SolidColorBrush(Color.FromRgb(0x15, 0x65, 0xC0)); // Azul
            Application.Current.Resources["PrimaryForegroundBrush"] = new SolidColorBrush(Colors.White);
            Application.Current.Resources["Background"] = new SolidColorBrush(Color.FromRgb(0x12, 0x12, 0x12)); // Negro
            Application.Current.Resources["Surface"] = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x1E)); // Gris oscuro
            Application.Current.Resources["TextPrimary"] = new SolidColorBrush(Colors.White);
            Application.Current.Resources["TextSecondary"] = new SolidColorBrush(Color.FromRgb(0xB0, 0xB0, 0xB0));

            // Actualizar el contenido del botón
            UpdateThemeButtonContent(false); // false = modo oscuro activo, mostrar opción para claro
        }

        private void ApplyLightTheme()
        {
            // Cambiar colores a modo claro (Celestes con texto negro)
            Application.Current.Resources["PrimaryBrush"] = new SolidColorBrush(Color.FromRgb(0x00, 0x97, 0xA7)); // Celeste
            Application.Current.Resources["PrimaryForegroundBrush"] = new SolidColorBrush(Colors.White);
            Application.Current.Resources["Background"] = new SolidColorBrush(Color.FromRgb(0xF8, 0xFA, 0xFB)); // Gris claro
            Application.Current.Resources["Surface"] = new SolidColorBrush(Colors.White);
            Application.Current.Resources["TextPrimary"] = new SolidColorBrush(Color.FromRgb(0x21, 0x21, 0x21)); // Negro
            Application.Current.Resources["TextSecondary"] = new SolidColorBrush(Color.FromRgb(0x75, 0x75, 0x75));

            // Actualizar el contenido del botón
            UpdateThemeButtonContent(true); // true = modo claro activo, mostrar opción para oscuro
        }

        private void UpdateThemeButtonContent(bool isLightMode)
        {
            var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };

            var icon = new PackIcon
            {
                Width = 22,
                Height = 22,
                Margin = new Thickness(0, 0, 12, 0)
            };

            var textBlock = new TextBlock { FontWeight = FontWeights.Medium };

            if (isLightMode)
            {
                icon.Kind = PackIconKind.WeatherNight;
                textBlock.Text = "Modo Oscuro";
            }
            else
            {
                icon.Kind = PackIconKind.WeatherSunny;
                textBlock.Text = "Modo Claro";
            }

            stackPanel.Children.Add(icon);
            stackPanel.Children.Add(textBlock);
            btnModoOscuro.Content = stackPanel;
        }
    }
}
