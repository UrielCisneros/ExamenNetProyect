# Biblioteca Virtual API

REST API para la gestión de documentos, videos, audios, usuarios y carpetas en una biblioteca digital universitaria. Construida con ASP.NET Core siguiendo Clean Architecture.

---

## Tecnologías

- **.NET 10** — ASP.NET Core Web API
- **Entity Framework Core** con **SQLite** (`biblioteca.db`)
- **JWT Bearer** para autenticación
- **BCrypt** para hash de contraseñas
- **FluentValidation** para validación de DTOs
- **Swagger / OpenAPI** en ambiente de desarrollo

---

## Arquitectura

El proyecto sigue **Clean Architecture** con cuatro capas:

```
BibliotecaVirtual.Domain          → Entidades, interfaces, enums, excepciones de dominio
BibliotecaVirtual.Application     → Servicios, DTOs, validadores, contratos
BibliotecaVirtual.Infrastructure  → EF Core, repositorios, JWT, BCrypt, almacenamiento, background services
BibliotecaVirtual.API             → Controllers, middlewares, configuración del pipeline
```

---

## Requisitos previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- No requiere servidor de base de datos externo (SQLite embebido)

---

## Levantar el proyecto

### Desde la terminal

```bash
# Desde la raíz del repositorio
dotnet run --project src/BibliotecaVirtual.API
```

La API queda disponible en `http://localhost:5080`.  
En modo Development también se expone HTTPS en `https://localhost:7080`.

### Desde Visual Studio / Rider

Abrir `BibliotecaVirtual.sln` y ejecutar el proyecto `BibliotecaVirtual.API`.

### Otros comandos útiles

```bash
# Compilar la solución completa
dotnet build

# Restaurar dependencias
dotnet restore

# Publicar (producción)
dotnet publish src/BibliotecaVirtual.API -c Release -o ./publish
```

---

## Base de datos

Al iniciar la aplicación, EF Core aplica migraciones automáticamente (o crea el esquema si no hay migraciones). También ejecuta un **seed** con el usuario administrador inicial:

| Campo    | Valor                    |
|----------|--------------------------|
| Correo   | `admin@biblioteca.local` |
| Password | `Admin123!`              |
| Perfil   | AdministracionBiblioteca |

El archivo `biblioteca.db` se genera en el directorio de trabajo al primer arranque.

---

## Configuración (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=biblioteca.db"
  },
  "Almacenamiento": {
    "RutaBase": "Storage",
    "TamanoMaxArchivoMB": 25,
    "TamanoMaxMultimediaMB": 200,
    "MinutosParaEliminacionFisica": 2
  },
  "Jwt": {
    "Issuer": "BibliotecaVirtual",
    "Audience": "BibliotecaVirtualUsuarios",
    "Key": "CAMBIAR-ESTA-LLAVE-EN-PRODUCCION-32CARACTERES-MINIMO",
    "ExpiracionMinutos": 240
  }
}
```

> En producción cambia el valor de `Jwt.Key` por una cadena segura de al menos 32 caracteres.

---

## Perfiles de usuario

| Perfil                    | Descripción                                                     |
|---------------------------|-----------------------------------------------------------------|
| `AdministracionBiblioteca`| Acceso total: usuarios, carpetas, archivos y logs               |
| `GerenteUniversidad`      | Acceso a usuarios y logs; puede gestionar carpetas y archivos   |
| `AsistenteBiblioteca`     | Puede gestionar carpetas/archivos si tiene el permiso adicional |
| `Empleado`                | Sólo puede navegar y descargar archivos                         |

---

## Tipos de archivo soportados

| Tipo      | Extensiones                       | Límite   |
|-----------|-----------------------------------|----------|
| Documento | `.pdf`, `.docx`, `.pptx`, `.xlsx` | 25 MB    |
| Video     | `.mov`, `.avi`                    | 200 MB   |
| Audio     | `.mp3`                            | 200 MB   |

---

## Endpoints principales

### Autenticación

| Método | Ruta             | Descripción              | Auth requerida |
|--------|------------------|--------------------------|----------------|
| POST   | `/api/auth/login`| Inicia sesión, retorna JWT | No           |

### Usuarios

| Método | Ruta                                   | Descripción                               | Roles permitidos              |
|--------|----------------------------------------|-------------------------------------------|-------------------------------|
| GET    | `/api/usuarios`                        | Listar usuarios                           | Admin, Gerente                |
| GET    | `/api/usuarios/{correo}`               | Obtener usuario por correo                | Admin, Gerente                |
| POST   | `/api/usuarios`                        | Crear usuario                             | Admin, Gerente                |
| PATCH  | `/api/usuarios/{correo}/inactivar`     | Inactivar usuario                         | Admin, Gerente                |
| PATCH  | `/api/usuarios/{correo}/reactivar`     | Reactivar usuario                         | Admin, Gerente                |
| PATCH  | `/api/usuarios/{correo}/permisos`      | Otorgar/revocar permiso de gestión        | Admin, Gerente                |
| GET    | `/api/usuarios/{correo}/historial`     | Historial de acciones del usuario         | Admin, Gerente                |

### Carpetas

| Método | Ruta                        | Descripción                              | Auth requerida |
|--------|-----------------------------|------------------------------------------|----------------|
| GET    | `/api/carpetas/raices`      | Carpetas raíz del sistema                | Si             |
| GET    | `/api/carpetas/arbol`       | Árbol completo con conteo de archivos    | Si             |
| GET    | `/api/carpetas/{id}`        | Obtener carpeta por ID                   | Si             |
| GET    | `/api/carpetas/{id}/hijas`  | Subcarpetas de una carpeta               | Si             |
| POST   | `/api/carpetas`             | Crear carpeta                            | Si (gestión)   |
| PATCH  | `/api/carpetas/{id}`        | Renombrar carpeta                        | Si (gestión)   |
| DELETE | `/api/carpetas/{id}`        | Eliminar carpeta                         | Si (gestión)   |

### Archivos

| Método | Ruta                             | Descripción                                          | Auth requerida |
|--------|----------------------------------|------------------------------------------------------|----------------|
| GET    | `/api/archivos/carpeta/{id}`     | Listar archivos activos de una carpeta               | Si             |
| GET    | `/api/archivos/{id}`             | Obtener archivo por ID                               | Si             |
| POST   | `/api/archivos/subir`            | Subir archivo (`multipart/form-data`)                | Si (gestión)   |
| PATCH  | `/api/archivos/{id}`             | Editar nombre y descripción                          | Si (gestión)   |
| DELETE | `/api/archivos/{id}`             | Eliminación lógica (borrado físico a los 2 min)      | Si (gestión)   |
| GET    | `/api/archivos/{id}/descargar`   | Descargar el archivo físico                          | Si             |

### Logs

| Método | Ruta        | Descripción                                                          | Roles permitidos |
|--------|-------------|----------------------------------------------------------------------|------------------|
| GET    | `/api/logs` | Consulta paginada con filtros por correo, entidad, acción y fechas  | Admin, Gerente   |

### Health check

| Método | Ruta           | Descripción        | Auth requerida |
|--------|----------------|--------------------|----------------|
| GET    | `/api/health`  | Estado de la API   | No             |

---

## Swagger

En ambiente de desarrollo, la documentación interactiva está disponible en:

```
http://localhost:5080/swagger
```

Para autenticarse en Swagger: clic en **Authorize** e ingresa `Bearer <token>`.

---

## Archivo de pruebas HTTP

El archivo [`requests.http`](requests.http) contiene ejemplos listos para usar con **VS Code REST Client** o **Rider HTTP Client**. Cubre todos los flujos principales: login, gestión de usuarios, carpetas, archivos y logs.

---

## Proceso de limpieza en segundo plano

Un `BackgroundService` revisa cada 30 segundos los archivos eliminados lógicamente. Aquellos cuya fecha de eliminación supera los **2 minutos** (configurable en `MinutosParaEliminacionFisica`) son borrados físicamente del disco y registrados en la bitácora del sistema.

---

## Documentación adicional

La carpeta [`documentation/`](documentation/) contiene:

- `1-Documento-Analisis.pdf` — Análisis y requerimientos del sistema
- `2-Documentacion-Tecnica.pdf` — Documentación técnica de la arquitectura
- `3-Manual-Usuario.pdf` — Manual de usuario

