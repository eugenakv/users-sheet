using System.ComponentModel.DataAnnotations;

namespace UsersSheet.Models
{
    public class RegistrationInfoViewModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
