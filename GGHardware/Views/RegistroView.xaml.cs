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

            if (MainWindow.UsuarioActual != null)
            {
                if (MainWindow.UsuarioActual.RolId == 2) // Usuario normal
                {
                    // Ocultar botón "Registrar Usuario"
                    btnRegistrarUsuario.Visibility = Visibility.Collapsed;

                    // No es necesario mostrar cmbRol, porque siempre será cliente
                }
                else if (MainWindow.UsuarioActual.RolId == 1) // Supervisor
                {
                    // Mostrar botón "Registrar Usuario"
                    btnRegistrarUsuario.Visibility = Visibility.Visible;
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