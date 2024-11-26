using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace VB_api.Models
{
    public class Transfer
    {
        [Key]
        public int TransferID { get; set; } // PK, sequence
        [Required]
        public int TransactionID { get; set; } // FK
        [Required]
        public long SourceAccountID { get; set; }
        [Required]
        public long TargetAccountID { get; set; } 
    }
}
