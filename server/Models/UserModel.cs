using System.ComponentModel.DataAnnotations;

namespace server.Models
{
    public class UserModel : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;

        public int FailedLoginAttempts { get; set; } = 0;
    }
}