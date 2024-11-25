using System.ComponentModel.DataAnnotations;

namespace VB_api.Models
{
    public class Currency
    {
        [Key]
        [MaxLength(50)]
        public string CurrencyType { get; set; } // PK
    }
}
