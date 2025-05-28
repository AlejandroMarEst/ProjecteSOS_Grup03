using ProjecteSOS_Grup03API.Tools;
using System.ComponentModel.DataAnnotations;

namespace ProjecteSOS_Grup03API.DTOs
{
    public class LoginDTO
    {
        [Required(ErrorMessage = ValidationMessages.EmailRequired)]
        [EmailAddress(ErrorMessage = ValidationMessages.EmailInvalid)]
        public string Email { get; set; }

        [Required(ErrorMessage = ValidationMessages.PasswordRequired)]
        [MinLength(8, ErrorMessage = ValidationMessages.PasswordMinLength)]
        public string Password { get; set; }
    }
}
