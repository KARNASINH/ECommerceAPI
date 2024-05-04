using ECommerceAPI.Data;
using ECommerceAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ECommerceAPI.Controllers
{
    //This Product Controller contains all the API End points related to Product CRUD and other Operations.
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        //Injecting ProductRepository object using DI Design Pattern
        private readonly ProductRepository _productRepository;
        public ProductController(ProductRepository productRepository)
        {
            _productRepository = productRepository;
        }



        //This End Point returns all the Products to the client if not marked as deleted.
        [HttpGet]
        public async Task<APIResponse<List<Product>>> GetAllProducts() 
        {
            try
            {   //This will fetch all the Product from the database if product is not deleted.
                var products = await _productRepository.GetAllProductsAsync();

                //Returns all the retrived products with 200 Http status code.
                return new APIResponse<List<Product>>(products, "All Products retrived successfully.");

            }
            catch (Exception ex) 
            {
                //If any exception occured then it retuns the reponse with 500 Http status code.
                return new APIResponse<List<Product>>(HttpStatusCode.InternalServerError, "Internal Server error." + ex.Message);
            }
        }



        //This End Point returns the Product to the client for the given Product Id if not marked as deleted.
        //GET: api/product/1
        [HttpGet("{id}")]
        public async Task<APIResponse<Product>> GetProductById(int id)
        {
            try
            {
                //Retrives the product from the database for the given product id.
                var product = await _productRepository.GetProductByIdAsync(id);

                //Return 404 Http status code if the product does not found.
                if (product == null)
                {
                    //Return the response with 404 Http status code if the Product doesn't found in the database.
                    return new APIResponse<Product>(HttpStatusCode.NotFound, "Product not found.");
                }
                //Returns the response with 200 Http status code along with retrived Product details.
                return new APIResponse<Product>(product, "Product retrieved successfully.");
            }
            catch (Exception ex)
            {
                //If any exception occured then it retuns the reponse with 500 Http status code.
                return new APIResponse<Product>(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }

    }
}
