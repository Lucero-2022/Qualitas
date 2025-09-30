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
            // 🔐 Validar sesión
            if (!TryGetSesion(out int usuarioId, out string idAgente, out string rol))
                return RedirectToAction("Login", "Usuario");

            // Última fecha registrada en la tabla (global)
            var ultimaFecha = await _context.Reservas
                .OrderByDescending(r => r.Fecha)
                .Select(r => r.Fecha)
                .FirstOrDefaultAsync();

            // Construir query base
            var query = _context.Reservas.AsQueryable();

            // Si no hay filtros → mostrar solo la última fecha registrada
            if (!fechaDesde.HasValue && !fechaHasta.HasValue)
            {
                query = query.Where(r => r.Fecha == ultimaFecha);
            }
            else
            {
                // Si hay filtros → aplicar rango
                if (fechaDesde.HasValue)
                    query = query.Where(r => r.Fecha >= fechaDesde.Value);

                if (fechaHasta.HasValue)
                    query = query.Where(r => r.Fecha <= fechaHasta.Value);
            }

            // Filtro por rol
            if (rol != "Admin")
                query = query.Where(r => r.UsuarioId == usuarioId && r.IDAgente == idAgente);

            // Filtro por búsqueda
            if (!string.IsNullOrWhiteSpace(busqueda) && int.TryParse(busqueda, out int numero))
                query = query.Where(r => r.Poliza == numero || r.Agente == numero);

            // Filtro por oficina
            if (!string.IsNullOrWhiteSpace(oficina))
                query = query.Where(r => r.NombreOficina == oficina);

            // Ejecutar query
            var rows = await query.ToListAsync();

            // ======================
            // INICIO DE KPIs
            // ======================
            int totalReservas;
            decimal sumaAjustes;
            decimal maximoHoy;

            if (!fechaDesde.HasValue && !fechaHasta.HasValue)
            {
                // KPIs con la última fecha global
                var rowsUltimaFecha = await _context.Reservas
                    .Where(r => r.Fecha == ultimaFecha)
                    .ToListAsync();

                totalReservas = rowsUltimaFecha.Count;
                sumaAjustes = rowsUltimaFecha.Sum(r => r.Ajuste);
                maximoHoy = rowsUltimaFecha.Count > 0 ? rowsUltimaFecha.Max(r => r.Ajuste) : 0m;
            }
            else
            {
                // KPIs con los registros filtrados
                totalReservas = rows.Count;
                sumaAjustes = rows.Sum(r => r.Ajuste);
                maximoHoy = rows.Count > 0 ? rows.Max(r => r.Ajuste) : 0m;
            }

            // KPI del mes actual (basado en fecha del sistema)
            var mesActual = DateTime.Now.Month;
            var añoActual = DateTime.Now.Year;

            var sumaMesActual = await _context.Reservas
                .Where(r => r.Fecha.Month == mesActual && r.Fecha.Year == añoActual)
                .SumAsync(r => r.Ajuste);

            // ======================
            // Paginación
            // ======================
            int pageSize = 10;
            int pageNumber = page ?? 1;
            var reservas = rows
                .OrderByDescending(r => r.Fecha)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // ======================
            // Construcción del modelo
            // ======================
            var model = new ReservaDTOs
            {
                Reservas = reservas,
                TotalReservas = totalReservas,
                SumaAjustes = sumaAjustes,
                MaximoHoy = maximoHoy,
                SumaMesActual = sumaMesActual,
                Coberturas = AgruparCoberturas(rows),
                Oficinas = AgruparOficinas(rows),
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                Busqueda = busqueda ?? "",
                Oficina = oficina ?? "",
                UltimaActualizacion = ultimaFecha != default(DateTime)
                    ? ultimaFecha.ToString("dd/MM/yyyy")
                    : "Sin datos",
                PaginaActual = pageNumber,
                TotalPaginas = (int)Math.Ceiling(rows.Count / (double)pageSize)
            };

            // 🔄 Si es AJAX → solo tabla
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_TablaReservas", model);

            // 🔄 Si es carga normal → vista completa
            return View(model);
        }

        // Endpoint AJAX para gráficas
        [HttpGet]
        public async Task<IActionResult> DatosGraficas(DateTime? fechaDesde, DateTime? fechaHasta, string? busqueda, string? oficina)
        {
            if (!TryGetSesion(out int usuarioId, out string idAgente, out string rol))
                return Unauthorized();

            // Última fecha registrada en la tabla (global)
            var ultimaFecha = await _context.Reservas
                .OrderByDescending(r => r.Fecha)
                .Select(r => r.Fecha)
                .FirstOrDefaultAsync();

            List<Reserva> rows;

            if (!fechaDesde.HasValue && !fechaHasta.HasValue)
            {
                // Sin filtros → usar solo la última fecha global
                rows = await _context.Reservas
                    .Where(r => r.Fecha == ultimaFecha)
                    .ToListAsync();
            }
            else
            {
                // Con filtros → usar helper con rango
                rows = await FiltrarReservas(usuarioId, idAgente, rol, fechaDesde, fechaHasta, busqueda, oficina);
            }

            var model = new ReservaDTOs
            {
                Coberturas = AgruparCoberturas(rows),
                Oficinas = AgruparOficinas(rows),
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                Busqueda = busqueda ?? "",
                Oficina = oficina ?? ""
            };

            return Json(model);
        }

        #region Helpers privados

        private async Task<List<Reserva>> FiltrarReservas(
            int usuarioId,
            string idAgente,
            string rol,
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            string? busqueda,
            string? oficina)
        {
            var query = _context.Reservas.AsQueryable();

            if (fechaDesde.HasValue)
                query = query.Where(r => r.Fecha >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(r => r.Fecha <= fechaHasta.Value);

            if (rol != "Admin")
                query = query.Where(r => r.UsuarioId == usuarioId && r.IDAgente == idAgente);

            if (!string.IsNullOrWhiteSpace(busqueda) && int.TryParse(busqueda, out int numero))
                query = query.Where(r => r.Poliza == numero || r.Agente == numero);

            if (!string.IsNullOrWhiteSpace(oficina))
                query = query.Where(r => r.NombreOficina == oficina);

            return await query.ToListAsync();
        }

        private List<CoberturaDTO> AgruparCoberturas(List<Reserva> rows) =>
            rows.GroupBy(r => string.IsNullOrWhiteSpace(r.Cobertura) ? "Sin cobertura" : r.Cobertura.Trim())
                .Select(g => new CoberturaDTO(g.Key, g.Sum(r => r.Ajuste), g.Count()))
                .OrderByDescending(x => x.SumaAjustes)
                .ToList();

        private List<OficinaDTO> AgruparOficinas(List<Reserva> rows) =>
            rows.GroupBy(r => string.IsNullOrWhiteSpace(r.NombreOficina) ? "Sin oficina" : r.NombreOficina.Trim())
                .Select(g => new OficinaDTO(g.Key, g.Sum(r => r.Ajuste)))
                .OrderByDescending(x => x.AjusteTotal)
                .ToList();

        #endregion
    }
}
