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
        public List<string> Oficinas { get; set; } = new();       // lista de oficinas disponibles
        public string OficinaSeleccionada { get; set; } = string.Empty; // oficina elegida en el filtro
        public List<int> Anios { get; set; } = new();             // lista de años disponibles
        public int AnioSeleccionado { get; set; }                 // año elegido en el filtro
        public List<string> Agentes { get; set; } = new();        // lista de agentes disponibles
        public string AgenteSeleccionado { get; set; } = string.Empty; // agente elegido en el filtro


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



        // ===== KPIs de Ventas =====
        public decimal VentasMesActual { get; set; }
        public decimal VentasAnioAnterior { get; set; }
        public decimal VentasAnioActual { get; set; }



        // ===== Series para gráficas =====
        public List<string> Labels { get; set; } = new();
        public List<decimal> SerieCobranza { get; set; } = new();
        public List<decimal> SerieProduccion { get; set; } = new();
        public List<decimal> SerieVentas { get; set; } = new();



        // ===== Tabla histórica =====

        public string PeriodicidadSeleccionada { get; set; } = "Mensual";

        public List<HistoricoRow> Historico { get; set; } = new();

        public bool MostrarProduccion { get; set; }



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
    
    // DTO para la tabla histórica
    public class HistoricoRow
    {
        public string Mes { get; set; } = string.Empty;
        public decimal ValorAnterior { get; set; }
        public decimal ValorActual { get; set; }
        public decimal Incremento { get; set; }

        // NUEVA propiedad para % de crecimiento
        public decimal IncrementoPorcentaje { get; set; }
    }

}
