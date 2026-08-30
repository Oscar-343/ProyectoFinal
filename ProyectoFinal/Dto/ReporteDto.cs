namespace ProyectoFinal.Dto
{
    public class ReporteDto
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public string? ModeloMasVendido { get; set; }
        public int CantidadMasVendido { get; set; }
        public string? ModeloMenosVendido { get; set; }
        public int CantidadMenosVendido { get; set; }
        public string? ModeloMasRentable { get; set; }
        public decimal UtilidadModeloMasRentable { get; set; }

        public decimal IngresosTotales { get; set; }
        public decimal GastoMateriales { get; set; }
        public decimal UtilidadTotal { get; set; }
        public decimal MargenUtilidad { get; set; }

        public decimal HorasTrabajadas { get; set; }
        public decimal UtilidadPorHora { get; set; }
        public int PedidosRealizados { get; set; }
        public decimal TicketPromedio { get; set; }

        public decimal UtilidadPeriodoAnterior { get; set; }
        public decimal VariacionUtilidadPorcentaje { get; set; }

        // NUEVO: detalle de cada pedido del rango, para la tabla del Excel
        public List<PedidoReporteItem> Pedidos { get; set; } = new();
    }

    public class PedidoReporteItem
    {
        public int IdPedido { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public DateTime FechaPedido { get; set; }
        public DateTime FechaEntrega { get; set; }
        public string Estado { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public decimal Horas { get; set; }
        public string Tipo { get; set; } = "Normal"; // "Normal" o "Personalizado"
    }
}