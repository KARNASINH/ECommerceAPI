using Azure;
using ECommerceAPI.Data;
using ECommerceAPI.DTO;
using ECommerceAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using static System.Net.WebRequestMethods;

namespace ECommerceAPI.Controllers
{
    //This controllers contains all the API End points related to Customer CRUD and other Operations
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : Controller
    {
        //Injecting CustomerRepository object using DI Design Pattern
        private readonly CustomerRepository _customerRepository;
        public CustomerController(CustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        //This end point return All Customers. 
        //GET: api/customer
        [HttpGet]
        public async Task<APIResponse<List<Customer>>> GetAllCustomers()
        {
            try
            {
                var customers = await _customerRepository.GetAllCustomersAsync();

                //Return List of Customer with 200 Staus code.
                return new APIResponse<List<Customer>>(customers, "Retrieved all customers successfully.");
            }
            catch (Exception ex)
            {
                //Return Errros message along with 500 Status code.
                return new APIResponse<List<Customer>>(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }



        //This end point retrives the Customer details based on the given Customer Id.
        //GET: api/customer/1
        [HttpGet("{id}")]
        public async Task<APIResponse<Customer>> GetCustomerById(int id)
        {
            try
            {
                //Fetches the Customer data from the database.
                var customer = await _customerRepository.GetCustomerByIdAsync(id);

                if (customer == null)
                {
                    //If customer not found in the Database it returns the reponse with 404 Http Status code.
                    return new APIResponse<Customer>(HttpStatusCode.NotFound, "Customer not found for the given Custoemr Id.");
                }

                //If customer founds then return the reponse with 200 Http Statu code.
                return new APIResponse<Customer>(customer, "Custome retrived successfully.");
            }
            catch (Exception ex)
            {
                //If any exception occured then it retuns the reponse with 500 Http status code.
                return new APIResponse<Customer>(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }


        //This End Point insert a new Customer into database and return the newly generated Customer Id to the client.
        //POST: api/customer
        [HttpPost]
        public async Task<APIResponse<CustomerResponseDTO>> CreateCustomer([FromBody] CustomerDTO customerDto)
        {
            //This will check the Model Binding and Validation.
            if (!ModelState.IsValid)
            {
                //This returns the response if the Databinding and Validation is not occured.
                return new APIResponse<CustomerResponseDTO>(HttpStatusCode.BadRequest, "Invalid data.", ModelState);
            }
            try
            {
                //Tries to create Customer into the Database.
                var customerId = await _customerRepository.InsertCustomerAsync(customerDto);

                //Holds the newly created Customer Id.
                var responseDTO = new CustomerResponseDTO { CustomerId = customerId };

                //Returns the API End point response with 200 Http status code.
                return new APIResponse<CustomerResponseDTO>(responseDTO, "Customer Created Successfully.");
            }
            catch (Exception ex)
            {
                //If any exception occured then it retuns the reponse with 500 Http status code.
                return new APIResponse<CustomerResponseDTO>(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }



        //This End Point update the existing customer into the database.
        //PUT: api/customer/1
        [HttpPut("{id}")]
        public async Task<APIResponse<bool>> UpdateCustomer(int id, [FromBody] CustomerDTO customerDto)
        {
            //This will check the Model Binding and Validation.
            if (!ModelState.IsValid)
            {
                //This returns the response if the Databinding and Validation is not occured.
                return new APIResponse<bool>(HttpStatusCode.BadRequest, "Invalid data", ModelState);
            }
            if (id != customerDto.CustomerId)
            {
                //This returns the response if the Customer Id in URL and in Body mismatches.
                return new APIResponse<bool>(HttpStatusCode.BadRequest, "Mismatched Customer ID.");
            }
            try
            {
                //This update the Customer and return the 200 Http Status code.
                await _customerRepository.UpdateCustomerAsync(customerDto);
                return new APIResponse<bool>(true, "Customer Updated Successfully.");
            }
            catch (Exception ex)
            {
                //If any exception occured then it retuns the reponse with 500 Http status code.
                return new APIResponse<bool>(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }


        //This End Point do a Soft Delete for the existing customer into database.
        //DELETE: api/customer/2
        [HttpDelete("{id}")]
        public async Task<APIResponse<bool>> DeleteCustomer(int id)
        {
            try
            {
                //This will fetche the CUstomer details if exists.
                var customer = await _customerRepository.GetCustomerByIdAsync(id);
                
                //Check the customer is exists into database or not.
                if (customer == null)
                {
                    //Returns the API End point response with 404 Http status code.
                    return new APIResponse<bool>(HttpStatusCode.NotFound, "Customer not found.");
                }

                //If customer exists in the database then it perform the Soft Delete into the database.
                await _customerRepository.DeleteCustomerAsync(id);

                //Returns the API End point response with 200 Http status code.
                return new APIResponse<bool>(true, "Customer deleted successfully.");
            }
            catch (Exception ex)
            {
                //If any exception occured then it retuns the reponse with 500 Http status code.
                return new APIResponse<bool>(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }
    }
}

