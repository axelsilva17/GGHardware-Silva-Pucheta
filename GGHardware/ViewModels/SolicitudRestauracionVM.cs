using System;
using System.Collections.Generic;

namespace TuProyecto.ViewModels
{
    public class SolicitudRestauracion
    {
        public int id_solicitud { get; set; }
        public int id_supervisor { get; set; }
        public string nombre_supervisor { get; set; }
        public int? id_gerente { get; set; }
        public string nombre_gerente { get; set; }
        public int id_backup { get; set; }
        public string nombre_archivo_backup { get; set; }
        public DateTime fecha_backup { get; set; }
        public string ruta_archivo { get; set; }
        public DateTime fecha_solicitud { get; set; }
        public string estado { get; set; }
        public string motivo_solicitud { get; set; }
        public string observaciones_gerente { get; set; }
        public DateTime? fecha_aprobacion { get; set; }
        public DateTime? fecha_restauracion { get; set; }
    }

    public class CrearSolicitudVM
    {
        public int id_backup { get; set; }
        public string motivo_solicitud { get; set; }
        public List<BackupDTO> backups_disponibles { get; set; }
    }

    public class BackupDTO
    {
        public int id { get; set; }
        public string nombre_archivo { get; set; }
        public DateTime fecha { get; set; }
        public string ruta_archivo { get; set; }
    }

    public class GestionarSolicitudVM
    {
        public int id_solicitud { get; set; }
        public bool aprobar { get; set; }
        public string observaciones { get; set; }
    }
}