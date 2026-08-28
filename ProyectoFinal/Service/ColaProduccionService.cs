using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Data;
using ProyectoFinal.Models;

namespace ProyectoFinal.Services
{
    // Calcula las fechas de producción de los pedidos.
    public class ColaProduccionService : IColaProduccionService
    {
        private readonly TiendaDbContext _context;

        private static readonly TimeSpan HORA_INICIO = new TimeSpan(8, 0, 0);
        private static readonly TimeSpan HORA_FIN = new TimeSpan(20, 0, 0);

        public ColaProduccionService(TiendaDbContext context)
        {
            _context = context;
        }

        // Obtiene la fecha de finalización del último pedido en cola.
        public DateTime ObtenerFechaFinCola()
        {
            var ultimo = _context.Pedido
                .Where(p => p.Estado == EstadoPedido.Pendiente ||
                            p.Estado == EstadoPedido.EnProduccion)
                .OrderByDescending(p => p.FechaEntrega)
                .FirstOrDefault();

            return ultimo?.FechaEntrega ?? DateTime.Now;
        }

        // Calcula la fecha real de inicio y la fecha de entrega.
        public (DateTime fechaInicioReal, DateTime fechaEntrega)
            CalcularFechasPedido(
                DateTime fechaInicioDeseada,
                decimal horasTotales)
        {
            var finCola = ObtenerFechaFinCola();

            // El pedido inicia cuando la línea de producción esté disponible.
            var inicioReal = fechaInicioDeseada > finCola
                ? fechaInicioDeseada
                : finCola;

            inicioReal = AjustarAHorarioLaboral(inicioReal);

            var entrega = CalcularFechaEntrega(
                inicioReal,
                horasTotales);

            return (inicioReal, entrega);
        }

        // Calcula la fecha de entrega según las horas de producción.
        private DateTime CalcularFechaEntrega(
            DateTime fechaInicio,
            decimal horasTotales)
        {
            var actual = AjustarAHorarioLaboral(fechaInicio);
            var horasRestantes = horasTotales;

            while (horasRestantes > 0)
            {
                var horasDisponiblesHoy =
                    (decimal)(HORA_FIN - actual.TimeOfDay).TotalHours;

                if (horasRestantes <= horasDisponiblesHoy)
                {
                    actual = actual.AddHours((double)horasRestantes);
                    horasRestantes = 0;
                }
                else
                {
                    horasRestantes -= horasDisponiblesHoy;

                    actual = SiguienteDiaLaboral(actual.Date)
                        .Add(HORA_INICIO);
                }
            }

            return actual;
        }

        // Ajusta una fecha al horario de producción.
        private DateTime AjustarAHorarioLaboral(DateTime fecha)
        {
            if (fecha.DayOfWeek == DayOfWeek.Sunday)
                return SiguienteDiaLaboral(fecha.Date)
                    .Add(HORA_INICIO);

            if (fecha.TimeOfDay < HORA_INICIO)
                return fecha.Date.Add(HORA_INICIO);

            if (fecha.TimeOfDay >= HORA_FIN)
                return SiguienteDiaLaboral(fecha.Date)
                    .Add(HORA_INICIO);

            return fecha;
        }

        // Obtiene el siguiente día disponible para producción.
        private DateTime SiguienteDiaLaboral(DateTime fecha)
        {
            var siguiente = fecha.AddDays(1);

            while (siguiente.DayOfWeek == DayOfWeek.Sunday)
                siguiente = siguiente.AddDays(1);

            return siguiente;
        }
    }
}