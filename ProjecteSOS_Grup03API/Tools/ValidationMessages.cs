namespace ProjecteSOS_Grup03API.Tools
{
    public static class ValidationMessages
    {
        // LoginDTO & RegisterDTO & UserProfileDTO
        public const string EmailRequired = "Email is required.";
        public const string EmailInvalid = "Invalid email address format.";
        public const string PasswordRequired = "Password is required.";
        public const string PasswordMinLength = "Password must be at least 8 characters.";
        public const string PhoneRequired = "Phone number is required.";
        public const string PhoneInvalid = "Invalid phone number format.";
        public const string PointsPositive = "Points must be 0 or greater.";

        // RegsiterEmployeeDTO & UserProfileListDTO (Both Uses All Upper Consts, Except PointsPositive)
        public const string IdRequired = "User ID is required.";
        public const string NameRequired = "Name is required.";
        public const string NameLength = "Name must be between 2 and 100 characters.";
        public const string SalaryRequired = "Salary is required.";
        public const string SalaryPositive = "Salary must be 0 or greater.";

        // General
        public const string PriceRequired = "Price is required.";
        public const string PricePositive = "Price must be greater than or equal to 0.";

        // OrderDTO & OrderListDTO
        public const string OrderIdRequired = "OrderId is required.";
        public const string ClientIdRequired = "ClientId is required.";
        public const string OrderDateRequired = "Order date is required.";

        // ProductDTO & ProductListDTO (Both Uses PointsPositive)
        public const string ProductIdRequired = "ProductId is required.";
        public const string ProductNameRequired = "Product name is required.";
        public const string ProductNameLength = "Product name must be between 2 and 100 characters.";
        public const string DescriptionRequired = "Product description is required.";
        public const string DescriptionLength = "Product description must be between 5 and 500 characters.";
        public const string ImageRequired = "Product image URL is required.";
        public const string StockRequired = "Stock is required.";
        public const string StockPositive = "Stock must be 0 or greater.";

        // ProductOrderCreateDTO (Uses ProductIdRequired)
        // & ProductOrderDetailsDTO (Uses OrderIdRequired, ProductIdRequired, ProductNameRequired, OrderDateRequired)
        // & ProductOrderDTO (Uses OrderDateRequired)
        // & ProductOrderUpdateDTO
        public const string QuantityRequired = "Quantity is required.";
        public const string QuantityPositive = "Quantity must be greater than 0.";
        public const string UnitPriceRequired = "Unit price is required.";
        public const string UnitPricePositive = "Unit price must be 0 or greater.";
    }
}
