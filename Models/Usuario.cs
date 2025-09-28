using System.ComponentModel.DataAnnotations;

namespace Qualitas.Models   // 👈 cámbialo de Modelos a Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string IDAgente { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Contraseña { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Rol { get; set; } = "Agente"; // 👈 aquí agregamos el rol
    }
}
