namespace ProyectoFinal.Models
{
    // ViewModel que agrupa los datos que se muestran en el dashboard principal (Home).
    public class HomeDashboardViewModel
    {
        // Cantidad total de modelos registrados, se calcula en el controlador.
        public int TotalModelos { get; set; }
        // Cantidad total de materiales registrados, se calcula en el controlador.
        public int TotalMateriales { get; set; }
        // Lista de materiales cuyo stock está por acabarse (bajo el mínimo), la llena el controlador.
        public List<material> MaterialesPorAcabarse { get; set; } = new();
        public List<Modelo> ModelosMasVendidos { get; set; } = new();
    }
}