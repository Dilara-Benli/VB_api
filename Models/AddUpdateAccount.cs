using System.ComponentModel.DataAnnotations;

namespace VB_api.Models
{
    public class AddUpdateAccount
    {
        [Required]
        public long CustomerID { get; set; } 

        [Required]
        [MaxLength(50)]
        public string AccountName { get; set; } 
    }
}
