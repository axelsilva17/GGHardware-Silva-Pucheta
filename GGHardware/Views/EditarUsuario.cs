using GGHardware.Data;
using GGHardware.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GGHardware.Views
{
    public partial class EditarUsuario : UserControl
    {
        private int usuarioId;
        private readonly ApplicationDbContext _context;

        public EditarUsuario(int idUsuario)
        {
            InitializeComponent();
            _context = new ApplicationDbContext();
            usuarioId = idUsuario;
            CargarUsuario();
        }

        private void CargarUsuario()
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.id_usuario == usuarioId);
            if (usuario != null)
            {
                txtDNI.Text = usuario.dni.ToString();
                txtNombre.Text = usuario.Nombre;
                txtApellido.Text = usuario.apellido;
                txtCorreo.Text = usuario.correo;
                pbContrasena.Password = usuario.contraseña;
                dpFechaNacimiento.SelectedDate = usuario.fecha_Nacimiento;

                foreach (ComboBoxItem item in cmbRol.Items)
                {
                    if (item.Content.ToString() == usuario.rol)
                    {
                        cmbRol.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void btnActualizar_Click(object sender, RoutedEventArgs e)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.id_usuario == usuarioId);
            if (usuario != null)
            {
                usuario.dni = int.Parse(txtDNI.Text);
                usuario.Nombre = txtNombre.Text;
                usuario.apellido = txtApellido.Text;
                usuario.correo = txtCorreo.Text;
                usuario.contraseña = pbContrasena.Password;
                usuario.fecha_Nacimiento = dpFechaNacimiento.SelectedDate;
                usuario.rol = (cmbRol.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "SinRol";

                _context.SaveChanges();
                MessageBox.Show("✅ Usuario actualizado correctamente.");
            }
        }

        private void btnVolver_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainContentBorder.Child = new Usuarios(); // vuelve a la vista de la grilla
            }
        }
    }
}
