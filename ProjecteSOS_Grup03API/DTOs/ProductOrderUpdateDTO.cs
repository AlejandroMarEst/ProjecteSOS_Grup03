using ProjecteSOS_Grup03API.Tools;
using System.ComponentModel.DataAnnotations;

namespace ProjecteSOS_Grup03API.DTOs
{
    public class ProductOrderUpdateDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = ValidationMessages.QuantityPositive)]
        public int Quantity { get; set; }
    }
}
