﻿namespace ProjecteSOS_Grup03API.Models
{
    public class ProductOrder
    {
        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public DateTime OrderDate { get; set; }

        public int Quantity { get; set; }
    }
}
