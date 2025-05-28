using System.ComponentModel;

namespace ProjecteSOS_Grup03API.Models
{
    public class Client : User
    {
        public int Points { get; set; } = 0;

        public int? CurrentOrderId { get; set; }

        public ICollection<Order> Orders { get; set; }

    }
}
