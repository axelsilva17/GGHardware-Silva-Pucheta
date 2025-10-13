using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Microsoft.Data.SqlClient;
using GGHardware.Data;
using Microsoft.EntityFrameworkCore;

namespace GGHardware.Views
{
    public partial class Backup : UserControl
    {
        public Backup()
        {
            InitializeComponent();
            CargarHistorial();
            CargarSolicitudesPendientes();
        }

        // Cargar el historial de backups en el DataGrid
        private void CargarHistorial()
        {
            using (var context = new ApplicationDbContext())
            {
                dgBackups.ItemsSource = context.Backups
                                               .OrderByDescending(b => b.Fecha)
                                               .ToList();
            }
        }

        // Cargar solicitudes pendientes para el gerente
        private void CargarSolicitudesPendientes()
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var solicitudes = context.SolicitudRestauraciones
                        .Include(s => s.Supervisor)
                        .Include(s => s.Backup)
                        .Where(s => s.estado == "Pendiente")
                        .OrderByDescending(s => s.fecha_solicitud)
                        .Select(s => new
                        {
                            s.id_solicitud,
                            nombre_supervisor = s.Supervisor != null ? s.Supervisor.Nombre : "N/A",
                            nombre_archivo_backup = s.Backup != null ? s.Backup.NombreArchivo : "N/A",
                            ruta_archivo_backup = s.Backup != null ? s.Backup.RutaArchivo : "",
                            s.fecha_backup,
                            s.fecha_solicitud,
                            motivo_solicitud = s.motivo_solicitud ?? "",
                            s.estado
                        })
                        .ToList();

                    dgSolicitudesPendientes.ItemsSource = solicitudes;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar solicitudes: {ex.Message}\n\nDetalle: {ex.InnerException?.Message}",
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Seleccionar ruta para guardar backup
        private void btnSeleccionarRuta_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                Title = "Guardar Backup",
                Filter = "Archivo de respaldo (*.bak)|*.bak",
                FileName = $"Backup_{DateTime.Now:yyyyMMdd_HHmm}.bak"
            };

            if (dlg.ShowDialog() == true)
            {
                txtRuta.Text = dlg.FileName;
            }
        }

        // Realizar backup y guardar registro
        private void btnBackup_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtRuta.Text))
            {
                MessageBox.Show("Seleccione una ubicación para guardar el backup.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            lblEstado.Text = "🔄 Realizando backup...";
            lblEstado.Foreground = System.Windows.Media.Brushes.Orange;

            string databaseName = "GGHardware";
            string backupPath = txtRuta.Text;

            try
            {
                string connectionString;
                using (var context = new ApplicationDbContext())
                {
                    connectionString = context.Database.GetDbConnection().ConnectionString;
                }

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string backupQuery = $"BACKUP DATABASE [{databaseName}] TO DISK = '{backupPath}' " +
                                         "WITH NOFORMAT, NOINIT, NAME = '{databaseName}-Full Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10";

                    using (SqlCommand cmd = new SqlCommand(backupQuery, con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                // Guardar registro del backup en la DB
                using (var context = new ApplicationDbContext())
                {
                    context.Backups.Add(new Models.Backup
                    {
                        NombreArchivo = System.IO.Path.GetFileName(backupPath),
                        Fecha = DateTime.Now,
                        RutaArchivo = backupPath
                    });
                    context.SaveChanges();
                }

                lblEstado.Text = "✅ Backup realizado correctamente";
                lblEstado.Foreground = System.Windows.Media.Brushes.Green;

                CargarHistorial();
            }
            catch (Exception ex)
            {
                lblEstado.Text = "❌ Error al realizar el backup";
                lblEstado.Foreground = System.Windows.Media.Brushes.Red;
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Evento de doble click en solicitudes pendientes
        private void dgSolicitudesPendientes_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgSolicitudesPendientes.SelectedItem == null) return;

            dynamic solicitud = dgSolicitudesPendientes.SelectedItem;

            string mensaje = $"Detalles de la Solicitud:\n\n" +
                           $"ID: {solicitud.id_solicitud}\n" +
                           $"Supervisor: {solicitud.nombre_supervisor}\n" +
                           $"Archivo: {solicitud.nombre_archivo_backup}\n" +
                           $"Fecha Backup: {solicitud.fecha_backup:dd/MM/yyyy}\n" +
                           $"Fecha Solicitud: {solicitud.fecha_solicitud:dd/MM/yyyy HH:mm}\n" +
                           $"Motivo: {solicitud.motivo_solicitud}\n" +
                           $"Estado: {solicitud.estado}";

            MessageBox.Show(mensaje, "Detalles de Solicitud",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Aprobar solicitud de restauración
        private void AprobarSolicitud_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext == null) return;

            dynamic solicitudData = button.DataContext;
            int idSolicitud = solicitudData.id_solicitud;
            string rutaBackup = solicitudData.ruta_archivo_backup;
            string nombreArchivo = solicitudData.nombre_archivo_backup;

            if (MessageBox.Show($"¿Desea aprobar la solicitud de restauración del backup '{nombreArchivo}'?\n\n" +
                              "ADVERTENCIA: Esto reemplazará la base de datos actual.",
                              "Confirmar aprobación", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var solicitud = context.SolicitudRestauraciones.Find(idSolicitud);
                    if (solicitud != null)
                    {
                        solicitud.estado = "EnRestauracion";
                        solicitud.fecha_aprobacion = DateTime.Now;
                        // Aquí deberías obtener el ID del gerente actual logueado
                        // solicitud.id_gerente = IdGerenteActual;
                        context.SaveChanges();
                    }
                }

                // Ejecutar la restauración
                EjecutarRestauracion(rutaBackup, idSolicitud);

                MessageBox.Show("Solicitud aprobada y restauración ejecutada correctamente.",
                              "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                CargarSolicitudesPendientes();
                CargarHistorial();
            }
            catch (Exception ex)
            {
                // Marcar como fallida en caso de error
                try
                {
                    using (var context = new ApplicationDbContext())
                    {
                        var solicitud = context.SolicitudRestauraciones.Find(idSolicitud);
                        if (solicitud != null)
                        {
                            solicitud.estado = "Pendiente";
                            solicitud.observaciones_gerente = $"Error: {ex.Message}";
                            context.SaveChanges();
                        }
                    }
                }
                catch { }

                MessageBox.Show($"Error al aprobar y restaurar: {ex.Message}",
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Rechazar solicitud de restauración
        private void RechazarSolicitud_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext == null) return;

            dynamic solicitudData = button.DataContext;
            int idSolicitud = solicitudData.id_solicitud;

            // Solicitar observaciones del rechazo
            var observaciones = Microsoft.VisualBasic.Interaction.InputBox(
                "Ingrese el motivo del rechazo:",
                "Rechazar Solicitud",
                "", -1, -1);

            if (string.IsNullOrWhiteSpace(observaciones))
            {
                MessageBox.Show("Debe ingresar un motivo para rechazar la solicitud.",
                              "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"¿Confirma que desea rechazar esta solicitud?",
                              "Confirmar rechazo", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var solicitud = context.SolicitudRestauraciones.Find(idSolicitud);
                    if (solicitud != null)
                    {
                        solicitud.estado = "Rechazada";
                        solicitud.fecha_aprobacion = DateTime.Now;
                        solicitud.observaciones_gerente = observaciones;
                        // Aquí deberías obtener el ID del gerente actual logueado
                        // solicitud.id_gerente = IdGerenteActual;
                        context.SaveChanges();
                    }
                }

                MessageBox.Show("Solicitud rechazada correctamente.", "Información",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                CargarSolicitudesPendientes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al rechazar solicitud: {ex.Message}",
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Método auxiliar para ejecutar la restauración
        private void EjecutarRestauracion(string rutaBackup, int idSolicitud)
        {
            string databaseName = "GGHardware";

            string connectionString;
            using (var context = new ApplicationDbContext())
            {
                connectionString = context.Database.GetDbConnection().ConnectionString;
            }

            var builder = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = "master"
            };

            using (SqlConnection con = new SqlConnection(builder.ConnectionString))
            {
                con.Open();
                string sql = $@"
                    ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    RESTORE DATABASE [{databaseName}] FROM DISK = N'{rutaBackup}' WITH REPLACE;
                    ALTER DATABASE [{databaseName}] SET MULTI_USER;
                ";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            // Actualizar estado a Completada
            using (var context = new ApplicationDbContext())
            {
                var solicitud = context.SolicitudRestauraciones.Find(idSolicitud);
                if (solicitud != null)
                {
                    solicitud.estado = "Completada";
                    solicitud.fecha_restauracion = DateTime.Now;
                    context.SaveChanges();
                }
            }
        }
    }
}