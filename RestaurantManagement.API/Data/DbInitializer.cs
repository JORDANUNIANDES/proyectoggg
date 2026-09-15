using System;
using System.Collections.Generic;
using System.Linq;
using RestaurantManagement.API.Models;

namespace RestaurantManagement.API.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Clientes.Any())
            {
                return; // DB ya ha sido inicializada
            }

            var clientes = new Cliente[]
            {
                new Cliente { Nombre = "Juan", Apellido = "Pérez", Cedula = "1710034065", Telefono = "0991234567", Email = "juan.perez@email.com", Direccion = "Av. Amazonas y Colón", FechaRegistro = DateTime.Now.AddDays(-30), Estado = true },
                new Cliente { Nombre = "Maria", Apellido = "Gómez", Cedula = "1713394631", Telefono = "0987654321", Email = "maria.gomez@email.com", Direccion = "Calle Larga y Benigno Malo", FechaRegistro = DateTime.Now.AddDays(-20), Estado = true },
                new Cliente { Nombre = "Carlos", Apellido = "López", Cedula = "0910000009", Telefono = "0998877665", Email = "carlos.lopez@email.com", Direccion = "Av. 10 de Agosto", FechaRegistro = DateTime.Now.AddDays(-10), Estado = true }
            };

            context.Clientes.AddRange(clientes);
            context.SaveChanges();

            var platos = new Plato[]
            {
                new Plato { Nombre = "Lomo Saltado", Descripcion = "Trozos de lomo con cebolla, tomate y papas fritas", Precio = 12.50m, Categoria = "Platos Fuertes", Disponible = true, FechaRegistro = DateTime.Now.AddDays(-30) },
                new Plato { Nombre = "Ceviche Mixto", Descripcion = "Mariscos frescos marinados en jugo de limón", Precio = 14.00m, Categoria = "Mariscos", Disponible = true, FechaRegistro = DateTime.Now.AddDays(-30) },
                new Plato { Nombre = "Sopa de Mariscos", Descripcion = "Sopa concentrada con mariscos variados", Precio = 10.00m, Categoria = "Sopas", Disponible = true, FechaRegistro = DateTime.Now.AddDays(-30) },
                new Plato { Nombre = "Jarra de Limonada", Descripcion = "Limonada natural 1 Litro", Precio = 4.50m, Categoria = "Bebidas", Disponible = true, FechaRegistro = DateTime.Now.AddDays(-30) },
                new Plato { Nombre = "Flan de la Casa", Descripcion = "Postre tradicional de caramelo", Precio = 3.50m, Categoria = "Postres", Disponible = true, FechaRegistro = DateTime.Now.AddDays(-30) }
            };

            context.Platos.AddRange(platos);
            context.SaveChanges();

            var mesas = new Mesa[]
            {
                new Mesa { Numero = 1, Capacidad = 2, Ubicacion = "Interior", Estado = "Ocupada" },
                new Mesa { Numero = 2, Capacidad = 4, Ubicacion = "Interior", Estado = "Disponible" },
                new Mesa { Numero = 3, Capacidad = 6, Ubicacion = "Terraza", Estado = "Disponible" },
                new Mesa { Numero = 4, Capacidad = 2, Ubicacion = "Terraza", Estado = "Reservada" },
                new Mesa { Numero = 5, Capacidad = 8, Ubicacion = "VIP", Estado = "Disponible" }
            };

            context.Mesas.AddRange(mesas);
            context.SaveChanges();

            var pedido1 = new Pedido
            {
                ClienteId = clientes[0].Id,
                MesaId = mesas[0].Id,
                FechaPedido = DateTime.Now.AddHours(-2),
                Estado = "En preparación",
                Subtotal = 26.50m,
                Total = 26.50m,
                Observaciones = "Sin cebolla en el lomo saltado",
                Detalles = new List<DetallePedido>
                {
                    new DetallePedido { PlatoId = platos[0].Id, Cantidad = 1, PrecioUnitario = 12.50m, Subtotal = 12.50m },
                    new DetallePedido { PlatoId = platos[1].Id, Cantidad = 1, PrecioUnitario = 14.00m, Subtotal = 14.00m }
                }
            };

            var pedido2 = new Pedido
            {
                ClienteId = clientes[1].Id,
                MesaId = mesas[1].Id,
                FechaPedido = DateTime.Now.AddDays(-1),
                Estado = "Pagado",
                Subtotal = 14.50m,
                Total = 14.50m,
                Observaciones = "Para llevar",
                Detalles = new List<DetallePedido>
                {
                    new DetallePedido { PlatoId = platos[2].Id, Cantidad = 1, PrecioUnitario = 10.00m, Subtotal = 10.00m },
                    new DetallePedido { PlatoId = platos[3].Id, Cantidad = 1, PrecioUnitario = 4.50m, Subtotal = 4.50m }
                }
            };

            context.Pedidos.AddRange(pedido1, pedido2);
            context.SaveChanges();
        }
    }
}
