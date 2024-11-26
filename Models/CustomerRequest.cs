using System.ComponentModel.DataAnnotations;

namespace VB_api.Models
{
    public class CustomerRequest
    {
        [Required]
        [MaxLength(50)]
        public string CustomerName { get; set; } 
        [Required]
        [MaxLength(50)]
        public string CustomerLastName { get; set; } 
        [Required]
        public DateTime CustomerBirthDate { get; set; } 
        [Required]
        [Range(10000000000, 99999999999, ErrorMessage = "TC Kimlik Numarası 11 Haneli Olmalıdır.")]
        public long CustomerIdentityNumber { get; set; } // unique
    }
}
