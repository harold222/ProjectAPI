# ProjectAPI

API REST para la gestión de Posts, Customers y Categories. Desarrollada en .NET 8 con Clean Architecture, como solución a una prueba técnica de la empresa Post Ltda. (Juju).

## Stack Tecnológico

| Componente | Versión | Propósito |
|---|---|---|
| .NET | 8.0 | Framework principal (migrado desde .NET Core 2.1) |
| Entity Framework Core | 8.0.0 | ORM y acceso a datos |
| EF Core SQL Server | 8.0.0 | Provider de base de datos |
| Serilog.AspNetCore | 8.0.0 | Logging estructurado |
| Serilog.Sinks.MSSqlServer | 6.0.0 | Sink de Serilog hacia SQL Server |
| Swashbuckle.AspNetCore | 6.5.0 | Documentación Swagger/OpenAPI |
| Microsoft.AspNetCore.OpenApi | 8.0.0 | Soporte OpenAPI nativo |
| SQL Server | LocalDB | Base de datos relacional |
| C# | 12 | Lenguaje (Nullable + ImplicitUsings habilitados) |

## Arquitectura — Clean Architecture

El proyecto sigue los principios de Clean Architecture con 4 capas y regla de dependencia estricta: las flechas solo apuntan hacia adentro.

```
┌─────────────────────────────────────────────────────────┐
│  API (Presentation Layer)                                │
│  Controllers delgados · Middleware · Swagger · CORS       │
└──────────────────────┬──────────────────────────────────┘
                       │ depende de
┌──────────────────────▼──────────────────────────────────┐
│  Application (Use Cases / Business Logic)                │
│  AppServices · BaseService<T> · DTOs · ValidationService │
└──────────────────────┬──────────────────────────────────┘
                       │ depende de
┌──────────────────────▼──────────────────────────────────┐
│  Domain (Enterprise Business Rules)                      │
│  Entities · IBaseRepository<T> · Sin dependencias        │
└──────────────────────┬──────────────────────────────────┘
                       │ implementado por
┌──────────────────────▼──────────────────────────────────┐
│  Infrastructure (Data Access)                            │
│  BaseRepository<T> · JujuTestContext · EF Core config    │
└─────────────────────────────────────────────────────────┘
```

### Regla de dependencia

- **Domain** no depende de ninguna capa (puro C#).
- **Infrastructure** implementa interfaces definidas en Domain (`IBaseRepository<T>`).
- **Application** orquesta la lógica de negocio usando las interfaces de Domain.
- **API** solo maneja HTTP y delega a Application.

### Estructura de proyectos

```
ProjectAPI.sln
├── API/
│   ├── Controllers/
│   │   ├── CustomerController.cs      # Endpoints CRUD de Customer
│   │   └── PostController.cs          # Endpoints CRUD de Post
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs  # Manejo centralizado de errores
│   ├── Program.cs                     # Composition Root + configuración
│   ├── appsettings.json               # Connection strings + Serilog config
│   ├── requests.http                  # Colección de pruebas HTTP
│   └── API.csproj
│
├── Application/
│   ├── DTOs/
│   │   ├── CategoryDto.cs             # DTOs de Category (Create, Response)
│   │   ├── CustomerDto.cs             # DTOs de Customer (Create, Update, Response, FailedItem, CreateAllResult)
│   │   └── PostDto.cs                 # DTOs de Post (Create, Update, Response, FailedItem, CreateAllResult)
│   ├── Services/
│   │   ├── BaseService.cs             # Servicio CRUD genérico
│   │   ├── CategoryAppService.cs      # Lógica de resolución/creación de categorías
│   │   ├── CustomerAppService.cs      # Lógica de negocio de Customer
│   │   └── PostAppService.cs           # Lógica de negocio de Post
│   ├── Shared/
│   │   └── ValidationService.cs        # Validaciones reutilizables (nombre duplicado)
│   └── Application.csproj
│
├── Domain/
│   ├── Entities/
│   │   ├── Category.cs                # Entidad Category (CategoryId, CategoryName, Posts)
│   │   ├── Customer.cs                # Entidad Customer (CustomerId, Name, Posts)
│   │   ├── Post.cs                     # Entidad Post (PostId, Title, Body, CategoryId, CustomerId)
│   │   └── Logs.cs                     # Entidad Logs (Serilog sink)
│   ├── IBaseRepository.cs             # Interfaz genérica del repositorio
│   └── Domain.csproj                  # Sin dependencias externas
│
├── Infrastructure/
│   ├── Data/
│   │   ├── BaseRepository.cs          # Implementación genérica con EF Core
│   │   └── JujuTestContext.cs         # DbContext + Fluent API configuration
│   └── Infrastructure.csproj
│
├── JujuTests.Script.sql                # Script completo de creación DB + seed data
└── Instructions.txt                    # Enunciado original de la prueba técnica
```

## Modelo de Base de Datos

```
┌──────────────┐       ┌──────────────────┐       ┌──────────────┐
│   Category   │       │      Post        │       │   Customer   │
├──────────────┤       ├──────────────────┤       ├──────────────┤
│ CategoryId   │◄──┐   │ PostId          │   ┌──►│ CustomerId   │
│ CategoryName │   │   │ Title (500)     │   │   │ Name (500)   │
└──────────────┘   │   │ Body (500)      │   │   └──────────────┘
                   │   │ CategoryId FK   │   │
                   │   │ CustomerId FK ──┘   │
                   │   └──────────────────┘   │
                   │                          │
                   │  FK_Post_Category:        │
                   │  ON DELETE SET NULL       │
                   └─ FK_Post_Customer:       │
                      ON DELETE CASCADE ──────┘
```

### Relaciones

- **Post → Customer**: N:1 obligatorio (`CustomerId` NOT NULL, `ON DELETE CASCADE`).
- **Post → Category**: N:1 opcional (`CategoryId` nullable, `ON DELETE SET NULL`).
- **Customer → Posts**: Colección virtual (1:N).
- **Category → Posts**: Colección virtual (1:N).

### Seed Data

- **Categorías**: Farándula (1), Política (2), Futbol (3).
- **Customers**: Maria con (4), Laura Sugey (1002), Juan P (2002), Juan albero (3002).
- **Posts**: 3 registros de ejemplo.

## Patrones de Diseño

### 1. Repository Pattern (Genérico)

`IBaseRepository<T>` define el contrato. `BaseRepository<T>` lo implementa con EF Core. Abstrae el acceso a datos y permite cambiar de ORM sin modificar la capa de negocio.

### 2. Service Layer Pattern

`BaseService<T>` como base genérica. Los `*AppService` encapsulan la lógica de negocio específica. Los controllers son delgados: reciben HTTP, delegan al AppService y retornan el status code correspondiente.

### 3. DTO Pattern (Clases Anidadas)

Cada DTO contiene clases internas para cada operación: `Create`, `Update`, `Response`, `FailedItem`, `CreateAllResult`. Esto separa el contrato de la API del modelo de dominio y evita exponer nombres de columnas de la base de datos.

### 4. Manual Mapping

Se eliminó AutoMapper por vulnerabilidad de seguridad (GHSA-rvv3-g6hj-g44x) y porque la versión 15+ requiere licencia paga. El mapeo se realiza con métodos estáticos en cada AppService. Con pocas entidades, es más simple, explícito y sin "magia".

### 5. Middleware Pattern

`ExceptionHandlingMiddleware` captura todas las excepciones no manejadas y las convierte en respuestas HTTP estructuradas con `TraceId`, `StatusCode` y `Message`.

### 6. Dependency Injection

Todos los servicios y repositorios se registran en `Program.cs` con el contenedor DI nativo de .NET, lifetime `Scoped`.

## Requerimientos Resueltos

| # | Requerimiento | Solución | Archivo |
|---|---|---|---|
| 1 | PUT Customer no funcionaba | El error original era `DbUpdateConcurrencyException` por uso síncrono. Se resolvió con `async/await` y `CurrentValues.SetValues` en `UpdateAsync` | `CustomerAppService.UpdateAsync` |
| 2 | No permitir Customer con nombre duplicado | `ValidationService.CustomerNameExists()` con comparación case-insensitive y `excludeId` para updates | `ValidationService.cs` |
| 3a | Validar que Customer exista al crear Post | Se consulta `_customerService.GetAsync(customerId)` y se lanza `InvalidOperationException` si es null | `PostAppService.CreateAsync` |
| 3b | Truncar Body > 20 caracteres | `FormatBody()`: si supera 20 caracteres → `substring(0, 97) + "..."` | `PostAppService.FormatBody` |
| 3c | Resolver Category según Type | Se normalizó: Category es tabla separada con FK. `CategoryAppService.ResolveOrCreateAsync()` resuelve por ID o crea nueva vía `CustomCategory` | `CategoryAppService.cs` |
| 4 | Eliminar Posts antes de eliminar Customer | La FK tiene `ON DELETE CASCADE` en la base de datos, SQL Server elimina los Posts automáticamente | `CustomerAppService.DeleteAsync` |
| 5 | API para crear N Posts | `CreateAllAsync` valida cada item, separa Created/Failed, y hace batch insert de los válidos | `PostAppService.CreateAllAsync` |
| 6 | Mejorar código con buenas prácticas | Clean Architecture, async/await, DTOs, Data Annotations, middleware de errores, separación de responsabilidades | Todo el proyecto |

## API Endpoints

### Customer

| Método | Ruta | Descripción | Body | Response |
|---|---|---|---|---|
| `GET` | `/Customer` | Obtener todos los customers | — | `CustomerDto.Response[]` |
| `POST` | `/Customer` | Crear un customer | `CustomerDto.Create` | `201 CustomerDto.Response` |
| `POST` | `/Customer/CreateAll` | Crear N customers | `CustomerDto.Create[]` | `200 CustomerDto.CreateAllResult` |
| `PUT` | `/Customer` | Actualizar un customer | `CustomerDto.Update` | `200 CustomerDto.Response` |
| `DELETE` | `/Customer/{id}` | Eliminar un customer (y sus posts en cascada) | — | `200 CustomerDto.DeleteResponse` |

### Post

| Método | Ruta | Descripción | Body | Response |
|---|---|---|---|---|
| `GET` | `/Post` | Obtener todos los posts | — | `PostDto.Response[]` |
| `POST` | `/Post` | Crear un post | `PostDto.Create` | `201 PostDto.Response` |
| `POST` | `/Post/CreateAll` | Crear N posts | `PostDto.Create[]` | `200 PostDto.CreateAllResult` |
| `PUT` | `/Post` | Actualizar un post | `PostDto.Update` | `200 PostDto.Response` |
| `DELETE` | `/Post/{id}` | Eliminar un post | — | `200 PostDto.DeleteResponse` |

### Ejemplos de Request/Response

**Crear Customer:**
```json
POST /Customer
{ "Name": "Juan Perez" }

// 201 Created
{ "Id": 5, "Name": "Juan Perez", "Ok": true }
```

**Crear Post con categoría existente:**
```json
POST /Post
{
  "Title": "Noticia deportiva",
  "Body": "Este es un texto de ejemplo que supera los veinte caracteres para demostrar la truncación",
  "CategoryId": 3,
  "CustomerId": 4
}

// 201 Created
{
  "Id": 5,
  "Title": "Noticia deportiva",
  "Body": "Este es un texto de ejemplo que supera los veinte caracteres para demostrar la truncaci\u00f3n y ahora se corta el texto con...",
  "CategoryId": 3,
  "CustomerId": 4,
  "Ok": true
}
```

**Crear Post con categoría custom:**
```json
POST /Post
{
  "Title": "Tecnología",
  "Body": "Contenido breve",
  "CustomCategory": "Tecnología",
  "CustomerId": 4
}

// 201 Created — Se crea la categoría "Tecnología" automáticamente
```

**Crear N Posts (batch):**
```json
POST /Post/CreateAll
[
  { "Title": "Post válido", "Body": "Contenido", "CategoryId": 1, "CustomerId": 4 },
  { "Title": "Post inválido", "Body": "Contenido", "CategoryId": 999, "CustomerId": 999 }
]

// 200 OK
{
  "Created": [
    { "Id": 6, "Title": "Post válido", "Body": "Contenido", "CategoryId": 1, "CustomerId": 4, "Ok": true }
  ],
  "Failed": [
    { "Title": "Post inválido", "CustomerId": 999, "Reason": "El usuario asociado no existe", "Ok": false }
  ]
}
```

**Error response (middleware):**
```json
// 400 Bad Request
{
  "traceId": "00-abc123-def456-01",
  "statusCode": 400,
  "message": "El nombre ingresado ya existe"
}
```

## Configuración y Ejecución

### Prerrequisitos

- .NET 8 SDK
- SQL Server (LocalDB o instancia completa)
- Visual Studio 2022+ o VS Code

### Pasos

1. **Base de datos**: Ejecutar el script `JujuTests.Script.sql` en SQL Server Management Studio para crear la base de datos `JujuTest` con sus tablas y datos semilla.

2. **Connection String**: Verificar que la cadena de conexión en `API/appsettings.json` apunte a tu instancia de SQL Server:
   ```json
   "ConnectionStrings": {
     "Development": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=JujuTest;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;"
   }
   ```

3. **Ejecutar la API**:
   ```bash
   dotnet run --project API
   ```
   O desde Visual Studio con IIS Express (puerto 5002).

4. **Swagger**: Navegar a `http://localhost:5002/swagger` para ver la documentación interactiva.

5. **Pruebas**: Usar el archivo `API/requests.http` con tu cliente HTTP favorito (REST Client en VS Code, IntelliJ HTTP Client, etc.).

## Decisiones Arquitectónicas

| Decisión | Razón |
|---|---|
| Migración .NET Core 2.1 → .NET 8 | .NET Core 2.1 fuera de soporte desde 2019, con vulnerabilidades conocidas y sin parches de seguridad |
| Clean Architecture sobre 3 capas | La estructura original (Business/DataAccess) mezclaba responsabilidades y exponía nombres de columnas DB en la API |
| Manual mapping en vez de AutoMapper | AutoMapper v12 tenía vulnerabilidad GHSA-rvv3-g6hj-g44x y v15+ requiere licencia paga. Con pocas entidades, mapeo manual es más simple y explícito |
| Category como tabla separada | El requerimiento original mapeaba Type 1/2/3 a categorías fijas. Normalizar a tabla permite categorías custom sin cambiar código, siguiendo principios de normalización |
| Async/await en toda la pila | El código original era síncrono, bloqueando threads del pool. Operaciones async escalan mejor bajo carga |
| Middleware de excepciones | Evita try/catch repetidos en cada controller y asegura respuestas de error consistentes con TraceId |
| DTOs con clases anidadas | Cada operación tiene su propio contrato. `Create` no tiene `Id`, `Update` sí. `Response` no expone nombres de columnas (`CustomerId` → `Id`) |
| `ExecuteDeleteAsync` para batch delete | Genera un único SQL `DELETE WHERE ...` en vez de N+1 consultas individuales |
| `ValidationService` como clase estática | Las validaciones de negocio (nombre duplicado) son puras y no necesitan estado ni dependencias |

## Manejo de Errores

El middleware `ExceptionHandlingMiddleware` mapea excepciones a códigos HTTP:

| Excepción | HTTP Status | Ejemplo |
|---|---|---|
| `KeyNotFoundException` | 404 Not Found | Post Id 999 no encontrado |
| `InvalidOperationException` | 400 Bad Request | El nombre ingresado ya existe |
| `ArgumentException` | 400 Bad Request | Parámetro inválido |
| Cualquier otra | 500 Internal Server Error | Error inesperado (no expone detalles internos) |

La validación de modelo con Data Annotations (`[Required]`, `[StringLength]`) se ejecuta antes del controller y retorna automáticamente 400 con los mensajes de error.

## Logging

Serilog está configurado con dos sinks:

1. **Console**: Para desarrollo y debugging en tiempo real.
2. **MSSqlServer**: Los logs se persisten en la tabla `Logs` de la base de datos `JujuTest`, con campos para Message, Level, TimeStamp, Exception y Properties.

Nivel mínimo: `Warning` (configurable en `appsettings.json`).
