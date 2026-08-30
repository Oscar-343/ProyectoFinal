using System.ComponentModel.DataAnnotations;
using ProyectoFinal.Models;

namespace ProyectoFinal.ViewModels
{
    // Datos utilizados para editar un pedido.
    public class PedidoEditViewModel
    {
        public int IdPedido { get; set; }

        [Required(ErrorMessage = "El cliente es obligatorio.")]
        public string Cliente { get; set; } = string.Empty;

        public DateTime FechaInicio { get; set; }

        public EstadoPedido Estado { get; set; }

        // Modelos disponibles para seleccionar en el pedido.
        public List<ModeloSeleccionViewModel> ModelosDisponibles { get; set; } = new();
    }

    // Datos de un modelo disponible para el pedido.
    public class ModeloSeleccionViewModel
    {
        public int IdModelo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Imagen { get; set; }
        public decimal TiempoProduccion { get; set; }
        public decimal PrecioVenta { get; set; }
        public int Cantidad { get; set; }
    }
}