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
using GGHardware.Services;
using GGHardware.ViewModels;
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
            if (UsuarioActual == null) return;

            // Ocultar todos por defecto
            btnProductos.Visibility = Visibility.Collapsed;
            btnVentas.Visibility = Visibility.Collapsed;
            btnRegistro.Visibility = Visibility.Collapsed;
            btnReportes.Visibility = Visibility.Collapsed;
            btnBackup.Visibility = Visibility.Collapsed;
            btnProveedores.Visibility = Visibility.Collapsed;
            btnRestauracion.Visibility = Visibility.Collapsed;


            // Mostrar botones según rol
            switch (UsuarioActual.RolId)
            {
                case 1: // Supervisor
                  
                    btnRegistro.Visibility = Visibility.Visible;
                    btnReportes.Visibility = Visibility.Visible;
                    btnProductos.Visibility = Visibility.Visible;
                    btnRestauracion.Visibility = Visibility.Visible;

                    break;

                case 2: // Usuario/Vendedor
                    btnProductos.Visibility = Visibility.Visible;
                    btnVentas.Visibility = Visibility.Visible;
                    btnRegistro.Visibility = Visibility.Visible;
                    break;

                case 3: // Cliente
                        // Sin acceso al menú
                    break;

                case 4: // Gerente
                    btnInicio.Visibility = Visibility.Visible;
                    btnBackup.Visibility = Visibility.Visible;
                    btnProveedores.Visibility = Visibility.Visible;
                   
                    break;
            }

            // Habilitar y restaurar opacidad del menú
            mainMenu.IsEnabled = true;
            mainMenu.Opacity = 1.0;
        }


        public MainWindow()
        {
            InitializeComponent();
            ApplyLightTheme();

            // Mostrar la vista de inicio por defecto
            MainContentBorder.Child = new InicioView();

            // Ocultar todos por defecto
            btnProductos.Visibility = Visibility.Collapsed;
            btnVentas.Visibility = Visibility.Collapsed;
            btnRegistro.Visibility = Visibility.Collapsed;
            btnReportes.Visibility = Visibility.Collapsed;
            btnBackup.Visibility = Visibility.Collapsed;
            btnProveedores.Visibility = Visibility.Collapsed;
            btnInicio.Visibility = Visibility.Collapsed;
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
            VentasView ventasView = new VentasView(MainWindow.UsuarioActual.id_usuario);
            MainContentBorder.Child = ventasView;
        }

        private void MostrarInicio()
        {
            if (UsuarioActual != null)
            {
                switch (UsuarioActual.RolId)
                {
                    case 4: // Gerente
                        MainContentBorder.Child = new Gerente();
                        break;

                    default:
                        MainContentBorder.Child = new DashboardView();
                        break;
                }
            }
            else
            {
                // No hay sesión, mostrar login
                MainContentBorder.Child = new InicioView();
            }
        }

        private void btnInicio_Click(object sender, RoutedEventArgs e)
        {
            MostrarInicio();
        }

        public void ActualizarSesion()
        {
            if (UsuarioActual != null)
            {
                btnCerrarSesion.Visibility = Visibility.Visible;
                
            }
            else
            {
                btnCerrarSesion.Visibility = Visibility.Collapsed;
            }
        }

        public void IniciarSesion(Usuario usuarioLogueado)
        {
            UsuarioActual = usuarioLogueado;   // Asignar usuario
            ActualizarSesion();                // Mostrar botón de cerrar sesión
            HabilitarMenu();                   // Habilitar menú según rol

            // Mostrar la vista según rol
            switch (usuarioLogueado.RolId)
            {
                case 4: // gerente
                    MainContentBorder.Child = new Gerente(); // tu UserControl Gerente
                    break;
                default:
                    MainContentBorder.Child = new DashboardView(); // otras vistas
                    break;
            }
        }


        private void btnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            // Ocultar todos los botones
            btnProductos.Visibility = Visibility.Collapsed;
            btnVentas.Visibility = Visibility.Collapsed;
            btnRegistro.Visibility = Visibility.Collapsed;
            btnReportes.Visibility = Visibility.Collapsed;
            btnBackup.Visibility = Visibility.Collapsed;
            btnProveedores.Visibility = Visibility.Collapsed;

            UsuarioActual = null; // Limpiar sesión
            ActualizarSesion();
            mainMenu.IsEnabled = false;
            mainMenu.Opacity = 0.5;
            MainContentBorder.Child = new InicioView();

        }

        private void btnReportes_Click(object sender, RoutedEventArgs e)
        {
            MainContentBorder.Child = new Reportes();
        }

        private void btnBackup_Click(object sender, RoutedEventArgs e)
        {
            MainContentBorder.Child = new GGHardware.Views.Backup();
        }

        private void btnProveedores_Click(object sender, RoutedEventArgs e)
        {
            MainContentBorder.Child = new ProveedoresView();
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


        private void btnRestauracion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (UsuarioActual == null)
                {
                    MessageBox.Show("Debe iniciar sesión primero", "Advertencia",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var context = new GGHardware.Data.ApplicationDbContext();
                var service = new GGHardware.Services.SolicitudRestauracionService(context);
                var solicitudView = new GGHardware.Views.SolicitudRestauracionView();

                // Usa UsuarioActual.id_usuario directamente
                var viewModel = new GGHardware.ViewModels.SolicitudRestauracionViewModel(
                    service,
                    UsuarioActual.id_usuario  // ← Aquí usas directamente la propiedad
                );

                solicitudView.DataContext = viewModel;
                MainContentBorder.Child = solicitudView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
