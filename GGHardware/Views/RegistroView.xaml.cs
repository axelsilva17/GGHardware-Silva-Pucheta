using System.Windows;
using System.Windows.Controls;
namespace GGHardware.Views
{
    public partial class RegistroView : UserControl
    {
        private MainWindow _mainWindow;
        public RegistroView(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;

           
            // Buscar el botón por nombre
            Button btnRegistrarCliente = this.FindName("btnRegistrarCliente") as Button;

            if (MainWindow.UsuarioActual != null)
            {
                if (MainWindow.UsuarioActual.RolId == 2) // Usuario normal
                {
                    // Ocultar botón "Registrar Usuario"
                    btnRegistrarUsuario.Visibility = Visibility.Collapsed;
                    // Mostrar botón "Registrar Cliente" si existe
                    if (btnRegistrarCliente != null)
                        btnRegistrarCliente.Visibility = Visibility.Visible;
                }
                else if (MainWindow.UsuarioActual.RolId == 1) // Supervisor
                {
                    // Mostrar botón "Registrar Usuario"
                    btnRegistrarUsuario.Visibility = Visibility.Visible;
                    // Ocultar botón "Registrar Cliente" si existe
                    if (btnRegistrarCliente != null)
                        btnRegistrarCliente.Visibility = Visibility.Collapsed;
                }
            }
        }
        // Método para el botón "Registrar Cliente"
        private void btnRegistrarCliente_Click(object sender, RoutedEventArgs e)
        {
            // Cambia el contenido del MainContentBorder a la vista de Clientes
            _mainWindow.MainContentBorder.Child = new ClientesView();
        }
        // Método para el botón "Registrar Usuario"
        private void btnRegistrarUsuario_Click(object sender, RoutedEventArgs e)
        {
            // Cambia el contenido del MainContentBorder a la vista de Usuarios
            _mainWindow.MainContentBorder.Child = new Usuarios();
        }
    }
}