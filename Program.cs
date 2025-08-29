using BaseApi.Data;
using BaseApi.Services;
using BaseApi.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============================
// 1. Agregar DbContext con SQL Server
// ============================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ============================
// 2. Registrar servicios de aplicación
// ============================
builder.Services.AddMemoryCache(); // ? AGREGAR MEMORY CACHE
builder.Services.AddScoped<AuthService>();

// ============================
// 3. Configurar JWT Authentication
// ============================
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrEmpty(jwtKey))
{
    throw new ArgumentNullException("Jwt:Key", "JWT Key no está configurado");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// ============================
// 4. Controllers
// ============================
builder.Services.AddControllers();

// ============================
// 5. Swagger con soporte para JWT
// ============================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BaseApi",
        Version = "v1",
        Description = "API con autenticación JWT y sistema de logout",
        Contact = new OpenApiContact
        {
            Name = "Tu Nombre",
            Email = "tu.email@example.com"
        }
    });

    // Agregar autorización con JWT en Swagger
    var securitySchema = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Ingresa el token JWT en el formato: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", securitySchema);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securitySchema, new[] { "Bearer" } }
    });
});

// ============================
// 6. Configurar logging
// ============================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// ============================
// 7. Auto aplicar migraciones al levantar contenedor
// ============================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// ============================
// 8. Middlewares
// ============================
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BaseApi v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "BaseApi Documentation";
    c.EnableDeepLinking();
    c.DisplayOperationId();
});

app.UseHttpsRedirection();

// ============================
// 8.1. MIDDLEWARE DE VALIDACIÓN DE TOKENS (NUEVO) ? IMPORTANTE
// ============================
app.UseTokenValidation();

app.UseAuthentication();
app.UseAuthorization();

// ============================
// 9. Endpoints mínimos para testing y diagnóstico
// ============================
app.MapGet("/", () => "¡BaseAPI está funcionando! Accede a /swagger para la documentación API.");
app.MapGet("/api/health", () => Results.Ok(new
{
    status = "Healthy",
    message = "API is running successfully",
    timestamp = DateTime.UtcNow,
    database = "Connected",
    version = "1.0.0"
}));

app.MapGet("/api/test-auth", (AuthService authService) =>
{
    try
    {
        var hashed = authService.HashPassword("test");
        var verified = authService.VerifyPassword("test", hashed);

        return Results.Ok(new
        {
            Service = "AuthService is working!",
            HashTest = hashed,
            VerifyTest = verified,
            Timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"AuthService error: {ex.Message}");
    }
});

app.MapGet("/api/debug/config", (IConfiguration config) =>
{
    var configValues = new
    {
        Database = config.GetConnectionString("DefaultConnection"),
        JwtKey = config["Jwt:Key"],
        JwtIssuer = config["Jwt:Issuer"],
        JwtAudience = config["Jwt:Audience"],
        Environment = app.Environment.EnvironmentName
    };

    return Results.Ok(configValues);
});

// ============================
// 10. Mapear controladores
// ============================
app.MapControllers();

// ============================
// 11. Log de inicio exitoso
// ============================
app.Lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine("Application started successfully!");
    Console.WriteLine("Listening on: http://0.0.0.0:8088");
    Console.WriteLine("Auth endpoints:");
    Console.WriteLine("   POST /api/auth/register");
    Console.WriteLine("   POST /api/auth/login");
    Console.WriteLine("   POST /api/auth/logout");
    Console.WriteLine("   POST /api/auth/validate");
    Console.WriteLine("   GET  /api/auth/me");
    Console.WriteLine("Health check: GET /api/health");
    Console.WriteLine("Debug: GET /api/debug/config");
    Console.WriteLine("Documentation: /swagger");
});

app.Run();