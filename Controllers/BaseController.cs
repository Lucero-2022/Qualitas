using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Qualitas.Models;
using Qualitas.Models.DTOs;

public class BaseController : Controller
{
    protected bool TryGetSesion(out int usuarioId, out string idAgente, out string rol)
    {
        usuarioId = 0;
        idAgente = HttpContext.Session.GetString("IDAgente") ?? "";
        rol = HttpContext.Session.GetString("Rol") ?? "User";

        var sid = HttpContext.Session.GetString("UsuarioId");
        return !string.IsNullOrEmpty(sid) && int.TryParse(sid, out usuarioId) && !string.IsNullOrEmpty(idAgente);
    }

    protected async Task<DateTime> ObtenerFechaValidaAsync(AppDbContext context, DateTime? fecha)
    {
        if (fecha != null) return fecha.Value.Date;

        return await context.Reservas
            .OrderByDescending(r => r.Fecha)
            .Select(r => (DateTime?)r.Fecha)
            .FirstOrDefaultAsync() ?? DateTime.Today;
    }
}
