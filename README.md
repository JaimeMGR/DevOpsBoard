# DevOpsBoard

Backend de una plataforma de gestión de proyectos orientada a equipos de desarrollo de software.

DevOpsBoard nace como un proyecto personal de portfolio para desarrollar y demostrar experiencia práctica en tecnologías backend modernas, arquitectura por capas, persistencia relacional, autenticación, autorización, testing y, posteriormente, prácticas de DevOps y CI/CD.

El objetivo del proyecto es construir progresivamente una aplicación similar, conceptualmente, a una combinación simplificada de herramientas como Jira, Linear o Trello, pero centrada en las necesidades de equipos de desarrollo.

> 🚧 **Estado del proyecto:** En desarrollo activo.

---

## Contenido

- [Descripción](#descripción)
- [Objetivos](#objetivos)
- [Características actuales](#características-actuales)
- [Roadmap](#roadmap)
- [Stack tecnológico](#stack-tecnológico)
- [Arquitectura](#arquitectura)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Modelo de dominio actual](#modelo-de-dominio-actual)
- [Autenticación](#autenticación)
- [Autorización](#autorización)
- [API](#api)
- [OpenAPI](#openapi)
- [Base de datos](#base-de-datos)
- [Migraciones](#migraciones)
- [Docker](#docker)
- [Configuración del entorno](#configuración-del-entorno)
- [Puesta en marcha](#puesta-en-marcha)
- [Tests](#tests)
- [Flujo de una petición](#flujo-de-una-petición)
- [Principios y decisiones de diseño](#principios-y-decisiones-de-diseño)
- [Próximos pasos](#próximos-pasos)
- [Autor](#autor)

---

# Descripción

DevOpsBoard es una API REST construida con ASP.NET Core y .NET 10.

La aplicación está diseñada para gestionar usuarios, equipos de desarrollo, proyectos y miembros, incorporando un sistema de autenticación y autorización basado en ASP.NET Core Identity y JWT.

La arquitectura está dividida en varias capas con responsabilidades diferenciadas:

```text
┌───────────────────────┐
│     DevOpsBoard.Api   │
│      HTTP / API       │
└───────────┬───────────┘
            │
            ▼
┌───────────────────────┐
│ DevOpsBoard.Application│
│     Use cases         │
│ DTOs / Services       │
└───────────┬───────────┘
            │
            ▼
┌───────────────────────┐
│   DevOpsBoard.Domain  │
│ Entities / Enums      │
│ Business rules        │
└───────────┬───────────┘
            │
            ▲
┌───────────┴───────────┐
│ DevOpsBoard.Infrastructure
│ EF Core / PostgreSQL  │
│ Identity / JWT        │
└───────────────────────┘
```

El objetivo de esta separación es evitar concentrar toda la lógica de negocio en los controllers y mantener una estructura preparada para crecer a medida que se añaden nuevas funcionalidades.

---

# Objetivos

Los principales objetivos técnicos de DevOpsBoard son:

- Diseñar una API REST escalable y mantenible.
- Utilizar una arquitectura por capas.
- Aplicar principios de separación de responsabilidades.
- Utilizar Entity Framework Core como ORM.
- Utilizar PostgreSQL como sistema de persistencia.
- Utilizar Docker para el entorno de desarrollo.
- Implementar autenticación mediante ASP.NET Core Identity y JWT.
- Implementar autorización mediante roles globales.
- Implementar autorización contextual basada en recursos.
- Mantener las reglas de negocio dentro del dominio y de la capa de aplicación.
- Utilizar DTOs para separar el dominio de los contratos HTTP.
- Implementar manejo global de excepciones.
- Utilizar `ProblemDetails` para respuestas de error consistentes.
- Escribir tests automatizados.
- Añadir posteriormente integración continua y despliegue automatizado.
- Utilizar el proyecto como entorno de aprendizaje y experimentación con buenas prácticas backend y DevOps.

---

# Características actuales

Actualmente el proyecto dispone de una primera versión funcional del backend.

## Usuarios

Se utiliza ASP.NET Core Identity para gestionar usuarios.

Actualmente se soporta:

- Registro.
- Inicio de sesión.
- Validación de contraseña.
- Usuario activo/inactivo.
- Consulta del usuario autenticado.
- Roles globales.
- Persistencia de usuarios en PostgreSQL.

Endpoint principal:

```http
POST /api/auth/register
POST /api/auth/login
GET  /api/auth/me
```

---

## Autenticación JWT

Tras iniciar sesión correctamente, la API genera un JSON Web Token (JWT).

El token contiene información relacionada con la identidad del usuario y sus roles.

Flujo:

```text
Usuario
   │
   ▼
POST /api/auth/login
   │
   ▼
ASP.NET Core Identity
   │
   ├── Usuario existe
   └── Contraseña válida
   │
   ▼
JWT Token Generator
   │
   ▼
Access Token
```

Las peticiones protegidas utilizan:

```http
Authorization: Bearer <token>
```

---

## Roles globales

Actualmente existen cuatro roles globales:

```text
ADMIN
MANAGER
DEVELOPER
VIEWER
```

Estos roles son gestionados mediante ASP.NET Core Identity.

La aplicación incluye un seeder para crear automáticamente los roles cuando son necesarios.

---

# Equipos

Los equipos representan grupos de usuarios dentro de la plataforma.

Actualmente se puede:

- Crear equipos.
- Listar equipos.
- Consultar un equipo concreto.
- Registrar quién creó un equipo.
- Gestionar miembros.
- Asignar roles dentro del equipo.
- Modificar roles de los miembros.
- Eliminar miembros.
- Aplicar autorización contextual.

Endpoints actuales:

```http
GET  /api/teams
GET  /api/teams/{id}
POST /api/teams
```

---

# Miembros de equipos

La relación entre usuarios y equipos se representa mediante `TeamMember`.

Cada miembro contiene:

```text
TeamId
UserId
Role
JoinedAt
```

Los roles disponibles actualmente son:

```text
Member
Lead
```

Endpoints actuales:

```http
GET    /api/teams/{teamId}/members
POST   /api/teams/{teamId}/members
PATCH  /api/teams/{teamId}/members/{userId}
DELETE /api/teams/{teamId}/members/{userId}
```

La tabla utiliza una clave primaria compuesta:

```text
TeamId + UserId
```

Esto impide que el mismo usuario pueda pertenecer dos veces al mismo equipo.

---

# Autorización contextual

Una de las decisiones de diseño del proyecto es no limitar la autorización exclusivamente a roles globales.

Por ejemplo:

```text
ADMIN
    ↓
Puede gestionar cualquier equipo.

LEAD
    ↓
Puede gestionar miembros de los equipos
    en los que tiene el rol Lead.

MEMBER
    ↓
Puede consultar el equipo,
pero no gestionar sus miembros.
```

Esto permite diferenciar entre:

```text
Rol global
```

y:

```text
Rol dentro de un recurso.
```

Ejemplo:

```text
Usuario
└── TeamMember
      ├── Team = DevOps
      └── Role = Lead
```

Por tanto, un usuario puede ser `Lead` en un equipo y no tener ninguna capacidad especial sobre otro equipo.

---

# Stack tecnológico

## Backend

- .NET 10
- ASP.NET Core
- ASP.NET Core Identity
- JWT Bearer Authentication
- Entity Framework Core 10
- C#

## Base de datos

- PostgreSQL 18

## Infraestructura de desarrollo

- Docker
- Docker Compose

## Testing

- xUnit

## API documentation

- OpenAPI 3.1.1
- OpenAPI nativo de ASP.NET Core

## Control de versiones

- Git
- GitHub

---

# Arquitectura

DevOpsBoard utiliza una arquitectura dividida en cuatro proyectos principales y un proyecto de tests.

```text
DevOpsBoard.Api
        │
        ▼
DevOpsBoard.Application
        │
        ▼
DevOpsBoard.Domain
        ▲
        │
DevOpsBoard.Infrastructure
```

La dependencia conceptual es:

```text
API
 ↓
Application
 ↓
Domain

Infrastructure
 ├── implementa las abstracciones de Application
 └── utiliza Domain
```

El dominio no depende directamente de Entity Framework Core, PostgreSQL ni ASP.NET Identity.

---

# DevOpsBoard.Api

Responsabilidad principal:

- Exponer endpoints HTTP.
- Recibir requests.
- Devolver responses.
- Gestionar autenticación y autorización HTTP.
- Configurar ASP.NET Core.
- Exponer OpenAPI.
- Registrar middleware.

Actualmente contiene:

```text
Controllers/
Extensions/
Program.cs
Properties/
appsettings.json
```

Controllers principales:

```text
AuthController
HealthController
TeamsController
TeamMembersController
```

---

# DevOpsBoard.Application

Contiene la lógica de aplicación y las abstracciones necesarias para ejecutar los casos de uso.

Actualmente contiene:

```text
Abstractions/
DTOs/
Exceptions/
Security/
Services/
```

Ejemplos:

```text
ITeamRepository
ITeamService
ITeamMemberRepository
ITeamMemberService
IUserRepository
ITeamAuthorizationService
```

Servicios:

```text
TeamService
TeamMemberService
```

---

# DevOpsBoard.Domain

Contiene las entidades y reglas fundamentales del dominio.

Actualmente:

```text
Entities/
├── Project.cs
├── ProjectMember.cs
├── Team.cs
└── TeamMember.cs

Enums/
├── ProjectRole.cs
└── TeamRole.cs
```

La capa de dominio intenta mantenerse independiente de frameworks de infraestructura.

Ejemplo:

```csharp
public class Team
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? CreatedByUserId { get; private set; }

    public DateTime CreatedAt { get; private set; }
}
```

Las propiedades utilizan setters privados para reducir modificaciones arbitrarias desde fuera de la entidad.

---

# DevOpsBoard.Infrastructure

Contiene implementaciones relacionadas con:

- Persistencia.
- Entity Framework Core.
- PostgreSQL.
- ASP.NET Identity.
- JWT.
- Autorización.
- Repositorios.

Estructura actual:

```text
Infrastructure/
├── Authorization/
├── Identity/
└── Persistence/
    ├── Configurations/
    ├── Migrations/
    └── Repositories/
```

---

# Modelo de dominio actual

El modelo actual puede representarse de forma simplificada así:

```text
                ┌───────────────┐
                │   User        │
                │ AspNetUsers   │
                └───────┬───────┘
                        │
             ┌──────────┴──────────┐
             │                     │
             ▼                     ▼
       ┌───────────┐        ┌────────────┐
       │TeamMember │        │ProjectMember
       └─────┬─────┘        └──────┬─────┘
             │                     │
             ▼                     ▼
       ┌───────────┐        ┌────────────┐
       │   Team    │        │  Project   │
       └───────────┘        └────────────┘
```

Actualmente existen las siguientes relaciones principales:

```text
ApplicationUser
      │
      ├── TeamMember ──> Team
      │
      └── ProjectMember ──> Project
```

Además:

```text
Project.OwnerId ──> AspNetUsers.Id
Team.CreatedByUserId ──> AspNetUsers.Id
```

---

# Autenticación

La autenticación utiliza dos componentes principales:

```text
ASP.NET Core Identity
        +
JWT Bearer
```

Identity es responsable de:

- Crear usuarios.
- Gestionar contraseñas.
- Gestionar roles.
- Validar credenciales.
- Persistir información de usuarios.

JWT es responsable de transportar la identidad del usuario entre peticiones.

---

# Autorización

La autorización combina dos niveles.

## Autorización global

Gestionada mediante roles de Identity:

```text
ADMIN
MANAGER
DEVELOPER
VIEWER
```

Ejemplo:

```csharp
[Authorize(Roles = RoleNames.Admin)]
```

## Autorización contextual

Gestionada mediante información específica del recurso.

Ejemplo:

```text
Usuario
  ↓
TeamMember
  ↓
TeamRole = Lead
  ↓
Team específico
```

La aplicación puede comprobar así si el usuario tiene permisos sobre un equipo concreto.

---

# Manejo de errores

DevOpsBoard utiliza un handler global de excepciones.

```text
Exception
    │
    ▼
GlobalExceptionHandler
    │
    ▼
ProblemDetails
```

Las excepciones de aplicación se traducen a códigos HTTP.

Actualmente:

```text
ValidationException
→ 400 Bad Request

NotFoundException
→ 404 Not Found

ConflictException
→ 409 Conflict

UnauthorizedAccessException
→ 401 Unauthorized

ForbiddenException
→ 403 Forbidden

Otras excepciones
→ 500 Internal Server Error
```

Esto evita duplicar bloques `try/catch` en todos los controllers.

---

# API

## Health check

```http
GET /api/health
```

Respuesta:

```json
{
  "status": "ok",
  "service": "DevOpsBoard API"
}
```

---

# Authentication

## Register

```http
POST /api/auth/register
Content-Type: application/json
```

Ejemplo:

```json
{
  "email": "user@example.local",
  "password": "Example123!",
  "displayName": "Example User"
}
```

Respuesta:

```json
{
  "accessToken": "<JWT>",
  "expiresAt": "2026-09-11T20:00:00Z"
}
```

---

## Login

```http
POST /api/auth/login
Content-Type: application/json
```

Ejemplo:

```json
{
  "email": "user@example.local",
  "password": "Example123!"
}
```

---

## Current user

```http
GET /api/auth/me
Authorization: Bearer <JWT>
```

Ejemplo de respuesta:

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "email": "user@example.local",
  "displayName": "Example User",
  "roles": [
    "ADMIN"
  ]
}
```

---

# Teams API

## Obtener equipos

```http
GET /api/teams
Authorization: Bearer <JWT>
```

---

## Obtener un equipo

```http
GET /api/teams/{id}
Authorization: Bearer <JWT>
```

---

## Crear equipo

```http
POST /api/teams
Authorization: Bearer <JWT>
Content-Type: application/json
```

Ejemplo:

```json
{
  "name": "Backend",
  "description": "Equipo encargado del backend"
}
```

El creador del equipo no se recibe desde el cliente.

La aplicación obtiene el identificador del usuario autenticado desde el JWT:

```text
JWT
 ↓
ClaimTypes.NameIdentifier
 ↓
UserId
 ↓
Team.CreatedByUserId
```

---

# Team Members API

## Listar miembros

```http
GET /api/teams/{teamId}/members
Authorization: Bearer <JWT>
```

Ejemplo de respuesta:

```json
[
  {
    "userId": "00000000-0000-0000-0000-000000000000",
    "displayName": "Example User",
    "email": "user@example.local",
    "role": "Lead",
    "joinedAt": "2026-09-11T17:00:00Z"
  }
]
```

---

## Añadir miembro

```http
POST /api/teams/{teamId}/members
Authorization: Bearer <JWT>
Content-Type: application/json
```

Ejemplo:

```json
{
  "userId": "00000000-0000-0000-0000-000000000000",
  "role": "Member"
}
```

Roles disponibles:

```text
Member
Lead
```

---

## Modificar rol

```http
PATCH /api/teams/{teamId}/members/{userId}
Authorization: Bearer <JWT>
Content-Type: application/json
```

Ejemplo:

```json
{
  "role": "Lead"
}
```

---

## Eliminar miembro

```http
DELETE /api/teams/{teamId}/members/{userId}
Authorization: Bearer <JWT>
```

Respuesta correcta:

```http
204 No Content
```

---

# OpenAPI

La API utiliza el sistema OpenAPI integrado en ASP.NET Core.

El documento está disponible en desarrollo mediante:

```text
http://localhost:5080/openapi/v1.json
```

Este documento describe automáticamente los endpoints y esquemas detectados en la API.

Actualmente incluye operaciones relacionadas con:

```text
Auth
Health
Teams
Team Members
```

---

# Base de datos

La aplicación utiliza PostgreSQL.

Durante el desarrollo se ejecuta mediante Docker Compose.

La base de datos contiene actualmente tablas de:

## ASP.NET Identity

```text
AspNetUsers
AspNetRoles
AspNetUserClaims
AspNetUserLogins
AspNetUserRoles
AspNetUserTokens
AspNetRoleClaims
```

## DevOpsBoard

```text
teams
projects
team_members
project_members
```

## Entity Framework Core

```text
__EFMigrationsHistory
```

---

# Relaciones principales de la base de datos

Simplificación del modelo:

```text
AspNetUsers
    │
    ├───────────────┐
    │               │
    ▼               ▼
teams          projects
    │               │
    ▼               ▼
team_members   project_members
    │               │
    └───────┬───────┘
            │
        AspNetUsers
```

`TeamMember` utiliza:

```text
PK(TeamId, UserId)
```

`ProjectMember` utiliza:

```text
PK(ProjectId, UserId)
```

De esta forma se evita la duplicación de membresías.

---

# Migraciones

Entity Framework Core se utiliza para gestionar la evolución del esquema de la base de datos.

Las migraciones actuales incluyen:

```text
InitialIdentity
AddTeamsAndProjects
AddMemberships
AddMembershipRoles
RemoveMembershipRoleDefaults
FixMembershipRoleDefaults
AddTeamCreator
```

Para listar las migraciones:

```powershell
dotnet ef migrations list `
  --project DevOpsBoard.Infrastructure `
  --startup-project DevOpsBoard.Api
```

Para aplicar las migraciones:

```powershell
dotnet ef database update `
  --project DevOpsBoard.Infrastructure `
  --startup-project DevOpsBoard.Api
```

Para crear una nueva migración:

```powershell
dotnet ef migrations add NombreDeLaMigracion `
  --project DevOpsBoard.Infrastructure `
  --startup-project DevOpsBoard.Api `
  --output-dir Persistence\Migrations
```

---

# Docker

PostgreSQL se ejecuta mediante Docker Compose.

Archivo:

```text
infra/docker-compose.yml
```

Para iniciar la infraestructura:

```powershell
cd infra
docker compose up -d
```

Para comprobar los contenedores:

```powershell
docker ps
```

Actualmente el servicio principal es:

```text
devopsboard-postgres
```

PostgreSQL está expuesto localmente mediante:

```text
localhost:5432
```

Para acceder directamente a PostgreSQL:

```powershell
docker exec -it devopsboard-postgres psql `
  -U devopsboard `
  -d devopsboard
```

---

# Configuración del entorno

Las credenciales locales no se almacenan en el repositorio.

El proyecto utiliza dos mecanismos distintos:

```text
Docker / PostgreSQL
        ↓
infra/.env

ASP.NET Core
        ↓
.NET User Secrets
```

El archivo:

```text
infra/.env
```

no debe subirse a Git.

Se proporciona:

```text
infra/.env.example
```

como plantilla.

---

# User Secrets

Las credenciales y claves utilizadas por la API durante el desarrollo se almacenan mediante .NET User Secrets.

Ejemplo:

```powershell
dotnet user-secrets set `
  "ConnectionStrings:DevOpsBoard" `
  "Host=localhost;Port=5432;Database=devopsboard;Username=devopsboard;Password=<LOCAL_PASSWORD>" `
  --project DevOpsBoard.Api
```

Configuración JWT:

```powershell
dotnet user-secrets set `
  "Jwt:Key" `
  "<GENERATED_SECRET_KEY>" `
  --project DevOpsBoard.Api
```

```powershell
dotnet user-secrets set `
  "Jwt:Issuer" `
  "DevOpsBoard.Api" `
  --project DevOpsBoard.Api
```

```powershell
dotnet user-secrets set `
  "Jwt:Audience" `
  "DevOpsBoard.Web" `
  --project DevOpsBoard.Api
```

Email del administrador de desarrollo:

```powershell
dotnet user-secrets set `
  "Identity:AdminEmail" `
  "admin@example.local" `
  --project DevOpsBoard.Api
```

Para comprobar los secretos configurados:

```powershell
dotnet user-secrets list --project DevOpsBoard.Api
```

> Nunca se deben añadir claves JWT, contraseñas de bases de datos u otros secretos reales al repositorio.

---

# Puesta en marcha

## Requisitos

Necesitas tener instalado:

- .NET 10 SDK
- Docker Desktop
- Git
- PowerShell o una terminal compatible

Comprobación:

```powershell
dotnet --version
docker --version
docker compose version
git --version
```

---

## 1. Clonar el repositorio

```powershell
git clone https://github.com/JaimeMGR/DevOpsBoard.git
cd DevOpsBoard
```

---

## 2. Iniciar PostgreSQL

```powershell
cd infra
docker compose up -d
```

Comprobar:

```powershell
docker ps
```

Debe aparecer el contenedor:

```text
devopsboard-postgres
```

---

## 3. Configurar User Secrets

Desde `apps`:

```powershell
cd ..\apps
```

Inicializar User Secrets:

```powershell
dotnet user-secrets init --project DevOpsBoard.Api
```

Configurar la conexión:

```powershell
dotnet user-secrets set `
  "ConnectionStrings:DevOpsBoard" `
  "Host=localhost;Port=5432;Database=devopsboard;Username=devopsboard;Password=<LOCAL_PASSWORD>" `
  --project DevOpsBoard.Api
```

Configurar JWT:

```powershell
dotnet user-secrets set `
  "Jwt:Key" `
  "<GENERATED_SECRET_KEY>" `
  --project DevOpsBoard.Api
```

```powershell
dotnet user-secrets set `
  "Jwt:Issuer" `
  "DevOpsBoard.Api" `
  --project DevOpsBoard.Api
```

```powershell
dotnet user-secrets set `
  "Jwt:Audience" `
  "DevOpsBoard.Web" `
  --project DevOpsBoard.Api
```

---

## 4. Restaurar dependencias

```powershell
dotnet restore
```

---

## 5. Compilar

```powershell
dotnet build DevOpsBoard.slnx
```

---

## 6. Aplicar migraciones

```powershell
dotnet ef database update `
  --project DevOpsBoard.Infrastructure `
  --startup-project DevOpsBoard.Api
```

---

## 7. Ejecutar la API

```powershell
dotnet run --project DevOpsBoard.Api
```

Por defecto, durante el desarrollo:

```text
http://localhost:5080
```

---

# Tests

Los tests se encuentran en:

```text
apps/DevOpsBoard.Tests/
```

Actualmente se utiliza xUnit.

Ejecutar todos los tests:

```powershell
dotnet test DevOpsBoard.slnx
```

Actualmente el proyecto contiene tests unitarios para los servicios relacionados con equipos y miembros de equipos.

Las pruebas cubren, entre otros casos:

```text
TeamService
├── creación de equipos
├── validación del nombre
└── equipos duplicados

TeamMemberService
├── añadir miembros
├── duplicados
├── autorización
├── modificación de roles
└── eliminación de miembros
```

Una de las decisiones del proyecto es que los tests de lógica de aplicación no dependan necesariamente de PostgreSQL.

Para ello se utilizan implementaciones fake de las abstracciones correspondientes.

Esto permite probar la lógica de negocio de forma rápida y aislada.

---

# Flujo de una petición

Un ejemplo de una petición para crear un equipo:

```text
HTTP Request
     │
     ▼
TeamsController
     │
     ▼
ITeamService
     │
     ▼
TeamService
     │
     ├── Validación
     ├── Comprobación de duplicados
     └── Creación de la entidad
     │
     ▼
ITeamRepository
     │
     ▼
TeamRepository
     │
     ▼
Entity Framework Core
     │
     ▼
PostgreSQL
```

En operaciones autenticadas:

```text
HTTP Request
     │
     ▼
JWT Bearer Authentication
     │
     ▼
Authorization
     │
     ▼
Controller
     │
     ▼
Application
     │
     ▼
Infrastructure
     │
     ▼
PostgreSQL
```

---

# Flujo de autenticación

```text
                 ┌────────────────────┐
                 │      Cliente       │
                 └─────────┬──────────┘
                           │
                           │ POST /login
                           ▼
                 ┌────────────────────┐
                 │   AuthController   │
                 └─────────┬──────────┘
                           │
                           ▼
                 ┌────────────────────┐
                 │    AuthService     │
                 └─────────┬──────────┘
                           │
                           ▼
                 ┌────────────────────┐
                 │ ASP.NET Identity   │
                 └─────────┬──────────┘
                           │
                      credenciales OK
                           │
                           ▼
                 ┌────────────────────┐
                 │ JwtTokenGenerator  │
                 └─────────┬──────────┘
                           │
                           ▼
                       JWT Token
```

Posteriormente:

```text
Client
  │
  │ Authorization: Bearer <JWT>
  ▼
JWT Authentication
  │
  ▼
ClaimsPrincipal
  │
  ├── UserId
  ├── Email
  └── Role
  │
  ▼
Authorization
  │
  ▼
Controller
```

---

# Principios y decisiones de diseño

## Separación entre Domain e Infrastructure

Las entidades del dominio no dependen directamente de:

```text
Entity Framework Core
PostgreSQL
ASP.NET Identity
JWT
```

Esto permite mantener las reglas principales del dominio independientes de la tecnología de persistencia.

---

## DTOs

Los DTOs permiten separar:

```text
Modelo interno
```

de:

```text
Contrato de la API
```

Por ejemplo:

```text
Team
```

no se expone directamente como entidad de persistencia en las respuestas HTTP.

En su lugar se utiliza:

```text
TeamDto
```

---

## Interfaces en Application

Application define abstracciones como:

```text
ITeamRepository
ITeamMemberRepository
IUserRepository
```

Infrastructure proporciona las implementaciones concretas.

Esto permite que la capa de aplicación no tenga que conocer detalles de PostgreSQL o EF Core.

---

## Repositorios

La lógica de acceso a datos se mantiene en Infrastructure.

Ejemplo:

```text
ITeamRepository
        │
        ▼
TeamRepository
        │
        ▼
DevOpsBoardDbContext
        │
        ▼
PostgreSQL
```

---

## Excepciones de aplicación

En lugar de utilizar únicamente excepciones genéricas, el proyecto define excepciones propias:

```text
ValidationException
NotFoundException
ConflictException
ForbiddenException
```

Esto permite mapearlas correctamente a HTTP.

---

## Autorización contextual

La autorización no se limita a:

```text
¿Eres ADMIN?
```

También puede preguntar:

```text
¿Eres Lead de este equipo concreto?
```

Esto permite construir políticas de acceso más realistas.

---

# Roadmap

El proyecto está siendo construido progresivamente.

## ✅ Completado

```text
[✓] Estructura inicial de la solución
[✓] .NET 10
[✓] ASP.NET Core API
[✓] Docker Compose
[✓] PostgreSQL
[✓] Entity Framework Core
[✓] Migraciones
[✓] ASP.NET Core Identity
[✓] Registro de usuarios
[✓] Login
[✓] JWT
[✓] Roles globales
[✓] Seeder de Identity
[✓] Teams
[✓] Team Members
[✓] Roles de equipo
[✓] Autorización basada en equipo
[✓] Global Exception Handler
[✓] ProblemDetails
[✓] OpenAPI
[✓] Tests unitarios
```

## 🚧 En desarrollo / próximos bloques

```text
[ ] Projects API
[ ] Project Members
[ ] Project roles
[ ] Project-level authorization
[ ] Issues / Tasks
[ ] Estados de tareas
[ ] Prioridades
[ ] Asignación de usuarios
[ ] Labels
[ ] Comentarios
[ ] Historial de cambios
[ ] Paginación
[ ] Filtros
[ ] Ordenación
```

## 🚀 Fase DevOps

```text
[ ] Dockerización completa de la aplicación
[ ] Health checks
[ ] GitHub Actions
[ ] CI
[ ] Ejecución automática de tests
[ ] Build automatizado
[ ] Docker image
[ ] CD
[ ] Gestión de variables de entorno
[ ] Observabilidad
[ ] Logs estructurados
[ ] Despliegue
```

## 🎨 Fase frontend

Está prevista una interfaz web para consumir la API.

Posibles tecnologías:

```text
React
TypeScript
Vite
```

La interfaz se conectará con la API mediante JWT y permitirá gestionar usuarios, equipos, proyectos y tareas.

---

# Estructura actual del repositorio

```text
DevOpsBoard/
│
├── .gitignore
├── README.md
│
├── apps/
│   │
│   ├── DevOpsBoard.Api/
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs
│   │   │   ├── HealthController.cs
│   │   │   ├── TeamMembersController.cs
│   │   │   └── TeamsController.cs
│   │   │
│   │   ├── Extensions/
│   │   │   ├── AuthenticationExtensions.cs
│   │   │   └── GlobalExceptionHandler.cs
│   │   │
│   │   ├── Properties/
│   │   │   └── launchSettings.json
│   │   │
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── appsettings.Development.json
│   │
│   ├── DevOpsBoard.Application/
│   │   ├── Abstractions/
│   │   ├── DTOs/
│   │   ├── Exceptions/
│   │   ├── Security/
│   │   └── Services/
│   │
│   ├── DevOpsBoard.Domain/
│   │   ├── Entities/
│   │   └── Enums/
│   │
│   ├── DevOpsBoard.Infrastructure/
│   │   ├── Authorization/
│   │   ├── Identity/
│   │   └── Persistence/
│   │       ├── Configurations/
│   │       ├── Migrations/
│   │       └── Repositories/
│   │
│   ├── DevOpsBoard.Tests/
│   │   ├── TeamMemberServiceTests.cs
│   │   └── TeamServiceTests.cs
│   │
│   └── DevOpsBoard.slnx
│
├── docs/
│
└── infra/
    ├── docker-compose.yml
    └── .env.example
```

---

# Calidad y estado del proyecto

El objetivo del desarrollo es mantener el repositorio en un estado que pueda compilar y probarse en cualquier momento.

Comprobación de build:

```powershell
dotnet build DevOpsBoard.slnx
```

Comprobación de tests:

```powershell
dotnet test DevOpsBoard.slnx
```

El proyecto actualmente cuenta con una suite de tests unitarios para la lógica de equipos y miembros de equipos.

---

# Seguridad

Este proyecto utiliza varias medidas para evitar exponer credenciales accidentalmente.

## Secretos

Los secretos locales se almacenan utilizando:

```text
.NET User Secrets
```

y:

```text
infra/.env
```

No deben almacenarse en Git.

---

## JWT

La clave utilizada para firmar JWT debe mantenerse fuera del repositorio.

En desarrollo se utiliza User Secrets.

En producción, la intención es utilizar un sistema de gestión de secretos adecuado para el entorno de despliegue.

---

# Estado del proyecto

DevOpsBoard se encuentra actualmente en una fase temprana de desarrollo.

La base del backend ya está implementada:

```text
Identity
    ↓
Authentication
    ↓
Authorization
    ↓
Teams
    ↓
Team Members
    ↓
Resource-based authorization
    ↓
Testing
```

La siguiente gran fase consiste en desarrollar:

```text
Projects
    ↓
Project Members
    ↓
Issues
    ↓
Workflow
    ↓
CI/CD
```

El proyecto se desarrolla incrementalmente, manteniendo el repositorio compilable y los tests funcionando a medida que se introducen nuevas funcionalidades.

---

# Autor

**Jaime Molina Granados**

Desarrollador Full Stack y desarrollador de videojuegos.

### Portfolio

https://jaime-molina-granados.vercel.app

### GitHub

https://github.com/JaimeMGR

---

# Licencia

Este proyecto se encuentra actualmente en desarrollo.

La licencia definitiva del repositorio se definirá posteriormente.