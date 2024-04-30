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
    }
}