using Qualitas.Models;

namespace Qualitas.Models.DTOs
{
    public class ReservaDTOs
    {
        public List<Reserva> Reservas { get; set; } = new();

        // KPIs
        public int TotalReservas { get; set; }
        public decimal SumaAjustes { get; set; }
        public decimal MaximoHoy { get; set; }
        public decimal SumaMesActual { get; set; }

        // Gráficas
        public List<CoberturaDTO> Coberturas { get; set; } = new();
        public List<OficinaDTO> Oficinas { get; set; } = new();

        // Filtros activos
        public string FechaFiltro { get; set; } = "";
        public string Busqueda { get; set; } = "";
        public string Oficina { get; set; } = "";

        // Otros
        public string UltimaActualizacion { get; set; } = "Sin datos";
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
    }

    public record CoberturaDTO(string Cobertura, decimal SumaAjustes, int Conteo);
    public record OficinaDTO(string Oficina, decimal AjusteTotal);
}
