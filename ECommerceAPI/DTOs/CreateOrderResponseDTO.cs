namespace ECommerceAPI.DTOs
{
    //This class holds the information when User place the order.
    public class CreateOrderResponseDTO
    {
        public int OrderId { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public bool IsCreated { get; set; }
    }
}
