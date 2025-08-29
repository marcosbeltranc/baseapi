using System.ComponentModel.DataAnnotations;

namespace BaseApi.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        // 0 = Admin, otros valores = niveles personalizados
        public int Level { get; set; } = 1;
    }
}
