﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProjecteSOS_Grup03API.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Image { get; set; }

        public int Stock { get; set; }

        public double Price { get; set; }

        public int Points { get; set; } = 0;

        public ICollection<ProductOrder> ProductsOrders { get; set; }
    }
}
