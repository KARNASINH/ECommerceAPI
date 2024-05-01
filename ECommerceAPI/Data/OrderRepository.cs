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
    }
}