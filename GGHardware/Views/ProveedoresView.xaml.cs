using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GGHardware.Data;
using GGHardware.Models;

namespace GGHardware.Views
{
    public partial class ProveedoresView : UserControl
    {
        private int? _proveedorEditandoId = null;

        public ProveedoresView()
        {
            InitializeComponent();
            CargarProveedores();
        }

        private void CargarProveedores()
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var proveedores = context.Proveedores
                        .OrderByDescending(p => p.activo)
                        .ThenBy(p => p.razon_social)
                        .ToList();

                    dgProveedores.ItemsSource = proveedores;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar proveedores: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRazonSocial.Text))
            {
                MessageBox.Show("La razón social es obligatoria",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtRazonSocial.Focus();
                return;
            }

            if (!string.IsNullOrWhiteSpace(txtCuit.Text) && txtCuit.Text.Length != 11)
            {
                MessageBox.Show("El CUIT debe tener 11 dígitos (sin guiones)",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCuit.Focus();
                return;
            }

            if (!string.IsNullOrWhiteSpace(txtEmail.Text) && !EsEmailValido(txtEmail.Text))
            {
                MessageBox.Show("El email no tiene un formato válido",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEmail.Focus();
                return;
            }

            try
            {
                using (var context = new ApplicationDbContext())
                {
                    // Verificar CUIT duplicado
                    if (!string.IsNullOrWhiteSpace(txtCuit.Text))
                    {
                        var cuitExiste = context.Proveedores
                            .Any(p => p.cuit == txtCuit.Text && p.id_proveedor != _proveedorEditandoId);

                        if (cuitExiste)
                        {
                            MessageBox.Show("Ya existe un proveedor con ese CUIT",
                                "CUIT duplicado", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    if (_proveedorEditandoId.HasValue)
                    {
                        // Editar
                        var proveedor = context.Proveedores.Find(_proveedorEditandoId.Value);
                        if (proveedor != null)
                        {
                            proveedor.razon_social = txtRazonSocial.Text.Trim();
                            proveedor.nombre_contacto = string.IsNullOrWhiteSpace(txtNombreContacto.Text)
                                ? null : txtNombreContacto.Text.Trim();
                            proveedor.cuit = string.IsNullOrWhiteSpace(txtCuit.Text)
                                ? null : txtCuit.Text.Trim();
                            proveedor.telefono = string.IsNullOrWhiteSpace(txtTelefono.Text)
                                ? null : txtTelefono.Text.Trim();
                            proveedor.email = string.IsNullOrWhiteSpace(txtEmail.Text)
                                ? null : txtEmail.Text.Trim();
                            proveedor.direccion = string.IsNullOrWhiteSpace(txtDireccion.Text)
                                ? null : txtDireccion.Text.Trim();
                            proveedor.codigo_postal = string.IsNullOrWhiteSpace(txtCodigoPostal.Text)
                                ? null : txtCodigoPostal.Text.Trim();

                            context.SaveChanges();
                            MessageBox.Show("Proveedor actualizado correctamente",
                                "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        // Crear nuevo
                        var proveedor = new Proveedor
                        {
                            razon_social = txtRazonSocial.Text.Trim(),
                            nombre_contacto = string.IsNullOrWhiteSpace(txtNombreContacto.Text)
                                ? null : txtNombreContacto.Text.Trim(),
                            cuit = string.IsNullOrWhiteSpace(txtCuit.Text)
                                ? null : txtCuit.Text.Trim(),
                            telefono = string.IsNullOrWhiteSpace(txtTelefono.Text)
                                ? null : txtTelefono.Text.Trim(),
                            email = string.IsNullOrWhiteSpace(txtEmail.Text)
                                ? null : txtEmail.Text.Trim(),
                            direccion = string.IsNullOrWhiteSpace(txtDireccion.Text)
                                ? null : txtDireccion.Text.Trim(),
                            codigo_postal = string.IsNullOrWhiteSpace(txtCodigoPostal.Text)
                                ? null : txtCodigoPostal.Text.Trim(),
                            activo = true,
                            fecha_alta = DateTime.Now
                        };

                        context.Proveedores.Add(proveedor);
                        context.SaveChanges();

                        MessageBox.Show("Proveedor registrado correctamente",
                            "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    LimpiarCampos();
                    CargarProveedores();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el proveedor:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is Proveedor proveedor)
            {
                _proveedorEditandoId = proveedor.id_proveedor;

                txtRazonSocial.Text = proveedor.razon_social;
                txtNombreContacto.Text = proveedor.nombre_contacto ?? "";
                txtCuit.Text = proveedor.cuit ?? "";
                txtTelefono.Text = proveedor.telefono ?? "";
                txtEmail.Text = proveedor.email ?? "";
                txtDireccion.Text = proveedor.direccion ?? "";
                txtCodigoPostal.Text = proveedor.codigo_postal ?? "";

                btnGuardar.Content = "Actualizar";
                txtRazonSocial.Focus();
            }
        }

        private void btnActivarDesactivar_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is Proveedor proveedor)
            {
                try
                {
                    using (var context = new ApplicationDbContext())
                    {
                        var prov = context.Proveedores.Find(proveedor.id_proveedor);
                        if (prov != null)
                        {
                            prov.activo = !prov.activo;
                            context.SaveChanges();
                            CargarProveedores();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarCampos();
        }

        private void LimpiarCampos()
        {
            txtRazonSocial.Clear();
            txtNombreContacto.Clear();
            txtCuit.Clear();
            txtTelefono.Clear();
            txtEmail.Clear();
            txtDireccion.Clear();
            txtCodigoPostal.Clear();

            _proveedorEditandoId = null;
            btnGuardar.Content = "Guardar";
            txtRazonSocial.Focus();
        }

        private void txtNumeros_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        private bool EsEmailValido(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}