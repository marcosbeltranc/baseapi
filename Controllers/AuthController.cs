using BaseApi.Data;
using BaseApi.DTOs;
using BaseApi.Models;
using BaseApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BaseApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuthService _auth;

        public AuthController(AppDbContext context, AuthService auth)
        {
            _context = context;
            _auth = auth;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserCreateDto dto)
        {
            if (await _context.Users.AnyAsync(x => x.Username == dto.Username))
                return BadRequest("El usuario ya existe");

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = _auth.HashPassword(dto.Password),
                Email = dto.Email,
                Level = dto.Level
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario registrado" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == dto.Username);
            if (user == null) return Unauthorized("Usuario no encontrado");

            if (!_auth.VerifyPassword(dto.Password, user.PasswordHash))
                return Unauthorized("Contraseña incorrecta");

            var token = _auth.GenerateJwtToken(user);
            return Ok(new { token });
        }
    }
}
