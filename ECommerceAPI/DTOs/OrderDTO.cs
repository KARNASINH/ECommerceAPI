using System.ComponentModel.DataAnnotations;

namespace ECommerceAPI.DTOs
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
