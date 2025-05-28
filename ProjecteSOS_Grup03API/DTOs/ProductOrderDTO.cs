using ProjecteSOS_Grup03API.Models;
using ProjecteSOS_Grup03API.Tools;
using System.ComponentModel.DataAnnotations;

namespace ProjecteSOS_Grup03API.DTOs
{
    public class ProductOrderDTO
    {
        [Required(ErrorMessage = ValidationMessages.OrderDateRequired)]
        public DateTime OrderDate { get; set; }

        [Required(ErrorMessage = ValidationMessages.QuantityRequired)]
        [Range(1, int.MaxValue, ErrorMessage = ValidationMessages.QuantityPositive)]
        public int Quantity { get; set; }
    }
}
