using ECommerceAPI.Data;
using ECommerceAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ECommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        //Injecting OrderRepository object using DI Design Pattern.
        private readonly OrderRepository _orderRepository;
        public OrderController(OrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        //This End Point retrives all the Orders which has Pending status from the database and returns to the client.
        //GET: api/order
        [HttpGet]
        public async Task<ActionResult<APIResponse<List<Order>>>> GetAllOrders(string Status = "Pending")
        {
            try
            {
                //This will fetch all the Orders from the database if orders are in pending state.
                var orders = await _orderRepository.GetAllOrdersAsync(Status);

                //Returns all the retrived Orders with 200 Http status code.
                return Ok(new APIResponse<List<Order>>(orders, "Retrieved all orders successfully."));
            }
            catch (Exception ex)
            {
                //If any exception occured then it retuns the reponse with 500 Http status code.
                return StatusCode(500, new APIResponse<List<Order>>(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message));
            }
        }




        //This End Point returns the Order to the client for the given Order Id.
        //GET: api/order/2
        [HttpGet("{id}")]
        public async Task<APIResponse<Order>> GetOrderById(int id)
        {
            try
            {
                //Retrives the Order from the database for the given Order Id.
                var order = await _orderRepository.GetOrderDetailsAsync(id);

                //Checks Order has any data or not.
                if (order == null)
                {
                    //Return the response with 404 Http status code if the Order doesn't found in the database.
                    return new APIResponse<Order>(HttpStatusCode.NotFound, "Order not found.");
                }

                //Returns the response with 200 Http status code along with retrived Order details.
                return new APIResponse<Order>(order, "Order retrieved successfully.");
            }
            catch (Exception ex)
            {
                //If any exception occured then it retuns the reponse with 500 Http status code.
                return new APIResponse<Order>(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }
    }
}
