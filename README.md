# ?? API Base en .NET 8 con SQL Server + JWT + Docker


- CRUD de usuarios ??  
- Manejo de niveles (`level`) donde `0 = admin`  
- Autenticación con **JWT** ??  
- Contraseñas cifradas con **BCrypt** ??  
- Base de datos **SQL Server** ??  
- Soporte para **Docker** y **docker-compose** ??  

El objetivo es que cualquier miembro del equipo pueda clonar este repo, configurar variables de entorno y tener una base sólida para iniciar proyectos sin partir desde cero.

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
Copiar código
git clone https://github.com/tu-org/api-base-dotnet.git
cd api-base-dotnet
2. Crear el proyecto (si inicias desde cero)
bash
Copiar código
dotnet new webapi -n ApiBase
cd ApiBase
3. Instalar dependencias
bash
Copiar código
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package BCrypt.Net-Next
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
4. Configurar variables de entorno
Crea un archivo appsettings.json con:

json
Copiar código
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
?? Ajusta la contraseña y llaves según tu entorno.

?? Usando Docker
1. Levantar contenedores
bash
Copiar código
docker-compose up --build
Esto levantará:

API en .NET 8 en http://localhost:5000

SQL Server en localhost:1433

2. Migraciones (base de datos)
El Dockerfile está configurado para correr automáticamente:

bash
Copiar código
dotnet ef database update
Esto asegura que la base de datos y tablas se creen al levantar el contenedor ??

Si deseas correrlo manualmente:

bash
Copiar código
dotnet ef migrations add InitialCreate
dotnet ef database update
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

??? Comandos útiles
Compilar y correr localmente
bash
Copiar código
dotnet run
Ver migraciones
bash
Copiar código
dotnet ef migrations list
Crear nueva migración
bash
Copiar código
dotnet ef migrations add NombreMigracion
dotnet ef database update