using BaseApi.Services;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace BaseApi.Middleware
{
	public class TokenValidationMiddleware
	{
		private readonly RequestDelegate _next;

		public TokenValidationMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context, AuthService authService)
		{
			// Skip token validation for auth endpoints and swagger
			var path = context.Request.Path;
			if (path.StartsWithSegments("/api/auth") ||
				path.StartsWithSegments("/swagger") ||
				path.StartsWithSegments("/health"))
			{
				await _next(context);
				return;
			}

			var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

			if (!string.IsNullOrEmpty(token))
			{
				if (authService.IsTokenInvalidated(token))
				{
					context.Response.StatusCode = 401;
					context.Response.ContentType = "application/json";
					await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
					{
						error = "Token invalidado",
						message = "El token ha sido revocado. Por favor, inicia sesión nuevamente."
					}));
					return;
				}
			}

			await _next(context);
		}
	}

	public static class TokenValidationMiddlewareExtensions
	{
		public static IApplicationBuilder UseTokenValidation(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<TokenValidationMiddleware>();
		}
	}
}