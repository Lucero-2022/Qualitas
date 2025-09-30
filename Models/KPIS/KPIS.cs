namespace Qualitas.Models.KPIS
{
    public class KPIS
    {
        public int TotalReservas { get; set; }
        public int TotalReservasMesAnterior { get; set; }

        public decimal SumaAjustes { get; set; }
        public decimal SumaAjustesMesAnterior { get; set; }

        public decimal MaximoHoy { get; set; }
        public decimal MaximoMesAnterior { get; set; }

        public decimal SumaMesActual { get; set; }
        public decimal SumaMesAnterior { get; set; }

        // Variaciones calculadas
        public decimal VariacionReservas => CalcularVariacion(TotalReservas, TotalReservasMesAnterior);
        public decimal VariacionAjustes => CalcularVariacion(SumaAjustes, SumaAjustesMesAnterior);
        public decimal VariacionMaximo => CalcularVariacion(MaximoHoy, MaximoMesAnterior);
        public decimal VariacionSumaMes => CalcularVariacion(SumaMesActual, SumaMesAnterior);

        private decimal CalcularVariacion(decimal actual, decimal anterior)
        {
            if (anterior == 0) return 0;
            return ((actual - anterior) / anterior) * 100;
        }
    }
}
