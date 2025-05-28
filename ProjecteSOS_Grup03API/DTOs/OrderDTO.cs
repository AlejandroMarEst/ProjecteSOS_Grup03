using ProjecteSOS_Grup03API.Tools;
using System.ComponentModel.DataAnnotations;

namespace ProjecteSOS_Grup03API.DTOs
{
    public class OrderDTO
    {
        [Required(ErrorMessage = ValidationMessages.ClientIdRequired)]
        public string? ClientId { get; set; }

        public string? SalesRepId { get; set; }

        [Required(ErrorMessage = ValidationMessages.OrderDateRequired)]
        public DateOnly OrderDate { get; set; }

        [Required(ErrorMessage = ValidationMessages.PriceRequired)]
        [Range(0, double.MaxValue, ErrorMessage = ValidationMessages.PricePositive)]
        public double Price { get; set; }
    }
}
