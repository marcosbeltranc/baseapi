# ?? API Base en .NET 8 con SQL Server + JWT + Docker
El objetivo es que cualquier miembro del equipo pueda clonar este repo, 
configurar variables de entorno y tener una base sólida para iniciar proyectos sin partir desde cero.

## ?? Estructura del Proyecto

src/
??? Controllers/
?   ??? AuthController.cs
?   ??? UsersController.cs
??? Data/
?   ??? AppDbContext.cs
??? Models/
?   ??? User.cs
??? Services/
?   ??? IUserService.cs
?   ??? UserService.cs
??? Program.cs
??? Dockerfile
??? docker-compose.yml
??? README.md


?? Requisitos
.NET 8 SDK

Docker y Docker Compose

SQL Server (se levanta con docker-compose)

?? Pasos para correr el proyecto
1. Clonar el repositorio

Copiar código
# Crear nuevo repositorio desde esta plantilla
gh repo create mi-nueva-api --template marcosbeltranc/baseapi

# Clonar el nuevo repositorio
gh repo clone mi-nueva-api


2. Instalar dependencias
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package BCrypt.Net-Next
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer

3. Configurar variables de entorno
En el archivo appsettings.json y docker-compose.yml, ajusta las variables según tu entorno.
# Ejemplo de variables de entorno
JWT_KEY=Minimum32CharactersKeyForJWTTokenSecurity!
DB_PASSWORD=YourStrong!Passw0rd
DB_SERVER=sqlserver
DB_NAME=BaseApiDb

?? Usando Docker

docker-compose up --build

Esto levantará:
API en .NET 8 en http://localhost:5000
SQL Server en localhost:1433


?? Endpoints principales
Auth
POST /api/auth/register ? Crear usuario

POST /api/auth/login ? Iniciar sesión y obtener JWT

POST /api/auth/logout ? (dummy endpoint, invalida en frontend)

Users
GET /api/users ? Listar usuarios (requiere JWT admin)

GET /api/users/{id} ? Obtener usuario por ID

PUT /api/users/{id} ? Editar usuario

DELETE /api/users/{id} ? Eliminar usuario
