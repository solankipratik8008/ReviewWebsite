using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Project_of_Sharan.Models
{
    public class Review
    {
        public int ReviewId { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string? CoffeeShopItemName { get; set; }

        [Required]
        [StringLength(500)]
        public string? ReviewText { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public DateTime DatePosted { get; set; }

        public virtual User? User { get; set; }

        public virtual ICollection<ReviewImage>? ReviewImages { get; set; }
    }
}
