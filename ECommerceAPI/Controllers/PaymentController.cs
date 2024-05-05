using ECommerceAPI.Data;
using ECommerceAPI.DTO;
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

        //This End Point insert a Payment into Database.
        //POST: api/payment/makepayment
        [HttpPost("MakePayment")]
        public async Task<APIResponse<PaymentResponseDTO>> MakePayment([FromBody] PaymentDTO paymentDto)
        {
            //This will perform Model Binding and Validation on the recevied data from the Http request body.
            if (!ModelState.IsValid)
            {
                //Returns the response with 400 Http status code.
                return new APIResponse<PaymentResponseDTO>(HttpStatusCode.BadRequest, "Invalid Data", ModelState);
            }
            try
            {
                //This will insert the Payment in the database and will return the inserted Payment details.
                var response = await _paymentRepository.MakePaymentAsync(paymentDto);

                //Returns the newly created Payment Details along with 200 Http status code.
                return new APIResponse<PaymentResponseDTO>(response, response.Message);
            }
            catch (Exception ex)
            {
                //If any exception occured then it retuns the reponse with 500 Http status code.
                return new APIResponse<PaymentResponseDTO>(HttpStatusCode.InternalServerError, "Internal Server Error: " + ex.Message);
            }
        }


    }
}
