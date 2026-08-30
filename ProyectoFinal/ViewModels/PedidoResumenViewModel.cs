using ProyectoFinal.Models;

namespace ProyectoFinal.ViewModels
{
    // Representa una fila de la lista combinada de pedidos (normales + personalizados).
    public class PedidoResumenViewModel
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty; // "Normal" o "Personalizado"
        public string Cliente { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty; // modelos, o nombre de referencia
        public DateTime FechaPedido { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaEntrega { get; set; }
        public EstadoPedido Estado { get; set; }
        public decimal Total { get; set; }
    }
}