namespace ProjecteSOS_Grup03API.Tools
{
    public static class ErrorMessages
    {
        // AuthController
        public const string ClientRegistered = "Client was registered";
        public const string EmployeeRegistered = "Employee was registered";
        public const string AdminRegistered = "Admin was registered";
        public const string InvalidEmailOrPassword = "Invalid email or password";
        public const string UserNotFound = "User not found";
        public const string NoRoleFound = "No role found for the user";
        public const string ProfileNotFound = "Profile not found";
        public const string NoRegisteredUsers = "No registered users found";
        public const string UserIsNotRegistered = "User is not registered";
        public const string UserUpdated = "User was updated";
        public const string PasswordUpdated = "Password was updated";
        public const string IncorrectPassword = "Incorrect password";
        public const string AccountDeleted = "Your account was deleted";

        // OrdersController
        public const string OrderNotFound = "Order not found";
        public const string NoPermissionToViewOrder = "You do not have permission to view this order";
        public const string OrderNotFoundForEdit = "Order not found.";
        public const string NoPermissionToEditOrder = "You do not have permission to edit this order.";
        public const string NoPermissionToChangeOwner = "You cannot change the owner of the order.";
        public const string InvalidNewClientId = "The new ClientId is not valid.";
        public const string InvalidSalesRepId = "The SalesRepId is not valid.";
        public const string ClientNotFound = "Client does not exist";
        public const string OrderAlreadyExists = "An order already exists";
        public const string OrderCreated = "Order created successfully";
        public const string EmployeeNotFound = "Employee not found";
        public const string NoActiveOrder = "There is no active order";
        public const string OrderConfirmed = "The order has been confirmed successfully.";
        public const string NoPermissionToDeleteOrder = "You do not have permission to delete this order";
        public const string OrderDeleted = "The order with id {0} has been deleted successfully.";

        // ProductsController
        public const string ProductNotFound = "Product not found";
        public const string NoProducts = "There are no products";
        public const string ProductNotDeleted = "Product not deleted";
        public const string ProductDeleted = "Product {0} deleted";
        public const string ProductRestocked = "Product {0} restocked";
        public const string ProductUpdated = "Product {0} updated";

        // OrderedProductsController (Uses OrderNotFound, ClientNotFound, NoActiveOrder, ProductNotFound)
        public const string NoProductsForOrder = "No products found for order with id {0}.";
        public const string ProductOrderNotFound = "Product order not found.";
        public const string NoOrderedProductsForUser = "No ordered products found for this user.";
        public const string ProductNotFoundInOrder = "Product not found in the order.";
        public const string NoPermissionForItem = "You do not have permission to view this item.";
        public const string OnlyAddToOwnOrders = "You can only add products to your own orders.";
        public const string NotEnoughStock = "Not enough stock for product {0}. Available: {1}.";
        public const string ProductAlreadyInOrder = "This product already exists in the order. Use PUT to update the quantity.";
        public const string ErrorSavingToDatabase = "Error saving to the database.";
        public const string NoPermissionToEdit = "You do not have permission to edit this order item.";
        public const string OrderItemDeletedOrModified = "The order item has been deleted or modified by another user.";
        public const string ConcurrencyProblem = "Concurrency issue. Reload data and try again.";
        public const string ErrorUpdatingDb = "Error updating the database: ";
        public const string QuantityUpdated = "Quantity has been successfully updated";
        public const string NoPermissionToDelete = "You do not have permission to delete this order item.";
        public const string ErrorDeletingDb = "Error deleting the order item from the database: ";
        public const string ProductOrderDeleted = "The product item with id {0} from order with id {1} has been successfully deleted.";
        public const string ProductDeletedFromOrder = "Product deleted from order";
    }
}
