namespace ProjecteSOS_Grup03WebPage.DTOs
{
    public class OrderDTO
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } // Nom del producte per referència
        public int Quantity { get; set; }
        public DateTime OrderDate { get; set; }
        public double UnitPrice { get; set; } // Preu del producte en el moment
        public double TotalPrice => UnitPrice * Quantity;
    }
}
