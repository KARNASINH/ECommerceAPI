namespace ECommerceAPI.DTO
{
    //This class holds the infromation whenever we confirm/Place the Order.
    public class ConfirmOrderResponseDTO
    {
        public int OrderId { get; set; }
        public bool IsConfirmed { get; set; }
        public string Message { get; set; }
    }
}
