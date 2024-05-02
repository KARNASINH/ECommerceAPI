using ECommerceAPI.Data;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers
{
    //This controllers contains all the API End points related to Customer CRUD and other Operations
    public class CustomerController : Controller
    {
        //Injecting CustomerRepository object using DI Design Pattern
        private readonly CustomerRepository _customerRepository;
        public CustomerController(CustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }
    }
}
