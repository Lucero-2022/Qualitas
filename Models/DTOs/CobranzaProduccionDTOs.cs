using System;
using System.Collections.Generic;
using Qualitas.Models;

namespace Qualitas.Models.DTOs
{
    public class CobranzaProduccionDTOs
    {
        public List<Produccion> Producciones { get; set; } = new();
        public List<Cobranza> Cobranzas { get; set; } = new();

        public decimal TotalProduccion { get; set; }
        public decimal TotalCobranza { get; set; }
        public double PorcentajeCobranzaProduccion { get; set; }
        public DateTime FechaUltimaActualizacion { get; set; }

        public List<string> Oficinas { get; set; } = new();
        public List<string> Agentes { get; set; } = new();
    }
}
