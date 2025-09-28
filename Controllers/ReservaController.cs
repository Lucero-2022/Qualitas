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
        public async Task<IActionResult> Reservas(string? busqueda, string? oficina, DateTime? fecha, int? page)
        {
            // 🔐 Validar sesión
            if (!TryGetSesion(out int usuarioId, out string idAgente, out string rol))
                return RedirectToAction("Login", "Usuario");

            var dia = await ObtenerFechaValidaAsync(_context, fecha);
            var rows = await FiltrarReservas(usuarioId, idAgente, rol, dia, busqueda, oficina);

            // KPIs
            var totalReservas = rows.Count;
            var sumaAjustes = rows.Sum(r => r.Ajuste);
            var maximoHoy = rows.Count > 0 ? rows.Max(r => r.Ajuste) : 0m;

            var mesActual = DateTime.Now.Month;
            var añoActual = DateTime.Now.Year;

            var sumaMesActual = await _context.Reservas
                .Where(r => r.Fecha.Month == mesActual && r.Fecha.Year == añoActual)
                .Where(r => rol == "Admin" || (r.UsuarioId == usuarioId && r.IDAgente == idAgente))
                .SumAsync(r => r.Ajuste);

            // Paginación
            int pageSize = 10;
            int pageNumber = page ?? 1;
            var reservas = rows
                .OrderByDescending(r => r.Fecha)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var ultimaFecha = rows
                .OrderByDescending(r => r.Fecha)
                .Select(r => r.Fecha)
                .FirstOrDefault();

            // DTO
            var model = new ReservaDTOs
            {
                Reservas = reservas,
                TotalReservas = totalReservas,
                SumaAjustes = sumaAjustes,
                MaximoHoy = maximoHoy,
                SumaMesActual = sumaMesActual,
                Coberturas = AgruparCoberturas(rows),
                Oficinas = AgruparOficinas(rows),
                FechaFiltro = dia.ToString("yyyy-MM-dd"),
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
        public async Task<IActionResult> DatosGraficas(DateTime? fecha, string? busqueda, string? oficina)
        {
            if (!TryGetSesion(out int usuarioId, out string idAgente, out string rol))
                return Unauthorized();

            var dia = await ObtenerFechaValidaAsync(_context, fecha);
            var rows = await FiltrarReservas(usuarioId, idAgente, rol, dia, busqueda, oficina);

            var model = new ReservaDTOs
            {
                Coberturas = AgruparCoberturas(rows),
                Oficinas = AgruparOficinas(rows),
                FechaFiltro = dia.ToString("yyyy-MM-dd"),
                Busqueda = busqueda ?? "",
                Oficina = oficina ?? ""
            };

            return Json(model);
        }


        #region Helpers privados

        private async Task<List<Reserva>> FiltrarReservas(int usuarioId, string idAgente, string rol, DateTime fecha, string? busqueda, string? oficina)
        {
            var query = _context.Reservas.AsQueryable();

            query = query.Where(r => r.Fecha == fecha);

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
