using ProjecteSOS_Grup03API.Tools;
using System.ComponentModel.DataAnnotations;

namespace ProjecteSOS_Grup03API.DTOs
{
    public class ProductOrderCreateDTO
    {
        [Required(ErrorMessage = ValidationMessages.ProductIdRequired)]
        public int ProductId { get; set; }

        [Required(ErrorMessage = ValidationMessages.QuantityRequired)]
        [Range(1, int.MaxValue, ErrorMessage = ValidationMessages.QuantityPositive)]
        public int Quantity { get; set; }
    }
}
