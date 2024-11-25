using System.ComponentModel.DataAnnotations;

namespace VB_api.Models
{
    public class TransactionType
    {
        [Key]
        [MaxLength(50)]
        public string TransactionTypeName { get; set; } // PK
    }
}
