using System.ComponentModel.DataAnnotations;

namespace ECommerceAPI.DTO
{
    //This class represents details about Order placed
    public class OrderDTO
    {
        [Required]
        public int CustomerId { get; set; }
        [Required]
        public List<OrderItemDetailsDTO> Items { get; set; }
    }
}
