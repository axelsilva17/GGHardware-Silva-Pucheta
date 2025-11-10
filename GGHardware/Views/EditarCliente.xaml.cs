using GGHardware.Data;
using GGHardware.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GGHardware.Views
{
    public partial class EditarCliente : UserControl
    {
        private int clienteId;
        private readonly ApplicationDbContext _context;

        public EditarCliente(int idCliente)
        {
            InitializeComponent();
            _context = new ApplicationDbContext();
            clienteId = idCliente;
            CargarCliente();
        }

        private void CargarCliente()
        {
            var cliente = _context.Clientes.FirstOrDefault(c => c.id_cliente == clienteId);
            if (cliente != null)
            {
                txtCuit.Text = cliente.cuit.ToString();
                txtNombre.Text = cliente.nombre;
                txtApellido.Text = cliente.apellido;
                txtEmail.Text = cliente.email;
                txtTelefono.Text = cliente.telefono.ToString();
                txtDireccion.Text = cliente.direccion;
                txtProvincia.Text = cliente.provincia;
                txtLocalidad.Text = cliente.localidad;
                txtCondicionFiscal.Text = cliente.condicion_fiscal;
            }
        }

        private void btnActualizar_Click(object sender, RoutedEventArgs e)
        {
            var cliente = _context.Clientes.FirstOrDefault(c => c.id_cliente == clienteId);
            if (cliente != null)
            {
                cliente.cuit = txtCuit.Text;
                cliente.nombre = txtNombre.Text;
                cliente.apellido = txtApellido.Text;
                cliente.email = txtEmail.Text;
                cliente.telefono = txtTelefono.Text;
                cliente.direccion = txtDireccion.Text;
                cliente.provincia = txtProvincia.Text;
                cliente.localidad = txtLocalidad.Text;
                cliente.condicion_fiscal = txtCondicionFiscal.Text;

                _context.SaveChanges();

                MessageBox.Show("✅ Cliente actualizado correctamente.");
            }
        }

        private void btnVolver_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainContentBorder.Child = new ClientesView(); // la vista de clientes
                
            }
        }
    }
}
