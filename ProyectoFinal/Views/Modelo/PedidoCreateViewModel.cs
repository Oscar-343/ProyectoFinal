namespace ProyectoFinal.ViewModels
{
    public class PedidoCreateViewModel
    {
        public string Cliente { get; set; } = string.Empty;

        // Fecha que el usuario propone como inicio.
        public DateTime FechaInicio { get; set; } = DateTime.Now;

        // Fecha sugerida por el sistema (fin de la cola actual), se muestra como referencia.
        public DateTime FechaInicioSugerida { get; set; }

        public List<PedidoDetalleViewModel> Detalles { get; set; } = new();

        public decimal HorasProduccionTotal { get; set; }
        public decimal PrecioVentaTotal { get; set; }
    }

    public class PedidoDetalleViewModel
    {
        public int IdModelo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Imagen { get; set; }
        public int Cantidad { get; set; }
        public decimal TiempoProduccion { get; set; }
        public decimal PrecioVenta { get; set; }

        public decimal Subtotal => Cantidad * PrecioVenta;
    }
}