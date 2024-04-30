using System.ComponentModel.DataAnnotations;

namespace ECommerceAPI.DTOs
{
    //This class represents Order status details.
    public class OrderStatusDTO
    {
        [Required]
        public int OrderId { get; set; }
        [Required]
        public string Status { get; set; }
    }
}
