using System;
using System.Collections.Generic;

namespace Qualitas.Models.DTOs
{
    public class VentaDtos
    {
        // ===== Listas principales =====
        public List<Produccion> Producciones { get; set; } = new();
        public List<Cobranza> Cobranzas { get; set; } = new();

        // ===== Metadatos =====
        public string UltimaActualizacion { get; set; } = "Sin datos";
        public DateTime FechaUltimaActualizacion { get; set; }

        // ===== Totales generales =====
        public decimal TotalProduccion { get; set; }
        public decimal TotalCobranza { get; set; }

        // ===== Acumulados de Cobranza =====
        public decimal AcumuladoCobranzaAnterior { get; set; }
        public decimal AcumuladoCobranzaActual { get; set; }

        // ===== Acumulados de Producción =====
        public decimal AcumuladoProduccionAnterior { get; set; }
        public decimal AcumuladoProduccionActual { get; set; }

        // ===== Relación entre cobranza y producción =====
        public double PorcentajeCobranzaProduccion { get; set; }

        // ===== Filtros =====
        public List<string> Oficinas { get; set; } = new();
        public List<string> Agentes { get; set; } = new();

        // ===== Datos para la tabla =====
        public List<VentaItemDTO> Ventas { get; set; } = new();
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }

        // ===== KPIs (para tarjetas del dashboard) =====
        public decimal CobranzaMesActual { get; set; }
        public decimal ProduccionMesActual { get; set; }
        public decimal CobranzaAnioAnterior { get; set; }
        public decimal CobranzaAnioActual { get; set; }
        public decimal ProduccionAnioAnterior { get; set; }
        public decimal ProduccionAnioActual { get; set; }
    }

    // DTO de cada fila de la tabla de Ventas
    public class VentaItemDTO
    {
        public DateTime Fecha { get; set; }
        public string Folio { get; set; } = string.Empty;
        public string Agente { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string Oficina { get; set; } = string.Empty;
        public string Entidad { get; set; } = string.Empty;
        public decimal Produccion { get; set; }
        public decimal Cobranza { get; set; }
        public decimal Total { get; set; }
    }
}
