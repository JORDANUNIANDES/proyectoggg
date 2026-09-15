namespace RestaurantManagement.API.Models
{
    public enum PedidoEstado
    {
        Pendiente = 0,
        EnPreparacion = 1,
        Completado = 2,
        Cancelado = 3
    }

    public enum MesaEstado
    {
        Disponible = 0,
        Ocupada = 1,
        Reservada = 2
    }
}
