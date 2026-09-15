# RestaurantManagement — Sistema de Gestión de Restaurantes

## 1. Nombre del Proyecto
**RestaurantManagement — Sistema de Gestión de Restaurantes**

---

## 2. Descripción
**RestaurantManagement** es una aplicación web académica integral diseñada para automatizar y gestionar las operaciones esenciales de un establecimiento gastronómico. Permite administrar clientes, la carta de platos, las mesas del restaurante, el registro e historial de pedidos y la generación de reportes consolidados por cliente.

El sistema resuelve problemas comunes en la gestión de restaurantes como:
* Errores en la toma de pedidos y cálculo erróneo de cuentas.
* Pérdida o alteración no autorizada de datos históricos de ventas.
* Dificultad para controlar el estado y la disponibilidad en tiempo real de las mesas.
* Inconsistencias en el registro de clientes e identificación fiscal/documental.

---

## 3. Tecnologías Utilizadas

### Backend
* **Visual Studio Community** (IDE Principal)
* **.NET 10** & **ASP.NET Core Web API**
* **C#**
* **Entity Framework Core 10** (Code First)
* **SQL Server** (con soporte en desarrollo para LocalDB y SQLite)

### Frontend
* **HTML5** (Semántico)
* **CSS3** (Diseño moderno responsive, tema oscuro con acentos cálidos terracota, alto contraste y reglas de impresión)
* **JavaScript Vanilla** (Fetch API para comunicación RESTful asynchronous, sin frameworks externos)

### Pruebas Unitarias
* **xUnit Test Framework**
* **Entity Framework Core In-Memory Database** (para aislamiento de pruebas unitarias)

---

## 4. Arquitectura del Proyecto
La solución implementa una arquitectura limpia y desacoplada pero sencilla, apropiada para un proyecto universitario:

```text
RestaurantManagement.slnx / RestaurantManagement.sln
│
├── RestaurantManagement.API/
│   ├── Controllers/
│   │   ├── ClientesController.cs
│   │   ├── MesasController.cs
│   │   ├── PedidosController.cs
│   │   ├── PlatosController.cs
│   │   └── ReportesController.cs
│   ├── Data/
│   │   └── RestaurantDbContext.cs
│   ├── DTOs/
│   │   └── DTOs.cs
│   ├── Helpers/
│   │   └── EcuadorianValidator.cs
│   ├── Migrations/
│   │   ├── 20260915122859_InitialCreate.cs
│   │   └── RestaurantDbContextModelSnapshot.cs
│   ├── Models/
│   │   ├── Cliente.cs
│   │   ├── DetallePedido.cs
│   │   ├── Enums.cs
│   │   ├── Mesa.cs
│   │   ├── Pedido.cs
│   │   └── Plato.cs
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── wwwroot/
│   │   ├── css/
│   │   │   └── styles.css
│   │   ├── js/
│   │   │   └── app.js
│   │   └── index.html
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Program.cs
│
└── RestaurantManagement.Tests/
    ├── BusinessRulesTests.cs
    ├── EcuadorianValidatorTests.cs
    └── RestaurantManagement.Tests.csproj
```

---

## 5. Requisitos Previos

Para compilar, ejecutar y evaluar la solución en su entorno de desarrollo, asegúrese de contar con:

1. **IDE:** Visual Studio Community 2022 / 2025 o Visual Studio Code con soporte C#.
2. **SDK:** .NET 10 SDK (`10.0.103` o superior).
3. **Motor de Base de Datos:** SQL Server 2019+ / SQL Server Express / LocalDB (`(localdb)\mssqllocaldb`).
4. **Herramienta EF Core CLI (opcional):** `dotnet-ef` para gestión manual de migraciones.
5. **Navegador Web:** Google Chrome, Microsoft Edge o Mozilla Firefox actualizado.

---

## 6. Configuración de la Base de Datos

La cadena de conexión a la base de datos se configura dentro del archivo `RestaurantManagement.API/appsettings.json`.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=RestaurantManagementDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

### Inicialización Automática
Al iniciar la aplicación (`Program.cs`), se ejecuta automáticamente `db.Database.EnsureCreated()` o las migraciones pendientes de EF Core, creando la base de datos `RestaurantManagementDB` junto con los datos iniciales de prueba (*Seed Data*).

---

## 7. Migraciones de Entity Framework Core

El proyecto utiliza Entity Framework Core Migrations para gestionar la estructura relacional.

Para crear una nueva migración desde la terminal de comandos:
```bash
dotnet ef migrations add InitialCreate --project RestaurantManagement.API
```

Para aplicar las migraciones a la base de datos configurada en SQL Server:
```bash
dotnet ef database update --project RestaurantManagement.API
```

---

## 8. Ejecución del Proyecto

### Opción A: Desde Visual Studio Community
1. Abra el archivo de solución `RestaurantManagement.slnx` o `RestaurantManagement.sln`.
2. Establezca `RestaurantManagement.API` como **Proyecto de inicio** (*StartUp Project*).
3. Presione `F5` o el botón **Ejecutar** (Perfil `http`).
4. El navegador se abrirá automáticamente en la dirección: `http://localhost:5160`.

### Opción B: Desde Terminal / Consola
Execute los siguientes comandos en la raíz del repositorio:
```bash
dotnet run --project RestaurantManagement.API/RestaurantManagement.API.csproj
```
Abra en su navegador la URL: `http://localhost:5160`.

---

## 9. Funcionalidades del Sistema

El sistema se compone de seis módulos interactivos integrados en un panel Dashboard:

1. **Dashboard:** Muestra tarjetas de métricas en tiempo real (total de clientes activos, platos registrados, mesas disponibles u ocupadas, cantidad de pedidos del día y ventas totales del día) junto con la tabla de pedidos recientes.
2. **Clientes:** Formulario de registro y tabla de clientes. Incluye validación del algoritmo de cédula ecuatoriana de 10 dígitos (Módulo 10) y protección contra duplicados.
3. **Platos y Menú:** Gestión del catálogo de alimentos y bebidas, definición de categorías, precios mayores a cero y selector de disponibilidad inmediata.
4. **Mesas:** Vista gráfica en tarjetas con indicadores visuales de capacidad y estado (`Disponible`, `Ocupada`, `Reservada`).
5. **Pedidos:** Asistente interactivo para seleccionar cliente, asignar mesa opcional, agregar platos con control de cantidades, cálculo visual de totales y envío al servidor. Permite cambiar el estado de pedidos existentes (`Pendiente` $\rightarrow$ `EnPreparacion` $\rightarrow$ `Completado` / `Cancelado`).
6. **Reportes por Cliente:** Módulo de consultas consolidando compras por cliente, con filtros dinámicos por rango de fechas y estado del pedido, mostrando resúmenes de ventas acumuladas y opción de impresión formateada (`window.print()`).

---

## 10. Reglas de Negocio e Integridad

1. **Protección de Datos Históricos (HTTP 409 Conflict):**
   * No se permite eliminar físicamente un cliente que tenga pedidos registrados.
   * No se permite eliminar un plato que forme parte de detalles de pedidos previos.
   * No se permite eliminar una mesa que posea pedidos asociados.
   * *Solución:* El sistema retorna un código HTTP `409 Conflict` con un mensaje explicativo y ofrece la opción de desactivar/inhabilitar el registro.
2. **Cálculo Estricto de Totales en Backend:**
   * El total del pedido y subtotales por ítem son calculados obligatoriamente en el servidor utilizando los precios vigentes en la base de datos SQL Server dentro de una transacción. No se confía en montos o subtotales calculados únicamente en el cliente.
3. **Sincronización Automática del Estado de Mesas:**
   * Al registrar un nuevo pedido con mesa asignada, la mesa pasa automáticamente a estado `Ocupada`.
   * Al marcar un pedido como `Completado` o `Cancelado`, la mesa regresa automáticamente a estado `Disponible`.
4. **Validación de Transiciones de Estado en Pedidos:**
   * Transiciones permitidas: `Pendiente` $\rightarrow$ `EnPreparacion` $\rightarrow$ `Completado` o `Cancelado`.
   * Una vez que un pedido entra en estado terminal (`Completado` o `Cancelado`), no se permite ninguna modificación posterior que altere su historial.
5. **Validación de Cédula Ecuatoriana (Módulo 10):**
   * Verificación algorítmica obligatoria de los 10 dígitos tanto en el navegador como en el API REST.
6. **Validación de Precios y Cantidades:**
   * Todo precio de plato y cantidad en detalle de pedido debe ser estrictamente mayor a cero ($> 0$).
7. **Consistencia de Pedido:**
   * No se permite crear pedidos vacíos sin detalles ni seleccionar platos inactivos o agotados.

---

## 11. API REST Endpoints

### Clientes (`/api/clientes`)
* `GET /api/clientes?buscar={term}&soloActivos={bool}` — Obtener lista de clientes con filtros opcionales.
* `GET /api/clientes/{id}` — Obtener detalle de un cliente por ID.
* `POST /api/clientes` — Registrar un nuevo cliente (valida cédula y duplicados).
* `PUT /api/clientes/{id}` — Actualizar información de cliente.
* `DELETE /api/clientes/{id}` — Eliminar cliente (retorna `409 Conflict` si tiene pedidos).

### Platos (`/api/platos`)
* `GET /api/platos?categoria={cat}&soloDisponibles={bool}&soloActivos={bool}` — Obtener platos del menú.
* `GET /api/platos/{id}` — Obtener plato por ID.
* `POST /api/platos` — Registrar plato en el menú.
* `PUT /api/platos/{id}` — Actualizar plato o cambiar disponibilidad.
* `DELETE /api/platos/{id}` — Eliminar plato (retorna `409 Conflict` si está en pedidos históricos).

### Mesas (`/api/mesas`)
* `GET /api/mesas?estado={estado}&soloActivas={bool}` — Consultar listado de mesas.
* `GET /api/mesas/{id}` — Obtener mesa por ID.
* `POST /api/mesas` — Crear nueva mesa.
* `PUT /api/mesas/{id}` — Actualizar capacidad o estado de mesa.
* `DELETE /api/mesas/{id}` — Eliminar mesa (retorna `409 Conflict` si tiene pedidos).

### Pedidos (`/api/pedidos`)
* `GET /api/pedidos?estado={estado}&clienteId={id}&fecha={fecha}` — Consultar pedidos.
* `GET /api/pedidos/{id}` — Obtener información completa y detalles de un pedido.
* `POST /api/pedidos` — Registrar pedido (calcula precios en backend y sincroniza mesa).
* `PUT /api/pedidos/{id}/estado` — Cambiar estado del pedido (`EnPreparacion`, `Completado`, `Cancelado`).

### Reportes y Dashboard (`/api/reportes`)
* `GET /api/reportes/dashboard` — Obtener métricas resumidas para el panel principal.
* `GET /api/reportes/cliente/{clienteId}?fechaInicio={date}&fechaFin={date}&estado={estado}` — Reporte detallado de historial por cliente.

---

## 12. Modelo de Datos Relacional

Las entidades principales y sus relaciones dentro de EF Core se organizan de la siguiente manera:

```text
  ┌───────────┐         1 : N         ┌───────────┐
  │  Cliente  ├───────────────────────┤  Pedido   │
  └───────────┘                       └─────┬─────┘
                                            │
  ┌───────────┐         1 : N               │ 1 : N
  │   Mesa    ├─────────────────────────────┤
  └───────────┘                             │
                                      ┌─────┴──────────┐
  ┌───────────┐         1 : N         │ DetallePedido  │
  │   Plato   ├───────────────────────┤                │
  └───────────┘                       └────────────────┘
```

* **Cliente (1) $\rightarrow$ (N) Pedido:** Un cliente puede registrar múltiples pedidos.
* **Mesa (1) $\rightarrow$ (N) Pedido:** Una mesa puede alojar múltiples pedidos a lo largo del tiempo (opcional, `MesaId` es nullable).
* **Pedido (1) $\rightarrow$ (N) DetallePedido:** Un pedido contiene uno o más detalles de consumo.
* **Plato (1) $\rightarrow$ (N) DetallePedido:** Un plato puede estar presente en múltiples detalles de pedido.

---

## 13. Validaciones Integradas

| Tipo de Validación | Frontend (JS) | Backend (C# Web API) |
| :--- | :--- | :--- |
| **Cédula Ecuatoriana (10 dígitos)** | Algoritmo Módulo 10 en tiempo real antes de enviar | `EcuadorianValidator.ValidarCedula` antes de guardar |
| **Campos Obligatorios** | Atributos `required` en formularios HTML y chequeos JS | Validación de Cadenas vacías o nulas en Controllers |
| **Precios y Cantidades** | `<input type="number" min="0.01">` en interfaz | Validación `Precio > 0` y `Cantidad > 0` |
| **Unicidad de Registros** | Verificación mediante mensajes de respuesta de API | Índices únicos en BD para Cédula y Número de Mesa |
| **Disponibilidad de Platos** | Deshabilitación visual e impide agregarlos al carrito | Verificación de `Disponible == true` y `Activo == true` |

---

## 14. Manejo de Errores e Indicadores HTTP

El API REST responde con códigos de estado HTTP estándar e información estructurada en formato JSON (`{ "mensaje": "..." }`):

* **`200 OK` / `201 Created`:** Operaciones exitosas de consulta, creación o actualización.
* **`400 Bad Request`:** Errores de validación (cédula inválida, precio $\le 0$, pedido sin platos, transición de estado no válida).
* **`404 Not Found`:** Solicitud de recursos inexistentes (cliente, plato, mesa o pedido no encontrado).
* **`409 Conflict`:** Intento de eliminación física de registros que poseen historial asociado (clientes con pedidos, platos consumidos, mesas usadas).
* **`500 Internal Server Error`:** Errores no controlados del servidor.

---

## 15. Ejecución de Pruebas Unitarias

El proyecto de pruebas `RestaurantManagement.Tests` verifica el correcto funcionamiento de las reglas de negocio críticas.

Para ejecutar todas las pruebas unitarias desde la terminal:
```bash
dotnet test
```

### Cobertura de Pruebas Incluidas:
1. **`EcuadorianValidatorTests`:**
   * Verificación con cédulas reales válidas (`1710034065`, `0926629916`).
   * Verificación de rechazo de cédulas inválidas, longitudes erróneas, cadenas vacías y valores nulos.
2. **`BusinessRulesTests`:**
   * Rechazo de creación de cliente con cédula inválida (`400 Bad Request`).
   * Retorno de conflicto (`409 Conflict`) al intentar eliminar clientes con historial.
   * Rechazo de platos con precio cero o negativo (`400 Bad Request`).
   * Retorno de conflicto (`409 Conflict`) al intentar eliminar platos presentes en pedidos.
   * Verificación del cálculo estricto de totales en el backend e importes agregados.
   * Sincronización automática de estado de mesa a `Ocupada` al registrar un pedido.
   * Prohibición de cambio de estado a pedidos finalizados o cancelados.

---

## 16. Módulo de Reportes por Cliente

El módulo de reportes permite a los administradores e instructores revisar la actividad histórica completa de cada cliente:

1. Ingrese a la sección **Reportes por Cliente** desde el menú lateral.
2. Seleccione un cliente del listado desplegable.
3. Configure opcionalmente los filtros de **Fecha Inicial**, **Fecha Final** y **Estado del Pedido**.
4. Haga clic en **Generar Reporte**.
5. Se mostrará el encabezado con datos del cliente (Nombre, Cédula, Teléfono, Email), resumen de ventas acumuladas y el detalle desglose de cada pedido (Platos, Cantidades, Precio Unitario, Subtotal y Total).
6. Para obtener una copia física o guardar en formato PDF, presione el botón **🖨️ Imprimir Reporte**, el cual activa el motor de impresión del navegador (`window.print()`) aplicando estilos de impresión limpios que ocultan las barras de navegación.

---

## 17. Evidencias Académicas Recomendadas

Para presentaciones o entregables universitarios, se sugiere capturar las siguientes pantallas de la aplicación:

1. **Dashboard Principal:** Vista general con métricas dinámicas y la tabla de pedidos del día.
2. **Gestión de Clientes:** Tabla de clientes registrados y modal con validación de cédula ecuatoriana.
3. **Catálogo de Platos:** Menú formateado con filtros por categoría y badges de disponibilidad.
4. **Estado de Mesas:** Rejilla visual de tarjetas mostrando mesas disponibles y ocupadas.
5. **Creación de Pedido:** Asistente de pedido mostrando selección de cliente, mesa, carrito con cantidades y total.
6. **Detalle y Cambio de Estado de Pedido:** Modal de consulta de ítems consumidos y menú de transición de estados.
7. **Reporte por Cliente e Impresión:** Resumen consolidado del cliente con desglose de facturación y vista previa de impresión.
8. **Prueba de Regla de Integridad (HTTP 409):** Mensaje de alerta de conflicto al intentar borrar un cliente o plato con historial.
9. **Resultados de Pruebas Unitarias:** Ejecución exitosa de `dotnet test` en consola.

---

## 18. Solución de Problemas Frecuentes

* **Error de Conexión a SQL Server / LocalDB:**
  * *Causa:* El servicio de SQL Server o LocalDB no se encuentra activo en su máquina.
  * *Solución:* Verifique que el servicio `mssqllocaldb` o SQL Server esté iniciado. Alternativamente, la aplicación cuenta con un mecanismo de fallback seguro que utiliza SQLite cuando se ejecuta en entornos de desarrollo Linux/Docker.
* **Puerto 5160 en Uso:**
  * *Causa:* Otra instancia del servidor o proceso se encuentra escuchando en el mismo puerto.
  * *Solución:* Detenga los procesos activos o modifique el puerto dentro de `RestaurantManagement.API/Properties/launchSettings.json`.
* **Visualización de Cambios en Frontend:**
  * *Causa:* Caché persistente del navegador sobre archivos en `wwwroot`.
  * *Solución:* Recargue la página web utilizando `Ctrl + F5` o limpie la caché del navegador.

---

## 19. Estado del Proyecto

El sistema **RestaurantManagement** se encuentra **100% implementado, funcional y probado**, cumpliendo con la totalidad de los requisitos académicos obligatorios y opcionales especificados.

---

## 20. Licencia y Uso Académico

Este proyecto ha sido desarrollado exclusivamente con fines académicos y de evaluación universitaria. Libre para fines educativos.
