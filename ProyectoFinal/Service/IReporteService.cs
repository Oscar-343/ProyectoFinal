using ProyectoFinal.Dto;

namespace ProyectoFinal.Service
{
    public interface IReporteService
    {
        Task<ReporteDto> GenerarReporteAsync(DateTime desde, DateTime hasta);
    }
}