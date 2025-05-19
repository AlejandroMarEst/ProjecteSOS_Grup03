using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProjecteSOS_Grup03API.Models
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderId { get; set; }

        public string? ClientId { get; set; }
        public Client Client { get; set; }

        public Employee? SalesRep { get; set; }
        public string? SalesRepId { get; set; }

        public double Price { get; set; }

        public ICollection<ProductOrder> OrdersProducts { get; set; }
    }
}
