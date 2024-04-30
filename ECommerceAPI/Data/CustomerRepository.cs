using ECommerceAPI.Models;
using Microsoft.Data.SqlClient;

namespace ECommerceAPI.Data
{
    public class CustomerRepository
    {
        //Creates SqlConnectionFactory object to access Database data.
        private readonly SqlConnectionFactory _connectionFactory;
        public CustomerRepository(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        //This method will fetch all the Customer data.
        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            var customers = new List<Customer>();
            
            //T-SQL Query to fetch the all data.
            var query = "SELECT CustomerId, Name, Email, Address FROM Customers WHERE IsDeleted = 0";
            
            //Establishes the connection with the SQL DB and executes the T-SQL Query.
            using (var connection = _connectionFactory.CreateConnection())
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(query, connection))
                {
                    //Asynchrouse operation to fetch the data.
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            customers.Add(new Customer
                            {
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                Address = reader.GetString(reader.GetOrdinal("Address")),
                                IsDeleted = false
                            });
                        }
                    }
                }
            }
            //Returns the List of Students.
            return customers;
        }
    }
}
