using ECommerceAPI.Data;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers
{
    //This Product Controller contains all the API End points related to Product CRUD and other Operations.
    public class ProductController : ControllerBase
    {
        //Injecting ProductRepository object using DI Design Pattern
        private readonly CustomerRepository _customerRepository;
        public ProductController(CustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

    }
}
