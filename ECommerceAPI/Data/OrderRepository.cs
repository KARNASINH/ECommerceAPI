using ECommerceAPI.DTO;
using ECommerceAPI.Models;
using Microsoft.Data.SqlClient;

namespace ECommerceAPI.Data
{
    public class OrderRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;
        public OrderRepository(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        //This method will fetch the Order details based on the passed Status.
        public async Task<List<Order>> GetAllOrdersAsync(string Status)
        {
            var orders = new List<Order>();

            //T-SQL query to fetch the data from Database
            var query = "SELECT OrderId, CustomerId, TotalAmount, Status, OrderDate FROM Orders WHERE Status = @Status";

            using (var connection = _connectionFactory.CreateConnection())
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Status", Status);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        //Looping through all the Orders
                        while (await reader.ReadAsync())
                        {
                            var order = new Order
                            {
                                OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                                Status = reader.GetString(reader.GetOrdinal("Status")),
                                OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate"))
                            };

                            //Adding fetched order to the List of Order
                            orders.Add(order);
                        }
                    }
                }
            }
            //Return order
            return orders;
        }

        //Create the Order with Pending State
        public async Task<CreateOrderResponseDTO> CreateOrderAsync(OrderDTO orderDto)
        {
            //T-SQL query to fetch the Product details from the database table
            var productQuery = "SELECT ProductId, Price, Quantity FROM Products WHERE ProductId = @ProductId AND IsDeleted = 0";

            //Sotring Total Order Value
            decimal totalAmount = 0m;

            //Adding Product in the List if it has passed all validation good to put into the Order
            List<OrderItem> validatedItems = new List<OrderItem>();

            CreateOrderResponseDTO createOrderResponseDTO = new CreateOrderResponseDTO();

            using (var connection = _connectionFactory.CreateConnection())
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        //This will loop through all the Items mentioend in the Order and will calculate the total Order amount
                        foreach (OrderItemDetailsDTO item in orderDto.Items)
                        {
                            using (var productCommand = new SqlCommand(productQuery, connection, transaction))
                            {
                                productCommand.Parameters.AddWithValue("@ProductId", item.ProductId);

                                //Fetches the Product details
                                using (var reader = await productCommand.ExecuteReaderAsync())
                                {
                                    //This returns true if it has Product data row
                                    if (await reader.ReadAsync())
                                    {
                                        //Fetches product's Quantity
                                        int stockQuantity = reader.GetInt32(reader.GetOrdinal("Quantity"));

                                        //Fetches product's price
                                        decimal price = reader.GetDecimal(reader.GetOrdinal("Price"));

                                        //Checking the Product quantity in the Order is greater or equal to the Quantity available in the stock
                                        if (stockQuantity >= item.Quantity)
                                        {
                                            //Total amount for a perticular product in the Order
                                            totalAmount += price * item.Quantity;

                                            //This will add the Product along with Qunatity in to Order, if are available and sufficient in stock
                                            validatedItems.Add(new OrderItem
                                            {
                                                ProductId = item.ProductId,
                                                Quantity = item.Quantity,
                                                PriceAtOrder = price  //This is total price E.g. 2 Nos of Apple with price of 10$ >> 2*10 = 20$
                                            });
                                        }
                                        else
                                        {
                                            //Handle the case where there isn't enough stock
                                            createOrderResponseDTO.Message = $"Insufficient Stock for Product ID {item.ProductId}";
                                            createOrderResponseDTO.IsCreated = false;
                                            return createOrderResponseDTO;
                                        }
                                    }
                                    else
                                    {
                                        //Handle the case for Invalid Product Id
                                        createOrderResponseDTO.Message = $"Product Not Found for Product ID {item.ProductId}";
                                        createOrderResponseDTO.IsCreated = false;
                                        return createOrderResponseDTO;
                                    }
                                    reader.Close(); //Ensure the reader is closed before next iteration to save the resources
                                }
                            }
                        }

                        //T-SQL query to insert the order and to keep a track of which OrderId is inserted into the table
                        var orderQuery = "INSERT INTO Orders (CustomerId, TotalAmount, Status, OrderDate) OUTPUT INSERTED.OrderId VALUES (@CustomerId, @TotalAmount, @Status, @OrderDate)";

                        //T-SQL query to insert the OrderItems details
                        var itemQuery = "INSERT INTO OrderItems (OrderId, ProductId, Quantity, PriceAtOrder) VALUES (@OrderId, @ProductId, @Quantity, @PriceAtOrder)";


                        //Proceed with creating the order if all items are validated and product has enough stock
                        using (var orderCommand = new SqlCommand(orderQuery, connection, transaction))
                        {
                            orderCommand.Parameters.AddWithValue("@CustomerId", orderDto.CustomerId);
                            orderCommand.Parameters.AddWithValue("@TotalAmount", totalAmount);
                            orderCommand.Parameters.AddWithValue("@Status", "Pending");
                            orderCommand.Parameters.AddWithValue("@OrderDate", DateTime.Now);
                            var orderId = (int)await orderCommand.ExecuteScalarAsync();

                            // Insert all validated items
                            foreach (var validatedItem in validatedItems)
                            {
                                using (var itemCommand = new SqlCommand(itemQuery, connection, transaction))
                                {
                                    itemCommand.Parameters.AddWithValue("@OrderId", orderId);
                                    itemCommand.Parameters.AddWithValue("@ProductId", validatedItem.ProductId);
                                    itemCommand.Parameters.AddWithValue("@Quantity", validatedItem.Quantity);
                                    itemCommand.Parameters.AddWithValue("@PriceAtOrder", validatedItem.PriceAtOrder);
                                    await itemCommand.ExecuteNonQueryAsync();
                                }
                            }

                            //Committing the Transaction to make changes permanent
                            transaction.Commit();

                            createOrderResponseDTO.Status = "Pending";
                            createOrderResponseDTO.IsCreated = true;
                            createOrderResponseDTO.OrderId = orderId;
                            createOrderResponseDTO.Message = "Order Created Successfully";
                            return createOrderResponseDTO;
                        }
                    }
                    catch (Exception)
                    {
                        //This will rollback the transaction and discard all the changes we made into the Database
                        transaction.Rollback();

                        throw; // Re-throw to handle the exception further up the call stack
                    }
                }
            }
        }

        //Confirms the Order if Payment is completed and matches with the Total Amount in the Order receipt.
        public async Task<ConfirmOrderResponseDTO> ConfirmOrderAsync(int orderId)
        {
            // Queries to fetch order and payment details
            var orderDetailsQuery = "SELECT TotalAmount FROM Orders WHERE OrderId = @OrderId";

            var paymentDetailsQuery = "SELECT Amount, Status FROM Payments WHERE OrderId = @OrderId";

            var updateOrderStatusQuery = "UPDATE Orders SET Status = 'Confirmed' WHERE OrderId = @OrderId";

            var getOrderItemsQuery = "SELECT ProductId, Quantity FROM OrderItems WHERE OrderId = @OrderId";

            var updateProductQuery = "UPDATE Products SET Quantity = Quantity - @Quantity WHERE ProductId = @ProductId";

            ConfirmOrderResponseDTO confirmOrderResponseDTO = new ConfirmOrderResponseDTO()
            {
                OrderId = orderId,
            };

            using (var connection = _connectionFactory.CreateConnection())
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        //Declared variables to store the fetched values
                        decimal orderAmount = 0m;
                        decimal paymentAmount = 0m;
                        string paymentStatus = string.Empty;

                        //Fetches the Total Amount for the given order
                        using (var orderCommand = new SqlCommand(orderDetailsQuery, connection, transaction))
                        {
                            orderCommand.Parameters.AddWithValue("@OrderId", orderId);

                            using (var reader = await orderCommand.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    //Storing the Order's Total amount
                                    orderAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount"));
                                }
                                reader.Close();
                            }
                        }

                        //Retrives Amount and Status value from the Payment table
                        using (var paymentCommand = new SqlCommand(paymentDetailsQuery, connection, transaction))
                        {
                            paymentCommand.Parameters.AddWithValue("@OrderId", orderId);

                            using (var reader = await paymentCommand.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    paymentAmount = reader.GetDecimal(reader.GetOrdinal("Amount"));
                                    paymentStatus = reader.GetString(reader.GetOrdinal("Status"));
                                }

                                reader.Close();
                            }
                        }

                        //Checks if the Payment is completed and Order Amount is same as the Payment Amount for the given Order Id
                        if (paymentStatus == "Completed" && paymentAmount == orderAmount)
                        {
                            // Update product quantities
                            using (var itemCommand = new SqlCommand(getOrderItemsQuery, connection, transaction))
                            {
                                itemCommand.Parameters.AddWithValue("@OrderId", orderId);

                                using (var reader = await itemCommand.ExecuteReaderAsync())
                                {
                                    //This will loop through all the Items for the given order and deduct the quantity from the Product table
                                    while (reader.Read())
                                    {
                                        int productId = reader.GetInt32(reader.GetOrdinal("ProductId"));
                                        int quantity = reader.GetInt32(reader.GetOrdinal("Quantity"));

                                        using (var updateProductCommand = new SqlCommand(updateProductQuery, connection, transaction))
                                        {
                                            updateProductCommand.Parameters.AddWithValue("@ProductId", productId);
                                            updateProductCommand.Parameters.AddWithValue("@Quantity", quantity);

                                            //Executes the T-SQL query and deduct the quantity from the Product table
                                            await updateProductCommand.ExecuteNonQueryAsync();
                                        }
                                    }
                                    reader.Close();
                                }
                            }

                            //Updates the Order status in the Orders Table, mark it as "Confirmed"
                            using (var statusCommand = new SqlCommand(updateOrderStatusQuery, connection, transaction))
                            {
                                statusCommand.Parameters.AddWithValue("@OrderId", orderId);

                                await statusCommand.ExecuteNonQueryAsync();
                            }

                            //Committing the Transaction if everything is well
                            transaction.Commit();

                            confirmOrderResponseDTO.IsConfirmed = true;
                            confirmOrderResponseDTO.Message = "Order Confirmed Successfully.";

                            return confirmOrderResponseDTO;
                        }
                        else
                        {
                            transaction.Rollback();

                            confirmOrderResponseDTO.IsConfirmed = false;
                            confirmOrderResponseDTO.Message = "Cannot Confirm Order: Either Payment is incomplete or Payment amount does not match the Order's Total Amount.";

                            return confirmOrderResponseDTO;
                        }
                    }
                    catch (Exception ex)
                    {
                        //Rolling back the Transaction and restoring the changes made in the Database for this Transaction.
                        transaction.Rollback();

                        //Throwing the Exception if not able to conforming the Order
                        throw new Exception("Error Confirming Order: " + ex.Message);
                    }
                }
            }
        }

        //This method checks the Current status of the Order based on the passed Order Id and update it according to passed New Order Status if transition is possible.
        public async Task<OrderStatusResponseDTO> UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            //Creates an OrderStatusResponseDTO
            OrderStatusResponseDTO orderStatusDTO = new OrderStatusResponseDTO()
            {
                OrderId = orderId
            };

            using (var connection = _connectionFactory.CreateConnection())
            {
                await connection.OpenAsync();

                try
                {
                    //Fetches the current status of the Order
                    var currentStatusQuery = "SELECT Status FROM Orders WHERE OrderId = @OrderId";

                    string currentStatus;

                    using (var statusCommand = new SqlCommand(currentStatusQuery, connection))
                    {
                        statusCommand.Parameters.AddWithValue("@OrderId", orderId);

                        //Fetches the status an storing into variable
                        var result = await statusCommand.ExecuteScalarAsync();

                        if (result == null)
                        {
                            orderStatusDTO.Message = "Order not found.";
                            orderStatusDTO.IsUpdated = false;

                            return orderStatusDTO;
                        }

                        currentStatus = result.ToString();
                    }

                    //Checks the Order Status transition is valid or not.
                    //If the transition is valid then it applies otherwise it will reject it.
                    if (!IsValidStatusTransition(currentStatus, newStatus))
                    {
                        orderStatusDTO.Message = $"Invalid status transition from {currentStatus} to {newStatus}.";
                        orderStatusDTO.IsUpdated = false;

                        return orderStatusDTO;
                    }

                    //T-SQL query to update the Order status in the database
                    var updateStatusQuery = "UPDATE Orders SET Status = @NewStatus WHERE OrderId = @OrderId";

                    //
                    using (var updateCommand = new SqlCommand(updateStatusQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@OrderId", orderId);
                        updateCommand.Parameters.AddWithValue("@NewStatus", newStatus);

                        //T-SQL query executing to update the status of the Order in the database
                        int rowsAffected = await updateCommand.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            orderStatusDTO.Message = $"Order status updated to {newStatus}";
                            orderStatusDTO.Status = newStatus;
                            orderStatusDTO.IsUpdated = true;
                        }
                        else
                        {
                            orderStatusDTO.IsUpdated = false;
                            orderStatusDTO.Message = $"No order found with ID {orderId}";
                        }
                    }

                    //Returns the Order status updation object
                    return orderStatusDTO;
                }
                catch (Exception ex)
                {
                    //Throws the error if it founds the error while updating the Order Status
                    throw new Exception("Error updating order status: " + ex.Message, ex);
                }
            }

        }

        //This method takes Current and New Order status to update.
        //Based on the current status it allows to update status to the specific values
        private bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            //This check and allows to update the status to the specific values only
            switch (currentStatus)
            {
                case "Pending":
                    return newStatus == "Processing" || newStatus == "Cancelled";
                case "Confirmed":
                    return newStatus == "Processing";
                case "Processing":
                    return newStatus == "Delivered";
                //Delivered orders should not transition to any other status
                case "Delivered":
                    return false;
                //Cancelled orders should not transition to any other status
                case "Cancelled":
                    return false;
                default:
                    return false;
            }
        }

        //This method get the Order Details based on passed Order Id
        public async Task<Order?> GetOrderDetailsAsync(int orderId)
        {
            //T-SQL query to fetch the Order details based on the given Order Id
            var query = "SELECT OrderId, CustomerId, TotalAmount, Status, OrderDate FROM Orders WHERE OrderId = @OrderId";

            using (var connection = _connectionFactory.CreateConnection())
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderId", orderId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        //If Order is not found then returns null.
                        if (!reader.Read()) return null;

                        //Returns the Order details based on the given Order Id
                        return new Order
                        {
                            OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                            Status = reader.GetString(reader.GetOrdinal("Status")),
                            OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate"))
                        };
                    }
                }
            }
        }
    }
}