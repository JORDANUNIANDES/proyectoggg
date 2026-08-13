using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MuscleHouse.Models;

namespace MuscleHouse.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // 1. Roles seeding
            string[] roles = { "Administrador", "Recepcionista", "Entrenador", "Usuario" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Core Users seeding
            var adminUser = await SeedUserAsync(userManager, "admin@musclehouse.com", "MuscleHouse123!", "Administrador");
            var recepUser = await SeedUserAsync(userManager, "recepcionista@musclehouse.com", "MuscleHouse123!", "Recepcionista");
            var trainerUser1 = await SeedUserAsync(userManager, "entrenador@musclehouse.com", "MuscleHouse123!", "Entrenador");
            var trainerUser2 = await SeedUserAsync(userManager, "entrenador2@musclehouse.com", "MuscleHouse123!", "Entrenador");
            var clientUser1 = await SeedUserAsync(userManager, "cliente@musclehouse.com", "MuscleHouse123!", "Usuario");
            var clientUser2 = await SeedUserAsync(userManager, "cliente2@musclehouse.com", "MuscleHouse123!", "Usuario");

            // Save basic changes
            await context.SaveChangesAsync();

            // 3. Recepcionistas entities
            var recep = await context.Recepcionistas.FirstOrDefaultAsync(r => r.UserId == recepUser.Id);
            if (recep == null)
            {
                recep = new Recepcionista
                {
                    UserId = recepUser.Id,
                    Nombre = "Ana",
                    Apellido = "Martínez",
                    Descripcion = "Atención al cliente y soporte administrativo de MUSCLE HOUSE.",
                    HorarioAtencion = "Lunes a Viernes de 08:00 a 16:00",
                    Fotografia = "uploads/default-avatar.png",
                    Activo = true
                };
                context.Recepcionistas.Add(recep);
            }

            // 4. Entrenadores entities
            var trainer1 = await context.Entrenadores.FirstOrDefaultAsync(e => e.UserId == trainerUser1.Id);
            if (trainer1 == null)
            {
                trainer1 = new Entrenador
                {
                    UserId = trainerUser1.Id,
                    Nombre = "Carlos",
                    Apellido = "Rojas",
                    Especialidad = "Culturismo y Fuerza",
                    Experiencia = 7,
                    Descripcion = "Especialista en sobrecarga progresiva y desarrollo muscular.",
                    Fotografia = "uploads/default-avatar.png",
                    Activo = true
                };
                context.Entrenadores.Add(trainer1);
            }

            var trainer2 = await context.Entrenadores.FirstOrDefaultAsync(e => e.UserId == trainerUser2.Id);
            if (trainer2 == null)
            {
                trainer2 = new Entrenador
                {
                    UserId = trainerUser2.Id,
                    Nombre = "María",
                    Apellido = "Gómez",
                    Especialidad = "Acondicionamiento Físico y Pérdida de Peso",
                    Experiencia = 5,
                    Descripcion = "Experta en rutinas metabólicas de alta intensidad.",
                    Fotografia = "uploads/default-avatar.png",
                    Activo = true
                };
                context.Entrenadores.Add(trainer2);
            }

            // Save so we have TrainerIds
            await context.SaveChangesAsync();

            // 5. Clientes entities
            var client1 = await context.Clientes.FirstOrDefaultAsync(c => c.UserId == clientUser1.Id);
            if (client1 == null)
            {
                client1 = new Cliente
                {
                    UserId = clientUser1.Id,
                    Nombre = "Juan",
                    Apellido = "Pérez",
                    Telefono = "555-0199",
                    FechaNacimiento = new DateTime(1993, 8, 20),
                    Objetivo = "Aumento de masa muscular",
                    EntrenadorId = trainer1.Id,
                    Activo = true
                };
                context.Clientes.Add(client1);
            }

            var client2 = await context.Clientes.FirstOrDefaultAsync(c => c.UserId == clientUser2.Id);
            if (client2 == null)
            {
                client2 = new Cliente
                {
                    UserId = clientUser2.Id,
                    Nombre = "Sofía",
                    Apellido = "Rodríguez",
                    Telefono = "555-0188",
                    FechaNacimiento = new DateTime(1997, 4, 12),
                    Objetivo = "Tonificación y resistencia",
                    EntrenadorId = trainer2.Id,
                    Activo = true
                };
                context.Clientes.Add(client2);
            }

            await context.SaveChangesAsync();

            // 6. AsignacionEntrenador history
            var ae1 = await context.AsignacionesEntrenadores.FirstOrDefaultAsync(ae => ae.ClienteId == client1.Id && ae.EntrenadorId == trainer1.Id);
            if (ae1 == null)
            {
                context.AsignacionesEntrenadores.Add(new AsignacionEntrenador
                {
                    ClienteId = client1.Id,
                    EntrenadorId = trainer1.Id,
                    FechaInicio = DateTime.Now.AddDays(-30),
                    FechaFin = null
                });
            }

            var ae2 = await context.AsignacionesEntrenadores.FirstOrDefaultAsync(ae => ae.ClienteId == client2.Id && ae.EntrenadorId == trainer2.Id);
            if (ae2 == null)
            {
                context.AsignacionesEntrenadores.Add(new AsignacionEntrenador
                {
                    ClienteId = client2.Id,
                    EntrenadorId = trainer2.Id,
                    FechaInicio = DateTime.Now.AddDays(-15),
                    FechaFin = null
                });
            }

            // 7. Planes Seed
            var planesSeed = new List<(string Nombre, int Dias, decimal Precio, string Desc)>
            {
                ("Diario", 1, 5.00m, "Acceso total por 1 día a todas las áreas del gimnasio."),
                ("Mensual", 30, 45.00m, "Acceso mensual completo con derecho a asesoría básica."),
                ("Trimestral", 90, 99.00m, "Plan trimestral ideal para consolidar el hábito de entrenamiento."),
                ("Semestral", 180, 160.00m, "Plan semestral con descuento y acceso a clases especiales."),
                ("Anual", 365, 260.00m, "La mejor inversión. Acceso anual completo para un desarrollo definitivo.")
            };

            foreach (var pInfo in planesSeed)
            {
                var plan = await context.Planes.FirstOrDefaultAsync(p => p.Nombre == pInfo.Nombre);
                if (plan == null)
                {
                    context.Planes.Add(new Plan
                    {
                        Nombre = pInfo.Nombre,
                        DuracionDias = pInfo.Dias,
                        Precio = pInfo.Precio,
                        Descripcion = pInfo.Desc
                    });
                }
            }

            await context.SaveChangesAsync();

            // Resolve plan objects for memberships
            var planMensual = await context.Planes.FirstAsync(p => p.Nombre == "Mensual");
            var planAnual = await context.Planes.FirstAsync(p => p.Nombre == "Anual");

            // 8. Membresias seed (Historical + Active)
            var memb1 = await context.Membresias.FirstOrDefaultAsync(m => m.ClienteId == client1.Id);
            if (memb1 == null)
            {
                // Historical expired membership
                var oldMemb = new Membresia
                {
                    ClienteId = client1.Id,
                    PlanId = planMensual.Id,
                    FechaInicio = DateTime.Now.AddDays(-60),
                    FechaVencimiento = DateTime.Now.AddDays(-30),
                    PrecioPagado = planMensual.Precio,
                    Estado = "Vencida"
                };
                context.Membresias.Add(oldMemb);

                // Active membership
                memb1 = new Membresia
                {
                    ClienteId = client1.Id,
                    PlanId = planMensual.Id,
                    FechaInicio = DateTime.Now.AddDays(-30),
                    FechaVencimiento = DateTime.Now.AddDays(30),
                    PrecioPagado = planMensual.Precio,
                    Estado = "Activa"
                };
                context.Membresias.Add(memb1);
            }

            var memb2 = await context.Membresias.FirstOrDefaultAsync(m => m.ClienteId == client2.Id);
            if (memb2 == null)
            {
                memb2 = new Membresia
                {
                    ClienteId = client2.Id,
                    PlanId = planAnual.Id,
                    FechaInicio = DateTime.Now.AddDays(-15),
                    FechaVencimiento = DateTime.Now.AddDays(350),
                    PrecioPagado = planAnual.Precio,
                    Estado = "Activa"
                };
                context.Membresias.Add(memb2);
            }

            await context.SaveChangesAsync();

            // 9. Pagos seed
            // Ensure every membership has a payment generated
            var activeMembresias = await context.Membresias.ToListAsync();
            foreach (var m in activeMembresias)
            {
                var pago = await context.Pagos.FirstOrDefaultAsync(p => p.MembresiaId == m.Id);
                if (pago == null)
                {
                    context.Pagos.Add(new Pago
                    {
                        ClienteId = m.ClienteId,
                        MembresiaId = m.Id,
                        Monto = m.PrecioPagado,
                        Fecha = m.FechaInicio,
                        MetodoPago = m.PlanId == planAnual.Id ? "Tarjeta" : "Efectivo",
                        Estado = "Completado"
                    });
                }
            }

            // 10. Asistencias seed
            if (!await context.Asistencias.AnyAsync(a => a.ClienteId == client1.Id))
            {
                context.Asistencias.AddRange(new List<Asistencia>
                {
                    new Asistencia { ClienteId = client1.Id, FechaHora = DateTime.Now.AddDays(-5).AddHours(8) },
                    new Asistencia { ClienteId = client1.Id, FechaHora = DateTime.Now.AddDays(-3).AddHours(9) },
                    new Asistencia { ClienteId = client1.Id, FechaHora = DateTime.Now.AddDays(-1).AddHours(8) }
                });
            }

            if (!await context.Asistencias.AnyAsync(a => a.ClienteId == client2.Id))
            {
                context.Asistencias.AddRange(new List<Asistencia>
                {
                    new Asistencia { ClienteId = client2.Id, FechaHora = DateTime.Now.AddDays(-4).AddHours(18) },
                    new Asistencia { ClienteId = client2.Id, FechaHora = DateTime.Now.AddDays(-2).AddHours(17) },
                    new Asistencia { ClienteId = client2.Id, FechaHora = DateTime.Now.AddDays(-1).AddHours(19) }
                });
            }

            // 11. Ejercicios seed
            var ejerciciosSeed = new List<(string Nombre, string Grupo, string Desc, string Inst)>
            {
                ("Sentadilla con Barra", "Piernas", "Ejercicio compuesto para cuádriceps, glúteos y core.", "Coloca la barra sobre los trapecios, desciende flexionando rodillas hasta 90 grados y sube manteniendo la espalda recta."),
                ("Press de Banca", "Pecho", "Ejercicio fundamental de empuje para pectoral mayor, tríceps y hombro anterior.", "Acuéstate boca arriba en el banco, baja la barra al pecho controlado y empuja con potencia hacia arriba."),
                ("Peso Muerto", "Espalda", "Ejercicio de fuerza de cadena posterior, femorales y lumbar.", "Mantén la barra pegada a las espinillas, flexiona caderas y rodillas, levanta la carga extendiendo caderas."),
                ("Dominadas", "Espalda", "Ejercicio de tracción con peso corporal para dorsales y bíceps.", "Sujétate de la barra de dominadas con agarre prono y elévate hasta que la barbilla pase la barra."),
                ("Press Militar", "Hombros", "Ejercicio vertical de empuje para deltoides anteriores e internos.", "De pie o sentado, empuja la barra verticalmente desde los hombros hasta la extensión completa de los brazos.")
            };

            foreach (var ejInfo in ejerciciosSeed)
            {
                var ej = await context.Ejercicios.FirstOrDefaultAsync(e => e.Nombre == ejInfo.Nombre);
                if (ej == null)
                {
                    context.Ejercicios.Add(new Ejercicio
                    {
                        Nombre = ejInfo.Nombre,
                        GrupoMuscular = ejInfo.Grupo,
                        Descripcion = ejInfo.Desc,
                        Instrucciones = ejInfo.Inst
                    });
                }
            }

            await context.SaveChangesAsync();

            // Resolve exercises
            var exSquat = await context.Ejercicios.FirstAsync(e => e.Nombre == "Sentadilla con Barra");
            var exBench = await context.Ejercicios.FirstAsync(e => e.Nombre == "Press de Banca");
            var exDeadlift = await context.Ejercicios.FirstAsync(e => e.Nombre == "Peso Muerto");
            var exOverhead = await context.Ejercicios.FirstAsync(e => e.Nombre == "Press Militar");

            // 12. Rutinas seed
            var rut1 = await context.Rutinas.FirstOrDefaultAsync(r => r.ClienteId == client1.Id && r.Nombre == "Rutina Fuerza Muscle House");
            if (rut1 == null)
            {
                rut1 = new Rutina
                {
                    ClienteId = client1.Id,
                    EntrenadorId = trainer1.Id,
                    Nombre = "Rutina Fuerza Muscle House",
                    FechaCreacion = DateTime.Now.AddDays(-28),
                    Activa = true
                };
                context.Rutinas.Add(rut1);
                await context.SaveChangesAsync(); // get Id

                // RutinaEjercicio
                context.RutinaEjercicios.AddRange(new List<RutinaEjercicio>
                {
                    new RutinaEjercicio { RutinaId = rut1.Id, EjercicioId = exSquat.Id, Series = 4, Repeticiones = 5, PesoRecomendado = 80.00m, DescansoSegundos = 180, Orden = 1, Observaciones = "Enfocarse en rango de movimiento completo." },
                    new RutinaEjercicio { RutinaId = rut1.Id, EjercicioId = exBench.Id, Series = 4, Repeticiones = 5, PesoRecomendado = 60.00m, DescansoSegundos = 150, Orden = 2, Observaciones = "Mantener los codos metidos a 45 grados." },
                    new RutinaEjercicio { RutinaId = rut1.Id, EjercicioId = exDeadlift.Id, Series = 3, Repeticiones = 5, PesoRecomendado = 100.00m, DescansoSegundos = 180, Orden = 3, Observaciones = "No redondear la espalda lumbar." }
                });
            }

            var rut2 = await context.Rutinas.FirstOrDefaultAsync(r => r.ClienteId == client2.Id && r.Nombre == "Rutina Tonificación Muscle House");
            if (rut2 == null)
            {
                rut2 = new Rutina
                {
                    ClienteId = client2.Id,
                    EntrenadorId = trainer2.Id,
                    Nombre = "Rutina Tonificación Muscle House",
                    FechaCreacion = DateTime.Now.AddDays(-14),
                    Activa = true
                };
                context.Rutinas.Add(rut2);
                await context.SaveChangesAsync();

                context.RutinaEjercicios.AddRange(new List<RutinaEjercicio>
                {
                    new RutinaEjercicio { RutinaId = rut2.Id, EjercicioId = exSquat.Id, Series = 3, Repeticiones = 12, PesoRecomendado = 30.00m, DescansoSegundos = 60, Orden = 1, Observaciones = "Controlar el descenso." },
                    new RutinaEjercicio { RutinaId = rut2.Id, EjercicioId = exOverhead.Id, Series = 3, Repeticiones = 10, PesoRecomendado = 15.00m, DescansoSegundos = 60, Orden = 2, Observaciones = "Mantener el abdomen contraído." }
                });
            }

            // 13. Progresos seed (Historical physically progressive metrics)
            if (!await context.Progresos.AnyAsync(p => p.ClienteId == client1.Id))
            {
                context.Progresos.AddRange(new List<Progreso>
                {
                    new Progreso
                    {
                        ClienteId = client1.Id,
                        Fecha = DateTime.Now.AddDays(-30),
                        Peso = 78.50m,
                        Pecho = 98.00m,
                        Cintura = 84.00m,
                        Brazo = 34.50m,
                        Pierna = 56.00m,
                        Cadera = 96.00m,
                        Observaciones = "Estado inicial antes de comenzar el plan de aumento muscular."
                    },
                    new Progreso
                    {
                        ClienteId = client1.Id,
                        Fecha = DateTime.Now.AddDays(-15),
                        Peso = 79.20m,
                        Pecho = 99.50m,
                        Cintura = 83.50m,
                        Brazo = 35.00m,
                        Pierna = 56.80m,
                        Cadera = 96.00m,
                        Observaciones = "Buen progreso en hombros y pecho. Cintura disminuyendo levemente."
                    },
                    new Progreso
                    {
                        ClienteId = client1.Id,
                        Fecha = DateTime.Now,
                        Peso = 80.10m,
                        Pecho = 101.00m,
                        Cintura = 83.00m,
                        Brazo = 35.80m,
                        Pierna = 57.50m,
                        Cadera = 96.20m,
                        Observaciones = "Aumento notable de masa magra y fuerza."
                    }
                });
            }

            if (!await context.Progresos.AnyAsync(p => p.ClienteId == client2.Id))
            {
                context.Progresos.AddRange(new List<Progreso>
                {
                    new Progreso
                    {
                        ClienteId = client2.Id,
                        Fecha = DateTime.Now.AddDays(-15),
                        Peso = 62.00m,
                        Pecho = 90.00m,
                        Cintura = 72.00m,
                        Brazo = 28.00m,
                        Pierna = 52.00m,
                        Cadera = 98.00m,
                        Observaciones = "Inicio de acondicionamiento metabólico."
                    },
                    new Progreso
                    {
                        ClienteId = client2.Id,
                        Fecha = DateTime.Now,
                        Peso = 61.20m,
                        Pecho = 89.50m,
                        Cintura = 70.00m,
                        Brazo = 28.20m,
                        Pierna = 51.50m,
                        Cadera = 96.80m,
                        Observaciones = "Reducción de cintura y cadera. Excelente asimilación de la carga."
                    }
                });
            }

            // 14. RegistroEntrenamiento seed (Progressive overload logs)
            if (!await context.RegistrosEntrenamiento.AnyAsync(re => re.ClienteId == client1.Id))
            {
                context.RegistrosEntrenamiento.AddRange(new List<RegistroEntrenamiento>
                {
                    new RegistroEntrenamiento
                    {
                        ClienteId = client1.Id,
                        EjercicioId = exSquat.Id,
                        Fecha = DateTime.Now.AddDays(-12),
                        Series = 4,
                        Repeticiones = 5,
                        Peso = 80.00m,
                        RPE = 8,
                        Observaciones = "Series sólidas con buena profundidad."
                    },
                    new RegistroEntrenamiento
                    {
                        ClienteId = client1.Id,
                        EjercicioId = exSquat.Id,
                        Fecha = DateTime.Now.AddDays(-5),
                        Series = 4,
                        Repeticiones = 5,
                        Peso = 82.50m,
                        RPE = 9,
                        Observaciones = "Sobrecarga progresiva exitosa. Repetición final desafiante."
                    },
                    new RegistroEntrenamiento
                    {
                        ClienteId = client1.Id,
                        EjercicioId = exBench.Id,
                        Fecha = DateTime.Now.AddDays(-10),
                        Series = 4,
                        Repeticiones = 5,
                        Peso = 60.00m,
                        RPE = 7,
                        Observaciones = "Cómodo en el banco."
                    },
                    new RegistroEntrenamiento
                    {
                        ClienteId = client1.Id,
                        EjercicioId = exBench.Id,
                        Fecha = DateTime.Now.AddDays(-3),
                        Series = 4,
                        Repeticiones = 5,
                        Peso = 62.50m,
                        RPE = 8,
                        Observaciones = "Fuerza de empuje aumentando sólidamente."
                    }
                });
            }

            // 15. Notificaciones seed
            if (!await context.Notificaciones.AnyAsync(n => n.UserId == clientUser1.Id))
            {
                context.Notificaciones.AddRange(new List<Notificacion>
                {
                    new Notificacion
                    {
                        UserId = clientUser1.Id,
                        Titulo = "¡Bienvenido a MUSCLE HOUSE!",
                        Mensaje = "Tu membresía mensual activa ha sido configurada con éxito. ¡A darlo todo en cada entrenamiento!",
                        Fecha = DateTime.Now.AddDays(-30),
                        Leida = true
                    },
                    new Notificacion
                    {
                        UserId = clientUser1.Id,
                        Titulo = "Nueva Rutina Asignada",
                        Mensaje = "Tu entrenador Carlos Rojas te ha asignado la rutina 'Rutina Fuerza Muscle House'. Revísala en tu perfil.",
                        Fecha = DateTime.Now.AddDays(-28),
                        Leida = false
                    }
                });
            }

            if (!await context.Notificaciones.AnyAsync(n => n.UserId == clientUser2.Id))
            {
                context.Notificaciones.AddRange(new List<Notificacion>
                {
                    new Notificacion
                    {
                        UserId = clientUser2.Id,
                        Titulo = "¡Bienvenida a MUSCLE HOUSE!",
                        Mensaje = "Disfruta de tu membresía anual. Solicita tu asesoría inicial con tu entrenadora Gómez.",
                        Fecha = DateTime.Now.AddDays(-15),
                        Leida = false
                    }
                });
            }

            await context.SaveChangesAsync();
        }

        private static async Task<ApplicationUser> SeedUserAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    throw new Exception($"Fallo al crear usuario semilla {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }

            return user;
        }
    }
}
