using GGHardware.Data;
using GGHardware.Models;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GGHardware.Views
{
    public partial class Usuarios : UserControl
    {
        public Usuarios()
        {
            InitializeComponent();
            CargarUsuarios();
            this.PreviewKeyDown += Usuarios_PreviewKeyDown;

        }
        private void Usuarios_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Detectar si el foco está en el botón Limpiar
                if (btnLimpiar.IsKeyboardFocusWithin)
                {
                    btnLimpiar.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    e.Handled = true;
                }
                else
                {
                    // Ejecuta Guardar por defecto si no está en Limpiar
                    btnGuardar.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    e.Handled = true;
                }
            }
        }
        private void btnVolver_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainContentBorder.Child = new RegistroView(mainWindow);
            }
        }

        private void txtDNI_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void txtLetras_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^a-zA-ZáéíóúÁÉÍÓÚñÑ\\s]+");
            e.Handled = regex.IsMatch(e.Text);
        }

       
        private void CargarUsuarios()
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var roles = new Dictionary<int, string>
                    {
                        {1, "Supervisor"},
                        {2, "Usuario"},
                        {3, "Cliente"}
                    };

                    var usuarios = context.Usuarios.ToList();

                    foreach (var u in usuarios)
                    {
                        u.NombreRol = roles.ContainsKey(u.RolId) ? roles[u.RolId] : "Desconocido";
                    }

                    dgUsuarios.ItemsSource = usuarios;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Error al cargar usuarios: " + ex.Message);
            }
        }


        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (!Regex.IsMatch(txtNombre.Text, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                MessageBox.Show("El nombre solo debe contener letras.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Regex.IsMatch(txtApellido.Text, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                MessageBox.Show("El apellido solo debe contener letras.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!txtCorreo.Text.Contains("@"))
            {
                MessageBox.Show("El correo electrónico no es válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (pbContrasena.Password.Length < 6)
            {
                MessageBox.Show("La contraseña debe tener un mínimo de 6 caracteres.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (cmbRol.SelectedIndex == -1)
            {
                MessageBox.Show("Por favor, selecciona un rol.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (dpFechaNacimiento.SelectedDate == null)
            {
                MessageBox.Show("Por favor, selecciona una fecha de nacimiento.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var usuario = new Usuario
                    {
                        dni = int.Parse(txtDNI.Text),
                        Nombre = txtNombre.Text,
                        apellido = txtApellido.Text,
                        correo = txtCorreo.Text,
                        contraseña = pbContrasena.Password,
                        fecha_Nacimiento = dpFechaNacimiento.SelectedDate,
                        RolId = (int)cmbRol.SelectedValue,
                        Activo = true // 👈 nuevo: los usuarios se crean activos por defecto
                    };

                    context.Usuarios.Add(usuario);
                    context.SaveChanges();
                }

                MessageBox.Show("✅ Usuario guardado con éxito.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                LimpiarCampos();
                CargarUsuarios();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error al guardar. Detalles: {ex.InnerException?.Message ?? ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarCampos();
        }

        private void LimpiarCampos()
        {
            txtDNI.Text = string.Empty;
            txtNombre.Text = string.Empty;
            txtApellido.Text = string.Empty;
            txtCorreo.Text = string.Empty;
            pbContrasena.Password = string.Empty;
            cmbRol.SelectedIndex = -1;
            dpFechaNacimiento.SelectedDate = null;
            txtDNI.Focus();
        }

        
        private void btnActivarDesactivar_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is Usuario usuario)
            {
                try
                {
                    using (var context = new ApplicationDbContext())
                    {
                        var userDb = context.Usuarios.FirstOrDefault(u => u.id_usuario == usuario.id_usuario);
                        if (userDb != null)
                        {
                            // Cambiar estado
                            userDb.Activo = !userDb.Activo;
                            context.SaveChanges();

                            MessageBox.Show(
                                userDb.Activo ? "✅ Usuario activado." : "⚠️ Usuario desactivado.",
                                "Información",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information
                            );
                        }
                    }

                    CargarUsuarios(); // refrescar grilla
                }
                catch (Exception ex)
                {
                    MessageBox.Show("❌ Error al cambiar estado: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is GGHardware.Models.Usuario usuario)
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.MainContentBorder.Child = new EditarUsuario(usuario.id_usuario);
                }
            }
        }


    }
}
