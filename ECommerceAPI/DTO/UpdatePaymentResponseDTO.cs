namespace ECommerceAPI.DTO
{
    //This class hold the information about the Payment Status updation process.
    public class UpdatePaymentResponseDTO
    {
        public int PaymentId { get; set; }
        public string CurrentStatus { get; set; }
        public string UpdatedStatus { get; set; }
        public string Message { get; set; }
        public bool IsUpdated { get; set; }
    }
}
