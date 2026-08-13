# MUSCLE HOUSE — Sistema Web Inteligente de Gestión de Gimnasio

¡Bienvenido a **MUSCLE HOUSE**! Un sistema web deportivo, premium y sumamente moderno diseñado para la administración de un gimnasio y el seguimiento interactivo del rendimiento de sus miembros utilizando Inteligencia Artificial.

Este proyecto ha sido desarrollado bajo una arquitectura limpia de Modelo-Vista-Controlador (MVC), asegurando que todas las reglas de negocio, autorizaciones y control de seguridad horizontal y vertical se cumplan de forma robusta.

---

## 🚀 Tecnologías Obligatorias Utilizadas

- **.NET 10** (ASP.NET Core MVC)
- **C# 13**
- **Entity Framework Core 10** (Mapeador de base de datos)
- **SQL Server / LocalDB** (Proveedor oficial y exclusivo en producción)
- **ASP.NET Core Identity** (Módulo de autenticación y autorización segura)
- **Razor Views** & **Bootstrap 5** (Layout deportivo con tema premium oscuro, acentos naranja y amarillo)
- **JavaScript / jQuery / AJAX** (Peticiones asíncronas para el Asistente IA)
- **Chart.js** (Gráficos interactivos de evolución física y reportes de cobros)
- **xUnit** (Suite de pruebas automatizadas)

---

## 🛠️ Estructura del Proyecto

```text
MuscleHouse/                 <- Proyecto Web Principal
│
├── Controllers/             <- Account, Admin, Recepcionista, Entrenador, Cliente y Chat (IA)
├── Models/                  <- Modelos de datos del negocio (14 entidades) y ApplicationUser (Identity)
├── ViewModels/              <- ViewModels para cada vista, consultas y formularios con Data Annotations
├── Data/                    <- DbContext con Fluent API y DbInitializer (seeding idempotente)
├── Services/                <- Servicios de negocio desacoplados (Membership, Payment, Attendance, etc.)
├── wwwroot/                 <- CSS fragmentado (theme, layout, components), uploads de personal, JS y más
└── Migrations/              <- Única migración inicial limpia 'InitialCreate' para SQL Server
```

---

## 🔑 Cuentas y Credenciales de Prueba

Al ejecutar la aplicación por primera vez, el **DbInitializer** poblará automáticamente los roles e insertará de forma idempotente las siguientes cuentas de prueba:

- **Administrador**:
  - **Usuario**: `admin@musclehouse.com`
  - **Contraseña**: `MuscleHouse123!`
- **Recepcionista**:
  - **Usuario**: `recepcionista@musclehouse.com`
  - **Contraseña**: `MuscleHouse123!`
- **Entrenador**:
  - **Usuario**: `entrenador@musclehouse.com`
  - **Contraseña**: `MuscleHouse123!`
- **Usuario (Cliente)**:
  - **Usuario**: `cliente@musclehouse.com`
  - **Contraseña**: `MuscleHouse123!`

---

## 🏃 Paso a Paso para la Instalación y Ejecución

Sigue estos sencillos pasos para compilar y ejecutar **MUSCLE HOUSE**:

### 1. Requisitos Previos
- Asegúrate de tener instalado el **.NET SDK 10**.
- Disponer de **SQL Server LocalDB** (en Windows) o una instancia de SQL Server accesible mediante cadena de conexión.

### 2. Configurar la Cadena de Conexión
Abre el archivo `MuscleHouse/appsettings.json` y edita la cadena de conexión de ser necesario:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=GymManagementDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

### 3. Restaurar Paquetes NuGet y Compilar
Abre una terminal en la raíz del repositorio y ejecuta:
```bash
dotnet restore
dotnet build
```

### 4. Crear la Base de Datos y Aplicar Migraciones
Para generar la base de datos `GymManagementDB` en tu servidor local con todas las tablas e índices de **MUSCLE HOUSE**, ejecuta:
```bash
dotnet ef database update --project MuscleHouse
```

### 5. Iniciar la Aplicación
Para ejecutar el servidor web de desarrollo, utiliza:
```bash
dotnet run --project MuscleHouse
```
Abre tu navegador en la dirección local que se muestre en pantalla (habitualmente `http://localhost:5000` o `https://localhost:5001`).

---

## 🤖 Configuración del Asistente de IA (OpenAI)

El Asistente de IA cuenta con un esquema de fallback transparente y sumamente robusto:
- Si deseas utilizar la API oficial de OpenAI, configura tu clave de API en la variable de entorno **`OPENAI_API_KEY`** o en `appsettings.json` bajo la sección:
  ```json
  "OpenAI": {
    "ApiKey": "TU_API_KEY_AQUÍ"
  }
  ```
- Si la clave está ausente, el sistema utilizará automáticamente el **`MockAIService`**, el cual analiza las métricas de progreso corporal reales del cliente logueado (peso, cintura, brazo), su objetivo y sus cargas de entrenamiento, formulando una respuesta 100% personalizada y en español sin romper la aplicación.

---

## 🧪 Ejecución de las Pruebas Automatizadas

El proyecto de pruebas de xUnit cuenta con **17 pruebas integrales y de seguridad**. Para ejecutarlas, simplemente corre:
```bash
dotnet test
```

---

## 🛡️ Medidas de Seguridad y Privacidad

- **Acceso Horizontal Protegido**: Un cliente nunca puede visualizar o registrar el progreso físico, rutinas o membresías de otro miembro inyectando IDs en la URL. El sistema valida la pertenencia basándose en el usuario de la sesión autenticada.
- **Acceso Restringido para Entrenadores**: El entrenador está confinado únicamente a leer, diseñar y editar rutinas de sus alumnos asignados, arrojando excepciones inmediatas en caso de intrusiones.
- **Protección CSRF**: Todas las acciones POST de modificación de datos están protegidas mediante antiforgery tokens.
