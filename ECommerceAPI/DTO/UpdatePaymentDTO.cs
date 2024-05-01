using System.ComponentModel.DataAnnotations;

namespace ECommerceAPI.DTO
{
    //This class update the Payment for the specific Order in the Database.
    public class UpdatePaymentDTO
    {
        [Required]
        public int PaymentId { get; set; }
        [Required]
        public string Status { get; set; }
    }
}
