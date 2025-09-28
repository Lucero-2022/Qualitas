using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Qualitas.Models;

namespace Qualitas.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly AppDbContext _context;

        public UsuarioController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Si ya hay sesión activa, manda directo al dashboard
            var usuarioId = HttpContext.Session.GetString("UsuarioId");
            if (!string.IsNullOrEmpty(usuarioId))
                return RedirectToAction("Index", "Home");


            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var agente = (model.IDAgente ?? string.Empty).Trim();
            var pass = (model.Contraseña ?? string.Empty).Trim();

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IDAgente == agente && u.Contraseña == pass);

            if (usuario is null)
            {
                ModelState.AddModelError(string.Empty, "Credenciales inválidas");
                return View(model);
            }

            // 🔹 Guardar en sesión los datos clave
            HttpContext.Session.SetString("UsuarioId", usuario.Id.ToString());
            HttpContext.Session.SetString("IDAgente", usuario.IDAgente);
            HttpContext.Session.SetString("Rol", usuario.Rol); // 👈 ahora sí guardamos el rol

            return RedirectToAction("Index", "Home");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]  
        public IActionResult Logout()
        {
            // 🔹 Limpiar toda la sesión
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }
    }
}
