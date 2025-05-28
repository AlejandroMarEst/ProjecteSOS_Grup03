namespace ProjecteSOS_Grup03API.Tools
{
    /// <summary>
    /// Centralized error and status messages for all controllers.
    /// </summary>
    public static class ErrorMessages
    {
        // -------------------- AuthController --------------------

        /// <summary>Message when a client is registered successfully.</summary>
        public const string ClientRegistered = "Client was registered";

        /// <summary>Message when an employee is registered successfully.</summary>
        public const string EmployeeRegistered = "Employee was registered";

        /// <summary>Message when an admin is registered successfully.</summary>
        public const string AdminRegistered = "Admin was registered";

        /// <summary>Message when the email or password is invalid during login.</summary>
        public const string InvalidEmailOrPassword = "Invalid email or password";

        /// <summary>Message when a user is not found.</summary>
        public const string UserNotFound = "User not found";

        /// <summary>Message when no role is found for the user.</summary>
        public const string NoRoleFound = "No role found for the user";

        /// <summary>Message when a user profile is not found.</summary>
        public const string ProfileNotFound = "Profile not found";

        /// <summary>Message when no registered users are found.</summary>
        public const string NoRegisteredUsers = "No registered users found";

        /// <summary>Message when a user is not registered.</summary>
        public const string UserIsNotRegistered = "User is not registered";

        /// <summary>Message when a user is updated successfully.</summary>
        public const string UserUpdated = "User was updated";

        /// <summary>Message when a password is updated successfully.</summary>
        public const string PasswordUpdated = "Password was updated";

        /// <summary>Message when the provided password is incorrect.</summary>
        public const string IncorrectPassword = "Incorrect password";

        /// <summary>Message when a user account is deleted.</summary>
        public const string AccountDeleted = "Your account was deleted";

        // -------------------- OrdersController --------------------

        /// <summary>Message when an order is not found.</summary>
        public const string OrderNotFound = "Order not found";

        /// <summary>Message when the user does not have permission to view an order.</summary>
        public const string NoPermissionToViewOrder = "You do not have permission to view this order";

        /// <summary>Message when an order is not found for editing.</summary>
        public const string OrderNotFoundForEdit = "Order not found.";

        /// <summary>Message when the user does not have permission to edit an order.</summary>
        public const string NoPermissionToEditOrder = "You do not have permission to edit this order.";

        /// <summary>Message when the user cannot change the owner of the order.</summary>
        public const string NoPermissionToChangeOwner = "You cannot change the owner of the order.";

        /// <summary>Message when the new ClientId is invalid.</summary>
        public const string InvalidNewClientId = "The new ClientId is not valid.";

        /// <summary>Message when the SalesRepId is invalid.</summary>
        public const string InvalidSalesRepId = "The SalesRepId is not valid.";

        /// <summary>Message when a client is not found.</summary>
        public const string ClientNotFound = "Client does not exist";

        /// <summary>Message when an order already exists for the client.</summary>
        public const string OrderAlreadyExists = "An order already exists";

        /// <summary>Message when an order is created successfully.</summary>
        public const string OrderCreated = "Order created successfully";

        /// <summary>Message when an employee is not found.</summary>
        public const string EmployeeNotFound = "Employee not found";

        /// <summary>Message when there is no active order for the client.</summary>
        public const string NoActiveOrder = "There is no active order";

        /// <summary>Message when an order is confirmed successfully.</summary>
        public const string OrderConfirmed = "The order has been confirmed successfully.";

        /// <summary>Message when the user does not have permission to delete an order.</summary>
        public const string NoPermissionToDeleteOrder = "You do not have permission to delete this order";

        /// <summary>Message when an order is deleted successfully.</summary>
        public const string OrderDeleted = "The order with id {0} has been deleted successfully.";

        // -------------------- ProductsController --------------------

        /// <summary>Message when a product is not found.</summary>
        public const string ProductNotFound = "Product not found";

        /// <summary>Message when there are no products.</summary>
        public const string NoProducts = "There are no products";

        /// <summary>Message when a product could not be deleted.</summary>
        public const string ProductNotDeleted = "Product not deleted";

        /// <summary>Message when a product is deleted successfully.</summary>
        public const string ProductDeleted = "Product {0} deleted";

        /// <summary>Message when a product is restocked successfully.</summary>
        public const string ProductRestocked = "Product {0} restocked";

        /// <summary>Message when a product is updated successfully.</summary>
        public const string ProductUpdated = "Product {0} updated";

        // -------------------- OrderedProductsController --------------------
        // (Uses OrderNotFound, ClientNotFound, NoActiveOrder, ProductNotFound)

        /// <summary>Message when no products are found for an order.</summary>
        public const string NoProductsForOrder = "No products found for order with id {0}.";

        /// <summary>Message when a product order is not found.</summary>
        public const string ProductOrderNotFound = "Product order not found.";

        /// <summary>Message when no ordered products are found for the user.</summary>
        public const string NoOrderedProductsForUser = "No ordered products found for this user.";

        /// <summary>Message when a product is not found in the order.</summary>
        public const string ProductNotFoundInOrder = "Product not found in the order.";

        /// <summary>Message when the user does not have permission to view an item.</summary>
        public const string NoPermissionForItem = "You do not have permission to view this item.";

        /// <summary>Message when a user tries to add products to someone else's order.</summary>
        public const string OnlyAddToOwnOrders = "You can only add products to your own orders.";

        /// <summary>Message when there is not enough stock for a product.</summary>
        public const string NotEnoughStock = "Not enough stock for product {0}. Available: {1}.";

        /// <summary>Message when a product is already in the order.</summary>
        public const string ProductAlreadyInOrder = "This product already exists in the order. Use PUT to update the quantity.";

        /// <summary>Message when there is an error saving to the database.</summary>
        public const string ErrorSavingToDatabase = "Error saving to the database.";

        /// <summary>Message when the user does not have permission to edit an order item.</summary>
        public const string NoPermissionToEdit = "You do not have permission to edit this order item.";

        /// <summary>Message when the order item has been deleted or modified by another user.</summary>
        public const string OrderItemDeletedOrModified = "The order item has been deleted or modified by another user.";

        /// <summary>Message for concurrency issues.</summary>
        public const string ConcurrencyProblem = "Concurrency issue. Reload data and try again.";

        /// <summary>Message when there is an error updating the database.</summary>
        public const string ErrorUpdatingDb = "Error updating the database: ";

        /// <summary>Message when the quantity has been updated successfully.</summary>
        public const string QuantityUpdated = "Quantity has been successfully updated";

        /// <summary>Message when the user does not have permission to delete an order item.</summary>
        public const string NoPermissionToDelete = "You do not have permission to delete this order item.";

        /// <summary>Message when there is an error deleting an order item from the database.</summary>
        public const string ErrorDeletingDb = "Error deleting the order item from the database: ";

        /// <summary>Message when a product order is deleted successfully.</summary>
        public const string ProductOrderDeleted = "The product item with id {0} from order with id {1} has been successfully deleted.";

        /// <summary>Message when a product is deleted from an order.</summary>
        public const string ProductDeletedFromOrder = "Product deleted from order";
    }

}
