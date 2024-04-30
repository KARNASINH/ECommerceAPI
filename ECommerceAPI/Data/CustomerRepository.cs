using ECommerceAPI.DTO;
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
            //Returns the List of Customers.
            return customers;
        }

        //This method returns the data based on the specified Custoemr ID.
        public async Task<Customer?> GetCustomerByIdAsync(int customerId)
        {
            //T-SQL Query to get the specified Customer.
            var query = "SELECT CustomerId, Name, Email, Address FROM Customers WHERE CustomerId = @CustomerId AND IsDeleted = 0";
            
            Customer? customer = null;

            //Establishes the connection with the SQL DB and executes the T-SQL Query.
            using (var connection = _connectionFactory.CreateConnection())
            {
                await connection.OpenAsync();
             
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerId", customerId);

                    //Asynchrouse operation to fetch the data.
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            customer = new Customer
                            {
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                Address = reader.GetString(reader.GetOrdinal("Address")),
                                IsDeleted = false
                            };
                        }
                    }
                }
            }
            //Returns the Customer.
            return customer;
        }

        //This method inserts a new Customer and return the created Customer ID
        public async Task<int> InsertCustomerAsync(CustomerDTO customer)
        {
            var query = @"INSERT INTO Customers (Name, Email, Address, IsDeleted)
                        VALUES (@Name, @Email, @Address, 0);
                        SELECT CAST(SCOPE_IDENTITY() as int);";
            
            using (var connection = _connectionFactory.CreateConnection())
            {
                await connection.OpenAsync();
            
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", customer.Name);
                    command.Parameters.AddWithValue("@Email", customer.Email);
                    command.Parameters.AddWithValue("@Address", customer.Address);

                    //ExecuteScalar is used to get the value generted by SCOPE_IDENTITY() function
                    int customerId = (int)await command.ExecuteScalarAsync();
                    
                    return customerId;
                }
            }
        }

        //This method updates Customer details in the Database.
        public async Task UpdateCustomerAsync(CustomerDTO customer)
        {
            //T-SQL Query
            var query = "UPDATE Customers SET Name = @Name, Email = @Email, Address = @Address WHERE CustomerId = @CustomerId";
            
            //Establishing connection and perforing Update operation.
            using (var connection = _connectionFactory.CreateConnection())
            {
                await connection.OpenAsync();
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerId", customer.CustomerId);
                    command.Parameters.AddWithValue("@Name", customer.Name);
                    command.Parameters.AddWithValue("@Email", customer.Email);
                    command.Parameters.AddWithValue("@Address", customer.Address);

                    //ExecuteNonQueryAsync method returns if any row is modified in the Database.
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        //This method deletes the Customer from the Database.
        public async Task DeleteCustomerAsync(int customerId)
        {
            //T-SQL query to delete the Customer from the database.
            var query = "UPDATE Customers SET IsDeleted = 1 WHERE CustomerId = @CustomerId";
            
            using (var connection = _connectionFactory.CreateConnection())
            {
                await connection.OpenAsync();
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerId", customerId);

                    //ExecuteNonQueryAsync method returns if any row is affected in the Database.
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
