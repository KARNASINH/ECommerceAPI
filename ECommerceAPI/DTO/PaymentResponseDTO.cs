namespace ECommerceAPI.DTO
{
    //This class holds the response details whenver a new payment is registered for the Order.
    //E.g. Payment is registered or not.
    public class PaymentResponseDTO
    {
        public int PaymentId { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public bool IsCreated { get; set; }
    }
}
