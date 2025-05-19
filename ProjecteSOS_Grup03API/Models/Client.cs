using System.ComponentModel;

namespace ProjecteSOS_Grup03API.Models
{
    public class Client : User
    {
        [DefaultValue(false)]
        public bool IsMember { get; set; } = false;

        public int Points { get; set; } = 0;

        public ICollection<Order> Orders { get; set; }

    }
}
