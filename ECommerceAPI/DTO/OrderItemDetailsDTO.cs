using System.ComponentModel.DataAnnotations;

namespace ECommerceAPI.DTO
{
    //This class represents the Items along with Quantity of the Order.
    public class OrderItemDetailsDTO
    {
        [Required]
        public int ProductId { get; set; }
        [Required]
        public int Quantity { get; set; }
    }
}
