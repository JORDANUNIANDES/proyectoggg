# Sistema de Gestión de Restaurantes (RestaurantManagement)

Proyecto académico desarrollado en **Visual Studio Community** y **.NET 10**, diseñado para la administración integral de un restaurante mediante una arquitectura limpia y separada en Backend API REST y Frontend Web Vanilla.

---

## 1. Tecnologías Utilizadas

### Backend
* **Plataforma:** .NET 10 SDK / C#
* **API:** ASP.NET Core Web API
* **ORM:** Entity Framework Core Code First
* **Base de Datos:** SQL Server / SQL Server LocalDB (`(localdb)\MSSQLLocalDB`)
* **Documentación:** Swagger / OpenAPI
* **Pruebas:** xUnit (.NET 10)

### Frontend
* **HTML5**
* **CSS3** (Diseño responsivo con CSS Grid y Flexbox)
* **JavaScript Vanilla** (ES6+)
* **Fetch API** para consumo asíncrono del backend REST

---

## 2. Estructura de la Solución

La solución `RestaurantManagement.sln` se compone de los siguientes proyectos:

```
RestaurantManagement/
├── RestaurantManagement.API/       # Web API ASP.NET Core (.NET 10)
│   ├── Controllers/               # Endpoints RESTful
│   ├── Data/                      # DbContext y DbInitializer (Seed Data)
│   ├── DTOs/                      # Data Transfer Objects
│   ├── Migrations/                # Migraciones EF Core
│   ├── Models/                    # Entidades de Dominio
│   ├── Services/                  # Reglas de Negocio y Validación Cédula
│   └── wwwroot/                   # Frontend Estático Servido
├── RestaurantManagement.Client/    # Frontend Web (HTML, CSS, JS)
│   ├── css/
│   ├── js/
│   ├── assets/
│   └── index.html
├── RestaurantManagement.Tests/     # Proyecto de pruebas unitarias xUnit
└── RestaurantManagement.sln        # Solución de Visual Studio
```

---

## 3. Entidades Principales y Reglas de Negocio

1. **Cliente:**
   - Campos: Id, Nombre, Apellido, Cédula, Teléfono, Email, Dirección, FechaRegistro, Estado.
   - Validaciones: Algoritmo oficial de cédula ecuatoriana (10 dígitos / módulo 10), email válido, cédula y email únicos.
   - Regla de eliminación: Si tiene pedidos asociados, se devuelve **HTTP 409 Conflict** ("No se puede eliminar el cliente porque tiene pedidos registrados").

2. **Plato:**
   - Campos: Id, Nombre, Descripción, Precio, Categoría, Disponible, FechaRegistro.
   - Validaciones: Precio mayor a 0, nombre único.
   - Regla de eliminación: Si forma parte del historial de pedidos, se aplica **baja lógica** (`Disponible = false`).

3. **Mesa:**
   - Campos: Id, Número, Capacidad, Ubicación, Estado (Disponible, Ocupada, Reservada, Mantenimiento).
   - Validaciones: Número único, capacidad mayor a 0.
   - Regla de eliminación: Retorna **HTTP 409 Conflict** si posee pedidos registrados.

4. **Pedido y DetallePedido:**
   - Relaciones: Cliente (1:N), Mesa (1:N opcional), Pedido-DetallePedido (1:N), Plato-DetallePedido (1:N).
   - Regla de cálculo: Los precios unitarios y subtotales son calculados y validados estrictamente en el backend desde la base de datos dentro de una transacción.

---

## 4. Requisitos de Ejecución

* **Visual Studio Community 2022 / 2025** con la carga de trabajo de desarrollo ASP.NET y web.
* **.NET 10.0 SDK**
* **SQL Server Express LocalDB**

---

## 5. Instrucciones de Instalación y Ejecución

### Opción A: Desde Visual Studio Community

1. Abrir `RestaurantManagement.sln` en Visual Studio.
2. Hacer clic derecho sobre la solución y seleccionar **Restaurar paquetes NuGet**.
3. Asegurar que `RestaurantManagement.API` esté configurado como **Proyecto de Inicio** (Startup Project).
4. En la consola del Administrador de Paquetes o terminal, ejecutar las migraciones:
   ```bash
   dotnet ef database update --project RestaurantManagement.API
   ```
5. Presionar **F5** o **Ctrl + F5** para ejecutar.
6. La aplicación se abrirá en el navegador. Podrá acceder a:
   - **Frontend App:** `https://localhost:7198/`
   - **Swagger OpenAPI:** `https://localhost:7198/swagger`

### Opción B: Desde CLI / Línea de Comandos

```bash
# 1. Clonar el repositorio
git clone <URL_DEL_REPOSITORIO>
cd gestion-clientes

# 2. Restaurar dependencias
dotnet restore

# 3. Aplicar migraciones para crear la base de datos SQL Server LocalDB
dotnet ef database update --project RestaurantManagement.API

# 4. Ejecutar el backend API
dotnet run --project RestaurantManagement.API

# 5. Ejecutar pruebas unitarias
dotnet test RestaurantManagement.Tests
```

---

## 6. Endpoints Principales de la API

* **Clientes:** `GET/POST /api/clientes`, `GET/PUT/DELETE /api/clientes/{id}`
* **Platos:** `GET/POST /api/platos`, `GET/PUT/DELETE /api/platos/{id}`
* **Mesas:** `GET/POST /api/mesas`, `GET/PUT/DELETE /api/mesas/{id}`
* **Pedidos:** `GET/POST /api/pedidos`, `GET /api/pedidos/{id}`, `PUT /api/pedidos/{id}/estado`
* **Reportes:** `GET /api/reportes/pedidos-por-cliente/{clienteId}`, `GET /api/reportes/dashboard`

---

## 7. Instrucciones para subir a GitHub

```bash
git add .
git commit -m "feat: Sistema de Gestion de Restaurantes finalizado"
git branch -M main
git remote add origin <URL_DE_TU_REPOSITORIO_GITHUB>
git push -u origin main
```
