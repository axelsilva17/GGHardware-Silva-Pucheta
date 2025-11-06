using System.Windows;

namespace GGHardware.Views
{
    public partial class DetallesSolicitudWindow : Window
    {
        public DetallesSolicitudWindow(object solicitud)
        {
            InitializeComponent();
            DataContext = solicitud; // enlazamos los datos recibidos
        }

        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
