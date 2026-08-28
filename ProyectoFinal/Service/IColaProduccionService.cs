namespace ProyectoFinal.Services
{
    public interface IColaProduccionService
    {
        // Calcula la fecha de inicio real (respetando la cola de producción y el horario laboral)
        (DateTime fechaInicioReal, DateTime fechaEntrega) CalcularFechasPedido(DateTime fechaInicioDeseada, decimal horasTotales);

        // Fecha/hora en la que se libera la línea de producción (fin del último pedido en cola).
        DateTime ObtenerFechaFinCola();
    }
}