using ECommerceAPI.DTOs;
using ECommerceAPI.Models;
using Microsoft.Data.SqlClient;

namespace ECommerceAPI.Data
{
    public class ProductRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;
        public ProductRepository(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        //This method fetch all the Products from the Database.
        public async Task<List<Product>> GetAllProductsAsync()
        {
            //This will hold the product
            var products = new List<Product>();

            //T-SQL query to fetch the product data.
            var query = "SELECT ProductId, Name, Price, Quantity, Description, IsDeleted FROM Products WHERE IsDeleted = 0";

            using (var connection = _connectionFactory.CreateConnection())
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(query, connection))
                {
                    //ExecuteReaderAsync method will fetch the data from the database based on the given T-SQL query.
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            products.Add(new Product
                            {
                                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                                Description = reader.GetString(reader.GetOrdinal("Description")),
                                IsDeleted = reader.GetBoolean(reader.GetOrdinal("IsDeleted"))
                            });
                        }
                    }
                }
            }
            //Returns the all products
            return products;
        }

        //This method serch product based on the given Id and return all the data for that Product.
        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            //T-SQL query to fetch Product if the product exists in the database
            var query = "SELECT ProductId, Name, Price, Quantity, Description, IsDeleted FROM Products WHERE ProductId = @ProductId AND              IsDeleted = 0";

            Product? product = null;

            using (var connection = _connectionFactory.CreateConnection())
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductId", productId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            product = new Product
                            {
                                //GetOrdinal method fetch the data for a defined column from a raw.
                                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                                Description = reader.GetString(reader.GetOrdinal("Description")),
                                IsDeleted = reader.GetBoolean(reader.GetOrdinal("IsDeleted"))
                            };
                        }
                    }
                }
            }

            //Returns the product.
            return product;
        }

        //This method inserts a new Product in the database.
        public async Task<int> InsertProductAsync(ProductDTO product)
        {
            //T-SQL query to insert the data into Database.
            var query = @"INSERT INTO Products (Name, Price, Quantity, Description, IsDeleted)
                        VALUES (@Name, @Price, @Quantity, @Description, 0);
                        SELECT CAST(SCOPE_IDENTITY() as int);";

            using (var connection = _connectionFactory.CreateConnection())
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", product.Name);
                    command.Parameters.AddWithValue("@Price", product.Price);
                    command.Parameters.AddWithValue("@Quantity", product.Quantity);
                    command.Parameters.AddWithValue("@Description", product.Description ?? (object)DBNull.Value);

                    //ExecuteScalarAsync method capture the value returned by SCOPE_IDENTITY() which tells which Product Id is inseted last.
                    int productId = (int)await command.ExecuteScalarAsync();

                    //Return last inserted Product Id.
                    return productId;
                }
            }
        }

        //This method Updates details about a perticular product in the database
        public async Task UpdateProductAsync(ProductDTO product)
        {
            //SQL Query to update the Product in the Database
            var query = "UPDATE Products SET Name = @Name, Price = @Price, Quantity = @Quantity, Description = @Description WHERE ProductId = @ProductId";

            using (var connection = _connectionFactory.CreateConnection())
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductId", product.ProductId);
                    command.Parameters.AddWithValue("@Name", product.Name);
                    command.Parameters.AddWithValue("@Price", product.Price);
                    command.Parameters.AddWithValue("@Quantity", product.Quantity);
                    //Inserts a null value if the Produt description is not mention by the User
                    command.Parameters.AddWithValue("@Description", product.Description ?? (object)DBNull.Value);

                    //ExecuteNonQueryAsync method run the SQL query to update the Product data into the Database
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
