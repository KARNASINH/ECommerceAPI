﻿using ECommerceAPI.Data;
using ECommerceAPI.DTO;
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




        //This End Point insert a Order into Database to make it available for the customer.
        //POST: api/order
        [HttpPost]
        public async Task<APIResponse<CreateOrderResponseDTO>> CreateOrder([FromBody] OrderDTO orderDto)
        {
            //This will perform Model Binding and Validation on the recevied data from the Http request body.
            if (!ModelState.IsValid)
            {
                //Returns the response with 400 Http status code.
                return new APIResponse<CreateOrderResponseDTO>(HttpStatusCode.BadRequest, "Invalid data", ModelState);
            }
            try
            {

                //This will insert the Order in the database and will return the inserted Order details.
                var response = await _orderRepository.CreateOrderAsync(orderDto);

                //Returns the newly created Order Id along with 200 Http status code.
                return new APIResponse<CreateOrderResponseDTO>(response, response.Message);
            }
            catch (Exception ex)
            {
                //If any exception occured then it retuns the reponse with 500 Http status code.
                return new APIResponse<CreateOrderResponseDTO>(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }




        //This End Point updates Order's details into Database.
        //PUT: api/order/2/status
        [HttpPut("{id}/status")]
        public async Task<APIResponse<OrderStatusResponseDTO>> UpdateOrderStatus(int id, [FromBody] OrderStatusDTO status)
        {
            //This will check the Model Binding and Validation.
            if (!ModelState.IsValid)
            {
                //This returns the response if the Databinding and Validation fails.
                return new APIResponse<OrderStatusResponseDTO>(HttpStatusCode.BadRequest, "Invalid data", ModelState);
            }

            //Checks Order Id passed in URL is matched to Id passed in Http Request Body or not.
            if (id != status.OrderId)
            {
                //This returns the response if the Order Id in URL and in Body mismatches.
                return new APIResponse<OrderStatusResponseDTO>(HttpStatusCode.BadRequest, "Mismatched Order ID");
            }
            
            try
            {
                //This tries to update the Old order status with the new status.
                var response = await _orderRepository.UpdateOrderStatusAsync(id, status.Status);

                //This updates the Order and return the 200 Http Status code along with Order Response object.
                return new APIResponse<OrderStatusResponseDTO>(response, response.Message);
            }
            catch (Exception ex)
            {
                //If any exception occured then it retuns the reponse with 500 Http status code.
                return new APIResponse<OrderStatusResponseDTO>(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }



        //This End point confirms the Order if evverything is okay with Order iteam availablility, quantity, payment etc.
        // PUT: api/order/5/confirm
        [HttpPut("{id}/confirm")]
        public async Task<APIResponse<ConfirmOrderResponseDTO>> ConfirmOrder(int id)
        {
            try
            {
                //This updates the Order status from old to new.
                var response = await _orderRepository.ConfirmOrderAsync(id);

                //Returns the response with some order details along with 200 Http status code.
                return new APIResponse<ConfirmOrderResponseDTO>(response, response.Message);
            }
            catch (Exception ex)
            {
                //If any exception occured then it retuns the reponse with 500 Http status code.
                return new APIResponse<ConfirmOrderResponseDTO>(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }
    }
}
