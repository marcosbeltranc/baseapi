# ?? API Base en .NET 8 con SQL Server + JWT + Docker


- CRUD de usuarios ??  
- Manejo de niveles (`level`) donde `0 = admin`  
- Autenticaci�n con **JWT** ??  
- Contrase�as cifradas con **BCrypt** ??  
- Base de datos **SQL Server** ??  
- Soporte para **Docker** y **docker-compose** ??  

El objetivo es que cualquier miembro del equipo pueda clonar este repo, configurar variables de entorno y tener una base s�lida para iniciar proyectos sin partir desde cero.

---

## ?? Estructura del Proyecto

```bash
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
bash
Copiar c�digo
git clone https://github.com/tu-org/api-base-dotnet.git
cd api-base-dotnet
2. Crear el proyecto (si inicias desde cero)
bash
Copiar c�digo
dotnet new webapi -n ApiBase
cd ApiBase
3. Instalar dependencias
bash
Copiar c�digo
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package BCrypt.Net-Next
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
4. Configurar variables de entorno
Crea un archivo appsettings.json con:

json
Copiar c�digo
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sqlserver;Database=ApiBaseDb;User Id=sa;Password=Your_strong_password123;"
  },
  "Jwt": {
    "Key": "ClaveSuperSecretaDeJwt123456",
    "Issuer": "api-base",
    "Audience": "api-base-users"
  }
}
?? Ajusta la contrase�a y llaves seg�n tu entorno.

?? Usando Docker
1. Levantar contenedores
bash
Copiar c�digo
docker-compose up --build
Esto levantar�:

API en .NET 8 en http://localhost:5000

SQL Server en localhost:1433

2. Migraciones (base de datos)
El Dockerfile est� configurado para correr autom�ticamente:

bash
Copiar c�digo
dotnet ef database update
Esto asegura que la base de datos y tablas se creen al levantar el contenedor ??

Si deseas correrlo manualmente:

bash
Copiar c�digo
dotnet ef migrations add InitialCreate
dotnet ef database update
?? Endpoints principales
Auth
POST /api/auth/register ? Crear usuario

POST /api/auth/login ? Iniciar sesi�n y obtener JWT

POST /api/auth/logout ? (dummy endpoint, invalida en frontend)

Users
GET /api/users ? Listar usuarios (requiere JWT admin)

GET /api/users/{id} ? Obtener usuario por ID

PUT /api/users/{id} ? Editar usuario

DELETE /api/users/{id} ? Eliminar usuario

??? Comandos �tiles
Compilar y correr localmente
bash
Copiar c�digo
dotnet run
Ver migraciones
bash
Copiar c�digo
dotnet ef migrations list
Crear nueva migraci�n
bash
Copiar c�digo
dotnet ef migrations add NombreMigracion
dotnet ef database update