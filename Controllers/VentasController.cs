using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Qualitas.Data;
using Qualitas.Models;
using Qualitas.Models.DTOs;
using System.Globalization;

namespace Qualitas.Controllers
{
    public class VentasController : Controller
    {
        private readonly AppDbContext _context;

        public VentasController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Ventas(string oficina, string periodicidad = "Mensual", bool mostrarProduccion = false, int anioSeleccionado = 0, int page = 1)
        {
            var model = new VentaDtos();

            // ------------------------------------------------------------------
            // CORRECCIÓN: si no envían año, usar el año actual
            // ------------------------------------------------------------------
            if (anioSeleccionado == 0)
                anioSeleccionado = DateTime.Now.Year;

            model.AnioSeleccionado = anioSeleccionado;

            // ---------------------------------------------------------
            // 1. LISTA DE OFICINAS
            // ---------------------------------------------------------
            model.Oficinas = await _context.Cobranzas
                .Select(x => x.NombreOficina)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            model.OficinaSeleccionada = oficina;
            model.PeriodicidadSeleccionada = periodicidad;
            model.MostrarProduccion = mostrarProduccion;

            // ---------------------------------------------------------
            // 2. FILTRO POR OFICINA
            // ---------------------------------------------------------
            IQueryable<Cobranza> cobranzasQuery = _context.Cobranzas;
            IQueryable<Produccion> produccionesQuery = _context.Producciones;

            if (!string.IsNullOrWhiteSpace(oficina))
            {
                cobranzasQuery = cobranzasQuery.Where(x => x.NombreOficina == oficina);
                produccionesQuery = produccionesQuery.Where(x => x.NombreOficina == oficina);
            }

            // ---------------------------------------------------------
            // 3. KPI — COBRANZA
            // ---------------------------------------------------------
            int mes = DateTime.Now.Month;
            int año = anioSeleccionado;

            model.CobranzaMesActual = await cobranzasQuery
                .Where(x => x.FechaPago.Month == mes && x.FechaPago.Year == año)
                .SumAsync(x => (decimal?)x.Primaneta) ?? 0;

            model.CobranzaAnioActual = await cobranzasQuery
                .Where(x => x.FechaPago.Year == año)
                .SumAsync(x => (decimal?)x.Primaneta) ?? 0;

            model.CobranzaAnioAnterior = await cobranzasQuery
                .Where(x => x.FechaPago.Year == año - 1)
                .SumAsync(x => (decimal?)x.Primaneta) ?? 0;

            // ---------------------------------------------------------
            // 4. KPI — PRODUCCIÓN
            // ---------------------------------------------------------
            model.ProduccionMesActual = await produccionesQuery
                .Where(x => x.FechaEmision.Month == mes && x.FechaEmision.Year == año)
                .SumAsync(x => (decimal?)x.Primaneta) ?? 0;

            model.ProduccionAnioActual = await produccionesQuery
                .Where(x => x.FechaEmision.Year == año)
                .SumAsync(x => (decimal?)x.Primaneta) ?? 0;

            model.ProduccionAnioAnterior = await produccionesQuery
                .Where(x => x.FechaEmision.Year == año - 1)
                .SumAsync(x => (decimal?)x.Primaneta) ?? 0;

            // ---------------------------------------------------------
            // 5. GRÁFICAS
            // ---------------------------------------------------------
            model.Labels = new List<string>
    {
        "Ene","Feb","Mar","Abr","May","Jun",
        "Jul","Ago","Sep","Oct","Nov","Dic"
    };

            model.SerieCobranza = new List<decimal>();
            model.SerieProduccion = new List<decimal>();

            for (int m = 1; m <= 12; m++)
            {
                model.SerieCobranza.Add(
                    await cobranzasQuery
                        .Where(x => x.FechaPago.Month == m && x.FechaPago.Year == año)
                        .SumAsync(x => (decimal?)x.Primaneta) ?? 0);

                model.SerieProduccion.Add(
                    await produccionesQuery
                        .Where(x => x.FechaEmision.Month == m && x.FechaEmision.Year == año)
                        .SumAsync(x => (decimal?)x.Primaneta) ?? 0);
            }

            // ---------------------------------------------------------
            // 6. TABLA HISTÓRICA
            // ---------------------------------------------------------

            model.Historico = new List<HistoricoRow>();

            CultureInfo ci = CultureInfo.GetCultureInfo("es-MX");

            if (periodicidad == "Mensual")
            {
                for (int m = 1; m <= 12; m++)
                {
                    decimal a1 = 0, a2 = 0;

                    if (mostrarProduccion)
                    {
                        a1 = await produccionesQuery.Where(x => x.FechaEmision.Year == año - 1 && x.FechaEmision.Month == m)
                                                    .SumAsync(x => (decimal?)x.Primaneta) ?? 0;
                        a2 = await produccionesQuery.Where(x => x.FechaEmision.Year == año && x.FechaEmision.Month == m)
                                                    .SumAsync(x => (decimal?)x.Primaneta) ?? 0;
                    }
                    else
                    {
                        a1 = await cobranzasQuery.Where(x => x.FechaPago.Year == año - 1 && x.FechaPago.Month == m)
                                                 .SumAsync(x => (decimal?)x.Primaneta) ?? 0;
                        a2 = await cobranzasQuery.Where(x => x.FechaPago.Year == año && x.FechaPago.Month == m)
                                                 .SumAsync(x => (decimal?)x.Primaneta) ?? 0;
                    }

                    model.Historico.Add(new HistoricoRow
                    {
                        Mes = ci.DateTimeFormat.GetMonthName(m), // nombre completo del mes
                        ValorAnterior = a1,
                        ValorActual = a2,
                        Incremento = a2 - a1,
                        IncrementoPorcentaje = a1 != 0 ? ((a2 - a1) / a1 * 100) : 0
                    });
                }
            }
            else if (periodicidad == "Cuatrimestral")
            {
                int[][] cuatrimestres = new int[][]
                {
        new int[] {1,2,3,4},
        new int[] {5,6,7,8},
        new int[] {9,10,11,12}
                };

                foreach (var grupo in cuatrimestres)
                {
                    decimal a1 = 0, a2 = 0;
                    foreach (var m in grupo)
                    {
                        if (mostrarProduccion)
                        {
                            a1 += await produccionesQuery.Where(x => x.FechaEmision.Year == año - 1 && x.FechaEmision.Month == m)
                                                        .SumAsync(x => (decimal?)x.Primaneta) ?? 0;
                            a2 += await produccionesQuery.Where(x => x.FechaEmision.Year == año && x.FechaEmision.Month == m)
                                                        .SumAsync(x => (decimal?)x.Primaneta) ?? 0;
                        }
                        else
                        {
                            a1 += await cobranzasQuery.Where(x => x.FechaPago.Year == año - 1 && x.FechaPago.Month == m)
                                                      .SumAsync(x => (decimal?)x.Primaneta) ?? 0;
                            a2 += await cobranzasQuery.Where(x => x.FechaPago.Year == año && x.FechaPago.Month == m)
                                                      .SumAsync(x => (decimal?)x.Primaneta) ?? 0;
                        }
                    }

                    model.Historico.Add(new HistoricoRow
                    {
                        Mes = $"{ci.DateTimeFormat.GetMonthName(grupo.First())} - {ci.DateTimeFormat.GetMonthName(grupo.Last())}",
                        ValorAnterior = a1,
                        ValorActual = a2,
                        Incremento = a2 - a1,
                        IncrementoPorcentaje = a1 != 0 ? ((a2 - a1) / a1 * 100) : 0
                    });
                }
            }
            else if (periodicidad == "Anual")
            {
                decimal a1 = 0, a2 = 0;
                for (int m = 1; m <= 12; m++)
                {
                    if (mostrarProduccion)
                    {
                        a1 += await produccionesQuery.Where(x => x.FechaEmision.Year == año - 1 && x.FechaEmision.Month == m)
                                                    .SumAsync(x => (decimal?)x.Primaneta) ?? 0;
                        a2 += await produccionesQuery.Where(x => x.FechaEmision.Year == año && x.FechaEmision.Month == m)
                                                    .SumAsync(x => (decimal?)x.Primaneta) ?? 0;
                    }
                    else
                    {
                        a1 += await cobranzasQuery.Where(x => x.FechaPago.Year == año - 1 && x.FechaPago.Month == m)
                                                 .SumAsync(x => (decimal?)x.Primaneta) ?? 0;
                        a2 += await cobranzasQuery.Where(x => x.FechaPago.Year == año && x.FechaPago.Month == m)
                                                 .SumAsync(x => (decimal?)x.Primaneta) ?? 0;
                    }
                }

                model.Historico.Add(new HistoricoRow
                {
                    Mes = "Año completo",
                    ValorAnterior = a1,
                    ValorActual = a2,
                    Incremento = a2 - a1,
                    IncrementoPorcentaje = a1 != 0 ? ((a2 - a1) / a1 * 100) : 0

                });
            }



            // ---------------------------------------------------------
            // 7. FECHA DE ACTUALIZACIÓN
            // ---------------------------------------------------------
            if (mostrarProduccion)
            {
                model.FechaUltimaActualizacion = await produccionesQuery
                    .OrderByDescending(x => x.FechaEmision)
                    .Select(x => x.FechaEmision)
                    .FirstOrDefaultAsync();
            }
            else
            {
                model.FechaUltimaActualizacion = await cobranzasQuery
                    .OrderByDescending(x => x.FechaPago)
                    .Select(x => x.FechaPago)
                    .FirstOrDefaultAsync();
            }

            return View(model);
        }

    }
}
