using System.ComponentModel.DataAnnotations;

namespace ProjecteSOS_Grup03API.DTOs
{
    public class ProductOrderUpdateDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "La quantitat ha de ser com a mínim 1.")]
        public int Quantity { get; set; }
    }
}
