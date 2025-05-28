using ProjecteSOS_Grup03API.Tools;
using System.ComponentModel.DataAnnotations;

namespace ProjecteSOS_Grup03API.DTOs
{
    public class ProductOrderDetailsDTO
    {
        [Required(ErrorMessage = ValidationMessages.OrderIdRequired)]
        public int OrderId { get; set; }

        [Required(ErrorMessage = ValidationMessages.ProductIdRequired)]
        public int ProductId { get; set; }

        [Required(ErrorMessage = ValidationMessages.ProductNameRequired)]
        public string ProductName { get; set; }

        [Required(ErrorMessage = ValidationMessages.QuantityRequired)]
        [Range(1, int.MaxValue, ErrorMessage = ValidationMessages.QuantityPositive)]
        public int Quantity { get; set; }

        [Required(ErrorMessage = ValidationMessages.OrderDateRequired)]
        public DateTime OrderDate { get; set; }

        [Required(ErrorMessage = ValidationMessages.UnitPriceRequired)]
        [Range(0, double.MaxValue, ErrorMessage = ValidationMessages.UnitPricePositive)]
        public double UnitPrice { get; set; }
        public double TotalPrice => UnitPrice * Quantity;
    }
}
