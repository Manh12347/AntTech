using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AntTech.Models
{
    [Table("CommentLike")]
    public class CommentLike
    {
        // Khóa chính phức hợp sẽ được cấu hình trong DbContext
        [Required]
        public int CommentId { get; set; } // Khóa ngoại đến Comment

        [Required]
        public int AccountId { get; set; } // Khóa ngoại đến Account

        public DateTime LikedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("CommentId")]
        public virtual Comment Comment { get; set; }

        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
    }
}