using ECommerceAPI.Data;
using Microsoft.AspNetCore.Mvc;

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
    }
}
