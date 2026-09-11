# DevOpsBoard

Backend de una plataforma de gestión de proyectos orientada a equipos de desarrollo de software.

DevOpsBoard es un proyecto personal de portfolio desarrollado para demostrar experiencia práctica en backend moderno, diseño de APIs REST, arquitectura por capas, persistencia relacional, autenticación, autorización contextual, testing y, progresivamente, prácticas de DevOps y CI/CD.

Conceptualmente, el proyecto busca construir una plataforma inspirada en herramientas como Jira, Linear o Trello, pero simplificada y orientada específicamente a equipos de desarrollo de software.

> 🚧 **Estado del proyecto:** Backend en desarrollo activo.

---

## Contenido

- [Descripción](#descripción)
- [Objetivos](#objetivos)
- [Características actuales](#características-actuales)
- [Stack tecnológico](#stack-tecnológico)
- [Arquitectura](#arquitectura)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Modelo de dominio](#modelo-de-dominio)
- [Autenticación](#autenticación)
- [Autorización](#autorización)
- [Gestión de equipos](#gestión-de-equipos)
- [Gestión de proyectos](#gestión-de-proyectos)
- [Issues](#issues)
- [Comentarios](#comentarios)
- [Historial y auditoría](#historial-y-auditoría)
- [API](#api)
- [OpenAPI](#openapi)
- [Manejo de errores](#manejo-de-errores)
- [Base de datos](#base-de-datos)
- [Migraciones](#migraciones)
- [Docker](#docker)
- [Configuración del entorno](#configuración-del-entorno)
- [Puesta en marcha](#puesta-en-marcha)
- [Testing](#testing)
- [Flujo de una petición](#flujo-de-una-petición)
- [Principios y decisiones de diseño](#principios-y-decisiones-de-diseño)
- [Roadmap](#roadmap)
- [Seguridad](#seguridad)
- [Autor](#autor)
- [Licencia](#licencia)

---

# Descripción

DevOpsBoard es una API REST construida con ASP.NET Core y .NET 10.

La aplicación permite gestionar:

- usuarios;
- equipos;
- miembros de equipos;
- proyectos;
- miembros de proyectos;
- issues;
- asignación de usuarios;
- comentarios;
- historial de cambios;
- autorización basada en recursos.

El proyecto está organizado en una arquitectura por capas con responsabilidades separadas entre API, aplicación, dominio e infraestructura.

La separación principal es:

```text
┌─────────────────────────────┐
│       DevOpsBoard.Api       │
│                             │
│ HTTP / Controllers / JWT    │
│ OpenAPI / Middleware        │
└──────────────┬──────────────┘
               │
               ▼
┌─────────────────────────────┐
│   DevOpsBoard.Application   │
│                             │
│ Use Cases / Services        │
│ DTOs / Abstractions         │
│ Exceptions / Security       │
└──────────────┬──────────────┘
               │
               ▼
┌─────────────────────────────┐
│     DevOpsBoard.Domain      │
│                             │
│ Entities / Enums            │
│ Business Rules              │
└─────────────────────────────┘
               ▲
               │
┌──────────────┴──────────────┐
│ DevOpsBoard.Infrastructure  │
│                             │
│ EF Core / PostgreSQL        │
│ Identity / JWT              │
│ Repositories / AuthZ        │
└─────────────────────────────┘
```

La capa de infraestructura implementa las abstracciones definidas por Application y se encarga de los detalles tecnológicos relacionados con persistencia, Identity, JWT y autorización.

---

# Objetivos

Los principales objetivos técnicos del proyecto son:

- Diseñar una API REST mantenible y preparada para crecer.
- Aplicar una arquitectura por capas.
- Separar la lógica de negocio de la infraestructura.
- Utilizar Entity Framework Core como ORM.
- Utilizar PostgreSQL como sistema de persistencia.
- Utilizar Docker para el entorno de desarrollo.
- Implementar autenticación con ASP.NET Core Identity.
- Utilizar JWT Bearer para autenticación de peticiones.
- Implementar roles globales mediante Identity.
- Implementar autorización contextual basada en recursos.
- Utilizar DTOs como contratos de entrada y salida de la API.
- Mantener las reglas importantes dentro del dominio y de la capa de aplicación.
- Implementar manejo centralizado de excepciones.
- Utilizar `ProblemDetails` para respuestas de error consistentes.
- Mantener una suite de tests automatizados.
- Aplicar migraciones de Entity Framework Core.
- Construir progresivamente una base preparada para CI/CD y observabilidad.

---

# Características actuales

Actualmente existe una primera versión funcional del backend.

Las funcionalidades implementadas son:

```text
Authentication
    ↓
Teams
    ↓
Team Members
    ↓
Projects
    ↓
Project Members
    ↓
Issues
    ├── Assignment
    ├── Status
    ├── Priority
    ├── Soft Delete
    ├── Resource Authorization
    │
    ├── Comments
    │
    └── History / Audit
```

Además, el proyecto dispone de:

```text
JWT Authentication
ASP.NET Core Identity
Global Roles
Resource-based Authorization
PostgreSQL
Entity Framework Core
Docker Compose
OpenAPI
ProblemDetails
Global Exception Handler
Unit Tests
EF Core Migrations
```

La suite actual cuenta con:

```text
85 tests
```

Todos los tests existentes deben mantenerse pasando antes de considerar estable un cambio importante.

---

# Stack tecnológico

## Backend

- C#
- .NET 10
- ASP.NET Core
- ASP.NET Core Identity
- JWT Bearer Authentication
- Entity Framework Core 10

## Base de datos

- PostgreSQL 18

## Infraestructura

- Docker
- Docker Compose

## Testing

- xUnit

## API documentation

- OpenAPI
- OpenAPI 3.1.1

## Control de versiones

- Git
- GitHub

---

# Arquitectura

DevOpsBoard utiliza una arquitectura por capas.

```text
DevOpsBoard.Api
        │
        ▼
DevOpsBoard.Application
        │
        ▼
DevOpsBoard.Domain

DevOpsBoard.Infrastructure
        │
        ├── implementa abstracciones de Application
        └── utiliza Domain
```

Las dependencias se mantienen orientadas hacia el dominio.

El dominio no depende directamente de:

```text
Entity Framework Core
PostgreSQL
ASP.NET Core Identity
JWT
```

Esto permite aislar las reglas principales del negocio de los detalles de infraestructura.

---

# DevOpsBoard.Api

Responsabilidades:

- Exponer endpoints HTTP.
- Recibir requests.
- Validar y enlazar modelos HTTP.
- Obtener la identidad del usuario autenticado.
- Devolver responses HTTP.
- Configurar autenticación.
- Configurar OpenAPI.
- Configurar middleware.
- Gestionar el pipeline de ASP.NET Core.

Controllers actuales:

```text
AuthController
HealthController

TeamsController
TeamMembersController

ProjectsController
ProjectMembersController

IssuesController
IssueCommentsController
IssueHistoryController
```

---

# DevOpsBoard.Application

Contiene los casos de uso y las abstracciones que necesita la aplicación.

Estructura:

```text
Application/
├── Abstractions/
├── DTOs/
├── Exceptions/
├── Security/
└── Services/
```

Entre las abstracciones actuales se encuentran:

```text
IAuthService

ITeamRepository
ITeamService
ITeamMemberRepository
ITeamMemberService
ITeamAuthorizationService

IProjectRepository
IProjectService
IProjectAuthorizationService
IProjectMemberRepository
IProjectMemberService
IProjectMemberAuthorizationService

IIssueRepository
IIssueService
IIssueAuthorizationService

IIssueCommentRepository
IIssueCommentService
ICommentAuthorizationService

IIssueHistoryRepository
IIssueHistoryService

IUserRepository
```

Servicios principales:

```text
TeamService
TeamMemberService

ProjectService
ProjectMemberService

IssueService
IssueCommentService
IssueHistoryService
```

---

# DevOpsBoard.Domain

La capa de dominio contiene las entidades y enums fundamentales.

Entidades actuales:

```text
Entities/
├── Issue.cs
├── IssueComment.cs
├── IssueHistory.cs
├── Project.cs
├── ProjectMember.cs
├── Team.cs
└── TeamMember.cs
```

Enums actuales:

```text
Enums/
├── IssueHistoryAction.cs
├── IssuePriority.cs
├── IssueStatus.cs
├── ProjectRole.cs
└── TeamRole.cs
```

Las entidades utilizan setters privados para limitar modificaciones arbitrarias desde otras capas.

Ejemplo simplificado:

```csharp
public class Issue
{
    public Guid Id { get; private set; }

    public Guid ProjectId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public IssueStatus Status { get; private set; }

    public IssuePriority Priority { get; private set; }
}
```

Las modificaciones relevantes se realizan mediante métodos de dominio.

Por ejemplo:

```text
Issue.Update(...)
Issue.ChangeStatus(...)
Issue.ChangePriority(...)
Issue.AssignTo(...)
Issue.Unassign(...)
Issue.Delete(...)
```

En el caso de `IssueComment`:

```text
IssueComment.UpdateContent(...)
```

---

# DevOpsBoard.Infrastructure

La infraestructura contiene las implementaciones concretas de los mecanismos definidos mediante abstracciones en Application.

Responsabilidades actuales:

- Persistencia con Entity Framework Core.
- Acceso a PostgreSQL.
- ASP.NET Core Identity.
- Generación y validación de JWT.
- Repositorios.
- Configuraciones EF Core.
- Autorización contextual.
- Migraciones.

Estructura:

```text
Infrastructure/
├── Authorization/
│
├── Identity/
│
└── Persistence/
    ├── Configurations/
    ├── Migrations/
    └── Repositories/
```

Servicios de autorización actuales:

```text
TeamAuthorizationService
ProjectAuthorizationService
ProjectMemberAuthorizationService
IssueAuthorizationService
CommentAuthorizationService
```

---

# Modelo de dominio

El modelo de dominio actual puede simplificarse de la siguiente manera:

```text
                         ApplicationUser
                               │
                 ┌─────────────┼─────────────┐
                 │             │             │
                 ▼             ▼             ▼
            TeamMember   ProjectMember   Issues / History
                 │             │
                 ▼             ▼
                Team        Project
                               │
                               ▼
                              Issue
                         ┌──────┴──────┐
                         │             │
                         ▼             ▼
                    Comments       History
```

Relaciones principales:

```text
ApplicationUser
    │
    ├── TeamMember ──────> Team
    │
    ├── ProjectMember ───> Project
    │
    ├── ReporterId ──────> Issue
    │
    ├── AssigneeId ──────> Issue
    │
    ├── AuthorId ────────> IssueComment
    │
    └── ActorId ─────────> IssueHistory
```

Además:

```text
Team.CreatedByUserId ──> AspNetUsers.Id

Project.OwnerId ───────> AspNetUsers.Id
```

---

# Autenticación

La autenticación combina:

```text
ASP.NET Core Identity
        +
JWT Bearer Authentication
```

Identity gestiona:

- usuarios;
- contraseñas;
- roles;
- validación de credenciales;
- persistencia de usuarios.

Después de un login correcto, la aplicación genera un JWT.

Flujo:

```text
Cliente
   │
   │ POST /api/auth/login
   ▼
AuthController
   │
   ▼
AuthService
   │
   ▼
ASP.NET Core Identity
   │
   │ credenciales válidas
   ▼
JwtTokenGenerator
   │
   ▼
Access Token
```

Las peticiones protegidas utilizan:

```http
Authorization: Bearer <token>
```

La identidad del usuario se obtiene desde claims del JWT.

La aplicación utiliza principalmente:

```text
ClaimTypes.NameIdentifier
```

para obtener el identificador del usuario autenticado.

---

# Roles globales

La aplicación dispone actualmente de cuatro roles globales:

```text
ADMIN
MANAGER
DEVELOPER
VIEWER
```

Estos roles se gestionan mediante ASP.NET Core Identity.

Los nombres se centralizan en:

```text
apps/DevOpsBoard.Application/Security/RoleNames.cs
```

El proyecto incluye un seeder de Identity encargado de crear los roles cuando no existen.

El rol `ADMIN` se utiliza para otorgar acceso administrativo global.

---

# Autorización

Uno de los aspectos fundamentales de DevOpsBoard es la combinación de roles globales con autorización contextual.

No se comprueba únicamente:

```text
¿El usuario tiene el rol ADMIN?
```

También puede comprobarse:

```text
¿El usuario es miembro de este proyecto?

¿Qué ProjectRole tiene?

¿Es propietario de este recurso?

¿Es el autor de este comentario?
```

Esto permite distinguir entre:

```text
Rol global
```

y:

```text
Rol contextual dentro de un recurso
```

---

## Project Roles

Los proyectos utilizan actualmente:

```text
Viewer
Developer
Manager
```

De forma simplificada:

```text
Viewer
    ↓
Lectura

Developer
    ↓
Lectura
Creación de Issues
Modificación de Issues
Gestión de comentarios propios

Manager
    ↓
Lectura
Creación / modificación de Issues
Gestión de comentarios
Gestión de miembros del proyecto
Eliminación de Issues
```

El propietario de un proyecto también dispone de permisos de gestión.

`ADMIN` tiene acceso administrativo global.

Las reglas concretas se implementan en servicios de autorización, no directamente en los controllers.

---

# Gestión de equipos

Los equipos representan grupos de usuarios dentro de la plataforma.

Actualmente permiten:

- crear equipos;
- listar equipos;
- consultar un equipo;
- gestionar miembros;
- asignar roles;
- modificar roles;
- eliminar miembros.

Los roles de equipo actuales son:

```text
Member
Lead
```

Un `Lead` puede gestionar miembros del equipo correspondiente.

Un usuario puede tener un rol diferente en diferentes equipos.

---

# Teams API

## Listar equipos

```http
GET /api/teams
Authorization: Bearer <JWT>
```

## Obtener un equipo

```http
GET /api/teams/{teamId}
Authorization: Bearer <JWT>
```

## Crear un equipo

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

El creador no se recibe desde el cliente.

La aplicación obtiene el usuario autenticado desde el JWT:

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

Respuesta:

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

## Añadir miembro

```http
POST /api/teams/{teamId}/members
Authorization: Bearer <JWT>
Content-Type: application/json
```

```json
{
  "userId": "00000000-0000-0000-0000-000000000000",
  "role": "Member"
}
```

## Modificar rol

```http
PATCH /api/teams/{teamId}/members/{userId}
Authorization: Bearer <JWT>
Content-Type: application/json
```

```json
{
  "role": "Lead"
}
```

## Eliminar miembro

```http
DELETE /api/teams/{teamId}/members/{userId}
Authorization: Bearer <JWT>
```

Respuesta correcta:

```http
204 No Content
```

La relación utiliza una clave primaria compuesta:

```text
TeamId + UserId
```

Esto evita duplicar una misma membresía.

---

# Gestión de proyectos

Los proyectos representan unidades de trabajo dentro de la plataforma.

Cada proyecto dispone de:

```text
Id
Name
Key
Description
CreatedAt
OwnerId
```

La `Key` se normaliza en mayúsculas.

Ejemplo:

```text
dbapi
```

se almacena como:

```text
DBAPI
```

La clave del proyecto es única.

---

# Projects API

## Listar proyectos

```http
GET /api/projects
Authorization: Bearer <JWT>
```

## Obtener un proyecto

```http
GET /api/projects/{projectId}
Authorization: Bearer <JWT>
```

## Crear un proyecto

```http
POST /api/projects
Authorization: Bearer <JWT>
Content-Type: application/json
```

Ejemplo:

```json
{
  "name": "DevOpsBoard API",
  "key": "DBAPI",
  "description": "Backend principal de DevOpsBoard"
}
```

El propietario se obtiene desde el usuario autenticado.

## Modificar un proyecto

```http
PATCH /api/projects/{projectId}
Authorization: Bearer <JWT>
Content-Type: application/json
```

Ejemplo:

```json
{
  "name": "DevOpsBoard API Updated",
  "description": "Descripción actualizada"
}
```

## Eliminar un proyecto

```http
DELETE /api/projects/{projectId}
Authorization: Bearer <JWT>
```

Respuesta:

```http
204 No Content
```

La modificación y eliminación del proyecto utilizan autorización contextual.

---

# Project Members

Los miembros de proyecto se representan mediante:

```text
ProjectMember
```

Cada relación contiene:

```text
ProjectId
UserId
Role
JoinedAt
```

Los roles actuales:

```text
Viewer
Developer
Manager
```

---

# Project Members API

## Listar miembros

```http
GET /api/projects/{projectId}/members
Authorization: Bearer <JWT>
```

## Añadir miembro

```http
POST /api/projects/{projectId}/members
Authorization: Bearer <JWT>
Content-Type: application/json
```

Ejemplo:

```json
{
  "userId": "00000000-0000-0000-0000-000000000000",
  "role": "Developer"
}
```

## Modificar rol

```http
PATCH /api/projects/{projectId}/members/{userId}
Authorization: Bearer <JWT>
Content-Type: application/json
```

Ejemplo:

```json
{
  "role": "Manager"
}
```

## Eliminar miembro

```http
DELETE /api/projects/{projectId}/members/{userId}
Authorization: Bearer <JWT>
```

Respuesta:

```http
204 No Content
```

La membresía utiliza:

```text
PK(ProjectId, UserId)
```

---

# Issues

Las Issues representan unidades de trabajo dentro de un proyecto.

Cada Issue contiene actualmente:

```text
Id
ProjectId
Title
Description
Status
Priority
ReporterId
AssigneeId
CreatedAt
UpdatedAt
IsDeleted
DeletedAt
```

---

## Estados

Actualmente existen:

```text
Todo
InProgress
InReview
Done
```

---

## Prioridades

Actualmente existen:

```text
Low
Medium
High
Critical
```

---

## Creación

Una Issue se crea proporcionando:

```text
Title
Description
Priority
```

El sistema establece automáticamente:

```text
Status = Todo
ReporterId = usuario autenticado
CreatedAt = UTC
UpdatedAt = UTC
```

El proyecto debe existir y el usuario debe disponer de permisos para crear Issues en él.

---

# Issues API

## Listar Issues

```http
GET /api/projects/{projectId}/issues
Authorization: Bearer <JWT>
```

## Obtener una Issue

```http
GET /api/projects/{projectId}/issues/{issueId}
Authorization: Bearer <JWT>
```

## Crear una Issue

```http
POST /api/projects/{projectId}/issues
Authorization: Bearer <JWT>
Content-Type: application/json
```

Ejemplo:

```json
{
  "title": "Implementar autenticación JWT",
  "description": "Completar el flujo de autenticación de la API",
  "priority": "High"
}
```

## Modificar una Issue

```http
PATCH /api/projects/{projectId}/issues/{issueId}
Authorization: Bearer <JWT>
Content-Type: application/json
```

Ejemplo:

```json
{
  "title": "Implementar autenticación JWT",
  "description": "JWT terminado y probado",
  "status": "InProgress",
  "priority": "Critical",
  "assigneeId": "00000000-0000-0000-0000-000000000000"
}
```

También se puede eliminar la asignación:

```json
{
  "title": "Implementar autenticación JWT",
  "description": "JWT terminado y probado",
  "status": "InReview",
  "priority": "High",
  "assigneeId": null
}
```

## Eliminar una Issue

```http
DELETE /api/projects/{projectId}/issues/{issueId}
Authorization: Bearer <JWT>
```

Respuesta:

```http
204 No Content
```

La eliminación es un **soft delete**.

La Issue no se borra físicamente de la base de datos.

Se establece:

```text
IsDeleted = true
DeletedAt = timestamp
```

Las consultas normales utilizan un filtro global de EF Core para ocultar Issues eliminadas.

---

# Autorización de Issues

La autorización de Issues está separada de los controllers.

Actualmente se distinguen permisos de:

```text
View
Create
Modify
Delete
```

De forma simplificada:

```text
ADMIN
    → acceso global

Owner del proyecto
    → acceso de gestión

Manager
    → lectura
    → creación
    → modificación
    → eliminación

Developer
    → lectura
    → creación
    → modificación

Viewer
    → lectura
```

Un usuario que no pertenece al proyecto no puede acceder a sus Issues.

---

# Comentarios

Las Issues pueden tener comentarios independientes.

Entidad:

```text
IssueComment
```

Cada comentario contiene:

```text
Id
IssueId
AuthorId
Content
CreatedAt
UpdatedAt
```

Los datos del autor se enriquecen en las lecturas con:

```text
DisplayName
Email
```

---

# Comments API

## Listar comentarios

```http
GET /api/projects/{projectId}/issues/{issueId}/comments
Authorization: Bearer <JWT>
```

Respuesta:

```json
[
  {
    "id": "00000000-0000-0000-0000-000000000000",
    "authorId": "00000000-0000-0000-0000-000000000000",
    "authorDisplayName": "Developer",
    "authorEmail": "developer@example.local",
    "content": "Comentario de prueba",
    "createdAt": "2026-09-11T22:13:34Z",
    "updatedAt": "2026-09-11T22:13:34Z"
  }
]
```

## Crear comentario

```http
POST /api/projects/{projectId}/issues/{issueId}/comments
Authorization: Bearer <JWT>
Content-Type: application/json
```

Ejemplo:

```json
{
  "content": "Primer comentario"
}
```

El autor se obtiene desde el usuario autenticado.

## Modificar comentario

```http
PATCH /api/projects/{projectId}/issues/{issueId}/comments/{commentId}
Authorization: Bearer <JWT>
Content-Type: application/json
```

Ejemplo:

```json
{
  "content": "Comentario actualizado"
}
```

## Eliminar comentario

```http
DELETE /api/projects/{projectId}/issues/{issueId}/comments/{commentId}
Authorization: Bearer <JWT>
```

Respuesta:

```http
204 No Content
```

---

# Autorización de comentarios

La gestión de comentarios utiliza una autorización específica.

De forma simplificada:

```text
Viewer
    → leer

Developer
    → leer
    → crear
    → modificar sus propios comentarios
    → eliminar sus propios comentarios

Manager
    → leer
    → crear
    → modificar comentarios
    → eliminar comentarios

Owner
    → acceso de gestión

ADMIN
    → acceso global
```

Los desarrolladores no pueden modificar ni eliminar comentarios pertenecientes a otros usuarios.

Managers y propietarios pueden gestionar comentarios ajenos.

---

# Historial y auditoría

DevOpsBoard incluye un sistema de historial para registrar cambios importantes sobre las Issues.

Entidad:

```text
IssueHistory
```

Cada entrada contiene:

```text
Id
IssueId
ActorId
CorrelationId
Action
OldValue
NewValue
CreatedAt
```

El actor identifica al usuario que realizó la operación.

El historial también devuelve:

```text
ActorDisplayName
ActorEmail
```

para facilitar el consumo desde clientes.

---

# Tipos de eventos

Actualmente existen:

```text
Created

TitleChanged
DescriptionChanged
StatusChanged
PriorityChanged

Assigned
Unassigned

CommentAdded
CommentEdited
CommentDeleted

Deleted
```

---

# CorrelationId

Las operaciones que generan varios cambios comparten un mismo:

```text
CorrelationId
```

Por ejemplo, una única petición `PATCH` puede generar:

```text
StatusChanged
PriorityChanged
Assigned
```

Los tres eventos comparten el mismo `CorrelationId`.

Esto permite reconstruir una única operación aunque haya producido múltiples eventos.

Ejemplo:

```text
PATCH Issue
    │
    ├── StatusChanged
    │       CorrelationId = A
    │
    ├── PriorityChanged
    │       CorrelationId = A
    │
    └── Assigned
            CorrelationId = A
```

Una nueva petición genera otro identificador:

```text
PATCH Issue
    │
    ├── StatusChanged
    │       CorrelationId = B
    │
    └── Unassigned
            CorrelationId = B
```

---

# Historial de comentarios

Los comentarios también forman parte de la auditoría.

Crear un comentario:

```text
CommentAdded
```

Modificar:

```text
CommentEdited
```

Eliminar:

```text
CommentDeleted
```

En `CommentDeleted`, el sistema conserva el contenido anterior en:

```text
OldValue
```

Esto permite saber qué contenido fue eliminado incluso después de eliminar físicamente el comentario.

---

# History API

## Consultar historial

```http
GET /api/projects/{projectId}/issues/{issueId}/history
Authorization: Bearer <JWT>
```

Ejemplo de respuesta:

```json
[
  {
    "id": "00000000-0000-0000-0000-000000000000",
    "actorId": "00000000-0000-0000-0000-000000000000",
    "actorDisplayName": "Jaime",
    "actorEmail": "jaime@example.local",
    "action": "Created",
    "oldValue": null,
    "newValue": null,
    "correlationId": "00000000-0000-0000-0000-000000000000",
    "createdAt": "2026-09-11T20:28:11Z"
  }
]
```

El acceso al historial también utiliza autorización contextual.

Los usuarios pertenecientes al proyecto pueden consultar la auditoría de sus Issues según las reglas de acceso del recurso.

---

# Health Check

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

# Authentication API

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

## Current User

```http
GET /api/auth/me
Authorization: Bearer <JWT>
```

Ejemplo:

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

# OpenAPI

La API utiliza el sistema OpenAPI integrado en ASP.NET Core.

Durante el desarrollo puede consultarse el documento en:

```text
http://localhost:5080/openapi/v1.json
```

El documento permite inspeccionar:

- endpoints;
- parámetros;
- cuerpos de petición;
- responses;
- esquemas.

Actualmente incluye operaciones relacionadas con:

```text
Auth
Health
Teams
Team Members
Projects
Project Members
Issues
Issue Comments
Issue History
```

---

# Manejo de errores

DevOpsBoard utiliza un handler global de excepciones.

Flujo:

```text
Exception
    │
    ▼
GlobalExceptionHandler
    │
    ▼
ProblemDetails
```

Las excepciones específicas de la aplicación se traducen a respuestas HTTP.

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

Unhandled Exception
    → 500 Internal Server Error
```

Ejemplo de error:

```json
{
  "title": "Acceso denegado.",
  "status": 403,
  "detail": "No tienes permisos para crear comentarios.",
  "instance": "/api/projects/..."
}
```

El uso de `ProblemDetails` evita respuestas de error inconsistentes entre controllers.

---

# Base de datos

DevOpsBoard utiliza PostgreSQL como sistema de persistencia.

Durante el desarrollo se ejecuta mediante Docker Compose.

La base de datos contiene actualmente tablas relacionadas con:

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

## Teams

```text
teams
team_members
```

## Projects

```text
projects
project_members
```

## Issues

```text
issues
```

## Comments

```text
issue_comments
```

## History

```text
issue_history
```

## Entity Framework Core

```text
__EFMigrationsHistory
```

---

# Relaciones de base de datos

Simplificación:

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
                    │
                    ▼
                  issues
                 ┌──┴───────┐
                 │          │
                 ▼          ▼
          issue_comments  issue_history
```

Relaciones importantes:

```text
TeamMember
    PK(TeamId, UserId)

ProjectMember
    PK(ProjectId, UserId)
```

Issues:

```text
Issue.ProjectId
    → Project

Issue.ReporterId
    → AspNetUsers

Issue.AssigneeId
    → AspNetUsers
```

Comments:

```text
IssueComment.IssueId
    → Issue

IssueComment.AuthorId
    → AspNetUsers
```

History:

```text
IssueHistory.IssueId
    → Issue

IssueHistory.ActorId
    → AspNetUsers
```

La relación de comentario utiliza `ON DELETE RESTRICT` para el autor, mientras que los comentarios se eliminan automáticamente al eliminar el recurso padre correspondiente cuando la relación lo permite.

---

# Migraciones

Entity Framework Core gestiona la evolución del esquema.

Las migraciones actuales incluyen:

```text
InitialIdentity
AddTeamsAndProjects
AddMemberships
AddMembershipRoles
RemoveMembershipRoleDefaults
FixMembershipRoleDefaults
AddTeamCreator
AddIssues
AddIssueHistory
AddIssueComments
```

Para listar las migraciones:

```powershell
dotnet ef migrations list `
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

Para aplicar las migraciones:

```powershell
dotnet ef database update `
  --project DevOpsBoard.Infrastructure `
  --startup-project DevOpsBoard.Api
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

Comprobar:

```powershell
docker ps
```

El servicio principal es:

```text
devopsboard-postgres
```

PostgreSQL se expone localmente mediante:

```text
localhost:5432
```

Para acceder directamente:

```powershell
docker exec -it devopsboard-postgres psql `
  -U devopsboard `
  -d devopsboard
```

---

# Configuración del entorno

Las credenciales locales no deben almacenarse en Git.

DevOpsBoard utiliza:

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

no debe subirse al repositorio.

Existe una plantilla:

```text
infra/.env.example
```

---

# User Secrets

Las credenciales y claves de desarrollo se almacenan mediante .NET User Secrets.

Inicialización:

```powershell
dotnet user-secrets init `
  --project DevOpsBoard.Api
```

Configuración de la conexión:

```powershell
dotnet user-secrets set `
  "ConnectionStrings:DevOpsBoard" `
  "Host=localhost;Port=5432;Database=devopsboard;Username=devopsboard;Password=<LOCAL_PASSWORD>" `
  --project DevOpsBoard.Api
```

Configuración de JWT:

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

Configuración opcional del administrador de desarrollo:

```powershell
dotnet user-secrets set `
  "Identity:AdminEmail" `
  "admin@example.local" `
  --project DevOpsBoard.Api
```

Para consultar los secretos registrados:

```powershell
dotnet user-secrets list `
  --project DevOpsBoard.Api
```

> Nunca deben almacenarse claves JWT, contraseñas, connection strings reales u otros secretos en Git.

---

# Puesta en marcha

## Requisitos

Se necesita:

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

Debe aparecer:

```text
devopsboard-postgres
```

---

## 3. Configurar User Secrets

Ir a:

```powershell
cd ..\apps
```

Inicializar User Secrets:

```powershell
dotnet user-secrets init `
  --project DevOpsBoard.Api
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

Durante el desarrollo:

```text
http://localhost:5080
```

---

# Testing

Los tests se encuentran en:

```text
apps/DevOpsBoard.Tests/
```

Actualmente se utiliza xUnit.

Ejecutar todos los tests:

```powershell
dotnet test DevOpsBoard.slnx
```

El proyecto cuenta actualmente con una suite de:

```text
85 tests
```

La batería cubre lógica de:

```text
Teams
Team Members

Projects
Project Members

Issues
Issue Authorization

Comments
Comment Authorization

Issue History
Audit Events
```

---

# Test Strategy

La estrategia de testing busca aislar la lógica de negocio de PostgreSQL siempre que sea posible.

Para ello se utilizan implementaciones fake de las abstracciones de Application.

Ejemplo conceptual:

```text
IssueService
     │
     ├── IIssueRepository
     ├── IProjectRepository
     ├── IUserRepository
     ├── IIssueAuthorizationService
     └── IIssueHistoryRepository
             │
             ▼
      Fake implementations
```

Esto permite comprobar rápidamente:

- validaciones;
- autorización;
- creación;
- modificación;
- eliminación;
- eventos de historial;
- reglas de comentarios.

---

# Flujo de una petición

Ejemplo simplificado de una petición autenticada:

```text
HTTP Request
     │
     ▼
JWT Bearer Authentication
     │
     ▼
ClaimsPrincipal
     │
     ▼
Controller
     │
     ▼
Application Service
     │
     ├── Validación
     ├── Autorización
     ├── Caso de uso
     └── Auditoría
     │
     ▼
Repository
     │
     ▼
Entity Framework Core
     │
     ▼
PostgreSQL
```

---

# Flujo de creación de una Issue

```text
POST /api/projects/{projectId}/issues
                │
                ▼
        IssuesController
                │
                ▼
          IssueService
                │
       ┌────────┴────────┐
       │                 │
       ▼                 ▼
  Project exists?   CanCreate?
       │                 │
       └────────┬────────┘
                │
                ▼
          Create Issue
                │
                ├──────────────┐
                │              │
                ▼              ▼
           Issue entity   IssueHistory
                          Created event
                │              │
                └──────┬───────┘
                       ▼
                  SaveChanges
                       │
                       ▼
                   PostgreSQL
```

---

# Flujo de actualización de una Issue

Una petición `PATCH` puede modificar varias propiedades al mismo tiempo.

Ejemplo:

```text
Status
Priority
Assignee
```

El servicio genera eventos individuales:

```text
StatusChanged
PriorityChanged
Assigned
```

pero los agrupa con un único:

```text
CorrelationId
```

Flujo:

```text
PATCH
 │
 ▼
IssueService
 │
 ├── Authorization
 │
 ├── Change Status
 │       └── History
 │
 ├── Change Priority
 │       └── History
 │
 └── Assign User
         └── History
 │
 ▼
SaveChanges
```

---

# Flujo de eliminación de una Issue

La eliminación utiliza soft delete:

```text
DELETE
 │
 ▼
Authorization
 │
 ▼
Issue.Delete()
 │
 ├── IsDeleted = true
 └── DeletedAt = UTC timestamp
 │
 ▼
IssueHistory
 │
 └── Deleted
 │
 ▼
SaveChanges
```

La Issue permanece en la base de datos para preservar integridad y trazabilidad.

Las consultas normales la ocultan mediante el filtro global de EF Core.

---

# Principios y decisiones de diseño

## Separación entre Domain e Infrastructure

Las entidades de dominio no dependen directamente de:

```text
EF Core
PostgreSQL
Identity
JWT
```

La infraestructura implementa los detalles técnicos.

---

## DTOs

La API no expone directamente las entidades de dominio.

Se utilizan DTOs para definir contratos específicos.

Ejemplo:

```text
Issue
    ↓
IssueDto
```

y:

```text
IssueComment
    ↓
IssueCommentDto
```

Esto permite modificar internamente el modelo sin romper necesariamente el contrato HTTP.

---

## Interfaces en Application

Application define abstracciones como:

```text
IIssueRepository
IProjectRepository
IIssueAuthorizationService
IIssueHistoryRepository
```

Infrastructure proporciona las implementaciones:

```text
IssueRepository
ProjectRepository
IssueAuthorizationService
IssueHistoryRepository
```

Esto desacopla los casos de uso de PostgreSQL y Entity Framework Core.

---

## Autorización contextual

Las decisiones de autorización no dependen únicamente de roles globales.

Ejemplo:

```text
Usuario
   │
   ▼
ProjectMember
   │
   ▼
ProjectRole = Manager
   │
   ▼
Proyecto específico
```

Esto permite que un mismo usuario tenga diferentes permisos en diferentes proyectos.

---

## Auditoría

Los cambios importantes se registran en `IssueHistory`.

Esto permite responder preguntas como:

```text
¿Quién creó esta Issue?

¿Quién cambió su estado?

¿Cuándo ocurrió?

¿Qué valor tenía antes?

¿Qué valor tiene ahora?

¿Qué acciones pertenecieron a la misma operación?
```

---

## Soft Delete

Las Issues no se eliminan físicamente mediante el endpoint normal.

Esto proporciona:

- trazabilidad;
- integridad referencial;
- posibilidad de auditoría;
- protección frente a pérdida accidental de datos.

---

# Roadmap

El proyecto se desarrolla de forma incremental.

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
[✓] Projects
[✓] Project Members
[✓] Project Roles
[✓] Autorización basada en proyecto
[✓] Issues
[✓] Issue Status
[✓] Issue Priority
[✓] Issue Assignment
[✓] Issue Authorization
[✓] Soft Delete
[✓] Issue History
[✓] CorrelationId
[✓] History API
[✓] Issue Comments
[✓] Comment Authorization
[✓] Comment Audit
[✓] Global Exception Handler
[✓] ProblemDetails
[✓] OpenAPI
[✓] Tests unitarios
```

---

## 🚧 Próximos bloques

El siguiente bloque principal es mejorar las consultas de Issues.

```text
[ ] Paginación
[ ] Filtros por estado
[ ] Filtros por prioridad
[ ] Filtro por asignado
[ ] Búsqueda por texto
[ ] Ordenación configurable
[ ] Respuestas paginadas
```

Después:

```text
[ ] Labels
[ ] Issue Labels
[ ] Más metadatos para Issues
[ ] Historial más avanzado
[ ] Mejoras de consulta
```

---

# Fase DevOps

Una vez estabilizada la API:

```text
[ ] Dockerización completa de la aplicación
[ ] Health checks de infraestructura
[ ] GitHub Actions
[ ] CI
[ ] Build automatizado
[ ] Ejecución automática de tests
[ ] Docker image
[ ] Registro de imágenes
[ ] CD
[ ] Gestión de variables de entorno
[ ] Observabilidad
[ ] Logs estructurados
[ ] Métricas
[ ] Despliegue
```

---

# Fase frontend

Está prevista una interfaz web para consumir la API.

Posibles tecnologías:

```text
React
TypeScript
Vite
```

La interfaz tendrá como objetivo permitir:

```text
Authentication
Teams
Projects
Issues
Comments
History
```

La autenticación se realizará utilizando JWT.

---

# Seguridad

La aplicación utiliza diferentes mecanismos para reducir riesgos de seguridad durante el desarrollo.

## Secretos

Los secretos locales se almacenan mediante:

```text
.NET User Secrets
infra/.env
```

No deben almacenarse en Git.

---

## JWT

La clave de firma de JWT debe mantenerse fuera del repositorio.

En desarrollo se utiliza User Secrets.

En producción deberá utilizarse un sistema de gestión de secretos adecuado.

---

## Autorización

La autorización se realiza en la capa de aplicación/infrastructure mediante servicios específicos.

No se confía únicamente en el frontend para restringir acciones.

Cada operación sensible debe validar el usuario que ejecuta la acción.

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
│   │   │   ├── IssueCommentsController.cs
│   │   │   ├── IssueHistoryController.cs
│   │   │   ├── IssuesController.cs
│   │   │   ├── ProjectMembersController.cs
│   │   │   ├── ProjectsController.cs
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
│   │   │   ├── Issue.cs
│   │   │   ├── IssueComment.cs
│   │   │   ├── IssueHistory.cs
│   │   │   ├── Project.cs
│   │   │   ├── ProjectMember.cs
│   │   │   ├── Team.cs
│   │   │   └── TeamMember.cs
│   │   │
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
│   │   ├── IssueCommentServiceTests.cs
│   │   ├── IssueCommentTests.cs
│   │   ├── IssueHistoryServiceTests.cs
│   │   ├── IssueHistoryTests.cs
│   │   ├── IssueServiceTests.cs
│   │   ├── IssueTests.cs
│   │   ├── ProjectMemberServiceTests.cs
│   │   ├── ProjectServiceTests.cs
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

El objetivo del desarrollo es mantener el repositorio en un estado compilable y testeable.

Build:

```powershell
dotnet build DevOpsBoard.slnx
```

Tests:

```powershell
dotnet test DevOpsBoard.slnx
```

Estado actual de la suite:

```text
85 tests
0 errores
```

La intención es utilizar cada cambio significativo como un checkpoint:

```text
Modificar
   ↓
Compilar
   ↓
Ejecutar tests
   ↓
Probar API real si aplica
   ↓
Commit
   ↓
Push
```

Esto permite reducir el riesgo de acumular cambios difíciles de aislar.

---

# Estado actual

DevOpsBoard ha pasado de una API básica centrada en autenticación y equipos a una plataforma de backend con:

```text
Identity
    ↓
JWT Authentication
    ↓
Global Roles
    ↓
Contextual Authorization
    ↓
Teams
    ↓
Projects
    ↓
Project Members
    ↓
Issues
    ├── Assignment
    ├── Workflow
    ├── Soft Delete
    │
    ├── Comments
    │
    └── Audit History
```

La siguiente evolución importante consiste en mejorar la capacidad de consulta de Issues mediante:

```text
Pagination
Filtering
Search
Sorting
```

antes de continuar ampliando el dominio funcional.

---

# Autor

**Jaime Molina Granados**

Desarrollador Full Stack y desarrollador de videojuegos.

## Portfolio

https://jaime-molina-granados.vercel.app

## GitHub

https://github.com/JaimeMGR

---

# Licencia

Este proyecto se encuentra actualmente en desarrollo.

La licencia definitiva del repositorio se definirá posteriormente.