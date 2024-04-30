﻿using ECommerceAPI.Data;
using ECommerceAPI.Models;
using Microsoft.Data.SqlClient;

namespace ECommerceAPI.DTOs
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
    }
}
