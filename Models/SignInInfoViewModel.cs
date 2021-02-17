using System.ComponentModel.DataAnnotations;

namespace UsersSheet.Models
{
    public class SignInInfoViewModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
