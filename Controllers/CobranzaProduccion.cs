using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Qualitas.Models;
using Qualitas.Models.DTOs;

namespace Qualitas.Controllers
{
    public class CobranzaProduccionController : Controller
    {
        private readonly AppDbContext _context;

        public CobranzaProduccionController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? fechaDesde, DateTime? fechaHasta, string? oficina, string? agente)
        {
            var producciones = await _context.Producciones
                .Where(p =>
                    (!fechaDesde.HasValue || p.FechaEmision >= fechaDesde) &&
                    (!fechaHasta.HasValue || p.FechaEmision <= fechaHasta) &&
                    (string.IsNullOrEmpty(oficina) || p.NombreOficina == oficina) &&
                    (string.IsNullOrEmpty(agente) || p.NombreAgente == agente))
                .ToListAsync();

            var cobranzas = await _context.Cobranzas
                .Where(c =>
                    (!fechaDesde.HasValue || c.FechaPago >= fechaDesde) &&
                    (!fechaHasta.HasValue || c.FechaPago <= fechaHasta) &&
                    (string.IsNullOrEmpty(oficina) || c.NombreOficina == oficina) &&
                    (string.IsNullOrEmpty(agente) || c.NombreAgente == agente))
                .ToListAsync();

            var totalProduccion = producciones.Sum(p => p.Primaneta);
            var totalCobranza = cobranzas.Sum(c => c.Primaneta);
            var porcentaje = totalProduccion == 0 ? 0 : (double)totalCobranza / (double)totalProduccion;

            var model = new CobranzaProduccionDTOs
            {
                Producciones = producciones,
                Cobranzas = cobranzas,
                TotalProduccion = totalProduccion,
                TotalCobranza = totalCobranza,
                PorcentajeCobranzaProduccion = porcentaje,
                FechaUltimaActualizacion = DateTime.Now,
                Oficinas = await _context.Producciones.Select(p => p.NombreOficina).Distinct().ToListAsync(),
                Agentes = await _context.Producciones.Select(p => p.NombreAgente).Distinct().ToListAsync()
            };


            ViewBag.Oficinas = await _context.Producciones
                .Select(p => p.NombreOficina)
                .Distinct()
                .ToListAsync();

            ViewBag.Agentes = await _context.Producciones
                .Select(p => p.NombreAgente)
                .Distinct()
                .ToListAsync();

            return View("CobranzaProduccion", model);

        }
    }
}
