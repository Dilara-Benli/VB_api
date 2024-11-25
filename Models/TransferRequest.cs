using System.ComponentModel.DataAnnotations;

namespace VB_api.Models
{
    public class TransferRequest
    {
        [Required]
        public long SourceAccountID { get; set; }
        [Required]
        public long TargetAccountID { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int Amount { get; set; }
        [Required]
        [MaxLength(50)]
        public string Explanation { get; set; }
    }
}
