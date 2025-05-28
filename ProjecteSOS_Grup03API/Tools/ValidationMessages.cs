namespace ProjecteSOS_Grup03API.Tools
{
    /// <summary>
    /// Centralized validation messages for DTO data annotations.
    /// </summary>
    public static class ValidationMessages
    {
        // --- LoginDTO, RegisterDTO, UserProfileDTO ---

        /// <summary>Displayed when the email field is required.</summary>
        public const string EmailRequired = "Email is required.";

        /// <summary>Displayed when the email format is invalid.</summary>
        public const string EmailInvalid = "Invalid email address format.";

        /// <summary>Displayed when the password field is required.</summary>
        public const string PasswordRequired = "Password is required.";

        /// <summary>Displayed when the password is too short.</summary>
        public const string PasswordMinLength = "Password must be at least 8 characters.";

        /// <summary>Displayed when the phone number field is required.</summary>
        public const string PhoneRequired = "Phone number is required.";

        /// <summary>Displayed when the phone number format is invalid.</summary>
        public const string PhoneInvalid = "Invalid phone number format.";

        /// <summary>Displayed when points are negative.</summary>
        public const string PointsPositive = "Points must be 0 or greater.";

        // --- RegisterEmployeeDTO, UserProfileListDTO ---

        /// <summary>Displayed when the user ID is required.</summary>
        public const string IdRequired = "User ID is required.";

        /// <summary>Displayed when the name field is required.</summary>
        public const string NameRequired = "Name is required.";

        /// <summary>Displayed when the name is outside the valid length range.</summary>
        public const string NameLength = "Name must be between 2 and 100 characters.";

        /// <summary>Displayed when the salary field is required.</summary>
        public const string SalaryRequired = "Salary is required.";

        /// <summary>Displayed when the salary is negative.</summary>
        public const string SalaryPositive = "Salary must be 0 or greater.";

        // --- General ---

        /// <summary>Displayed when the price field is required.</summary>
        public const string PriceRequired = "Price is required.";

        /// <summary>Displayed when the price is negative.</summary>
        public const string PricePositive = "Price must be greater than or equal to 0.";

        // --- OrderDTO, OrderListDTO ---

        /// <summary>Displayed when the order ID is required.</summary>
        public const string OrderIdRequired = "OrderId is required.";

        /// <summary>Displayed when the client ID is required.</summary>
        public const string ClientIdRequired = "ClientId is required.";

        /// <summary>Displayed when the order date is required.</summary>
        public const string OrderDateRequired = "Order date is required.";

        // --- ProductDTO, ProductListDTO ---

        /// <summary>Displayed when the product ID is required.</summary>
        public const string ProductIdRequired = "ProductId is required.";

        /// <summary>Displayed when the product name is required.</summary>
        public const string ProductNameRequired = "Product name is required.";

        /// <summary>Displayed when the product name is outside the valid length range.</summary>
        public const string ProductNameLength = "Product name must be between 2 and 100 characters.";

        /// <summary>Displayed when the product description is required.</summary>
        public const string DescriptionRequired = "Product description is required.";

        /// <summary>Displayed when the product description is outside the valid length range.</summary>
        public const string DescriptionLength = "Product description must be between 5 and 500 characters.";

        /// <summary>Displayed when the product image URL is required.</summary>
        public const string ImageRequired = "Product image URL is required.";

        /// <summary>Displayed when the stock field is required.</summary>
        public const string StockRequired = "Stock is required.";

        /// <summary>Displayed when the stock is negative.</summary>
        public const string StockPositive = "Stock must be 0 or greater.";

        // --- ProductOrderCreateDTO, ProductOrderDetailsDTO, ProductOrderDTO, ProductOrderUpdateDTO ---

        /// <summary>Displayed when the quantity field is required.</summary>
        public const string QuantityRequired = "Quantity is required.";

        /// <summary>Displayed when the quantity is not greater than 0.</summary>
        public const string QuantityPositive = "Quantity must be greater than 0.";

        /// <summary>Displayed when the unit price field is required.</summary>
        public const string UnitPriceRequired = "Unit price is required.";

        /// <summary>Displayed when the unit price is negative.</summary>
        public const string UnitPricePositive = "Unit price must be 0 or greater.";
    }

}
