using Microsoft.AspNetCore.Http;

namespace ProyectoFinal.ViewModels
{
    public class PedidoPersonalizadoCreateViewModel
    {
        public string Cliente { get; set; } = string.Empty;
        public string NombreReferencia { get; set; } = string.Empty;
        public string? Descripcion { get; set; }

        // Archivo subido por drag & drop desde el formulario.
        public IFormFile? ImagenArchivo { get; set; }

        public string Dificultad { get; set; } = "Media";
        public decimal TiempoProduccion { get; set; }

        public DateTime FechaInicio { get; set; } = DateTime.Now;
        public DateTime FechaInicioSugerida { get; set; }

        public List<MaterialSeleccionViewModel> MaterialesDisponibles { get; set; } = new();
    }

    public class MaterialSeleccionViewModel
    {
        public int IdMaterial { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Color { get; set; }
        public string UnidadMedida { get; set; } = string.Empty;
        public decimal PrecioUnitario { get; set; }
        public decimal Cantidad { get; set; }
    }
}