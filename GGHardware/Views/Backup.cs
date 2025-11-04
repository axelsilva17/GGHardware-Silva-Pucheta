using System;
using System.Collections.Generic;
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
            CargarInstancias();
            CargarBasesDeDatos();
            CargarHistorial();
            CargarSolicitudesPendientes();
        }

        // Cargar lista de instancias y bases de datos disponibles
        private void CargarInstancias()
        {
            try
            {
                string connectionString;
                using (var context = new ApplicationDbContext())
                {
                    connectionString = context.Database.GetDbConnection().ConnectionString;
                }

                var builder = new SqlConnectionStringBuilder(connectionString);
                string servidorActual = builder.DataSource;

                // Lista de instancias disponibles
                var instancias = new List<string>
        {
            servidorActual,              // Instancia actual (para backups)
            "localhost\\SQLEXPRESS"      // Instancia de respaldo (para restauraciones)
        };

                // Eliminar duplicados
                instancias = instancias.Distinct().ToList();

                cmbInstancia.ItemsSource = instancias;
                cmbInstancia.SelectedItem = servidorActual;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar instancias: {ex.Message}",
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Cargar bases de datos de la instancia seleccionada
        private void CargarBasesDeDatos()
        {
            try
            {
                if (cmbInstancia.SelectedItem == null)
                {
                    return;
                }

                string instanciaSeleccionada = cmbInstancia.SelectedItem.ToString();
                string connectionString;

                using (var context = new ApplicationDbContext())
                {
                    connectionString = context.Database.GetDbConnection().ConnectionString;
                }

                // Cambiar a la instancia seleccionada
                var builder = new SqlConnectionStringBuilder(connectionString)
                {
                    DataSource = instanciaSeleccionada,
                    InitialCatalog = "master"
                };

                using (SqlConnection con = new SqlConnection(builder.ConnectionString))
                {
                    con.Open();
                    string query = @"SELECT name FROM sys.databases 
                                   WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb')
                                   AND state_desc = 'ONLINE'
                                   ORDER BY name";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        var databases = new List<string>();
                        while (reader.Read())
                        {
                            databases.Add(reader["name"].ToString());
                        }

                        cmbBaseDatos.ItemsSource = databases;

                        if (databases.Count > 0)
                        {
                            cmbBaseDatos.SelectedIndex = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar bases de datos: {ex.Message}",
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Evento cuando cambia la instancia seleccionada
        private void cmbInstancia_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CargarBasesDeDatos();
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
                        .AsEnumerable() // Traer a memoria primero
                        .Select(s => new TuProyecto.ViewModels.SolicitudRestauracion
                        {
                            id_solicitud = s.id_solicitud,
                            id_supervisor = s.id_supervisor,
                            nombre_supervisor = s.Supervisor?.Nombre ?? "N/A",
                            id_gerente = s.id_gerente,
                            nombre_gerente = s.Gerente?.Nombre ?? "",
                            id_backup = s.id_backup,
                            nombre_archivo_backup = s.Backup?.NombreArchivo ?? "N/A",
                            fecha_backup = s.fecha_backup ?? DateTime.MinValue,
                            ruta_archivo = s.Backup?.RutaArchivo ?? "",
                            fecha_solicitud = s.fecha_solicitud ?? DateTime.Now,
                            estado = s.estado ?? "Pendiente",
                            motivo_solicitud = s.motivo_solicitud ?? "",
                            observaciones_gerente = s.observaciones_gerente ?? "",
                            fecha_aprobacion = s.fecha_aprobacion,
                            fecha_restauracion = s.fecha_restauracion
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
            string instancia = cmbInstancia.SelectedItem?.ToString()?.Replace("\\", "_") ?? "Instance";
            string databaseName = cmbBaseDatos.SelectedItem?.ToString() ?? "Database";

            var dlg = new SaveFileDialog
            {
                Title = "Guardar Backup",
                Filter = "Archivo de respaldo (*.bak)|*.bak",
                FileName = $"Backup_{instancia}_{databaseName}_{DateTime.Now:yyyyMMdd_HHmm}.bak"
            };

            if (dlg.ShowDialog() == true)
            {
                txtRuta.Text = dlg.FileName;
            }
        }

        // Realizar backup y guardar registro
        private void btnBackup_Click(object sender, RoutedEventArgs e)
        {
            if (cmbInstancia.SelectedItem == null)
            {
                MessageBox.Show("Seleccione una instancia de SQL Server.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (cmbBaseDatos.SelectedItem == null)
            {
                MessageBox.Show("Seleccione una base de datos para hacer backup.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(txtRuta.Text))
            {
                MessageBox.Show("Seleccione una ubicación para guardar el backup.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            lblEstado.Text = "🔄 Realizando backup...";
            lblEstado.Foreground = System.Windows.Media.Brushes.Orange;

            string instanciaSeleccionada = cmbInstancia.SelectedItem.ToString();
            string databaseName = cmbBaseDatos.SelectedItem.ToString();
            string backupPath = txtRuta.Text;

            try
            {
                string connectionString;
                using (var context = new ApplicationDbContext())
                {
                    connectionString = context.Database.GetDbConnection().ConnectionString;
                }

                // Cambiar a la instancia y BD seleccionadas
                var builder = new SqlConnectionStringBuilder(connectionString)
                {
                    DataSource = instanciaSeleccionada,
                    InitialCatalog = databaseName
                };

                using (SqlConnection con = new SqlConnection(builder.ConnectionString))
                {
                    con.Open();
                    string backupQuery = $"BACKUP DATABASE [{databaseName}] TO DISK = '{backupPath}' " +
                                         $"WITH NOFORMAT, NOINIT, NAME = '{databaseName}-Full Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10";

                    using (SqlCommand cmd = new SqlCommand(backupQuery, con))
                    {
                        cmd.CommandTimeout = 300; // 5 minutos timeout
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

                lblEstado.Text = $"✅ Backup de '{instanciaSeleccionada}\\{databaseName}' realizado correctamente";
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

            var solicitud = dgSolicitudesPendientes.SelectedItem as TuProyecto.ViewModels.SolicitudRestauracion;
            if (solicitud == null) return;

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

            var solicitudData = button.DataContext as TuProyecto.ViewModels.SolicitudRestauracion;
            if (solicitudData == null) return;

            int idSolicitud = solicitudData.id_solicitud;
            string rutaBackup = solicitudData.ruta_archivo;
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
                        context.SaveChanges();
                    }
                }

                EjecutarRestauracion(rutaBackup, idSolicitud);

                MessageBox.Show("Solicitud aprobada y restauración ejecutada correctamente.",
                              "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                CargarSolicitudesPendientes();
                CargarHistorial();
            }
            catch (Exception ex)
            {
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

            var solicitudData = button.DataContext as TuProyecto.ViewModels.SolicitudRestauracion;
            if (solicitudData == null) return;

            int idSolicitud = solicitudData.id_solicitud;

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
        // Método auxiliar para ejecutar la restauración EN LA INSTANCIA SQLEXPRESS
        private void EjecutarRestauracion(string rutaBackup, int idSolicitud)
        {
            string databaseName = "GGHardware_Restored";
            string instanciaRestauracion = "localhost\\SQLEXPRESS";

            try
            {
                string connectionString;
                using (var context = new ApplicationDbContext())
                {
                    connectionString = context.Database.GetDbConnection().ConnectionString;
                }

                var builder = new SqlConnectionStringBuilder(connectionString)
                {
                    DataSource = instanciaRestauracion,
                    InitialCatalog = "master"
                };

                using (SqlConnection con = new SqlConnection(builder.ConnectionString))
                {
                    con.Open();

                    // 1. Verificar si la BD ya existe y eliminarla
                    string checkDbQuery = $@"
                IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{databaseName}')
                BEGIN
                    ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    DROP DATABASE [{databaseName}];
                END
            ";

                    using (SqlCommand cmd = new SqlCommand(checkDbQuery, con))
                    {
                        cmd.CommandTimeout = 300;
                        cmd.ExecuteNonQuery();
                    }

                    // 2. Restaurar con los nombres lógicos correctos
                    string restoreQuery = $@"
                RESTORE DATABASE [{databaseName}] 
                FROM DISK = N'{rutaBackup}' 
                WITH MOVE 'GGHardware' TO 'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\{databaseName}.mdf',
                     MOVE 'GGHardware_log' TO 'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\{databaseName}_log.ldf',
                     REPLACE;
            ";

                    using (SqlCommand cmd = new SqlCommand(restoreQuery, con))
                    {
                        cmd.CommandTimeout = 300;
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
                        solicitud.observaciones_gerente = $"✅ Restaurado en {instanciaRestauracion} como '{databaseName}'";
                        context.SaveChanges();
                    }
                }

                MessageBox.Show(
                    $"✅ BACKUP RESTAURADO EXITOSAMENTE\n\n" +
                    $"📍 Instancia: {instanciaRestauracion}\n" +
                    $"💾 Base de datos: {databaseName}\n\n" +
                    $"ℹ️ La base de datos restaurada está disponible en la instancia SQLEXPRESS.\n" +
                    $"La base de datos en producción NO fue afectada.",
                    "Restauración Exitosa",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // Actualizar estado a Fallida
                using (var context = new ApplicationDbContext())
                {
                    var solicitud = context.SolicitudRestauraciones.Find(idSolicitud);
                    if (solicitud != null)
                    {
                        solicitud.estado = "Fallida";
                        solicitud.observaciones_gerente = $"❌ Error: {ex.Message}";
                        context.SaveChanges();
                    }
                }

                throw;
            }
        }
    }
}