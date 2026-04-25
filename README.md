# ProjectAPI

API REST para gestión de Posts y Customers. Desarrollada en .NET 8 con arquitectura limpia.

## Stack

- **.NET 8** + Entity Framework Core
- **SQL Server** como base de datos
- **Serilog** para logging
- **Swagger** para documentación de endpoints

## Estructura del proyecto

```
├── API/ # Controllers, middleware
├── Application/ # Services, DTOs, lógica de negocio
├── Domain/ # Entidades y contratos (interfaces de repositorio)
├── Infrastructure/ # DbContext, implementaciones de repositorio
```

## Configuración

1. Ejecutar `JujuTests.Script.sql` para tener la base de datos completas, con modificaciones que se realizaron
2. Si se desean hacer pruebas se creo archivo `requests.http` para hacer pruebas de todos los endpoints

## Ajustes

- El API incluye middleware de manejo centralizado de excepciones
- Se actualizo version de .NET
- Se opto por cambiar la estructura del proyecto
- Se implemento interfaz para capas del repositorio
- Se implemento auto mapeo de las propiedades
- Se opto por usar async/await en los endpoints
