using BaseApi.Data;
using BaseApi.DTOs;
using BaseApi.Models;
using BaseApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using System.Security.Claims;

namespace BaseApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuthService _auth;
        private readonly IMemoryCache _cache;

        public AuthController(AppDbContext context, AuthService auth, IMemoryCache cache)
        {
            _context = context;
            _auth = auth;
            _cache = cache;
        }

        /// <summary>
        /// Registra un nuevo usuario
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserCreateDto dto)
        {
            if (await _context.Users.AnyAsync(x => x.Username == dto.Username))
                return BadRequest(new { error = "El usuario ya existe" });

            if (await _context.Users.AnyAsync(x => x.Email == dto.Email))
                return BadRequest(new { error = "El email ya está registrado" });

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = _auth.HashPassword(dto.Password),
                Email = dto.Email,
                Level = dto.Level
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Usuario registrado exitosamente",
                user = new { user.Id, user.Username, user.Email }
            });
        }

        /// <summary>
        /// Inicia sesión y obtiene token JWT
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == dto.Username);
            if (user == null)
                return Unauthorized(new { error = "Usuario no encontrado" });

            if (!_auth.VerifyPassword(dto.Password, user.PasswordHash))
                return Unauthorized(new { error = "Contraseña incorrecta" });

            var token = _auth.GenerateJwtToken(user);

            return Ok(new
            {
                token,
                user = new { user.Id, user.Username, user.Email, user.Level },
                expiresIn = 7200 // 2 horas en segundos
            });
        }

        /// <summary>
        /// Cierra sesión e invalida el token actual
        /// </summary>
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
                return BadRequest(new { error = "No se proporcionó token" });

            _auth.InvalidateToken(token);

            return Ok(new
            {
                message = "Logout exitoso",
                details = "El token ha sido invalidado y no podrá ser usado nuevamente"
            });
        }

        /// <summary>
        /// Valida si el token actual es válido
        /// </summary>
        [HttpPost("validate")]
        public IActionResult ValidateToken()
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
                return BadRequest(new { error = "No se proporcionó token" });

            if (_auth.IsTokenInvalidated(token))
                return Unauthorized(new
                {
                    valid = false,
                    error = "Token invalidado"
                });

            var expiry = _auth.GetTokenExpiry(token);

            return Ok(new
            {
                valid = true,
                expiresAt = expiry,
                message = "Token válido"
            });
        }

        /// <summary>
        /// Obtiene información del usuario actual
        /// </summary>
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var id))
                return Unauthorized(new { error = "Token inválido" });

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { error = "Usuario no encontrado" });

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Email,
                user.Level
            });
        }
    }
}