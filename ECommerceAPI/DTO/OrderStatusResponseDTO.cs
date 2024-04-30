namespace ECommerceAPI.DTO
{
    //This class stores the Order status Response infromation whenver we update Order Status.
    public class OrderStatusResponseDTO
    {
        public int OrderId { get; set; }
        public string Status { get; set; }
        public bool IsUpdated { get; set; }
        public string Message { get; set; }
    }
}
