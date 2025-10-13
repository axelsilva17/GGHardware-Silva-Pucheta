using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GGHardware.Data;
using GGHardware.Models;
using Microsoft.EntityFrameworkCore;

namespace GGHardware.Services
{
    public class SolicitudRestauracionService
    {
        private readonly ApplicationDbContext _context;

        public SolicitudRestauracionService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Supervisor crea solicitud de restauración
        public async Task<int> CrearSolicitud(int id_supervisor, int id_backup, string motivo)
        {
            var backup = await _context.Backups
                .FirstOrDefaultAsync(b => b.Id == id_backup);

            if (backup == null)
                throw new Exception("El backup no existe");

            var solicitud = new SolicitudRestauracion
            {
                id_supervisor = id_supervisor,
                id_backup = id_backup,
                fecha_solicitud = DateTime.Now,
                fecha_backup = backup.Fecha,
                estado = "Pendiente",
                motivo_solicitud = motivo
            };

            _context.SolicitudRestauraciones.Add(solicitud);
            await _context.SaveChangesAsync();

            return solicitud.id_solicitud;
        }

        // Obtener solicitudes pendientes para gerentes (devuelve ViewModel)
        public async Task<List<SolicitudRestauracionVM>> ObtenerSolicitudesPendientes()
        {
            return await _context.SolicitudRestauraciones
                .Where(s => s.estado == "Pendiente")
                .Include(s => s.Supervisor)
                .Include(s => s.Backup)
                .Select(s => new SolicitudRestauracionVM
                {
                    id_solicitud = s.id_solicitud,
                    id_supervisor = s.id_supervisor,
                    nombre_supervisor = s.Supervisor != null ? (s.Supervisor.Nombre + " " + s.Supervisor.apellido) : "N/A",
                    nombre_archivo_backup = s.Backup != null ? s.Backup.NombreArchivo : "N/A",
                    fecha_backup = s.fecha_backup,
                    ruta_archivo = s.Backup != null ? s.Backup.RutaArchivo : "",
                    fecha_solicitud = s.fecha_solicitud,
                    estado = s.estado,
                    motivo_solicitud = s.motivo_solicitud ?? ""
                })
                .ToListAsync();
        }

        // Gerente aprueba o rechaza solicitud
        public async Task<bool> ProcesarSolicitud(int id_solicitud, int id_gerente, bool aprobar, string observaciones)
        {
            var solicitud = await _context.SolicitudRestauraciones
                .FirstOrDefaultAsync(s => s.id_solicitud == id_solicitud);

            if (solicitud == null)
                throw new Exception("La solicitud no existe");

            solicitud.id_gerente = id_gerente;
            solicitud.observaciones_gerente = observaciones;
            solicitud.fecha_aprobacion = DateTime.Now;

            if (aprobar)
            {
                solicitud.estado = "Aprobada";
            }
            else
            {
                solicitud.estado = "Rechazada";
            }

            _context.SolicitudRestauraciones.Update(solicitud);
            await _context.SaveChangesAsync();

            return aprobar;
        }

        // Restaurar base de datos
        public async Task<bool> RestaurarBaseDatos(int id_solicitud, string connectionString)
        {
            var solicitud = await _context.SolicitudRestauraciones
                .Include(s => s.Backup)
                .FirstOrDefaultAsync(s => s.id_solicitud == id_solicitud);

            if (solicitud == null)
                throw new Exception("La solicitud no existe");

            if (solicitud.estado != "Aprobada")
                throw new Exception("La solicitud no ha sido aprobada");

            try
            {
                solicitud.estado = "EnRestauracion";
                _context.SolicitudRestauraciones.Update(solicitud);
                await _context.SaveChangesAsync();

                // Aquí va la lógica de restauración de la BD
                await EjecutarRestauracion(solicitud.Backup.RutaArchivo, connectionString);

                solicitud.estado = "Completada";
                solicitud.fecha_restauracion = DateTime.Now;
                _context.SolicitudRestauraciones.Update(solicitud);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                solicitud.estado = "Aprobada";
                solicitud.observaciones_gerente += $" | Error: {ex.Message}";
                _context.SolicitudRestauraciones.Update(solicitud);
                await _context.SaveChangesAsync();
                throw;
            }
        }

        // Obtener backups disponibles para selector
        public async Task<List<BackupDTO>> ObtenerBackupsDisponibles()
        {
            return await _context.Backups
                .Select(b => new BackupDTO
                {
                    id = b.Id,
                    nombre_archivo = b.NombreArchivo,
                    fecha = b.Fecha,
                    ruta_archivo = b.RutaArchivo
                })
                .OrderByDescending(b => b.fecha)
                .ToListAsync();
        }

        // Obtener solicitudes por usuario (supervisor) - devuelve ViewModel
        public async Task<List<SolicitudRestauracionVM>> ObtenerSolicitudesPorSupervisor(int id_supervisor)
        {
            return await _context.SolicitudRestauraciones
                .Where(s => s.id_supervisor == id_supervisor)
                .Include(s => s.Gerente)
                .Include(s => s.Backup)
                .OrderByDescending(s => s.fecha_solicitud)
                .Select(s => new SolicitudRestauracionVM
                {
                    id_solicitud = s.id_solicitud,
                    nombre_archivo_backup = s.Backup != null ? s.Backup.NombreArchivo : "N/A",
                    fecha_backup = s.fecha_backup,
                    fecha_solicitud = s.fecha_solicitud,
                    estado = s.estado,
                    motivo_solicitud = s.motivo_solicitud ?? "",
                    nombre_gerente = s.Gerente != null ? (s.Gerente.Nombre + " " + s.Gerente.apellido) : "Sin asignar",
                    observaciones_gerente = s.observaciones_gerente ?? ""
                })
                .ToListAsync();
        }

        // Método privado para restauración
        private async Task EjecutarRestauracion(string rutaBackup, string connectionString)
        {
            // Implementar según tu motor de BD y tipo de backup
            // Ejemplo con SQL Server:
            // RESTORE DATABASE [NombreBD] FROM DISK = 'ruta.bak' WITH REPLACE;

            await Task.Delay(1000); // Simular proceso
        }
    }

    // DTO para Backup
    public class BackupDTO
    {
        public int id { get; set; }
        public string nombre_archivo { get; set; }
        public DateTime fecha { get; set; }
        public string ruta_archivo { get; set; }
    }

    // ViewModel para solicitudes
    public class SolicitudRestauracionVM
    {
        public int id_solicitud { get; set; }
        public int id_supervisor { get; set; }
        public string nombre_supervisor { get; set; }
        public string nombre_archivo_backup { get; set; }
        public string ruta_archivo { get; set; }
        public DateTime fecha_solicitud { get; set; }
        public DateTime fecha_backup { get; set; }
        public string estado { get; set; }
        public string nombre_gerente { get; set; }
        public string motivo_solicitud { get; set; }
        public string observaciones_gerente { get; set; }
    }
}