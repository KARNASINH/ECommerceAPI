﻿using ECommerceAPI.Data;
using ECommerceAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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
                //Return Errros message along with Status code.
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

                if(customer == null)
                {
                    //If customer not found in the Database it returns the reponse with 404 Http Status code.
                    return new APIResponse<Customer>(HttpStatusCode.NotFound,"Customer not found for the given Custoemr Id.");
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

    }
}
