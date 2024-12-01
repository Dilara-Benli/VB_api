﻿using System.ComponentModel.DataAnnotations;

namespace VB_api.Models
{
    public class TransactionRequest
    {
        [Required]
        public long AccountID { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Amount { get; set; }

        [Required]
        [MaxLength(50)]
        public string Explanation { get; set; } 
    }
}
