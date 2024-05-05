using ECommerceAPI.Data;
using ECommerceAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ECommerceAPI.Controllers
{
    //This class holds all the Payment related CURD and otehr operation's End Points.
    public class PaymentController : ControllerBase
    {
        //Injecting OrderRepository object using DI Design Pattern.
        private readonly PaymentRepository _paymentRepository;
        public PaymentController(PaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }


        //This API End point get the Payment details for the given Payment Id.
        //GET: api/payment/paymentdetails/5
        [HttpGet("PaymentDetails/{id}")]
        public async Task<APIResponse<Payment>> GetPaymentDetails(int id)
        {
            try
            {
                //This will fetch the Payment details from the Database for the given Payment Id.
                var payment = await _paymentRepository.GetPaymentDetailsAsync(id);

                //Checks Payment has any data or not.
                if (payment == null)
                {

                    //Return the response with 404 Http status code if the Payment doesn't found in the database.
                    return new APIResponse<Payment>(HttpStatusCode.NotFound, $"Payment with ID {id} not found.");
                }

                //Returns the response with 200 Http status code along with retrived Payment details.
                return new APIResponse<Payment>(payment, "Payment retrieved successfully.");
            }
            catch (Exception ex)
            {
                //If any exception occured then it retuns the reponse with 500 Http status code.
                return new APIResponse<Payment>(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }
    }
}
