namespace ProjecteSOS_Grup03API.DTOs
{
    public class ProductDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public int Stock { get; set; }
        public int Points { get; set; } = 0;
        public double Price { get; set; }
    }
}
