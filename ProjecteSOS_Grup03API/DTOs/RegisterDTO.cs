using ProjecteSOS_Grup03API.Tools;
using System.ComponentModel.DataAnnotations;

namespace ProjecteSOS_Grup03API.DTOs
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = ValidationMessages.EmailRequired)]
        [EmailAddress(ErrorMessage = ValidationMessages.EmailInvalid)]
        public string Email { get; set; }

        [Required(ErrorMessage = ValidationMessages.PasswordRequired)]
        [MinLength(6, ErrorMessage = ValidationMessages.PasswordMinLength)]
        public string Password { get; set; }

        [Required(ErrorMessage = ValidationMessages.ProductNameRequired)]
        [StringLength(100, MinimumLength = 2, ErrorMessage = ValidationMessages.ProductNameLength)]
        public string Name { get; set; }

        [Required(ErrorMessage = ValidationMessages.PhoneRequired)]
        [Phone(ErrorMessage = ValidationMessages.PhoneInvalid)]
        public string Phone { get; set; }
    }
}
