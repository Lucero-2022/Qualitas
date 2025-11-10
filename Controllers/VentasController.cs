using System;
using System.Collections.Generic;
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

        public async Task<IActionResult> Ventas(DateTime? fechaDesde, DateTime? fechaHasta, string? oficina = null, int? anioSeleccionado = null, string? periodicidad = "Mensual")
        {
            if (!TryGetSesion(out int usuarioId, out string idAgente, out string rol))
                return RedirectToAction("Login", "Usuario");

            var hoy = DateTime.Now;
            var anioActual = anioSeleccionado ?? hoy.Year;
            var anioAnterior = anioActual - 1;
            var mesActual = hoy.Month;

            var cobranzas = _context.Cobranzas.AsQueryable();
            var producciones = _context.Producciones.AsQueryable();

            // 🔒 Filtrar por agente si no es Admin
            if (rol != "Admin")
            {
                cobranzas = cobranzas.Where(c => c.IDAgente.Trim().ToUpper() == idAgente.Trim().ToUpper());
                producciones = producciones.Where(p => p.IDAgente.Trim().ToUpper() == idAgente.Trim().ToUpper());
            }

            // 🔒 Filtrar por oficina si se selecciona
            if (!string.IsNullOrEmpty(oficina))
            {
                cobranzas = cobranzas.Where(c => c.NombreOficina == oficina);
                producciones = producciones.Where(p => p.NombreOficina == oficina);
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

            // ========== SERIES PARA GRÁFICA ==========
            var labels = new List<string>();
            var serieCobranza = new List<decimal>();
            var serieProduccion = new List<decimal>();

            if (periodicidad == "Mensual")
            {
                for (int m = 1; m <= 12; m++)
                {
                    labels.Add(new DateTime(anioActual, m, 1).ToString("MMM"));
                    serieCobranza.Add(await cobranzas.Where(c => c.FechaPago.Year == anioActual && c.FechaPago.Month == m)
                                                     .SumAsync(c => (decimal?)c.Primaneta) ?? 0);
                    serieProduccion.Add(await producciones.Where(p => p.FechaEmision.Year == anioActual && p.FechaEmision.Month == m)
                                                          .SumAsync(p => (decimal?)p.Primaneta) ?? 0);
                }
            }
            else if (periodicidad == "Cuatrimestral")
            {
                var periodos = new[] { (1, 4, "Q1"), (5, 8, "Q2"), (9, 12, "Q3") };
                foreach (var (ini, fin, nombre) in periodos)
                {
                    labels.Add(nombre);
                    serieCobranza.Add(await cobranzas.Where(c => c.FechaPago.Year == anioActual && c.FechaPago.Month >= ini && c.FechaPago.Month <= fin)
                                                     .SumAsync(c => (decimal?)c.Primaneta) ?? 0);
                    serieProduccion.Add(await producciones.Where(p => p.FechaEmision.Year == anioActual && p.FechaEmision.Month >= ini && p.FechaEmision.Month <= fin)
                                                          .SumAsync(p => (decimal?)p.Primaneta) ?? 0);
                }
            }
            else // Anual
            {
                labels.Add(anioActual.ToString());
                serieCobranza.Add(await cobranzas.Where(c => c.FechaPago.Year == anioActual)
                                                 .SumAsync(c => (decimal?)c.Primaneta) ?? 0);
                serieProduccion.Add(await producciones.Where(p => p.FechaEmision.Year == anioActual)
                                                      .SumAsync(p => (decimal?)p.Primaneta) ?? 0);
            }

            // ========== TABLA HISTÓRICA ==========

            var historico = new List<HistoricoRow>();
            for (int m = 1; m <= 12; m++)
            {
                var anterior = await cobranzas.Where(c => c.FechaPago.Year == anioAnterior && c.FechaPago.Month == m)
                                              .SumAsync(c => (decimal?)c.Primaneta) ?? 0;
                var actual = await cobranzas.Where(c => c.FechaPago.Year == anioActual && c.FechaPago.Month == m)
                                            .SumAsync(c => (decimal?)c.Primaneta) ?? 0;
                var prevMesActual = m == 1 ? 0 :
                    await cobranzas.Where(c => c.FechaPago.Year == anioActual && c.FechaPago.Month == m - 1)
                                   .SumAsync(c => (decimal?)c.Primaneta) ?? 0;

                historico.Add(new HistoricoRow
                {
                    Mes = new DateTime(anioActual, m, 1).ToString("MMM"),
                    ValorAnterior = anterior,
                    ValorActual = actual,
                    Incremento = actual - prevMesActual
                });
            }

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
                FechaUltimaActualizacion = ultimaFechaGlobal ?? DateTime.MinValue,
                Labels = labels,
                SerieCobranza = serieCobranza,
                SerieProduccion = serieProduccion,
                Historico = historico,
                AnioSeleccionado = anioActual,
                Oficinas = await _context.Cobranzas.Select(c => c.NombreOficina).Distinct().ToListAsync()
            };

            return View(model);
        }
    }
}
