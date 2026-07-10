# Sistema de Gestión de Agencia de Publicidad

Este es un sistema web completo de **Gestión de Agencia de Publicidad** desarrollado con **.NET 10** (ASP.NET Core MVC), **Entity Framework Core**, y **SQL Server**. El proyecto está optimizado para ejecutarse y abrirse directamente en **Visual Studio Community 2026** (o Visual Studio 2022).

---

## 📋 Requisitos Previos

Antes de ejecutar la aplicación, asegúrate de tener instalado en tu equipo lo siguiente:

1. **Visual Studio Community 2026** (o Visual Studio 2022 con soporte para .NET 10).
2. **SDK de .NET 10** (incluido en las instalaciones recientes de Visual Studio).
3. **Microsoft SQL Server** (con la instancia por defecto de **LocalDB**: `(localdb)\mssqllocaldb`) o una base de datos SQL Server configurada.

---

## 🛠️ Estructura del Modelo de Base de Datos

El sistema incluye única y exclusivamente las siguientes tablas y relaciones, modeladas de manera segura utilizando caracteres ASCII en código/Base de Datos para prevenir errores de encoding, pero presentadas con acentos y eñes correspondientes en la interfaz de usuario:

### 1. **Clientes** (`Clientes`)
* `cliente_id` (PK, Entero Autoincremental)
* `nombre_empresa` (Cadena de texto, Obligatorio)
* `contacto` (Cadena de texto, Obligatorio)
* `telefono` (Cadena de texto, Obligatorio)
* `email` (Cadena de texto, Obligatorio)

### 2. **Campanas** (`Campanas`)
* `campana_id` (PK, Entero Autoincremental)
* `cliente_id` (FK a Clientes)
* `nombre` (Cadena de texto, Obligatorio)
* `presupuesto` (Decimal, Obligatorio)
* `fecha_inicio` (Fecha, Obligatorio)

### 3. **Disenadores** (`Disenadores`)
* `disenador_id` (PK, Entero Autoincremental)
* `nombre` (Cadena de texto, Obligatorio)
* `especialidad` (Cadena de texto, Obligatorio)
* `email` (Cadena de texto, Obligatorio)
* `telefono` (Cadena de texto, Obligatorio)

### 4. **Entregables** (`Entregables`)
* `entregable_id` (PK, Entero Autoincremental)
* `campana_id` (FK a Campanas)
* `disenador_id` (FK a Disenadores)
* `tipo` (Cadena de texto, Obligatorio)
* `fecha_entrega` (Fecha, Obligatorio)

### **Relaciones Configuradas:**
* Un Cliente puede tener muchas Campañas.
* Una Campaña pertenece a un Cliente.
* Un Entregable pertenece a una Campaña (con Eliminación en Cascada).
* Un Entregable es asignado a un Diseñador (con Restricción de Eliminación para evitar ciclos de cascada múltiples en SQL Server).

---

## 🚀 Instrucciones de Configuración y Ejecución (Paso a Paso)

### Paso 1: Clonar o Descargar el Repositorio
Coloca los archivos del proyecto en la carpeta local de tu preferencia en tu máquina Windows.

### Paso 2: Abrir el Proyecto en Visual Studio
1. Abre **Visual Studio Community 2026** (o 2022).
2. Selecciona **Abrir un proyecto o una solución**.
3. Navega al directorio raíz del proyecto y abre el archivo de solución `AgenciaPublicidad.slnx` (o importa el proyecto `AgenciaPublicidad.csproj`).

### Paso 3: Revisar la Cadena de Conexión (Opcional)
En el archivo `AgenciaPublicidad/appsettings.json`, verás la configuración predeterminada que apunta a SQL Server LocalDB:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AgenciaPublicidadDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```
*Si prefieres utilizar una instancia diferente de SQL Server (por ejemplo, SQL Server Express o Developer Edition), puedes cambiar la cadena de conexión en este archivo.*

### Paso 4: Restaurar Paquetes y Compilar
1. En el Explorador de soluciones, haz clic derecho sobre la solución `AgenciaPublicidad` y selecciona **Restaurar paquetes NuGet**.
2. Presiona `Ctrl + Shift + B` o ve al menú **Compilar > Compilar solución** para compilar el proyecto.

### Paso 5: Generar y Aplicar la Base de Datos (Migraciones)
El proyecto ya cuenta con las migraciones iniciales (`InitialCreate`) generadas.
1. Abre la **Consola del Administrador de Paquetes** en Visual Studio (**Herramientas > Administrador de paquetes NuGet > Consola del Administrador de Paquetes**).
2. Asegúrate de que el proyecto predeterminado seleccionado sea `AgenciaPublicidad`.
3. Ejecuta el siguiente comando para crear automáticamente la base de datos y todas sus tablas en tu SQL Server LocalDB:
   ```powershell
   Update-Database
   ```
*(Alternativamente, la aplicación cuenta con un inicializador automático en `Program.cs` que intentará crear y migrar la base de datos automáticamente al iniciar el servidor por primera vez, por lo que este paso también puede automatizarse).*

### Paso 6: Iniciar la Aplicación
1. Presiona **F5** o haz clic en el botón **Iniciar (IIS Express / AgenciaPublicidad)** en la barra de herramientas superior de Visual Studio.
2. Tu navegador se abrirá automáticamente en la página de inicio del **Sistema de Gestión de Agencia de Publicidad** (`http://localhost:5207`).

---

## 🖥️ Uso de la Aplicación

La aplicación se inicia con un panel interactivo (Dashboard) que te permite navegar cómodamente hacia las cuatro secciones del sistema:
* **Clientes:** Permite el flujo completo CRUD (listar, crear nuevo, ver detalles, editar existentes y eliminar).
* **Campañas:** Permite gestionar campañas publicitarias vinculándolas a los Clientes existentes mediante selectores dropdown.
* **Diseñadores:** Permite registrar y actualizar los datos del equipo de diseño.
* **Entregables:** Permite crear entregables de trabajo, asignando cada uno a una campaña específica y a un diseñador responsable.
