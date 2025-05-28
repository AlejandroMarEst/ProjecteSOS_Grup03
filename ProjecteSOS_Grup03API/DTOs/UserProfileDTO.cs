using ProjecteSOS_Grup03API.Tools;
using System.ComponentModel.DataAnnotations;

namespace ProjecteSOS_Grup03API.DTOs
{
    public class UserProfileDTO
    {
        [Required(ErrorMessage = ValidationMessages.EmailRequired)]
        [EmailAddress(ErrorMessage = ValidationMessages.EmailInvalid)]
        public string Email { get; set; }

        [Required(ErrorMessage = ValidationMessages.NameRequired)]
        [StringLength(100, MinimumLength = 2, ErrorMessage = ValidationMessages.NameLength)]
        public string Name { get; set; }

        [Required(ErrorMessage = ValidationMessages.PhoneRequired)]
        [Phone(ErrorMessage = ValidationMessages.PhoneInvalid)]
        public string Phone { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = ValidationMessages.PointsPositive)]
        public int? Points { get; set; }
        public DateOnly? StartDate { get; set; }
        public bool? IsAdmin { get; set; }
    }
}
