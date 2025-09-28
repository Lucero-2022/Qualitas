using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qualitas.Models
{
    public class Reserva
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string IDAgente { get; set; } = string.Empty;

        [Column(TypeName = "date")] // Mapea a SQL Server tipo DATE
        public DateTime Fecha { get; set; }

        public int Ramo { get; set; }

        public int Ejercicio { get; set; }

        public int Oficina { get; set; }

        [Required]
        public string Cobertura { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Ajuste { get; set; }

        [Required]
        public string Entidad { get; set; } = string.Empty;

        // ⚠️ En SQL es INT, así que aquí también debe ser int
        public int Poliza { get; set; }

        [Required]
        public string NombreAsegurado { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal SumaAsegurada { get; set; }

        public int Agente { get; set; } 

        [Required]
        public string NombreAgente { get; set; } = string.Empty;

        [Required]
        public string NombreOficina { get; set; } = string.Empty;

        // FK a Usuario
        public int UsuarioId { get; set; }
    }
}

