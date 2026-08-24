namespace ProyectoFinal.Models
{
    public class HomeDashboardViewModel
    {
        public int TotalModelos { get; set; }
        public int TotalMateriales { get; set; }
        public List<material> MaterialesPorAcabarse { get; set; } = new();

        // TODO: reemplazar cuando exista la tabla de ventas/pedidos.
        // Por ahora se deja vacío para mostrar el estado "próximamente".
        public List<Modelo> ModelosMasVendidos { get; set; } = new();
    }
}