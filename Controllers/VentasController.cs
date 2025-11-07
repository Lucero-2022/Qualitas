using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Qualitas.Models;
using Qualitas.Models.DTOs;

namespace Qualitas.Controllers
{
    public class VentasController : BaseController
    {
        private readonly AppDbContext _context;

        public VentasController(AppDbContext context) => _context = context;

        public async Task<IActionResult> Ventas(DateTime? fechaDesde, DateTime? fechaHasta)

        {
            if (!TryGetSesion(out int usuarioId, out string idAgente, out string rol))
                return RedirectToAction("Login", "Usuario");

            var hoy = DateTime.Now;
            var anioActual = hoy.Year;
            var anioAnterior = hoy.Year - 1;
            var mesActual = hoy.Month;

            var cobranzas = _context.Cobranzas.AsQueryable();
            var producciones = _context.Producciones.AsQueryable();


            // 🔒 Filtrar por agente si no es Admin

            if (rol != "Admin")
            {
                cobranzas = cobranzas.Where(c => c.IDAgente.Trim().ToUpper() == idAgente.Trim().ToUpper());
                producciones = producciones.Where(p => p.IDAgente.Trim().ToUpper() == idAgente.Trim().ToUpper());

            }


            // ========== KPIs COBRANZA ==========

            decimal cobranzaMesActual = await cobranzas
                .Where(c => c.FechaPago.Month == mesActual && c.FechaPago.Year == anioActual)
                .SumAsync(c => (decimal?)c.Primaneta) ?? 0;

            decimal cobranzaAnioAnterior = await cobranzas
                .Where(c => c.FechaPago.Year == anioAnterior)
                .SumAsync(c => (decimal?)c.Primaneta) ?? 0;

            decimal cobranzaAnioActual = await cobranzas
                .Where(c => c.FechaPago.Year == anioActual)
                .SumAsync(c => (decimal?)c.Primaneta) ?? 0;


            // ========== KPIs PRODUCCIÓN ==========

            decimal produccionMesActual = await producciones
                .Where(p => p.FechaEmision.Month == mesActual && p.FechaEmision.Year == anioActual)
                .SumAsync(p => (decimal?)p.Primaneta) ?? 0;

            decimal produccionAnioAnterior = await producciones
                .Where(p => p.FechaEmision.Year == anioAnterior)
                .SumAsync(p => (decimal?)p.Primaneta) ?? 0;

            decimal produccionAnioActual = await producciones
                .Where(p => p.FechaEmision.Year == anioActual)
                .SumAsync(p => (decimal?)p.Primaneta) ?? 0;

            // Última fecha global
            DateTime? ultimaFechaCobranza = await cobranzas.MaxAsync(c => (DateTime?)c.FechaPago);
            DateTime? ultimaFechaProduccion = await producciones.MaxAsync(p => (DateTime?)p.FechaEmision);
            DateTime? ultimaFechaGlobal = new[] { ultimaFechaCobranza, ultimaFechaProduccion }
                .Where(f => f.HasValue)
                .Max();

            var model = new VentaDtos
            {
                CobranzaMesActual = cobranzaMesActual,
                ProduccionMesActual = produccionMesActual,
                CobranzaAnioAnterior = cobranzaAnioAnterior,
                CobranzaAnioActual = cobranzaAnioActual,
                ProduccionAnioAnterior = produccionAnioAnterior,
                ProduccionAnioActual = produccionAnioActual,
                UltimaActualizacion = ultimaFechaGlobal.HasValue
                    ? ultimaFechaGlobal.Value.ToString("dd/MM/yyyy")
                    : "Sin datos",
                FechaUltimaActualizacion = ultimaFechaGlobal ?? DateTime.MinValue
            };

            return View(model);
        }
    }
}
