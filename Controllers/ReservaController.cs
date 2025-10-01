using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Qualitas.Models;
using Qualitas.Models.DTOs;

namespace Qualitas.Controllers
{
    public class ReservaController : BaseController
    {
        private readonly AppDbContext _context;

        public ReservaController(AppDbContext context) => _context = context;

        // Vista principal
        public async Task<IActionResult> Reservas(string? busqueda, string? oficina, DateTime? fechaDesde, DateTime? fechaHasta, int? page)
        {
            if (!TryGetSesion(out int usuarioId, out string idAgente, out string rol))
                return RedirectToAction("Login", "Usuario");

            // Query base con filtros y restricción de último día cuando no hay fechas
            var query = ConstruirQuery(usuarioId, idAgente, rol, fechaDesde, fechaHasta, busqueda, oficina);

            // ========== KPIs por agregación en SQL ==========
            int totalReservas = await query.CountAsync();
            decimal sumaAjustes = totalReservas > 0 ? await query.SumAsync(r => r.Ajuste) : 0m;
            decimal maximoHoy = totalReservas > 0 ? await query.MaxAsync(r => r.Ajuste) : 0m;

            // Mes actual (se mantiene tu lógica original)
            var mesActual = DateTime.Now.Month;
            var añoActual = DateTime.Now.Year;
            var sumaMesActual = await _context.Reservas
                .Where(r => r.Fecha.Month == mesActual && r.Fecha.Year == añoActual)
                .SumAsync(r => r.Ajuste);

            // ========== Paginación de la tabla (solo filas necesarias) ==========
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var reservas = await query
                .OrderByDescending(r => r.Fecha)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Última fecha global en toda la tabla (sin filtros)
            DateTime? ultimaFechaGlobal = await _context.Reservas
                .MaxAsync(r => (DateTime?)r.Fecha);


            // ========== Agregados para el modelo (en SQL, sin rows en memoria) ==========
            var coberturasAgg = await query
                .GroupBy(r => string.IsNullOrWhiteSpace(r.Cobertura) ? "Sin cobertura" : r.Cobertura.Trim())
                .Select(g => new
                {
                    Cobertura = g.Key,
                    Suma = g.Sum(r => r.Ajuste),
                    Conteo = g.Count()
                })
                .OrderByDescending(x => x.Suma)
                .ToListAsync();

            var oficinasAgg = await query
                .GroupBy(r => string.IsNullOrWhiteSpace(r.NombreOficina) ? "Sin oficina" : r.NombreOficina.Trim())
                .Select(g => new
                {
                    Oficina = g.Key,
                    Suma = g.Sum(r => r.Ajuste)
                })
                .OrderByDescending(x => x.Suma)
                .ToListAsync();

            var model = new ReservaDTOs
            {
                Reservas = reservas,
                TotalReservas = totalReservas,
                SumaAjustes = sumaAjustes,
                MaximoHoy = maximoHoy,
                SumaMesActual = sumaMesActual,

                // Mapeo a tus DTOs
                Coberturas = coberturasAgg
                    .Select(x => new CoberturaDTO(x.Cobertura, x.Suma, x.Conteo))
                    .ToList(),
                Oficinas = oficinasAgg
                    .Select(x => new OficinaDTO(x.Oficina, x.Suma))
                    .ToList(),

                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                Busqueda = busqueda ?? "",
                Oficina = oficina ?? "",
                UltimaActualizacion = ultimaFechaGlobal.HasValue
                ? ultimaFechaGlobal.Value.ToString("dd/MM/yyyy")
                : "Sin datos",

                PaginaActual = pageNumber,
                TotalPaginas = (int)Math.Ceiling(totalReservas / (double)pageSize)
         };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_TablaReservas", model);

            return View(model);
        }

        // Graficas consultas

        [HttpGet]
        public async Task<IActionResult> DatosGraficas(DateTime? fechaDesde, DateTime? fechaHasta, string? busqueda, string? oficina)
        {
            if (!TryGetSesion(out int usuarioId, out string idAgente, out string rol))
                return Unauthorized();

            var query = ConstruirQuery(usuarioId, idAgente, rol, fechaDesde, fechaHasta, busqueda, oficina);

            var coberturas = await query
                .GroupBy(r => string.IsNullOrWhiteSpace(r.Cobertura) ? "Sin cobertura" : r.Cobertura.Trim())
                .Select(g => new { Label = g.Key, Value = g.Sum(r => r.Ajuste) })
                .ToListAsync();

            var oficinas = await query
                .GroupBy(r => string.IsNullOrWhiteSpace(r.NombreOficina) ? "Sin oficina" : r.NombreOficina.Trim())
                .Select(g => new { Label = g.Key, Value = g.Sum(r => r.Ajuste) })
                .ToListAsync();

            return Json(new
            {
                coberturas = new
                {
                    labels = coberturas.Select(c => c.Label).ToList(),
                    data = coberturas.Select(c => c.Value).ToList(),
                    colors = new[] {
                "#941B80", "#E63946", "#457B9D", "#2A9D8F",
                "#F4A261", "#E9C46A", "#8D99AE", "#06D6A0",
                "#FFD166", "#118AB2"

                }.Take(coberturas.Count).ToList()
                },
                oficinas = new
                {
                    labels = oficinas.Select(o => o.Label).ToList(),
                    data = oficinas.Select(o => o.Value).ToList(),
                    colors = new[] {
                "#457B9D", "#2A9D8F", "#F4A261", "#E63946", "#118AB2",
                "#06D6A0", "#FFD166", "#9D4EDD", "#5A189A", "#C77DFF"
                }.Take(oficinas.Count).ToList()
                }



            });
        }



        #region Helpers privados

        private IQueryable<Reserva> ConstruirQuery(
            int usuarioId,
            string idAgente,
            string rol,
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            string? busqueda,
            string? oficina)
        {
            var query = _context.Reservas.AsQueryable();

            // Última fecha global si no hay filtros
            if (!fechaDesde.HasValue && !fechaHasta.HasValue)
            {
                var ultimaFecha = _context.Reservas
                    .OrderByDescending(r => r.Fecha)
                    .Select(r => r.Fecha)
                    .FirstOrDefault();
                query = query.Where(r => r.Fecha == ultimaFecha);
            }
            else
            {
                if (fechaDesde.HasValue)
                    query = query.Where(r => r.Fecha >= fechaDesde.Value);

                if (fechaHasta.HasValue)
                    query = query.Where(r => r.Fecha <= fechaHasta.Value);
            }

            if (rol != "Admin")
                query = query.Where(r => r.UsuarioId == usuarioId && r.IDAgente == idAgente);

            if (!string.IsNullOrWhiteSpace(busqueda) && int.TryParse(busqueda, out int numero))
                query = query.Where(r => r.Poliza == numero || r.Agente == numero);

            if (!string.IsNullOrWhiteSpace(oficina))
                query = query.Where(r => r.NombreOficina == oficina);

            return query;
        }

        #endregion

        private List<string> GenerarColoresGradiente(int cantidad, string baseHex = "#941B80")
        {
            var colores = new List<string>();

            // Convertir color base a RGB
            int r = Convert.ToInt32(baseHex.Substring(1, 2), 16);
            int g = Convert.ToInt32(baseHex.Substring(3, 2), 16);
            int b = Convert.ToInt32(baseHex.Substring(5, 2), 16);

            // Generar variaciones aclarando el color progresivamente
            for (int i = 0; i < cantidad; i++)
            {
                double factor = 1.0 - (i * 0.1); // cada paso 10% más claro
                int nr = Math.Min(255, (int)(r + (255 - r) * (1 - factor)));
                int ng = Math.Min(255, (int)(g + (255 - g) * (1 - factor)));
                int nb = Math.Min(255, (int)(b + (255 - b) * (1 - factor)));

                colores.Add($"#{nr:X2}{ng:X2}{nb:X2}");
            }

            return colores;
        }

    }
}
