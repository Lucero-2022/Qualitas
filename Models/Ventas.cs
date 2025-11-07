using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Qualitas.Models
{
    public class Cobranza
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string IDAgente { get; set; } = string.Empty;

        [Required]
        public int Promotor { get; set; }

        [Required]
        [MaxLength(100)]
        public string NombreAgente { get; set; } = string.Empty;

        [Required]
        public string TipoPoliza { get; set; } = string.Empty;

        [Required]
        public int Poliza { get; set; }

        [Required]
        public string NombreAsegurado { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Primaneta { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime FechaPago { get; set; }


        [Required]
        public int Oficina { get; set; }

        [Required]
        public string NombreOficina { get; set; } = string.Empty;
    }
}