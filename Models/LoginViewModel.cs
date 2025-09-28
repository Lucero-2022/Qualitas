using System.ComponentModel.DataAnnotations;
namespace Qualitas.Models;


public class LoginViewModel
{
    [Required]
    public string? IDAgente { get; set; }

    [Required]
    public string?  Contraseña { get; set; }
}
