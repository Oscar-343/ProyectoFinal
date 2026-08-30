// ViewModels/PedidoPersonalizadoEditViewModel.cs
using Microsoft.AspNetCore.Http;
using ProyectoFinal.ViewModels;

namespace ProyectoFinal.Models
{
    public class PedidoPersonalizadoEditViewModel
    {
        public int IdPedidoPersonalizado { get; set; }

        public string Cliente { get; set; } = string.Empty;
        public string NombreReferencia { get; set; } = string.Empty;
        public string? Descripcion { get; set; }

        // Ruta de la imagen que ya tiene guardada (para mostrarla en el formulario).
        public string? ImagenActual { get; set; }

        // Si el usuario sube una nueva, reemplaza a la actual.
        public IFormFile? ImagenArchivo { get; set; }

        public string Dificultad { get; set; } = "Media";
        public decimal TiempoProduccion { get; set; }

        public DateTime FechaInicio { get; set; }
        public EstadoPedido Estado { get; set; }

        public List<MaterialSeleccionViewModel> MaterialesDisponibles { get; set; } = new();
    }
}