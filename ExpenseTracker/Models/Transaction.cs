﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseTracker.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Amount should be greater than 0.")]
        public int Amount { get; set; }
        [MaxLength(100)]
        public string? Note { get; set; }
        public DateTime Date { get; set; }= DateTime.Now;
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }


        [NotMapped]
        public string? CategoryTitleWithIcon { get {
            return Category==null?"": Category.Icon +" "+Category.Title;
            } }
        [NotMapped]
        public string? FormattedAmount { get
            {
                return ((Category == null || Category.Type == "Expense") ? "-" : "+") + Amount.ToString("c0");
            } }
    }
}
