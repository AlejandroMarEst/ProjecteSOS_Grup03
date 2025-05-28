using ProjecteSOS_Grup03API.Tools;
using System.ComponentModel.DataAnnotations;

namespace ProjecteSOS_Grup03API.DTOs
{
    public class ProductDTO
    {
        [Required(ErrorMessage = ValidationMessages.ProductNameRequired)]
        [StringLength(100, MinimumLength = 2, ErrorMessage = ValidationMessages.ProductNameLength)]
        public string Name { get; set; }

        [Required(ErrorMessage = ValidationMessages.DescriptionRequired)]
        [StringLength(500, MinimumLength = 5, ErrorMessage = ValidationMessages.DescriptionLength)]
        public string Description { get; set; }

        [Required(ErrorMessage = ValidationMessages.ImageRequired)]
        public string Image { get; set; }

        [Required(ErrorMessage = ValidationMessages.StockRequired)]
        [Range(0, int.MaxValue, ErrorMessage = ValidationMessages.StockPositive)]
        public int Stock { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = ValidationMessages.PointsPositive)]
        public int Points { get; set; } = 0;

        [Required(ErrorMessage = ValidationMessages.PriceRequired)]
        [Range(0, double.MaxValue, ErrorMessage = ValidationMessages.PricePositive)]
        public double Price { get; set; }
    }
}
