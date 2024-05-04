using ECommerceAPI.Data;
using ECommerceAPI.DTO;
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



        //This End Point insert a Product into Database to make it available for the customer.
        //POST: api/product
        [HttpPost]
        public async Task<APIResponse<ProductResponseDTO>> CreateProduct([FromBody] ProductDTO product)
        {
            //This will perform Model Binding and Validation on the recevied data from the Http request body.
            if (!ModelState.IsValid)
            {
                //Returns the response with 400 Http status code.
                return new APIResponse<ProductResponseDTO>(HttpStatusCode.BadRequest, "Data given are Invalid.", ModelState);
            }

            try
            {
                //This will insert the Product in the database and will return the inserted Product id.
                var productId = await _productRepository.InsertProductAsync(product);

                //Returns the inserted Product Id to the client.
                var responseDTO = new ProductResponseDTO { ProductId = productId };

                //Returns the newly created Product Id along with 200 Http status code.
                return new APIResponse<ProductResponseDTO>(responseDTO, "Product created successfully.");
            }
            catch (Exception ex)
            {
                //If any exception occured then it retuns the reponse with 500 Http status code.
                return new APIResponse<ProductResponseDTO>(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }



        //This End Point updates Product's details into Database.
        //PUT: api/product/5
        [HttpPut("{id}")]
        public async Task<APIResponse<bool>> UpdateProduct(int id, [FromBody] ProductDTO product)
        {
            //This will check the Model Binding and Validation.
            if (!ModelState.IsValid)
            {
                //This returns the response if the Databinding and Validation fails.
                return new APIResponse<bool>(HttpStatusCode.BadRequest, "Invalid data", ModelState);
            }

            if (id != product.ProductId)
            {
                //This returns the response if the Product Id in URL and in Body mismatches.
                return new APIResponse<bool>(HttpStatusCode.BadRequest, "Mismatched product ID");
            }

            try
            {
                //This update the Product and return the 200 Http Status code.
                await _productRepository.UpdateProductAsync(product);
                return new APIResponse<bool>(true, "Product updated successfully.");
            }
            catch (Exception ex)
            {
                //If any exception occured then it retuns the reponse with 500 Http status code.
                return new APIResponse<bool>(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }


        //This End Point do a Soft Delete for the existing Product into database.
        // DELETE: api/product/5
        [HttpDelete("{id}")]
        public async Task<APIResponse<bool>> DeleteProduct(int id)
        {
            try
            {
                //This will fetche the Product details if exists.
                var product = await _productRepository.GetProductByIdAsync(id);

                //Check the Product exists into database or not.
                if (product == null)
                {
                    //Returns the API End point response with 404 Http status code.
                    return new APIResponse<bool>(HttpStatusCode.NotFound, "Product not found.");
                }

                //If Product exists in the database then it perform the Soft Delete into the database.
                await _productRepository.DeleteProductAsync(id);

                //Returns the API End point response with 200 Http status code.
                return new APIResponse<bool>(true, "Product deleted successfully.");
            }
            catch (Exception ex)
            {
                //If any exception occured then it retuns the reponse with 500 Http status code.
                return new APIResponse<bool>(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }
    }
}
